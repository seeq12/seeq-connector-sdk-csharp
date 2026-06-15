@echo off
setlocal

if defined SEEQ_CONNECTOR_SDK_HOME goto :InDevEnvironment

echo.
echo You're not in the Connector SDK Dev Environment.
echo Execute 'environment' first.
echo.
exit /b 1
goto :EOF

:InDevEnvironment

dotnet restore "%~dp0Seeq.Connector.SDK.sln"
if ERRORLEVEL 1 goto :Error

dotnet build "%~dp0Seeq.Connector.SDK.sln" --configuration Release --no-restore

if ERRORLEVEL 1 goto :Error

goto :EOF

:Error
exit /b 1
