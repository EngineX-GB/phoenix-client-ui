# powershell -ExecutionPolicy bypass "./install-dev.ps1"

$PHOENIX_HOME_DIR="$env:USERPROFILE\phoenix"
$PHOENIX_PROD_DIR="$PHOENIX_HOME_DIR\phoenix-client-ui"
$CURRENT_DEV_DIR="$PHOENIX_HOME_DIR\DEV"

if (-not(Test-Path -Path $PHOENIX_PROD_DIR)) {
    Write-Host("[ERROR] Phoenix Production Application [" + $PHOENIX_PROD_DIR + "] does not exist. Install the prod version first. Exiting.")
    exit 1
}


# Delete the old dev instance
if (Test-Path -Path $CURRENT_DEV_DIR) {
    Remove-Item -Path $CURRENT_DEV_DIR -Recurse -Force
}

# copy the prod version into dev directory

Copy-Item -Path "$PHOENIX_HOME_DIR\phoenix-client-ui" -Destination "$CURRENT_DEV_DIR" -Recurse

# go through each service repo to get the dev script runner

Invoke-WebRequest -Uri "https://raw.githubusercontent.com/EngineX-GB/phoenix-client-service/refs/heads/master/scripts/dev-phoenix-client-service.bat" -OutFile "$CURRENT_DEV_DIR\net8.0-windows\services\dev-phoenix-client-service.bat"
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/EngineX-GB/phoenix-ingestion-service/refs/heads/master/scripts/dev-phoenix-ingestion-service.bat" -OutFile "$CURRENT_DEV_DIR\net8.0-windows\services\dev-phoenix-ingestion-service.bat"
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/EngineX-GB/phoenix-market-connector/refs/heads/master/scripts/dev-market-connector-service.bat" -OutFile "$CURRENT_DEV_DIR\net8.0-windows\services\dev-market-connector-service.bat"


# download a copy of the dev-services script to orchestrate the running of the 3 services in standalone mode

Invoke-WebRequest -Uri "https://raw.githubusercontent.com/EngineX-GB/phoenix-client-ui/refs/heads/master/dev-services.bat" -OutFile "$CURRENT_DEV_DIR\net8.0-windows\services.bat"

# Now update the appsettings.json file to use

$APP_SETTINGS_FILE = "$CURRENT_DEV_DIR\net8.0-windows\appsettings.json"

$APP_SETTINGS = Get-Content $APP_SETTINGS_FILE -Raw | ConvertFrom-Json
$APP_SETTINGS."environment" = "DEV"
$APP_SETTINGS."clientServiceEndpoint" = "http://localhost:9081"
$APP_SETTINGS."clientServiceWsEndpoint" = "ws://localhost:9081"
$APP_SETTINGS | ConvertTo-Json -Depth 10 | Set-Content $APP_SETTINGS_FILE


Write-Host "[INFO] Dev installation complete."