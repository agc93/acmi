using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;

namespace PackCreator
{
    class Program
    {
        private static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("ACMI_DEBUG");
            return string.IsNullOrWhiteSpace(envVar) 
                ? LogLevel.Information
                :  envVar.ToLower() == "trace"
                    ? LogLevel.Trace
                    : LogLevel.Debug;
        }
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            // services.AddSingleton<IOptionsPrompt<BuildCommand.Settings>, SharpromptOptionsPrompt>();
            services.AddSingleton<AppInfoService>();
            services.AddSingleton<ScriptDownloadService>();
            services.AddSingleton<PythonService>();
            services.AddSingleton< ExecEngine.CommandRunner>(provider => new ExecEngine.CommandRunner("python"));
            services.AddLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = GetLogLevel();
                });
            });
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.SetDefaultCommand<PackCommand>();
            app.Configure(c => {
                c.SetApplicationName("acmi-pack");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<InfoCommand>("info");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return await app.RunAsync(args);
        }
    }
}
