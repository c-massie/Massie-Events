using Xunit;
using Xunit.Abstractions;

namespace MassieEventsTests;

public class EventTest
{
    public ITestOutputHelper Output;

    public EventTest(ITestOutputHelper output)
    {
        Output = output;
    }
    
    // TO DO: Add tests by inheriting from IInvocableEventTest.
}

