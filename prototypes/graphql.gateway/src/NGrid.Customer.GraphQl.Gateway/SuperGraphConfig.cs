namespace NGrid.Customer.GraphQl.Gateway;

public class SuperGraphConfig
{
    public readonly IDictionary<string, string> SchemaToEndpointMap = new Dictionary<string, string>
    {
        { "user", "http://user-api:8080/user-api/graphql" },
        { "accountlink", "http://account-link-api:8080/account-link-api/graphql" },
        { "billingaccount", "http://billing-account-api:8080/billing-account-api/graphql" }
    };

    public string GetBaseUrl(string schemaName)
    {
        if (!SchemaToEndpointMap.TryGetValue(schemaName, out var endPoint))
        {
            throw new ApplicationException($"Endpoint for service {schemaName} is not defined in the super graph configuration");
        }

        return endPoint;
    }
}