using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

/// <summary>
/// Wrapper for events allowing you to register listeners and dependent events to the wrapped event, while explicitly
/// not allowing you to invoke it or examine the listeners.
/// </summary>
/// <remarks>
/// This is intended to wrap events such that they can be made available on the public interface in a way that doesn't
/// allow invocation, and prevents access to invocation method via casting.
/// </remarks>
/// <typeparam name="TArgs">
/// The type of the event args. This will be a type that encloses information relating to the event. e.g. an event for
/// a sound playing might include fields in the event args object relating to which sound, or the volume.
/// </typeparam>
public class ProtectedEvent<TArgs> : IEvent<TArgs> 
    where TArgs : IEventArgs
{
    private readonly IEvent<TArgs> _inner;

    /// <summary>
    /// Wraps the given event in a new protected event instance.
    /// </summary>
    /// <param name="inner">The event to wrap.</param>
    public ProtectedEvent(IEvent<TArgs> inner)
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

    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        _inner.Register(dependentEvent);
    }

    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent, Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : IEventArgs
    {
        _inner.Register(dependentEvent, argConverter);
    }

    public void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : IEventArgs
    {
        _inner.Deregister(dependentEvent);
    }

    public void Deregister(EventListener<TArgs> listener)
    {
        _inner.Deregister(listener);
    }
}
