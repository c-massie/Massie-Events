namespace Scot.Massie.Events.Args;

/// <summary>
/// Cancellable event. Generally, the event of an event args object implementing this would be fired before the actual
/// thing happening, and would check the event args object to see if the thing should happen or not.
/// </summary>
/// <example>
/// <code>
/// var args = new SomeCancellableEventArgs();
/// 
/// this.MyEvent.Invoke(args);
/// 
/// if(!args.EventIsCancelled)
/// {
///     DoTheThing();
/// }
/// </code>
/// </example>
public interface ICancellableEventArgs : IEventArgs
{
    /// <summary>
    /// Whether the event is cancelled. If false, the thing will happen as expected. If true, the thing shouldn't
    /// happen.
    /// </summary>
    bool EventIsCancelled { get; }
}
