using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using Scot.Massie.Events.CallInfo;
using Scot.Massie.Events.Dummies;
using Xunit;
using Xunit.Abstractions;

namespace Scot.Massie.Events;

[TestSubject(typeof(IInvocableEvent))]
[TestSubject(typeof(IInvocableEvent<>))]
// ReSharper disable once InconsistentNaming
public abstract class IInvocableEventTest
{
    // NOTE: There is not IEventTest because it doesn't make any sense to have an implementation of IEvent that doesn't
    //       implement IInvocableEvent, or wrap another event - A class that can't be read from and doesn't have any
    //       functionality can't really be tested because there'd be nothing to test.
    
    public ITestOutputHelper Output;

    protected IInvocableEventTest(ITestOutputHelper output)
    {
        Output = output;
    }

    protected abstract IInvocableEvent<EventArgsWithString> MakeEvent();

    protected abstract IInvocableEvent<EventArgsWithInt> MakeDifferentEvent();

    protected abstract IInvocablePriorityEvent<EventArgsWithString> MakeDifferentEventWithPriority();

    [Fact]
    public void RegisterListener_Single()
    {
        var e = MakeEvent();
        EventListener<EventArgsWithString> l = _ => { };
        
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
        var e = MakeEvent();
        var c = new Counter();
        void L() => c.Number += 1;
        
        e.Register(L);
        
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
        var e = MakeEvent();
        void L1() => Console.Out.WriteLine("First");
        void L2() => Console.Out.WriteLine("Second");
        void L3() => Console.Out.WriteLine("Third");
        
        e.Register(L1);
        e.Register(L2);
        e.Register(L3);
        e.ClearListeners();
        
        e.Listeners.Should().BeEmpty();
    }

    [Fact]
    public void ClearDependentEvents()
    {
        var e  = MakeEvent();
        var d1 = MakeEvent();
        var d2 = MakeEvent();
        var d3 = MakeEvent();

        e.Register(d1);
        e.Register(d2);
        e.Register(d3);
        e.ClearDependentEvents();

        e.DependentEvents.Should().BeEmpty();
    }

    [Fact]
    public void Clear()
    {
        var  e  = MakeEvent();
        void L1() => Console.Out.WriteLine("First");
        void L2() => Console.Out.WriteLine("Second");
        void L3() => Console.Out.WriteLine("Third");
        var  d1 = MakeEvent();
        var  d2 = MakeEvent();
        var  d3 = MakeEvent();
        
        e.Register(L1);
        e.Register(L2);
        e.Register(L3);
        e.Register(d1);
        e.Register(d2);
        e.Register(d3);
        e.Clear();

        e.Listeners.Should().BeEmpty();
        e.DependentEvents.Should().BeEmpty();
    }

    [Fact]
    public void GenerateCallInfo_NoListeners()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");

