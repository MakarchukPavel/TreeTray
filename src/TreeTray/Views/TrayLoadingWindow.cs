#region Class: TrayLoadingWindow

namespace TreeTray.Views;

public sealed class TrayLoadingWindow : Window
{
	#region Constants: Private

	private const int WindowHeight = 128;

	private const int WindowWidth = 280;

	#endregion

	#region Fields: Private

	private readonly PixelPoint _screenPosition;

	#endregion

	#region Constructors: Public

	public TrayLoadingWindow(PixelPoint screenPosition)
	{
		_screenPosition = screenPosition;

		CanResize = false;
		Content = CreateContent();
		Height = WindowHeight;
		ShowActivated = true;
		ShowInTaskbar = false;
		SystemDecorations = SystemDecorations.None;
		Topmost = true;
		Width = WindowWidth;

		Deactivated += OnDeactivated;
		Opened += OnOpened;
	}

	#endregion

	#region Methods: Private

	private Control CreateContent()
	{
		return new Border
		{
			Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FCFCF7")),
			BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D9DDD1")),
			BorderThickness = new Thickness(1),
			BoxShadow = Avalonia.Media.BoxShadows.Parse("0 10 28 0 #24000000"),
			CornerRadius = new CornerRadius(18),
			Padding = new Thickness(18),
			Child = new StackPanel
			{
				Spacing = 12,
				Children =
				{
					new TextBlock
					{
						FontSize = 18,
						FontWeight = Avalonia.Media.FontWeight.SemiBold,
						Text = "Loading launchers..."
					},
					new ProgressBar
					{
						IsIndeterminate = true,
						Height = 10
					},
					new TextBlock
					{
						FontSize = 12,
						Opacity = 0.75,
						Text = "TreeTray is still indexing the configured launcher directory.",
						TextWrapping = Avalonia.Media.TextWrapping.Wrap
					}
				}
			}
		};
	}

	private void OnDeactivated(object? sender, EventArgs eventArgs)
	{
		Close();
	}

	private void OnOpened(object? sender, EventArgs eventArgs)
	{
		Opened -= OnOpened;
		Position = new PixelPoint(
			Math.Max(0, _screenPosition.X - WindowWidth + 24),
			Math.Max(0, _screenPosition.Y - WindowHeight - 16));
	}

	#endregion
}

#endregion
