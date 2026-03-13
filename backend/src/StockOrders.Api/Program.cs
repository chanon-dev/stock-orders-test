using Scalar.AspNetCore;
using StockOrders.Application;
using StockOrders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Clean Architecture DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Next.js default
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto Migrations & Seeding (runs in all environments)
await app.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
