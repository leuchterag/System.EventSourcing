using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Reflection
{
    public static class ReflectionClientExtension
    {
        public static EventClient UseReflectionNameResolution(this EventClient subject)
        {
            subject.UseParser(
                (evt, type, evnt) =>
                {
                    var attribute = type.GetTypeInfo().GetCustomAttribute<EventAttribute>();
                    if(attribute == null)
                    {
                        throw new ArgumentException($"The type {type.Name} does not declare the EventAttribute");
                    }

                    evnt.Name = $"{attribute.Subject}.{attribute.Action}";

                    return Task.CompletedTask;
                });

            return subject;
        }
    }
}
