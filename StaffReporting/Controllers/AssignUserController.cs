using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Manager,User,Default")]
    public class AssignUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AssignUserController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ProjectAssignUser(int? Id)
        {
            if (Id == null) return BadRequest("Invalid Work Id");

            var usernames = _context.WriteUps
                            .Where(w => w.Work.Id == Id)
                            .Select(w => new ProjectAssignUser
                            {
                                UserID = w.Users.UserId,
                                UserName = w.Users != null ? w.Users.Username : "Unknown",
                                Designation = w.Users.Desi.DesiName != null ? w.Users.Desi.DesiName : "Unknown"
                            })
                            .Distinct()
                            .ToList();

            return PartialView("_ProjectAssignUser", usernames);
        }
    }
}
