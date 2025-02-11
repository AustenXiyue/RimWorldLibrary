using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

/// <summary>Defines a specialized text run that is used to mark a range of hidden characters.</summary>
public class TextHidden : TextRun
{
	private int _length;

	/// <summary>Gets a reference to the <see cref="T:System.Windows.Media.TextFormatting.TextHidden" /> character buffer.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> value.</returns>
	public sealed override CharacterBufferReference CharacterBufferReference => default(CharacterBufferReference);

	/// <summary>Gets the character length of the <see cref="T:System.Windows.Media.TextFormatting.TextHidden" /> character buffer.</summary>
	/// <returns>An <see cref="T:System.Int32" /> object that represents the length of the character buffer.</returns>
	public sealed override int Length => _length;

	/// <summary>Gets the set of properties shared by every text character of the <see cref="T:System.Windows.Media.TextFormatting.TextHidden" /> character buffer.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value that represents the properties shared by every text character.</returns>
	public sealed override TextRunProperties Properties => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextHidden" /> class using a specified character length.</summary>
	/// <param name="length">The number of characters in the <see cref="T:System.Windows.Media.TextFormatting.TextHidden" /> buffer.</param>
	public TextHidden(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterMustBeGreaterThanZero);
		}
		_length = length;
	}
}