        var info = e.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().BeEmpty();
        orderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_OneListener()
    {
        IInvocableEvent<EventArgsWithString> e = MakeEvent();
        EventArgsWithString                  a = new EventArgsWithString("Doot");
        EventListener<EventArgsWithString>   l = _ => { };

        e.Register(l);
        var info = e.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().HaveCount(1);
        info[0].Should().BeAssignableTo<IEventListenerCallInfo<EventArgsWithString>>();
        var i = (IEventListenerCallInfo<EventArgsWithString>)info[0];
        i.Args.Should().BeSameAs(a);
        i.Listener.Should().BeSameAs(l);
        i.Priority.Should().BeNull();
        orderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_MultipleListeners()
    {
        IInvocableEvent<EventArgsWithString> e  = MakeEvent();
        EventArgsWithString                  a  = new EventArgsWithString("Doot");
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>   l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>   l3 = _ => { Console.Out.WriteLine("Third"); };

        e.Register(l1);
        e.Register(l2);
        e.Register(l3);
        var info = e.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().HaveCount(3);
        info.Should().AllSatisfy(x => x.Should().BeAssignableTo<IEventListenerCallInfo<EventArgsWithString>>());
        var infoCasted = info.Select(x => (IEventListenerCallInfo<EventArgsWithString>)x).ToList();
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l1));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l2));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l3));
        var i1 = infoCasted.First(x => ReferenceEquals(x.Listener, l1));
        var i2 = infoCasted.First(x => ReferenceEquals(x.Listener, l2));
        var i3 = infoCasted.First(x => ReferenceEquals(x.Listener, l3));
        i1.Priority.Should().BeNull();
        i2.Priority.Should().BeNull();
        i3.Priority.Should().BeNull();
        i1.Args.Should().Be(a);
        i2.Args.Should().Be(a);
        i3.Args.Should().Be(a);
        orderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_OneDependentEvent()
    {
        IInvocableEvent<EventArgsWithString> e1 = MakeEvent();
        IInvocableEvent<EventArgsWithInt>    e2 = MakeDifferentEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithInt>      l2 = _ => { Console.Out.WriteLine("Second"); };
        EventArgsWithString                  a  = new EventArgsWithString("7");
        
        e1.Register(l1);
        e2.Register(l2);
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        var info = e1.GenerateCallInfo(a, out var orderMatters).ToList();

        
        info.Should().HaveCount(2);
        info.Should().Contain(x => x is IEventListenerCallInfo<EventArgsWithString>);
        info.Should().Contain(x => x is IEventListenerCallInfo<EventArgsWithInt>);
        
        var i1 = (IEventListenerCallInfo<EventArgsWithString>)info
           .First(x => x is IEventListenerCallInfo<EventArgsWithString>);
        
        var i2 = (IEventListenerCallInfo<EventArgsWithInt>)info
           .First(x => x is IEventListenerCallInfo<EventArgsWithInt>);

        i1.Listener.Should().BeSameAs(l1);
        i1.Args.Should().BeSameAs(a);
        i1.Priority.Should().BeNull();
        i2.Listener.Should().BeSameAs(l2);
        i2.Args.MyInt.Should().Be(7);
        i2.Priority.Should().BeNull();
        orderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_MultipleDependentEvents()
    {
        IInvocableEvent<EventArgsWithString> e1 = MakeEvent();
        IInvocableEvent<EventArgsWithInt>    e2 = MakeDifferentEvent();
        IInvocableEvent<EventArgsWithString> e3 = MakeEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithInt>      l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>   l3 = _ => { Console.Out.WriteLine("Third"); };
        EventArgsWithString                  a  = new EventArgsWithString("7");
        
        e1.Register(l1);
        e2.Register(l2);
        e3.Register(l3);
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e1.Register(e3, x => new EventArgsWithString($"{x.MyString}{x.MyString}"));
        var info = e1.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().HaveCount(3);
        
        var infoForStr = info.Where(x => x is IEventListenerCallInfo<EventArgsWithString>)
                             .Cast<IEventListenerCallInfo<EventArgsWithString>>()
                             .ToList();
        
        var infoForInt = info.Where(x => x is IEventListenerCallInfo<EventArgsWithInt>)
                             .Cast<IEventListenerCallInfo<EventArgsWithInt>>()
                             .ToList();

        infoForStr.Should().HaveCount(2);
        infoForInt.Should().HaveCount(1);
        infoForStr.Should().Contain(x => ReferenceEquals(x.Listener, l1));
        infoForInt.Should().Contain(x => ReferenceEquals(x.Listener, l2));
        infoForStr.Should().Contain(x => ReferenceEquals(x.Listener, l3));
        var i1 = infoForStr.First(x => ReferenceEquals(x.Listener, l1));
        var i2 = infoForInt.Single();
        var i3 = infoForStr.First(x => ReferenceEquals(x.Listener, l3));

        i1.Args.Should().BeSameAs(a);
        i2.Args.MyInt.Should().Be(7);
        i3.Args.MyString.Should().Be("77");
        
        i1.Priority.Should().BeNull();
        i2.Priority.Should().BeNull();
        i3.Priority.Should().BeNull();

        orderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_MutuallyDependentEvents()
    {
        IInvocableEvent<EventArgsWithString> e1 = MakeEvent();
        IInvocableEvent<EventArgsWithInt>    e2 = MakeDifferentEvent();
        EventListener<EventArgsWithString>   l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithInt>      l2 = _ => { Console.Out.WriteLine("Second"); };
        EventArgsWithString                  a1 = new EventArgsWithString("7");
        EventArgsWithInt                     a2 = new EventArgsWithInt(13);

        e1.Register(l1);
        e2.Register(l2);
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e2.Register(e1, x => new EventArgsWithString(x.MyInt.ToString()));
        var e1Info = e1.GenerateCallInfo(a1, out var e1OrderMatters).ToList();
        var e2Info = e2.GenerateCallInfo(a2, out var e2OrderMatters).ToList();

        e1Info.Should().HaveCount(2);
        e2Info.Should().HaveCount(2);
        e1Info.Should().ContainSingle(x => x is IEventListenerCallInfo<EventArgsWithString>);
        e1Info.Should().ContainSingle(x => x is IEventListenerCallInfo<EventArgsWithInt>);
        e2Info.Should().ContainSingle(x => x is IEventListenerCallInfo<EventArgsWithInt>);
        e2Info.Should().ContainSingle(x => x is IEventListenerCallInfo<EventArgsWithString>);
        
        // ReSharper disable InconsistentNaming
        var e1i1 = (IEventListenerCallInfo<EventArgsWithString>)e1Info
           .First(x => x is IEventListenerCallInfo<EventArgsWithString>);
        
        var e1i2 = (IEventListenerCallInfo<EventArgsWithInt>)e1Info
           .First(x => x is IEventListenerCallInfo<EventArgsWithInt>);
        
        var e2i1 = (IEventListenerCallInfo<EventArgsWithInt>)e2Info
           .First(x => x is IEventListenerCallInfo<EventArgsWithInt>);
        
        var e2i2 = (IEventListenerCallInfo<EventArgsWithString>)e2Info
           .First(x => x is IEventListenerCallInfo<EventArgsWithString>);
        // ReSharper enable InconsistentNaming

        e1i1.Args.Should().BeSameAs(a1);
        e1i1.Listener.Should().BeSameAs(l1);
        e1i1.Priority.Should().BeNull();

        e1i2.Args.MyInt.Should().Be(7);
        e1i2.Listener.Should().BeSameAs(l2);
        e1i2.Priority.Should().BeNull();
        
        e2i1.Args.Should().BeSameAs(a2);
        e2i1.Listener.Should().BeSameAs(l2);
        e2i1.Priority.Should().BeNull();

        e2i2.Args.MyString.Should().Be("13");
        e2i2.Listener.Should().BeSameAs(l1);
        e2i2.Priority.Should().BeNull();

        e1OrderMatters.Should().BeFalse();
        e2OrderMatters.Should().BeFalse();
    }

    [Fact]
    public void GenerateCallInfo_DependentEventWithPriority_NoListeners()
    {
        IInvocableEvent<EventArgsWithString>         e1 = MakeEvent();
        IInvocablePriorityEvent<EventArgsWithString> e2 = MakeDifferentEventWithPriority();
        EventArgsWithString                          a  = new EventArgsWithString("7");
        
        e1.Register(e2);
        var info = e1.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().BeEmpty();
        orderMatters.Should().BeFalse();
    }
    
    [Fact]
    public void GenerateCallInfo_DependentEventWithPriority_ListenerWithoutPriority()
    {
        IInvocableEvent<EventArgsWithString>         e1 = MakeEvent();
        IInvocablePriorityEvent<EventArgsWithString> e2 = MakeDifferentEventWithPriority();
        EventListener<EventArgsWithString>           l  = _ => { Console.Out.WriteLine("Doot"); };
        EventArgsWithString                          a  = new EventArgsWithString("7");
        
        e2.Register(l);
        e1.Register(e2);
        var info = e1.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().HaveCount(1);
        info.Should().Contain(x => x is IEventListenerCallInfo<EventArgsWithString>);
        
        var i = (IEventListenerCallInfo<EventArgsWithString>)info
           .First(x => x is IEventListenerCallInfo<EventArgsWithString>);

        i.Listener.Should().BeSameAs(l);
        i.Args.Should().BeSameAs(a);
        i.Priority.Should().BeNull();
        orderMatters.Should().BeFalse();
    }
    
    [Fact]
    public void GenerateCallInfo_DependentEventWithPriority_ListenerWithPriority()
    {
        IInvocableEvent<EventArgsWithString>         e1 = MakeEvent();
        IInvocablePriorityEvent<EventArgsWithString> e2 = MakeDifferentEventWithPriority();
        EventListener<EventArgsWithString>           l  = _ => { Console.Out.WriteLine("Doot"); };
        EventArgsWithString                          a  = new EventArgsWithString("7");
        
        e2.Register(l, 7);
        e1.Register(e2);
        var info = e1.GenerateCallInfo(a, out var orderMatters).ToList();

        info.Should().HaveCount(1);
        info.Should().Contain(x => x is IEventListenerCallInfo<EventArgsWithString>);
        
        var i = (IEventListenerCallInfo<EventArgsWithString>)info
           .First(x => x is IEventListenerCallInfo<EventArgsWithString>);

        i.Listener.Should().BeSameAs(l);
        i.Args.Should().BeSameAs(a);
        i.Priority.Should().Be(7);
        orderMatters.Should().BeTrue();
    }
    
    [Fact]
    public void Invoke_NoListeners()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        
        e.Invoke(a);
        
        // Since invoke shouldn't do anything in this case, there's nothing to test; just making sure it actually runs.
    }

    [Fact]
    public void Invoke_OneListener()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"));
        e.Invoke(a);

        l.Should().HaveCount(1);
        l.Should().ContainSingle(x => x == "Noot");
    }

    [Fact]
    public void Invoke_MultipleListeners()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"));
        e.Register(_ => l.Add("Toot"));
        e.Register(_ => l.Add("Boot"));
        e.Invoke(a);

        l.Should().HaveCount(3);
        l.Should().ContainSingle(x => x == "Noot");
        l.Should().ContainSingle(x => x == "Toot");
        l.Should().ContainSingle(x => x == "Boot");
    }

    [Fact]
    public void Invoke_OneDependentEvent()
    {
        var e1 = MakeEvent();
        var e2 = MakeDifferentEvent();
        var a  = new EventArgsWithString("7");
        var l  = new List<string>();
        
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e1.Register(_ => l.Add("Noot"));
        e2.Register(_ => l.Add("Toot"));
        e1.Invoke(a);

        l.Should().HaveCount(2);
        l.Should().ContainSingle(x => x == "Noot");
        l.Should().ContainSingle(x => x == "Toot");
    }

    [Fact]
    public void Invoke_MultipleDependentEvents()
    {
        var e1 = MakeEvent();
        var e2 = MakeDifferentEvent();
        var e3 = MakeEvent();
        var a  = new EventArgsWithString("7");
        var l  = new List<string>();
        
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString) * 2));
        e1.Register(e3, x => new EventArgsWithString($"{x.MyString}{x.MyString}"));
        e1.Register(args => l.Add(args.MyString));
        e2.Register(args => l.Add(args.MyInt.ToString()));
        e3.Register(args => l.Add(args.MyString));
        e1.Invoke(a);

        l.Should().HaveCount(3);
        l.Should().ContainSingle(x => x == "7");
        l.Should().ContainSingle(x => x == "14");
        l.Should().ContainSingle(x => x == "77");
    }
}
