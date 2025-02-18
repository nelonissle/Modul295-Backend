while ($true) {
    Write-Host "Running Unit Tests..."
    dotnet test --logger "console;verbosity=detailed"
    Start-Sleep -Seconds 1800  # Wait 30 minutes (1800 seconds)
}
