using gRPCOnHttp3.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace ES_gRPC.UnitTests;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly ITestOutputHelper _output;
    private readonly Action<IServiceCollection> _configure;
    private readonly string _dbName;

    public TestWebApplicationFactory(ITestOutputHelper output, Action<IServiceCollection> configure)
    {
        _dbName = Guid.NewGuid().ToString();
        _output = output;
        _configure = configure;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(loggingBuilder =>
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.TestOutput(_output, LogEventLevel.Information)
                .CreateLogger();
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            services.Remove(descriptor);

            _configure(services);

            services.AddDbContext<AppDbContext>(options =>
            {
#pragma warning disable CS0162 // Unreachable code detected
                if (TestBase.UseSqlDataBase)
                    options.UseSqlServer("Server=DESKTOP-SOL3OK2;Database=Es-learn-app;Integrated Security=True;TrustServerCertificate=True");
                else
                    options.UseInMemoryDatabase(_dbName);
#pragma warning restore CS0162 // Unreachable code detected
            });
        });
    }

}
