using Scot.Massie.Events.Args;

namespace Scot.Massie.Events.CallInfo;

public class EventListenerCallInfo<TArgs> : IEventListenerCallInfo<TArgs>
    where TArgs : EventArgs
{
    public EventListener<TArgs> Listener { get; }

    public double? Priority { get; }

    public TArgs Args { get; }

    public EventListenerCallInfo(EventListener<TArgs> listener, double? priority, TArgs args)
    {
        Listener = listener;
        Priority = priority;
        Args     = args;
    }

    public void CallListener()
    {
        Listener(Args);
    }
}
