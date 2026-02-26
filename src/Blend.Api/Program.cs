using Blend.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Spoonacular API integration with two-tier caching
builder.Services.AddSpoonacularServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHealthChecks("/healthz");

app.UseAuthorization();

app.MapControllers();

app.Run();
