using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class ThreadsafeEventTest : IInvocableEventTest
{
    public ThreadsafeEventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    protected override IInvocableEvent<EventArgsWithString> MakeEvent()
    {
        return new ThreadsafeEvent<EventArgsWithString>();
    }

    protected override IInvocableEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new ThreadsafeEvent<EventArgsWithInt>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new ThreadsafePriorityEvent<EventArgsWithString>();
    }
}

