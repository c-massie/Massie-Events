using Scot.Massie.Events.Args;

namespace Scot.Massie.Events;

/// <summary>
/// Event listener delegate that doesn't take an <see cref="IEventArgs"/> object. This will be called by the
/// <see cref="IEvent">event</see> it's registered to.
/// </summary>
public delegate void EventListener();

/// <summary>
/// Event listener delegate. This will be called by the <see cref="IEvent">event</see> it's registered to, which will
/// pass an <see cref="IEventArgs"/> object containing information about the current event invocation.
/// </summary>
/// <typeparam name="TArgs">The type of the event args object.</typeparam>
public delegate void EventListener<in TArgs>(TArgs args) where TArgs : IEventArgs;
