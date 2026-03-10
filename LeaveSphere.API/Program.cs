using LeaveSphere.API.Data;
using LeaveSphere.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5002")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddScoped<LeaveSphere.API.Repositories.ILeaveRepository, LeaveSphere.API.Repositories.LeaveRepository>();
builder.Services.AddScoped<LeaveSphere.API.Services.ILeaveService, LeaveSphere.API.Services.LeaveService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "VERY_SECRET_KEY_PLACEHOLDER_12345";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeaveSphereAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LeaveSphereClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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

builder.Services.AddAuthorization();

// 🔥 IMPORTANT: Use Configuration directly
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString ?? "server=localhost;database=leavesphere_db;user=root;password=4356;",
        new MySqlServerVersion(new Version(8, 0, 34))
    ));



var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", (HttpContext context) => context.Response.Redirect("/swagger"));

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData.Initialize(context);
}

app.Run();