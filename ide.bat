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

set "SOLUTION_FILE=%~dp0Seeq.Connector.SDK.sln"
set "VSWHERE_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
set "DEVENV_PATH="

if exist "%VSWHERE_PATH%" (
    for /f "tokens=*" %%i in ('"%VSWHERE_PATH%" -products * -version [18.0^,19.0^) -requires Microsoft.Component.MSBuild -property productPath') do (
        if not defined DEVENV_PATH set "DEVENV_PATH=%%i"
    )

    if not defined DEVENV_PATH (
        for /f "tokens=*" %%i in ('"%VSWHERE_PATH%" -latest -products * -requires Microsoft.Component.MSBuild -property productPath') do (
            if not defined DEVENV_PATH set "DEVENV_PATH=%%i"
        )
    )
)

if defined DEVENV_PATH (
    start "" "%DEVENV_PATH%" "%SOLUTION_FILE%"
) else (
    start "" "%SOLUTION_FILE%"
)
