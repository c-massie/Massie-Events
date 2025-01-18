using System.Collections;
using Scot.Massie.Events.Args;
using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

/// <summary>
/// Listenable and invocable event.
///
/// You can register listeners that will be called when this event happens/is invoked. If referring to this event using
/// a type argument, you would be able to register your listeners accepting an event args object, which should contain
/// information about the event being called.
///
/// You can also invoke this event, calling all registered listeners, and those of dependent events.
/// </summary>
/// <remarks>
/// Events should be exposed as read-only properties, and not as invocable events - see
/// <see cref="ProtectedEvent{TArgs}"/>. Invocable events should not be exposed as part of the public interface,
/// especially in APIs, since it would allow them to be invoked/managed outwith the class responsible for them.
/// </remarks>
/// <example>
/// <code>
/// private readonly IInvocableEvent&lt;MyEventArgs&gt; _thingHappened = new Event&lt;MyEventArgs&gt;();
///  
/// public IEvent&lt;MyEventArgs&gt; ThingHappened { get; } = new ProtectedEvent&lt;MyEventArgs&gt;(_thingHappened);
/// </code>
///
/// This may be invoked with:
///
/// <code>
/// _thingHappened.Invoke(new MyEventArgs("Whatever gets passed to the event args object."));
/// </code>
/// </example>
public interface IInvocableEvent : IEvent
{
    /// <summary>
    /// The listeners registered to this event. When this event is invoked, these listeners will be called.
    /// </summary>
    IEnumerable Listeners { get; }

    /// <summary>
    /// The events registered as being dependent on this one. When this event is invoked, so will these events.
    /// </summary>
    ICollection<IInvocableEvent> DependentEvents { get; }
    
    /// <summary>
    /// Whether invoking this event would result in its listeners being called in order, partially or wholly. This may
    /// be because this event or another event dependent on this one has listeners that were registered with a priority.
    /// </summary>
    bool ListenerOrderMatters { get; }

    /// <summary>
    /// Calls all listeners of this event, and invoked all dependent events.
    /// </summary>
    /// <param name="args">The event args object to pass to listeners.</param>
    void Invoke(IEventArgs args);
    
    /// <summary>
    /// Removes all listeners registered to this event; after calling, there will be no listeners registered directly to
    /// this event.
    /// </summary>
    void ClearListeners();

    /// <summary>
    /// Removes all dependent events registered to this event; after calling, this event will have no registered
    /// dependent events.
    /// </summary>
    void ClearDependentEvents();

    /// <summary>
    /// Removes all listeners and dependent events; after calling, this event will have no registered listeners nor
    /// dependent events.
    /// </summary>
    void Clear();

    /// <summary>
    /// Produces an enumerable of the event listeners registered to this event and all dependent events, paired with the
    /// event args object being passed to them (as this may be different for listeners if they're directly registered to
    /// different events) and the listener's priority.
    /// </summary>
    /// <param name="args">
    /// The event args object to pass to listeners, and which will be used to produce the event args object for
    /// dependent events if required.
    /// </param>
    /// <param name="alreadyInvolvedEvents">
    /// A set containing the events that have already produced call info as a result of this event invocation; these
    /// events shall not have additional call info requested of them. This allows circular dependency and co-dependency
    /// of events.
    /// </param>
    /// <returns>
    /// An enumerable of the event listeners registered to this event and all dependent events, paired with the event
    /// args object being passed to them (as this may be different for listeners if they're directly registered to
    /// different events) and the listener's priority. This is not necessarily in any particular order, even if this
    /// event is expected to call them in a particular order.
    /// </returns>
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(IEventArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents);
    
    /// <summary>
    /// Produces an enumerable of the event listeners registered to this event and all dependent events, paired with the
    /// event args object being passed to them (as this may be different for listeners if they're directly registered to
    /// different events) and the listener's priority.
    /// </summary>
    /// <param name="args">
    /// The event args object to pass to listeners, and which will be used to produce the event args object for
    /// dependent events if required.
    /// </param>
    /// <returns>
    /// An enumerable of the event listeners registered to this event and all dependent events, paired with the event
    /// args object being passed to them (as this may be different for listeners if they're directly registered to
    /// different events) and the listener's priority. This is not necessarily in any particular order, even if this
    /// event is expected to call them in a particular order.
    /// </returns>
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(IEventArgs args);
}

/// <summary>
/// Listenable and invocable generic event. You can register listeners that will be called when this event happens/is
/// invoked. You can also invoke this event, calling all registered listeners, and those of dependent events.
/// </summary>
/// <remarks>
/// Events should be exposed as read-only properties, and not as invocable events - see
/// <see cref="ProtectedEvent{TArgs}"/>. Invocable events should not be exposed as part of the public interface,
/// especially in APIs, since it would allow them to be invoked/managed outwith the class responsible for them.
/// </remarks>
/// <typeparam name="TArgs">
/// The type of the event args. This will be a type that encloses information relating to the event. e.g. an event for
/// a sound playing might include fields in the event args object relating to which sound, or the volume.
/// </typeparam>
/// <example>
/// <code>
/// private readonly IInvocableEvent&lt;MyEventArgs&gt; _thingHappened = new Event&lt;MyEventArgs&gt;();
///  
/// public IEvent&lt;MyEventArgs&gt; ThingHappened { get; } = new ProtectedEvent&lt;MyEventArgs&gt;(_thingHappened);
/// </code>
///
/// This may be invoked with:
///
/// <code>
/// _thingHappened.Invoke(new MyEventArgs("Whatever gets passed to the event args object."));
/// </code>
/// </example>
public interface IInvocableEvent<TArgs> : IInvocableEvent, IEvent<TArgs>
    where TArgs : IEventArgs
{
    /// <inheritdoc cref="IInvocableEvent.Listeners"/>
    new ICollection<EventListener<TArgs>> Listeners { get; }

    IEnumerable IInvocableEvent.Listeners => Listeners;

    /// <inheritdoc cref="IInvocableEvent.Invoke"/>
    void Invoke(TArgs args);

    void IInvocableEvent.Invoke(IEventArgs args)
    {
        Invoke((TArgs)args);
    }

    /// <inheritdoc cref="IInvocableEvent.GenerateCallInfo(Scot.Massie.Events.Args.IEventArgs,System.Collections.Generic.ISet{Scot.Massie.Events.IInvocableEvent})"/>
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents);

    
    IEnumerable<IEventListenerCallInfo> IInvocableEvent.GenerateCallInfo(
        IEventArgs            args,
        ISet<IInvocableEvent> alreadyInvolvedEvents)
    {
        return GenerateCallInfo((TArgs)args, alreadyInvolvedEvents);
    }
    
    /// <inheritdoc cref="IInvocableEvent.GenerateCallInfo(Scot.Massie.Events.Args.IEventArgs)"/>
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args);

    IEnumerable<IEventListenerCallInfo> IInvocableEvent.GenerateCallInfo(IEventArgs args)
    {
        return GenerateCallInfo((TArgs)args);
    }
}
