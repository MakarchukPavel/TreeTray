#region Class: TrayAppearanceService

namespace TreeTray.Services;

public sealed class TrayAppearanceService : ITrayAppearanceService
{
	#region Constants: Private

	private const string DefaultToolTipText = "TreeTray";

	private const int TrayIconCanvasSize = 64;

	#endregion
	#region Fields: Private

	private readonly ConcurrentDictionary<string, byte[]> _trayIconPngCache = new(StringComparer.Ordinal);

	private readonly ConcurrentDictionary<string, WindowIcon> _trayWindowIconCache = new(StringComparer.Ordinal);

	private readonly IIconService _iconService;

	#endregion
	#region Constructors: Public

	public TrayAppearanceService(IIconService iconService)
	{
		_iconService = iconService;
	}

	#endregion
	#region Methods: Private

	private static string BuildCacheKey(TreeTrayConfiguration configuration)
	{
		return string.Join(
			"|",
			GetNormalizedGlyph(configuration),
			GetNormalizedColor(configuration.TrayIconForegroundColor, "#FFFFFF"),
			GetNormalizedColor(configuration.TrayIconBackgroundColor, "#2F6FED"));
	}

	private static byte[] CreateCustomTrayIconPngBytes(TreeTrayConfiguration configuration)
	{
		var glyph = GetNormalizedGlyph(configuration);
		var foregroundColor = ParseColor(configuration.TrayIconForegroundColor, "#FFFFFF");
		var backgroundColor = ParseColor(configuration.TrayIconBackgroundColor, "#2F6FED");

		using var bitmap = new RenderTargetBitmap(new PixelSize(TrayIconCanvasSize, TrayIconCanvasSize));
		using (var drawingContext = bitmap.CreateDrawingContext())
		{
			var backgroundBrush = new Avalonia.Media.SolidColorBrush(backgroundColor);
			var foregroundBrush = new Avalonia.Media.SolidColorBrush(foregroundColor);
			var bounds = new Rect(0, 0, TrayIconCanvasSize, TrayIconCanvasSize);
			var contentBounds = new Rect(4, 4, TrayIconCanvasSize - 8, TrayIconCanvasSize - 8);

			drawingContext.DrawRectangle(backgroundBrush, null, contentBounds, 14, 14);

			var typeface = new Avalonia.Media.Typeface(
				new Avalonia.Media.FontFamily("Segoe UI"),
				Avalonia.Media.FontStyle.Normal,
				Avalonia.Media.FontWeight.Bold);
			var formattedText = new Avalonia.Media.FormattedText(
				glyph,
				CultureInfo.CurrentCulture,
				Avalonia.Media.FlowDirection.LeftToRight,
				typeface,
				38,
				foregroundBrush);
			var textOrigin = new Point(
				Math.Floor(bounds.Center.X - (formattedText.Width / 2)),
				Math.Floor(bounds.Center.Y - (formattedText.Height / 2)) - 1);

			drawingContext.DrawText(formattedText, textOrigin);
		}

		using var stream = new MemoryStream();
		bitmap.Save(stream);
		return stream.ToArray();
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static IntPtr CreateCustomWindowsTrayIconHandle(TreeTrayConfiguration configuration)
	{
		var glyph = GetNormalizedGlyph(configuration);
		var foregroundColor = ToDrawingColor(ParseColor(configuration.TrayIconForegroundColor, "#FFFFFF"));
		var backgroundColor = ToDrawingColor(ParseColor(configuration.TrayIconBackgroundColor, "#2F6FED"));
		var contentBounds = new System.Drawing.RectangleF(4, 4, TrayIconCanvasSize - 8, TrayIconCanvasSize - 8);

		using var bitmap = new System.Drawing.Bitmap(
			TrayIconCanvasSize,
			TrayIconCanvasSize,
			System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using var graphics = System.Drawing.Graphics.FromImage(bitmap);
		using var backgroundBrush = new System.Drawing.SolidBrush(backgroundColor);
		using var foregroundBrush = new System.Drawing.SolidBrush(foregroundColor);
		using var contentPath = CreateRoundedRectanglePath(contentBounds, 14f);
		using var font = new System.Drawing.Font("Segoe UI", 36f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
		using var stringFormat = new System.Drawing.StringFormat
		{
			Alignment = System.Drawing.StringAlignment.Center,
			LineAlignment = System.Drawing.StringAlignment.Center,
			FormatFlags = System.Drawing.StringFormatFlags.NoClip
		};

		graphics.Clear(System.Drawing.Color.Transparent);
		graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
		graphics.FillPath(backgroundBrush, contentPath);
		graphics.DrawString(glyph, font, foregroundBrush, contentBounds, stringFormat);

		return bitmap.GetHicon();
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(System.Drawing.RectangleF bounds, float cornerRadius)
	{
		var diameter = cornerRadius * 2;
		var path = new System.Drawing.Drawing2D.GraphicsPath();

		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
		path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
		path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();

		return path;
	}

	private byte[] GetCustomTrayIconPngBytes(TreeTrayConfiguration configuration)
	{
		var cacheKey = BuildCacheKey(configuration);
		return _trayIconPngCache.GetOrAdd(cacheKey, _ => CreateCustomTrayIconPngBytes(configuration));
	}

	private static string GetNormalizedColor(string? rawColor, string fallbackColor)
	{
		return string.IsNullOrWhiteSpace(rawColor)
			? fallbackColor
			: rawColor.Trim();
	}

	private static string GetNormalizedGlyph(TreeTrayConfiguration configuration)
	{
		if (string.IsNullOrWhiteSpace(configuration.TrayIconGlyph))
		{
			return string.Empty;
		}

		var trimmedValue = configuration.TrayIconGlyph.Trim();
		return StringInfo.GetNextTextElement(trimmedValue);
	}

	private static string NormalizeToolTipText(string? rawToolTipText)
	{
		return string.IsNullOrWhiteSpace(rawToolTipText)
			? DefaultToolTipText
			: rawToolTipText.Trim();
	}

	private static Avalonia.Media.Color ParseColor(string? rawColor, string fallbackColor)
	{
		if (!Avalonia.Media.Color.TryParse(rawColor, out var parsedColor)
			&& !Avalonia.Media.Color.TryParse(fallbackColor, out parsedColor))
		{
			return Avalonia.Media.Colors.Transparent;
		}

		return parsedColor;
	}

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	private static System.Drawing.Color ToDrawingColor(Avalonia.Media.Color color)
	{
		return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private static bool UsesCustomTrayIcon(TreeTrayConfiguration configuration)
	{
		return !string.IsNullOrWhiteSpace(GetNormalizedGlyph(configuration));
	}

	#endregion
	#region Methods: Public

	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
	public nint CreateWindowsTrayIconHandle(TreeTrayConfiguration configuration)
	{
		if (!OperatingSystem.IsWindows())
		{
			return IntPtr.Zero;
		}

		if (!UsesCustomTrayIcon(configuration))
		{
			return _iconService.CreateWindowsTrayIconHandle();
		}

		try
		{
			return CreateCustomWindowsTrayIconHandle(configuration);
		}
		catch
		{
			return _iconService.CreateWindowsTrayIconHandle();
		}
	}

	public byte[] GetTrayIconPngBytes(TreeTrayConfiguration configuration)
	{
		if (!UsesCustomTrayIcon(configuration))
		{
			using var stream = AssetLoader.Open(new Uri("avares://TreeTray/Assets/AppIcon.png"));
			using var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}

		var pngBytes = GetCustomTrayIconPngBytes(configuration);
		return pngBytes.ToArray();
	}

	public WindowIcon GetTrayIcon(TreeTrayConfiguration configuration)
	{
		if (!UsesCustomTrayIcon(configuration))
		{
			return _iconService.ApplicationIcon;
		}

		var cacheKey = BuildCacheKey(configuration);
		return _trayWindowIconCache.GetOrAdd(
			cacheKey,
			_ =>
			{
				var pngBytes = GetCustomTrayIconPngBytes(configuration);
				using var stream = new MemoryStream(pngBytes, writable: false);
				return new WindowIcon(stream);
			});
	}

	public string GetToolTipText(TreeTrayConfiguration configuration)
	{
		return NormalizeToolTipText(configuration.TrayToolTipText);
	}

	#endregion
}

#endregion
