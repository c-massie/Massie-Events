using Scot.Massie.Events.Dummies;
using Xunit.Abstractions;

namespace Scot.Massie.Events.Threadsafe;

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

