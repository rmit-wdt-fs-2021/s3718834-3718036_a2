using System.ComponentModel.DataAnnotations;

namespace Assignment2.Models
{
    public enum State
    {
        Vic,
        Nsw,
        Sa,
        Qld,
        Tas
    }

    public abstract class Person
    {
        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(40)]
        public string City { get; set; }

        [StringLength(10)]
        [RegularExpression("[0-9]{4}")]
        public string PostCode { get; set; }
        
        [StringLength(20)]
        public State State { get; set; }

        [Phone, Required, StringLength(15)]
        [RegularExpression("^(\\+61) [0-9]{4} [0-9]{4}")]
        public string Phone { get; set; }
    }
}