using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public enum Period
    {
        Monthly, 
        Quarterly,
        OnceOff
    }

    public class BillPay
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillPayId { get; set; }

        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public int PayeeId { get; set; }

        [Required, Column(TypeName = "decimal(18, 2)")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ScheduleDate { get; set; }

        [Required] 
        public Period Period { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ModifyDate { get; set; }

        [ForeignKey("AccountForeignKey")]
        public Account Account { get; set; }

        [ForeignKey("PayeeForeignKey")]
        public Payee Payee { get; set; }

    }
}
