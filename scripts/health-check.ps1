$ErrorActionPreference = "Stop"

$containers = @(
    "api-gateway",
    "order-service",
    "payment-service",
    "notification-service",
    "rabbitmq",
    "sqlserver",
    "seq"
)

$apiChecks = @(
    @{ Name = "notification-service"; Port = 5030 },
    @{ Name = "order-service"; Port = 5211 },
    @{ Name = "payment-service"; Port = 5104 },
    @{ Name = "api-gateway"; Port = 5000 }
)

$anyFail = $false

Write-Host "=== Container health checks ==="
foreach ($container in $containers) {
    try {
        $status = docker inspect --format "{{if .State.Health}}{{.State.Health.Status}}{{else}}{{.State.Status}}{{end}}" $container 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($status)) {
            throw "not found"
        }

        if ($status -eq "healthy" -or $status -eq "running") {
            Write-Host "PASS  $container ($status)"
        }
        else {
            Write-Host "FAIL  $container ($status)"
            $anyFail = $true
        }
    }
    catch {
        Write-Host "FAIL  $container (not found)"
        $anyFail = $true
    }
}

Write-Host ""
Write-Host "=== API health endpoints ==="
foreach ($check in $apiChecks) {
    $url = "http://localhost:$($check.Port)/health"
    try {
        $response = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "PASS  $($check.Name) $url ($($response.StatusCode))"
        }
        else {
            Write-Host "FAIL  $($check.Name) $url ($($response.StatusCode))"
            $anyFail = $true
        }
    }
    catch {
        Write-Host "FAIL  $($check.Name) $url (error)"
        $anyFail = $true
    }
}

Write-Host ""
if (-not $anyFail) {
    Write-Host "All checks passed."
    exit 0
}

Write-Host "One or more checks failed."
exit 1
