@echo off
call lib.cmd nogreeting
echo Stopping MediaFlyout...
taskkill /f /im %EXECUTABLE_NAME% %OUTPUT%
echo MediaFlyout stopped