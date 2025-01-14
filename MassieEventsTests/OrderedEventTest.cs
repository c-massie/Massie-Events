using Xunit.Abstractions;

namespace MassieEventsTests;

public class OrderedEventTest
{
    public ITestOutputHelper Output;

    public OrderedEventTest(ITestOutputHelper output)
    {
        Output = output;
    }
}

