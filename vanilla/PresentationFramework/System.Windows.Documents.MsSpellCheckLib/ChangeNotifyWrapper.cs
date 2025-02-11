using System.ComponentModel;

namespace System.Windows.Documents.MsSpellCheckLib;

internal class ChangeNotifyWrapper<T> : IChangeNotifyWrapper<T>, IChangeNotifyWrapper, INotifyPropertyChanged
{
	private T _value;

	private bool _shouldThrowInvalidCastException;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
		}
	}

	object IChangeNotifyWrapper.Value
	{
		get
		{
			return Value;
		}
		set
		{
			T val = default(T);
			try
			{
				val = (T)value;
			}
			catch (InvalidCastException) when (!_shouldThrowInvalidCastException)
			{
				return;
			}
			Value = val;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	internal ChangeNotifyWrapper(T value = default(T), bool shouldThrowInvalidCastException = false)
	{
		Value = value;
		_shouldThrowInvalidCastException = shouldThrowInvalidCastException;
	}
}
