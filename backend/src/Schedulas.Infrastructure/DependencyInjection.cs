using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Infrastructure.Common;
using Schedulas.Infrastructure.Identity;
using Schedulas.Infrastructure.Notifications;
using Schedulas.Infrastructure.Persistence;
using Schedulas.Infrastructure.Persistence.Interceptors;

namespace Schedulas.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the full Infrastructure surface: persistence (EF Core +
    /// Supabase Postgres), identity (Supabase Auth, JWKS-based JWT key
    /// resolution), push notifications (FCM), file storage (Supabase
    /// Storage), and the supporting current-user/date-time services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<AuditableSaveChangesInterceptor>();

        services.AddDbContext<SchedulasDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("SchedulasDb")
                ?? Environment.GetEnvironmentVariable("SCHEDULAS_DB_CONNECTION")
                ?? throw new InvalidOperationException(
                    "No database connection string configured. Set ConnectionStrings:SchedulasDb " +
                    "or the SCHEDULAS_DB_CONNECTION environment variable.");

            if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Ssl Mode=Require;Trust Server Certificate=true;";
            }

            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditableSaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SchedulasDbContext>());
        services.AddScoped<Schedulas.Application.Common.Behaviors.IApplicationDbContextEventSource>(
            sp => sp.GetRequiredService<SchedulasDbContext>());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddTransient<Microsoft.AspNetCore.Authentication.IClaimsTransformation, SupabaseRoleClaimsTransformation>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.Configure<SupabaseOptions>(configuration.GetSection(SupabaseOptions.SectionName));
        services.AddHttpClient<IIdentityService, SupabaseIdentityService>();
        services.AddHttpClient(nameof(SupabaseJwksProvider));
        services.AddSingleton<SupabaseJwksProvider>();

        services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));
        services.AddHttpClient(nameof(FcmTokenProvider));
        services.AddSingleton<FcmTokenProvider>();
        services.AddHttpClient<IPushNotificationService, FcmPushNotificationService>();

        services.AddHttpClient<IFileStorageService, Storage.SupabaseStorageService>();

        return services;
    }
}
