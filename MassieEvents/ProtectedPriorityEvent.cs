using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

public class ProtectedPriorityEvent<TArgs> : IPriorityEvent<TArgs>
    where TArgs : IEventArgs
{
    private readonly IPriorityEvent<TArgs> _inner;

    public ProtectedPriorityEvent(IPriorityEvent<TArgs> inner)
    {
        _inner = inner;
    }

    public void Register(EventListener listener)
    {
        _inner.Register(listener);
    }

    public void Register(EventListener<TArgs> listener)
    {
        _inner.Register(listener);
    }

    public void Register(EventListener listener, double priority)
    {
        _inner.Register(listener, priority);
    }

    public void Register(EventListener<TArgs> listener, double priority)
    {
        _inner.Register(listener, priority);
    }

    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        _inner.Register(dependentEvent);
    }

    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent, Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : IEventArgs
    {
        _inner.Register(dependentEvent, argConverter);
    }

    public void Deregister(EventListener<TArgs> listener)
    {
        _inner.Deregister(listener);
    }

    public void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : IEventArgs
    {
        _inner.Deregister(dependentEvent);
    }
}
