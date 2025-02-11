using MS.Internal;

namespace System.Windows.Documents;

internal struct TextSegment
{
	internal static readonly TextSegment Null;

	private readonly ITextPointer _start;

	private readonly ITextPointer _end;

	internal ITextPointer Start => _start;

	internal ITextPointer End => _end;

	internal bool IsNull
	{
		get
		{
			if (_start != null)
			{
				return _end == null;
			}
			return true;
		}
	}

	internal TextSegment(ITextPointer startPosition, ITextPointer endPosition)
		: this(startPosition, endPosition, preserveLogicalDirection: false)
	{
	}

	internal TextSegment(ITextPointer startPosition, ITextPointer endPosition, bool preserveLogicalDirection)
	{
		ValidationHelper.VerifyPositionPair(startPosition, endPosition);
		if (startPosition.CompareTo(endPosition) == 0)
		{
			_start = startPosition.GetFrozenPointer(startPosition.LogicalDirection);
			_end = _start;
		}
		else
		{
			Invariant.Assert(startPosition.CompareTo(endPosition) < 0);
			_start = startPosition.GetFrozenPointer(preserveLogicalDirection ? startPosition.LogicalDirection : LogicalDirection.Backward);
			_end = endPosition.GetFrozenPointer((!preserveLogicalDirection) ? LogicalDirection.Forward : endPosition.LogicalDirection);
		}
	}

	internal bool Contains(ITextPointer position)
	{
		if (!IsNull && _start.CompareTo(position) <= 0)
		{
			return position.CompareTo(_end) <= 0;
		}
		return false;
	}
}
