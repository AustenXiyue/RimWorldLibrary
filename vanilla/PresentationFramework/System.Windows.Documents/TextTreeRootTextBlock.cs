using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeRootTextBlock : SplayTreeNode
{
	private TextTreeTextBlock _containedNode;

	internal override SplayTreeNode ParentNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set ParentNode on TextBlock root!");
		}
	}

	internal override SplayTreeNode ContainedNode
	{
		get
		{
			return _containedNode;
		}
		set
		{
			_containedNode = (TextTreeTextBlock)value;
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
			Invariant.Assert(condition: false, "TextContainer root is never a sibling!");
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
			Invariant.Assert(value == 0);
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
			Invariant.Assert(condition: false, "TextBlock root never has sibling nodes!");
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
			Invariant.Assert(condition: false, "TextBlock root never has sibling nodes!");
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
			Invariant.Assert(condition: false, "TextTreeRootTextBlock does not track Generation!");
		}
	}

	internal override int SymbolOffsetCache
	{
		get
		{
			return 0;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeRootTextBlock does not track SymbolOffsetCache!");
		}
	}

	internal override int SymbolCount
	{
		get
		{
			return -1;
		}
		set
		{
			Invariant.Assert(condition: false, "TextTreeRootTextBlock does not track symbol count!");
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
			Invariant.Assert(value == 0);
		}
	}

	internal TextTreeRootTextBlock()
	{
		new TextTreeTextBlock(2).InsertAtNode(this, ElementEdge.AfterStart);
	}
}
