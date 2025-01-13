using System.Collections;
using Scot.Massie.Events.Args;
using Scot.Massie.Events.CallInfo;

namespace Scot.Massie.Events;

public interface IInvocableEvent : IEvent
{
    IEnumerable Listeners { get; }

    ICollection<IInvocableEvent> DependentEvents { get; }
    
    bool ListenerOrderMatters { get; }

    void Invoke(IEventArgs args);
    
    void ClearListeners();

    void ClearDependentEvents();

    void Clear();

    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(IEventArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents);
    
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(IEventArgs args);
}

public interface IInvocableEvent<TArgs> : IInvocableEvent, IEvent<TArgs>
    where TArgs : IEventArgs
{
    new ICollection<EventListener<TArgs>> Listeners { get; }

    IEnumerable IInvocableEvent.Listeners => Listeners;

    void Invoke(TArgs args);

    void IInvocableEvent.Invoke(IEventArgs args)
    {
        Invoke((TArgs)args);
    }

    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args, ISet<IInvocableEvent> alreadyInvolvedEvents);

    IEnumerable<IEventListenerCallInfo> IInvocableEvent.GenerateCallInfo(
        IEventArgs            args,
        ISet<IInvocableEvent> alreadyInvolvedEvents)
    {
        return GenerateCallInfo((TArgs)args, alreadyInvolvedEvents);
    }
    
    IEnumerable<IEventListenerCallInfo> GenerateCallInfo(TArgs args);

    IEnumerable<IEventListenerCallInfo> IInvocableEvent.GenerateCallInfo(IEventArgs args)
    {
        return GenerateCallInfo((TArgs)args);
    }
}
