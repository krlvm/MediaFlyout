@echo off

title MediaFlyout Uninstaller

cd /d %~dp0%
call lib.cmd %1

echo Uninstalling MediaFlyout...

if not exist "%LOCAL_INSTALLATION_PATH%" (
    echo MediaFlyout is not installed for current user
    goto :end
)

reg delete HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v "%REG_AUTORUN_KEY%" /f %OUTPUT%

call Stop.cmd
timeout 3 %OUTPUT%
rmdir /S /Q "%LOCAL_INSTALLATION_PATH%"

echo MediaFlyout sucessfully uninstalled

:end
pause
exit