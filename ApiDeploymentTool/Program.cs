using System.CommandLine;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;

var client = new ArmClient(new DefaultAzureCredential());

var subscriptionOption = new Option<string>("--subscriptionId", () => "8040b17d-0827-4b09-86cb-4f48a197f68f");
var resourceGroupOption = new Option<string>("--resourceGroup", () => "grd-dev-eus2-gapi-rg");
var apimServiceOption = new Option<string>("--apimServiceName", () => "grd-dev-eus2-gapi-apim-01");
var apiNameOption = new Option<string>("--apiName");
var schemaFileOption = new Option<string>("--schemaFile");
var rootCommand = new RootCommand("API Deployment Tool") { subscriptionOption, resourceGroupOption, apimServiceOption, apiNameOption, schemaFileOption };

rootCommand.SetHandler((subscriptionId, resourceGroup, apimServiceName, apiName, schemaFile) =>
{
    var resourceId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.ApiManagement/service/{apimServiceName}/apis/{apiName}/schemas/graphql");
    var schemaResource = client.GetApiSchemaResource(resourceId);
    var schemaValue = File.ReadAllText(schemaFile);
    var schemaData = new ApiSchemaData
    {
        ContentType = "application/vnd.ms-azure-apim.graphql.schema",
        Value = schemaValue
    };
    schemaResource.Update(WaitUntil.Completed, schemaData);
}, subscriptionOption, resourceGroupOption, apimServiceOption, apiNameOption, schemaFileOption);

return await rootCommand.InvokeAsync(args);