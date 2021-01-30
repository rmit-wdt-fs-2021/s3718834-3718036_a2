using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Assignment2.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

            builder.Entity<Customer>(entity =>
            {
                entity.HasMany(customer => customer.Accounts)
                    .WithOne(account => account.Customer)
                    .HasForeignKey(account => account.CustomerId);
                entity.Property(customer => customer.State)
                    .HasConversion(state => state.ToString(),
                        dbValue => Enum.Parse<State>(dbValue));
            });
            
            builder.Entity<Account>(entity =>
            {
                entity.HasMany(account => account.BillPays)
                    .WithOne(billPay => billPay.Account)
                    .HasForeignKey(billPay => billPay.AccountNumber);

                entity.HasMany(account => account.Transactions)
                    .WithOne(transaction => transaction.Account)
                    .HasForeignKey(transaction => transaction.AccountNumber);

                entity.Property(account => account.AccountType)
                    .HasConversion(accountType => (char) accountType,
                        dbValue => (AccountType) dbValue);
            });

            builder.Entity<Payee>(entity =>
            {
                entity.HasMany(payee => payee.BillPays)
                    .WithOne(billPay => billPay.Payee)
                    .HasForeignKey(billPay => billPay.PayeeId);

                entity.Property(customer => customer.State)
                    .HasConversion(state => state.ToString(),
                        dbValue => Enum.Parse<State>(dbValue));
            });

            builder.Entity<ApplicationUser>()
                .HasOne(login => login.Customer)
                .WithOne(customer => customer.Login)
                .HasForeignKey<ApplicationUser>(applicationUser => applicationUser.CustomerId);
            

            builder.Entity<BillPay>()
                .Property(billPay => billPay.Period)
                .HasConversion(period => period.ToString(),
                    dbValue => Enum.Parse<Period>(dbValue));


            builder.Entity<Transaction>()
                .Property(transaction => transaction.TransactionType)
                .HasConversion(transactionType => (char) transactionType,
                    dbValue => (TransactionType) dbValue);
        }

        public DbSet<Assignment2.Models.Account> Account { get; set; }
        public DbSet<Assignment2.Models.Customer> Customer { get; set; }
        public DbSet<Assignment2.Models.BillPay> BillPay { get; set; }
        public DbSet<Assignment2.Models.Payee> Payee { get; set; }
        public DbSet<Assignment2.Models.Transaction> Transaction { get; set; }
        public DbSet<Assignment2.Models.ActivityReportHistory> ActivityReportHistory { get; set; }
    }
}