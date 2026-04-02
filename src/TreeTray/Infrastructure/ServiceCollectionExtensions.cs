#region Class: ServiceCollectionExtensions

namespace TreeTray.Infrastructure;

public static class ServiceCollectionExtensions
{
	#region Methods: Public

	public static ServiceProvider CreateServiceProvider(ApplicationStartupOptions? startupOptions = null)
	{
		var services = new ServiceCollection();
		services.AddSingleton(startupOptions ?? new ApplicationStartupOptions());

		services.AddSingleton<IApplicationPaths, Services.ApplicationPaths>();
		services.AddSingleton<IConfigurationService, Services.ConfigurationService>();
		services.AddSingleton<IPlatformLauncherResolver, Services.PlatformLauncherResolver>();
		services.AddSingleton<ILauncherCatalogService, Services.LauncherCatalogService>();
		services.AddSingleton<ILauncherExecutionService, Services.LauncherExecutionService>();
		services.AddSingleton<IStartupRegistrationService, Services.StartupRegistrationService>();
		services.AddSingleton<IIconService, Services.IconService>();
		services.AddSingleton<ITrayAppearanceService, Services.TrayAppearanceService>();
		services.AddSingleton<IWindowsShellContextMenuService, Services.WindowsShellContextMenuService>();
		services.AddSingleton<IPlatformContextMenuService, Services.PlatformContextMenuService>();
		services.AddSingleton<IMacOsStatusItemService, Services.MacOsStatusItemService>();
		services.AddSingleton<ITrayMenuBuilder, Services.TrayMenuBuilder>();
		services.AddSingleton<ITrayContextMenuBuilder, Services.TrayContextMenuBuilder>();
		services.AddSingleton<ITrayPopupMenuService, Services.TrayPopupMenuService>();
		services.AddSingleton<IWindowsTrayIconService, Services.WindowsTrayIconService>();
		services.AddSingleton<IApplicationController, Services.ApplicationController>();
		services.AddSingleton<IApplicationBootstrapper, Services.ApplicationBootstrapper>();
		services.AddSingleton<Views.MainWindow>();
		services.AddSingleton<MainWindowViewModel>();

		return services.BuildServiceProvider(new ServiceProviderOptions
		{
			ValidateOnBuild = true,
			ValidateScopes = true
		});
	}

	#endregion
}

#endregion
