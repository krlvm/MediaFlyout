if not "%1" == "nogreeting" (
    echo MediaFlyout v1.0
    echo Media Control Flyout for Windows 10 Taskbar
    echo https://github.com/krlvm/MediaFlyout
    echo Licensed under the MIT License
    echo.
)

SET EXECUTABLE_NAME=MediaFlyout.exe
SET DIST_PATH=dist
SET LOCAL_INSTALLATION_PATH=%LocalAppData%\MediaFlyout
SET GLOBAL_INSTALLATION_PATH=%ProgramFiles%\MediaFlyout
SET REG_AUTORUN_KEY=Media Flyout

SET DEBUG_INSTALLER=""
if not DEBUG_INSTALLER == "" SET OUTPUT=^> nul