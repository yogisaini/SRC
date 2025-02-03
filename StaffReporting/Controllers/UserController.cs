﻿using Management.Data;
using Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Management.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.Include(x => x.Desi).OrderByDescending(x => x.UpdateDate).Where(p => p.IsDelete == false).ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.Include(x => x.Desi).Where(x => x.UserId == id).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            List<Desi> Depts = _context.Desi.Where(p => p.IsActive == true).ToList();
            ViewBag.Desis = Depts.Select(temp =>
              new SelectListItem() { Text = temp.DesiName, Value = temp.DesiId.ToString() }
            );
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Users user)
        {
            ModelState.Remove("Desi");
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            List<Desi> Depts = _context.Desi.Where(p => p.IsActive == true).ToList();
            ViewBag.Desis = Depts.Select(temp =>
              new SelectListItem() { Text = temp.DesiName, Value = temp.DesiId.ToString() }
            );
            ViewBag.Roles = new SelectList(new[] { "Admin", "Manager", "User" }, user.Role);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Users user)
        {
            if (id != user.UserId)
            { return NotFound(); }
            ModelState.Remove("Desi");
            if (ModelState.IsValid)
            {
                try
                {
                    var users = await _context.Users.FindAsync(id);
                    if (users != null)
                    {
                        users.Username = user.Username;
                        users.PasswordHash = user.PasswordHash;
                        users.Role = user.Role;
                        users.DesiId = user.DesiId;
                        users.Email = user.Email;
                        users.Mobile = user.Mobile;
                        users.IsActive = user.IsActive;
                        users.permission = user.permission;
                        users.UpdateDate = DateTime.Now;
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
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDelete = true;
                user.UpdateDate = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
