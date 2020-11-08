using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AceCore;
using AceCore.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;

namespace PackCreator
{
    public class IndexCommand : AsyncCommand<IndexCommand.Settings>
    {
        private CommandApp GetApp() {
            var app = new CommandApp(new DependencyInjectionRegistrar(GetServices()));
            // app.SetDefaultCommand<PackCommand>();
            app.Configure(c => {
                c.PropagateExceptions();
                c.SetApplicationName("acmi-pack");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<InitCommand>("init");
                c.AddExample(new[] { "build" });
                c.AddExample(new[] { "build", "./ModPackFiles" });
                c.AddExample(new[] { "build", "--author", "agc93", "--title", "\"My Awesome Skin Pack\"", "--version", "1.0.0" });
            });
            return app;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var di = new DirectoryInfo(settings.FolderPath);
            if (di.Name != "Nimbus" && di.GetDirectories().Length == 0 && di.GetFiles().Length == 0) {
                //time to init
                var app = GetApp();
                var args = new[] {"init", settings.FolderPath}.Concat(context.Remaining.Raw);
                return await app.RunAsync(args);
            } else {
                //time to pack boys
                var app = GetApp();
                var args = new[] { "pack", settings.FolderPath}.Concat(context.Remaining.Raw);
                return await app.RunAsync(args);
            }
            
            // return await app.RunAsync(args);
            return 0;
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "<folder-path>")]
            public string FolderPath {get;set;}
        }

        private IServiceCollection GetServices() {
            var services = new ServiceCollection();
            services.AddSingleton<ScriptDownloadService>();
            services.AddSingleton<PythonService>();
            services.AddSingleton<ExecEngine.CommandRunner>(provider => new ExecEngine.CommandRunner("python"));
            services.AddSingleton<AppInfoService>();
            services.AddSingleton<BuildContextFactory>();
            services.AddSingleton<BuildService>();
            services.AddSingleton<FileNameService>();
            services.AddLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = Program.GetLogLevel();
                });
            });
            // services.Scan(scan =>
            //     scan.FromAssemblyOf<Identifier>()
            //         .AddClasses(classes => classes.AssignableTo(typeof(AceCore.Parsers.IIdentifierParser))).AsImplementedInterfaces().WithSingletonLifetime()
            // );
            services.AddSingleton<IIdentifierParser, AceCore.Parsers.SkinParser>();
            return services;
        }
    }
}