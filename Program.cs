using ConsumerATM.Services.Ivanti;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using CommandLine;
using ConsumerATM;

public class Program
{
    private static string _logPath = "";
    private static string _configPath = "";
    public class Options
    {
        [Option('c', "config", Required = false, HelpText = "Path to config file")]
        public string ConfigPath { get; set; }
        [Option('l', "log", Required = false, HelpText = "Path to logs")]
        public string LogsPath { get; set; }
    }
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .Enrich.WithMachineName()
            .CreateBootstrapLogger();

        Log.Information("Starting up!");
        Parser.Default.ParseArguments<Options>(args)
              .WithParsed(o =>
              {
                  _configPath = o.ConfigPath ?? "";
                  _logPath = o.LogsPath ?? "";
              });
        Log.Information($"config path {_configPath}");
        Log.Information($"log path {_logPath}");
        //if (args.Contains("--help"))
        //    return;
        //if (string.IsNullOrWhiteSpace(_configPath))
        //{
        //    Log.Error("     ");
        //    return;
        //}
        //if (args == null || args.Length < 1)
        //{
        //    Log.Warning("     ");
        //    args = new string[] { "" };
        //    return;
        //}
        try
        {
            CreateDefaultBuilder(args).Build().Run();

            Log.Information("Stopped cleanly");
            return;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
            return;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
               .UseSerilog((context, services, configuration) => configuration
               .ReadFrom.Configuration(context.Configuration)
               .ReadFrom.Services(services)
               .Enrich.FromLogContext()
               .MinimumLevel.Debug()
               .WriteTo.Console(outputTemplate:
                   "[{Timestamp:HH:mm:ss} {Level:u3}] [{MachineName}] {Message:lj}{NewLine}")
               .WriteTo.File(Path.Combine(_logPath, "log.txt"), rollingInterval: RollingInterval.Day)
               .Enrich.WithMachineName())
               .ConfigureServices((hostContext, services) =>
               {
                   IConfigurationRoot configuration;

                   var builder = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(_configPath, "appsettings.json"), optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                   configuration = builder.Build();

                   services.AddSingleton(configuration);

                   services.AddHostedService<Worker>();                 
                   services.AddSingleton<IIvantiService, IvantiService>();
               });
    }

}