using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }
        //public IActionResult GetDetail(int id)
        //{
        //    Book book = _context.Books.Include(x=>x.BookImages).FirstOrDefault(x => x.Id == id);

        //    if (book == null)
        //        return NotFound();

        //    return Json(new { name = book.Name,poster = book.BookImages.FirstOrDefault(x=>x.PosterStatus==true)?.Name });
        //}

        public IActionResult GetBookModal(int id)
        {
            Book book = _context.Books.Include(x => x.Genre).Include(x => x.Author).Include(x => x.BookImages).FirstOrDefault(x => x.Id == id);

            if (book == null)
                return NotFound();

            return PartialView("_BookModalPartial", book);
        }
    }
}
