using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pustok.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50,ErrorMessage = "Uzunluq max 50 ola biler ")]
        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public List<Book> Books { get; set; }
    }
}
