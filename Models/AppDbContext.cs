using Microsoft.EntityFrameworkCore;
using StayShare.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace StayShare.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomOccupancy> RoomOccupancies { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ParentLink> ParentLinks { get; set; }
        public DbSet<BookingRequest> BookingRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId);

            modelBuilder.Entity<Room>()
                .HasMany(r => r.Occupants)
                .WithOne(o => o.Room)
                .HasForeignKey(o => o.RoomId);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Rooms)
                .WithOne(r => r.Property)
                .HasForeignKey(r => r.PropertyId);

            modelBuilder.Entity<RoomOccupancy>()
                .HasOne(ro => ro.User)
                .WithMany(u => u.RoomOccupancies)
                .HasForeignKey(ro => ro.UserId);

            modelBuilder.Entity<ParentLink>()
                .HasOne(pl => pl.Parent)
                .WithMany(u => u.ParentLinks)
                .HasForeignKey(pl => pl.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParentLink>()
                .HasOne(pl => pl.Child)
                .WithMany()
                .HasForeignKey(pl => pl.ChildId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingRequest>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId);

            modelBuilder.Entity<BookingRequest>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId);
        }
    }
}
