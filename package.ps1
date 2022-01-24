function ZipFiles( [string] $zipfilename, [string] $sourcedir )
{
   Add-Type -Assembly System.IO.Compression.FileSystem
   $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
   [System.IO.Compression.ZipFile]::CreateFromDirectory($sourcedir,
        $zipfilename, $compressionLevel, $false)
}

$packagesdir = "$env:SEEQ_CONNECTOR_SDK_HOME\packages"
$projectfolder = "$env:SEEQ_CONNECTOR_SDK_HOME\$env:SEEQ_CONNECTOR_NAME"
$releasefolder_anycpu = "$projectfolder\bin\Release"
$releasefolder_x86 = "$projectfolder\bin\x86\Release"
$releasefolder_x64 = "$projectfolder\bin\x64\Release"
$nicenamecontainer = "$projectfolder\pkg"
$nicenamefolder = "$nicenamecontainer\$env:SEEQ_CONNECTOR_NAME"
$packagename = "$packagesdir\$env:SEEQ_CONNECTOR_NAME.zip"

$releasefolder = $releasefolder_anycpu
if (Test-Path $releasefolder_x86) {
    $releasefolder = $releasefolder_x86
}

if (Test-Path $releasefolder_x64) {
    $releasefolder = $releasefolder_x64
}

if (Test-Path $packagesdir) {
	Remove-Item -Force -Recurse $packagesdir
}

if (Test-Path $nicenamecontainer) {
	Remove-Item -Force -Recurse $nicenamecontainer
}

New-Item -ItemType Directory -Force -Path $packagesdir | Out-Null
New-Item -ItemType Directory -Force -Path $nicenamefolder | Out-Null

Copy-Item -Path "$releasefolder\*" -Destination $nicenamefolder -Recurse | Out-Null

ZipFiles $packagename $nicenamecontainer

Remove-Item -Force -Recurse $nicenamecontainer

Write-Output ""
Write-Output "Connector package '$packagename' created."
