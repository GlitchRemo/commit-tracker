namespace FakeDataLoader.CustomGenerators;

public class CustomGeneratorConfig
{
    public string? FieldName { get; set; }
    public string? DataSetFileName { get; set; }
    public FieldRange? Range { get; set; }
}

public class FieldRange
{
    public int Min { get; set; }
    public int Max { get; set; }
}