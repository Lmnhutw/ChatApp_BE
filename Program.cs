using AutoMapper;
using ChatApp_BE.Data;
using ChatApp_BE.Helpers;
using ChatApp_BE.Hubs;
using ChatApp_BE.Mappings;
using ChatApp_BE.Models;
using ChatApp_BE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MessageService and RoomService
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<UserService>();

// Register EmailSenders
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailSenders>();

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
//Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChatApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});


// Register AutoMapper
builder.Services.AddAutoMapper(typeof(UserProfile));

// Register Email Sender
builder.Services.AddScoped<IEmailSenders>();
//builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ChatAppContext>();

// Configure JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "default_secret_key");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

// Configure SignalR
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

//Add CORS
app.UseCors("ChatApp");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hub");

app.Run();