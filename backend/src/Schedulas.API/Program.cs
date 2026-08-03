using Asp.Versioning;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Schedulas.API.Middleware;
using Schedulas.Application;
using Schedulas.Infrastructure;
using Schedulas.Infrastructure.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ==================== 1. Serilog (Constitution §15) ====================
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Schedulas.API")
    .WriteTo.Console());

// ==================== 2. Layer registration (Architecture §3) ====================
// Application: MediatR, FluentValidation, AutoMapper, pipeline behaviors, RuleEngineOrchestrator.
// Infrastructure: EF Core + SchedulasDbContext, Supabase Auth/Storage integrations, FCM, current-user/date-time services.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ==================== 3. Authentication: Supabase Auth is the ONLY provider ====================
// No ASP.NET Identity anywhere in this solution -- Supabase's auth.users is
// the single source of truth for credentials; this API never stores or
// validates a password. It only ever validates a JWT that Supabase already
// issued, using Supabase's own public signing keys (JWKS), never a shared
// symmetric secret.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false, // Supabase's issuer URL varies per project; the signing key itself is the trust anchor
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
    });

// Resolves SupabaseJwksProvider (a singleton, registered in AddInfrastructure)
// through DI and wires its cached public keys into JwtBearerOptions --
// the officially supported way to inject a service into an auth handler's
// options, since AddJwtBearer's own configuration delegate has no DI access.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<SupabaseJwksProvider>((options, jwks) =>
    {
        options.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, kid, _) =>
            jwks.GetSigningKeys().Where(key => kid == null || key.KeyId == kid);
    });

// Role-based authorization built on top of the JWT's promoted role claim
// (see SupabaseRoleClaimsTransformation) -- application-level roles, not
// ASP.NET Identity roles.
builder.Services.AddAuthorization();

// ==================== 4. Rate limiting (Constitution §16) ====================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ==================== 5. CORS: Flutter app + React dashboard origins only ====================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("SchedulasClients", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ==================== 6. API Versioning ====================
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); // matches the /api/v1/ routes already in every controller
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ==================== 7. Controllers + Swagger (Constitution §11) ====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Schedulas API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "أدخل رمز JWT الخاص بك (الصادر من Supabase Auth)"
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] }
    });

    // Surfaces the XML doc comments already written on controllers and
    // DTOs (e.g. GetMyTaughtClassesQuery/MyClassDto) in the generated
    // OpenAPI document — without this, GenerateDocumentationFile produces
    // the .xml files but Swagger never reads them, and "documented in
    // code" silently stays "not documented in the API surface."
    var apiXmlFile = System.IO.Path.Combine(AppContext.BaseDirectory, $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (System.IO.File.Exists(apiXmlFile)) options.IncludeXmlComments(apiXmlFile, includeControllerXmlComments: true);

    var applicationXmlFile = System.IO.Path.Combine(AppContext.BaseDirectory, "Schedulas.Application.xml");
    if (System.IO.File.Exists(applicationXmlFile)) options.IncludeXmlComments(applicationXmlFile);
});

// ==================== 8. Health Checks ====================
// Validates real database connectivity (not just "process is up") --
// required by Render's monitoring (Architecture §8) and by this session's
// explicit "verify database connectivity" requirement.
var dbConnectionString = builder.Configuration.GetConnectionString("SchedulasDb")
    ?? Environment.GetEnvironmentVariable("SCHEDULAS_DB_CONNECTION")
    ?? throw new InvalidOperationException(
        "No database connection string configured. Set ConnectionStrings:SchedulasDb " +
        "or the SCHEDULAS_DB_CONNECTION environment variable."); // same requirement AddInfrastructure enforces for the DbContext itself

builder.Services.AddHealthChecks()
    .AddNpgSql(
        dbConnectionString,
        name: "postgresql",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: ["db", "ready"]);

// ==================== 9. Background Job infrastructure (Quartz) ====================
// Wired per this session's explicit instruction: infrastructure only, no
// business jobs registered yet. The scheduler starts, is hosted, and is
// ready for the first real job (e.g. the deadline-reminder scan from
// Sequence Diagram 3) to be added as a JobDetail + Trigger in a future
// pass -- adding one will not require touching this registration block.
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

// ==================== Middleware pipeline ====================
app.UseGlobalExceptionHandling(); // outermost: catches everything below

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // structured per-request log line (method, path, status, elapsed) -- Constitution §15

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseCors("SchedulasClients");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // liveness: process is up and answering HTTP, no dependency checks
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") // readiness: can this instance actually serve traffic (DB reachable)
}).AllowAnonymous();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions())
    .AllowAnonymous(); // combined view, kept for Render's single-URL monitoring config

app.Run();

public partial class Program { }
