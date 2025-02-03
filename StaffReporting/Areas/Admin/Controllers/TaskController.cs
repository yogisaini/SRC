using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tasklist.Where(x => x.IsActive == true && x.IsDelete == false).ToListAsync());
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var Task = await _context.Tasklist.FindAsync(id);
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
        public async Task<IActionResult> Create(Tasklist Task)
        {
            if (ModelState.IsValid)
            {
                _context.Add(Task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Task);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var Task = await _context.Tasklist.FindAsync(id);
            if (Task == null)
                return NotFound();
            return View(Task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tasklist Task)
        {
            if (id != Task.TaskID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.ChangeTracker.Clear();
                    _context.Update(Task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(Task.TaskID))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var Task = await _context.Tasklist
                .FirstOrDefaultAsync(m => m.TaskID == id);
            if (Task == null)
                return NotFound();

            return View(Task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Task = await _context.Tasklist.FindAsync(id);
            if (Task == null)
                return NotFound();
            Task.IsDelete = true;
            _context.Tasklist.Update(Task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(int id)
        {
            return _context.Tasklist.Any(e => e.TaskID == id);
        }
    }
}
