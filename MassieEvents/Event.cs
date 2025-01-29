using Scot.Massie.Events.Args;
using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

/// <inheritdoc cref="IInvocableEvent{TArgs}"/>
public class Event<TArgs> : IInvocableEvent<TArgs>
    where TArgs : IEventArgs
{
    private readonly IList<(IInvocableEvent Event, Func<TArgs, IEventArgs> Converter)> 
        _dependentEventsWithArgConverters = new List<(IInvocableEvent, Func<TArgs, IEventArgs>)>();
    
    private readonly ICollection<EventListener<TArgs>> _listeners = new HashSet<EventListener<TArgs>>();

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
                return _listeners.ToList();
            }
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
                return _dependentEventsWithArgConverters.Any(x => x.Event.ListenerOrderMatters);
            }
        }
    }

    /// <summary>
    /// Creates a new event object.
    /// </summary>
    public Event()
    {
        
    }

    public void Invoke(TArgs args)
    {
        var toCall = GenerateCallInfo(args, out var listenerOrderMatters);

        if(listenerOrderMatters)
            toCall = toCall.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in toCall)
            c.CallListener();
    }

    public void Register(EventListener listener)
    {
        Register(_ => listener());
    }

    public void Register(EventListener<TArgs> listener)
    {
        lock(_lock)
        {
            _listeners.Add(listener);
        }
    }

    public void Register(IInvocableEvent<TArgs> dependentEvent)
    {
        Register(dependentEvent, x => x);
    }

    public void Register<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent,
                                     Func<TArgs, TOtherArgs>     argConverter)
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
            _listeners.Remove(listener);
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
            _listeners.Clear();
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
        return GenerateCallInfo(args, alreadyInvolvedEvents, out _);
    }

    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>());
    }

    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs                 args,
                                                                ISet<IInvocableEvent> alreadyInvolvedEvents,
                                                                out bool              listenerOrderMatters)
    {
        listenerOrderMatters = false;
        
        if(!alreadyInvolvedEvents.Add(this))
            return Enumerable.Empty<IEventListenerCallInfo>();

        lock(_lock)
        {
            IEnumerable<IEventListenerCallInfo> result
                = _listeners
                 .Select(IEventListenerCallInfo (x) => new EventListenerCallInfo<TArgs>(x, null, args))
                 .ToList();

            if(_dependentEventsWithArgConverters.Count != 0)
            {
                var forDependentEvents = new List<IEventListenerCallInfo>();

                foreach(var dep in _dependentEventsWithArgConverters)
                {
                    forDependentEvents.AddRange(dep.Event.GenerateCallInfo(dep.Converter(args),
                                                                           alreadyInvolvedEvents,
                                                                           out var depOrderMatters));

                    if(depOrderMatters)
                        listenerOrderMatters = true;
                }
                
                // See the note in the same method in OrderedEvent.
                
                result = result.Concat(forDependentEvents);
            }

            return result;
        }
    }

    public IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, out bool listenerOrderMatters)
    {
        return GenerateCallInfo(args, new HashSet<IInvocableEvent>(), out listenerOrderMatters);
    }
}
