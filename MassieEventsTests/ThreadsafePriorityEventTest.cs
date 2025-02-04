using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class ThreadsafePriorityEventTest : IInvocablePriorityEventTest
{
    public ThreadsafePriorityEventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeEvent()
    {
        return new ThreadsafePriorityEvent<EventArgsWithString>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithInt> MakeDifferentEvent()
    {
        return new ThreadsafePriorityEvent<EventArgsWithInt>();
    }

    protected override IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority()
    {
        return new ThreadsafePriorityEvent<EventArgsWithString>();
    }
}

