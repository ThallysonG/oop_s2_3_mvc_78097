using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Services
{
    public class BranchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchService> _logger;

        public BranchService(ApplicationDbContext context, ILogger<BranchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Branch>> GetAllAsync()
        {
            return await _context.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Branches.FindAsync(id);
        }

        public async Task CreateAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Branch created. BranchId: {BranchId}, Name: {Name}", branch.Id, branch.Name);
        }

        public async Task UpdateAsync(Branch branch)
        {
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Branch updated. BranchId: {BranchId}, Name: {Name}", branch.Id, branch.Name);
        }

        public async Task DeleteAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                _logger.LogWarning("Attempt to delete non-existing Branch. BranchId: {BranchId}", id);
                return;
            }

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Branch deleted. BranchId: {BranchId}", id);
        }
    }
}
