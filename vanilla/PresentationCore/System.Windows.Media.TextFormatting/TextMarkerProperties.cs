namespace System.Windows.Media.TextFormatting;

/// <summary>Represents an abstract class for defining text markers.</summary>
public abstract class TextMarkerProperties
{
	/// <summary>Gets the distance from the start of the line to the end of the text marker symbol.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the offset of the marker symbol from the beginning of the line.</returns>
	public abstract double Offset { get; }

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> that represents the source of the text runs for the marker symbol.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextFormatting.TextSource" /> that represents the text marker.</returns>
	public abstract TextSource TextSource { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextMarkerProperties" /> class.</summary>
	protected TextMarkerProperties()
	{
	}
}
