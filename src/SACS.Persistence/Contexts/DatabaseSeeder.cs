using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SACS.Domain.Common;
using SACS.Domain.Entities;

namespace SACS.Persistence.Contexts;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Ensure Database is created and migrations are applied
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        // 2. Seed Default Institution
        Institution institution;
        if (!await context.Institutions.AnyAsync())
        {
            institution = new Institution
            {
                Name = "SACS Academy",
                Code = "SACS",
                Domain = "sacs.edu",
                TimeZone = "Africa/Lagos",
                IsActive = true
            };
            await context.Institutions.AddAsync(institution);
            await context.SaveChangesAsync();
        }
        else
        {
            institution = await context.Institutions.FirstAsync();
        }

        // 3. Seed Roles
        var requiredRoles = new[]
        {
            new Role { Name = Roles.Student, Description = "Default Student Role" },
            new Role { Name = Roles.Lecturer, Description = "Default Lecturer Role" },
            new Role { Name = Roles.Admin, Description = "Default Administrator Role" }
        };

        foreach (var role in requiredRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
            {
                await context.Roles.AddAsync(role);
            }
        }
        await context.SaveChangesAsync();

        var studentRole = await context.Roles.FirstAsync(r => r.Name == Roles.Student);
        var lecturerRole = await context.Roles.FirstAsync(r => r.Name == Roles.Lecturer);
        var adminRole = await context.Roles.FirstAsync(r => r.Name == Roles.Admin);

        // 4. Seed Departments & Courses
        if (!await context.Courses.AnyAsync())
        {
            var scienceFaculty = new Faculty { Name = "Science", Code = "SCI", InstitutionId = institution.Id };
            var engineeringFaculty = new Faculty { Name = "Engineering", Code = "ENG", InstitutionId = institution.Id };
            await context.Faculties.AddRangeAsync(scienceFaculty, engineeringFaculty);
            await context.SaveChangesAsync();

            var cscDepartment = new Department { Name = "Computer Science", Code = "CSC", FacultyId = scienceFaculty.Id };
            var eeeDepartment = new Department { Name = "Electrical Engineering", Code = "EEE", FacultyId = engineeringFaculty.Id };
            var civDepartment = new Department { Name = "Civil Engineering", Code = "CIV", FacultyId = engineeringFaculty.Id };
            await context.Departments.AddRangeAsync(cscDepartment, eeeDepartment, civDepartment);
            await context.SaveChangesAsync();

            var course1 = new Course { Code = "CSC201", Title = "Java Programming", DepartmentId = cscDepartment.Id, CreditUnits = 3 };
            var course2 = new Course { Code = "CSC301", Title = "Operating Systems", DepartmentId = cscDepartment.Id, CreditUnits = 3 };
            var course3 = new Course { Code = "EEE210", Title = "Circuit Theory", DepartmentId = eeeDepartment.Id, CreditUnits = 3 };
            var course4 = new Course { Code = "CIV220", Title = "Structural Analysis", DepartmentId = civDepartment.Id, CreditUnits = 3 };
            await context.Courses.AddRangeAsync(course1, course2, course3, course4);
            await context.SaveChangesAsync();

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

            var offering1 = new CourseSemesterOffering { CourseId = course1.Id, SemesterId = semester.Id };
            var offering2 = new CourseSemesterOffering { CourseId = course2.Id, SemesterId = semester.Id };
            var offering3 = new CourseSemesterOffering { CourseId = course3.Id, SemesterId = semester.Id };
            var offering4 = new CourseSemesterOffering { CourseId = course4.Id, SemesterId = semester.Id };
            await context.CourseSemesterOfferings.AddRangeAsync(offering1, offering2, offering3, offering4);
            await context.SaveChangesAsync();

            // 5. Seed Users: Admin, 5 Lecturers, 15 Students
            string defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            // Admin
            if (!await context.Users.AnyAsync(u => u.Email == "admin@sacs.edu"))
            {
                var adminUser = new User
                {
                    InstitutionId = institution.Id,
                    Email = "admin@sacs.edu",
                    NormalizedEmail = "ADMIN@SACS.EDU",
                    PasswordHash = defaultPasswordHash,
                    FirstName = "SACS",
                    LastName = "Admin",
                    IsActive = true,
                    IsEmailVerified = true
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
                await context.UserRoles.AddAsync(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
                await context.SaveChangesAsync();
            }

            // 5 Lecturers
            var lecturers = new List<User>();
            var lecturerFirstNames = new[] { "Thomas", "Sarah", "Michael", "Emily", "James" };
            var lecturerLastNames = new[] { "Taylor", "Anderson", "Thomas", "Jackson", "White" };
            var staffTitles = new[] { "Prof.", "Dr.", "Senior Lecturer", "Dr.", "Prof." };

            for (int i = 0; i < 5; i++)
            {
                var email = $"lecturer{i + 1}@sacs.edu";
                var user = new User
                {
                    InstitutionId = institution.Id,
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    PasswordHash = defaultPasswordHash,
                    FirstName = lecturerFirstNames[i],
                    LastName = lecturerLastNames[i],
                    IsActive = true,
                    IsEmailVerified = true
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = lecturerRole.Id });
                var profile = new LecturerProfile
                {
                    Id = user.Id,
                    StaffId = $"SACS/STAFF/{ (i + 1).ToString().PadLeft(3, '0') }",
                    OfficeLocation = $"Engineering Building Room {100 + i * 5}",
                    AcademicTitle = staffTitles[i]
                };
                await context.LecturerProfiles.AddAsync(profile);
                await context.SaveChangesAsync();
                lecturers.Add(user);
            }

            // Assign lecturers to course instructors
            await context.CourseInstructors.AddRangeAsync(
                new CourseInstructor { CourseOfferingId = offering1.Id, LecturerId = lecturers[0].Id },
                new CourseInstructor { CourseOfferingId = offering2.Id, LecturerId = lecturers[1].Id },
                new CourseInstructor { CourseOfferingId = offering3.Id, LecturerId = lecturers[2].Id },
                new CourseInstructor { CourseOfferingId = offering4.Id, LecturerId = lecturers[3].Id }
            );
            await context.SaveChangesAsync();

            // 15 Students
            var students = new List<User>();
            var studentFirstNames = new[] { "John", "Alice", "Bob", "Clara", "David", "Eva", "Frank", "Grace", "Henry", "Ivy", "Jack", "Karen", "Leo", "Mia", "Nathan" };
            var studentLastNames = new[] { "Doe", "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson" };
            var levels = new[] { 200, 200, 300, 300, 400, 200, 300, 400, 200, 300, 400, 200, 300, 400, 200 };
            var gpas = new[] { 3.50m, 3.82m, 3.12m, 3.90m, 3.45m, 2.80m, 3.65m, 3.75m, 3.20m, 3.40m, 3.85m, 2.95m, 3.10m, 3.30m, 3.60m };

            for (int i = 0; i < 15; i++)
            {
                var email = $"student{i + 1}@sacs.edu";
                var user = new User
                {
                    InstitutionId = institution.Id,
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    PasswordHash = defaultPasswordHash,
                    FirstName = studentFirstNames[i],
                    LastName = studentLastNames[i],
                    IsActive = true,
                    IsEmailVerified = true
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = studentRole.Id });
                var profile = new StudentProfile
                {
                    Id = user.Id,
                    MatriculationNumber = $"SACS/STUD/{ (i + 1).ToString().PadLeft(3, '0') }",
                    AcademicLevel = levels[i],
                    CurrentGPA = gpas[i],
                    CurrentCGPA = gpas[i]
                };
                await context.StudentProfiles.AddAsync(profile);
                await context.SaveChangesAsync();
                students.Add(user);

                // Enroll student in CSC201 and CSC301 (at least)
                await context.CourseEnrollments.AddRangeAsync(
                    new CourseEnrollment { CourseOfferingId = offering1.Id, StudentId = user.Id },
                    new CourseEnrollment { CourseOfferingId = offering2.Id, StudentId = user.Id }
                );
                
                // Enroll some in eee or civil
                if (i % 2 == 0)
                {
                    await context.CourseEnrollments.AddAsync(new CourseEnrollment { CourseOfferingId = offering3.Id, StudentId = user.Id });
                }
                else
                {
                    await context.CourseEnrollments.AddAsync(new CourseEnrollment { CourseOfferingId = offering4.Id, StudentId = user.Id });
                }
                await context.SaveChangesAsync();
            }

            // 6. Seed 10 Announcements
            var announcements = new List<Announcement>();
            var announcementTitles = new[]
            {
                "Welcome to the New Academic Session",
                "CSC201 Quiz Scheduled",
                "Operating Systems Lab Relocation",
                "Midterm Examination Timetable Published",
                "System Maintenance Window",
                "Guest Lecture on Artificial Intelligence",
                "EEE210 Homework Deadline Extension",
                "CIV220 Site Visit Guidelines",
                "Scholarship Applications Open",
                "Sports Day Registrations"
            };

            var announcementContents = new[]
            {
                "We welcome all returning and new students to the 2025/2026 Academic Session. Please complete your registration.",
                "There will be a short multiple-choice quiz covering Java classes and objects next Tuesday.",
                "The Operating Systems practical lab has been relocated to Computer Science Lab 3.",
                "The First Semester Midterm timetable is now available on the notice boards.",
                "SACS mobile systems will undergo brief maintenance this Friday from 10:00 PM to 12:00 AM.",
                "Join us for a seminar on LLMs in Education presented by Dr. Thomas Taylor.",
                "The submission deadline for EEE210 Circuit Theory homework has been extended by 48 hours.",
                "Students taking CIV220 must wear safety helmets and booths for the site visit on Thursday.",
                "Academic excellence scholarship applications are now open. Visit the admin office.",
                "Register for the annual inter-departmental football tournament by Friday afternoon."
            };

            var priorities = new[] { "Normal", "High", "Normal", "Urgent", "Normal", "Normal", "Normal", "High", "Normal", "Normal" };

            for (int i = 0; i < 10; i++)
            {
                var ann = new Announcement
                {
                    InstitutionId = institution.Id,
                    CreatedByUserId = lecturers[i % 5].Id,
                    Title = announcementTitles[i],
                    Content = announcementContents[i],
                    Priority = priorities[i],
                    IsPinned = i == 3 || i == 4,
                    ExpiresAt = DateTime.UtcNow.AddDays(14)
                };
                await context.Announcements.AddAsync(ann);
                await context.SaveChangesAsync();
                announcements.Add(ann);

                // Add recipients (first 10 students for mock data)
                for (int sIdx = 0; sIdx < 10; sIdx++)
                {
                    await context.AnnouncementRecipients.AddAsync(new AnnouncementRecipient
                    {
                        AnnouncementId = ann.Id,
                        UserId = students[sIdx].Id,
                        IsRead = sIdx % 3 == 0
                    });
                }
                await context.SaveChangesAsync();
            }

            // 7. Seed Academic Events (Exams, Quizzes, Assignments)
            var events = new List<AcademicEvent>
            {
                new AcademicEvent
                {
                    CourseOfferingId = offering1.Id,
                    CreatedByUserId = lecturers[0].Id,
                    Title = "Java Programming Quiz 1",
                    Description = "Multiple choice quiz on basic OOP syntax.",
                    EventType = "Quiz",
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Venue = "Online - SACS Portal",
                    MaxScore = 20,
                    Weight = 5,
                    Priority = "High",
                    Status = "Active"
                },
                new AcademicEvent
                {
                    CourseOfferingId = offering2.Id,
                    CreatedByUserId = lecturers[1].Id,
                    Title = "OS Project Submission",
                    Description = "Implement a custom round-robin scheduler simulation in C++.",
                    EventType = "Project",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    Venue = "Git Repository Link Submission",
                    MaxScore = 100,
                    Weight = 20,
                    Priority = "Critical",
                    Status = "Active"
                },
                new AcademicEvent
                {
                    CourseOfferingId = offering3.Id,
                    CreatedByUserId = lecturers[2].Id,
                    Title = "EEE210 Midterm Exam",
                    Description = "Covers circuit analysis theorems and AC circuits.",
                    EventType = "Exam",
                    DueDate = DateTime.UtcNow.AddDays(10),
                    Venue = "Lecture Theater 1",
                    MaxScore = 60,
                    Weight = 30,
                    Priority = "High",
                    Status = "Active"
                },
                new AcademicEvent
                {
                    CourseOfferingId = offering4.Id,
                    CreatedByUserId = lecturers[3].Id,
                    Title = "Structural Analysis Homework 2",
                    Description = "Solve problems 1-10 on frame reactions.",
                    EventType = "Assignment",
                    DueDate = DateTime.UtcNow.AddDays(5),
                    Venue = "Submission Box A",
                    MaxScore = 10,
                    Weight = 5,
                    Priority = "Medium",
                    Status = "Active"
                }
            };
            await context.AcademicEvents.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // 8. Seed Attendance Tracking Logs (past sessions)
            var baseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
            for (int day = 0; day < 10; day++)
            {
                var classDate = baseDate.AddDays(day);
                // CSC201 Session Attendance
                foreach (var student in students)
                {
                    // CSC201: present 80% of the time, absent 20%
                    bool present = (student.Id + day) % 5 != 0;
                    var tracking = new AttendanceTracking
                    {
                        CourseOfferingId = offering1.Id,
                        StudentId = student.Id,
                        Date = classDate,
                        Status = present ? "Present" : "Absent",
                        RecordedByUserId = lecturers[0].Id,
                        Notes = present ? $"Checked in at 09:{ (10 + (day % 15)).ToString().PadLeft(2, '0') }:00 AM" : "Unexcused absence"
                    };
                    await context.AttendanceTrackings.AddAsync(tracking);
                }

                // CSC301 Session Attendance
                foreach (var student in students)
                {
                    bool present = (student.Id + day) % 6 != 0;
                    var tracking = new AttendanceTracking
                    {
                        CourseOfferingId = offering2.Id,
                        StudentId = student.Id,
                        Date = classDate,
                        Status = present ? "Present" : "Absent",
                        RecordedByUserId = lecturers[1].Id,
                        Notes = present ? $"Checked in at 11:{ (05 + (day % 10)).ToString().PadLeft(2, '0') }:00 AM" : "Unexcused absence"
                    };
                    await context.AttendanceTrackings.AddAsync(tracking);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
