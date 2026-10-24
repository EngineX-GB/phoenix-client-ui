# To run this script:
# powershell.exe -ExecutionPolicy Bypass -File "release.ps1"
#
# This script must only be run on the updated master branch.
#
# Note: For usage:
# 1. Ensure the github cli directory exists
# 2. Ensure git directory exists
# 3. Ensure python directory exists
# 4. Reference the GH token in an external file and reference it in this pipeline.
#
# Pipeline process:
# 1. Build the project to a binary to ensure that the project builds successfully
# 2. Update the version.json file to increment the patch version number (the third number in n.n.n)
# 3. Tag the master branch with the new version
# 4. Commit the updated version file and push it into the master branch
# 5. Build the project again (containing the new updated version.json file embedded in the binary)
# 6. Push the artefact as a release against the new version.


function GetGHToken{
    param([string]$TOKEN_FILE_PATH)
    if (-not(Test-Path $TOKEN_FILE_PATH -PathType Leaf)) {
        Write-Host("[ERROR] Cannot find $TOKEN_FILE_PATH. This is needed to get the Github Token")
        exit 1    
    }
    $TOKEN_KEY_VALUE_STRING = Get-Content $TOKEN_FILE_PATH
    $KEY_VALUE_PAIR = $TOKEN_KEY_VALUE_STRING.Split('=')
    return $KEY_VALUE_PAIR[1]
}

function GetLatestTagVersion {
    param([string]$OWNER, [string]$REPO_NAME)
    Write-Host "Repository name : $REPO_NAME"
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

# Tools setup
$TOKEN_FILE_PATH="$env:USERPROFILE\Documents\Development\code\deployment\scm-config.properties"
$GH_TOKEN = GetGHToken($TOKEN_FILE_PATH)
$env:GH_TOKEN=$GH_TOKEN

# Tools Setup
$DOTNET_HOME = "C:\Program Files\dotnet"
$GIT_HOME = "C:\Program Files\Git"
$GIT_BIN = "$GIT_HOME\bin\git.exe"
$GITHUB_CLI_HOME = "C:\Program Files\GitHub CLI"
$GITHUB_CLI_BIN = "$GITHUB_CLI_HOME\gh.exe"
$GITHUB_REPO_OWNER = "EngineX-GB"

# Project Setup
$PROJECT_NAME = "phoenix-client-ui"
$PROJECT_HOME_DIRECTORY = "$env:USERPROFILE\source\repos\$PROJECT_NAME"
$SOLUTION_FILE = $PROJECT_NAME.sln
$RELEASE_FOLDER="$PROJECT_HOME_DIRECTORY/$PROJECT_NAME/bin/release/net8.0-windows"
$TEMP_FOLDER="$PROJECT_HOME_DIRECTORY/temp"

if (-not(Test-Path $DOTNET_HOME -PathType Container)) {
    Write-Host "[ERROR] $DOTNET_HOME does not exist"
    return
}

if (-not(Test-Path $PROJECT_HOME_DIRECTORY -PathType Container)) {
    Write-Host "[ERROR] $PROJECT_HOME_DIRECTORY does not exist"
    exit 1
}

if (-not(Test-Path $GIT_HOME -PathType Container)) {
    Write-Host "[ERROR] $GIT_HOME does not exist"
    exit 1
}

if (-not(Test-Path $GIT_BIN -PathType Leaf)) {
    Write-Host "[ERROR] $GIT_BIN does not exist"
    exit 1
}

if (-not(Test-Path $GITHUB_CLI_HOME -PathType Container)) {
    Write-Host "[ERROR] $GITHUB_CLI_HOME does not exist"
    exit 1
}

if (-not(Test-Path $GITHUB_CLI_BIN -PathType Leaf)) {
    Write-Host "[ERROR] $GITHUB_CLI_BIN does not exist"
    exit 1
}

cd $PROJECT_HOME_DIRECTORY
& $DOTNET_HOME\dotnet.exe build $SOLUTION_FILE

IF ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}


# Here, write read the version manifest file in the client directory.
# Then read the version number

$VERSIONS_FILE = Join-Path $PROJECT_HOME_DIRECTORY "version.json"
$VERSIONS = Get-Content $VERSIONS_FILE -Raw | ConvertFrom-Json
$CLIENT_UI_VERSION = $VERSIONS."client-ui.version"

Write-Host $CLIENT_UI_VERSION

# The increment the minor version

# Split the version into parts
$PARTS = $CLIENT_UI_VERSION.Split('.')

# Increment the last digit
$PARTS[2] = [int]$PARTS[2] + 1

