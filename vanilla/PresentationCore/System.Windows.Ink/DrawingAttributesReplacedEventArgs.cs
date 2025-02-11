namespace System.Windows.Ink;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.DefaultDrawingAttributesReplaced" /> event.</summary>
public class DrawingAttributesReplacedEventArgs : EventArgs
{
	private DrawingAttributes _newDrawingAttributes;

	private DrawingAttributes _previousDrawingAttributes;

	/// <summary>Gets the new <see cref="T:System.Windows.Ink.DrawingAttributes" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Ink.DrawingAttributes" />. </returns>
	public DrawingAttributes NewDrawingAttributes => _newDrawingAttributes;

	/// <summary>Gets the old <see cref="T:System.Windows.Ink.DrawingAttributes" />.</summary>
	/// <returns>The old <see cref="T:System.Windows.Ink.DrawingAttributes" />.</returns>
	public DrawingAttributes PreviousDrawingAttributes => _previousDrawingAttributes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.DrawingAttributesReplacedEventArgs" /> class. </summary>
	/// <param name="newDrawingAttributes">The new <see cref="T:System.Windows.Ink.DrawingAttributes" />.</param>
	/// <param name="previousDrawingAttributes">The old <see cref="T:System.Windows.Ink.DrawingAttributes" />.</param>
	public DrawingAttributesReplacedEventArgs(DrawingAttributes newDrawingAttributes, DrawingAttributes previousDrawingAttributes)
	{
		if (newDrawingAttributes == null)
		{
			throw new ArgumentNullException("newDrawingAttributes");
		}
		if (previousDrawingAttributes == null)
		{
			throw new ArgumentNullException("previousDrawingAttributes");
		}
		_newDrawingAttributes = newDrawingAttributes;
		_previousDrawingAttributes = previousDrawingAttributes;
	}
}
