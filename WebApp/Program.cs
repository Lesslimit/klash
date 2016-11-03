using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Klash.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootDir = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(rootDir)
                 .AddCommandLine(args)
                 .AddJsonFile("appsettings.json")
                 .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                 .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(rootDir)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
