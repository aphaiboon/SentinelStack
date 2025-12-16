
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Configure the HTTP request pipeline
var app = builder.Build();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();