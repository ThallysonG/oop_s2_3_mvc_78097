using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class AttendanceRecordServiceTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenDuplicateWeekExists()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldThrow_WhenDuplicateWeekExists));

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = 1,
                WeekNumber = 1,
                Present = true
            });

            await context.SaveChangesAsync();

            var service = new AttendanceRecordService(context, NullLogger<AttendanceRecordService>.Instance);

            var duplicate = new AttendanceRecord
            {
                CourseEnrolmentId = 1,
                WeekNumber = 1,
                Present = false
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(duplicate));
        }

        [Fact]
        public async Task CreateAsync_ShouldSucceed_WhenWeekIsDifferent()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldSucceed_WhenWeekIsDifferent));

            context.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = 1,
                WeekNumber = 1,
                Present = true
            });

            await context.SaveChangesAsync();

            var service = new AttendanceRecordService(context, NullLogger<AttendanceRecordService>.Instance);

            var record = new AttendanceRecord
            {
                CourseEnrolmentId = 1,
                WeekNumber = 2,
                Present = false
            };

            await service.CreateAsync(record);

            Assert.Equal(2, context.AttendanceRecords.Count());
        }
    }
}
