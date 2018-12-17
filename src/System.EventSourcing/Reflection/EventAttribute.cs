namespace System.EventSourcing.Reflection
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EventAttribute : Attribute
    {
        readonly string _subjectType;
        readonly string _action;

        // This is a positional argument
        public EventAttribute(string subject, string action)
        {
            _subjectType = subject;
            _action = action;
        }

        public string Subject
        {
            get { return _subjectType; }
        }
        
        public string Action
        {
            get { return _action; }
        }
    }
}
