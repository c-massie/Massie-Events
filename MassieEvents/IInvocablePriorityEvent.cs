namespace Scot.Massie.Events;

/// <summary>
/// Listenable and invocable event with support for registering listeners with priority. You can register listeners that
/// will be called when this event happens/is invoked. You can also invoke this event, calling all registered listeners,
/// and those of dependent events.
/// </summary>
/// <remarks>Events should be exposed as read-only properties.</remarks>
/// <remarks>
/// Lower priorities are called first, higher priorities are called last. This results in higher priory listeners having
/// final say over the results of an event.
/// </remarks>
/// <typeparam name="TArgs">
/// The type of the event args. This will be a type that encloses information relating to the event. e.g. an event for
/// a sound playing might include fields in the event args object relating to which sound, or the volume.
/// </typeparam>
/// <example>
/// <code>
/// private readonly IInvocablePriorityEvent&lt;MyEventArgs&gt; _thingHappened
///     = new OrderedEvent&lt;MyEventArgs&gt; ();
///  
/// public IPriorityEvent&lt;MyEventArgs&gt; ThingHappened { get; }
///     = new ProtectedPriorityEvent&lt;MyEventArgs&gt;(_thingHappened);
/// </code>
///
/// This may be invoked with:
///
/// <code>
/// _thingHappened.Invoke(new MyEventArgs("Whatever gets passed to the event args object."));
/// </code>
/// </example>
public interface IInvocablePriorityEvent<TArgs> : IInvocableEvent<TArgs>, IPriorityEvent<TArgs>
    where TArgs : EventArgs
{
    /// <summary>
    /// The listeners registered to this event, paired with their priorities. Where a listener was registered without
    /// specifying a priority, it will appear in this list with a priority of null.
    /// </summary>
    IList<(EventListener<TArgs> Listener, double? Priority)> ListenersWithPriorities { get; }
    
    /// <summary>
    /// Provides this event, wrapped in a new <see cref="ProtectedPriorityEvent{TArgs}"/> instance.
    /// </summary>
    /// <returns>A new instance of <see cref="ProtectedPriorityEvent{TArgs}"/> wrapping this event.</returns>
    new ProtectedPriorityEvent<TArgs> Protected();
}
