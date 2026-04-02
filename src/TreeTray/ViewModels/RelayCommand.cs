#region Class: RelayCommand

namespace TreeTray.ViewModels;

public sealed class RelayCommand : System.Windows.Input.ICommand
{
	#region Fields: Private

	private readonly Func<bool>? _canExecute;

	private readonly Action _execute;

	#endregion

	#region Constructors: Public

	public RelayCommand(Action execute, Func<bool>? canExecute = null)
	{
		_execute = execute ?? throw new ArgumentNullException(nameof(execute));
		_canExecute = canExecute;
	}

	#endregion

	#region Events: Public

	public event EventHandler? CanExecuteChanged;

	#endregion

	#region Methods: Public

	public bool CanExecute(object? parameter)
	{
		return _canExecute?.Invoke() ?? true;
	}

	public void Execute(object? parameter)
	{
		if (CanExecute(parameter))
		{
			_execute();
		}
	}

	public void NotifyCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	#endregion
}

#endregion
