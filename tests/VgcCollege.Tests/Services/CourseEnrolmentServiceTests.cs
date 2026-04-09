using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class CourseEnrolmentServiceTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenStudentAlreadyEnrolledInSameCourse()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldThrow_WhenStudentAlreadyEnrolledInSameCourse));

            var course = new Course
            {
                Id = 1,
                Name = "Software Development",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 6, 1)
            };

            context.Courses.Add(course);
            context.CourseEnrolments.Add(new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 1,
                EnrolDate = new DateTime(2026, 1, 2),
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var service = new CourseEnrolmentService(context, NullLogger<CourseEnrolmentService>.Instance);

            var newEnrolment = new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 1,
                EnrolDate = new DateTime(2026, 1, 3),
                Status = "Active"
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(newEnrolment));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenStudentHasOverlappingActiveEnrolment()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldThrow_WhenStudentHasOverlappingActiveEnrolment));

            var course1 = new Course
            {
                Id = 1,
                Name = "Course A",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 6, 1)
            };

            var course2 = new Course
            {
                Id = 2,
                Name = "Course B",
                StartDate = new DateTime(2026, 3, 1),
                EndDate = new DateTime(2026, 8, 1)
            };

            context.Courses.AddRange(course1, course2);

            context.CourseEnrolments.Add(new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 1,
                EnrolDate = new DateTime(2026, 1, 2),
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var service = new CourseEnrolmentService(context, NullLogger<CourseEnrolmentService>.Instance);

            var newEnrolment = new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 2,
                EnrolDate = new DateTime(2026, 3, 2),
                Status = "Active"
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(newEnrolment));
        }

        [Fact]
        public async Task CreateAsync_ShouldSucceed_WhenPeriodsDoNotOverlap()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldSucceed_WhenPeriodsDoNotOverlap));

            var course1 = new Course
            {
                Id = 1,
                Name = "Course A",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 6, 1)
            };

            var course2 = new Course
            {
                Id = 2,
                Name = "Course B",
                StartDate = new DateTime(2026, 7, 1),
                EndDate = new DateTime(2026, 12, 1)
            };

            context.Courses.AddRange(course1, course2);

            context.CourseEnrolments.Add(new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 1,
                EnrolDate = new DateTime(2026, 1, 2),
                Status = "Active"
            });

            await context.SaveChangesAsync();

            var service = new CourseEnrolmentService(context, NullLogger<CourseEnrolmentService>.Instance);

            var newEnrolment = new CourseEnrolment
            {
                StudentProfileId = 1,
                CourseId = 2,
                EnrolDate = new DateTime(2026, 7, 2),
                Status = "Active"
            };

            await service.CreateAsync(newEnrolment);

            Assert.Equal(2, context.CourseEnrolments.Count());
        }
    }
}
