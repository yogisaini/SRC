using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Management.Controllers
{
    [Authorize(Roles = "Admin,Manager,Default")]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminWork/Index
        public IActionResult Index()
        {
            var Works = _context.Works
            .Include(w => w.Users) // Include Users (FK UserId)
            .Include(w => w.Dept)
            .OrderByDescending(x => x.UpdateDate)  // Include Dept (FK DeptId)
            .Where(w => !w.IsDelete)
            .ToList();
            return View(Works);
        }

        // GET: AdminWork/Create
        public IActionResult Create()
        {
            List<Dept> Depts = _context.Dept.Where(p => p.IsActive == true && p.IsDelete == false).ToList();
            ViewBag.Depts = Depts.Select(temp =>
              new SelectListItem() { Text = temp.DeptName, Value = temp.DeptId.ToString() }
            );
            List<Users> users = _context.Users.Where(p => p.Role == "Manager" && p.IsActive == true && p.IsDelete == false).ToList();
            ViewBag.Manager = users.Select(temp =>
              new SelectListItem() { Text = temp.Username, Value = temp.UserId.ToString() }
            );
            return View();
        }

        // POST: AdminWork/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Work Work)
        {
            ModelState.Remove("Users");
            ModelState.Remove("Dept");
            if (ModelState.IsValid)
            {
                int newCode = _context.Works.Count();
                Work.Code = "GBP" + (newCode + 1000);
                Work.CreatedDate = DateTime.Now;
                Work.CreatedBy = User.Identity?.Name;
                Work.UpdateDate = DateTime.Now;
                Work.UpdateBy = User.Identity?.Name;
                _context.Works.Add(Work);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Create");
        }

        // GET: AdminWork/Edit/5
        public IActionResult Edit(int id)
        {
            var Work = _context.Works.FirstOrDefault(t => t.Id == id);
            if (Work == null)
            {
                return NotFound();
            }
            List<Dept> Depts = _context.Dept.Where(p => p.IsActive == true && p.IsDelete == false).ToList();
            ViewBag.Depts = Depts.Select(temp =>
              new SelectListItem() { Text = temp.DeptName, Value = temp.DeptId.ToString() }
            );
            List<Users> users = _context.Users.Where(p => p.Role == "Manager" && p.IsActive == true && p.IsDelete == false).ToList();
            ViewBag.Manager = users.Select(temp =>
              new SelectListItem() { Text = temp.Username, Value = temp.UserId.ToString() }
            );
            return View(Work);
        }

        // POST: AdminWork/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Work updatedWork)
        {
            if (id != updatedWork.Id)
            {
                return BadRequest();
            }
            ModelState.Remove("Users");
            ModelState.Remove("Dept");
            if (ModelState.IsValid)
            {
                var Work = _context.Works.FirstOrDefault(t => t.Id == id);
                if (Work == null)
                {
                    return NotFound();
                }

                Work.Title = updatedWork.Title;
                Work.Description = updatedWork.Description;
                Work.UserId = updatedWork.UserId;
                Work.DeptId = updatedWork.DeptId;
                Work.IsActive = updatedWork.IsActive;
                Work.UpdateDate = DateTime.Now;
                Work.UpdateBy = User.Identity?.Name;
                Work.IsComplete = updatedWork.IsComplete;
                Work.DeadLine = updatedWork.DeadLine;
                _context.Works.Update(Work);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedWork);
        }

        // GET: AdminWork/Details/5
        public IActionResult Details(int id)
        {
            var Works = _context.Works.Include(x => x.Users).Include(x => x.Dept).FirstOrDefault(t => t.Id == id);
            if (Works == null)
            {
                return NotFound();
            }
            return View(Works);
        }

        // GET: AdminWork/Delete/5
        public IActionResult Delete(int id)
        {
            var Works = _context.Works.Include(x => x.Users).Include(x => x.Dept).FirstOrDefault(t => t.Id == id);
            if (Works == null)
            {
                return NotFound();
            }
            return View(Works);
        }

        // POST: AdminWork/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var Work = _context.Works.FirstOrDefault(t => t.Id == id);
            if (Work == null)
            {
                return NotFound();
            }
            Work.IsDelete = true;
            Work.UpdateDate = DateTime.Now;
            Work.UpdateBy = User.Identity?.Name;
            _context.Works.Update(Work);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
