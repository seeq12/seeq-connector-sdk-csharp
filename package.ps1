$DIST_DIR = "dist"
$DIST_FILE_NAME = "$env:SEEQ_CONNECTOR_NAME.zip"
$TEMP_CONNECTOR_DLL_DIR = Join-Path $DIST_DIR "$env:SEEQ_CONNECTOR_NAME"

$PROJECT_DIR = "$env:SEEQ_CONNECTOR_SDK_HOME\$env:SEEQ_CONNECTOR_NAME"
$RELEASE_DIR_ANYCPU = "$PROJECT_DIR\bin\Release"
$RELEASE_DIR_X86 = "$PROJECT_DIR\bin\x86\Release"
$RELEASE_DIR_X64 = "$PROJECT_DIR\bin\x64\Release"

Write-Output "Packaging '$env:SEEQ_CONNECTOR_NAME'..."

Write-Output "Cleaning publish directory '$DIST_DIR'..."

if (Test-Path $DIST_DIR) {
    Remove-Item -Force -Recurse $DIST_DIR
}

New-Item -ItemType Directory -Path $DIST_DIR | Out-Null
New-Item -ItemType Directory -Path $TEMP_CONNECTOR_DLL_DIR | Out-Null

$RELEASE_DIR = $RELEASE_DIR_ANYCPU
if (Test-Path $RELEASE_DIR_X86) {
    $RELEASE_DIR = $RELEASE_DIR_X86
}

if (Test-Path $RELEASE_DIR_X64) {
    $RELEASE_DIR = $RELEASE_DIR_X64
}

Write-Output "Copying '$RELEASE_DIR' to '$TEMP_CONNECTOR_DLL_DIR'..."
Copy-Item -Path "$RELEASE_DIR\*" -Destination $TEMP_CONNECTOR_DLL_DIR -Recurse | Out-Null

Write-Output "Compressing '$env:SEEQ_CONNECTOR_NAME'..."
Compress-Archive -Path "$TEMP_CONNECTOR_DLL_DIR\*" -DestinationPath "$DIST_DIR\$DIST_FILE_NAME"

Write-Output "Cleaning up temporary files"
Remove-Item -Force -Recurse $TEMP_CONNECTOR_DLL_DIR

Write-Output "Connector package created: $DIST_DIR\$DIST_FILE_NAME"
