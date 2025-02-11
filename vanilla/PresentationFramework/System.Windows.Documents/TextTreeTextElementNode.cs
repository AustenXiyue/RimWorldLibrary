using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeTextElementNode : TextTreeNode
{
	private int _leftSymbolCount;

	private int _leftCharCount;

	private TextTreeNode _parentNode;

	private TextTreeNode _leftChildNode;

	private TextTreeNode _rightChildNode;

	private TextTreeNode _containedNode;

	private uint _generation;

	private int _symbolOffsetCache;

	private int _symbolCount;

	private int _imeCharCount;

	private TextElement _textElement;

	private ElementEdge _edgeReferenceCounts;

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
			return _symbolCount;
		}
		set
		{
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
			_imeCharCount = value;
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
			return (_edgeReferenceCounts & ElementEdge.AfterStart) != 0;
		}
		set
		{
			Invariant.Assert(value);
			_edgeReferenceCounts |= ElementEdge.AfterStart;
		}
	}

	internal override bool BeforeEndReferenceCount
	{
		get
		{
			return (_edgeReferenceCounts & ElementEdge.BeforeEnd) != 0;
		}
		set
		{
			Invariant.Assert(value);
			_edgeReferenceCounts |= ElementEdge.BeforeEnd;
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

	internal TextElement TextElement
	{
		get
		{
			return _textElement;
		}
		set
		{
			_textElement = value;
		}
	}

	internal int IMELeftEdgeCharCount
	{
		get
		{
			if (_textElement != null)
			{
				return _textElement.IMELeftEdgeCharCount;
			}
			return -1;
		}
	}

	internal bool IsFirstSibling
	{
		get
		{
			Splay();
			return _leftChildNode == null;
		}
	}

	internal TextTreeTextElementNode()
	{
		_symbolOffsetCache = -1;
	}

	internal override TextTreeNode Clone()
	{
		return new TextTreeTextElementNode
		{
			_symbolCount = _symbolCount,
			_imeCharCount = _imeCharCount,
			_textElement = _textElement
		};
	}

	internal override TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		if (direction != LogicalDirection.Forward)
		{
			return TextPointerContext.ElementEnd;
		}
		return TextPointerContext.ElementStart;
	}
}
