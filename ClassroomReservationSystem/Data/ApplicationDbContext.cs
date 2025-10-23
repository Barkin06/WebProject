//I asked GPT:
//I shared the codes in models folder, help me out to create db context file then creating database with migration on SSMS.
//Also, define an ApplicationUser class inheriting from IdentityUser which has name attributes and sys logs.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Term> Terms { get; set; }
        
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<SystemLog> SystemLogs { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Term)  
                .WithMany()           
                .HasForeignKey(r => r.TermId) 
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
