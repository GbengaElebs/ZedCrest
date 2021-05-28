using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using ZedCrestTest.Persistence.DBContexts;

namespace ZedCrestTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.Development.json", optional: false)

                        .Build();

            var logPath = config.GetValue<string>("Logger:LogPath");
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.Hosting.LifeTime", LogEventLevel.Information)
                        .WriteTo.File(new JsonFormatter(), path: $@"{logPath}\{DateTime.Now:yyyy-MM-dd}\UserLogs.json",
                        rollingInterval: RollingInterval.Hour)
                        .CreateLogger();
            try
            {
                Log.Information("Starting up");
                //CreateHostBuilder(args).Build().Run();
                var host = CreateHostBuilder(args).Build();
                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ZedCrestContext>();
                        context.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An Error Occured during Migration");
                    }
                }
                host.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
