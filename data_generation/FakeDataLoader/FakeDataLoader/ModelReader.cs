using System.Text.Json;
using FakeDataLoader.Models;

namespace FakeDataLoader;

public class ModelReader
{
    public IEnumerable<FieldDescriptor> GetModelDescriptorFromSampleFile(string fileName)
    {
        Console.WriteLine($"Analyzing sample document: {fileName}");
        var jsonContents = File.ReadAllText($"Models/{fileName}");
        using var jsonDocument = JsonDocument.Parse(jsonContents);
        foreach (var jsonProperty in jsonDocument.RootElement.EnumerateObject())
        {
            yield return new FieldDescriptor
            {
                Name = jsonProperty.Name,
                Type = jsonProperty.Value.ValueKind switch
                {
                    JsonValueKind.Number => SupportedType.Number,
                    JsonValueKind.String => SupportedType.String,
                    JsonValueKind.False or JsonValueKind.True => SupportedType.Bool,
                    JsonValueKind.Null => SupportedType.Null,
                    JsonValueKind.Object when jsonProperty.Value.EnumerateObject().Single().Name == "$date" => SupportedType.Date,
                    _ => throw UnsupportedTypeException(jsonProperty)
                },
                SampleSize = jsonProperty.Value.ValueKind switch
                {
                    JsonValueKind.Number => jsonProperty.Value.GetInt32(),
                    JsonValueKind.String => jsonProperty.Value.GetString()!.Length,
                    JsonValueKind.False or JsonValueKind.True => 0,
                    JsonValueKind.Null => 0,
                    JsonValueKind.Object when jsonProperty.Value.EnumerateObject().Single().Name == "$date" => 0,
                    _ => throw UnsupportedTypeException(jsonProperty)
                }
            };
        }
    }

    private static Exception UnsupportedTypeException(JsonProperty jsonProperty)
    {
        return new ApplicationException($"Unsupported JSON property: {Enum.GetName(typeof(JsonValueKind), jsonProperty.Value.ValueKind)}");
    }
}