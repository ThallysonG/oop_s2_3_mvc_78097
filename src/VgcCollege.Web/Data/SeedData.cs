using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

            await context.Database.EnsureCreatedAsync();

            string[] roles = { "Administrator", "Faculty", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Role created: {Role}", role);
                }
            }

            var adminUser = await CreateUserAsync(userManager, "admin@vgc.com", "123456", "Administrator", logger);
            var facultyUser = await CreateUserAsync(userManager, "faculty@vgc.com", "123456", "Faculty", logger);
            var studentUser1 = await CreateUserAsync(userManager, "student1@vgc.com", "123456", "Student", logger);
            var studentUser2 = await CreateUserAsync(userManager, "student2@vgc.com", "123456", "Student", logger);

            if (await context.Branches.AnyAsync())
            {
                logger.LogInformation("Seed skipped because Branches already exist.");
                return;
            }

            var branches = new List<Branch>
            {
                new() { Name = "Dublin Branch", Address = "1 Main Street, Dublin" },
                new() { Name = "Cork Branch", Address = "22 River Road, Cork" },
                new() { Name = "Galway Branch", Address = "15 Harbour View, Galway" }
            };

            context.Branches.AddRange(branches);
            await context.SaveChangesAsync();

            var courses = new List<Course>
            {
                new() { Name = "Software Development", BranchId = branches[0].Id, StartDate = new DateTime(2026, 1, 10), EndDate = new DateTime(2026, 12, 20) },
                new() { Name = "Business Analytics", BranchId = branches[1].Id, StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 11, 30) },
                new() { Name = "Cyber Security", BranchId = branches[2].Id, StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 12, 15) }
            };

            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            var facultyProfile = new FacultyProfile
            {
                IdentityUserId = facultyUser.Id,
                Name = "Mary Faculty",
                Email = facultyUser.Email,
                Phone = "0871111111"
            };

            var studentProfile1 = new StudentProfile
            {
                IdentityUserId = studentUser1.Id,
                Name = "John Student",
                Email = studentUser1.Email,
                Phone = "0872222222",
                Address = "Dublin City",
                DOB = new DateTime(2002, 5, 10)
            };

            var studentProfile2 = new StudentProfile
            {
                IdentityUserId = studentUser2.Id,
                Name = "Sarah Student",
                Email = studentUser2.Email,
                Phone = "0873333333",
                Address = "Cork City",
                DOB = new DateTime(2001, 9, 20)
            };

            context.FacultyProfiles.Add(facultyProfile);
            context.StudentProfiles.AddRange(studentProfile1, studentProfile2);
            await context.SaveChangesAsync();

            var enrolments = new List<CourseEnrolment>
            {
                new() { StudentProfileId = studentProfile1.Id, CourseId = courses[0].Id, EnrolDate = new DateTime(2026, 1, 15), Status = "Active" },
                new() { StudentProfileId = studentProfile2.Id, CourseId = courses[1].Id, EnrolDate = new DateTime(2026, 2, 5), Status = "Active" }
            };

            context.CourseEnrolments.AddRange(enrolments);
            await context.SaveChangesAsync();

            var attendanceRecords = new List<AttendanceRecord>
            {
                new() { CourseEnrolmentId = enrolments[0].Id, WeekNumber = 1, Present = true },
                new() { CourseEnrolmentId = enrolments[0].Id, WeekNumber = 2, Present = true },
                new() { CourseEnrolmentId = enrolments[1].Id, WeekNumber = 1, Present = false },
                new() { CourseEnrolmentId = enrolments[1].Id, WeekNumber = 2, Present = true }
            };

            context.AttendanceRecords.AddRange(attendanceRecords);
            await context.SaveChangesAsync();

            var assignments = new List<Assignment>
            {
                new() { CourseId = courses[0].Id, Title = "C# Fundamentals", MaxScore = 100, DueDate = new DateTime(2026, 4, 15) },
                new() { CourseId = courses[1].Id, Title = "Data Analysis Report", MaxScore = 100, DueDate = new DateTime(2026, 4, 20) }
            };

            context.Assignments.AddRange(assignments);
            await context.SaveChangesAsync();

            var assignmentResults = new List<AssignmentResult>
            {
                new() { AssignmentId = assignments[0].Id, StudentProfileId = studentProfile1.Id, Score = 82, Feedback = "Very good work" },
                new() { AssignmentId = assignments[1].Id, StudentProfileId = studentProfile2.Id, Score = 74, Feedback = "Good structure" }
            };

            context.AssignmentResults.AddRange(assignmentResults);
            await context.SaveChangesAsync();

            var exams = new List<Exam>
            {
                new() { CourseId = courses[0].Id, Title = "Semester Exam", Date = new DateTime(2026, 6, 10), MaxScore = 100, ResultsReleased = true },
                new() { CourseId = courses[1].Id, Title = "Final Exam", Date = new DateTime(2026, 6, 20), MaxScore = 100, ResultsReleased = false }
            };

            context.Exams.AddRange(exams);
            await context.SaveChangesAsync();

            var examResults = new List<ExamResult>
            {
                new() { ExamId = exams[0].Id, StudentProfileId = studentProfile1.Id, Score = 78, Grade = "B" },
                new() { ExamId = exams[1].Id, StudentProfileId = studentProfile2.Id, Score = 69, Grade = "C" }
            };

            context.ExamResults.AddRange(examResults);
            await context.SaveChangesAsync();

            logger.LogInformation("Full seed completed successfully.");
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role,
            ILogger logger)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
                return user;

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("User created: {Email} with role {Role}", email, role);
                return user;
            }

            logger.LogError("Failed to create user {Email}: {Errors}",
                email,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            throw new Exception($"Could not create seed user {email}");
        }
    }
}
