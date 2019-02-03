using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Primagaz.Standard.Entities;

namespace Primagaz.Standard
{
    public class Repository : DbContext
    {

        public DbSet<Call> Calls { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DeliveryDocket> DeliveryDockets { get; set; }
        public DbSet<DeliveryDocketItem> DeliveryDocketItems { get; set; }
        public DbSet<DriverStock> DriverStock { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<LendingStatus> LendingStatus { get; set; }
        public DbSet<MobileDevice> MobileDevices { get; set; }
        public DbSet<NonDelivery> NonDeliveries { get; set; }
        public DbSet<NonDeliveryReason> NonDeliveryReasons { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Trailer> Trailers { get; set; }

        const string databaseName = "database.db";

        public Repository()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = GetDatabasePath();
            optionsBuilder.UseSqlite($"Filename={path}");
        }

        public static string GetDatabasePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), databaseName);
        }
    }

 }
