using Scot.Massie.Events.Args;

namespace Scot.Massie.Events.CallInfo;

public interface IEventListenerCallInfo
{
    public double? Priority { get; }

    public void CallListener();
}

public interface IEventListenerCallInfo<TArgs> : IEventListenerCallInfo
    where TArgs : IEventArgs
{
    public EventListener<TArgs> Listener { get; }

    public TArgs Args { get; }
}
