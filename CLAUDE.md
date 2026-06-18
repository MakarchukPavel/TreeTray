# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

TreeTray is a cross-platform Avalonia (`net9.0`) desktop app that turns a folder of launchers into a native tray icon (Windows/Linux), a menu bar item (macOS), and an Avalonia launcher window. Its product goal is to restore the Windows 10 "taskbar toolbar" workflow that Windows 11 removed. See [README.md](README.md) and [docs/Architecture.md](docs/Architecture.md) for the product/architecture overview.

## Build, Run, Test

```powershell
dotnet restore .\TreeTray.slnx
dotnet build .\TreeTray.slnx

# Run the app
dotnet run --project .\src\TreeTray\TreeTray.csproj
# Run with a specific config file (enables multiple side-by-side instances)
dotnet run --project .\src\TreeTray\TreeTray.csproj -- --config "D:\TreeTray\Work.settings.xml"

# Tests (xUnit)
dotnet test .\tests\TreeTray.Tests\TreeTray.Tests.csproj
# Single test class / method
dotnet test .\tests\TreeTray.Tests\TreeTray.Tests.csproj --filter "FullyQualifiedName~WindowsTrayIconServiceTests"
dotnet test .\tests\TreeTray.Tests\TreeTray.Tests.csproj --filter "Name=ResolvePreferredScreenPosition_PrefersFreshAnchorPointForImmediateOpen"
```

Note: the release CI (`.github/workflows/release.yml`) runs `dotnet test src/TreeTray/TreeTray.csproj`, which targets the app project, not the test project. To actually execute the unit tests locally, target `tests/TreeTray.Tests`. Release builds publish self-contained per-OS zips and are triggered only by pushing a `*.*.*` tag.

NuGet packages restore into a **repo-local** folder (`RestorePackagesPath` → `.nuget/packages`), not the global cache. Restore is offline-tolerant (`RestoreIgnoreFailedSources`, falls back to the user global cache via `RestoreAdditionalProjectSources`).

## Architecture

Plain Microsoft DI (`Microsoft.Extensions.DependencyInjection`) — **no MVVM framework**. Startup chain:

1. `Program.Main` parses args (`ApplicationStartupOptionsParser`) → builds the DI container (`ServiceCollectionExtensions.CreateServiceProvider`) → starts the Avalonia classic desktop lifetime. The container is exposed as a static `Program.Services`.
2. `App.OnFrameworkInitializationCompleted` resolves `IApplicationBootstrapper`, which calls `ApplicationController.Start(app)`.
3. `ApplicationController` is the orchestrator: loads config, applies theme + autostart registration, syncs the shell (tray vs. window), then loads the launcher catalog **asynchronously** in the background.

`ApplicationController` (`src/TreeTray/Services/ApplicationController.cs`) holds essentially all runtime state and is the single source of truth. Key behaviors to preserve when editing:
- **Async snapshot loading is version-guarded.** `BeginSnapshotLoad` bumps `_loadVersion` via `Interlocked`; `LoadSnapshotAsync` discards its result if a newer load started or the app is shutting down. Catalog scanning runs on a `Task.Run` thread; UI updates are marshaled back via `Dispatcher.UIThread`.
- **`SyncShell(initialStartup)`** decides tray icon vs. main window, sets `ShutdownMode` (`OnExplicitShutdown` when tray is enabled so closing the window doesn't kill the process, else `OnMainWindowClose`).
- **Theme** is derived from `Configuration.ThemeMode` (`System`/`Light`/`Dark`) and applied via `Application.RequestedThemeVariant`. In `System` mode the controller listens to `ActualThemeVariant` changes to refresh the UI.
- `UpdateConfiguration` clones the config (`CloneConfiguration`), saves it, re-applies autostart, and re-syncs the shell — runtime settings changes flow through here.

### Platform branching

Platform-specific behavior is selected at runtime with `OperatingSystem.IsWindows()` / `IsMacOS()`, with **Linux as the implicit `else` fallback** (Avalonia's built-in `TrayIcon`). Each platform concern has a dedicated service:
- Windows: `WindowsTrayIconService` (native tray host + click anchor math), `WindowsShellContextMenuService`.
- macOS: `MacOsStatusItemService` (AppKit status item).
- Tray visuals/menus: `TrayAppearanceService`, `TrayMenuBuilder`, `TrayContextMenuBuilder`, `TrayPopupMenuService` (Windows popup window in `Views/TrayPopupWindow.cs`).

When adding platform logic, keep the same shape: branch on `OperatingSystem.*`, Linux is the default.

### Layering

- `Abstractions/` — one `I*` interface per service. **Everything is wired as a singleton** in `ServiceCollectionExtensions` and consumers depend on the interface, never the concrete type. Adding a service means: add the interface, the implementation under `Services/`, and a `services.AddSingleton<I…, …>()` line.
- `Models/` — config and launcher domain types (`TreeTrayConfiguration`, `LauncherEntry`, `LauncherSnapshot`, `LaunchCommand`, `LauncherEntryType`, `ApplicationThemeModes`). `LauncherEntryLaunchBatchResolver` computes the direct-children set for the Ctrl+click batch-launch gesture (does not recurse into nested folders).
- `Services/` — platform interaction and app behavior. `LauncherCatalogService` scans the directory into a tree; `PlatformLauncherResolver` maps files/bundles to `LaunchCommand`s; `LauncherExecutionService` runs them; `IconService` resolves icons.
- `ViewModels/` — **hand-rolled MVVM**: `ObservableObject`, `RelayCommand`, `ViewModelBase` are custom (no CommunityToolkit). `MainWindowViewModel` subscribes to `ApplicationController.StateChanged` and rebuilds the visible tree.
- `Views/` — Avalonia XAML (`App.axaml`, `Views/MainWindow.axaml`) with compiled bindings on by default. `TrayPopupWindow` and `TrayLoadingWindow` are code-only windows.

## Code Conventions

- **Region style (strict, applied everywhere):** every file wraps its type in `#region Class: TypeName` … `#endregion`, and members are grouped into regions: `Fields: Private`, `Constructors: Public`, `Properties: Private`, `Properties: Public`, `Events: Public`, `Methods: Private`, `Methods: Public` (public/private ordered alphabetically within each). Match this when adding members.
- **Tabs** for indentation in `.cs` files.
- `Nullable` and `ImplicitUsings` are enabled, but namespace imports are centralized in `src/TreeTray/GlobalUsings.cs` (and the test project's equivalent) rather than per-file `using` lines — add shared usings there.
- Config is XML with inline English doc comments, written by `ConfigurationService` on first run. Default path is per-OS under the app data dir; `--config <path>` (or first positional arg) overrides it.
