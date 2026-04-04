#region Class: UserNotificationService

namespace TreeTray.Services;

public sealed class UserNotificationService : IUserNotificationService
{
	#region Fields: Private

	private const uint MessageBoxIconError = 0x00000010;

	#endregion

	#region Methods: Private

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

	#endregion

	#region Methods: Public

	public void ShowError(string title, string message)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(title);
		ArgumentException.ThrowIfNullOrWhiteSpace(message);

		if (OperatingSystem.IsWindows())
		{
			MessageBox(IntPtr.Zero, message, title, MessageBoxIconError);
			return;
		}

		Debug.WriteLine($"{title}: {message}");
	}

	#endregion
}

#endregion
