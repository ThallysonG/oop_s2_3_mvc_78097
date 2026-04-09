using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class AssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(ApplicationDbContext context, ILogger<AssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Assignment>> GetAllAsync()
        {
            return await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(a => a.Course!.Name)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                    .ThenInclude(c => c!.Branch)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Branch)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment created. AssignmentId: {AssignmentId}, Title: {Title}",
                assignment.Id, assignment.Title);
        }

        public async Task UpdateAsync(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment updated. AssignmentId: {AssignmentId}, Title: {Title}",
                assignment.Id, assignment.Title);
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                _logger.LogWarning("Attempt to delete non-existing Assignment. AssignmentId: {AssignmentId}", id);
                return;
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assignment deleted. AssignmentId: {AssignmentId}", id);
        }
    }
}
