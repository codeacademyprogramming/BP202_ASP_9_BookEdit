using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using System;
using System.Linq;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AuthorController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int page=1)
        {
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.Authors.Include(x => x.Books).Where(x => !x.IsDeleted).Count() / 2d);
            var data = _context.Authors.Include(x=>x.Books).Where(x=>!x.IsDeleted).Skip((page-1)*2).Take(2).ToList();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Author author)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            _context.Authors.Add(author);
            author.CreatedAt = DateTime.UtcNow.AddHours(4);
            author.ModifiedAt = DateTime.UtcNow.AddHours(4);

            AppUser admin = _context.Users.FirstOrDefault(c => c.UserName == User.Identity.Name);
            author.ModifiedBy = admin.Id;

            _context.SaveChanges();

            return RedirectToAction("index");
        }


        public IActionResult Edit(int id)
        {
            Author author = _context.Authors.FirstOrDefault(x => x.Id == id && !x.IsDeleted);

            if (author == null)
                return RedirectToAction("error", "dashboard");

            return View(author);
        }

        [HttpPost]
        public IActionResult Edit(Author author)
        {
            if (!ModelState.IsValid)
                return View();

            Author existAuth = _context.Authors.FirstOrDefault(x => x.Id == author.Id && !x.IsDeleted);

            if (existAuth == null)
                return RedirectToAction("error", "dashboard");

            existAuth.FullName = author.FullName;
            existAuth.BirthDate = author.BirthDate;

            existAuth.ModifiedAt = DateTime.UtcNow.AddHours(4);

            AppUser admin = _context.Users.FirstOrDefault(c => c.UserName == User.Identity.Name);
            existAuth.ModifiedBy = admin.Id;

            _context.SaveChanges();

            return RedirectToAction("index");
        }


        public IActionResult Delete(int id)
        {
            Author author = _context.Authors.Include(x=>x.Books).FirstOrDefault(x => x.Id == id);

            if (author == null)
                return RedirectToAction("error", "dashboard");

            author.ModifiedAt = DateTime.UtcNow.AddHours(4);

            AppUser admin = _context.Users.FirstOrDefault(c => c.UserName == User.Identity.Name);
            author.ModifiedBy = admin.Id;

            return View(author);
        }
            
        [HttpPost]
        public IActionResult Delete(Author author)
        {
            Author existAuth = _context.Authors.FirstOrDefault(x => x.Id == author.Id);
           

            if(existAuth == null)
                return RedirectToAction("error", "dashboard");

            //_context.Authors.Remove(existAuth);
            existAuth.IsDeleted = true;
            existAuth.ModifiedAt = DateTime.UtcNow.AddHours(4);

            AppUser admin = _context.Users.FirstOrDefault(c => c.UserName == User.Identity.Name);
            existAuth.ModifiedBy = admin.Id;
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public IActionResult SweetDelete(int id)
        {
            Author author = _context.Authors.FirstOrDefault(x => x.Id == id);

            if (author == null)
                return NotFound();

            //_context.Authors.Remove(author);
            author.ModifiedAt = DateTime.UtcNow.AddHours(4);

            AppUser admin = _context.Users.FirstOrDefault(c => c.UserName == User.Identity.Name);
            author.ModifiedBy = admin.Id;
            author.IsDeleted = true;
            _context.SaveChanges();

            return Ok();
        }

    }
}
