using System.Collections.Generic;
using ClinicSystem.Authentication;
using ClinicSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.DB
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> context) : base(context)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Appointment> Appointment { get; set; }
    public DbSet<DocInfo> DocInfo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<DocInfo>()
            .HasOne<Appointment>(s => s.Appointment)
            .WithMany(g => g.DocInfos)
            .HasForeignKey(s => s.AppointmentId);
      modelBuilder.Entity<DocInfo>()
            .HasOne<User>(s => s.User)
            .WithMany(g => g.DocInfos)
            .HasForeignKey(s => s.UserId);

      // modelBuilder.Entity<DocInfo>()
      //        .HasOne(p => p.DoctorId)
      //        .WithMany()
      //        .IsRequired();
      // modelBuilder.Entity<User>()
      //        .HasMany(p => p.DoctorId)
      //        .WithMany();
      // modelBuilder.Entity<User>()
      //       .HasKey(t => new { t.DoctorId } );
      // modelBuilder.Entity<Appointment>()
      //       .HasOne(p => p.DoctorID);
      // .WithMany(t => t.DocInfos);
      // modelBuilder.Entity<DocInfo>()
      //         .HasNoKey();
      // modelBuilder.Ignore<IdentityUserLogin<string>>();

    }
  }
}
