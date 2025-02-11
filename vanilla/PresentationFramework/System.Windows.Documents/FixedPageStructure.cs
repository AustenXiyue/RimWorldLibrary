using System.Collections.Generic;

namespace System.Windows.Documents;

internal sealed class FixedPageStructure
{
	private readonly int _pageIndex;

	private FlowNode _flowStart;

	private FlowNode _flowEnd;

	private FixedNode _fixedStart;

	private FixedNode _fixedEnd;

	private FixedSOMPageConstructor _fixedSOMPageConstructor;

	private FixedSOMPage _fixedSOMPage;

	private FixedDSBuilder _fixedDSBuilder;

	private FixedLineResult[] _lineResults;

	internal FixedNode[] LastLine
	{
		get
		{
			if (_lineResults.Length != 0)
			{
				return _lineResults[_lineResults.Length - 1].Nodes;
			}
			return null;
		}
	}

	internal FixedNode[] FirstLine
	{
		get
		{
			if (_lineResults.Length != 0)
			{
				return _lineResults[0].Nodes;
			}
			return null;
		}
	}

	internal int PageIndex => _pageIndex;

	internal bool Loaded
	{
		get
		{
			if (_flowStart != null)
			{
				return _flowStart.Type != FlowNodeType.Virtual;
			}
			return false;
		}
	}

	internal FlowNode FlowStart => _flowStart;

	internal FlowNode FlowEnd => _flowEnd;

	internal FixedSOMPage FixedSOMPage
	{
		get
		{
			return _fixedSOMPage;
		}
		set
		{
			_fixedSOMPage = value;
		}
	}

	internal FixedDSBuilder FixedDSBuilder
	{
		get
		{
			return _fixedDSBuilder;
		}
		set
		{
			_fixedDSBuilder = value;
		}
	}

	internal FixedSOMPageConstructor PageConstructor
	{
		get
		{
			return _fixedSOMPageConstructor;
		}
		set
		{
			_fixedSOMPageConstructor = value;
		}
	}

	internal FixedPageStructure(int pageIndex)
	{
		_pageIndex = pageIndex;
		_flowStart = new FlowNode(-1, FlowNodeType.Virtual, pageIndex);
		_flowEnd = _flowStart;
		_fixedStart = FixedNode.Create(pageIndex, 1, int.MinValue, -1, null);
		_fixedEnd = FixedNode.Create(pageIndex, 1, int.MaxValue, -1, null);
	}

	internal void SetupLineResults(FixedLineResult[] lineResults)
	{
		_lineResults = lineResults;
	}

	internal FixedNode[] GetNextLine(int line, bool forward, ref int count)
	{
		if (forward)
		{
			while (line < _lineResults.Length - 1 && count > 0)
			{
				line++;
				count--;
			}
		}
		else
		{
			while (line > 0 && count > 0)
			{
				line--;
				count--;
			}
		}
		if (count <= 0)
		{
			line = Math.Max(0, Math.Min(line, _lineResults.Length - 1));
			return _lineResults[line].Nodes;
		}
		return null;
	}

	internal FixedNode[] FindSnapToLine(Point pt)
	{
		FixedLineResult fixedLineResult = null;
		FixedLineResult fixedLineResult2 = null;
		double num = double.MaxValue;
		double num2 = double.MaxValue;
		double num3 = double.MaxValue;
		FixedLineResult[] lineResults = _lineResults;
		foreach (FixedLineResult fixedLineResult3 in lineResults)
		{
			double num4 = Math.Max(0.0, (pt.Y > fixedLineResult3.LayoutBox.Y) ? (pt.Y - fixedLineResult3.LayoutBox.Bottom) : (fixedLineResult3.LayoutBox.Y - pt.Y));
			double num5 = Math.Max(0.0, (pt.X > fixedLineResult3.LayoutBox.X) ? (pt.X - fixedLineResult3.LayoutBox.Right) : (fixedLineResult3.LayoutBox.X - pt.X));
			if (num4 == 0.0 && num5 == 0.0)
			{
				return fixedLineResult3.Nodes;
			}
			if (num4 < num || (num4 == num && num5 < num2))
			{
				num = num4;
				num2 = num5;
				fixedLineResult = fixedLineResult3;
			}
			double num6 = 5.0 * num4 + num5;
			if (num6 < num3 && num4 < fixedLineResult3.LayoutBox.Height)
			{
				num3 = num6;
				fixedLineResult2 = fixedLineResult3;
			}
		}
		if (fixedLineResult != null)
		{
			if (fixedLineResult2 != null && (fixedLineResult2.LayoutBox.Left > fixedLineResult.LayoutBox.Right || fixedLineResult.LayoutBox.Left > fixedLineResult2.LayoutBox.Right))
			{
				return fixedLineResult2.Nodes;
			}
			return fixedLineResult.Nodes;
		}
		return null;
	}

	internal void SetFlowBoundary(FlowNode flowStart, FlowNode flowEnd)
	{
		_flowStart = flowStart;
		_flowEnd = flowEnd;
	}

	public void ConstructFixedSOMPage(List<FixedNode> fixedNodes)
	{
		_fixedSOMPageConstructor.ConstructPageStructure(fixedNodes);
	}
}
