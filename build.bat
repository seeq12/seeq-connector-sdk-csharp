@echo off

if defined SEEQ_CONNECTOR_SDK_HOME goto :InDevEnvironment

echo.
echo You're not in the Connector SDK Dev Environment.
echo Execute 'environment' first.
echo.
exit /b 1
goto :EOF

:InDevEnvironment

if not exist ".\nuget.exe" powershell -Command "(new-object System.Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe', '.\nuget.exe')"

.\nuget install Seeq.Link.SDK.Debugging.Agent\packages.config -o packages
.\nuget install MyCompany.Seeq.Link.Connector.MyConnector\packages.config -o packages

dotnet build -c Release "%~dp0.\Seeq.Connector.SDK.sln"
if ERRORLEVEL 1 goto :Error

goto :EOF

:Error
exit /b 1
