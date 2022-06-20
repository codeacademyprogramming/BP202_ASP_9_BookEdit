using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
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

        public IActionResult Detail(int id)
        {
            Book book = _context.Books
                .Include(x=>x.BookImages)
                .Include(x=>x.Genre)
                .Include(x=>x.Author)
                .Include(x=>x.BookTags).ThenInclude(x=>x.Tag)
                .FirstOrDefault(x => x.Id == id);

            if (book == null)
                return RedirectToAction("error", "dashboard");

            BookDetailViewModel bookVM = new BookDetailViewModel
            {
                Book = book,
                RelatedBooks = _context.Books.Include(x=>x.BookImages).Include(x=>x.Author).Where(x => x.GenreId == book.GenreId).Take(6).ToList(),
                BookComment = new BookCommentPostViewModel { BookId = id}
            };

            return View(bookVM);
        }


        [HttpPost]
        public async Task<IActionResult> Comment(BookCommentPostViewModel commentVM)
        {
            AppUser user = await _context.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == User.Identity.Name.ToUpper());

            BookComment comment = new BookComment
            {
                BookId = commentVM.BookId,
                AppUserId = user.Id,
                Rate = commentVM.Rate,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                Text = commentVM.Text
            };

            await _context.BookComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("detail", new { id = commentVM.BookId });
        }
    }
}
