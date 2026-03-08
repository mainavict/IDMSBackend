using Microsoft.EntityFrameworkCore;
using IDMSBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IDMSBackend.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // --- Tables ---
    public DbSet<User> Users { get; set; }
    public DbSet<Students> Students { get; set; }
    public DbSet<StudentCards> StudentCards { get; set; }
    public DbSet<Events> Events { get; set; }
    public DbSet<EventsRecurrenceRules> EventsRecurrenceRules { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Domains> Domains { get; set; }
    public DbSet<UserDomainRole> UserDomainRoles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // --- Automatic Auditing Logic ---
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Capture the changes before they are committed to the DB
        OnBeforeSaveChanges();
        
        // 2. Actually save the data
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaveChanges()
    {
        var httpContext = _httpContextAccessor.HttpContext;
    
        var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
        var traceId = httpContext?.TraceIdentifier;
        var userId = httpContext?.User?.Identity?.Name ?? "System/Sync";

        // 1. Create a temporary list to hold our new logs
        var temporaryAuditList = new List<AuditLog>();

        // 2. Loop through the changes
        foreach (var entry in ChangeTracker.Entries().ToList()) // .ToList() creates a snapshot to prevent errors
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = entry.State.ToString(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Details = $"Table: {entry.Metadata.GetTableName()} | Action by {userId}"
            };

            // 3. Add to the TEMPORARY list, NOT the database yet
            temporaryAuditList.Add(auditLog);
        }

        // 4. Now that the loop is finished, add everything to the AuditLogs table
        if (temporaryAuditList.Any())
        {
            AuditLogs.AddRange(temporaryAuditList);
        }
    }
    // --- Model Configuration ---
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User Configuration ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.SchoolId).IsUnique()
            .HasFilter("\"SchoolId\" IS NOT NULL AND \"SchoolId\" <> ''");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.FacultyId).IsUnique()
            .HasFilter("\"FacultyId\" IS NOT NULL AND \"FacultyId\" <> ''");

        modelBuilder.Entity<User>().Property(u => u.Status).HasConversion<string>();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.userType).HasConversion<string>();
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        // --- UserDomainRole (Many-to-Many Join) ---
        modelBuilder.Entity<UserDomainRole>()
            .HasOne(udr => udr.User).WithMany(u => u.UserDomainRoles)
            .HasForeignKey(udr => udr.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserDomainRole>()
            .HasOne(udr => udr.Role).WithMany(r => r.UserDomainRoles)
            .HasForeignKey(udr => udr.RoleId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserDomainRole>()
            .HasOne(udr => udr.Domain).WithMany(d => d.UserDomainRoles)
            .HasForeignKey(udr => udr.DomainId).OnDelete(DeleteBehavior.Cascade);

        // --- Events Configuration ---
        modelBuilder.Entity<Events>()
            .HasOne(e => e.Domain).WithMany(d => d.Events)
            .HasForeignKey(e => e.DomainId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Events>()
            .HasOne(e => e.Creator).WithMany(u => u.CreatedEvents)
            .HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Events>()
            .HasOne(e => e.Updater).WithMany()
            .HasForeignKey(e => e.UpdatedBy).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Events>()
            .HasOne(e => e.EventsRecurrenceRule).WithOne(r => r.Event)      
            .HasForeignKey<EventsRecurrenceRules>(r => r.EventId).OnDelete(DeleteBehavior.Cascade);

        // --- Students & Cards ---
        modelBuilder.Entity<StudentCards>()
            .HasOne(s => s.Student).WithOne(s => s.StudentCards)
            .HasForeignKey<StudentCards>(s => s.StudentId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentCards>().HasIndex(s => s.CardUuid).IsUnique();
        modelBuilder.Entity<Students>().HasIndex(s => s.SchoolId).IsUnique();

        // --- Roles ---
        modelBuilder.Entity<Roles>().HasIndex(r => r.Name).IsUnique();
    }
}