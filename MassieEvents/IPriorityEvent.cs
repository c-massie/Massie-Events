namespace Scot.Massie.Events;

/// <summary>
/// Listenable event with support for registering listeners with priority. You can register listeners that will be
/// called when this event happens/is invoked.
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
/// This may be listened to with:
///
/// <code>
/// ThingHappened.Register(() => Console.Out.WriteLine("Thing happened!"), priority: 7);
/// </code>
/// </example>
public interface IPriorityEvent<TArgs> : IEvent<TArgs>
    where TArgs : EventArgs
{
    /// <summary>
    /// Registers a listener to this event, with an associated priority. When this event happens, the listener will be
    /// called - after listeners without a priority or a lower priority, and before listeners with a higher priority.
    /// </summary>
    /// <param name="listener">The function that should be called when this event happens.</param>
    /// <param name="priority">
    /// The priority indicating where in the order of the listeners this event should be called. (lower first, higher
    /// last)
    /// </param>
    /// <remarks>
    /// You can register a listener that can read the event args object, which will contain information about the event
    /// being called.
    /// </remarks>
    void Register(EventListener listener, double priority);

    /// <summary>
    /// Registers a listener to this event, with an associated priority. When this event happens, the listener will be
    /// called - after listeners without a priority or a lower priority, and before listeners with a higher priority.
    /// </summary>
    /// <param name="listener">
    /// The function that should be called when this event happens. The function is passed a single "event args" object,
    /// which contains information about the event being invoked.
    /// </param>
    /// <param name="priority">
    /// The priority indicating where in the order of the listeners this event should be called. (lower first, higher
    /// last)
    /// </param>
    void Register(EventListener<TArgs> listener, double priority);
}
