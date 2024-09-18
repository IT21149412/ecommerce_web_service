using DotNetEnv;  // For loading environment variables
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Fetch connection string and database name from environment variables
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");

// Ensure that environment variables are properly loaded
if (string.IsNullOrEmpty(mongoDbConnectionString) || string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("MongoDB connection string or database name is missing from environment variables.");
}

// Add MongoDbContext as a singleton service
builder.Services.AddSingleton<MongoDbContext>(sp => new MongoDbContext(mongoDbConnectionString, databaseName));

// Register services (e.g., UserService, ProductService)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI for API documentation
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

// Map API routes
app.MapControllers();

app.Run();
