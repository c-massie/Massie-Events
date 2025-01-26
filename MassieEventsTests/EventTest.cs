using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class EventTest : IInvocableEventTest
{
    public EventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    public override IInvocableEvent<EventArgsWithString> MakeEvent()
    {
        return new Event<EventArgsWithString>();
    }

    public override IInvocableEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new Event<EventArgsWithInt>();
    }
}

