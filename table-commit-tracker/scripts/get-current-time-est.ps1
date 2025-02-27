$currentTimeUtc = Get-Date -AsUTC

$estZone = [TimeZoneInfo]::FindSystemTimeZoneById("Eastern Standard Time")

$currentTimeEst = [TimeZoneInfo]::ConvertTimeFromUtc($currentTimeUtc, $estZone)

$currentTimeEst