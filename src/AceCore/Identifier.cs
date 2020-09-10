namespace AceCore
{
    public abstract class Identifier
    {
        public string RawValue { get; protected set; }
        public abstract string BaseObjectName {get; }
        // public string Slot {get;protected set;}
        public virtual string GetSlotName() {
            return RawValue;
        }
        public override abstract string ToString();

        protected string GetAircraftName(string aircraftCode) {
            var knownName = Constants.AircraftNames.TryGetValue(aircraftCode, out var name);
            if (knownName) {
                return name;
            } else {
                return aircraftCode.ToUpper();
            }
        }

        protected string GetItemName(string vehicleCode) {
            var knownName = Constants.AllItemNames.TryGetValue(vehicleCode, out var name);
            if (knownName) {
                return name;
            } else {
                return vehicleCode.ToUpper();
            }
        }

        public virtual string ObjectPath => "Nimbus/Content/";
    }
}