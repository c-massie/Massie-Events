using System;
using System.Linq;
using FluentAssertions;
using MassieEventsTests.Dummies;
using Scot.Massie.Events;
using Scot.Massie.Events.CallInfo;
using Xunit;
using Xunit.Abstractions;

namespace MassieEventsTests;

// ReSharper disable once InconsistentNaming
public abstract class IInvocablePriorityEventTest : IInvocableEventTest
{
    protected IInvocablePriorityEventTest(ITestOutputHelper output)
        : base(output)
    {
        
    }

    public abstract override IInvocablePriorityEvent<EventArgsWithString> MakeEvent();

    public abstract override IInvocableEvent<EventArgsWithInt> MakeDifferentEvent();

    [Fact]
    public void RegisterListener_WithoutPriority()
    {
        IInvocablePriorityEvent<EventArgsWithString> e = MakeEvent();
        EventListener<EventArgsWithString>           l = _ => { };
        
        e.Register(l);

        {
            var listeners = e.ListenersWithPriorities;
            listeners.Should().HaveCount(1);
            var listener = listeners.Single();
            listener.Listener.Should().BeSameAs(l);
            listener.Priority.Should().BeNull();
        }

        {
            var listeners = e.Listeners;
            listeners.Should().HaveCount(1);
            var listener = listeners.Single();
            listener.Should().BeSameAs(l);
        }
    }
    
    [Fact]
    public void RegisterListener_WithPriority_Single()
    {
        IInvocablePriorityEvent<EventArgsWithString> e = MakeEvent();
        EventListener<EventArgsWithString>           l = _ => { };
        
        e.Register(l, 7);

        {
            var listeners = e.ListenersWithPriorities;
            listeners.Should().HaveCount(1);
            var listener = listeners.Single();
            listener.Listener.Should().BeSameAs(l);
            listener.Listener.Should().Be(7);
        }

        {
            var listeners = e.Listeners;
            listeners.Should().HaveCount(1);
            var listener = listeners.Single();
            listener.Should().BeSameAs(l);
        }
    }

    [Fact]
    public void RegisterListener_WithPriority_Multiple()
    {
        IInvocablePriorityEvent<EventArgsWithString> e  = MakeEvent();
        EventListener<EventArgsWithString>           l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>           l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>           l3 = _ => { Console.Out.WriteLine("Third"); };

        e.Register(l1, 7);
        e.Register(l2, 21);
        e.Register(l3, 3);
        
        var lwps = e.ListenersWithPriorities;
        lwps.Should().HaveCount(3);
        lwps.Should().ContainSingle(x => ReferenceEquals(x.Listener, l1));
        lwps.Should().ContainSingle(x => ReferenceEquals(x.Listener, l2));
        lwps.Should().ContainSingle(x => ReferenceEquals(x.Listener, l3));
        var lwp1 = lwps.First(x => ReferenceEquals(x.Listener, l1));
        var lwp2 = lwps.First(x => ReferenceEquals(x.Listener, l2));
        var lwp3 = lwps.First(x => ReferenceEquals(x.Listener, l3));
        lwp1.Priority.Should().Be(7);
        lwp2.Priority.Should().Be(21);
        lwp3.Priority.Should().Be(3);
        
    }

    [Fact]
    public void RegisterListener_WithPriority_NonGeneric()
    {
        IInvocablePriorityEvent<EventArgsWithString> e = MakeEvent();
        Counter                                      c = new Counter();
        EventListener                                l = () => { c.Number += 1; };

        e.Register(l, 7);


        var listeners = e.ListenersWithPriorities;
        listeners.Should().HaveCount(1);

        var lwp = listeners.First();
        lwp.Priority.Should().Be(7);
        lwp.Listener.Invoke(new EventArgsWithString("doot"));
        c.Number.Should().Be(1, because: "The listener should have incremented the counter.");
    }

