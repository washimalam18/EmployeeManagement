using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Employee.Models
{
    public class Emp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Department { get; set; }

        [Required]
        [Display(Name = "Joining Date")]
        public DateOnly JoiningDate { get; set; }

      
        public String Url { get; set; }




    }
}
