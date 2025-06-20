using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }


        [Required]
        public int AddressId { get; set; }

        [Required]
        public string OrderTotal { get; set; }

        public string Address { get; set; }

        public Clients client { get; set; }

        public List<ClientAddresses> Addersses { get; set; } = new List<ClientAddresses>();

        public List<Item> items { get; set; } = new List<Item>();
        public List<Product> Products { get; set; }  = new List<Product>();

        

        

        

       

    }
}
