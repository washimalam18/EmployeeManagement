using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string ItemTotal { get; set; }

        public List<Product> products { get; set; } = new List<Product>();

        public string ProductName { get; set; }

        public string ProductRate { get; set; }


    }
}
