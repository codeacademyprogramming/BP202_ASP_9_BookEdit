using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.ViewModels
{
    public class BasketItemViewModel
    {
        public int BookId { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string Poster { get; set; }
    }
}
