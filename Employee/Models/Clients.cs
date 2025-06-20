using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class Clients
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
       
        public string Email { get; set; }


        public string Role { get; set; }

        public List<ClientAddresses> Addresses { get; set; } = new List<ClientAddresses>();

    }
}
