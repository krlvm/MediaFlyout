<div align="center">
<img src="https://raw.githubusercontent.com/krlvm/MediaFlyout/master/github-images/logo.png" height="150px" width="auto" />
<br><h1>MediaFlyout</h1>
Windows 10 Media Control Taskbar Flyout
<br><br>
<a href="https://github.com/krlvm/MediaFlyout/blob/master/LICENSE"><img src="https://img.shields.io/github/license/krlvm/MediaFlyout?style=flat-square" alt="License"/></a>
<a href="https://github.com/krlvm/MediaFlyout/releases/latest"><img src="https://img.shields.io/github/v/release/krlvm/MediaFlyout?style=flat-square" alt="Latest release"/></a>
<br>
<img src="https://raw.githubusercontent.com/krlvm/MediaFlyout/master/github-images/ui.png" alt="Flyout UI" style="width:60%" />
<br>
<br>
<img src="https://raw.githubusercontent.com/krlvm/MediaFlyout/master/github-images/demo.gif" alt="Flyout opening animation" style="width:60%" />
</div>

MediaFlyout adds a tray icon that allows you to control music and video playback.

It is a small C# WPF application that resembles a built-in Windows 10 taskbar flyout like Volume or Network control. It keeps the flyout window in memory for quick access, but consumes almost no system resources.

Tray icon is hidden when there's no media is playing.

### Features
* Double Click on tray icon to pause all media or play the last paused element
* Supports showing accent color on surface
* Fluent Design support, including acrylic transparency

<div align="center">
<img src="https://raw.githubusercontent.com/krlvm/MediaFlyout/master/github-images/ui_light.png" alt="Flyout UI (Light)" style="width:45%" />
&nbsp;
<img src="https://raw.githubusercontent.com/krlvm/MediaFlyout/master/github-images/ui_accent.png" alt="Flyout UI (Accent)" style="width:45%" />
</div>

### Installation
You can download the application on the [Releases](https://github.com/krlvm/MediaFlyout/releases) page.

After extracting the archive you will see a folder containing batch files for automatic installation and the directory containing app files (`dist`).

- **To try the application without installation:** launch `dist/MediaFlyout.exe`. You can exit the app via Task Manager or by launching `Stop.cmd` batch file.
- **To install the application for current user:** launch `Install.cmd` batch file and check your taskbar. The script copies `dist` folder to `%LocalAppData%\MediaFlyout` and adds a record to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` registry key.
- **To install the application for all users:** launch `InstallAllUsers.cmd` batch file - it copies `dist` folder to `%ProgramFiles%\MediaFlyout` and adds a record to `HKLM\Software\Microsoft\Windows\CurrentVersion\Run` registry key.

Uninstalling is very simple too, execute `Uninstall.cmd` if you installed MediaFlyout for current user or `UninstallAllUsers.cmd` otherwise.

**NOTE:** via MediaFlyout, you can control only those media players, which you can control in FN menu.

### Contributions
Any kind of contributions are appreciated, you can also help by translating the app into your language.

## Acknowledgements
Controlling playback from the taskbar was a very long awaited feature for me, which was supposed to appear in the next update of Windows 10 features, but due to the release of Windows 11 and putting Windows 10 into support mode, I decided to develop it myself. This would not have been possible without the following open source projects:

* [AudioFlyout](https://github.com/ADeltaX/AudioFlyout) and [ModernFlyouts](https://github.com/ModernFlyouts-Community/ModernFlyouts) - basic implementation and source application detection
* [EarTrumpet](https://github.com/File-New-Project/EarTrumpet) - the idea and notification icon colorization
* [FluentWPF](https://github.com/sourcechord/FluentWPF) - modern Fluent Design look and feel

The source code is released under the [MIT License](https://github.com/krlvm/MediaFlyout/blob/master/LICENSE).