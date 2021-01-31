using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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


    public class Customer : Person
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; }
        
        [StringLength(11)]
        public string Tfn { get; set; }

        public string LoginId { get; set; }

        public List<Account> Accounts { get; set; }

        public ApplicationUser Login { get; set; }
    }
}
