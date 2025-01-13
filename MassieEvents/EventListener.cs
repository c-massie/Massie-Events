using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

public delegate void EventListener();

public delegate void EventListener<in TArgs>(TArgs args) where TArgs : IEventArgs;
