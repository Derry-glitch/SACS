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
using SACS.Application.Events.Commands.CreateEvent;
using SACS.Application.Events.Commands.UpdateEvent;
using SACS.Application.Events.Commands.DeleteEvent;
using SACS.Application.Events.Commands.ConfigureReminders;
using SACS.Application.Events.Commands.DeleteReminder;
using SACS.Application.Events.Queries.GetEventById;
using SACS.Application.Events.Queries.GetEvents;
using SACS.Application.Events.Queries.GetCalendar;
using SACS.Application.Events.Queries.GetMyReminders;
using SACS.Application.Events.DTOs;
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
            Id = studentUser.Id,
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
    public async Task CreateEvent_ShouldCreateAndReturnEventDto_ForAssignment()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateEventCommandHandler(uow, currentUserService);

        var command = new CreateEventCommand(
            Title: "Assignment 1",
            Description: "Solve problem set 1",
            CourseId: offering.Id,
            EventType: AcademicEventType.Assignment,
            DueDateTime: DateTime.UtcNow.AddDays(7),
            PriorityLevel: "High",
            Notes: "Graded event",
            AttachmentUrl: "https://sacs.blob.core.windows.net/files/p1.pdf"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Assignment 1", result.Title);
        Assert.Equal(AcademicEventType.Assignment, result.EventType);
        Assert.Equal("https://sacs.blob.core.windows.net/files/p1.pdf", result.AttachmentUrl);

        var savedEvent = await context.AcademicEvents.FirstOrDefaultAsync(ae => ae.Id == result.Id);
        Assert.NotNull(savedEvent);
        Assert.Equal("Assignment", savedEvent.EventType);
    }

    [Fact]
    public async Task CreateEvent_ShouldCreateAndReturnEventDto_ForQuiz()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateEventCommandHandler(uow, currentUserService);

        var command = new CreateEventCommand(
            Title: "Pop Quiz",
            Description: "In-class quiz",
            CourseId: offering.Id,
            EventType: AcademicEventType.Quiz,
            DueDateTime: DateTime.UtcNow.AddDays(2),
            PriorityLevel: "Medium",
            Notes: "No calculators",
            AttachmentUrl: null,
            DurationMinutes: 20
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.DurationMinutes);

        var savedEvent = await context.AcademicEvents.FindAsync(result.Id);
        Assert.NotNull(savedEvent);
        var meta = JsonSerializer.Deserialize<EventMetadata>(savedEvent.Description ?? "{}");
        Assert.Equal(20, meta?.DurationMinutes);
    }

    [Fact]
    public async Task CreateEvent_ShouldCreateAndReturnEventDto_ForExam()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateEventCommandHandler(uow, currentUserService);

        var command = new CreateEventCommand(
            Title: "Final Exam",
            Description: "Comprehensive exam",
            CourseId: offering.Id,
            EventType: AcademicEventType.Exam,
            DueDateTime: DateTime.UtcNow.AddDays(14),
            PriorityLevel: "Critical",
            Notes: "Bring pen and ID",
            AttachmentUrl: null,
            DurationMinutes: 180,
            Venue: "Main Gym",
            SeatNumber: "Seat-B12"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Main Gym", result.Venue);
        Assert.Equal("Seat-B12", result.SeatNumber);
        Assert.Equal(180, result.DurationMinutes);
    }

    [Fact]
    public async Task CreateEvent_ShouldCreateAndReturnEventDto_ForProject()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateEventCommandHandler(uow, currentUserService);

        var command = new CreateEventCommand(
            Title: "Compiler Project",
            Description: "Build a lexer",
            CourseId: offering.Id,
            EventType: AcademicEventType.Project,
            DueDateTime: DateTime.UtcNow.AddDays(30),
            PriorityLevel: "High",
            Notes: "Team project",
            AttachmentUrl: null,
            SupervisorName: "Dr. Dave",
            ProgressPercentage: 10
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Dr. Dave", result.SupervisorName);
        Assert.Equal(10, result.ProgressPercentage);
    }

    [Fact]
    public async Task CreateEvent_ShouldCreateAndReturnEventDto_ForStudySession()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var handler = new CreateEventCommandHandler(uow, currentUserService);

        var command = new CreateEventCommand(
            Title: "Study Session 1",
            Description: "Group discussion",
            CourseId: offering.Id,
            EventType: AcademicEventType.StudySession,
            DueDateTime: DateTime.UtcNow.AddDays(3),
            PriorityLevel: "Low",
            Notes: "Reviewing lecture slides",
            AttachmentUrl: null,
            StudyTopic: "Recursion and Big-O",
            StudyDuration: 90
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Recursion and Big-O", result.StudyTopic);
        Assert.Equal(90, result.StudyDuration);
    }

    [Fact]
    public async Task UpdateEvent_ShouldUpdateFieldsAndReturnEventDto()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var initialMetadata = new EventMetadata { Notes = "Old notes" };
        var existingEvent = new AcademicEvent
        {
            Title = "Old Event",
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

        var handler = new UpdateEventCommandHandler(uow);
        var command = new UpdateEventCommand(
            Id: existingEvent.Id,
            Title: "New Event Title",
            Description: "New description",
            DueDateTime: DateTime.UtcNow.AddDays(10),
            PriorityLevel: "Critical",
            Notes: "Updated notes",
            AttachmentUrl: "https://newurl.com"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Event Title", result.Title);
        Assert.Equal("Updated notes", result.Notes);
        Assert.Equal("https://newurl.com", result.AttachmentUrl);
        Assert.Equal("Critical", result.PriorityLevel);
    }

    [Fact]
    public async Task DeleteEvent_ShouldRemoveFromDatabase()
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

        var handler = new DeleteEventCommandHandler(uow);
        var command = new DeleteEventCommand(existingEvent.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedEvent = await context.AcademicEvents.FirstOrDefaultAsync(ae => ae.Id == existingEvent.Id);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task GetEventById_ShouldReturnEventDto()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var metadata = new EventMetadata { Notes = "Special notes", DurationMinutes = 45 };
        var existingEvent = new AcademicEvent
        {
            Title = "Fetch Me",
            Description = JsonSerializer.Serialize(metadata),
            EventType = "Quiz",
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

        var handler = new GetEventByIdQueryHandler(uow);

        // Act
        var result = await handler.Handle(new GetEventByIdQuery(existingEvent.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fetch Me", result.Title);
        Assert.Equal(AcademicEventType.Quiz, result.EventType);
        Assert.Equal(45, result.DurationMinutes);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEventsForEnrolledCoursesOnly()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var enrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var nonEnrolledOffering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC202");

        var event1 = new AcademicEvent
        {
            Title = "Enrolled Event",
            Description = "{}",
            EventType = "Assignment",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = "Medium",
            CourseOfferingId = enrolledOffering.Id,
            CreatedByUserId = studentUser.Id,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        var event2 = new AcademicEvent
        {
            Title = "Non-enrolled Event",
            Description = "{}",
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

        var handler = new GetEventsQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetEventsQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Enrolled Event", result[0].Title);
    }

    [Fact]
    public async Task GetCalendar_Views_ShouldReturnCorrectEvents()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        
        var today = DateTime.UtcNow.Date;

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
            DueDate = targetDate.AddDays(10), // June 25, 2027
            CourseOfferingId = offering.Id,
            CreatedByUserId = studentUser.Id,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await context.AcademicEvents.AddRangeAsync(eventToday, eventWeek, eventDayFixed, eventMonthFixed);
        await context.SaveChangesAsync();

        // 1. Day View Handler
        var dailyHandler = new GetDailyCalendarQueryHandler(uow, currentUserService);
        var dayResult = (await dailyHandler.Handle(new GetDailyCalendarQuery(targetDate), CancellationToken.None)).ToList();
        Assert.Single(dayResult);
        Assert.Equal("Fixed Day Event", dayResult[0].Title);

        // 2. Week View Handler
        var weeklyHandler = new GetWeeklyCalendarQueryHandler(uow, currentUserService);
        var weekResult = (await weeklyHandler.Handle(new GetWeeklyCalendarQuery(targetDate), CancellationToken.None)).ToList();
        Assert.Single(weekResult);
        Assert.Equal("Fixed Day Event", weekResult[0].Title);

        // 3. Month View Handler
        var monthlyHandler = new GetMonthlyCalendarQueryHandler(uow, currentUserService);
        var monthResult = (await monthlyHandler.Handle(new GetMonthlyCalendarQuery(targetDate), CancellationToken.None)).ToList();
        Assert.Equal(2, monthResult.Count);

        // 4. Upcoming View Handler
        var upcomingHandler = new GetUpcomingCalendarQueryHandler(uow, currentUserService);
        var upcomingResult = (await upcomingHandler.Handle(new GetUpcomingCalendarQuery(), CancellationToken.None)).ToList();
        Assert.NotEmpty(upcomingResult);
    }

    [Fact]
    public async Task ConfigureReminders_Global_ShouldUpdateNotificationPreferences()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var handler = new ConfigureRemindersCommandHandler(uow, currentUserService);
        var command = new ConfigureRemindersCommand(
            Reminders: new List<ReminderItemRequestDto>
            {
                new("24hours", true),
                new("72hours", false)
            }
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var prefs = await context.NotificationPreferences.Where(np => np.UserId == studentUser.Id).ToListAsync();
        Assert.Equal(2, prefs.Count);
        Assert.Contains(prefs, np => np.ReminderType == "24hours" && np.IsEnabled);
        Assert.Contains(prefs, np => np.ReminderType == "72hours" && !np.IsEnabled);
    }

    [Fact]
    public async Task ConfigureReminders_EventSpecific_ShouldScheduleNotifications()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var academicEvent = new AcademicEvent
        {
            Title = "Target Event",
            Description = "{}",
            EventType = "Assignment",
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

        var handler = new ConfigureRemindersCommandHandler(uow, currentUserService);
        var command = new ConfigureRemindersCommand(
            Reminders: new List<ReminderItemRequestDto>
            {
                new("24hours", true),
                new("72hours", true)
            },
            EventId: academicEvent.Id
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var scheduled = await context.ScheduledNotifications
            .Where(sn => sn.AcademicEventId == academicEvent.Id && sn.UserId == studentUser.Id)
            .ToListAsync();

        Assert.Equal(2, scheduled.Count);
        Assert.Contains(scheduled, sn => sn.ReminderType == "24hours" && sn.Status == "Pending");
    }

    [Fact]
    public async Task GetMyReminders_ShouldReturnPendingNotifications()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var academicEvent = new AcademicEvent
        {
            Title = "Event Title",
            Description = "{}",
            EventType = "Assignment",
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

        var notification = new ScheduledNotification
        {
            UserId = studentUser.Id,
            AcademicEventId = academicEvent.Id,
            ReminderType = "72hours",
            ScheduledTime = DateTime.UtcNow.AddDays(2),
            Status = "Pending"
        };
        await context.ScheduledNotifications.AddAsync(notification);
        await context.SaveChangesAsync();

        var handler = new GetMyRemindersQueryHandler(uow, currentUserService);

        // Act
        var result = (await handler.Handle(new GetMyRemindersQuery(), CancellationToken.None)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Event Title", result[0].EventTitle);
        Assert.Equal("72hours", result[0].ReminderType);
    }

    [Fact]
    public async Task DeleteReminder_ShouldRemoveNotification()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var studentUser = await SeedBaseDataAsync(context);
        currentUserService.UserId = studentUser.Id.ToString();

        var offering = await context.CourseSemesterOfferings.FirstAsync(co => co.Course.Code == "CSC201");
        var academicEvent = new AcademicEvent
        {
            Title = "Event",
            Description = "{}",
            EventType = "Assignment",
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

        var notification = new ScheduledNotification
        {
            UserId = studentUser.Id,
            AcademicEventId = academicEvent.Id,
            ReminderType = "24hours",
            ScheduledTime = DateTime.UtcNow.AddDays(4),
            Status = "Pending"
        };
        await context.ScheduledNotifications.AddAsync(notification);
        await context.SaveChangesAsync();

        var handler = new DeleteReminderCommandHandler(uow);

        // Act
        await handler.Handle(new DeleteReminderCommand(notification.Id), CancellationToken.None);

        // Assert
        var deleted = await context.ScheduledNotifications.FindAsync(notification.Id);
        Assert.Null(deleted);
    }
}
