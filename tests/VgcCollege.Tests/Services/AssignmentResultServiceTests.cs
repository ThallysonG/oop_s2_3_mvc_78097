using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;
using Xunit;

namespace VgcCollege.Tests.Services
{
    public class AssignmentResultServiceTests
    {
        private ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenDuplicateAssignmentResultExists()
        {
            using var context = CreateContext(nameof(CreateAsync_ShouldThrow_WhenDuplicateAssignmentResultExists));

            context.AssignmentResults.Add(new AssignmentResult
            {
                AssignmentId = 1,
                StudentProfileId = 1,
                Score = 80,
                Feedback = "Good"
            });

            await context.SaveChangesAsync();

            var service = new AssignmentResultService(context, NullLogger<AssignmentResultService>.Instance);

            var duplicate = new AssignmentResult
            {
                AssignmentId = 1,
                StudentProfileId = 1,
                Score = 85,
                Feedback = "Better"
            };

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(duplicate));
        }
    }
}
