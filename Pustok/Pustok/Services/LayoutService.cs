using Pustok.DAL;
using Pustok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }

        public List<Genre> GetGenres()
        {
            return _context.Genres.ToList();
        }

        public Dictionary<string,string> GetSettings()
        {
            return _context.Settings.ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
