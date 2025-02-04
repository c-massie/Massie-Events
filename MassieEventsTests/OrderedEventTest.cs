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

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeEvent()
    {
        return new OrderedEvent<EventArgsWithString>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new OrderedEvent<EventArgsWithInt>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new OrderedEvent<EventArgsWithString>();
    }
}

