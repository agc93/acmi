namespace AceCore.Vehicles
{
    public static class VehicleExtensions
    {
        public static bool HasFaction(this AircraftSlot slot, NPCFaction faction) {
            return slot.Factions.HasValue && ((slot.Factions.Value & faction) == faction);
        }
    }
}