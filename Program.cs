using AutoMapper;
using ChatApp_BE.Data;
using ChatApp_BE.Helpers;
using ChatApp_BE.Hubs;
using ChatApp_BE.Mappings;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Entity Framework Core
builder.Services.AddDbContext<ChatAppContext>(options =>
{
    var ConnectionStrings = builder.Configuration.GetConnectionString("ChatApp");
    if (ConnectionStrings != null)
    {
        options.UseSqlServer(ConnectionStrings);
    }
    else
    {
        Console.WriteLine("Something went wrong when connecting to DB");
    }
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile));

builder.Services.AddScoped<IEmailSenders>();

// Configure Identity
//builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
//{
//    options.SignIn.RequireConfirmedAccount = true; // Require email confirmation
//})
//    .AddRoles<IdentityRole>()
//    .AddEntityFrameworkStores<ChatAppContext>()
//    .AddDefaultTokenProviders();
builder.Services.AddIdentity<User, IdentityRole>()
                //// Thêm triển khai EF lưu trữ thông tin về Idetity (theo AppMvcContext -> MS SQL Server).
                .AddDefaultTokenProviders()
                // Thêm Token Provider - nó sử dụng để phát sinh token (reset password, confirm email ...)
                // đổi email, số điện thoại ...
                .AddEntityFrameworkStores<ChatAppContext>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["SecretKey"]);
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();