namespace AceCore
{
    public abstract class Identifier
    {
        public string RawValue { get; protected set; }
        public virtual string GetSlotName() {
            return RawValue;
        }
        public override abstract string ToString();
    }
}