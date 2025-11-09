@echo off
cd /d "%~dp0"
setlocal

REM === CONFIGURATION ===
set WAIT_INTERVAL=2

echo Starting .NET app...
start "DotNetApp" cmd /c "dotnet run --project "src\NewJoinerFeedbackWizard.HttpApi.Host""

echo Waiting for the app to start on port 44334...

:WAIT_LOOP
REM Check if the port is listening
netstat -an | find ":44334" | find "LISTENING" >nul
if %errorlevel%==0 (
    echo App is running on port 44334!
    goto AFTER_START
)

REM If not found yet, wait then retry
timeout /t %WAIT_INTERVAL% /nobreak >nul
goto WAIT_LOOP

:AFTER_START
echo Running the Blazor Project
start "DotNetBlazorApp" cmd /k "dotnet run --project "src\NewJoinerFeedbackWizard.Blazor""

echo Waiting for the app to start on port 44341...

:WAIT_LOOP_SECOND
REM Check if the port is listening
netstat -an | find ":44341" | find "LISTENING" >nul
if %errorlevel%==0 (
    echo App is running on port 44341!
    goto END
)

REM If not found yet, wait then retry
timeout /t %WAIT_INTERVAL% /nobreak >nul
goto WAIT_LOOP_SECOND

:END
echo Done!
endlocal
