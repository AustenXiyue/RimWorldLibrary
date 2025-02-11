namespace System.Windows.Media.TextFormatting;

/// <summary>Represents a sequence of characters that share a single property set.</summary>
public abstract class TextRun
{
	/// <summary>Gets a reference to the text run character buffer.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> value representing the characters in the text run.</returns>
	public abstract CharacterBufferReference CharacterBufferReference { get; }

	/// <summary>Gets the number of characters in the text run.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the number of characters.</returns>
	public abstract int Length { get; }

	/// <summary>Gets the set of text properties that are shared by every character in the text run, such as typeface or foreground brush.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value that represents the set of shared text properties.</returns>
	public abstract TextRunProperties Properties { get; }

	/// <summary>Creates an instance of a <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> object.</summary>
	protected TextRun()
	{
	}
}
