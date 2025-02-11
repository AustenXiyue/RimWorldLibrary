using System.Windows.Documents;

namespace MS.Internal.PtsHost;

internal struct DirtyTextRange
{
	internal int StartIndex { get; set; }

	internal int PositionsAdded { get; set; }

	internal int PositionsRemoved { get; set; }

	internal bool FromHighlightLayer { get; set; }

	internal DirtyTextRange(int startIndex, int positionsAdded, int positionsRemoved, bool fromHighlightLayer = false)
	{
		StartIndex = startIndex;
		PositionsAdded = positionsAdded;
		PositionsRemoved = positionsRemoved;
		FromHighlightLayer = fromHighlightLayer;
	}

	internal DirtyTextRange(TextContainerChangeEventArgs change)
	{
		StartIndex = change.ITextPosition.Offset;
		PositionsAdded = 0;
		PositionsRemoved = 0;
		FromHighlightLayer = false;
		switch (change.TextChange)
		{
		case TextChangeType.ContentAdded:
			PositionsAdded = change.Count;
			break;
		case TextChangeType.ContentRemoved:
			PositionsRemoved = change.Count;
			break;
		case TextChangeType.PropertyModified:
			PositionsAdded = change.Count;
			PositionsRemoved = change.Count;
			break;
		}
	}
}
