using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GestaoDespesas.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register TokenService so it can be injected into controllers via dependency injection
builder.Services.AddScoped<GestaoDespesas.Api.Services.TokenService>();

// Add services to the container.

builder.Services.AddControllers();

// Register Swagger generator, which builds the OpenAPI spec and powers the Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adds a padlock/authorize button to Swagger UI, so we can test
    // authenticated endpoints by pasting in a JWT token
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register the ApplicationDbContext, telling EF Core to use SQL Server
// with the connection string defined in appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ASP.NET Core Identity, which provides user registration,
// login, password hashing, and role management out of the box
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Read JWT settings from appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

// Configure JWT Bearer authentication: this tells the API how to
// validate incoming tokens (signature, issuer, audience, expiration)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Prevent ASP.NET Core from remapping short claim names (e.g. "sub", "email")
    // to long legacy XML-based URIs, so we can read claims using their original names
    options.MapInboundClaims = false; 

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable middleware to serve the generated Swagger JSON document
    app.UseSwagger();

    // Enable middleware to serve the Swagger UI (HTML, JS, CSS), specifying the endpoint
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GestaoDespesas API v1");
    });
}


app.UseHttpsRedirection();

// Authentication must run before Authorization
// (Authentication: "who are you?" / Authorization: "what are you allowed to do?")
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();