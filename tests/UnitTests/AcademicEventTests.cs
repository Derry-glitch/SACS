using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SACS.Application.Common.Interfaces;
using SACS.Application.Common.Models;
using SACS.Application.Events.Commands.CreateAssignment;
using SACS.Application.Events.Commands.CreateExam;
using SACS.Application.Events.Commands.CreateQuiz;
using SACS.Application.Events.Commands.CreateProject;
using SACS.Application.Events.Commands.UpdateAssignment;
using SACS.Application.Events.Commands.DeleteAssignment;
using SACS.Application.Events.Commands.SetReminders;
using SACS.Application.Events.Queries.GetAssignments;
using SACS.Application.Events.Queries.GetCalendar;
using SACS.Application.Events.Queries.GetExams;
using SACS.Application.Events.Queries.GetQuizzes;
using SACS.Application.Events.Queries.GetProjects;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class AcademicEventTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public AcademicEventTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; } = "1";
        public string? Email => "student@sacs.edu";
    }

    private async Task<(ApplicationDbContext context, UnitOfWork uow, TestCurrentUserService currentUserService)> CreateTestContextAsync()
    {
        var currentUserService = new TestCurrentUserService();
        var context = new ApplicationDbContext(_dbContextOptions, currentUserService);
        await DatabaseSeeder.SeedAsync(context);
        var uow = new UnitOfWork(context);
        return (context, uow, currentUserService);
    }

    private async Task<User> SeedBaseDataAsync(ApplicationDbContext context)
    {
        var institution = await context.Institutions.FirstAsync();

        // Seed Faculty
        var faculty = new Faculty { Name = "Science", Code = "SCI", InstitutionId = institution.Id };
        await context.Faculties.AddAsync(faculty);
        await context.SaveChangesAsync();

        // Seed Department
        var department = new Department { Name = "Computer Science", Code = "CSC", FacultyId = faculty.Id };
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        // Seed Course
        var course = new Course { Code = "CSC201", Title = "Java Programming", DepartmentId = department.Id, CreditUnits = 3 };
        var course2 = new Course { Code = "CSC202", Title = "Data Structures", DepartmentId = department.Id, CreditUnits = 3 };
        await context.Courses.AddRangeAsync(course, course2);
        await context.SaveChangesAsync();

        // Seed Academic Session & Semester
        var session = new AcademicSession
        {
            Name = "2025/2026",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(9)),
            InstitutionId = institution.Id,
            IsCurrent = true
        };
        await context.AcademicSessions.AddAsync(session);
        await context.SaveChangesAsync();

        var semester = new Semester
        {
            Name = "First Semester",
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            AcademicSessionId = session.Id,
            IsCurrent = true
        };
        await context.Semesters.AddAsync(semester);
        await context.SaveChangesAsync();

        // Seed Course Semester Offerings
        var offering = new CourseSemesterOffering { CourseId = course.Id, SemesterId = semester.Id };
        var offering2 = new CourseSemesterOffering { CourseId = course2.Id, SemesterId = semester.Id };
        await context.CourseSemesterOfferings.AddRangeAsync(offering, offering2);
        await context.SaveChangesAsync();

        // Seed Student User
        var studentUser = new User
        {
            Email = "student@sacs.edu",
            NormalizedEmail = "STUDENT@SACS.EDU",
            PasswordHash = "hashed",
            FirstName = "John",
            LastName = "Doe",
            InstitutionId = institution.Id
        };
        await context.Users.AddAsync(studentUser);
        await context.SaveChangesAsync();

        var studentProfile = new StudentProfile
        {
            Id = studentUser.Id, // Links to User.Id
            MatriculationNumber = "MAT-123",
            AcademicLevel = 200
        };
        await context.StudentProfiles.AddAsync(studentProfile);
        await context.SaveChangesAsync();

        // Enroll in first offering only (CSC201)
        var enrollment = new CourseEnrollment
        {
            CourseOfferingId = offering.Id,
            StudentId = studentProfile.Id,
            Status = "Active"
        };
        await context.CourseEnrollments.AddAsync(enrollment);
        await context.SaveChangesAsync();

        return studentUser;
    }

    [Fact]
    public async Task CreateAssignment_ShouldCreateAndReturnAssignmentDto_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateAssignmentCommandHandler(uow, currentUserService);

        var command = new CreateAssignmentCommand(
            Title: "Assignment 1",
            CourseOfferingId: offering.Id,
            Description: "Implement a binary search tree",
            DeadlineDate: DateTime.UtcNow.AddDays(7),
            Priority: "High",
            Attachments: new List<AttachmentRequestDto>
            {
                new AttachmentRequestDto("ref.pdf", "https://sacs.blob.core.windows.net/ref.pdf", 1024, "application/pdf")
            }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Assignment 1", result.Title);
        Assert.Equal("Implement a binary search tree", result.Description);
        Assert.Equal("High", result.Priority);
        Assert.Single(result.Attachments);
        Assert.Equal("ref.pdf", result.Attachments[0].FileName);

        var savedEvent = await context.AcademicEvents.Include(ae => ae.Attachments).FirstOrDefaultAsync(ae => ae.Id == result.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Assignment", savedEvent.EventType);
        Assert.Single(savedEvent.Attachments);
    }

    [Fact]
    public async Task UpdateAssignment_ShouldUpdateFieldsAndReturnAssignmentDto_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var initialMetadata = new EventMetadata { RawDescription = "Old description" };
        var existingEvent = new AcademicEvent
        {
            Title = "Old Title",
            Description = JsonSerializer.Serialize(initialMetadata),
            EventType = "Assignment",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = "Medium",
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };
        await context.AcademicEvents.AddAsync(existingEvent);
        await context.SaveChangesAsync();

        var handler = new UpdateAssignmentCommandHandler(uow);
        var command = new UpdateAssignmentCommand(
            Id: existingEvent.Id,
            Title: "New Title",
            Description: "New description",
            DeadlineDate: DateTime.UtcNow.AddDays(10),
            Priority: "Critical"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal("Critical", result.Priority);

        var updatedEvent = await context.AcademicEvents.FindAsync(existingEvent.Id);
        Assert.NotNull(updatedEvent);
        Assert.Equal("New Title", updatedEvent.Title);
        Assert.Equal("Critical", updatedEvent.Priority);
    }

    [Fact]
    public async Task DeleteAssignment_ShouldRemoveFromDatabase_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var existingEvent = new AcademicEvent
        {
            Title = "To Delete",
            Description = "{}",
            EventType = "Assignment",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = "Medium",
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };
        await context.AcademicEvents.AddAsync(existingEvent);
        await context.SaveChangesAsync();

        var handler = new DeleteAssignmentCommandHandler(uow);
        var command = new DeleteAssignmentCommand(existingEvent.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        // We use FirstOrDefaultAsync because soft delete global query filter will filter out deleted events.
        var deletedEvent = await context.AcademicEvents.FirstOrDefaultAsync(ae => ae.Id == existingEvent.Id);
        Assert.Null(deletedEvent);

        var rawDeletedEvent = await context.AcademicEvents.IgnoreQueryFilters().FirstOrDefaultAsync(ae => ae.Id == existingEvent.Id);
        Assert.NotNull(rawDeletedEvent);
        Assert.True(rawDeletedEvent.IsDeleted);
    }

    [Fact]
    public async Task GetAssignments_ShouldReturnAssignmentsForEnrolledCoursesOnly()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var enrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var nonEnrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC202");

        // Assignment in enrolled course
        var event1 = new AcademicEvent
        {
            Title = "Enrolled Course Assignment",
            Description = JsonSerializer.Serialize(new EventMetadata { RawDescription = "Desc 1" }),
            EventType = "Assignment",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = "Medium",
            CourseOfferingId = enrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        // Assignment in non-enrolled course
        var event2 = new AcademicEvent
        {
            Title = "Non-enrolled Course Assignment",
            Description = JsonSerializer.Serialize(new EventMetadata { RawDescription = "Desc 2" }),
            EventType = "Assignment",
            DueDate = DateTime.UtcNow.AddDays(5),
            Priority = "High",
            CourseOfferingId = nonEnrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(event1, event2);
        await context.SaveChangesAsync();

        var handler = new GetAssignmentsQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetAssignmentsQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Enrolled Course Assignment", result[0].Title);
        Assert.Equal("Desc 1", result[0].Description);
    }

    [Fact]
    public async Task CreateQuiz_ShouldCreateAndSaveQuizMetadata_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var handler = new CreateQuizCommandHandler(uow, currentUserService);
        var command = new CreateQuizCommand(
            Title: "Quiz 1",
            CourseOfferingId: offering.Id,
            Date: DateTime.UtcNow.AddDays(2),
            DurationMinutes: 30,
            ReminderWindow: "2hours",
            Notes: "Calculators allowed"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Quiz 1", result.Title);
        Assert.Equal(30, result.DurationMinutes);
        Assert.Equal("2hours", result.ReminderWindow);
        Assert.Equal("Calculators allowed", result.Notes);

        var savedEvent = await context.AcademicEvents.FindAsync(result.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Quiz", savedEvent.EventType);
        
        var meta = JsonSerializer.Deserialize<EventMetadata>(savedEvent.Description ?? "{}");
        Assert.Equal(30, meta?.DurationMinutes);
        Assert.Equal("Calculators allowed", meta?.Notes);
    }

    [Fact]
    public async Task GetQuizzes_ShouldReturnQuizzesForEnrolledCoursesOnly()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var enrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var nonEnrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC202");

        var quiz1 = new AcademicEvent
        {
            Title = "Enrolled Quiz",
            Description = JsonSerializer.Serialize(new EventMetadata { DurationMinutes = 15, Notes = "Note 1" }),
            EventType = "Quiz",
            DueDate = DateTime.UtcNow.AddDays(2),
            CourseOfferingId = enrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        var quiz2 = new AcademicEvent
        {
            Title = "Non-enrolled Quiz",
            Description = JsonSerializer.Serialize(new EventMetadata { DurationMinutes = 45, Notes = "Note 2" }),
            EventType = "Quiz",
            DueDate = DateTime.UtcNow.AddDays(4),
            CourseOfferingId = nonEnrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(quiz1, quiz2);
        await context.SaveChangesAsync();

        var handler = new GetQuizzesQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetQuizzesQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Enrolled Quiz", result[0].Title);
        Assert.Equal(15, result[0].DurationMinutes);
    }

    [Fact]
    public async Task CreateExam_ShouldCreateAndSaveExamMetadata_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var handler = new CreateExamCommandHandler(uow, currentUserService);
        var command = new CreateExamCommand(
            Title: "Semester Exam",
            CourseOfferingId: offering.Id,
            ExamDate: DateTime.UtcNow.AddDays(14),
            Venue: "Hall A",
            DurationMinutes: 120,
            SeatNumber: "Seat-45",
            Notes: "Bring your ID card"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Semester Exam", result.Title);
        Assert.Equal("Hall A", result.Venue);
        Assert.Equal(120, result.DurationMinutes);
        Assert.Equal("Seat-45", result.SeatNumber);

        var savedEvent = await context.AcademicEvents.FindAsync(result.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Exam", savedEvent.EventType);
        Assert.Equal("Hall A", savedEvent.Venue);
    }

    [Fact]
    public async Task GetExams_ShouldReturnExamsForEnrolledCoursesOnly()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var enrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var nonEnrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC202");

        var exam1 = new AcademicEvent
        {
            Title = "Enrolled Exam",
            Description = JsonSerializer.Serialize(new EventMetadata { DurationMinutes = 90, SeatNumber = "S1" }),
            EventType = "Exam",
            DueDate = DateTime.UtcNow.AddDays(15),
            Venue = "Hall A",
            CourseOfferingId = enrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        var exam2 = new AcademicEvent
        {
            Title = "Non-enrolled Exam",
            Description = JsonSerializer.Serialize(new EventMetadata { DurationMinutes = 120, SeatNumber = "S2" }),
            EventType = "Exam",
            DueDate = DateTime.UtcNow.AddDays(20),
            Venue = "Hall B",
            CourseOfferingId = nonEnrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(exam1, exam2);
        await context.SaveChangesAsync();

        var handler = new GetExamsQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetExamsQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Enrolled Exam", result[0].Title);
        Assert.Equal("S1", result[0].SeatNumber);
    }

    [Fact]
    public async Task CreateProject_ShouldCreateAndSaveProjectMetadata_WhenValid()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var handler = new CreateProjectCommandHandler(uow, currentUserService);
        var command = new CreateProjectCommand(
            Title: "Capstone Project",
            CourseOfferingId: offering.Id,
            SupervisorName: "Dr. Smith",
            SubmissionDate: DateTime.UtcNow.AddDays(30),
            ProgressPercentage: 25,
            Notes: "Requires weekly updates",
            Attachments: new List<AttachmentRequestDto>
            {
                new AttachmentRequestDto("proposal.docx", "https://sacs.blob.core.windows.net/proposal.docx", 2048, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            }
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Capstone Project", result.Title);
        Assert.Equal("Dr. Smith", result.SupervisorName);
        Assert.Equal(25, result.ProgressPercentage);
        Assert.Single(result.Attachments);

        var savedEvent = await context.AcademicEvents.Include(ae => ae.Attachments).FirstOrDefaultAsync(ae => ae.Id == result.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Project", savedEvent.EventType);
        Assert.Single(savedEvent.Attachments);
    }

    [Fact]
    public async Task GetProjects_ShouldReturnProjectsForEnrolledCoursesOnly()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var enrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var nonEnrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC202");

        var proj1 = new AcademicEvent
        {
            Title = "Enrolled Project",
            Description = JsonSerializer.Serialize(new EventMetadata { SupervisorName = "Dr. A", ProgressPercentage = 10 }),
            EventType = "Project",
            DueDate = DateTime.UtcNow.AddDays(30),
            CourseOfferingId = enrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        var proj2 = new AcademicEvent
        {
            Title = "Non-enrolled Project",
            Description = JsonSerializer.Serialize(new EventMetadata { SupervisorName = "Dr. B", ProgressPercentage = 50 }),
            EventType = "Project",
            DueDate = DateTime.UtcNow.AddDays(40),
            CourseOfferingId = nonEnrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(proj1, proj2);
        await context.SaveChangesAsync();

        var handler = new GetProjectsQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetProjectsQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Enrolled Project", result[0].Title);
        Assert.Equal("Dr. A", result[0].SupervisorName);
        Assert.Equal(10, result[0].ProgressPercentage);
    }

    [Fact]
    public async Task GetCalendar_ShouldReturnCalendarEventsByViewType()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var today = DateTime.UtcNow.Date;

        // Today's event
        var eventToday = new AcademicEvent
        {
            Title = "Today Event",
            Description = "{}",
            EventType = "Assignment",
            DueDate = today.AddHours(12),
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        // Event later this week (e.g. 2 days later, keeping in mind Sunday start/Saturday end logic)
        var eventWeek = new AcademicEvent
        {
            Title = "Week Event",
            Description = "{}",
            EventType = "Quiz",
            DueDate = today.AddDays(2).AddHours(14),
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        // Event later this month on a fixed date
        var targetDate = new DateTime(2027, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        
        var eventDayFixed = new AcademicEvent
        {
            Title = "Fixed Day Event",
            Description = "{}",
            EventType = "Exam",
            DueDate = targetDate,
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        var eventMonthFixed = new AcademicEvent
        {
            Title = "Fixed Month Event",
            Description = "{}",
            EventType = "Project",
            DueDate = targetDate.AddDays(10), // June 25, 2026
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(eventToday, eventWeek, eventDayFixed, eventMonthFixed);
        await context.SaveChangesAsync();

        var handler = new GetCalendarQueryHandler(uow, currentUserService);

        // 1. Test Day View (for targetDate: June 15, 2026)
        var dayResult = (await handler.Handle(new GetCalendarQuery("day", targetDate), CancellationToken.None)).ToList();
        Assert.Single(dayResult);
        Assert.Equal("Fixed Day Event", dayResult[0].Title);

        // 2. Test Week View (June 15, 2026 is a Monday. Week view starts on Sunday June 14, ends Saturday June 20)
        var weekResult = (await handler.Handle(new GetCalendarQuery("week", targetDate), CancellationToken.None)).ToList();
        Assert.Single(weekResult);
        Assert.Equal("Fixed Day Event", weekResult[0].Title);

        // 3. Test Month View (June 2026)
        var monthResult = (await handler.Handle(new GetCalendarQuery("month", targetDate), CancellationToken.None)).ToList();
        Assert.Equal(2, monthResult.Count);
        Assert.Contains(monthResult, e => e.Title == "Fixed Day Event");
        Assert.Contains(monthResult, e => e.Title == "Fixed Month Event");

        // 4. Test Upcoming View
        var upcomingResult = (await handler.Handle(new GetCalendarQuery("upcoming", DateTime.UtcNow), CancellationToken.None)).ToList();
        // Check that at least the seeded events with future due dates are returned
        Assert.NotEmpty(upcomingResult);
    }

    [Fact]
    public async Task SetReminders_ShouldClearOldAndSetNewScheduledNotifications()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var academicEvent = new AcademicEvent
        {
            Title = "Exam with reminders",
            Description = "{}",
            EventType = "Exam",
            DueDate = DateTime.UtcNow.AddDays(5),
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };
        await context.AcademicEvents.AddAsync(academicEvent);
        await context.SaveChangesAsync();

        // Old scheduled notification for this event & user
        var oldNotification = new ScheduledNotification
        {
            UserId = studentUser.Id,
            AcademicEventId = academicEvent.Id,
            ReminderType = "1day",
            ScheduledTime = DateTime.UtcNow.AddHours(-1),
            Status = "Sent"
        };
        await context.ScheduledNotifications.AddAsync(oldNotification);
        await context.SaveChangesAsync();

        var handler = new SetRemindersCommandHandler(uow, currentUserService);
        var command = new SetRemindersCommand(
            AcademicEventId: academicEvent.Id,
            Reminders: new List<string> { "1day", "3days" }
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify old one is deleted
        var containsOld = await context.ScheduledNotifications.AnyAsync(sn => sn.Id == oldNotification.Id);
        Assert.False(containsOld);

        // Verify new ones are created
        var newNotifications = await context.ScheduledNotifications
            .Where(sn => sn.AcademicEventId == academicEvent.Id && sn.UserId == studentUser.Id)
            .ToListAsync();

        Assert.Equal(2, newNotifications.Count);
        Assert.Contains(newNotifications, sn => sn.ReminderType == "1day" && sn.Status == "Pending");
        Assert.Contains(newNotifications, sn => sn.ReminderType == "3days" && sn.Status == "Pending");
    }
}
