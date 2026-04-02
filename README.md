# TreeTray

TreeTray is a cross-platform Avalonia application that turns a folder of launchers into a native tray/menu experience and a desktop launcher window. On Windows 11, it is designed to restore the practical "toolbars" workflow that many users previously relied on in Windows 10 before Microsoft removed that taskbar feature. The application is designed for Windows, macOS, and Linux and uses Microsoft Dependency Injection with a service-oriented architecture.

## What It Does

- Recreates a Windows 10 style launcher-toolbar workflow for Windows 11 users who still want a fast taskbar-adjacent shortcuts panel.
- Scans a configured directory for launcher entries and nested folders.
- Builds a hierarchical launcher tree.
- Exposes the same catalog through:
  - a native tray icon on Windows and Linux,
  - a menu bar item on macOS,
  - a launcher window that can appear in the taskbar or Dock.
- Starts target applications when a launcher entry is selected.
- Can register itself to start automatically with the operating system.
- Creates an XML configuration file with inline English documentation comments.

## Supported Launcher Types

### Windows

- `.lnk`
- `.url`
- `.exe`
- `.cmd`
- `.bat`
- `.appref-ms`

### macOS

- aliases
- `.app` bundles
- symbolic links
- `.command`
- `.sh`
- `.workflow`

### Linux

- `.desktop`
- `.AppImage`
- shell scripts
- symbolic links
- executable files

## Solution Layout

- `TreeTray.slnx` - the XML solution file.
- `src/TreeTray` - the Avalonia application.
- `specs/Application-NativeLanguage.md` - the original native-language specification.
- `specs/Application.md` - the English translation of the specification.
- `docs/Architecture.md` - a short architecture overview.

## Architecture

The application is split into focused classes with distinct responsibilities:

- `ApplicationController` manages startup, tray integration, shell visibility, and runtime state.
- `ConfigurationService` loads and creates the XML configuration file.
- `LauncherCatalogService` scans the launcher directory and builds the hierarchical model.
- `PlatformLauncherResolver` maps platform-specific launcher files into executable commands.
- `LauncherExecutionService` starts launchers and opens supporting folders.
- `StartupRegistrationService` creates or removes autostart registration entries for each operating system.
- `TrayMenuBuilder` creates the native Avalonia menu from the launcher tree.
- `MainWindowViewModel` exposes state and commands to the Avalonia UI.

All runtime services are registered through Microsoft Dependency Injection in `ServiceCollectionExtensions`.

## Configuration File

TreeTray creates the configuration file on the first run if it does not already exist.

Default location:

- Windows: `%AppData%\\TreeTray\\TreeTray.settings.xml`
- macOS: `~/Library/Application Support/TreeTray/TreeTray.settings.xml`
- Linux: `~/.config/TreeTray/TreeTray.settings.xml`

Custom location:

- Pass `--config <path>` to use a different configuration file.
- You can also pass the configuration file path as the first positional argument.
- This makes it possible to run multiple TreeTray instances with different launcher folders.

Tray appearance:

- `TrayIconGlyph` lets you render a generated tray icon from a single visible text element such as `W`, `H`, or `A`.
- `TrayIconForegroundColor` sets the glyph color.
- `TrayIconBackgroundColor` sets the generated tray icon background color.
- `TrayToolTipText` sets the tooltip text shown on hover and is also used in the launcher window title and the internal header.
- If `TrayIconGlyph` is empty, TreeTray uses the default application icon.

These values can be edited at runtime from the main window in the `Runtime Settings` section.

Tray click behavior:

- `InvertTrayIconMouseButtons` changes the tray icon or menu bar click behavior on Windows and macOS.
- When `false`, left click opens the launcher menu and right click opens the main window.
- When `true`, left click opens the main window and right click opens the launcher menu.

Example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- TreeTray configuration file. -->
<!-- Edit the values below and reload the application from the launcher window or the tray menu. -->
<TreeTrayConfiguration>
  <!-- The absolute or relative path to the directory that contains launchers. -->
  <LaunchersDirectory>C:\Users\Name\AppData\Roaming\Microsoft\Windows\Start Menu\Programs</LaunchersDirectory>
  <!-- When true, TreeTray shows a tray icon on Windows and Linux, or a menu bar item on macOS. -->
  <EnableTrayIcon>true</EnableTrayIcon>
  <!-- When true, TreeTray registers itself to start automatically after sign-in. -->
  <StartWithOperatingSystem>false</StartWithOperatingSystem>
  <!-- Optional tray or menu bar click inversion on Windows and macOS. -->
  <InvertTrayIconMouseButtons>false</InvertTrayIconMouseButtons>
  <!-- Optional custom tray glyph. -->
  <TrayIconGlyph>W</TrayIconGlyph>
  <!-- Optional generated tray glyph color. -->
  <TrayIconForegroundColor>#FFFFFF</TrayIconForegroundColor>
  <!-- Optional generated tray icon background color. -->
  <TrayIconBackgroundColor>#2F6FED</TrayIconBackgroundColor>
  <!-- Optional tray tooltip text. -->
  <TrayToolTipText>Work shortcuts</TrayToolTipText>
</TreeTrayConfiguration>
```

Multiple instance example:

```powershell
.\TreeTray.exe --config "D:\TreeTray\Work.settings.xml"
.\TreeTray.exe --config "D:\TreeTray\Home.settings.xml"
.\TreeTray.exe --config "D:\TreeTray\Admin.settings.xml"
```

## Build

Restore and build the solution with the .NET SDK:

```powershell
dotnet restore .\TreeTray.slnx
dotnet build .\TreeTray.slnx
```

Run the application:

```powershell
dotnet run --project .\src\TreeTray\TreeTray.csproj
```

Run with a custom configuration file:

```powershell
dotnet run --project .\src\TreeTray\TreeTray.csproj -- --config "D:\TreeTray\Work.settings.xml"
```

Windows executable example:

```powershell
.\src\TreeTray\bin\Debug\net9.0\TreeTray.exe --config "D:\TreeTray\Work.settings.xml"
```

## Publish Examples

Windows:

```powershell
dotnet publish .\src\TreeTray\TreeTray.csproj -c Release -r win-x64 --self-contained false
```

macOS:

```powershell
dotnet publish ./src/TreeTray/TreeTray.csproj -c Release -r osx-x64 --self-contained false
```

Linux:

```powershell
dotnet publish ./src/TreeTray/TreeTray.csproj -c Release -r linux-x64 --self-contained false
```

## Notes

- On Windows 11, TreeTray can be used as a practical replacement for the old Windows 10 taskbar toolbar experience that is no longer available in the operating system.
- The tray/menu experience is native, but taskbar/Dock behavior is represented by the Avalonia launcher window.
- Launcher discovery is path-based and intentionally conservative so that unrelated files are skipped whenever possible.
