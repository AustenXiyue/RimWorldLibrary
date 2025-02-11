namespace System.Windows.Ink;

/// <summary>Represents the method that will handle the <see cref="E:System.Windows.Ink.IncrementalStrokeHitTester.StrokeHit" /> event of a <see cref="T:System.Windows.Ink.IncrementalStrokeHitTester" />. </summary>
public class StrokeHitEventArgs : EventArgs
{
	private Stroke _stroke;

	private StrokeIntersection[] _hitFragments;

	/// <summary>Gets the <see cref="T:System.Windows.Ink.Stroke" /> that is intersected by the eraser path.</summary>
	/// <returns>The <see cref="T:System.Windows.Ink.Stroke" /> that is intersected by the eraser path.</returns>
	public Stroke HitStroke => _stroke;

	internal StrokeHitEventArgs(Stroke stroke, StrokeIntersection[] hitFragments)
	{
		_stroke = stroke;
		_hitFragments = hitFragments;
	}

	/// <summary>Returns the strokes that are a result of the eraser path erasing a stroke.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the strokes that are created after the eraser path erases part of <see cref="P:System.Windows.Ink.StrokeHitEventArgs.HitStroke" />.</returns>
	public StrokeCollection GetPointEraseResults()
	{
		return _stroke.Erase(_hitFragments);
	}
}
