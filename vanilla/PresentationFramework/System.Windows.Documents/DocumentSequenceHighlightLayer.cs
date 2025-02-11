using System.Collections;

namespace System.Windows.Documents;

internal class DocumentSequenceHighlightLayer : HighlightLayer
{
	private class DocumentSequenceHighlightChangedEventArgs : HighlightChangedEventArgs
	{
		private readonly IList _ranges;

		internal override IList Ranges => _ranges;

		internal override Type OwnerType => typeof(TextSelection);

		internal DocumentSequenceHighlightChangedEventArgs(IList ranges)
		{
			_ranges = ranges;
		}
	}

	private readonly DocumentSequenceTextContainer _docSeqContainer;

	internal override Type OwnerType => typeof(TextSelection);

	internal override event HighlightChangedEventHandler Changed;

	internal DocumentSequenceHighlightLayer(DocumentSequenceTextContainer docSeqContainer)
	{
		_docSeqContainer = docSeqContainer;
	}

	internal override object GetHighlightValue(StaticTextPointer staticTextPointer, LogicalDirection direction)
	{
		return null;
	}

	internal override bool IsContentHighlighted(StaticTextPointer staticTextPointer, LogicalDirection direction)
	{
		return _docSeqContainer.Highlights.IsContentHighlighted(staticTextPointer, direction);
	}

	internal override StaticTextPointer GetNextChangePosition(StaticTextPointer staticTextPointer, LogicalDirection direction)
	{
		return _docSeqContainer.Highlights.GetNextHighlightChangePosition(staticTextPointer, direction);
	}

	internal void RaiseHighlightChangedEvent(IList ranges)
	{
		if (Changed != null)
		{
			DocumentSequenceHighlightChangedEventArgs args = new DocumentSequenceHighlightChangedEventArgs(ranges);
			Changed(this, args);
		}
	}
}
