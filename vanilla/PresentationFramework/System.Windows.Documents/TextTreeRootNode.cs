using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeRootNode : TextTreeNode
{
	private readonly TextContainer _tree;

	private TextTreeNode _containedNode;

	private int _symbolCount;

	private int _imeCharCount;

	private uint _generation;

	private uint _positionGeneration;

	private uint _layoutGeneration;

	private TextTreeRootTextBlock _rootTextBlock;

	private bool _caretUnitBoundaryCache;

	private int _caretUnitBoundaryCacheOffset;

	private DispatcherProcessingDisabled _processingDisabled;

	internal override SplayTreeNode ParentNode
	{
		get
		{
			return null;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set ParentNode on TextContainer root!");
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
			_containedNode = (TextTreeNode)value;
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
			Invariant.Assert(condition: false, "TextContainer root is never a sibling!");
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
			Invariant.Assert(condition: false, "TextContainer root never has sibling nodes!");
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
			Invariant.Assert(condition: false, "TextContainer root never has sibling nodes!");
		}
	}

	internal override uint Generation
	{
		get
		{
			return _generation;
		}
		set
		{
			_generation = value;
		}
	}

	internal uint PositionGeneration
	{
		get
		{
			return _positionGeneration;
		}
		set
		{
			_positionGeneration = value;
		}
	}

	internal uint LayoutGeneration
	{
		get
		{
			return _layoutGeneration;
		}
		set
		{
			_layoutGeneration = value;
			_caretUnitBoundaryCacheOffset = -1;
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
			Invariant.Assert(value == 0, "Bad SymbolOffsetCache on TextContainer root!");
		}
	}

	internal override int SymbolCount
	{
		get
		{
			return _symbolCount;
		}
		set
		{
			Invariant.Assert(value >= 2, "Bad _symbolCount on TextContainer root!");
			_symbolCount = value;
		}
	}

	internal override int IMECharCount
	{
		get
		{
			return _imeCharCount;
		}
		set
		{
			Invariant.Assert(value >= 0, "IMECharCount may never be negative!");
			_imeCharCount = value;
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
			Invariant.Assert(!value, "Root node BeforeStart edge can never be referenced!");
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
			Invariant.Assert(!value, "Root node AfterEnd edge can never be referenced!");
		}
	}

	internal TextContainer TextContainer => _tree;

	internal TextTreeRootTextBlock RootTextBlock
	{
		get
		{
			return _rootTextBlock;
		}
		set
		{
			_rootTextBlock = value;
		}
	}

	internal DispatcherProcessingDisabled DispatcherProcessingDisabled
	{
		get
		{
			return _processingDisabled;
		}
		set
		{
			_processingDisabled = value;
		}
	}

	internal bool CaretUnitBoundaryCache
	{
		get
		{
			return _caretUnitBoundaryCache;
		}
		set
		{
			_caretUnitBoundaryCache = value;
		}
	}

	internal int CaretUnitBoundaryCacheOffset
	{
		get
		{
			return _caretUnitBoundaryCacheOffset;
		}
		set
		{
			_caretUnitBoundaryCacheOffset = value;
		}
	}

	internal TextTreeRootNode(TextContainer tree)
	{
		_tree = tree;
		_symbolCount = 2;
		_caretUnitBoundaryCacheOffset = -1;
	}

	internal override TextTreeNode Clone()
	{
		Invariant.Assert(condition: false, "Unexpected call to TextTreeRootNode.Clone!");
		return null;
	}

	internal override TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		return TextPointerContext.None;
	}
}
