namespace System.Collections;

/// <summary>Defines a dictionary key/value pair that can be set or retrieved.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public struct DictionaryEntry
{
	private object _key;

	private object _value;

	/// <summary>Gets or sets the key in the key/value pair.</summary>
	/// <returns>The key in the key/value pair.</returns>
	/// <filterpriority>1</filterpriority>
	public object Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	/// <summary>Gets or sets the value in the key/value pair.</summary>
	/// <returns>The value in the key/value pair.</returns>
	/// <filterpriority>1</filterpriority>
	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Collections.DictionaryEntry" /> type with the specified key and value.</summary>
	/// <param name="key">The object defined in each key/value pair. </param>
	/// <param name="value">The definition associated with <paramref name="key" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null and the .NET Framework version is 1.0 or 1.1. </exception>
	public DictionaryEntry(object key, object value)
	{
		_key = key;
		_value = value;
	}

	public void Deconstruct(out object key, out object value)
	{
		key = Key;
		value = Value;
	}
}
