# Massie Events

This is an object-oriented event system with support for listener priority and cascading event invocation.

## How to install

### From Nuget

Include the package `Scot.Massie.Events`

### From Github

1. Go to https://github.com/c-massie/Massie-Events/releases
2. Download the latest `.dll` version of the library and the accompanying `.xml` file, containing documentation for the library. Place them together in the same desired directory.
3. Right-click the project you want to add this to, go to `Add Reference...` in VS, `Add... > Add Reference...` in Rider.
4. Click the "Add from..."/"Browse" button in the window that opens, and navigate to the .dll file downloaded.

**NOTE:** When installing in this method, some documentation (specifically, anything using the `<inheritdoc>` tag) will not be displayed properly in Rider or anything else using the ReSharper engine. [This issue is tracked here.](https://youtrack.jetbrains.com/issue/RSRP-478940/Rider-and-inheritdoc-support)

## How to use

### Creating events

#### Basic events

Events can be created on an interface or class by declaring them as a regular, read-only property.

```c#
public class MyEventArgs : EventArgs
{
    // ...    
}

public interface IMyInterface
{
    IEvent<MyEventArgs> ThingHappened { get; }
}

public class MyClass : IMyInterface
{
    public IEvent<MyEventArgs> ThingHappened { get; } = new Event<MyEventArgs>();
}
```

Events should generally be named with past-tense verbs, (e.g. `ThingHappened`) although it may make sense to name them with present-tense or future-tense verbs (e.g. `ThingHappening`, `ThingAboutToHappen`) where multiple events represent different steps in the process of something happening. Where the past tense could be misconstrued 

Events take a generic type argument, which is the type of the event args object passed to them. This should be a class that inherits from the `EventArgs` class - this is the same base class as is used by C#'s native delegate-based events.

The above example assumes a single-threaded context. If creating an event field that may be interacted with by multiple different threads, you should use `ThreadsafeEvent<...>` instead.

**Note:** C# has language-level syntax support for events, using the `event` keyword. Do not use this - this syntax is intended for delegate-based events, which is a separate system from this.

#### Events with support for listener priority

Instead of using `IEvent<...>` and `Event<...>`, you can use `IPriorityEvent` and `PriorityEvent` to declare an event where listeners can be registered with support for priorities.

```c#
public interface IMyInterface
{
    IPriorityEvent<MyEventArgs> ThingHappened { get; }
}

public class MyClass : IMyInterface
{
    public IPriorityEvent<MyEventArgs> ThingHappened { get; } = new PriorityEvent<MyEventArgs>();
}
```

Again, this example assumes a single-threaded context. If creating an event with support for registering listeners with a priority that may be interacted with by multiple different threads, you should use `ThreadsafePriorityEvent<...>` instead.

#### Protected events

In the above examples, although `IEvent<...>` and `IPriorityEvent<...>` don't expose any of the methods/properties required to invoke or manage the event, those events may still be cast to `IInvocableEvent<...>`, `IInvocablePriorityEvent<...>`, or their actual class types, which would provide access to those methods/properties. This can be prevented by using `ProtectedEvent<...>` or `ProtectedPriorityEvent<...>` as wrappers, which explicitly do not support any of the methods/properties introduced by `IInvocableEvent<...>` or `IInvocablePriorityEvent<...>`, and by storing a private reference to the original event.

You may want to do this where you're exposing events as part of an API or library, and don't want them to be invoked or managed outwith the class they're declared in.

```c#
public interface IMyInterface
{
    IEvent<MyEventArgs>         ThingHappened      { get; }
    IPriorityEvent<MyEventArgs> OtherThingHappened { get; }
}

public class MyClass : IMyInterface
{
    private readonly IInvocableEvent<MyEventArgs>         _thingHappened      = new Event<MyEventArgs>();
    private readonly IInvocablePriorityEvent<MyEventArgs> _otherThingHappened = new PriorityEvent<MyEventArgs>();
    
    public IEvent<MyEventArgs> ThingHappened { get; }
        = new ProtectedEvent<MyEventArgs>(_thingHappened);
    
    public IPriorityEvent<MyEventArgs> OtherThingHappened { get; }
        = new ProtectedPriorityEvent<MyEventArgs>(_otherThingHappened);
}
```

This can be shortened slightly with the `Protected()` method, which is available on instances of IInvocableEvent.

```c#
public interface IMyInterface
{
    IEvent<MyEventArgs>         ThingHappened      { get; }
    IPriorityEvent<MyEventArgs> OtherThingHappened { get; }
}

public class MyClass : IMyInterface
{
    private readonly IInvocableEvent<MyEventArgs>         _thingHappened      = new Event<MyEventArgs>();
    private readonly IInvocablePriorityEvent<MyEventArgs> _otherThingHappened = new PriorityEvent<MyEventArgs>();
    
    public IEvent<MyEventArgs>         ThingHappened      { get; } = _thingHappened     .Protected();
    public IPriorityEvent<MyEventArgs> OtherThingHappened { get; } = _otherThingHappened.Protected();
}
```

#### Dependent events

Events can be declared as being dependent on other events with `.Register(...)` - that is, when those other events are invoked, the dependent event also gets invoked. An event and all of its dependent events (recursively) are invoked together, and if any listeners are registered with a priority, those listeners are called in order of their priority with reference to all listeners of any event being invoked, and not just the event to which it's registered.

Events can also be deregistered as dependent events with `.Deregister(...)`.

```c#
public class BaaedEventArgs : EventArgs
{
    public double Volume { get; set; }
    
    public BaaedEventArgs(double volume)
    {
        this.Volume = volume;    
    }
}

public class MySheep
{
    private readonly IInvocableEvent<BaaedEventArgs> _baaed = new Event<BaaedEventArgs>();
    
    public IEvent<BaaedEventArgs> Baaed { get; } = _baaed.Protected();
}

public class MyPasture
{
    private IList<MySheep> Sheeps = new List<MySheep>();
    
    private readonly IInvocableEvent<BaaedEventArgs> _sheepBaaed = new Event<BaaedEventArgs>();
    
    public IEvent<BaaedEventArgs> SheepBaaed { get; } = _sheepBaaed.Protected();
    
    public void AddSheep(MySheep mySheep)
    {
        this.Sheeps.Add(mySheep);
        mySheep.Baaed.Register(this._sheepBaaed);
    }
    
    public void RemoveSheep(MySheep mySheep)
    {
        this.Sheeps.Remove(mySheep);
        mySheep.Baaed.Deregister(this._sheepBaaed);
    }
}
```

In the above example, when any sheep baas, (i.e. the `_baaed` event is invoked) the `_sheepBaaed` event of the pasture the sheep is in will also be invoked. The `_sheepBaaed` event being invoked on the pasture will not trigger the `_baaed`/`Baaed` event of any particular sheep.

This can be amended to convert the event args object to be more appropriate for the dependent event, by using different event args classes and providing an anonymous function to convert from one to the other. See below.

```c#
public class BaaedEventArgs : EventArgs
{
    public double Volume { get; set; }
    
    public BaaedEventArgs(double volume)
    {
        this.Volume = volume;    
    }
}

public class MySheep
{
    private readonly IInvocableEvent<BaaedEventArgs> _baaed = new Event<BaaedEventArgs>();
    
    public IEvent<BaaedEventArgs> Baaed { get; } = _baaed.Protected();
}

public class PastureSheepBaaedEventArgs : EventArgs
{
    private readonly BaaedEventArgs _sourceEventArgs;
    
    public MySheep Sheep  { get; }
    
    public double  Volume 
    {
        get => _sourceEventArgs.Volume;
        set => _sourceEventArgs.Volume = value;
    }
    
    public PastureSheepBaaedEventArgs(BaaedEventArgs perSheepEventArgs, MySheep sheep)
    {
        this._sourceEventArgs = perSheepEventArgs;
        this.Sheep            = sheep;
    }
}

public class MyPasture
{
    private IList<MySheep> Sheeps = new List<MySheep>();
    
    private readonly IInvocableEvent<PastureSheepBaaedEventArgs> _sheepBaaed = new Event<PastureSheepBaaedEventArgs>();
    
    public IEvent<PastureSheepBaaedEventArgs> SheepBaaed { get; } = _sheepBaaed.Protected();
    
    public void AddSheep(MySheep mySheep)
    {
        this.Sheeps.Add(mySheep);
        mySheep.Baaed.Register(this._sheepBaaed, args => new PastureSheepBaaedEventArgs(src, mySheep));
    }
    
    public void RemoveSheep(MySheep mySheep)
    {
        this.Sheeps.Remove(mySheep);
        mySheep.Baaed.Deregister(this._sheepBaaed);
    }
}
```

Two events can both be registered as being dependent on each-other. That is, where one is invoked, both are invoked.

An event having dependent events doesn't prevent it from being garbage-collected.

### Registering listeners

Listeners may be registered to events with `.Register(...)`, which takes an anonymous function. If the event is an `IPriorityEvent<...>`, you can pass a double as a second argument, which will be the priority.

```c#
myClassInstance.ThingHappened.Register(args => HandleThingHavingHappened());
myClassInstance.OtherThingHappened.Register(args => HandleOtherThingHavingHappened(), 7.5);
```

Event listeners will be called in order of priority, from lowest to highest, with listeners without a priority being called first. Where event args having some properties that may be set before whatever the event represents actually happens, this allows listeners with the highest priority to have the final say.

If you store the listener in a field or variable (as a `EventListener<TArgs>` where TArgs is an `EventArgs` type), you can pass this to the `.Deregister(...)` method to deregister the listener from the event.

```c#
EventListener<MyEventArgs> listener = args => DoSomethingElse();

myClassInstance.ThingHappened.Register(listener);

myClassInstance.ThingHappened.Deregister(listener);
```

### Invoking events

#### Single events

You can invoke an event with the `.Invoke(...)` method. This accepts an `EventArgs` object (matching the event) which will be passed to each direct listener, and used to derive an `EventArgs` object to pass to each dependent event.

This will call listeners without a priority, then listeners with a priority in order from lowest to highest priority.

#### Multiple events

Multiple unrelated events that are not dependent on each-other may be invoked at the same time (intermixing their listeners) with `Events.InvokeMultiple(...)`. There are overloads accepting pairs of events and args to pass to those events, as well as events with a single args object to pass to all events.

## Why

This is a re-write of an earlier library element that was written to project object-oriented C#-style events in Java. It was written for use in games, particularly for using in modding/plugin systems, where listeners from many different sources may have to listen to the same events.

C#, obviously, already has a native events system centred around delegates, which will be more performant than this - but this library adds the ability to call listeners in order of priority, to cascade event calls, and to invoke multiple events at once while respecting listener priority.
