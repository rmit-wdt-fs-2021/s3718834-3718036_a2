using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models.Data
{
    public class ActivityReportHistory
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime OccuredWhen { get; set; }
        public int EmailsSent { get; set; }
    }
}
