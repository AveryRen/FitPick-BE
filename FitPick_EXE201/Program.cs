using FitPick_EXE201.Data;
using FitPick_EXE201.Helpers;
using FitPick_EXE201.Models;
using FitPick_EXE201.Repositories.Interface;
using FitPick_EXE201.Repositories.Repo;
using FitPick_EXE201.Services;
using FitPick_EXE201.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using Microsoft.Extensions.Options;
using FitPick_EXE201.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// ✅ Add DbContext (fix lỗi DI)
builder.Services.AddDbContext<FitPickContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token:",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();

// ✅ Register your services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<HealthprofileService>();
builder.Services.AddScoped<IHealthprofileRepo, HealthprofileRepo>();
builder.Services.AddScoped<SpendinglogService>();
builder.Services.AddScoped<ISpendinglogRepo, SpendinglogRepo>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationRepo, NotificationRepo>();
builder.Services.AddScoped<INotificationTypeRepo, NotificationTypeRepo>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IUserBlogRepo, UserBlogRepo>();
builder.Services.AddScoped<UserBlogService>();

builder.Services.AddScoped<IAdminBlogRepo, AdminBlogRepo>();
builder.Services.AddScoped<AdminBlogService>();

builder.Services.AddScoped<IAdminManageUserRepo, AdminManageUserRepo>();
builder.Services.AddScoped<AdminManageUserService>();
builder.Services.AddScoped<CloudinaryService>();

builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IAdminIngredientRepo, AdminIngredientRepo>();
builder.Services.AddScoped<AdminIngredientService>();

builder.Services.AddScoped<IUserIngredientRepo, UserIngredientRepo>();
builder.Services.AddScoped<UserIngredientService>();

builder.Services.AddScoped<IAdminMealRepo, AdminMealRepo>();
builder.Services.AddScoped<AdminMealService>();


builder.Services.AddScoped<IUserMealRepository, UserMealRepository>();
builder.Services.AddScoped<UserMealService>();

builder.Services.AddScoped<IMealHistoryRepo, MealHistoryRepo>();
builder.Services.AddScoped<MealHistoryService>();

builder.Services.AddScoped<IMealReviewRepo, MealReviewRepo>();
builder.Services.AddScoped<MealReviewService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailVerificationRepo, EmailVerificationRepo>();
builder.Services.AddScoped<EmailVerificationService>();

builder.Services.AddScoped<IForgetPasswordRepo, ForgetPasswordRepo>();
builder.Services.AddScoped<ForgetPasswordService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

// Swagger UI (Local Host)
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitPick API v1");
//        c.RoutePrefix = string.Empty;
//    });
//}


app.UseHttpsRedirection();

// Fix token without Bearer prefix
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(token) && !token.StartsWith("Bearer "))
    {
        context.Request.Headers["Authorization"] = $"Bearer {token}";
    }
    await next();
});

//To deploy
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");
Console.WriteLine($"Listening on port {port}");

app.UseCors("AllowReactApp");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger";  // Để swagger UI chạy ở /swagger thay vì /
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
