#region Class: ObservableObject

namespace TreeTray.ViewModels;

public abstract class ObservableObject : INotifyPropertyChanged
{
	#region Events: Public

	public event PropertyChangedEventHandler? PropertyChanged;

	#endregion

	#region Methods: Protected

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	#endregion
}

#endregion
