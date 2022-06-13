using Microsoft.EntityFrameworkCore;
using Pustok.Models;

namespace Pustok.DAL
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookImage> BookImages { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
    }

}
