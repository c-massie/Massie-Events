using Scot.Massie.Events.CallInfo;
using Scot.Massie.Events.Protected;
using Scot.Massie.Events.Threadsafe;

namespace Scot.Massie.Events;

/// <inheritdoc cref="IInvocablePriorityEvent{TArgs}"/>
/// /// <remarks>
/// This implementation of <see cref="IPriorityEvent{TArgs}"/> is intended to be used in a single-threaded context, and
/// does nothing to preserve thread safety. If thread safety is required, (i.e. if working in a multi-threaded context)
/// you should use <see cref="ThreadsafePriorityEvent{TArgs}"/> instead.
/// </remarks>
public class PriorityEvent<TArgs> : IInvocablePriorityEvent<TArgs>
    where TArgs : EventArgs
{
    private readonly IList<(IInvocableEvent Event, Func<TArgs, EventArgs> Converter)>
        _dependentEventsWithArgConverters = new List<(IInvocableEvent, Func<TArgs, EventArgs>)>();

    private readonly ICollection<EventListener<TArgs>> _listenersWithoutPriority = new HashSet<EventListener<TArgs>>();

    private readonly ICollection<(EventListener<TArgs> Listener, double Priority)> _listenersWithPriority
        = new List<(EventListener<TArgs> Listener, double Priority)>();

    /// <inheritdoc />
    public ICollection<EventListener<TArgs>> Listeners =>
        _listenersWithPriority.Select(x => x.Listener)
                              .Concat(_listenersWithoutPriority)
                              .ToList();

    /// <inheritdoc />
    public IList<(EventListener<TArgs> Listener, double? Priority)> ListenersWithPriorities =>
        _listenersWithoutPriority.Select(x => (x, (double?)null))
                                 .Concat(_listenersWithPriority.Select(x => (x.Listener, (double?)x.Priority)))
                                 .ToList();

    /// <inheritdoc />
    public ICollection<IInvocableEvent> DependentEvents =>
        _dependentEventsWithArgConverters.Select(x => x.Event)
                                         .ToList();

    /// <inheritdoc />
    public bool ListenerOrderMatters =>
        _listenersWithPriority.Count != 0 || _dependentEventsWithArgConverters.Any(x => x.Event.ListenerOrderMatters);

    /// <summary>
    /// Creates a new event object with support for calling listeners in order of priority.
    /// </summary>
    public PriorityEvent()
    {
    }

    ProtectedEvent<TArgs> IInvocableEvent<TArgs>.Protected()
    {
        return new ProtectedEvent<TArgs>(this);
    }

    ProtectedPriorityEvent<TArgs> IInvocablePriorityEvent<TArgs>.Protected()
    {
        return new ProtectedPriorityEvent<TArgs>(this);
    }

    /// <inheritdoc />
    public void Invoke(TArgs args)
    {
        foreach(var listener in _listenersWithoutPriority)
            listener(args);

        if(_dependentEventsWithArgConverters.Count == 0)
        {
            foreach(var (listener, _) in _listenersWithPriority.OrderBy(x => x.Priority))
                listener(args);

            return;
        }

        var toCall = _listenersWithPriority
           .Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x.Listener, x.Priority, args));

        var alreadyInvolved = new HashSet<IInvocableEvent>() { this };
        var orderMatters    = _listenersWithPriority.Count != 0;
        
        foreach(var (depEvent, argConverter) in _dependentEventsWithArgConverters)
        {
            toCall = toCall.Concat(depEvent.GenerateCallInfo(argConverter(args),
                                                             alreadyInvolved,
                                                             out var orderMattersForThisDepEvent));

            if(orderMattersForThisDepEvent)
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
        _listenersWithoutPriority.Add(_ => listener());
    }

    /// <inheritdoc />
    public void Register(EventListener<TArgs> listener)
    {
        _listenersWithoutPriority.Add(listener);
    }

    /// <inheritdoc />
    public void Register(EventListener listener, double priority)
    {
        _listenersWithPriority.Add((_ => listener(), priority));
    }

    /// <inheritdoc />
    public void Register(EventListener<TArgs> listener, double priority)
    {
        _listenersWithPriority.Add((listener, priority));
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
        _listenersWithoutPriority.Remove(listener);

        foreach(var item in _listenersWithPriority.Where(x => x.Listener == listener).ToList()) 
            _listenersWithPriority.Remove(item);
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
        _listenersWithoutPriority.Clear();
        _listenersWithPriority.Clear();
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
        listenerOrderMatters = _listenersWithPriority.Count != 0;
        
        if(!alreadyInvolvedEvents.Add(this))
            return Enumerable.Empty<IEventListenerCallInfo>();

        var result = _listenersWithoutPriority
                    .Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x, null, args))
                    .Concat(_listenersWithPriority
                               .Select(IEventListenerCallInfo (x) 
                                           => new EventListenerCallInfo<TArgs>(x.Listener, x.Priority, args)));

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
