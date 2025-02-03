using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
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
        public IActionResult Index(int page = 1, int? TaskID = null, int? UserId = null, int? CommentID = null, int? WriteUpId = null, int? WorkId = null, int? DeptId = null, string sortOrder = "date_desc", DateTime? FromDate = null, DateTime? ToDate = null)
        {
            int pageSize = 50; // Number of records per page
            ViewBag.Task = _context.Tasklist.Where(x => x.IsActive == true && x.IsDelete == false)
                .Select(u => new SelectListItem
                {
                    Value = u.TaskID.ToString(),
                    Text = u.TaskName,
                    Selected = TaskID.HasValue && u.TaskID == TaskID.Value
                })
                .ToList();
            // Populate ViewBag.User with a list of users for the dropdown
            ViewBag.Dept = _context.Dept.Where(x => x.IsActive == true && x.IsDelete == false)
                .Select(u => new SelectListItem
                {
                    Value = u.DeptId.ToString(),
                    Text = u.DeptName,
                    Selected = DeptId.HasValue && u.DeptId == DeptId.Value
                })
                .ToList();

            // Populate ViewBag.User with a list of users for the dropdown
            ViewBag.User = _context.Users.Where(x => x.IsActive == true && x.IsDelete == false)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Username + "-(" + u.Desi.DesiName + ')',
                    Selected = UserId.HasValue && u.UserId == UserId.Value
                })
                .ToList();

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
                       new SelectListItem { Value = "username_asc", Text = "Username (Ascending)", Selected = sortOrder == "username_asc" },
                       new SelectListItem { Value = "username_desc", Text = "Username (Descending)", Selected = sortOrder == "username_desc" },
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
                .Include(z => z.Linkpaths)
                .Include(z => z.Tasklist)
                .Include(a => a.Comment).ThenInclude(l => l.Users);

            // Apply the filter if UserId is provided
            if (UserId.HasValue)
            {
                query = query.Where(q => q.UserId == UserId.Value);
            }
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
            if (TaskID.HasValue)
            {
                query = query.Where(q => q.Tasklist.TaskID == TaskID.Value);
            }
            // Apply the filter if WorkId is provided
            if (WorkId.HasValue)
            {
                query = query.Where(q => q.Work.Id == WorkId.Value);
            }
            if (DeptId.HasValue)
            {
                query = query.Where(q => q.Work.DeptId == DeptId.Value);
            }
            // Apply the filter if endDate is provided
            if (FromDate.HasValue)
            {
                query = query.Where(q => q.SubmittedDate >= FromDate.Value);
            }
            if (ToDate.HasValue)
            {
                query = query.Where(q => q.SubmittedDate <= ToDate.Value);
            }
            // Apply ordering based on the sortOrder parameter
            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(q => q.SubmittedDate),
                "username_asc" => query.OrderBy(q => q.Users.Username),
                "username_desc" => query.OrderByDescending(q => q.Users.Username),
                "project_asc" => query.OrderBy(q => q.Work.Title),
                "project_desc" => query.OrderByDescending(q => q.Work.Title),
                _ => query.OrderByDescending(q => q.SubmittedDate), // Default to date ascending
            };

            // Get the total count of filtered records
            int totalRecords = query.Count();
            // Pass data to the view
            ViewBag.CurrentPage = page;
            ViewBag.SelectedUserId = UserId;
            ViewBag.SelectedWorkId = WorkId;
            ViewBag.SelectedDeptId = DeptId;
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
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

            ViewBag.TotalPages = totalPages;
            return View(writeUps);
        }
    }
}
