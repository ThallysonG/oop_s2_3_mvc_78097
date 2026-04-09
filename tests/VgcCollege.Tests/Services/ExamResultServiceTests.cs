using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class ExamResultServiceTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetVisibleByStudentEmailAsync_ShouldReturnOnlyReleasedResults()
        {
            using var context = CreateContext(nameof(GetVisibleByStudentEmailAsync_ShouldReturnOnlyReleasedResults));

            var user = new ApplicationUser
            {
                Id = "student-1",
                Email = "student1@vgc.com",
                UserName = "student1@vgc.com"
            };

            var branch = new Branch
            {
                Id = 1,
                Name = "Dublin Branch",
                Address = "Main Street"
            };

            var course = new Course
            {
                Id = 1,
                Name = "Software Development",
                BranchId = 1,
                Branch = branch,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 6, 1)
            };

            var student = new StudentProfile
            {
                Id = 1,
                IdentityUserId = user.Id,
                Name = "Student One",
                Email = user.Email,
                User = user
            };

            var releasedExam = new Exam
            {
                Id = 1,
                Title = "Released Exam",
                CourseId = 1,
                Course = course,
                Date = new DateTime(2026, 5, 1),
                MaxScore = 100,
                ResultsReleased = true
            };

            var hiddenExam = new Exam
            {
                Id = 2,
                Title = "Hidden Exam",
                CourseId = 1,
                Course = course,
                Date = new DateTime(2026, 5, 15),
                MaxScore = 100,
                ResultsReleased = false
            };

            context.Users.Add(user);
            context.Branches.Add(branch);
            context.Courses.Add(course);
            context.StudentProfiles.Add(student);
            context.Exams.AddRange(releasedExam, hiddenExam);

            context.ExamResults.AddRange(
                new ExamResult
                {
                    ExamId = 1,
                    Exam = releasedExam,
                    StudentProfileId = 1,
                    StudentProfile = student,
                    Score = 75,
                    Grade = "B"
                },
                new ExamResult
                {
                    ExamId = 2,
                    Exam = hiddenExam,
                    StudentProfileId = 1,
                    StudentProfile = student,
                    Score = 90,
                    Grade = "A"
                });

            await context.SaveChangesAsync();

            var service = new ExamResultService(context, NullLogger<ExamResultService>.Instance);

            var results = await service.GetVisibleByStudentEmailAsync("student1@vgc.com");

            Assert.Single(results);
            Assert.Equal("Released Exam", results.First().Exam?.Title);
        }
    }
}
