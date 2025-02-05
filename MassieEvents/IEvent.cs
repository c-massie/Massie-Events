namespace Scot.Massie.Events;

/// <summary>
/// Listenable event. You can register listeners that will be called when this event happens/is invoked. If referring to
/// this event using a type argument, you would be able to register your listeners accepting an event args object, which
/// should contain information about the event being called.
/// </summary>
/// <remarks>
/// Events should be exposed as read-only properties.
/// </remarks>
/// <example>
/// <code>
/// private readonly IInvocableEvent&lt;MyEventArgs&gt; _thingHappened = new Event&lt;MyEventArgs&gt;&#40;&#41;;
/// // or `new ThreadsafeEvent&lt;MyEventArgs&gt;&#40;&#41;;` in a context where thread-safety is required.
/// 
/// public IEvent&lt;MyEventArgs&gt; ThingHappened { get; } = _thingHappened.Protected();
/// </code>
///
/// This may be listened to with:
///
/// <code>
/// ThingHappened.Register(() => Console.Out.WriteLine("Thing happened!"));
/// </code>
/// </example>
public interface IEvent
{
    /// <summary>
    /// Registers a listener to this event. When this event happens, the listener will be called.
    /// </summary>
    /// <param name="listener">The function that should be called when this event happens.</param>
    /// <remarks>
    /// If referring to this interface with a type argument specifying the event args type, you can register a listener
    /// that can read the event args object, which will contain information about the event being called.
    /// </remarks>
    void Register(EventListener listener);
}

/// <summary>
/// Listenable generic event. You can register listeners that will be called when this event happens/is invoked.
/// </summary>
/// <remarks>
/// Events should be exposed as read-only properties.
/// </remarks>
/// <typeparam name="TArgs">
/// The type of the event args. This will be a type that encloses information relating to the event. e.g. an event for
/// a sound playing might include fields in the event args object relating to which sound, or the volume.
/// </typeparam>
/// <example>
/// <code>
/// private readonly IInvocableEvent&lt;MyEventArgs&gt; _thingHappened = new Event&lt;MyEventArgs&gt;&#40;&#41;;
/// // or `new ThreadsafeEvent&lt;MyEventArgs&gt;&#40;&#41;;` in a context where thread-safety is required.
/// 
/// public IEvent&lt;MyEventArgs&gt; ThingHappened { get; } = _thingHappened.Protected();
/// </code>
///
/// This may be listened to with:
///
/// <code>
/// ThingHappened.Register(() => Console.Out.WriteLine("Thing happened!"));
/// </code>
/// </example>
public interface IEvent<TArgs> : IEvent where TArgs : EventArgs
{
    /// <summary>
    /// Registers a listener to this event. When this event happens, the listener will be called.
    /// </summary>
    /// <param name="listener">
    /// The function that should be called when this event happens. The function is passed a single "event args" object,
    /// which contains information about the event being invoked.
    /// </param>
    void Register(EventListener<TArgs> listener);

    /// <summary>
    /// Registers another event as being dependent on this one. When this event happens, that event will also happen.
    /// </summary>
    /// <param name="dependentEvent">The event to be invoked when this one is invoked.
    /// </param>
    /// <remarks>
    /// This event will not be invoked when the other one is invoked, unless this one is also registered as a dependent
    /// event of that one.
    /// </remarks>
    /// <remarks>
    /// This only accepts events using the same type of event args, and the event args object will be shared between
    /// the events. To register events taking different types of event args, or if otherwise needing to convert event
    /// args to be appropriate for a dependent event, you can pass in a function that will accept an event args object
    /// and produce another one.
    /// </remarks>
    void Register(IInvocableEvent<TArgs> dependentEvent);
    
    /// <summary>
    /// Registers another event as being dependent on this one. When this event happens, that event will also happen.
    /// </summary>
    /// <param name="dependentEvent">The event to be invoked when this one is invoked.
    /// </param>
    /// <param name="argConverter">
    /// The function to convert event args objects from this event's event args type to the other's.
    /// </param>
    /// <typeparam name="TOtherArgs">The type of the other event args.</typeparam>
    void Register<TOtherArgs>(IInvocableEvent<TOtherArgs>  dependentEvent,
                              Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : EventArgs;

    /// <summary>
    /// Deregisters a listener from this event. It will no longer be called when this event is invoked.
    /// </summary>
    /// <param name="listener">The listener to deregister.</param>
    void Deregister(EventListener<TArgs> listener);
    
    /// <summary>
    /// Deregisters a dependent event from this event. It will no longer be invoked when this event is invoked.
    /// </summary>
    /// <param name="dependentEvent">The event to deregister.</param>
    /// <typeparam name="TOtherArgs">The type of the other event's event args.</typeparam>
    void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : EventArgs;
}
