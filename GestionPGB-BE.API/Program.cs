using System.Text;
using GestionPGB_BE.API.API.Endpoints;
using GestionPGB_BE.API.API.Hubs;
using GestionPGB_BE.API.Application.Services;
using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Interfaces;
using GestionPGB_BE.API.Infrastructure.Data;
using GestionPGB_BE.API.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog: Console (all levels) + PostgreSQL (Warning+)
builder.Host.UseSerilog((context, _, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "system_logs",
        needAutoCreateTable: true,
        restrictedToMinimumLevel: LogEventLevel.Warning));

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET is not configured. Use user-secrets in development.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    // Permite que SignalR envíe el token por query string al conectar el WebSocket
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS - solo permite el origen del frontend
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// SignalR
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Serializar enums como strings en toda la API
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// OpenAPI (Swashbuckle) + Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "GestionPGB API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Repositorios
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

// Servicios
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();

// Seed de roles al iniciar
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Operator" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    const string adminUserName = "admin";
    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminUserName,
            Email = "admin@gestionpgb.com",
            FullName = "Administrador",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin1234!");
    }
    // Garantiza el rol aunque el usuario ya existiera sin él
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

if (app.Environment.IsDevelopment())
{
    // Expone el JSON de OpenAPI que Scalar consume
    app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
    // Scalar en /scalar/v1 (según spec)
    app.MapScalarApiReference(options =>
        options.WithTitle("GestionPGB API").WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
}

app.UseMiddleware<GestionPGB_BE.API.API.Middleware.ExceptionHandlingMiddleware>();
app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Minimal API Endpoints
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapStockMovementEndpoints();

// SignalR Hub
app.MapHub<StockHub>("/hubs/stock");

app.Run();
