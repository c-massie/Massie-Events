using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class OrderedEventTest : IInvocablePriorityEventTest
{
    public OrderedEventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    public override IInvocablePriorityEvent<EventArgsWithString> MakeEvent()
    {
        return new OrderedEvent<EventArgsWithString>();
    }

    public override IInvocablePriorityEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new OrderedEvent<EventArgsWithInt>();
    }

    public override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new OrderedEvent<EventArgsWithString>();
    }
}

