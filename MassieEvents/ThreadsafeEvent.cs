using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

/// <inheritdoc cref="IInvocableEvent{TArgs}"/>
public class ThreadsafeEvent<TArgs> : IInvocableEvent<TArgs>
    where TArgs : EventArgs
{
    private readonly IList<(IInvocableEvent Event, Func<TArgs, EventArgs> Converter)> 
        _dependentEventsWithArgConverters = new List<(IInvocableEvent, Func<TArgs, EventArgs>)>();
    
    private readonly ICollection<EventListener<TArgs>> _listeners = new HashSet<EventListener<TArgs>>();

    /// <summary>
    /// Threadsafe lock. All operations on this are in synchronisation with this lock.
    /// </summary>
    private readonly object _lock = new();

    /// <inheritdoc />
    public ICollection<EventListener<TArgs>> Listeners
    {
        get
        {
            lock(_lock)
            {
                return _listeners.ToList();
            }
        }
    }

    /// <inheritdoc />
    public ICollection<IInvocableEvent> DependentEvents
    {
        get
        {
            lock(_lock)
            {
                return _dependentEventsWithArgConverters.Select(x => x.Event).ToList();
            }
        }
    }

    /// <inheritdoc />
    public bool ListenerOrderMatters
    {
        get
        {
            lock(_lock)
            {
                return _dependentEventsWithArgConverters.Any(x => x.Event.ListenerOrderMatters);
            }
        }
    }

    /// <summary>
    /// Creates a new event object.
    /// </summary>
    public ThreadsafeEvent()
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
        var toCall = GenerateCallInfo(args, out var listenerOrderMatters);

        if(listenerOrderMatters)
            toCall = toCall.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in toCall)
            c.CallListener();
    }

    /// <inheritdoc />
    public void Register(EventListener listener)
    {
        Register(_ => listener());
    }

    /// <inheritdoc />
    public void Register(EventListener<TArgs> listener)
    {
        lock(_lock)
        {
            _listeners.Add(listener);
        }
    }

    /// <inheritdoc />
    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        Register(dependentEvent, x => x);
    }

    /// <inheritdoc />
    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent,
                                     Func<TArgs, TOtherArgs>     argConverter)
        where TOtherArgs : EventArgs
    {
        lock(_lock)
        {
            _dependentEventsWithArgConverters.Add((dependentEvent, argConverter));
        }
    }

    /// <inheritdoc />
    public void Deregister(EventListener<TArgs> listener)
    {
        lock(_lock)
        {
            _listeners.Remove(listener);
        }
    }

    /// <inheritdoc />
    public void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : EventArgs
    {
        lock(_lock)
        {
            for(int i = 0; i < _dependentEventsWithArgConverters.Count; i++)
                if(ReferenceEquals(dependentEvent, _dependentEventsWithArgConverters[i].Event))
                    _dependentEventsWithArgConverters.RemoveAt(i);
        }
    }

    /// <inheritdoc />
    public void ClearListeners()
    {
        lock(_lock)
        {
            _listeners.Clear();
        }
    }

    /// <inheritdoc />
    public void ClearDependentEvents()
    {
        lock(_lock)
        {
            _dependentEventsWithArgConverters.Clear();
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock(_lock)
        {
            ClearListeners();
            ClearDependentEvents();
        }
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
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs                 args,
                                                                ISet<IInvocableEvent> alreadyInvolvedEvents,
                                                                out bool              listenerOrderMatters)
    {
        listenerOrderMatters = false;
        
        if(!alreadyInvolvedEvents.Add(this))
            return Enumerable.Empty<IEventListenerCallInfo>();
        
        IList<(IInvocableEvent Event, Func<TArgs, EventArgs> Converter)>? dependentEventsWithArgConverters = null;
        IEnumerable<IEventListenerCallInfo> result;

        lock(_lock)
        {
            result = _listeners
                    .Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x, null, args))
                    .ToList();
            
            if(_dependentEventsWithArgConverters.Count != 0)
                dependentEventsWithArgConverters = _dependentEventsWithArgConverters.ToList();
        }

        if(dependentEventsWithArgConverters is not null)
        {
            var forDependentEvents = new List<IEventListenerCallInfo>();
            
            foreach(var (ev, converter) in dependentEventsWithArgConverters)
            {
                forDependentEvents.AddRange(ev.GenerateCallInfo(converter(args),
                                                                alreadyInvolvedEvents,
                                                                out var depEvListenerOrderMatters));
                
                if(depEvListenerOrderMatters)
                    listenerOrderMatters = true;
            }

            result = result.Concat(forDependentEvents);
        }

        return result;
    }

    /// <inheritdoc />
    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, out bool listenerOrderMatters)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>(), out listenerOrderMatters);
    }
}
