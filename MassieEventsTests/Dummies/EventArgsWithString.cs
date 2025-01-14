using Scot.Massie.Events.Args;

namespace MassieEventsTests.Dummies;

public class EventArgsWithString : IEventArgs
{
    public string MyString { get; }
    
    public EventArgsWithString(string myString)
    {
        MyString = myString;
    }
}
