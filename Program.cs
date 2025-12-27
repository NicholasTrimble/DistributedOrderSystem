using DistributedOrderSystem.Infrastructure.Data;
using DistributedOrderSystem.Middleware;
using DistributedOrderSystem.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

if (args.Contains("--generate-docker"))
{
    DistributedOrderSystem.Tools.DockerfileGenerator.GenerateDockerfile();
    return;
}


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<OrderProcessingWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiter =>
    {
        limiter.PermitLimit = 2;
        limiter.Window = TimeSpan.FromSeconds(5);
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();


app.UseRouting();

app.UseRateLimiter();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health");
    endpoints.MapGet("/debug-endpoints", async context =>
    {
        var dataSources = context.RequestServices.GetRequiredService<IEnumerable<EndpointDataSource>>();

        var endpointsList = dataSources
            .SelectMany(s => s.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText)
            .OrderBy(x => x)
            .ToList();

        await context.Response.WriteAsJsonAsync(endpointsList);
    });
});


// -------------------
// Database seeding
// -------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseSeeder.Seed(db);
}

app.Run();
