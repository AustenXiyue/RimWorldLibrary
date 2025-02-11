namespace System.Windows.Media.TextFormatting;

/// <summary>Defines a type of text content in which measuring, hit testing and drawing of the entire content is done in whole.</summary>
public abstract class TextEmbeddedObject : TextRun
{
	/// <summary>Gets the line breaking condition before the text object.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.LineBreakCondition" />.</returns>
	public abstract LineBreakCondition BreakBefore { get; }

	/// <summary>Gets the line breaking condition after the text object.</summary>
	/// <returns>An enumerated value of <see cref="T:System.Windows.LineBreakCondition" />.</returns>
	public abstract LineBreakCondition BreakAfter { get; }

	/// <summary>Determines whether the text object has a fixed size regardless of where it is placed within a line.</summary>
	/// <returns>true if the text object has a fixed size; otherwise, false.</returns>
	public abstract bool HasFixedSize { get; }

	/// <summary>Get text object measurement metrics that will fit within the specified remaining width of the paragraph.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObjectMetrics" /> value that represents the text object metrics.</returns>
	/// <param name="remainingParagraphWidth">A <see cref="T:System.Double" /> that represents the remaining paragraph width.</param>
	public abstract TextEmbeddedObjectMetrics Format(double remainingParagraphWidth);

	/// <summary>Gets the computed bounding box of the text object.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> value that represents the bounding box size of the text object.</returns>
	/// <param name="rightToLeft">A <see cref="T:System.Boolean" /> value that determines if the text object is drawn from right to left.</param>
	/// <param name="sideways">A <see cref="T:System.Boolean" /> value that determines if the text object is drawn with its side parallel to the baseline.</param>
	public abstract Rect ComputeBoundingBox(bool rightToLeft, bool sideways);

	/// <summary>Draws the text object.</summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> to use for rendering the text object.</param>
	/// <param name="origin">The <see cref="T:System.Windows.Point" /> value that represents the origin where the text object is drawn.</param>
	/// <param name="rightToLeft">A <see cref="T:System.Boolean" /> value that determines if the text object is drawn from right to left.</param>
	/// <param name="sideways">A <see cref="T:System.Boolean" /> value that determines if the text object is drawn with its side parallel to the baseline.</param>
	public abstract void Draw(DrawingContext drawingContext, Point origin, bool rightToLeft, bool sideways);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEmbeddedObject" /> class.</summary>
	protected TextEmbeddedObject()
	{
	}
}
