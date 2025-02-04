using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class EventTest : IInvocableEventTest
{
    public EventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    protected override IInvocableEvent<EventArgsWithString> MakeEvent()
    {
        return new Event<EventArgsWithString>();
    }

    protected override IInvocableEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new Event<EventArgsWithInt>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new OrderedEvent<EventArgsWithString>();
    }
}

