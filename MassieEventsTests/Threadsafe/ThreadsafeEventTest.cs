using JetBrains.Annotations;
using Scot.Massie.Events.Dummies;
using Xunit.Abstractions;

namespace Scot.Massie.Events.Threadsafe;

[TestSubject(typeof(ThreadsafeEvent<>))]
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

