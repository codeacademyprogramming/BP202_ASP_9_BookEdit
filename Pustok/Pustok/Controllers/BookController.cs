using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class BookController : BaseController
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
            BookDetailViewModel bookVM = GetBookDetailVM(id);

            if (bookVM == null)
                return RedirectToAction("error", "dashboard");

            return View(bookVM);
        }

        private BookDetailViewModel GetBookDetailVM(int id)
        {
            Book book = _context.Books
                .Include(x => x.BookImages)
                .Include(x => x.Genre)
                .Include(x => x.Author)
                .Include(x => x.BookComments).ThenInclude(bc => bc.AppUser)
                .Include(x => x.BookTags).ThenInclude(x => x.Tag)
                .FirstOrDefault(x => x.Id == id);

            if (book == null)
                return null;

            BookDetailViewModel bookVM = new BookDetailViewModel
            {
                Book = book,
                RelatedBooks = _context.Books.Include(x => x.BookImages).Include(x => x.Author).Where(x => x.GenreId == book.GenreId).Take(6).ToList(),
                BookComment = new BookCommentPostViewModel { BookId = id },
            };

            return bookVM;
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Comment(BookCommentPostViewModel commentVM)
        {
            if (!ModelState.IsValid)
            {
                var model = GetBookDetailVM(commentVM.BookId);

                if (model == null) return RedirectToAction("error", "home");
                else
                {
                    model.BookComment = commentVM;
                    return View("Detail", model);
                }
            }

            if (_context.BookComments.Any(x => x.BookId == commentVM.BookId && x.AppUserId == UserId))
            {
                ModelState
                    .AddModelError("", "Siz artiq comment yazmisiniz!");
            }

            Book book = _context.Books.Include(x => x.BookComments).FirstOrDefault(x => x.Id == commentVM.BookId);
            if (book == null)
                return RedirectToAction("error", "home");

            //AppUser user = await _context.Users.FirstOrDefaultAsync(x =>!x.IsAdmin && x.NormalizedUserName == User.Identity.Name.ToUpper());


            BookComment comment = new BookComment
            {
                BookId = commentVM.BookId,
                AppUserId = UserId,
                Rate = commentVM.Rate,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                Text = commentVM.Text
            };

            book.BookComments.Add(comment);

            book.Rate = (byte)Math.Round(book.BookComments.Average(x => x.Rate));

            await _context.SaveChangesAsync();

            return RedirectToAction("detail", new { id = commentVM.BookId });
        }

        public IActionResult SetSession()
        {
            HttpContext.Session.SetString("name", "Hikmet Abbasov");
            return Content("");
        }

        public IActionResult GetSession()
        {
            var value = HttpContext.Session.GetString("name");
            return Content(value);
        }

        public IActionResult SetCookie()
        {
            HttpContext.Response.Cookies.Append("name", "Hikmet Abbasov");
            return Content("");
        }

        public IActionResult GetCookie()
        {
            var value = HttpContext.Request.Cookies["name"];

            return Content(value);
        }

        public IActionResult AddToBasket(int id)
        {
            AppUser member = null;
            if (User.Identity.IsAuthenticated)
            {
                member = _context.Users.FirstOrDefault(x => !x.IsAdmin && x.UserName == User.Identity.Name);
            }
            List<CookieBasketItemViewModel> cookieBasketItems = null;

            BasketViewModel basketVM = null;
            if (member == null)
            {
                CookieBasketItemViewModel basketItem = null;

                string basketJson = Request.Cookies["Basket"];

                if (basketJson == null)
                {
                    cookieBasketItems = new List<CookieBasketItemViewModel>();
                }
                else
                {
                    cookieBasketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketJson);
                    basketItem = cookieBasketItems.FirstOrDefault(x => x.BookId == id);
                }

                if (basketItem == null)
                {
                    basketItem = new CookieBasketItemViewModel
                    {
                        BookId = id,
                        Count = 1
                    };
                    cookieBasketItems.Add(basketItem);
                }
                else
                    basketItem.Count++;


                string newBasketJson = JsonConvert.SerializeObject(cookieBasketItems);
                Response.Cookies.Append("Basket", newBasketJson);

                basketVM = GetBasketVM(cookieBasketItems);
            }
            else
            {
                List<BasketItem> basketItems = _context.BasketItems.Where(x => x.AppUserId == member.Id).ToList();

                BasketItem basketItem = basketItems.FirstOrDefault(x => x.BookId == id);

                if (basketItem == null)
                {
                    basketItem = new BasketItem
                    {
                        BookId = id,
                        AppUserId = member.Id,
                        Count = 1
                    };
                    basketItems.Add(basketItem);
                    _context.BasketItems.Add(basketItem);
                }
                else
                    basketItem.Count++;
                _context.SaveChanges();

                basketVM = GetBasketVM(basketItems);
            }

            //return RedirectToAction("index", "home");

          
            return PartialView("_BasketPartial", basketVM);
        }

        private BasketViewModel GetBasketVM(List<CookieBasketItemViewModel> cookieBasketItems)
        {
            BasketViewModel basket = new BasketViewModel();


            foreach (var item in cookieBasketItems)
            {
                Book book = _context.Books.Include(x => x.BookImages).FirstOrDefault(x => x.Id == item.BookId);

                BasketItemViewModel basketItem = new BasketItemViewModel
                {
                    BookId = book.Id,
                    Count = item.Count,
                    Name = book.Name,
                    Poster = book.BookImages.FirstOrDefault(x => x.PosterStatus == true)?.Name,
                    Price = book.DiscountPercent > 0 ? (book.SalePrice * (100 - book.DiscountPercent) / 100) : book.SalePrice
                };

                basket.TotalAmount += basketItem.Price * basketItem.Count;
                basket.BasketItems.Add(basketItem);
            }

            return basket;
        }
        private BasketViewModel GetBasketVM(List<BasketItem> basketItems)
        {
            BasketViewModel basket = new BasketViewModel();

            foreach (var item in basketItems)
            {
                Book book = _context.Books.Include(x => x.BookImages).FirstOrDefault(x => x.Id == item.BookId);

                BasketItemViewModel basketItem = new BasketItemViewModel
                {
                    BookId = book.Id,
                    Count = item.Count,
                    Name = book.Name,
                    Poster = book.BookImages.FirstOrDefault(x => x.PosterStatus == true)?.Name,
                    Price = book.DiscountPercent > 0 ? (book.SalePrice * (100 - book.DiscountPercent) / 100) : book.SalePrice
                };

                basket.TotalAmount += basketItem.Price * basketItem.Count;
                basket.BasketItems.Add(basketItem);
            }

            return basket;
        }



        public IActionResult ShowBasket()
        {
            List<CookieBasketItemViewModel> ids = null;


            string basketJson = Request.Cookies["Basket"];

            if (basketJson == null)
            {
                ids = new List<CookieBasketItemViewModel>();
            }
            else
            {
                ids = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketJson);
            }

            return Ok(ids);
        }
    }
}
