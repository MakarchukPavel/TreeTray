using Avalonia;
using TreeTray.Services;

namespace TreeTray.Tests;

public sealed class TrayContextMenuBuilderTests
{
	[Fact]
	public void CalculateSubmenuPopupOffsets_WhenPopupFitsToTheRight_UsesLeftOverlapOnly()
	{
		var popupBounds = new PixelRect(420, 180, 220, 180);
		var workingArea = new PixelRect(0, 0, 1920, 1080);
		var itemScreenPoint = new PixelPoint(300, 200);

		var (horizontalOffset, verticalOffset) = TrayContextMenuBuilder.CalculateSubmenuPopupOffsets(
			popupBounds,
			workingArea,
			itemScreenPoint);

		Assert.Equal(-8d, horizontalOffset);
		Assert.Equal(0d, verticalOffset);
	}

	[Fact]
	public void CalculateSubmenuPopupOffsets_WhenPopupOverflowsBottom_ShiftsItUpIntoWorkingArea()
	{
		var popupBounds = new PixelRect(420, 930, 220, 260);
		var workingArea = new PixelRect(0, 0, 1920, 1080);
		var itemScreenPoint = new PixelPoint(300, 940);

		var (_, verticalOffset) = TrayContextMenuBuilder.CalculateSubmenuPopupOffsets(
			popupBounds,
			workingArea,
			itemScreenPoint);

		Assert.Equal(-110d, verticalOffset);
	}

	[Fact]
	public void CalculateSubmenuPopupOffsets_WhenPopupWouldOverflowLeftAfterOverlap_ClampsIntoWorkingArea()
	{
		var popupBounds = new PixelRect(4, 200, 220, 180);
		var workingArea = new PixelRect(0, 0, 1920, 1080);
		var itemScreenPoint = new PixelPoint(300, 220);

		var (horizontalOffset, verticalOffset) = TrayContextMenuBuilder.CalculateSubmenuPopupOffsets(
			popupBounds,
			workingArea,
			itemScreenPoint);

		Assert.Equal(-4d, horizontalOffset);
		Assert.Equal(0d, verticalOffset);
	}
}
