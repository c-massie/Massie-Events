using Scot.Massie.Events.Args;

namespace MassieEventsTests.Dummies;

public class EventArgsWithInt : IEventArgs
{
    public int MyInt { get; }

    public EventArgsWithInt(int myInt)
    {
        MyInt = myInt;
    }
}
