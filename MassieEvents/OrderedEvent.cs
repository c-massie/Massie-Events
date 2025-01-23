using Scot.Massie.Events.Args;
using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

/// <inheritdoc cref="IInvocablePriorityEvent{TArgs}"/>
public class OrderedEvent<TArgs> : IInvocablePriorityEvent<TArgs>
    where TArgs : IEventArgs
{
    private readonly IList<(IInvocableEvent Event, Func<TArgs, IEventArgs> Converter)> 
        _dependentEventsWithArgConverters = new List<(IInvocableEvent, Func<TArgs, IEventArgs>)>();
    
    private readonly ICollection<EventListener<TArgs>> _listenersWithoutPriority = new HashSet<EventListener<TArgs>>();

    private readonly ICollection<(EventListener<TArgs> Listener, double Priority)> _listenersWithPriority
        = new List<(EventListener<TArgs> Listener, double Priority)>();
    
    /// <summary>
    /// Threadsafe lock. All operations on this are in synchronisation with this lock.
    /// </summary>
    private readonly object _lock = new();
    
    public ICollection<EventListener<TArgs>> Listeners
    {
        get
        {
            lock(_lock)
            {
                return _listenersWithPriority.Select(x => x.Listener)
                                             .Concat(_listenersWithoutPriority)
                                             .ToList();
            }
        }
    }

    public IList<(EventListener<TArgs> Listener, double? Priority)> ListenersWithPriorities
    {
        get
        {
            return _listenersWithoutPriority
                  .Select(x => (x, (double?)null))
                  .Concat(_listenersWithPriority.Select(x => (x.Listener, (double?)x.Priority)))
                  .ToList();
        }
    }

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

    public bool ListenerOrderMatters
    {
        get
        {
            lock(_lock)
            {
                return _listenersWithPriority.Count != 0
                    || _dependentEventsWithArgConverters.Any(x => x.Event.ListenerOrderMatters);
            }
        }
    }

    /// <summary>
    /// Creates a new ordered event object.
    /// </summary>
    public OrderedEvent()
    {
    }

    public void Invoke(TArgs args)
    {
        IList<IEventListenerCallInfo> callableListeners;

        lock(_lock)
        {
            var callInfo = GenerateCallInfo(args);

            if(ListenerOrderMatters)
                callInfo = callInfo.OrderBy(x => x.Priority ?? double.NegativeInfinity);

            callableListeners = callInfo.ToList();
        }

        foreach(var callInfo in callableListeners)
            callInfo.CallListener();
    }

    public void Register(EventListener listener)
    {
        Register(_ => listener());
    }

    public void Register(EventListener<TArgs> listener)
    {
        lock(_lock)
        {
            _listenersWithoutPriority.Add(listener);
        }
    }

    public void Register(EventListener listener, double priority)
    {
        Register(_ => listener(), priority);
    }

    public void Register(EventListener<TArgs> listener, double priority)
    {
        lock(_lock)
        {
            _listenersWithPriority.Add((listener, priority));
        }
    }

    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        Register(dependentEvent, x => x);
    }

    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent, Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : IEventArgs
    {
        lock(_lock)
        {
            _dependentEventsWithArgConverters.Add((dependentEvent, x => argConverter(x)));
        }
    }

    public void Deregister(EventListener<TArgs> listener)
    {
        lock(_lock)
        {
            _listenersWithoutPriority.Remove(listener);

            foreach(var item in _listenersWithPriority.Where(x => x.Listener == listener).ToList()) 
                _listenersWithPriority.Remove(item);
        }
    }

    public void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : IEventArgs
    {
        lock(_lock)
        {
            for(int i = 0; i < _dependentEventsWithArgConverters.Count; i++)
                if(ReferenceEquals(dependentEvent, _dependentEventsWithArgConverters[i].Event))
                    _dependentEventsWithArgConverters.RemoveAt(i);
        }
    }

    public void ClearListeners()
    {
        lock(_lock)
        {
            _listenersWithoutPriority.Clear();
            _listenersWithPriority.Clear();
        }
    }

    public void ClearDependentEvents()
    {
        lock(_lock)
        {
            _dependentEventsWithArgConverters.Clear();
        }
    }

    public void Clear()
    {
        lock(_lock)
        {
            ClearListeners();
            ClearDependentEvents();
        }
    }

    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents)
    {
        if(!alreadyInvolvedEvents.Add(this))
            return Enumerable.Empty<IEventListenerCallInfo>();

        lock(_lock)
        {
            var withPriority
                = _listenersWithPriority
                   .Select(IEventListenerCallInfo (x)
                               => new EventListenerCallInfo<TArgs>(x.Listener, x.Priority, args));

            var withoutPriority
                = _listenersWithoutPriority
                   .Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x, null, args));

            var forDependentEvents
                = _dependentEventsWithArgConverters
                   .SelectMany(x => x.Event.GenerateCallInfo(x.Converter(args), alreadyInvolvedEvents));

            return withPriority.Concat(withoutPriority).Concat(forDependentEvents);
        }
    }

    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>());
    }
}
