namespace FakeDataLoader.Models;

public class FieldDescriptor
{
    public string Name { get; set; }
    public SupportedType Type { get; set; }
    public int SampleSize { get; set; }
}