# Reassemble the version
$NEW_CLIENT_UI_VERSION = $PARTS -join '.'

# Update the JSON property
$VERSIONS."client-ui.version" = $NEW_CLIENT_UI_VERSION

###
# For the Client UI release, we need to get the latest version of the associated internal services
# that power the Client UI.
#
###

Write-Host "[INFO] Getting versions of other required internal services"

$CONNECTOR_LATEST_VERSION = GetLatestTagVersion -OWNER $GITHUB_REPO_OWNER -REPO_NAME "phoenix-market-connector"
$INGESTION_LATEST_VERSION = GetLatestTagVersion -OWNER $GITHUB_REPO_OWNER  -REPO_NAME "phoenix-ingestion-service"
$CLIENT_SERVICE_LATEST_VERSION = GetLatestTagVersion  -OWNER $GITHUB_REPO_OWNER -REPO_NAME "phoenix-client-service"

Write-Host ""
Write-Host "[INFO] CONNECTOR -> $CONNECTOR_LATEST_VERSION"
Write-Host "INFO] INGESTION -> $INGESTION_LATEST_VERSION"
Write-Host "INFO] CLIENT_SERVICE -> $CLIENT_SERVICE_LATEST_VERSION"
Write-Host ""

# Update the version manifest in the JSON with these new version numbers.

$VERSIONS."connector.version" = $CONNECTOR_LATEST_VERSION
$VERSIONS."ingestion.version" = $INGESTION_LATEST_VERSION
$VERSIONS."client-svc.version" = $CLIENT_SERVICE_LATEST_VERSION

# Save the JSON back to the file
$VERSIONS | ConvertTo-Json -Depth 10 | Set-Content $VERSIONS_FILE

Write-Host "[INFO] Version updated from $CLIENT_UI_VERSION to $NEW_CLIENT_UI_VERSION"

# Take the new ui version and tag the git master branch with this new version

Write-Host "[INFO] Creating tag on branch ($NEW_CLIENT_UI_VERSION)"
& $GIT_BIN tag -a $NEW_CLIENT_UI_VERSION -m "Version $NEW_CLIENT_UI_VERSION"

IF ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] An error occurred when trying to push to github.com: $LASTEXITCODE"
    exit $LASTEXITCODE
}

& $GIT_BIN push origin $NEW_CLIENT_UI_VERSION

IF ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] An error occurred when trying to push to github.com: $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Here, we need to commit the file version.json that has the up to date change log

git add version.json
Write-Host "[INFO] Committing updated version.json to repository"
& $GIT_BIN commit -m "Updating version.json change log with the new version ($NEW_CLIENT_UI_VERSION)."
& $GIT_BIN push origin master

IF ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] An error occurred when trying to push to github.com: $LASTEXITCODE"
    exit $LASTEXITCODE
}


# now do an optimised release binary (exe) of the client.
& $DOTNET_HOME\dotnet.exe build $SOLUTION_FILE --configuration Release

IF ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Optimised Release Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Copy the updated version.json file into the release folder before it gets packaged up.
Copy-Item -Path $PROJECT_HOME_DIRECTORY\version.json -Destination $RELEASE_FOLDER

# create the temp folder
New-Item -Path $TEMP_FOLDER -ItemType Directory -Force | Out-Null

# The zip the archive (below)

IF (Test-Path $RELEASE_FOLDER -PathType Container) {
    # if the release folder exists, then zip it up
    Compress-Archive -Path $RELEASE_FOLDER -DestinationPath $TEMP_FOLDER/$PROJECT_NAME -CompressionLevel Optimal -Force
} else {
    Write Host "[ERROR] $RELEASE_FOLDER does not exist. Exiting."
    exit 1
}

# Publish as a release against the version using the Github CLI token

$RELEASE_TITLE="$PROJECT_NAME Release Title ($NEW_CLIENT_UI_VERSION)"
$RELEASE_NOTES="$PROJECT_NAME Release Notes ($NEW_CLIENT_UI_VERSION)"

Write-Host "[INFO] Publishing artefact..."
& $GITHUB_CLI_BIN release create $NEW_CLIENT_UI_VERSION $TEMP_FOLDER/$PROJECT_NAME.zip --notes $RELEASE_NOTES -t $RELEASE_TITLE
Write-Host ""
Write-Host "[INFO] Removing Temp Files"
Remove-Item "$RELEASE_FOLDER" -Recurse -Force
Remove-Item "$TEMP_FOLDER" -Recurse -Force
Write-Host ""
Write-Host "[INFO] Release Completed."