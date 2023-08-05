using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.AdministrationService.EntityFrameworkCore;

public class AdministrationServiceHttpApiHostMigrationsDbContext : AbpDbContext<AdministrationServiceHttpApiHostMigrationsDbContext>
{
    public AdministrationServiceHttpApiHostMigrationsDbContext(DbContextOptions<AdministrationServiceHttpApiHostMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureAdministrationService();
    }
}
