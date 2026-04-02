# TreeTray Architecture

## Overview

TreeTray is a cross-platform launcher panel built with Avalonia and Microsoft Dependency Injection.

From a product perspective, TreeTray also serves as a practical replacement for the Windows taskbar toolbar workflow that existed in Windows 10 and was removed in Windows 11. The Windows implementation therefore emphasizes fast access to nested launcher folders from the tray and taskbar-adjacent UI.

The application is organized around a small set of focused services and view models:

- `ApplicationController` coordinates startup, runtime state, tray integration, launcher loading, and main window visibility.
- `ConfigurationService` loads, saves, and creates the XML configuration file with inline English documentation comments.
- `LauncherCatalogService` scans the configured launcher directory and builds the hierarchical launcher tree.
- `PlatformLauncherResolver` converts files and bundles into launch commands for Windows, macOS, and Linux.
- `LauncherExecutionService` starts launcher targets and opens supporting directories.
- `IconService` resolves application, folder, launcher, Windows shell, and macOS native file icons.
- `TrayAppearanceService` builds tray icon assets, tooltip text, and custom glyph-based tray visuals.
- `WindowsTrayIconService` hosts the native Windows tray icon and its click behavior.
- `MacOsStatusItemService` hosts the native macOS menu bar item and its click behavior.
- `PlatformContextMenuService` exposes platform-aware file and folder context menu behavior.
- `StartupRegistrationService` creates or removes autostart registration entries for each operating system.
- `MainWindowViewModel` exposes the application state and commands to the Avalonia launcher window.

## Layers

### Abstractions

Interfaces in `src/TreeTray/Abstractions` define the application contracts. The UI depends on interfaces instead of concrete implementations, which keeps responsibilities isolated and makes the structure easier to extend.

### Models

The model layer contains immutable or configuration-oriented types:

- `TreeTrayConfiguration`
- `LauncherEntry`
- `LauncherSnapshot`
- `LaunchCommand`
- `LauncherEntryType`

### Services

The service layer owns platform interaction and application behavior. Services are registered with Microsoft Dependency Injection in `ServiceCollectionExtensions`.

### View Models

The view model layer exposes bindable state for Avalonia XAML and keeps view code-behind minimal.

## Startup Flow

1. `Program` builds the DI container and starts the Avalonia desktop lifetime.
2. `App` resolves `IApplicationBootstrapper`.
3. `ApplicationBootstrapper` calls `ApplicationController.Start(...)`.
4. `ApplicationController` loads configuration, applies startup registration, builds the launcher catalog, and configures the tray icon, menu bar item, and main window.
5. `MainWindowViewModel` listens for controller state changes and refreshes the visible launcher tree.

## Runtime Shell Behavior

- If `EnableTrayIcon` is `true`, TreeTray starts in the background and exposes itself through a tray icon on Windows, a native menu bar item on macOS, or a tray icon on Linux.
- If `EnableTrayIcon` is `false`, TreeTray opens the main launcher window on startup and keeps it visible in the Windows taskbar, the macOS Dock, or the Linux task switcher.
- The launcher catalog is loaded asynchronously. During startup, the shell surface is created first and the launcher snapshot is populated in the background.
- The main window can be opened from the tray or menu bar and can be hidden again without shutting down the process when tray mode is enabled.

## Platform Integration

### Windows

- Uses a native tray host through `WindowsTrayIconService`.
- Uses native Windows shell context menus for launcher entries where available.
- Resolves launcher icons through Windows shell APIs, shortcut metadata, file associations, and AppX package metadata.
- Autostart is implemented by writing a `.cmd` file into the user's Startup folder instead of writing to the registry.

### macOS

- Uses a native AppKit-backed status item through `MacOsStatusItemService`.
- Supports configurable left-click and right-click behavior for the menu bar item.
- Resolves launcher icons through `NSWorkspace` first, then falls back to app bundle metadata and Quick Look thumbnails.
- Autostart is implemented through a user `LaunchAgent` plist in `~/Library/LaunchAgents`.

### Linux

- Uses Avalonia tray integration and native menus where supported by the current desktop environment.
- Supports `.desktop`, AppImage, shell script, symbolic link, and executable launcher entries.
- Autostart is implemented through a `.desktop` file in the user's autostart directory.

## Platform Notes

- Windows launchers are expected to be shortcuts or shell-launchable files such as `.lnk`, `.url`, `.exe`, `.cmd`, and `.bat`.
- macOS launchers are expected to be aliases, `.app` bundles, symbolic links, or shell launchers that can be opened with the `open` command.
- Linux launchers are expected to be `.desktop` files, AppImages, shell scripts, symbolic links, or executable files.

## Autostart Strategy

TreeTray stores per-user autostart entries and removes them again when `StartWithOperatingSystem` is disabled.

- Windows: `%AppData%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\TreeTray*.cmd`
- macOS: `~/Library/LaunchAgents/com.treetray.launcher*.plist`
- Linux: `~/.config/autostart/TreeTray*.desktop`

If the application is launched with `--config <path>`, the autostart entry includes the same configuration path. This allows multiple TreeTray instances to be registered independently for different launcher folders.

## Configuration Strategy

TreeTray creates the configuration file automatically on the first run. The file contains English XML comments that document every supported setting and its default value. This keeps the application self-documenting even without opening the README.

The configuration file can also be supplied explicitly through the command line. This allows multiple TreeTray instances to run side by side, each with its own launcher directory, tray appearance, startup registration, and shell behavior.
