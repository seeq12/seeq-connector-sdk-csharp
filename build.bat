@echo off

if defined SEEQ_CONNECTOR_SDK_HOME goto :InDevEnvironment

echo.
echo You're not in the Connector SDK Dev Environment.
echo Execute 'environment' first.
echo.
exit /b 1
goto :EOF

:InDevEnvironment

"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "%~dp0.\Seeq.Connector.SDK.sln" /p:Configuration="Release"
if ERRORLEVEL 1 goto :Error

goto :EOF

:Error
exit /b 1
