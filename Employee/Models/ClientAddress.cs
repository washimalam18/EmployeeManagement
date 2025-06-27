using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class ClientAddresses
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public string Street { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PinCode { get; set; }

        public string Country { get; set; }






        public string Address { get; set; }

        public bool IsDeleted { get; set; }








    }
}
