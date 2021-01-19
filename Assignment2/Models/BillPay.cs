using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class BillPay
    {
        [Key, Required]
        public int BillPayId { get; set; }

        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public int PayeeId { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ScheduleDate { get; set; }

        [Required] // TODO Must be M, Q or S
        public char Period { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ModifyDate { get; set; }

    }
}
