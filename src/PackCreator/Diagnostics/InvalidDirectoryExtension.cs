namespace PackCreator.Diagnostics
{

    [System.Serializable]
    public class InvalidDirectoryException : System.Exception
    {
        public InvalidDirectoryException() { }
        public InvalidDirectoryException(string message) : base(message) { }
        public InvalidDirectoryException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidDirectoryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}