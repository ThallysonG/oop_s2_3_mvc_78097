using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class CourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseService> _logger;

        public CourseService(ApplicationDbContext context, ILogger<CourseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Branch)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Branch>> GetBranchesAsync()
        {
            return await _context.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course created. CourseId: {CourseId}, Name: {Name}", course.Id, course.Name);
        }

        public async Task UpdateAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course updated. CourseId: {CourseId}, Name: {Name}", course.Id, course.Name);
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                _logger.LogWarning("Attempt to delete non-existing Course. CourseId: {CourseId}", id);
                return;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Course deleted. CourseId: {CourseId}", id);
        }
    }
}
