using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "User,Default")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? viewtype)
        {
            // Retrieve the UserId claim
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return NotFound(); // Return NotFound if the claim is missing or invalid
            }

            // Fetch the user from the database with their related Desi
            var user = await _context.Users
                .Include(u => u.Desi)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound(); // Return NotFound if the user doesn't exist
            }
            if (viewtype == 1)
            {
                // Return the user to the view
                return View(user);
            }
            else
            {
                return View("Details", user);
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int UserId, Users user)
        {
            if (UserId != user.UserId)
            { return NotFound(); }
            ModelState.Remove("Desi");
            if (ModelState.IsValid)
            {
                try
                {
                    var users = await _context.Users.FindAsync(UserId);
                    if (users != null)
                    {
                        users.PasswordHash = user.PasswordHash;
                        _context.ChangeTracker.Clear();
                        _context.Update(users);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Edit));
            }
            return View(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
