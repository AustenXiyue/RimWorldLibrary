namespace System.Windows.Ink;

/// <summary>Provides data for the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event. </summary>
public class LassoSelectionChangedEventArgs : EventArgs
{
	private StrokeCollection _selectedStrokes;

	private StrokeCollection _deselectedStrokes;

	/// <summary>Gets the strokes that have been surrounded by the lasso path since the last time the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event was raised.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the strokes that have been surrounded by the lasso path since the last time the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event was raised.</returns>
	public StrokeCollection SelectedStrokes
	{
		get
		{
			if (_selectedStrokes != null)
			{
				return new StrokeCollection { _selectedStrokes };
			}
			return new StrokeCollection();
		}
	}

	/// <summary>Gets the strokes that have been removed from lasso path since the last time the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event was raised. </summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the strokes that have been removed from the lasso path since the last time the <see cref="E:System.Windows.Ink.IncrementalLassoHitTester.SelectionChanged" /> event was raised. </returns>
	public StrokeCollection DeselectedStrokes
	{
		get
		{
			if (_deselectedStrokes != null)
			{
				return new StrokeCollection { _deselectedStrokes };
			}
			return new StrokeCollection();
		}
	}

	internal LassoSelectionChangedEventArgs(StrokeCollection selectedStrokes, StrokeCollection deselectedStrokes)
	{
		_selectedStrokes = selectedStrokes;
		_deselectedStrokes = deselectedStrokes;
	}
}
