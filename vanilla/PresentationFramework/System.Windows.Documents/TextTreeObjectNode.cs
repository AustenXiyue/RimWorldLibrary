using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeObjectNode : TextTreeNode
{
	private int _leftSymbolCount;

	private int _leftCharCount;

	private TextTreeNode _parentNode;

	private TextTreeNode _leftChildNode;

	private TextTreeNode _rightChildNode;

	private uint _generation;

	private int _symbolOffsetCache;

	private ElementEdge _edgeReferenceCounts;

	private readonly DependencyObject _embeddedElement;

	internal override SplayTreeNode ParentNode
	{
		get
		{
			return _parentNode;
		}
		set
		{
			_parentNode = (TextTreeNode)value;
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
			Invariant.Assert(condition: false, "Can't set contained node on a TextTreeObjectNode!");
		}
	}

	internal override int LeftSymbolCount
	{
		get
		{
			return _leftSymbolCount;
		}
		set
		{
			_leftSymbolCount = value;
		}
	}

	internal override int LeftCharCount
	{
		get
		{
			return _leftCharCount;
		}
		set
		{
			_leftCharCount = value;
		}
	}

	internal override SplayTreeNode LeftChildNode
	{
		get
		{
			return _leftChildNode;
		}
		set
		{
			_leftChildNode = (TextTreeNode)value;
		}
	}

	internal override SplayTreeNode RightChildNode
	{
		get
		{
			return _rightChildNode;
		}
		set
		{
			_rightChildNode = (TextTreeNode)value;
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

	internal override int SymbolOffsetCache
	{
		get
		{
			return _symbolOffsetCache;
		}
		set
		{
			_symbolOffsetCache = value;
		}
	}

	internal override int SymbolCount
	{
		get
		{
			return 1;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set SymbolCount on TextTreeObjectNode!");
		}
	}

	internal override int IMECharCount
	{
		get
		{
			return 1;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set CharCount on TextTreeObjectNode!");
		}
	}

	internal override bool BeforeStartReferenceCount
	{
		get
		{
			return (_edgeReferenceCounts & ElementEdge.BeforeStart) != 0;
		}
		set
		{
			Invariant.Assert(value);
			_edgeReferenceCounts |= ElementEdge.BeforeStart;
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
			Invariant.Assert(condition: false, "Object nodes don't have an AfterStart edge!");
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
			Invariant.Assert(condition: false, "Object nodes don't have a BeforeEnd edge!");
		}
	}

	internal override bool AfterEndReferenceCount
	{
		get
		{
			return (_edgeReferenceCounts & ElementEdge.AfterEnd) != 0;
		}
		set
		{
			Invariant.Assert(value);
			_edgeReferenceCounts |= ElementEdge.AfterEnd;
		}
	}

	internal DependencyObject EmbeddedElement => _embeddedElement;

	internal TextTreeObjectNode(DependencyObject embeddedElement)
	{
		_embeddedElement = embeddedElement;
		_symbolOffsetCache = -1;
	}

	internal override TextTreeNode Clone()
	{
		return new TextTreeObjectNode(_embeddedElement);
	}

	internal override TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		return TextPointerContext.EmbeddedElement;
	}
}
