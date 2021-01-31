using System.ComponentModel.DataAnnotations;

namespace Assignment2.Models
{
    public abstract class Person
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(40)]
        public string City { get; set; }

        [StringLength(4)]
        [RegularExpression("[0-9]{4}")]
        public string PostCode { get; set; }
        
        public State State { get; set; }

        [Phone, Required]
        [RegularExpression("^(\\+61) [0-9]{4} [0-9]{4}")]
        public string Phone { get; set; }
    }
}