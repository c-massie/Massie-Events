using Scot.Massie.Events.Args;
using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

public static class Events
{
    public static void InvokeMultiple(params (IInvocableEvent Event, IEventArgs Args)[] toInvoke)
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
}
