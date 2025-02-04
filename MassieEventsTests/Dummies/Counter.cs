namespace MassieEventsTests.Dummies;

public class Counter
{
    public int Number { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Counter(int number)
    {
        Number = number;
    }

    public Counter()
        : this(0)
    {
    }
}
