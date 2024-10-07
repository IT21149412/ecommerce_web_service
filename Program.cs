/*
 * ------------------------------------------------------------------------------
 * File:       Program.cs
 * Description: 
 *             This file configures and starts the ASP.NET Core Web API application. 
 *             It sets up essential services like MongoDB, JWT-based authentication, 
 *             CORS, and Swagger for API documentation. Environment variables are used 
 *             to configure MongoDB connection, database name, and JWT secret.
 * ------------------------------------------------------------------------------
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
DotNetEnv.Env.Load();

// Fetch connection string and database name from environment variables
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING")
    ?? throw new InvalidOperationException("MONGO_DB_CONNECTION_STRING is not set.");
var databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME")
    ?? throw new InvalidOperationException("DATABASE_NAME is not set.");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET is not set.");

// Register MongoDbContext and services
builder.Services.AddSingleton<MongoDbContext>(sp => new MongoDbContext(mongoDbConnectionString, databaseName));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<VendorReviewService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddSingleton(new TokenService(jwtSecret));

// CORS setup: Allow requests from localhost:3000 (React app)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());  // Adjust if you need to allow credentials
});

// JWT Authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Token failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully.");
                return Task.CompletedTask;
            }
        };
    });

// Add controllers
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS policy before authentication and authorization middleware
app.UseCors("AllowReactApp");

app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());


app.UseAuthentication(); // Use JWT authentication middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
