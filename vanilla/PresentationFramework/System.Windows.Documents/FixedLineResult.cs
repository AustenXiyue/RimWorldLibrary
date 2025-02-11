namespace System.Windows.Documents;

internal sealed class FixedLineResult : IComparable
{
	private readonly FixedNode[] _nodes;

	private readonly Rect _layoutBox;

	internal FixedNode Start => _nodes[0];

	internal FixedNode End => _nodes[_nodes.Length - 1];

	internal FixedNode[] Nodes => _nodes;

	internal double BaseLine => _layoutBox.Bottom;

	internal Rect LayoutBox => _layoutBox;

	internal FixedLineResult(FixedNode[] nodes, Rect layoutBox)
	{
		_nodes = nodes;
		_layoutBox = layoutBox;
	}

	public int CompareTo(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (o.GetType() != typeof(FixedLineResult))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(FixedLineResult)), "o");
		}
		FixedLineResult fixedLineResult = (FixedLineResult)o;
		return BaseLine.CompareTo(fixedLineResult.BaseLine);
	}
}
