using System.Collections;

namespace System.Windows.Input;

/// <summary>Encapsulates an input event when it is being processed by the input manager.</summary>
public class StagingAreaInputItem
{
	private bool _isMarker;

	private InputEventArgs _input;

	private Hashtable _dictionary;

	/// <summary>Gets the input event data associated with this <see cref="T:System.Windows.Input.StagingAreaInputItem" /> object </summary>
	/// <returns>The event.</returns>
	public InputEventArgs Input => _input;

	internal bool IsMarker => _isMarker;

	internal StagingAreaInputItem(bool isMarker)
	{
		_isMarker = isMarker;
	}

	internal void Reset(InputEventArgs input, StagingAreaInputItem promote)
	{
		_input = input;
		if (promote != null && promote._dictionary != null)
		{
			_dictionary = (Hashtable)promote._dictionary.Clone();
		}
		else if (_dictionary != null)
		{
			_dictionary.Clear();
		}
		else
		{
			_dictionary = new Hashtable();
		}
	}

	/// <summary>Gets the input data associated with the specified key. </summary>
	/// <returns>The data for this key, or null.</returns>
	/// <param name="key">An arbitrary key for the data. This cannot be null.</param>
	public object GetData(object key)
	{
		return _dictionary[key];
	}

	/// <summary>Creates a dictionary entry by using the specified key and the specified data. </summary>
	/// <param name="key">An arbitrary key for the data. This cannot be null.</param>
	/// <param name="value">The data to set for this key. This can be null.</param>
	public void SetData(object key, object value)
	{
		_dictionary[key] = value;
	}
}
