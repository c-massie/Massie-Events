using System;

namespace MassieEventsTests.Dummies;

public class EventArgsWithString : EventArgs
{
    public string MyString { get; }
    
    public EventArgsWithString(string myString)
    {
        MyString = myString;
    }
}
