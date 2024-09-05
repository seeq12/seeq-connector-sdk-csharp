@echo off

set SEEQ_CONNECTOR_SDK_HOME=%~dp0.

for /d %%i in ("%SEEQ_CONNECTOR_SDK_HOME%\*Seeq.Link.Connector*") do (
    echo %%~nxi | findstr /r /v "Test$" >nul
    if not errorlevel 1 (
        set "SEEQ_CONNECTOR_NAME=%%~nxi"
        goto :found
    )
)

:found
if not defined SEEQ_CONNECTOR_NAME (
    echo Failed to find your connector name. Exiting...
    exit 1
)

title Seeq Connector SDK C# Dev Environment

echo.
echo Seeq Connector SDK C# development environment is set up.
echo.
echo Your connector name is currently "%SEEQ_CONNECTOR_NAME%"
echo.
echo First, execute "build" to compile the connector.
echo Then execute "ide" to launch Visual Studio for development and debugging.
echo You could run "test" to execute tests you have included.
echo Finally, execute "package" to build a deployable connector package.
echo.
