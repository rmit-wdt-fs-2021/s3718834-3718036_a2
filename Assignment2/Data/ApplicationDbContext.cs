using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Assignment2.Models;

namespace Assignment2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Customer>()
                .HasMany(customer => customer.Accounts)
                .WithOne(account => account.Customer)
                .HasForeignKey(account => account.CustomerId);

            builder.Entity<ApplicationUser>()
                .HasOne(login => login.Customer)
                .WithOne(customer => customer.Login)
                .HasForeignKey<ApplicationUser>(applicationUser => applicationUser.CustomerId);

            builder.Entity<Account>()
                .HasMany(account => account.BillPays)
                .WithOne(billPay => billPay.Account)
                .HasForeignKey(billPay => billPay.AccountNumber);
            
            builder.Entity<Account>()
                .HasMany(account => account.Transactions)
                .WithOne(transaction => transaction.Account)
                .HasForeignKey(transaction => transaction.AccountNumber);

            builder.Entity<Payee>()
                .HasMany(payee => payee.BillPays)
                .WithOne(billPay => billPay.Payee)
                .HasForeignKey(billPay => billPay.PayeeId);


        }

        public DbSet<Assignment2.Models.Account> Account { get; set; }
        public DbSet<Assignment2.Models.Customer> Customer { get; set; }
        public DbSet<Assignment2.Models.BillPay> BillPay { get; set; }
        public DbSet<Assignment2.Models.Payee> Payee { get; set; }
        public DbSet<Assignment2.Models.Transaction> Transaction { get; set; }
        public DbSet<Assignment2.Models.ActivityReportHistory> ActivityReportHistory { get; set; }
    }
}