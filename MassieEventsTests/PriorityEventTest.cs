using Scot.Massie.Events.Dummies;
using Xunit.Abstractions;

namespace Scot.Massie.Events;

public class PriorityEventTest : IInvocablePriorityEventTest
{
    public PriorityEventTest(ITestOutputHelper output)
        : base(output)
    {
    }
    
    protected override IInvocablePriorityEvent<EventArgsWithString> MakeEvent()
    {
        return new PriorityEvent<EventArgsWithString>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new PriorityEvent<EventArgsWithInt>();
    }
    
    protected override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new PriorityEvent<EventArgsWithString>();
    }
}
