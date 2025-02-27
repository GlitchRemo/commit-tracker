using Bogus;
using FakeDataLoader.CustomGenerators;
using FakeDataLoader.Models;

namespace FakeDataLoader;

public class FakerWrapper
{
    private readonly List<CustomGeneratorConfig?> _customGeneratorConfigs;
    private static readonly Faker Faker = new();
    private readonly Dictionary<string, ICustomGenerator> _customGeneratorFactory;

    public FakerWrapper(List<CustomGeneratorConfig?> customGeneratorConfigs)
    {
        _customGeneratorConfigs = customGeneratorConfigs;
        _customGeneratorFactory = new Dictionary<string, ICustomGenerator>()
        {
            {
                nameof(CustomGeneratorFieldName.AccountNumber).ToLower(),
                new AccountNumberGenerator(GetConfig(nameof(CustomGeneratorFieldName.AccountNumber).ToLower()))
            },
            {
                nameof(CustomGeneratorFieldName.Zip).ToLower(),
                new AccountNumberGenerator(GetConfig(nameof(CustomGeneratorFieldName.Zip).ToLower()))
            },
            {
                "regioncsscode",
                new RegionCodeGenerator()
            },
            {
                "number",
                new AccountNumberGenerator(GetConfig("number"))
            },
            {
                "billingaccountid",
                new AccountNumberGenerator(GetConfig("billingaccountid"))
            }
        };
    }

    private CustomGeneratorConfig? GetConfig(string fieldName)
    {
        return _customGeneratorConfigs.FirstOrDefault(c => c.FieldName.ToLower() == fieldName);
    }

    public object? GenerateFieldValue(string fieldName, SupportedType type, int sampleSize)
    {
        if (_customGeneratorFactory.TryGetValue(fieldName.ToLower(), out var generator))
        {
            return generator.Generate();
        }

        return GenerateFieldValue(type, sampleSize);
    }

    private object? GenerateFieldValue(SupportedType type, int sampleSize)
    {
        return type switch
        {
            SupportedType.String => Faker.Random.String2(sampleSize / 2, sampleSize * 2),
            SupportedType.Number => Faker.Random.UInt((uint)sampleSize / 2, (uint)sampleSize * 2),
            SupportedType.Date => Faker.Date.Recent(sampleSize),
            SupportedType.Bool => Faker.Random.Bool(),
            SupportedType.Null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}