using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

/// <summary>
/// Utility static class for static methods pertaining to events.
/// </summary>
public static class Events
{
    /// <summary>
    /// Invokes multiple events together at the same time. Each event is passed its own <see cref="EventArgs"/> object,
    /// which may be used to generate the <see cref="EventArgs"/> objects passed to any dependent events.
    ///
    /// Listeners from all events are called in a single pool respecting priority if applicable. e.g. if event A and
    /// event B are invoked together with this static method, and event A has a listener with a higher priority than
    /// event B's listener, and a listener with a lower priority than event B's listener, then event A's lower priority
    /// listener will be called first, followed by event B's listener, followed by event A's higher priority listener.
    /// </summary>
    /// <param name="toInvoke">The events to invoke, along with their respective event args objects.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public static void InvokeMultiple(IEnumerable<(IInvocableEvent Event, EventArgs Args)> toInvoke)
    {
        var callInfo             = Enumerable.Empty<IEventListenerCallInfo>();
        var listenerOrderMatters = false;

        foreach(var (ev, args) in toInvoke)
        {
            callInfo             = callInfo.Concat(ev.GenerateCallInfo(args, out var listenerOrderMatterForThisEvent));
            listenerOrderMatters = listenerOrderMatters || listenerOrderMatterForThisEvent;
        }
        
        if(listenerOrderMatters)
            callInfo = callInfo.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in callInfo)
            c.CallListener();
    }
    
    /// <summary>
    /// Invokes multiple events together at the same time. Each event is passed its own <see cref="EventArgs"/> object,
    /// which may be used to generate the <see cref="EventArgs"/> objects passed to any dependent events.
    ///
    /// Listeners from all events are called in a single pool respecting priority if applicable. e.g. if event A and
    /// event B are invoked together with this static method, and event A has a listener with a higher priority than
    /// event B's listener, and a listener with a lower priority than event B's listener, then event A's lower priority
    /// listener will be called first, followed by event B's listener, followed by event A's higher priority listener.
    /// </summary>
    /// <param name="toInvoke">The events to invoke, along with their respective event args objects.</param>
    public static void InvokeMultiple(params (IInvocableEvent Event, EventArgs Args)[] toInvoke)
    {
        InvokeMultiple((IEnumerable<(IInvocableEvent, EventArgs)>)toInvoke);
    }

    /// <summary>
    /// Invokes multiple events together at the same time, passed the same <see cref="EventArgs"/> object to each one,
    /// which may be used to generate the <see cref="EventArgs"/> objects passed to any dependent events.
    ///
    /// Listeners from all events are called in a single pool respecting priority if applicable. e.g. if event A and
    /// event B are invoked together with this static method, and event A has a listener with a higher priority than
    /// event B's listener, and a listener with a lower priority than event B's listener, then event A's lower priority
    /// listener will be called first, followed by event B's listener, followed by event A's higher priority listener.
    /// </summary>
    /// <param name="toInvoke">The events to invoke.</param>
    /// <param name="args">The event args to pass to all directly invoked events.</param>
    public static void InvokeMultiple(IEnumerable<IInvocableEvent> toInvoke, EventArgs args)
    {
        var callInfo             = Enumerable.Empty<IEventListenerCallInfo>();
        var listenerOrderMatters = false;

        foreach(var ev in toInvoke)
        {
            callInfo             = callInfo.Concat(ev.GenerateCallInfo(args, out var listenerOrderMatterForThisEvent));
            listenerOrderMatters = listenerOrderMatters || listenerOrderMatterForThisEvent;
        }
        
        if(listenerOrderMatters)
            callInfo = callInfo.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in callInfo)
            c.CallListener();
    }
    
    /// <summary>
    /// Invokes multiple events together at the same time, passed the same <see cref="EventArgs"/> object to each one,
    /// which may be used to generate the <see cref="EventArgs"/> objects passed to any dependent events.
    ///
    /// Listeners from all events are called in a single pool respecting priority if applicable. e.g. if event A and
    /// event B are invoked together with this static method, and event A has a listener with a higher priority than
    /// event B's listener, and a listener with a lower priority than event B's listener, then event A's lower priority
    /// listener will be called first, followed by event B's listener, followed by event A's higher priority listener.
    /// </summary>
    /// <param name="toInvoke">The events to invoke.</param>
    /// <param name="args">The event args to pass to all directly invoked events.</param>
    public static void InvokeMultiple<TArgs>(IEnumerable<IInvocableEvent<TArgs>> toInvoke, TArgs args)
        where TArgs : EventArgs
    {
        var callInfo             = Enumerable.Empty<IEventListenerCallInfo>();
        var listenerOrderMatters = false;

        foreach(var ev in toInvoke)
        {
            callInfo             = callInfo.Concat(ev.GenerateCallInfo(args, out var listenerOrderMatterForThisEvent));
            listenerOrderMatters = listenerOrderMatters || listenerOrderMatterForThisEvent;
        }
        
        if(listenerOrderMatters)
            callInfo = callInfo.OrderBy(x => x.Priority ?? double.NegativeInfinity);

        foreach(var c in callInfo)
            c.CallListener();
    }
}
