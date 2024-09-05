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

.\nuget restore Seeq.Connector.SDK.sln

set "VSWHERE_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if exist "%VSWHERE_PATH%" (
    for /f "tokens=*" %%i in ('"%VSWHERE_PATH%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe') do (
        set "MSBUILD_PATH=%%i"
    )
) else (
    set "MSBUILD_PATH=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)

"%MSBUILD_PATH%" "%~dp0.\Seeq.Connector.SDK.sln" /p:Configuration="Release"

if ERRORLEVEL 1 goto :Error

goto :EOF

:Error
exit /b 1
