using System;
using System.Collections.Generic;
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

    public abstract override IInvocablePriorityEvent<EventArgsWithInt> MakeDifferentEvent();

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
            listener.Priority.Should().Be(7);
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
        i.Priority.Should().Be(7);
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

    // TO DO: Add tests for making sure that whether listener order matters is reported correctly by GenerateCallInfo.
    
    [Fact]
    public void Invoke_OneListenerWithPriority()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"), priority: 7);
        e.Invoke(a);

        l.Should().HaveCount(1);
        l.Should().ContainSingle(x => x == "Noot");
    }

    [Fact]
    public void Invoke_MultipleListenersWithPriority()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"), priority: 7);
        e.Register(_ => l.Add("Toot"), priority: 7);
        e.Register(_ => l.Add("Boot"), priority: 7);
        e.Invoke(a);

        l.Should().HaveCount(3);
        l.Should().ContainSingle(x => x == "Noot");
        l.Should().ContainSingle(x => x == "Toot");
        l.Should().ContainSingle(x => x == "Boot");
    }

    [Fact]
    public void Invoke_MultipleListenersWithPriorityInOrder()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"), priority: 7);
        e.Register(_ => l.Add("Toot"), priority: 19);
        e.Register(_ => l.Add("Boot"), priority: 3);
        e.Invoke(a);

        l.Should().HaveCount(3);
        l[0].Should().Be("Boot");
        l[1].Should().Be("Noot");
        l[2].Should().Be("Toot");
    }
    
    [Fact]
    public void Invoke_ListenersWithNegativePriority()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"), priority: -7);
        e.Register(_ => l.Add("Toot"), priority: 19);
        e.Register(_ => l.Add("Hoot"), priority: -2);
        e.Register(_ => l.Add("Boot"), priority: 3);
        e.Invoke(a);

        l.Should().HaveCount(4);
        l[0].Should().Be("Noot");
        l[1].Should().Be("Hoot");
        l[2].Should().Be("Boot");
        l[3].Should().Be("Toot");
    }

    [Fact]
    public void Invoke_MixedListenersWithPriorityAndNoPriority()
    {
        var e = MakeEvent();
        var a = new EventArgsWithString("Doot");
        var l = new List<string>();
        
        e.Register(_ => l.Add("Noot"), priority: 7);
        e.Register(_ => l.Add("Toot"), priority: 19);
        e.Register(_ => l.Add("Boot"), priority: 3);
        e.Invoke(a);

        l.Should().HaveCount(3);
        l[0].Should().Be("Boot");
        l[1].Should().Be("Noot");
        l[2].Should().Be("Toot");
    }

    [Fact]
    public void Invoke_MixedListenersAndDependentEventsWithPriorityAndNoPriority()
    {
        var e1 = MakeEvent();
        var e2 = MakeDifferentEvent();
        var a  = new EventArgsWithString("7");
        var l  = new List<string>();
        
        e1.Register(e2, x => new EventArgsWithInt(int.Parse(x.MyString)));
        e1.Register(_ => l.Add("Noot"), 19);
        e1.Register(_ => l.Add("Moot"));
        e1.Register(_ => l.Add("Toot"), 3);
        e2.Register(_ => l.Add("Boot"), 7);
        e2.Register(_ => l.Add("Poot"));
        e2.Register(_ => l.Add("Hoot"), 28);
        e1.Invoke(a);

        l.Should().HaveCount(6);
        var firstTwo = l.Take(2).ToList();
        firstTwo.Should().Contain("Moot");
        firstTwo.Should().Contain("Poot");
        l[2].Should().Be("Toot");
        l[3].Should().Be("Boot");
        l[4].Should().Be("Noot");
        l[5].Should().Be("Hoot");
    }
}
