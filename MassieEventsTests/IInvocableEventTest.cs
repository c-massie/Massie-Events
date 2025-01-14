using System;
using System.Linq;
using FluentAssertions;
using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Xunit;
using Xunit.Abstractions;

namespace MassieEventsTests;

// ReSharper disable once InconsistentNaming
public abstract class IInvocableEventTest
{
    // NOTE: There is not IEventTest because it doesn't make any sense to have an implementation of IEvent that doesn't
    //       implement IInvocableEvent, or wrap another event - A class that can't be read from and doesn't have any
    //       functionality can't really be tested because there'd be nothing to test.
    
    public ITestOutputHelper Output;

    public IInvocableEventTest(ITestOutputHelper output)
    {
        Output = output;
    }

    public abstract IInvocableEvent<EventArgsWithString> MakeEvent();

    public abstract IInvocableEvent<EventArgsWithInt> MakeDifferentEvent();

    [Fact]
    public void RegisterListener_Single()
    {
        IInvocableEvent<EventArgsWithString> e = MakeEvent();
        EventListener<EventArgsWithString>   l = _ => { };
        
        e.Register(l);

        var listeners = e.Listeners;
        listeners.Should().HaveCount(1);
        listeners.Should().Contain(x => ReferenceEquals(x, l));
    }

    [Fact]
    public void RegisterListener_Multiple()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>   l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>   l3 = _ => { Console.Out.WriteLine("Third"); };
        
        e.Register(l1);
        e.Register(l2);
        e.Register(l3);
        
        var listeners = e.Listeners;
        listeners.Should().HaveCount(3);
        listeners.Should().Contain(x => ReferenceEquals(x, l1));
        listeners.Should().Contain(x => ReferenceEquals(x, l2));
        listeners.Should().Contain(x => ReferenceEquals(x, l3));
    }

    [Fact]
    public void RegisterListener_NonGeneric()
    {
        IInvocableEvent<EventArgsWithString> e = MakeEvent();
        Counter                              c = new Counter();
        EventListener                        l = () => { c.Number += 1; };
        
        e.Register(l);
        
        
        var listeners = e.Listeners;
        listeners.Should().HaveCount(1);
        
        var listener = listeners.First();
        listener.Invoke(new EventArgsWithString("doot"));
        c.Number.Should().Be(1, because: "The listener should have incremented the counter.");
    }

    [Fact]
    public void RegisterDependentEvent_WithConversion_Single()
    {
        IInvocableEvent<EventArgsWithString> e = MakeEvent();
        IInvocableEvent<EventArgsWithInt>    d = MakeDifferentEvent();

        e.Register(d, x => new EventArgsWithInt(int.Parse(x.MyString)));

        var eDeps = e.DependentEvents;
        var dDeps = d.DependentEvents;

        eDeps.Should().HaveCount(1);
        eDeps.Should().Contain(d);
        dDeps.Should().BeEmpty();
    }

    [Fact]
    public void RegisterDependentEvent_WithConversion_Multiple()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        IInvocableEvent<EventArgsWithInt>    d1 = MakeDifferentEvent();
        IInvocableEvent<EventArgsWithInt>    d2 = MakeDifferentEvent();
        IInvocableEvent<EventArgsWithInt>    d3 = MakeDifferentEvent();

        e.Register(d1, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e.Register(d2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e.Register(d3, x => new EventArgsWithInt(int.Parse(x.MyString)));
        
        var eDeps = e.DependentEvents;
        var dDeps1 = d1.DependentEvents;
        var dDeps2 = d2.DependentEvents;
        var dDeps3 = d3.DependentEvents;
        
        eDeps.Should().HaveCount(3);
        eDeps.Should().Contain(d1);
        eDeps.Should().Contain(d2);
        eDeps.Should().Contain(d3);

        dDeps1.Should().BeEmpty();
        dDeps2.Should().BeEmpty();
        dDeps3.Should().BeEmpty();
    }

    [Fact]
    public void RegisterDependentEvent_WithoutConversion_Single()
    {
        IInvocableEvent<EventArgsWithString> e = MakeEvent();
        IInvocableEvent<EventArgsWithString> d = MakeEvent();

        e.Register(d);

        var eDeps = e.DependentEvents;
        var dDeps = d.DependentEvents;

        eDeps.Should().HaveCount(1);
        eDeps.Should().Contain(d);
        dDeps.Should().BeEmpty();
    }

    [Fact]
    public void RegisterDependentEvent_WithoutConversion_Multiple()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        IInvocableEvent<EventArgsWithString> d1 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d2 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d3 = MakeEvent();

        e.Register(d1);
        e.Register(d2);
        e.Register(d3);

        var eDeps = e.DependentEvents;
        var dDeps1 = d1.DependentEvents;
        var dDeps2 = d2.DependentEvents;
        var dDeps3 = d3.DependentEvents;

        eDeps.Should().HaveCount(3);
        eDeps.Should().Contain(d1);
        eDeps.Should().Contain(d2);
        eDeps.Should().Contain(d3);

        dDeps1.Should().BeEmpty();
        dDeps2.Should().BeEmpty();
        dDeps3.Should().BeEmpty();
    }

    [Fact]
    public void DeregisterListener()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>   l2 = _ => { Console.Out.WriteLine("Second"); };
        
        e.Register(l1);
        e.Register(l2);
        e.Deregister(l1);
        
        var listeners = e.Listeners;
        listeners.Should().HaveCount(1);
        listeners.Should().Contain(x => ReferenceEquals(x, l2));
    }

    [Fact]
    public void DeregisterDependentEvent()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        IInvocableEvent<EventArgsWithString> d1 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d2 = MakeEvent();
        
        e.Register(d1);
        e.Register(d2);
        e.Deregister(d1);
        
        var deps = e.DependentEvents;
        deps.Should().HaveCount(1);
        deps.Should().Contain(d2);
    }

    [Fact]
    public void ClearListeners()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>   l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>   l3 = _ => { Console.Out.WriteLine("Third"); };
        
        e.Register(l1);
        e.Register(l2);
        e.Register(l3);
        e.ClearListeners();
        
        e.Listeners.Should().BeEmpty();
    }

    [Fact]
    public void ClearDependentEvents()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        IInvocableEvent<EventArgsWithString> d1 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d2 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d3 = MakeEvent();

        e.Register(d1);
        e.Register(d2);
        e.Register(d3);
        e.ClearDependentEvents();

        e.DependentEvents.Should().BeEmpty();
    }

    [Fact]
    public void Clear()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>   l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>   l3 = _ => { Console.Out.WriteLine("Third"); };
        IInvocableEvent<EventArgsWithString> d1 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d2 = MakeEvent();
        IInvocableEvent<EventArgsWithString> d3 = MakeEvent();
        
        e.Register(l1);
        e.Register(l2);
        e.Register(l3);
        e.Register(d1);
        e.Register(d2);
        e.Register(d3);
        e.Clear();

        e.Listeners.Should().BeEmpty();
        e.DependentEvents.Should().BeEmpty();
    }
    
    // TO DO: Write tests for call info.
}
