using FakeDataLoader;
using FakeDataLoader.CustomGenerators;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var settings = new LoaderSettings();
configuration.GetRequiredSection(nameof(LoaderSettings)).Bind(settings);

var customeGeneratorConfigs = new List<CustomGeneratorConfig?>();
configuration.GetRequiredSection("CustomGeneratorConfigs").Bind(customeGeneratorConfigs);


var modelReader = new ModelReader();
var descriptor = modelReader.GetModelDescriptorFromSampleFile(settings.ModelSampleFileName);

var writer = new MongoWriter(settings.ConnectionString, settings.DatabaseName,customeGeneratorConfigs);

await writer.EnsureCollectionCreated(settings.CollectionName);
await writer.GenerateRecords(settings.NumberToGenerate, descriptor.ToList());