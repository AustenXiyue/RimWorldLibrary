using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Provides data for the <see cref="E:System.Windows.Ink.StrokeCollection.StrokesChanged" /> event.</summary>
public class StrokeCollectionChangedEventArgs : EventArgs
{
	private StrokeCollection.ReadOnlyStrokeCollection _added;

	private StrokeCollection.ReadOnlyStrokeCollection _removed;

	private int _index = -1;

	/// <summary>Gets the strokes that have been added to the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the added strokes.</returns>
	public StrokeCollection Added
	{
		get
		{
			if (_added == null)
			{
				_added = new StrokeCollection.ReadOnlyStrokeCollection(new StrokeCollection());
			}
			return _added;
		}
	}

	/// <summary>Gets the strokes that have been removed from the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the removed strokes. </returns>
	public StrokeCollection Removed
	{
		get
		{
			if (_removed == null)
			{
				_removed = new StrokeCollection.ReadOnlyStrokeCollection(new StrokeCollection());
			}
			return _removed;
		}
	}

	internal int Index => _index;

	internal StrokeCollectionChangedEventArgs(StrokeCollection added, StrokeCollection removed, int index)
		: this(added, removed)
	{
		_index = index;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.StrokeCollectionChangedEventArgs" /> class. </summary>
	/// <param name="added">A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the added strokes.</param>
	/// <param name="removed">A <see cref="T:System.Windows.Ink.StrokeCollection" /> that contains the removed strokes.</param>
	public StrokeCollectionChangedEventArgs(StrokeCollection added, StrokeCollection removed)
	{
		if (added == null && removed == null)
		{
			throw new ArgumentException(SR.Format(SR.CannotBothBeNull, "added", "removed"));
		}
		_added = ((added == null) ? null : new StrokeCollection.ReadOnlyStrokeCollection(added));
		_removed = ((removed == null) ? null : new StrokeCollection.ReadOnlyStrokeCollection(removed));
	}
}
