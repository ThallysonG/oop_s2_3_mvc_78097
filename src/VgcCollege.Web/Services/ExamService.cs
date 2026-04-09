using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class ExamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(ApplicationDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .OrderBy(e => e.Course!.Name)
                .ThenBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Branch)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Course>> GetCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Branch)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task CreateAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam created. ExamId: {ExamId}, Title: {Title}", exam.Id, exam.Title);
        }

        public async Task UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam updated. ExamId: {ExamId}, Title: {Title}", exam.Id, exam.Title);
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                _logger.LogWarning("Attempt to delete non-existing Exam. ExamId: {ExamId}", id);
                return;
            }

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Exam deleted. ExamId: {ExamId}", id);
        }
    }
}
