#region Class: LauncherExecutionException

namespace TreeTray.Services;

public sealed class LauncherExecutionException : Exception
{
	#region Constructors: Public

	public LauncherExecutionException(string userMessage, Exception innerException)
		: base(userMessage, innerException)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);
		ArgumentNullException.ThrowIfNull(innerException);

		UserMessage = userMessage;
	}

	#endregion

	#region Properties: Public

	public string UserMessage { get; }

	#endregion
}

#endregion
