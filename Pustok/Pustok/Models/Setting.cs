using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Models
{
    public class Setting
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(25)]
        public string Key { get; set; }
        [MaxLength(250)]
        public string Value { get; set; }
    }
}
