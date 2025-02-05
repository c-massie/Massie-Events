using System;

namespace Scot.Massie.Events.Dummies;

public class EventArgsWithInt : EventArgs
{
    public int MyInt { get; }

    public EventArgsWithInt(int myInt)
    {
        MyInt = myInt;
    }
}
