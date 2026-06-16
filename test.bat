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

dotnet test "%~dp0Seeq.Connector.SDK.sln" --configuration Debug

if ERRORLEVEL 1 goto :Error

echo Tests completed.

goto :EOF

:Error
exit /b 1