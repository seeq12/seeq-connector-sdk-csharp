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

echo Cleaning build artifacts...
echo.

call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%\bin"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%\obj"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\Seeq.Link.SDK.Debugging.Agent\bin"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\Seeq.Link.SDK.Debugging.Agent\obj"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%.Test\bin"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\%SEEQ_CONNECTOR_NAME%.Test\obj"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\dist"
call :RemoveDirectory "%SEEQ_CONNECTOR_SDK_HOME%\tools"
call :RemoveFile "%SEEQ_CONNECTOR_SDK_HOME%\nuget.exe"

echo.
echo Clean completed.
goto :EOF

:RemoveDirectory
if exist "%~1\" (
    echo Deleting directory: %~1
    rmdir /Q /S "%~1"
) else (
    echo Skipping missing directory: %~1
)
exit /b

:RemoveFile
if exist "%~1" (
    echo Deleting file: %~1
    del /Q "%~1"
) else (
    echo Skipping missing file: %~1
)
exit /b
