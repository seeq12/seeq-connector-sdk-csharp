@echo off

set "SEEQ_NUGET_PATH=nuget.exe"

if not exist "%SEEQ_NUGET_PATH%" (
    echo You do not appear to have built the connector projects.
    echo Execute 'build' first.
    exit /b 1
)

set "SEEQ_NUNIT_RUNNER=%~dp0.\tools\NUnit.ConsoleRunner.3.16.3\tools\nunit3-console.exe"

if not exist "%SEEQ_NUNIT_RUNNER%" .\nuget install NUnit.Console -Version 3.16.3 -o tools 

for /r "%~dp0." %%f in (bin\Debug\*Test.dll bin\Release\*Test.dll) do (
    echo Found test project: %%f
    "%SEEQ_NUNIT_RUNNER%" %%f --noresult
)

echo Tests completed.