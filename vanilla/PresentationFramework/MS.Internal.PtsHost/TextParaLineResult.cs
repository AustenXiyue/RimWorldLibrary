using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;

namespace MS.Internal.PtsHost;

internal sealed class TextParaLineResult : LineResult
{
	private readonly TextParaClient _owner;

	private int _dcp;

	private int _cch;

	private readonly Rect _layoutBox;

	private readonly double _baseline;

	private ITextPointer _startPosition;

	private ITextPointer _endPosition;

	private int _cchContent;

	private int _cchEllipses;

	internal override ITextPointer StartPosition
	{
		get
		{
			if (_startPosition == null)
			{
				_startPosition = _owner.GetTextPosition(_dcp, LogicalDirection.Forward);
			}
			return _startPosition;
		}
	}

	internal override ITextPointer EndPosition
	{
		get
		{
			if (_endPosition == null)
			{
				_endPosition = _owner.GetTextPosition(_dcp + _cch, LogicalDirection.Backward);
			}
			return _endPosition;
		}
	}

	internal override int StartPositionCP => _dcp + _owner.Paragraph.ParagraphStartCharacterPosition;

	internal override int EndPositionCP => _dcp + _cch + _owner.Paragraph.ParagraphStartCharacterPosition;

	internal override Rect LayoutBox => _layoutBox;

	internal override double Baseline => _baseline;

	internal int DcpLast
	{
		get
		{
			return _dcp + _cch;
		}
		set
		{
			_cch = value - _dcp;
		}
	}

	internal override ITextPointer GetTextPositionFromDistance(double distance)
	{
		return _owner.GetTextPositionFromDistance(_dcp, distance);
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		return false;
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		return null;
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		return null;
	}

	internal override ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		return null;
	}

	internal override ITextPointer GetContentEndPosition()
	{
		EnsureComplexData();
		return _owner.GetTextPosition(_dcp + _cchContent, LogicalDirection.Backward);
	}

	internal override ITextPointer GetEllipsesPosition()
	{
		EnsureComplexData();
		if (_cchEllipses != 0)
		{
			return _owner.GetTextPosition(_dcp + _cch - _cchEllipses, LogicalDirection.Forward);
		}
		return null;
	}

	internal override int GetContentEndPositionCP()
	{
		EnsureComplexData();
		return _dcp + _cchContent;
	}

	internal override int GetEllipsesPositionCP()
	{
		EnsureComplexData();
		return _dcp + _cch - _cchEllipses;
	}

	internal TextParaLineResult(TextParaClient owner, int dcp, int cch, Rect layoutBox, double baseline)
	{
		_owner = owner;
		_dcp = dcp;
		_cch = cch;
		_layoutBox = layoutBox;
		_baseline = baseline;
		_cchContent = (_cchEllipses = -1);
	}

	private void EnsureComplexData()
	{
		if (_cchContent == -1)
		{
			_owner.GetLineDetails(_dcp, out _cchContent, out _cchEllipses);
		}
	}
}
