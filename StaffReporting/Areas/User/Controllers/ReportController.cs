using System.Threading.Tasks;
using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User,Default")]
    public class ReportController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        public ReportController(ApplicationDbContext context, IWebHostEnvironment environment, ICompositeViewEngine viewEngine)
        {
            _environment = environment;
            _context = context;
            _viewEngine = viewEngine; // Inject the view engine
        }

        public IActionResult comman()
        {
            var abc = _context.WriteUps
             .Where(x => x.UserId == 3)
             .Select(x => new
             {
                 WriteUp = x,
                 Comments = x.Comment
             })
             .ToList();

            return View(abc);
        }
        public IActionResult Index(int page = 1, int? WorkId = null, int? CommentID = null, int? WriteUpId = null, string sortOrder = "date_desc")
        {
            if (WorkId > 0)
            {
                int pageSize = 50; // Number of records per page

                // Populate ViewBag.User with a list of users for the dropdown  

                // Populate ViewBag.Work with a list of work titles for the dropdown
                ViewBag.Work = _context.Works.Include(x => x.Dept).Where(x => x.IsActive == true && x.IsDelete == false)
                    .Select(w => new SelectListItem
                    {
                        Value = w.Id.ToString(),
                        Text = w.Code + '-' + w.Title + '-' + w.Dept.DeptName,
                        Selected = WorkId.HasValue && w.Id == WorkId.Value
                    })
                    .ToList();

                ViewBag.SortOrderList = new List<SelectListItem>
                   {
                       new SelectListItem { Value = "date_asc", Text = "Date (Ascending)", Selected = sortOrder == "date_asc" },
                       new SelectListItem { Value = "date_desc", Text = "Date (Descending)", Selected = sortOrder == "date_desc" },
                       new SelectListItem { Value = "project_asc", Text = "Project (Ascending)", Selected = sortOrder == "project_asc" },
                       new SelectListItem { Value = "project_desc", Text = "Project (Descending)", Selected = sortOrder == "project_desc" },
                   };


                // Store sorting order in ViewBag for maintaining state in the view
                ViewBag.SortOrder = sortOrder;

                // Initialize the query as IQueryable<WriteUp>
                IQueryable<WriteUp> query = _context.WriteUps
                    .Include(x => x.Work).ThenInclude(w => w.Dept)
                    .Include(x => x.Work).ThenInclude(w => w.Users)
                    .Include(x => x.Users).ThenInclude(w => w.Desi)
                    .Include(x => x.Users)
                    .Include(z => z.Linkpaths)
                    .Include(z => z.Tasklist)
                    .Include(a => a.Comment).ThenInclude(l => l.Users);

                if (CommentID.HasValue)
                {
                    var data = _context.Comment.FirstOrDefault(x => x.CommentID == CommentID.Value);
                    if (data != null)
                    {
                        data.CommentRead = true;
                        _context.SaveChanges();
                    }
                }

                // Apply the filter if WorkId is provided
                if (WriteUpId.HasValue)
                {
                    query = query.Where(q => q.Id == WriteUpId.Value);
                }
                if (WorkId.HasValue)
                {
                    query = query.Where(q => q.Work.Id == WorkId.Value);
                }
                // Apply ordering based on the sortOrder parameter
                query = sortOrder switch
                {
                    "date_asc" => query.OrderBy(q => q.SubmittedDate),
                    "project_asc" => query.OrderBy(q => q.Work.Title),
                    "project_desc" => query.OrderByDescending(q => q.Work.Title),
                    _ => query.OrderByDescending(q => q.SubmittedDate), // Default to date ascending
                };

                // Get the total count of filtered records
                int totalRecords = query.Count();

                // Handle no records scenario
                if (totalRecords == 0)
                {
                    ViewBag.CurrentPage = 1;
                    ViewBag.TotalPages = 1;
                    if (WorkId.HasValue)
                    {
                        int idd = WorkId ?? 0;
                        ViewBag.workdata = _context.Works.Where(x => x.Id == WorkId).FirstOrDefault();
                    }
                    return View(new List<WriteUp>());
                }

                // Calculate the total number of pages
                int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                // Ensure the requested page is within valid range
                if (page < 1) page = 1;
                if (page > totalPages) page = totalPages;

                // Fetch the paginated data
                var writeUps = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Pass data to the view
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SelectedWorkId = WorkId;
                return View(writeUps);
            }
            else
            {
                ViewBag.Work = _context.Works.Where(x => x.IsActive == true && x.IsDelete == false)
                    .Select(w => new SelectListItem
                    {
                        Value = w.Id.ToString(),
                        Text = w.Code + '-' + w.Title + '-' + w.Dept.DeptName,
                        Selected = WorkId.HasValue && w.Id == WorkId.Value
                    })
                    .ToList();
                ViewBag.SortOrderList = new List<SelectListItem>
                   {
                       new SelectListItem { Value = "date_asc", Text = "Date (Ascending)", Selected = sortOrder == "date_asc" },
                       new SelectListItem { Value = "date_desc", Text = "Date (Descending)", Selected = sortOrder == "date_desc" },
                       new SelectListItem { Value = "project_asc", Text = "Project (Ascending)", Selected = sortOrder == "project_asc" },
                       new SelectListItem { Value = "project_desc", Text = "Project (Descending)", Selected = sortOrder == "project_desc" },
                   };
                List<WriteUp> writeUps = new List<WriteUp>();
                return View(writeUps);
            }
        }

        [HttpPost]
        public IActionResult AddComment(int TaskId, string Comment)
        {
            if (!string.IsNullOrEmpty(Comment) && TaskId != 0)
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    Comment model = new Comment
                    {
                        CommentText = Comment,
                        CommentDate = DateTime.Now,
                        UserId = Convert.ToInt32(userId),
                        WriteUpId = TaskId
                    };
                    _context.Comment.Add(model);
                    _context.SaveChanges();

                    // Return updated comments section
                    var comments = _context.Comment
                        .Where(c => c.WriteUpId == TaskId)
                        .Include(c => c.Users)
                        .OrderByDescending(c => c.CommentDate)
                        .ToList();

                    string html = RenderPartialViewToString("_CommentsPartial", comments);
                    return Json(new { success = true, html });
                }
            }
            return Json(new { success = false });
        }

        // Helper to render a partial view as a string
        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new InvalidOperationException($"View {viewName} not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
