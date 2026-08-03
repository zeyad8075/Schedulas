using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Schedulas.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add <Name>` run against this project directly,
/// without needing the full API host to boot. Reads the connection string
/// from the SCHEDULAS_DB_CONNECTION environment variable so no secret is
/// ever committed to source control (Constitution §16).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SchedulasDbContext>
{
    public SchedulasDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SCHEDULAS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=schedulas_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<SchedulasDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SchedulasDbContext(optionsBuilder.Options, new DesignTimeTenantService());
    }

    private class DesignTimeTenantService : Schedulas.Application.Common.Interfaces.ITenantService
    {
        public Guid? TenantId => null;
        public bool IsTenantEnforced => false;
        public void SetTenantId(Guid tenantId) { }
        public IDisposable BeginBypassScope() => new DummyScope();
        private class DummyScope : IDisposable { public void Dispose() { } }
    }
}
