using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Documents;

internal class SpellerHighlightLayer : HighlightLayer
{
	private class SpellerHighlightChangedEventArgs : HighlightChangedEventArgs
	{
		private readonly ReadOnlyCollection<TextSegment> _ranges;

		internal override IList Ranges => _ranges;

		internal override Type OwnerType => typeof(SpellerHighlightLayer);

		internal SpellerHighlightChangedEventArgs(ITextPointer start, ITextPointer end)
		{
			Invariant.Assert(start.CompareTo(end) < 0, "Bogus start/end combination!");
			_ranges = new ReadOnlyCollection<TextSegment>(new List<TextSegment>(1)
			{
				new TextSegment(start, end)
			});
		}
	}

	private readonly Speller _speller;

	private static readonly TextDecorationCollection _errorTextDecorations;

	internal override Type OwnerType => typeof(SpellerHighlightLayer);

	internal override event HighlightChangedEventHandler Changed;

	static SpellerHighlightLayer()
	{
		_errorTextDecorations = GetErrorTextDecorations();
	}

	internal SpellerHighlightLayer(Speller speller)
	{
		_speller = speller;
	}

	internal override object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction)
	{
		if (IsContentHighlighted(textPosition, direction))
		{
			return _errorTextDecorations;
		}
		return DependencyProperty.UnsetValue;
	}

	internal override bool IsContentHighlighted(StaticTextPointer textPosition, LogicalDirection direction)
	{
		return _speller.StatusTable.IsRunType(textPosition, direction, SpellerStatusTable.RunType.Error);
	}

	internal override StaticTextPointer GetNextChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		return _speller.StatusTable.GetNextErrorTransition(textPosition, direction);
	}

	internal void FireChangedEvent(ITextPointer start, ITextPointer end)
	{
		if (Changed != null)
		{
			Changed(this, new SpellerHighlightChangedEventArgs(start, end));
		}
	}

	private static TextDecorationCollection GetErrorTextDecorations()
	{
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = drawingGroup.Open();
		Pen pen = new Pen(Brushes.Red, 0.33);
		drawingContext.DrawLine(pen, new Point(0.0, 0.0), new Point(0.5, 1.0));
		drawingContext.DrawLine(pen, new Point(0.5, 1.0), new Point(1.0, 0.0));
		drawingContext.Close();
		DrawingBrush drawingBrush = new DrawingBrush(drawingGroup);
		drawingBrush.TileMode = TileMode.Tile;
		drawingBrush.Viewport = new Rect(0.0, 0.0, 3.0, 3.0);
		drawingBrush.ViewportUnits = BrushMappingMode.Absolute;
		TextDecoration value = new TextDecoration(TextDecorationLocation.Underline, new Pen(drawingBrush, 3.0), 0.0, TextDecorationUnit.FontRecommended, TextDecorationUnit.Pixel);
		TextDecorationCollection textDecorationCollection = new TextDecorationCollection();
		textDecorationCollection.Add(value);
		textDecorationCollection.Freeze();
		return textDecorationCollection;
	}
}
