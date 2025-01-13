using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

public interface IInvocablePriorityEvent<TArgs> : IInvocableEvent<TArgs>, IPriorityEvent<TArgs>
    where TArgs : IEventArgs
{
    IList<(EventListener<TArgs> Listener, double? Priority)> ListenersWithPriorities { get; }
}
