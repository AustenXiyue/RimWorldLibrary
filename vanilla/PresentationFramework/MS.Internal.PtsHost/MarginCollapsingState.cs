using System;

namespace MS.Internal.PtsHost;

internal sealed class MarginCollapsingState : UnmanagedHandle
{
	private int _maxPositive;

	private int _minNegative;

	internal int Margin => _maxPositive + _minNegative;

	internal static void CollapseTopMargin(PtsContext ptsContext, MbpInfo mbp, MarginCollapsingState mcsCurrent, out MarginCollapsingState mcsNew, out int margin)
	{
		margin = 0;
		mcsNew = null;
		mcsNew = new MarginCollapsingState(ptsContext, mbp.MarginTop);
		if (mcsCurrent != null)
		{
			mcsNew.Collapse(mcsCurrent);
		}
		if (mbp.BPTop != 0)
		{
			margin = mcsNew.Margin;
			mcsNew.Dispose();
			mcsNew = null;
		}
		else if (mcsCurrent == null && DoubleUtil.IsZero(mbp.Margin.Top))
		{
			mcsNew.Dispose();
			mcsNew = null;
		}
	}

	internal static void CollapseBottomMargin(PtsContext ptsContext, MbpInfo mbp, MarginCollapsingState mcsCurrent, out MarginCollapsingState mcsNew, out int margin)
	{
		margin = 0;
		mcsNew = null;
		if (!DoubleUtil.IsZero(mbp.Margin.Bottom))
		{
			mcsNew = new MarginCollapsingState(ptsContext, mbp.MarginBottom);
		}
		if (mcsCurrent == null)
		{
			return;
		}
		if (mbp.BPBottom != 0)
		{
			margin = mcsCurrent.Margin;
			return;
		}
		if (mcsNew == null)
		{
			mcsNew = new MarginCollapsingState(ptsContext, 0);
		}
		mcsNew.Collapse(mcsCurrent);
	}

	internal MarginCollapsingState(PtsContext ptsContext, int margin)
		: base(ptsContext)
	{
		_maxPositive = ((margin >= 0) ? margin : 0);
		_minNegative = ((margin < 0) ? margin : 0);
	}

	private MarginCollapsingState(MarginCollapsingState mcs)
		: base(mcs.PtsContext)
	{
		_maxPositive = mcs._maxPositive;
		_minNegative = mcs._minNegative;
	}

	internal MarginCollapsingState Clone()
	{
		return new MarginCollapsingState(this);
	}

	internal bool IsEqual(MarginCollapsingState mcs)
	{
		if (_maxPositive == mcs._maxPositive)
		{
			return _minNegative == mcs._minNegative;
		}
		return false;
	}

	internal void Collapse(MarginCollapsingState mcs)
	{
		_maxPositive = Math.Max(_maxPositive, mcs._maxPositive);
		_minNegative = Math.Min(_minNegative, mcs._minNegative);
	}
}
