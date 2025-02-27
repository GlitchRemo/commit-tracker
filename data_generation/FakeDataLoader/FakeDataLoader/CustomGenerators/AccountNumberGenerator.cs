using System.Text.Json;
using Bogus;

namespace FakeDataLoader.CustomGenerators;

public class AccountNumberGenerator : ICustomGenerator, IDisposable
{
    private readonly CustomGeneratorConfig _customGeneratorConfig;
    private static readonly Faker Faker = new();
    private readonly HashSet<string> _sampleDataSet;
    private readonly IEnumerator<int>? _rangeEnumerator;

    public AccountNumberGenerator(CustomGeneratorConfig customGeneratorConfig)
    {
        _customGeneratorConfig = customGeneratorConfig;
        _sampleDataSet = !string.IsNullOrEmpty(_customGeneratorConfig.DataSetFileName) ? DataSetReader() : new HashSet<string>();
        if (_customGeneratorConfig.Range != null)
        {
            _rangeEnumerator = Enumerable.Range(customGeneratorConfig.Range!.Min, customGeneratorConfig.Range.Max).GetEnumerator();
        }
    }

    private string GenerateFromSet(IEnumerable<string> predefinedSet)
    {
        return Faker.PickRandom(predefinedSet);
    }

    public object Generate()
    {
        if (!string.IsNullOrEmpty(_customGeneratorConfig.DataSetFileName))
        {
            return GenerateFromSet(_sampleDataSet);
        }

        if (_rangeEnumerator != null)
        {
            _rangeEnumerator.MoveNext();
            return _rangeEnumerator.Current.ToString();
        }

        return Faker.Random.String2(12, 12);
    }

    private HashSet<string> DataSetReader()
    {
        Console.WriteLine($"Reading sample data set: {_customGeneratorConfig.DataSetFileName}");
        HashSet<string> dataSet = new HashSet<string>();
        var jsonContents = File.ReadAllText($"CustomGenerators/{_customGeneratorConfig.DataSetFileName}");
        using var jsonDocument = JsonDocument.Parse(jsonContents);
        foreach (var element in jsonDocument.RootElement.EnumerateArray())
        {
            dataSet.Add(element.GetString());
        }

        return dataSet;
    }

    public void Dispose()
    {
        _rangeEnumerator?.Dispose();
    }
}