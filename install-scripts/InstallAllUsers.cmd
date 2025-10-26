@echo off

SET TITLE_IG=MediaFlyout Installer (All Users)
title %TITLE_IG%

cd %~dp0%
call lib.cmd

echo Installing MediaFlyout for all users...

if not "%1"=="admin" (powershell start -verb runas '%0' admin & exit /b)

if exist "%LOCAL_INSTALLATION_PATH%" (
    echo Uninstalling local installation...
    echo.
    echo | call Uninstall.cmd nogreeting
    echo.
    echo Local installation has been removed
    title %TITLE_IG%
)
if exist "%GLOBAL_INSTALLATION_PATH%" (
    echo Uninstalling previous installation...
    echo.
    echo | call UninstallAllUsers.cmd admin nogreeting
    echo.
    echo Previous version has been uninstalled
    title %TITLE_IG%
)

xcopy "%DIST_PATH%" "%GLOBAL_INSTALLATION_PATH%"\ /sy %OUTPUT%
reg add HKLM\Software\Microsoft\Windows\CurrentVersion\Run /v "%REG_AUTORUN_KEY%" /d "%GLOBAL_INSTALLATION_PATH%\%EXECUTABLE_NAME%" /f %OUTPUT%

cd /d "%GLOBAL_INSTALLATION_PATH%"
start %EXECUTABLE_NAME%

echo MediaFlyout sucessfully installed for all users

pause
exit