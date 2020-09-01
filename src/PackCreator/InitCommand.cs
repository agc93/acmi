using AceCore;
using System.Threading.Tasks;
using Spectre.Cli;
using Microsoft.Extensions.Logging;
using Sharprompt;
using System.Collections.Generic;
using System.Linq;
using AceCore.Vehicles;
using System.IO;
using Spectre.Console;

namespace PackCreator
{
    public class InitCommand : AsyncCommand<InitCommand.Settings>
    {
        private readonly ILogger<InitCommand> _logger;

        public InitCommand(ILogger<InitCommand> logger)
        {
            _logger = logger;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var paths = new List<string>();
            var di = new DirectoryInfo(settings.FolderPath);
            var toContinue = Prompt.Confirm("Looks like this directory is empty. Would you like to initialise it for packing?", true);
            if (!toContinue) {
                return -1;
            } else {
                var objs = new List<VehicleSlot>();
                var categories = Prompt.MultiSelect("Please select the file types you will be packing", new[] {"Player Aircraft", "Non-playable Aircraft", "Vessels"});
                // var cat
                if (categories.Contains("Player Aircraft")) {
                    var aircraft = Prompt.MultiSelect("Please select the aircraft you want to set up", Constants.PlayerAircraft, valueSelector: n => n.Name);
                    var useNpc = false;
                    if (aircraft.Any(a => a.NPCSlots.Any())) {
                        useNpc = Prompt.Confirm("Would you like to create folders for the NPC variants?");
                    }
                    objs.AddRange(aircraft);
                }
                if (categories.Contains("Non-playable Aircraft")) {
                    var npAircraft = Prompt.MultiSelect("Please select the non-playable aircraft you want to set up", Constants.NonPlayableAircraft, valueSelector: n => n.Name);
                    objs.AddRange(npAircraft);
                }
                if (categories.Contains("Vessels")) {
                    var vesselChoice = Prompt.MultiSelect("Please select the vessels you want to set up", Constants.Vessels, valueSelector: n => n.Name);
                    objs.AddRange(vesselChoice);
                }
                foreach (var obj in objs)
                {
                    var objPath = obj.PathRoot + obj.ObjectName;
                    var targetPath = Path.Combine(di.Name == "Nimbus" ? Directory.GetParent(di.FullName).FullName : di.FullName, objPath);
                    _logger.LogInformation($"Created object structure for {obj.Name}/{obj.ObjectName}");
                    Directory.CreateDirectory(targetPath);
                    if (obj is AircraftSlot ac) {
                        Directory.CreateDirectory(Path.Combine(targetPath, "Materials"));
                        if (ac.Factions.HasValue) {
                            if (ac.HasFaction(NPCFaction.Universal)) {
                                Directory.CreateDirectory(Path.Combine(targetPath, "Textures"));
                            }
                            if (ac.HasFaction(NPCFaction.Osea)) {
                                Directory.CreateDirectory(Path.Combine(targetPath, "00"));
                            }
                            if (ac.HasFaction(NPCFaction.Erusea)) {
                                Directory.CreateDirectory(Path.Combine(targetPath, "01"));
                            }
                        } else {
                            foreach (var regSlot in Enumerable.Range(0,8))
                            {
                                Directory.CreateDirectory(Path.Combine(targetPath, $"0{regSlot}"));
                            }
                        }
                        foreach (var npcSlot in ac.NPCSlots)
                        {
                            Directory.CreateDirectory(Path.Combine(targetPath, npcSlot));
                        }
                    } if (obj is VesselSlot vs) {
                        Directory.CreateDirectory(Path.Combine(targetPath, "Materials"));
                        Directory.CreateDirectory(Path.Combine(targetPath, "Textures"));
                    }
                }
                _logger.LogWarning("Slot-specific folders have been created for all playable aircraft for slots 1-8");
                _logger.LogWarning("You should check exactly which slots your chosen aircraft actually suppports before packing!");
                _logger.LogInformation($"Object folders have been set up for {objs.Count} objects!");
                AnsiConsole.WriteLine("Press <ENTER> to continue...");
                System.Console.ReadLine();
            }
            return 0;
        }

        public class Settings : CommandSettings {
            [CommandArgument(0, "<folder-path>")]
            public string FolderPath {get;set;}
        }
    }
}