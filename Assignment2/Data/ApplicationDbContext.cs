using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Assignment2.Models;

namespace Assignment2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Assignment2.Models.Account> Account { get; set; }
        public DbSet<Assignment2.Models.Customer> Customer { get; set; }
        public DbSet<Assignment2.Models.BillPay> BillPay { get; set; }
        public DbSet<Assignment2.Models.Payee> Payee { get; set; }
        public DbSet<Assignment2.Models.Transaction> Transaction { get; set; }
    }
}
