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

rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%\bin" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%\obj" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\Seeq.Link.SDK.Debugging.Agent\bin" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\Seeq.Link.SDK.Debugging.Agent\obj" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%.Test\bin" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%.Test\obj" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\dist" >nul 2>&1
rmdir /Q /S "%SEEQ_CONNECTOR_SDK_HOME%\tools" >nul 2>&1
del /Q "%SEEQ_CONNECTOR_SDK_HOME%\nuget.exe" >nul 2>&1

echo Clean completed.
