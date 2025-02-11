namespace System.Windows.Media.TextFormatting;

/// <summary>Specifies properties for a <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObject" />.</summary>
public class TextEmbeddedObjectMetrics
{
	private double _width;

	private double _height;

	private double _baseline;

	/// <summary>Gets the width of the text object.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text object width.</returns>
	public double Width => _width;

	/// <summary>Gets the height of the text object.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text object height.</returns>
	public double Height => _height;

	/// <summary>Gets the baseline of the text object.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the text object baseline relative to its height.</returns>
	public double Baseline => _baseline;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObjectMetrics" /> class using specified width, height, and baseline values.</summary>
	/// <param name="width">A <see cref="T:System.Double" /> that represents the <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObject" /> width.</param>
	/// <param name="height">A <see cref="T:System.Double" /> that represents the <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObject" /> height.</param>
	/// <param name="baseline">A <see cref="T:System.Double" /> that represents the <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObject" /> baseline relative to <paramref name="height" />.</param>
	public TextEmbeddedObjectMetrics(double width, double height, double baseline)
	{
		_width = width;
		_height = height;
		_baseline = baseline;
	}
}
