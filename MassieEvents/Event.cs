using Scot.Massie.Events.CallInfo;
using Scot.Massie.Events.Protected;
using Scot.Massie.Events.Threadsafe;

namespace Scot.Massie.Events;

/// <inheritdoc cref="IEvent{TArgs}"/>
/// <remarks>
/// This implementation of <see cref="IEvent{TArgs}"/> is intended to be used in a single-threaded context, and does
/// nothing to preserve thread safety. If thread safety is required, (i.e. if working in a multi-threaded context) you
/// should use <see cref="ThreadsafeEvent{TArgs}"/> instead.
/// </remarks>
public class Event<TArgs> : IInvocableEvent<TArgs>
    where TArgs : EventArgs
{
    private readonly IList<(IInvocableEvent Event, Func<TArgs, EventArgs> Converter)> 
        _dependentEventsWithArgConverters = new List<(IInvocableEvent, Func<TArgs, EventArgs>)>();
    
    private readonly ICollection<EventListener<TArgs>> _listeners = new HashSet<EventListener<TArgs>>();

    /// <inheritdoc />
    public ICollection<EventListener<TArgs>> Listeners => _listeners.ToList();

    /// <inheritdoc />
    public ICollection<IInvocableEvent> DependentEvents =>
        _dependentEventsWithArgConverters.Select(x => x.Event).ToList();

    /// <inheritdoc />
    public bool ListenerOrderMatters => _dependentEventsWithArgConverters.Any(x => x.Event.ListenerOrderMatters);

    /// <summary>
    /// Creates a new event object.
    /// </summary>
    public Event()
    {
    }

    /// <inheritdoc />
    public ProtectedEvent<TArgs> Protected()
    {
        return new ProtectedEvent<TArgs>(this);
    }

    /// <inheritdoc />
    public void Invoke(TArgs args)
    {
        foreach(var listener in _listeners)
            listener(args);

        if(_dependentEventsWithArgConverters.Count == 0)
            return;

        var toCall                = Enumerable.Empty<IEventListenerCallInfo>();
        var orderMatters          = false;
        var alreadyInvolvedEvents = new HashSet<IInvocableEvent>(8) { this };

        foreach(var (depEvent, argConverter) in _dependentEventsWithArgConverters)
        {
            toCall = toCall.Concat(depEvent.GenerateCallInfo(argConverter(args),
                                                             alreadyInvolvedEvents,
                                                             out var orderMattersForThisEvent));

            if(orderMattersForThisEvent)
                orderMatters = true;
        }

        if(orderMatters)
            toCall = toCall.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in toCall)
            c.CallListener();
    }

    /// <inheritdoc />
    public void Register(EventListener listener)
    {
        _listeners.Add(_ => listener());
    }

    /// <inheritdoc />
    public void Register(EventListener<TArgs> listener)
    {
        _listeners.Add(listener);
    }

    /// <inheritdoc />
    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        _dependentEventsWithArgConverters.Add((dependentEvent, x => x));
    }

    /// <inheritdoc />
    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent, Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : EventArgs
    {
        _dependentEventsWithArgConverters.Add((dependentEvent, argConverter));
    }

    /// <inheritdoc />
    public void Deregister(EventListener<TArgs> listener)
    {
        _listeners.Remove(listener);
    }

    /// <inheritdoc />
    public void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : EventArgs
    {
        for(int i = _dependentEventsWithArgConverters.Count - 1; i >= 0; i--)
            if(ReferenceEquals(dependentEvent, _dependentEventsWithArgConverters[i].Event))
                _dependentEventsWithArgConverters.RemoveAt(i);
    }

    /// <inheritdoc />
    public void ClearListeners()
    {
        _listeners.Clear();
    }

    /// <inheritdoc />
    public void ClearDependentEvents()
    {
        _dependentEventsWithArgConverters.Clear();
    }

    /// <inheritdoc />
    public void Clear()
    {
        ClearListeners();
        ClearDependentEvents();
    }

    /// <inheritdoc />
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents)
    {
        return GenerateCallInfo(args, alreadyInvolvedEvents, out _);
    }

    /// <inheritdoc />
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>());
    }

    /// <inheritdoc />
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, 
                                                                ISet<IInvocableEvent> alreadyInvolvedEvents, 
                                                                out bool listenerOrderMatters)
    {
        listenerOrderMatters = false;

        if(!alreadyInvolvedEvents.Add(this))
            return Enumerable.Empty<IEventListenerCallInfo>();

        var result = _listeners.Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x, null, args));

        if(_dependentEventsWithArgConverters.Count != 0)
        {
            foreach(var (depEvent, argConverter) in _dependentEventsWithArgConverters)
            {
                result = result.Concat(depEvent.GenerateCallInfo(argConverter(args), 
                                                                 alreadyInvolvedEvents,
                                                                 out var orderMattersForThisDepEvent));

                if(orderMattersForThisDepEvent)
                    listenerOrderMatters = true;
            }
        }

        return result.ToList();
    }

    /// <inheritdoc />
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, out bool listenerOrderMatters)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>(), out listenerOrderMatters);
    }
}

