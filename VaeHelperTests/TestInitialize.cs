using Autofac;
using Autofac.Extensions.DependencyInjection;
using HospitalJob;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace VaeHelperTests
{
    public class TestInitialize
    {
        [AssemblyInitialize]
        public static IContainer Initialize<T>() where T : class
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");
            Environment.SetEnvironmentVariable("Environment", "dev");
            Environment.SetEnvironmentVariable("NewLine", "\r\n");
            var builder = CreateWebHostBuilder<T>(new string[] { });
            var provider = builder.Build().Services as AutofacServiceProvider;
            return provider.LifetimeScope as IContainer;
        }
        [AssemblyInitialize]
        public static IContainer Initialize()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");
            Environment.SetEnvironmentVariable("Environment", "dev");
            Environment.SetEnvironmentVariable("NewLine", "\r\n");
            var builder = CreateWebHostBuilder<Startup>(new string[] { });
            var provider = builder.Build().Services as AutofacServiceProvider;
            return provider.LifetimeScope as IContainer;
        }
        public static IWebHostBuilder CreateWebHostBuilder<T>(string[] args) where T : class =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<T>().ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddLog4Net("Config/log4net.xml", true);
#if DEBUG
                    logging.AddConsole();
                    logging.AddDebug();
#endif
                }).ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile(
                        "Config/appsettings.json", optional: true, reloadOnChange: true);
                });
    }
}
