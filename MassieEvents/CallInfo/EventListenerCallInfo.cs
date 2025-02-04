namespace Scot.Massie.Events.CallInfo;

/// <inheritdoc cref="IEventListenerCallInfo{TArgs}"/>
public class EventListenerCallInfo<TArgs> : IEventListenerCallInfo<TArgs>
    where TArgs : EventArgs
{
    /// <inheritdoc />
    public EventListener<TArgs> Listener { get; }

    /// <inheritdoc />
    public double? Priority { get; }

    /// <inheritdoc />
    public TArgs Args { get; }

    /// <summary>
    /// Creates a new call info object, pairing a listener with the args to be passed to it when it's called, also
    /// including the event priority, if applicable.
    /// </summary>
    /// <param name="listener">The listener that may be called.</param>
    /// <param name="priority">The priority of the listener upon event invocation.</param>
    /// <param name="args">The event args object to be passed to the listener when it's called.</param>
    public EventListenerCallInfo(EventListener<TArgs> listener, double? priority, TArgs args)
    {
        Listener = listener;
        Priority = priority;
        Args     = args;
    }

    /// <inheritdoc />
    public void CallListener()
    {
        Listener(Args);
    }
}
