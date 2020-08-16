﻿using System.Data.Entity;
using AirVinyl.Model;

namespace AirVinyl.DataAccessLayer
{
    public class AirVinylDbContext : DbContext
    {
        public AirVinylDbContext()
        {
            Database.SetInitializer(new AirVinylDbInitializer());
            // disable lazy loading
            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Person> People { get; set; }
        public DbSet<VinylRecord> VinylRecords { get; set; }
        public DbSet<RecordStore> RecordStores { get; set; }
        public DbSet<PressingDetail> PressingDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ensure the same person can be added to different collections
            // of friends (self-referencing many-to-many relationship)
            modelBuilder.Entity<Person>().HasMany(m => m.Friends).WithMany();

            modelBuilder.Entity<Person>().HasMany(p => p.VinylRecords)
                .WithRequired(r => r.Person).WillCascadeOnDelete(true);
        }
    }
}