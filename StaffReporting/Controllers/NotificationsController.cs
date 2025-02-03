using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkManagementSystem.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult GetNotifications()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                int userid = Convert.ToInt32(userId);
                var writeUps = _context.WriteUps
                    .Where(x => x.UserId == userid)
                    .Select(w => new WriteUp
                    {
                        Id = w.Id,
                        WorkId = w.WorkId,
                        Comment = w.Comment != null
                                  ? w.Comment.Where(x => x.UserId != userid && x.CommentRead == false).ToList()
                                  : new List<Comment>() // Prevent null reference
                    }).ToList();

                return PartialView("_Notifications", writeUps);
            }

            return PartialView("_Notifications", new List<WriteUp>());
        }


    }
}
