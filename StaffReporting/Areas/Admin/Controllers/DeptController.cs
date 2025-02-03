using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DeptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeptController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dept.Where(x => x.IsActive == true && x.IsDelete == false).ToListAsync());
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var Task = await _context.Dept.FindAsync(id);
            if (Task == null)
                return NotFound();

            return View(Task);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dept dpt)
        {
            dpt.CreatedBy = User.Identity?.Name;
            if (ModelState.IsValid)
            {
                _context.Add(dpt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dpt);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var dpt = await _context.Dept.FindAsync(id);
            if (dpt == null)
                return NotFound();
            return View(dpt);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dept dpt)
        {
            if (id != dpt.DeptId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dpt);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(dpt.DeptId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dpt);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var dpt = await _context.Dept
                .FirstOrDefaultAsync(m => m.DeptId == id);
            if (dpt == null)
                return NotFound();

            return View(dpt);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dpt = await _context.Dept.FindAsync(id);
            if (dpt == null)
                return NotFound();
            dpt.IsDelete = true;
            _context.Dept.Update(dpt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Dept.Any(e => e.DeptId == id);
        }
    }
}
