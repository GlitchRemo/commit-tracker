using Bogus;

namespace FakeDataLoader.CustomGenerators;

public class RegionCodeGenerator : ICustomGenerator
{
    private const uint Max = 3;
    private static readonly Faker Faker = new();
    private uint _counter;
    public object Generate()
    {
        IncrementCounter();
        if (_counter % Max == 0)
        {
            return "49";
        }

        return Faker.Random.UInt(10U, 20U);
    }

    private void IncrementCounter()
    {
        if (_counter >= Max) _counter = 0;
        else _counter += 1;
    }
}