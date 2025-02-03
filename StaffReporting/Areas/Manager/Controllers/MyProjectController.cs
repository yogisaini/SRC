using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Management.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = "Manager")]
    public class MyProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MyProjectController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var myProject = _context.WriteUps
                            .Where(w => w.UserId == Convert.ToInt32(userId))
                            .Select(w => new Work
                            {
                                Id = w.Work.Id,
                                Title = w.Work.Title != null ? w.Work.Title : "Unknown",
                                Description = w.Work.Description,
                                Code = w.Work.Code,
                                DeadLine = w.Work.DeadLine
                            })
                            .Distinct()
                            .ToList();
                return View(myProject);
            }
            else
            {
                return View(new List<Work>());
            }
        }
    }
}
