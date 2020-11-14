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

        public static string GetFriendlyName(this string objectName) {
            return Constants.AllItemNames.TryGetValue(objectName, out var fn) ? fn : objectName;
        }

        public static string OrDefault(this string value, string defaultValue) {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        public static string GetSlotNumber(this string slotValue, int padLength = 0) {
            return int.TryParse(slotValue, out var i)
                ? (i + 1).ToString($"D{padLength}")
                : slotValue;
        }

        public static List<string> AddIfSet(this List<string> collection, string value) {
            if (!string.IsNullOrWhiteSpace(value)) {
                collection.Add(value);
            }
            return collection;
        }

        public static List<string> AddIfNotExists(this List<string> collection, string value) {
            if (string.IsNullOrWhiteSpace(value) || collection.Contains(value)) {
                return collection;
            } else {
                collection.Add(value);
                return collection;
            }
        }
    }
}