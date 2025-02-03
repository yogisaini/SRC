using System.Security.Claims;
using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace WorkManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;
        private readonly ICompositeViewEngine _viewEngine;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<AccountController> logger, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _viewEngine = viewEngine;
        }

        [Authorize(Roles = "Admin,Manager,User,Default")]
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
                        .OrderBy(c => c.CommentDate)
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
        [HttpGet]
        public IActionResult Login()
        {
            if (User.IsInRole("User") || User.IsInRole("Default"))
            {
                return RedirectToDashboard("User");
            }
            if (User.IsInRole("Manager"))
            {
                return RedirectToDashboard("Manager");
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToDashboard("Admin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Try to find the user by either email or mobile
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Username || u.Mobile == model.Username && u.IsActive == true && u.IsDelete == false);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    if (user.permission == true)
                    {
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, "Default"),
                        new Claim("UserId", user.UserId.ToString())
                    };
                        // Create identity
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Sign in the user
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity));
                    }
                    else
                    {
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.UserId.ToString())
                    };
                        // Create identity
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Sign in the user
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(identity));
                    }
                    // Redirect based on role
                    return RedirectToDashboard(user.Role);
                }

                ModelState.AddModelError("", "Invalid Email/Mobile or password");
                _logger.LogWarning("Failed login attempt for username: {Username}", model.Username);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToDashboard(string role)
        {
            if (role.Contains("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            if (role.Contains("Manager"))
                return RedirectToAction("Index", "Dashboard", new { area = "Manager" });
            if (role.Contains("User"))
                return RedirectToAction("Index", "Dashboard", new { area = "User" });

            return RedirectToAction("Login", "Account"); // Default fallback
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return (password == passwordHash ? true : false);
        }
    }
}
