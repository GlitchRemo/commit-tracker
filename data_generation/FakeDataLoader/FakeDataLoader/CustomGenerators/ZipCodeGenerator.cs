using System.Text.Json;
using Bogus;

namespace FakeDataLoader.CustomGenerators;

public class ZipCodeGenerator:ICustomGenerator
{
    private readonly CustomGeneratorConfig _customGeneratorConfig;
    private static readonly Faker Faker = new();
    private readonly HashSet<ulong> _sampleDataSet;
    public ZipCodeGenerator(CustomGeneratorConfig customGeneratorConfig)
    {
        _customGeneratorConfig = customGeneratorConfig;
        _sampleDataSet = !string.IsNullOrEmpty(_customGeneratorConfig.DataSetFileName)?DataSetReader():new HashSet<ulong>();
    }
    public ulong GenerateByRange(long min,long max)
    {
        return Faker.Random.ULong((ulong)min, (ulong)max);
    }
    
    public ulong GenerateFromSet(IEnumerable<ulong> predefinedSet)
    {
        return Faker.PickRandom(predefinedSet);
    }

    public object Generate()
    {
        if (!string.IsNullOrEmpty(_customGeneratorConfig.DataSetFileName))
        {
            return GenerateFromSet(_sampleDataSet);
        }

        if (_customGeneratorConfig.Range!=null)
        {
            return GenerateByRange(_customGeneratorConfig.Range.Min, _customGeneratorConfig.Range.Max);
        }

        return Faker.Random.String2(12, 12);
    }

    private HashSet<ulong> DataSetReader()
    {
        Console.WriteLine($"Reading sample data set: {_customGeneratorConfig.DataSetFileName}");
        HashSet<ulong> dataSet = new HashSet<ulong>();
        var jsonContents = File.ReadAllText($"CustomGenerators/{_customGeneratorConfig.DataSetFileName}");
        using var jsonDocument = JsonDocument.Parse(jsonContents);
        foreach (var element in jsonDocument.RootElement.EnumerateArray())
        {
            dataSet.Add(element.GetUInt64());
        }

        return dataSet;
    }
}

