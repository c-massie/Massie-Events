namespace MassieEventsTests.Dummies;

public class Counter
{
    public int Number { get; set; }

    public Counter(int number)
    {
        Number = number;
    }

    public Counter()
        : this(0)
    {
    }
}
