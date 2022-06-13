using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using System.Linq;

namespace Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class GenreController : Controller
    {
        private readonly AppDbContext _context;

        public GenreController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var model = _context.Genres.Include(x => x.Books).ToList();
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Genre genre)
        {
            if (!ModelState.IsValid)
                return View();

            if (_context.Genres.Any(x => x.Name == genre.Name))
            {
                ModelState.AddModelError("Name", "This genre already exist");
                return View();
            }

            _context.Genres.Add(genre);
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public IActionResult Edit(int id)
        {
            Genre genre = _context.Genres.FirstOrDefault(x => x.Id == id);
            
            if (genre == null)
                return RedirectToAction("error", "dashboard");

            return View(genre);
        }

        [HttpPost]
        public IActionResult Edit(int id,Genre genre)
        {
            Genre existGenre = _context.Genres.FirstOrDefault(x => x.Id == id);

            if (existGenre == null)
                return RedirectToAction("error", "dashboard");

            if(_context.Genres.Any(x=>x.Id!=id && x.Name == genre.Name))
            {
                ModelState.AddModelError("Name", "This genre already exist");
                return View();
            }

            existGenre.Name = genre.Name;
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public IActionResult Delete(int id)
        {
            Genre genre = _context.Genres.FirstOrDefault(x => x.Id == id);

            if (genre == null)
                return RedirectToAction("error", "dashboard");

            _context.Genres.Remove(genre);
            _context.SaveChanges();

            return RedirectToAction("index");
        }
    }
}
