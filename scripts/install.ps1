# powershell -ExecutionPolicy bypass "./install.ps1"

$RELEASE_URL="https://api.github.com/repos/EngineX-GB/phoenix-client-ui/releases/latest"
$PHOENIX_HOME_DIR="$env:USERPROFILE\phoenix"
$TEMP_DIR = "$PHOENIX_HOME_DIR\temp"

if (-not(Test-Path -Path $PHOENIX_HOME_DIR)) {
    New-Item -Path $PHOENIX_HOME_DIR -ItemType Directory -Force | Out-Null
}

if (-not(Test-Path -Path $TEMP_DIR)) {
    New-Item -Path $TEMP_DIR -ItemType Directory -Force | Out-Null
}



# get the tag name of the latest version
function GetLatestTagVersion {
    param([string]$OWNER, [string]$REPO_NAME)
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/repos/$OWNER/$REPO_NAME/releases/latest" -ErrorAction Stop

        if ([string]::IsNullOrWhiteSpace($response.tag_name)) {
            Write-Warning "No version found for $REPO_NAME"
            return $null
        }
        return [string]$response.tag_name
    }
    catch {
        Write-Warning "Failed to get version for $REPO_NAME : $($_.Exception.Message)"
        exit 1
    }
}

$GITHUB_REPO_OWNER="EngineX-GB"
$CLIENT_UI_LATEST_VERSION = GetLatestTagVersion -OWNER $GITHUB_REPO_OWNER -REPO_NAME "phoenix-client-ui"

# donwload and unzip the contents to a folder
Write-Host "[INFO] Downloading Latest version of Phoenix Client : $CLIENT_UI_LATEST_VERSION"

$RELEASE_FILE_URL="https://github.com/EngineX-GB/phoenix-client-ui/releases/download/${CLIENT_UI_LATEST_VERSION}/phoenix-client-ui.zip"

Invoke-WebRequest -Uri $RELEASE_FILE_URL -OutFile "$TEMP_DIR\phoenix-client-ui.zip"

Expand-Archive -Path "$TEMP_DIR\phoenix-client-ui.zip" -DestinationPath "$PHOENIX_HOME_DIR\phoenix-client-ui" -Force

# read the version.json file and get the version numbers for the associated service components.

if (-not(Test-Path -Path "$PHOENIX_HOME_DIR\phoenix-client-ui")) {
    Write-Host "[ERROR] $PHOENIX_HOME_DIR\phoenix-client-ui does not exist. Exiting installation."
    exit 1
}



$VERSION_FILE_PATH = "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\version.json"
$VERSIONS = Get-Content $VERSION_FILE_PATH -Raw | ConvertFrom-Json

$CONNECTOR_VERSION=$VERSIONS."connector.version"
$INGESTION_VERSION=$VERSIONS."ingestion.version"
$CLIENT_SVC_VERSION=$VERSIONS."client-svc.version"

Write-Host "[INFO] Connector verion :  $CONNECTOR_VERSION"
Write-Host "[INFO] Ingestion verion :  $INGESTION_VERSION"
Write-Host "[INFO] Client Service verion :  $CLIENT_SVC_VERSION"


# download the version of each component into the temp folder
$CONNECTOR_RELEASE_FILE_URL="https://github.com/EngineX-GB/phoenix-market-connector/releases/download/${CONNECTOR_VERSION}/phoenix-market-connector-windows_${CONNECTOR_VERSION}.zip"
$INGESTION_RELEASE_FILE_URL="https://github.com/EngineX-GB/phoenix-ingestion-service/releases/download/${INGESTION_VERSION}/data-ingestion.zip"
$CLIENT_SVC_RELEASE_FILE_URL="https://github.com/EngineX-GB/phoenix-client-service/releases/download/${CLIENT_SVC_VERSION}/phoenix-client-service.zip"


Write-Host "[INFO] Downloading Assets...[connector]"
Invoke-WebRequest -Uri $CONNECTOR_RELEASE_FILE_URL -OutFile "$TEMP_DIR\phoenix-market-connector.zip"
Write-Host "[INFO] Downloading Assets...[ingestion]"
Invoke-WebRequest -Uri $INGESTION_RELEASE_FILE_URL -OutFile "$TEMP_DIR\data-ingestion.zip"
Write-Host "[INFO] Downloading Assets...[client-svc]"
Invoke-WebRequest -Uri $CLIENT_SVC_RELEASE_FILE_URL -OutFile "$TEMP_DIR\phoenix-client-service.zip"

# unzip all the files in the temp/services folder
Write-Host "[INFO] Unzip to $TEMP_DIR"
Expand-Archive -Path "$TEMP_DIR\phoenix-market-connector.zip" -DestinationPath "$TEMP_DIR\services" -Force
Expand-Archive -Path "$TEMP_DIR\data-ingestion.zip" -DestinationPath "$TEMP_DIR\services" -Force
Expand-Archive -Path "$TEMP_DIR\phoenix-client-service.zip" -DestinationPath "$TEMP_DIR\services" -Force

# Now copy the required binaries in the temp/services folder into the installation folder

if (-not(Test-Path -Path "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services")) {
    New-Item -Path "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services" -ItemType Directory -Force | Out-Null
}

Write-Host "[INFO] Copy binaries to $PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services"
Copy-Item -Path "$TEMP_DIR\services\market-connector-service.exe" -Destination "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services\market-connector-service.exe" -Force
Copy-Item -Path "$TEMP_DIR\services\dist\DataIngestionRunner.exe" -Destination "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services\DataIngestionRunner.exe" -Force
Copy-Item -Path "$TEMP_DIR\services\release\phoenix-client-service.exe" -Destination "$PHOENIX_HOME_DIR\phoenix-client-ui\net8.0-windows\services\phoenix-client-service.exe" -Force

# Delete temp folder. Note that this can be commented out for investigation purposes.
Write-Host "[INFO] Delete temporary files..."
Remove-Item -Path "$TEMP_DIR" -Recurse -Force

Write-Host "[INFO] Installation Complete."