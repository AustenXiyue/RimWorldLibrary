namespace System.Windows.Media.TextFormatting;

/// <summary>Represents a generic class that allows a simple pairing of an object of type T and a specified run length.</summary>
/// <typeparam name="T">The object type to pair.</typeparam>
public class TextSpan<T>
{
	private int _length;

	private T _value;

	/// <summary>Gets the number of characters in the text span.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the length of the text span.</returns>
	public int Length => _length;

	/// <summary>Gets the object associated with the text span.</summary>
	/// <returns>An object of type <paramref name="T" />.</returns>
	public T Value => _value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextSpan`1" /> class by specifying the length of the text span and the value associated with it.</summary>
	/// <param name="length">An <see cref="T:System.Int32" /> value that represents the number of characters in the text span.</param>
	/// <param name="value">The object associated with the text span.</param>
	public TextSpan(int length, T value)
	{
		_length = length;
		_value = value;
	}
}
