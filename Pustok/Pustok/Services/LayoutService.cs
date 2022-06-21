using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public LayoutService(AppDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public List<Genre> GetGenres()
        {
            return _context.Genres.ToList();
        }

        public Dictionary<string,string> GetSettings()
        {
            return _context.Settings.ToDictionary(x => x.Key, y => y.Value);
        }

        public BasketViewModel GetBasket()
        {
            AppUser member = null;
            BasketViewModel basket = new BasketViewModel();

            if (_contextAccessor.HttpContext.User.Identity.IsAuthenticated)
                member = _context.Users.FirstOrDefault(x => !x.IsAdmin && x.UserName == _contextAccessor.HttpContext.User.Identity.Name);


            if (member == null)
            {
                var basketJson = _contextAccessor.HttpContext.Request.Cookies["Basket"];
                if (basketJson != null)
                {
                    List<CookieBasketItemViewModel> cookieBasketItems = JsonConvert.DeserializeObject<List<CookieBasketItemViewModel>>(basketJson);

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
                }
            }
            else
            {
                List<BasketItem> basketItems = _context.BasketItems.Where(x => x.AppUserId == member.Id).ToList();
               
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
            }

            return basket;
        }
    }
}
