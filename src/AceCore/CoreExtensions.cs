using System;
using System.Collections.Generic;
using System.Linq;
using AceCore.Vehicles;

namespace AceCore
{
    public static class CoreExtensions
    {
        public static string CleanPath(this string rootPath) {
            return rootPath.Trim('\"', '/', '\\');
        }

        internal static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };

        public static string GetName(this Vehicles.VesselSlot vc) {
            return $"{vc.ObjectName}_{vc.Name.Split(':').First()}";
        }

        public static List<Vehicles.AircraftSlot> GetAircraft(this IEnumerable<VehicleSlot> vehicles) {
            return vehicles.Where(v => v.Type == VehicleType.Aircraft).Cast<AircraftSlot>().ToList();
        }

        public static List<Vehicles.VesselSlot> GetVessels(this IEnumerable<VehicleSlot> vehicles) {
            return vehicles.Where(v => v.Type == VehicleType.Vessel).Cast<VesselSlot>().ToList();
        }
    }
}