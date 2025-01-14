using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

public interface IEvent
{
    void Register(EventListener listener);
}

public interface IEvent<TArgs> : IEvent where TArgs : IEventArgs
{
    void Register(EventListener<TArgs> listener);

    void Register(IInvocableEvent<TArgs> dependentEvent);
    
    void Register<TOtherArgs>(IInvocableEvent<TOtherArgs>  dependentEvent,
                              Func<TArgs, TOtherArgs> argConverter)
        where TOtherArgs : IEventArgs;

    void Deregister(EventListener<TArgs> listener);
    
    void Deregister<TOtherArgs>(IInvocableEvent<TOtherArgs> dependentEvent)
        where TOtherArgs : IEventArgs;
}
