using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuvellAPI.Data;
using NuvellAPI.Models.Domain;
using Testcontainers.PostgreSql;

namespace NuvellApi.IntegrationTests;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithUsername("nuvell")
        .WithPassword("password")
        .WithDatabase("nuvell_test")
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        /*
         builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.test.json", optional: false);
        });
        */
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_databaseContainer.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestWebApplicationFactory>>();

        try
        {
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Database migration completed.");
            await SeedData(scope.ServiceProvider);
            Console.WriteLine("Database seeding completed.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    private async Task SeedData(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var logger = services.GetRequiredService<ILogger<IntegrationTestWebApplicationFactory>>();

        var existingUser = await userManager.FindByEmailAsync("test@test-mail.com");

        if (existingUser == null)
        {
            var user = new AppUser
            {
                UserName = "test@test-mail.com",
                Email = "test@test-mail.com",
            };

            var result = await userManager.CreateAsync(user, "P@ssw0rd");

            if (!result.Succeeded)
            {
                logger.LogError("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception("Failed to create user");
            }
        }
    }
    
    public new async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
    }
}