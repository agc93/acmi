using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AceCore.Parsers;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Spectre.Console;

namespace InstallerCreator.Commands
{
    [Description("Builds the Mod Installer XML files for the given mod files.")]
    public class InfoCommand : AsyncCommand<InfoCommand.Settings>
    {
        private readonly List<IIdentifierParser> _parsers;
        private readonly ILogger<InfoCommand> _logger;
        private readonly AppInfoService _infoService;

        public InfoCommand(ILogger<InfoCommand> logger, IEnumerable<IIdentifierParser> parsers, AppInfoService infoService)
        {
            _parsers = parsers?.ToList() ?? new List<IIdentifierParser>();
            _logger = logger;
            _infoService = infoService;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var version = _infoService.GetAppVersion();
            var path = _infoService.GetAppPath();
            var table = new Table();
            table.AddColumn("Version");
            table.AddColumn("Runtime");
            table.AddColumn("OS");
            table.AddRow(
                version,
                _infoService.GetRuntimeName(),
                _infoService.OperatingSystemName
            );
            AnsiConsole.Render(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"Running from [bold grey]{path}[/] with [bold red]{_parsers.Count}[/] configured parsers");
            if (settings.CheckForUpdates) {
                AnsiConsole.WriteLine();
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", $"ACMI/{version}");
                        var resp = await client.GetStringAsync("https://api.github.com/repos/agc93/acmi/tags?per_page=1");
                        var dict = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(resp);
                        var latest = dict.First()["name"].ToString();
                        if (latest != version) {
                            AnsiConsole.MarkupLine($"[orange3]It looks like you are not running the latest version! You are running [bold white]{version}[/] and [bold white]{latest}[/] is available.[/]");
                            AnsiConsole.MarkupLine("[orange3]Please consider upgrading to the latest version from GitHub as I cannot support older versions[/]");
                        } else {
                            AnsiConsole.MarkupLine($"[green]It looks like you are running the latest available version: [bold white]{latest}[/][/]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error encountered while checking for updates!");
                    _logger.LogDebug(ex.Message);
                }
            }
            return 0;
        }

        public class Settings : AppSettings {
            [CommandOption("-c|--check")]
            [Description("Attempts to check the latest version available")]
            public bool CheckForUpdates {get;set;}
        }
    }
}