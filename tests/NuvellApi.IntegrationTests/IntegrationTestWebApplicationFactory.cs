using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
        //builder.ConfigureAppConfiguration((context, config) =>
        //{
        //    config.AddJsonFile("appsettings.Tests.json", optional: false);
        //});
        
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

        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            await SeedData(dbContext);
        }
    }

    private async Task SeedData(AppDbContext context)
    {
        var userManager = Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (await userManager.FindByEmailAsync("test@test-mail.com") == null)
        {
            var user = new AppUser
            {
                UserName = "test@test-mail.com",
                Email = "test@test-mail.com",
            };
            var result = await userManager.CreateAsync(user, "P@ssw0rd");

            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user");
            }
        }
    }
    
    public new async Task DisposeAsync()
    {
        await _databaseContainer.StopAsync();
    }
}