namespace Scot.Massie.Events.CallInfo;

/// <summary>
/// An <see cref="EventListener{TArgs}"/> paired with the information required to call it.
/// </summary>
public interface IEventListenerCallInfo
{
    // ReSharper disable once GrammarMistakeInComment
    /// <summary>
    /// The priority of the event listener. The higher, the later. Listeners with a priority of null will be called
    /// first.
    /// </summary>
    public double? Priority { get; }

    /// <summary>
    /// Calls the event listener, passing in the relevant <see cref="EventArgs"/> object.
    /// </summary>
    public void CallListener();
}

/// <summary>
/// An <see cref="EventListener{TArgs}"/> paired with the information required to call it.
/// </summary>
/// <typeparam name="TArgs">The type of the event args object.</typeparam>
public interface IEventListenerCallInfo<TArgs> : IEventListenerCallInfo
    where TArgs : EventArgs
{
    /// <summary>
    /// The listener that can be called.
    /// </summary>
    public EventListener<TArgs> Listener { get; }

    /// <summary>
    /// The <see cref="EventArgs"/> object intended to be passed to the listener when the listener is called.
    /// </summary>
    public TArgs Args { get; }
}
