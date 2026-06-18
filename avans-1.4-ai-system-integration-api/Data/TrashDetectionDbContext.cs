using avans_1._4_ai_system_integration_api.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace avans_1_4_ai_system_integration_api.Data;

public class TrashDetectionDbContext(DbContextOptions<TrashDetectionDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<TrashDetection> TrashDetections => Set<TrashDetection>();

}