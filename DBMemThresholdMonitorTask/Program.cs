using System;
using System.Threading;
using System.Threading.Tasks;
using DBMemThresholdMonitorTask.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NavyBule.Core;
using NavyBule.Core.Events;
using NavyBule.Core.Infrastructure;
using NavyBule.Core.Messages;

namespace DBMemThresholdMonitorTask
{
    public class AppEngine : IHostedService
    {
        private static IMonitorService _monitorService;

        public AppEngine(IMonitorService monitorService)
        {
            _monitorService = monitorService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Application is running....");

            var j = new NJob();
            j.Run += (a, b) =>
          {
              string jobName = b.Name;
              bool paramResult = int.TryParse(b.RunParam, out int lowerBound);
              if (!paramResult)
              {
                  lowerBound = 99999;
              }
              _monitorService.RunMonitor(lowerBound, jobName).Wait();

          };


            j.Error += (a, b) =>
            {
                Console.WriteLine("{0} {1} ERROR：{2}", DateTime.Now, b.Def, b.Exception.Message);

            };

            j.Start();

            return Task.CompletedTask;
            //j.Stop();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class Program
    {
        private static IHost _host;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Application....");
            try
            {
                await StartUp();
            }
            catch (Exception e)
            {
                await _host.StopAsync();
            }

        }

        public static async Task StartUp()
        {
            //DI
            _host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", false, reloadOnChange: true);
            })
            .ConfigureAppConfiguration((hostContext, builder) =>
            {
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                builder.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true);
                //  builder.AddConfiguration(hostContext.Configuration.GetSection("MailConfig"));

            })
            .ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                serviceCollection.Configure<RMAConfig>(hostBuilderContext.Configuration.GetSection("MailConfig"));
                serviceCollection.AddHostedService<AppEngine>();
                serviceCollection.AddScoped<RMAConfig>();
                serviceCollection.AddTransient<IDBTargetRepository, DBTargetRepository>();
                serviceCollection.AddTransient<IContactRepository, ContactRepository>();
                serviceCollection.AddTransient<IMonitorService, MonitorService>();
                //   serviceCollection.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                //
                serviceCollection.AddScoped<ITokenizer, Tokenizer>();
                serviceCollection.AddScoped<ISmtpBuilder, SmtpBuilder>();

                serviceCollection.AddScoped<IEventPublisher, EventPublisher>();
                serviceCollection.AddScoped<IConsumer<ThresholdInfoDto>, EventMemoryWarning>();
                serviceCollection.AddScoped<IConsumer<ThresholdInfo>, EventDbConnectionFailed>();
                serviceCollection.AddScoped<INotifyService, NotifyService>();

            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {

                loggingBuilder.ClearProviders();
                loggingBuilder.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                //   loggingBuilder.AddConsole();
            })
            .Build();
            EngineContext.Create().ConfigureServiceProvider(_host.Services);
            await _host.RunAsync();


        }
    }

}
