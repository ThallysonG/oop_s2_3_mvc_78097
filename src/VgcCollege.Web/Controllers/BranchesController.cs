using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VgcCollege.Web.Models;
using VgcCollege.Web.Services;

namespace VgcCollege.Web.Controllers
{
    [Authorize]
    public class BranchesController : Controller
    {
        private readonly BranchService _branchService;

        public BranchesController(BranchService branchService)
        {
            _branchService = branchService;
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Index()
        {
            var items = await _branchService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "Administrator,Faculty,Student")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _branchService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (!ModelState.IsValid)
                return View(branch);

            await _branchService.CreateAsync(branch);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _branchService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(branch);

            await _branchService.UpdateAsync(branch);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _branchService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _branchService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
