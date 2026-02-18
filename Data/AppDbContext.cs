using Microsoft.EntityFrameworkCore;
using IDMSBackend.Models;

namespace IDMSBackend.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Students> Students { get; set; }
    public DbSet<StudentCards> StudentCards { get; set; }
    
    public DbSet<Events> Events { get; set; }
    
    public DbSet<EventsRecurrenceRules> EventsRecurrenceRules { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //SchoolId is unique (smaivi2309 can't be used twice)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.SchoolId)
            .IsUnique();
        

        //Ensure Email is unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Store the Role Enum as a String (OPERATOR/Admin) in the DB
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // 4. Postgres optimization: Map the DateTime fields to 'timestamp with time zone'
        // This is the best practice for Npgsql in 2026
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        
        
        modelBuilder.Entity<StudentCards>()
            .HasOne(s => s.Student)
            .WithOne(s=> s.StudentCards)
            .HasForeignKey<StudentCards>(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade
            );
        
        modelBuilder.Entity<StudentCards>().HasIndex(s => s.CardUuid).IsUnique();
        modelBuilder.Entity<Students>().HasIndex(s => s.SchoolId).IsUnique();
        
        modelBuilder.Entity<Events>()
            .HasOne(e => e.Creator)
            .WithMany(u => u.CreatedEvents)
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Events>()
            .HasOne(e => e.Updater)
            .WithMany()
            .HasForeignKey(e => e.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);modelBuilder.Entity<Events>()
            .HasOne(e => e.Updater)
            .WithMany()
            .HasForeignKey(e => e.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        modelBuilder.Entity<Events>()
            .HasOne(e => e.EventsRecurrenceRules)
            .WithOne(r => r.Event)      
            .HasForeignKey<EventsRecurrenceRules>(r=> r.EventId)
            .OnDelete(DeleteBehavior.Cascade);  
    }

    
}