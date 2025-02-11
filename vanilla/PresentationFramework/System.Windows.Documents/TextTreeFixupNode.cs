using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeFixupNode : TextTreeNode
{
	private readonly TextTreeNode _previousNode;

	private readonly ElementEdge _previousEdge;

	private readonly TextTreeNode _nextNode;

	private readonly ElementEdge _nextEdge;

	private readonly TextTreeNode _firstContainedNode;

	private readonly TextTreeNode _lastContainedNode;

	internal override SplayTreeNode ParentNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override SplayTreeNode ContainedNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override int LeftSymbolCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override int LeftCharCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override SplayTreeNode LeftChildNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override SplayTreeNode RightChildNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override uint Generation
	{
		get
		{
			return 0u;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override int SymbolOffsetCache
	{
		get
		{
			return -1;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override int SymbolCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override int IMECharCount
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(condition: false, "FixupNode");
		}
	}

	internal override bool BeforeStartReferenceCount
	{
		get
		{
			return false;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeFixupNode should never have a position reference!");
		}
	}

	internal override bool AfterStartReferenceCount
	{
		get
		{
			return false;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeFixupNode should never have a position reference!");
		}
	}

	internal override bool BeforeEndReferenceCount
	{
		get
		{
			return false;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeFixupNode should never have a position reference!");
		}
	}

	internal override bool AfterEndReferenceCount
	{
		get
		{
			return false;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeFixupNode should never have a position reference!");
		}
	}

	internal TextTreeNode PreviousNode => _previousNode;

	internal ElementEdge PreviousEdge => _previousEdge;

	internal TextTreeNode NextNode => _nextNode;

	internal ElementEdge NextEdge => _nextEdge;

	internal TextTreeNode FirstContainedNode => _firstContainedNode;

	internal TextTreeNode LastContainedNode => _lastContainedNode;

	internal TextTreeFixupNode(TextTreeNode previousNode, ElementEdge previousEdge, TextTreeNode nextNode, ElementEdge nextEdge)
		: this(previousNode, previousEdge, nextNode, nextEdge, null, null)
	{
	}

	internal TextTreeFixupNode(TextTreeNode previousNode, ElementEdge previousEdge, TextTreeNode nextNode, ElementEdge nextEdge, TextTreeNode firstContainedNode, TextTreeNode lastContainedNode)
	{
		_previousNode = previousNode;
		_previousEdge = previousEdge;
		_nextNode = nextNode;
		_nextEdge = nextEdge;
		_firstContainedNode = firstContainedNode;
		_lastContainedNode = lastContainedNode;
	}

	internal override TextTreeNode Clone()
	{
		Invariant.Assert(condition: false, "Unexpected call to TextTreeFixupNode.Clone!");
		return null;
	}

	internal override TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		Invariant.Assert(condition: false, "Unexpected call to TextTreeFixupNode.GetPointerContext!");
		return TextPointerContext.None;
	}
}
