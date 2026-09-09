@echo off
REM Quick run script - runs the latest built EXE with admin privileges

setlocal enabledelayedexpansion

REM Check if EXE exists
if not exist "bin\Release\publish\WindowsDataRecovery.exe" (
    echo.
    echo ERROR: WindowsDataRecovery.exe not found!
    echo Please run build-exe.bat first to create the executable.
    echo.
    pause
    exit /b 1
)

REM Launch with admin privileges
PowerShell -Command "Start-Process 'bin\Release\publish\WindowsDataRecovery.exe' -Verb RunAs"

if errorlevel 1 (
    echo.
    echo ERROR: Failed to launch application!
    echo Please ensure you have permission to run this application.
    pause
    exit /b 1
)
