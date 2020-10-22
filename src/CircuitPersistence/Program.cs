using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CircuitPersistence
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
               var host = CreateHostBuilder(args).Build();
               
               await host.RunAsync();
               return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "A fatal error caused service to crash");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
				{
					configuration
						.Enrich.FromLogContext()
						.Enrich.WithProperty("Application", "CircuitPersistenceSample")
						.ReadFrom.Configuration(context.Configuration);
				})
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
