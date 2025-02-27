param(
    [Parameter(Mandatory = $true, HelpMessage = "The path to the CSV file.")]
    [string]$Path
)

$table = Import-Csv -Path $Path

$header = $table[0].PSObject.Properties.Name
$markdownTable = "| " + ($header -join " | ") + " |"
$separator = "| " + (($header | ForEach-Object { "---" }) -join " | ") + " |"

$markdownTable += [Environment]::NewLine + $separator

foreach ($row in $table) {
    $rowData = $row.PSObject.Properties.Value
    $markdownTable += [Environment]::NewLine + ($rowData -join " | ") + " |"
}

$markdownTable