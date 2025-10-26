@echo off

title MediaFlyout Uninstaller (All Users)

cd /d %~dp0%
call lib.cmd %2

echo Uninstalling MediaFlyout for all users...

if not "%1"=="admin" (powershell start -verb runas '%0' 'admin %*' & exit /b)

if not exist "%GLOBAL_INSTALLATION_PATH%" (
    echo MediaFlyout is not installed for all users
    goto :end
)

reg delete HKLM\Software\Microsoft\Windows\CurrentVersion\Run /v "%REG_AUTORUN_KEY%" /f %OUTPUT%

call Stop.cmd
timeout 3 %OUTPUT%
rmdir /S /Q "%GLOBAL_INSTALLATION_PATH%"

echo MediaFlyout sucessfully uninstalled for all users

:end
pause
exit