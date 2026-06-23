using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.AI.Commands.GenerateStudyPlan;

public record ProcessStudyPlanGenerationCommand(
    string Name,
    Dictionary<string, double> AvailableFreeHours,
    long UserId
) : IRequest;

public class ProcessStudyPlanGenerationCommandHandler : IRequestHandler<ProcessStudyPlanGenerationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiServiceClient _aiServiceClient;

    public ProcessStudyPlanGenerationCommandHandler(IUnitOfWork unitOfWork, IAiServiceClient aiServiceClient)
    {
        _unitOfWork = unitOfWork;
        _aiServiceClient = aiServiceClient;
    }

    public async Task Handle(ProcessStudyPlanGenerationCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Student's active course offerings
        var enrollments = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Include(ce => ce.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ce => ce.StudentId == request.UserId && ce.Status == "Active")
            .ToListAsync(cancellationToken);

        if (!enrollments.Any())
        {
            return; // No active courses, cannot generate study plan
        }

        var enrolledOfferingIds = enrollments.Select(ce => ce.CourseOfferingId).ToList();
        var courseCodes = enrollments.Select(ce => ce.CourseOffering.Course.Code).Distinct().ToList();

        // 2. Fetch upcoming AcademicEvents (deadlines)
        var academicEvents = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => enrolledOfferingIds.Contains(ae.CourseOfferingId) && ae.DueDate > DateTime.UtcNow && ae.Status == "Active")
            .ToListAsync(cancellationToken);

        // 3. Fetch unconfirmed ExtractedDeadlines from messages
        var extractedDeadlines = await _unitOfWork.Repository<ExtractedDeadline>()
            .Query()
            .Include(ed => ed.IngestedMessage)
            .Where(ed => ed.IngestedMessage.UserId == request.UserId && !ed.IsConfirmed && !ed.IsRejected)
            .ToListAsync(cancellationToken);

        // 4. Map into AI Request DTOs
        var deadlinesList = new List<DeadlineInputDto>();
        foreach (var ae in academicEvents)
        {
            deadlinesList.Add(new DeadlineInputDto
            {
                CourseCode = ae.CourseOffering.Course.Code,
                Title = ae.Title,
                DueDate = ae.DueDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                Priority = ae.Priority
            });
        }

        foreach (var ed in extractedDeadlines)
        {
            deadlinesList.Add(new DeadlineInputDto
            {
                CourseCode = ed.CourseCodeGuess ?? "GEN101",
                Title = ed.Title,
                DueDate = ed.ParsedDueDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss"),
                Priority = "Medium"
            });
        }

        var aiRequest = new StudyPlanRequestDto
        {
            Courses = courseCodes,
            Deadlines = deadlinesList,
            FreeStudyHours = request.AvailableFreeHours
        };

        // 5. Call Python AI microservice
        var aiResponse = await _aiServiceClient.GenerateStudyPlanAsync(aiRequest, cancellationToken);

        // 6. Get or create Semester
        var semester = await _unitOfWork.Repository<Semester>().Query()
            .FirstOrDefaultAsync(s => s.IsCurrent, cancellationToken);

        if (semester == null)
        {
            semester = await _unitOfWork.Repository<Semester>().Query()
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (semester == null)
        {
            // Seed a default academic session and semester if they do not exist
            var institution = await _unitOfWork.Repository<Institution>().Query().FirstOrDefaultAsync(cancellationToken);
            if (institution != null)
            {
                var session = new AcademicSession
                {
                    InstitutionId = institution.Id,
                    Name = "Default Academic Session",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11)),
                    IsCurrent = true
                };
                await _unitOfWork.Repository<AcademicSession>().AddAsync(session, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                semester = new Semester
                {
                    AcademicSessionId = session.Id,
                    Name = "Semester 1",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(5)),
                    IsCurrent = true
                };
                await _unitOfWork.Repository<Semester>().AddAsync(semester, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("No institution exists to seed academic session/semester.");
            }
        }

        // 7. Persist StudyPlan
        var studyPlan = new StudyPlan
        {
            UserId = request.UserId,
            SemesterId = semester.Id,
            Name = request.Name,
            GeneratedAt = DateTime.UtcNow,
            IsActive = true,
            PreferencesJson = JsonSerializer.Serialize(request.AvailableFreeHours)
        };

        await _unitOfWork.Repository<StudyPlan>().AddAsync(studyPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Map and persist entries
        var courseMap = enrollments.ToDictionary(
            ce => ce.CourseOffering.Course.Code.ToUpperInvariant(),
            ce => ce.CourseOfferingId
        );

        foreach (var entry in aiResponse.Entries)
        {
            var courseCodeUpper = entry.CourseCode.ToUpperInvariant();
            long courseOfferingId;

            if (courseMap.ContainsKey(courseCodeUpper))
            {
                courseOfferingId = courseMap[courseCodeUpper];
            }
            else
            {
                // Try to find the closest match or default to first enrolled course
                var match = courseMap.Keys.FirstOrDefault(k => k.Contains(courseCodeUpper) || courseCodeUpper.Contains(k));
                if (match != null)
                {
                    courseOfferingId = courseMap[match];
                }
                else
                {
                    courseOfferingId = enrolledOfferingIds.First();
                }
            }

            var planEntry = new StudyPlanEntry
            {
                StudyPlanId = studyPlan.Id,
                CourseOfferingId = courseOfferingId,
                Date = DateOnly.Parse(entry.Date),
                StartTime = TimeOnly.Parse(entry.StartTime),
                EndTime = TimeOnly.Parse(entry.EndTime),
                TopicToStudy = entry.Topic,
                Priority = entry.Priority,
                IsCompleted = false
            };

            await _unitOfWork.Repository<StudyPlanEntry>().AddAsync(planEntry, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
