namespace FakeDataLoader;

public class LoaderSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
    public string ModelSampleFileName { get; set; }
    public int NumberToGenerate { get; set; }
    public string ControlledFieldName { get; set; }
}