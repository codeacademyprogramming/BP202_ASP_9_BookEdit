using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.ViewModels
{
    public class BookCommentPostViewModel
    {
        public int BookId { get; set; }
        [Required]
        [Range(1,5)]
        public int Rate { get; set; }
        [Required]
        [MaxLength(250)]
        public string Text { get; set; }
    }
}
