#region Class: LaunchCommand

namespace TreeTray.Models;

public sealed class LaunchCommand
{
	#region Fields: Private

	private readonly IReadOnlyList<string> _arguments;

	#endregion

	#region Constructors: Public

	public LaunchCommand(
		string fileName,
		IEnumerable<string>? arguments = null,
		string? workingDirectory = null,
		bool useShellExecute = false)
	{
		ArgumentNullException.ThrowIfNull(fileName);

		FileName = fileName;
		_arguments = arguments?
			.Where(argument => !string.IsNullOrWhiteSpace(argument))
			.ToArray()
			?? Array.Empty<string>();
		WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? null : workingDirectory;
		UseShellExecute = useShellExecute;
	}

	#endregion

	#region Properties: Public

	public IReadOnlyList<string> Arguments => _arguments;

	public string FileName { get; }

	public bool UseShellExecute { get; }

	public string? WorkingDirectory { get; }

	#endregion
}

#endregion
