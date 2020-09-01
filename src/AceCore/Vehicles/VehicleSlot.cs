using System;
using System.Collections.Generic;
using System.Linq;

namespace AceCore.Vehicles
{
    public enum VehicleType {
        Aircraft,
        Vessel
    }
    [Flags]
    public enum NPCFaction {
        None = 0,
        Universal = 1,
        Osea = 2,
        Erusea = 4,
        DLC = 8

    }
    public class VehicleSlot : ModObject {
        public VehicleSlot(string objName, string friendlyName) : base(objName, friendlyName)
        {
        }
        public VehicleSlot()
        {
            
        }
        public virtual VehicleType Type {get;set;}
        public virtual bool HasAce {get;set;}
        public virtual NPCFaction? Factions {get;set;} = null;
        public List<string> NPCSlots {get;set;} = new List<string>();
        public int? Slots {get;set;}

        public virtual string PathRoot => "Nimbus/Content/Vehicles/";
    }

    public class ModObject {
        public ModObject()
        {
            
        }
        public ModObject(string objName, string friendlyName)
        {
            ObjectName = objName;
            Name = friendlyName;
        }
        public string ObjectName {get;set;}
        public string Name {get;set;}
    }

    public class AircraftSlot : VehicleSlot {
        public AircraftSlot(string objName, string friendlyName) : base(objName, friendlyName) {
        }
        public AircraftSlot(string objName, string friendlyName, IEnumerable<string> extraSlots) : base(objName, friendlyName)
        {
            NPCSlots = extraSlots.ToList();
        }

        public override bool HasAce => NPCSlots.Contains("02a");

        public override VehicleType Type => VehicleType.Aircraft;

        public override string PathRoot => base.PathRoot + "Aircraft/";
    }

    public class VesselSlot : VehicleSlot {
        public VesselSlot(string objName, string friendlyName) : base(objName, friendlyName) {
        }
        public override VehicleType Type => VehicleType.Vessel;
        public override string PathRoot => base.PathRoot + "Vessels/";
        public override NPCFaction? Factions => NPCFaction.Universal;
    }
}