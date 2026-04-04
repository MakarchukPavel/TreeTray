#region Interface: IUserNotificationService

namespace TreeTray.Abstractions;

public interface IUserNotificationService
{
	#region Methods: Public

	void ShowError(string title, string message);

	#endregion
}

#endregion
