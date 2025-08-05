using Microsoft.EntityFrameworkCore;
using FitPick.Repository.Models;
using FitPick.Repository.Context;

var builder = WebApplication.CreateBuilder(args);

// ✅ Thêm DbContext và ConnectionString
builder.Services.AddDbContext<FitPickContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
