namespace System.Windows.Documents;

internal struct FixedPosition
{
	private readonly FixedNode _fixedNode;

	private readonly int _offset;

	internal int Page => _fixedNode.Page;

	internal FixedNode Node => _fixedNode;

	internal int Offset => _offset;

	internal FixedPosition(FixedNode fixedNode, int offset)
	{
		_fixedNode = fixedNode;
		_offset = offset;
	}
}
