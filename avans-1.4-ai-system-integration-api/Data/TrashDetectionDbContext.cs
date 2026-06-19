using avans_1._4_ai_system_integration_api.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace avans_1_4_ai_system_integration_api.Data;

public class TrashDetectionDbContext(DbContextOptions<TrashDetectionDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<TrashDetection> TrashDetections => Set<TrashDetection>();
    public DbSet<TrashDataFetchLog> TrashDataFetchLogs => Set<TrashDataFetchLog>();

    //zorgt ervoor dat er geen dubbele entries in de database komen voor dezelfde locatie en tijdstip
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TrashDetection>()
            .HasIndex(t => new { t.CameraLatitude, t.CameraLongitude, t.PhotoTakenAtUtc })
            .IsUnique();
    }
}