    [Fact]
    public void GenerateCallInfo_OneListenerWithPriority()
    {
        IInvocablePriorityEvent<EventArgsWithString> e = MakeEvent();
        EventArgsWithString                          a = new EventArgsWithString("Doot");
        EventListener<EventArgsWithString>           l = _ => { };

        e.Register(l, 7);
        var info = e.GenerateCallInfo(a).ToList();

        info.Should().HaveCount(1);
        info[0].Should().BeAssignableTo<IEventListenerCallInfo<EventArgsWithString>>();
        var i = (IEventListenerCallInfo<EventArgsWithString>)info[0];
        i.Args.Should().BeSameAs(a);
        i.Listener.Should().BeSameAs(l);
        i.Priority.Should().BeNull();
    }

    [Fact]
    public void GenerateCallInfo_MultipleListenersWithPriority()
    {
        IInvocablePriorityEvent<EventArgsWithString> e  = MakeEvent();
        EventArgsWithString                          a  = new EventArgsWithString("Doot");
        EventListener<EventArgsWithString>           l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>           l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>           l3 = _ => { Console.Out.WriteLine("Third"); };

        e.Register(l1, 7);
        e.Register(l2, 21);
        e.Register(l3, 3);
        var info = e.GenerateCallInfo(a).ToList();

        info.Should().HaveCount(3);
        info.Should().AllSatisfy(x => x.Should().BeAssignableTo<IEventListenerCallInfo<EventArgsWithString>>());
        var infoCasted = info.Select(x => (IEventListenerCallInfo<EventArgsWithString>)x).ToList();
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l1));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l2));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l3));
        var i1 = infoCasted.First(x => ReferenceEquals(x.Listener, l1));
        var i2 = infoCasted.First(x => ReferenceEquals(x.Listener, l2));
        var i3 = infoCasted.First(x => ReferenceEquals(x.Listener, l3));
        i1.Priority.Should().Be(7);
        i2.Priority.Should().Be(21);
        i3.Priority.Should().Be(3);
        i1.Args.Should().Be(a);
        i2.Args.Should().Be(a);
        i3.Args.Should().Be(a);
    }

    [Fact]
    public void GenerateCallInfo_MultipleListeners_SomeWithPrioritySomeWithout()
    {
        IInvocablePriorityEvent<EventArgsWithString> e  = MakeEvent();
        EventArgsWithString                          a  = new EventArgsWithString("Doot");
        EventListener<EventArgsWithString>           l1 = _ => { Console.Out.WriteLine("First"); };
        EventListener<EventArgsWithString>           l2 = _ => { Console.Out.WriteLine("Second"); };
        EventListener<EventArgsWithString>           l3 = _ => { Console.Out.WriteLine("Third"); };
        EventListener<EventArgsWithString>           l4 = _ => { Console.Out.WriteLine("Fourth"); };

        e.Register(l1, 7);
        e.Register(l2);
        e.Register(l3, 3);
        e.Register(l4);
        var info = e.GenerateCallInfo(a).ToList();

        info.Should().HaveCount(4);
        info.Should().AllSatisfy(x => x.Should().BeAssignableTo<IEventListenerCallInfo<EventArgsWithString>>());
        var infoCasted = info.Select(x => (IEventListenerCallInfo<EventArgsWithString>)x).ToList();
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l1));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l2));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l3));
        infoCasted.Should().ContainSingle(x => ReferenceEquals(x.Listener, l4));
        var i1 = infoCasted.First(x => ReferenceEquals(x.Listener, l1));
        var i2 = infoCasted.First(x => ReferenceEquals(x.Listener, l2));
        var i3 = infoCasted.First(x => ReferenceEquals(x.Listener, l3));
        var i4 = infoCasted.First(x => ReferenceEquals(x.Listener, l4));
        i1.Priority.Should().Be(7);
        i2.Priority.Should().BeNull();
        i3.Priority.Should().Be(3);
        i4.Priority.Should().BeNull();
        i1.Args.Should().Be(a);
        i2.Args.Should().Be(a);
        i3.Args.Should().Be(a);
        i4.Args.Should().Be(a);
    }
}
