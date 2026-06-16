@echo off
setlocal

if not defined SEEQ_CONNECTOR_SDK_HOME goto :NotInDevEnvironment
if not defined SEEQ_CONNECTOR_NAME goto :NotInDevEnvironment
goto :InDevEnvironment

:NotInDevEnvironment
echo.
echo You're not in the Connector SDK Dev Environment.
echo Execute 'environment' first.
echo.
exit /b 1

:InDevEnvironment

dotnet build "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%\%SEEQ_CONNECTOR_NAME%.csproj" --configuration Release
if ERRORLEVEL 1 goto :Error

PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0package.ps1"
if ERRORLEVEL 1 goto :Error

goto :EOF

:Error
exit /b 1
