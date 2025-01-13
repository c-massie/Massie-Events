using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

public interface IPriorityEvent<TArgs> : IEvent<TArgs>
    where TArgs : IEventArgs
{
    void Register(EventListener listener, double priority);

    void Register(EventListener<TArgs> listener, double priority);
}
