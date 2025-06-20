using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Rate { get; set; }



    }
}
