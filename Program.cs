using DistributedOrderSystem.Infrastructure.Data;
using DistributedOrderSystem.Middleware;
using DistributedOrderSystem.Workers;
using Microsoft.EntityFrameworkCore;





var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<OrderProcessingWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Seed(db);
}



    app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
