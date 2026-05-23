using Avalonia;
using TreeTray.Services;

namespace TreeTray.Tests;

public sealed class WindowsTrayIconServiceTests
{
	[Theory]
	[InlineData(0x0202)]
	[InlineData(0x0205)]
	[InlineData(0x0400)]
	[InlineData(0x0401)]
	public void ShouldUseCallbackAnchorPoint_ReturnsTrueForAnchorAwareEvents(int eventCode)
	{
		Assert.True(WindowsTrayIconService.ShouldUseCallbackAnchorPoint(eventCode));
	}

	[Fact]
	public void ShouldUseCallbackAnchorPoint_ReturnsFalseForContextMenuMessage()
	{
		Assert.False(WindowsTrayIconService.ShouldUseCallbackAnchorPoint(0x007B));
	}

	[Fact]
	public void TryGetCallbackAnchorPoint_IgnoresMessagesWithUndefinedCoordinates()
	{
		var anchorPoint = WindowsTrayIconService.TryGetCallbackAnchorPoint(0x007B, new IntPtr(0x001E000A));

		Assert.Null(anchorPoint);
	}

	[Fact]
	public void TryGetCallbackAnchorPoint_ReturnsSignedScreenCoordinatesForMouseMessages()
	{
		var anchorPoint = WindowsTrayIconService.TryGetCallbackAnchorPoint(0x0202, CreateWParam(-24, 1080));

		Assert.Equal(new PixelPoint(-24, 1080), anchorPoint);
	}

	[Fact]
	public void ResolvePreferredScreenPosition_PrefersFreshAnchorPointForImmediateOpen()
	{
		var resolvedPoint = WindowsTrayIconService.ResolvePreferredScreenPosition(
			new PixelPoint(1500, 1040),
			preferAnchorPoint: true,
			trayIconScreenPosition: new PixelPoint(12, 48),
			fallbackCursorPosition: new PixelPoint(24, 64));

		Assert.Equal(new PixelPoint(1500, 1040), resolvedPoint);
	}

	[Fact]
	public void ResolvePreferredScreenPosition_RecalculatesFromCurrentTrayPositionForDeferredOpen()
	{
		var resolvedPoint = WindowsTrayIconService.ResolvePreferredScreenPosition(
			new PixelPoint(12, 48),
			preferAnchorPoint: false,
			trayIconScreenPosition: new PixelPoint(1500, 1040),
			fallbackCursorPosition: new PixelPoint(24, 64));

		Assert.Equal(new PixelPoint(1500, 1040), resolvedPoint);
	}

	private static IntPtr CreateWParam(int x, int y)
	{
		var rawValue = ((long)(ushort)y << 16) | (ushort)x;
		return new IntPtr(rawValue);
	}
}
