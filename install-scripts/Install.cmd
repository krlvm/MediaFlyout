@echo off

SET TITLE_IL=MediaFlyout Installer
title %TITLE_IL%

call lib.cmd

echo Installing MediaFlyout...

if exist "%LOCAL_INSTALLATION_PATH%" (
    echo Uninstalling previous installation...
    echo.
    echo | call Uninstall.cmd nogreeting
    echo.
    echo Previous version has been uninstalled
    title %TITLE_IL%
)

xcopy "%DIST_PATH%" "%LOCAL_INSTALLATION_PATH%"\ /sy %OUTPUT%
reg add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v "%REG_AUTORUN_KEY%" /d "%LOCAL_INSTALLATION_PATH%\%EXECUTABLE_NAME%" /f %OUTPUT%

cd /d "%LOCAL_INSTALLATION_PATH%"
start %EXECUTABLE_NAME%

echo MediaFlyout sucessfully installed

pause
exit