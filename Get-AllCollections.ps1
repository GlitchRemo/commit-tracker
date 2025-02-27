[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $AccountName,
    [Parameter()]
    [string]
    $ResourceGroup,
    [Parameter()]
    [string]
    $SubscriptionId
)

$databases = az cosmosdb mongodb database list --account-name $AccountName --resource-group $ResourceGroup | ConvertFrom-Json | select { $_.name } -ExpandProperty name
foreach ($db in $databases)
{
    $collections = az cosmosdb mongodb collection list --account-name $AccountName --resource-group $ResourceGroup --database-name $db | ConvertFrom-Json | select { $_.name } -ExpandProperty name
    foreach($coll in $collections)
    {
        Write-Output "$db,$coll"
    }
}