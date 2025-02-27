param(
    [Parameter(Mandatory = $true, HelpMessage = "The path containing the .json files to process.")]
    [string]$JsonFilesPath,

    [Parameter(Mandatory = $true, HelpMessage = "The output path of the generated CSV files.")]
    [string]$OutputFilePath
)

$jsonFiles = Get-ChildItem -Path $JsonFilesPath -Filter *.json
$allData = @()

foreach ($file in $jsonFiles) {
    $jsonContent = Get-Content -Path $file.FullName -Raw
    $data = $jsonContent | ConvertFrom-Json
    $allData += $data
}

$environments = @('dev', 'qa', 'uat', 'npd', 'prod')

# Generate one CSV file per environment
foreach ($environment in $environments) {
    $deploymentsForEnvironment = $allData | Where-Object { $_.environment -eq $environment } | Sort-Object -Property name
    $rowsForEnvionrment = @()

    foreach ($deployment in $deploymentsForEnvironment) {
        $row = New-Object -TypeName PSObject
        $row | Add-Member -MemberType NoteProperty -Name "name" -Value $deployment.name
        $row | Add-Member -MemberType NoteProperty -Name "image" -Value $deployment.image
        $rowsForEnvionrment += $row
    }

    $rowsForEnvionrment | Export-Csv -Path (Join-Path -Path $OutputFilePath -ChildPath "$environment.csv")
}

# Generate a CSV file with all the environments together
$columnNames = @('name')
$columnNames += $environments | ForEach-Object { $_ + "-image" }
$columnNames += ($environments | ForEach-Object { $_ + "-replicas" })

$distinctNames = $allData | Select-Object -ExpandProperty name -Unique | Sort-Object
$allRows = @()

foreach ($name in $distinctNames) {
    $deployments = $allData | Where-Object { $_.name -eq $name }
    $row = New-Object -TypeName PSObject
    $row | Add-Member -MemberType NoteProperty -Name "name" -Value $name

    foreach ($environment in $environments) {
        $deployment = $deployments | Where-Object { $_.environment -eq $environment }
        $row | Add-Member -MemberType NoteProperty -Name "$environment-image" -Value $deployment.image
        $row | Add-Member -MemberType NoteProperty -Name "$environment-replicas" -Value $deployment.replicas
    }

    $allRows += $row | Select-Object $columnNames
}

$allRows | Export-Csv -Path (Join-Path -Path $OutputFilePath -ChildPath "all-environments.csv")