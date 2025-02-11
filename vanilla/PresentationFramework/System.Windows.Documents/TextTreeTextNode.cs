using MS.Internal;

namespace System.Windows.Documents;

internal class TextTreeTextNode : TextTreeNode
{
	private int _leftSymbolCount;

	private int _leftCharCount;

	private TextTreeNode _parentNode;

	private TextTreeNode _leftChildNode;

	private TextTreeNode _rightChildNode;

	private uint _generation;

	private int _symbolOffsetCache;

	private int _symbolCount;

	private int _positionRefCount;

	private ElementEdge _referencedEdge;

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
			Invariant.Assert(condition: false, "Can't set child on a TextTreeTextNode!");
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
			return SymbolCount;
		}
		set
		{
		}
	}

	internal override bool BeforeStartReferenceCount
	{
		get
		{
			if (_referencedEdge != ElementEdge.BeforeStart)
			{
				return false;
			}
			return _positionRefCount > 0;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set TextTreeTextNode ref counts directly!");
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
			Invariant.Assert(condition: false, "Text nodes don't have an AfterStart edge!");
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
			Invariant.Assert(condition: false, "Text nodes don't have a BeforeEnd edge!");
		}
	}

	internal override bool AfterEndReferenceCount
	{
		get
		{
			if (_referencedEdge != ElementEdge.AfterEnd)
			{
				return false;
			}
			return _positionRefCount > 0;
		}
		set
		{
			Invariant.Assert(condition: false, "Can't set TextTreeTextNode ref counts directly!");
		}
	}

	internal TextTreeTextNode()
	{
		_symbolOffsetCache = -1;
	}

	internal override TextTreeNode Clone()
	{
		TextTreeTextNode textTreeTextNode = null;
		if (_symbolCount > 0)
		{
			textTreeTextNode = new TextTreeTextNode();
			textTreeTextNode._symbolCount = _symbolCount;
		}
		return textTreeTextNode;
	}

	internal override TextPointerContext GetPointerContext(LogicalDirection direction)
	{
		return TextPointerContext.Text;
	}

	internal override TextTreeNode IncrementReferenceCount(ElementEdge edge, int delta)
	{
		Invariant.Assert(delta >= 0);
		Invariant.Assert(edge == ElementEdge.BeforeStart || edge == ElementEdge.AfterEnd, "Bad edge ref to TextTreeTextNode!");
		if (delta == 0)
		{
			return this;
		}
		TextTreeTextNode textTreeTextNode;
		if (_positionRefCount > 0 && edge != _referencedEdge)
		{
			textTreeTextNode = Split((edge != ElementEdge.BeforeStart) ? _symbolCount : 0, edge);
			textTreeTextNode._referencedEdge = edge;
			textTreeTextNode._positionRefCount += delta;
			TextTreeTextNode textTreeTextNode2 = ((edge != ElementEdge.BeforeStart) ? (textTreeTextNode.GetNextNode() as TextTreeTextNode) : (textTreeTextNode.GetPreviousNode() as TextTreeTextNode));
			if (textTreeTextNode2 != null && textTreeTextNode2._positionRefCount == 0)
			{
				textTreeTextNode2.Merge();
			}
		}
		else
		{
			textTreeTextNode = this;
			_referencedEdge = edge;
			_positionRefCount += delta;
		}
		return textTreeTextNode;
	}

	internal override void DecrementReferenceCount(ElementEdge edge)
	{
		Invariant.Assert(edge == _referencedEdge, "Bad edge decrement!");
		_positionRefCount--;
		Invariant.Assert(_positionRefCount >= 0, "Bogus PositionRefCount! ");
		if (_positionRefCount == 0)
		{
			Merge();
		}
	}

	internal TextTreeTextNode Split(int localOffset, ElementEdge edge)
	{
		Invariant.Assert(_symbolCount > 0, "Splitting a zero-width TextNode!");
		Invariant.Assert(localOffset >= 0 && localOffset <= _symbolCount, "Bad localOffset!");
		Invariant.Assert(edge == ElementEdge.BeforeStart || edge == ElementEdge.AfterEnd, "Bad edge parameter!");
		TextTreeTextNode textTreeTextNode = new TextTreeTextNode();
		textTreeTextNode._generation = _generation;
		Splay();
		ElementEdge edge2;
		TextTreeTextNode result;
		if (_positionRefCount > 0 && _referencedEdge == ElementEdge.BeforeStart)
		{
			textTreeTextNode._symbolOffsetCache = ((_symbolOffsetCache == -1) ? (-1) : (_symbolOffsetCache + localOffset));
			textTreeTextNode._symbolCount = _symbolCount - localOffset;
			_symbolCount = localOffset;
			edge2 = ElementEdge.AfterEnd;
			result = ((edge == ElementEdge.BeforeStart) ? this : textTreeTextNode);
		}
		else
		{
			textTreeTextNode._symbolOffsetCache = _symbolOffsetCache;
			textTreeTextNode._symbolCount = localOffset;
			_symbolOffsetCache = ((_symbolOffsetCache == -1) ? (-1) : (_symbolOffsetCache + localOffset));
			_symbolCount -= localOffset;
			edge2 = ElementEdge.BeforeStart;
			result = ((edge == ElementEdge.BeforeStart) ? textTreeTextNode : this);
		}
		Invariant.Assert(_symbolCount >= 0);
		Invariant.Assert(textTreeTextNode._symbolCount >= 0);
		textTreeTextNode.InsertAtNode(this, edge2);
		return result;
	}

	private void Merge()
	{
		Invariant.Assert(_positionRefCount == 0, "Inappropriate Merge call!");
		TextTreeTextNode textTreeTextNode = GetPreviousNode() as TextTreeTextNode;
		if (textTreeTextNode != null && (textTreeTextNode._positionRefCount == 0 || textTreeTextNode._referencedEdge == ElementEdge.BeforeStart))
		{
			Remove();
			_parentNode = null;
			textTreeTextNode.Splay();
			textTreeTextNode._symbolCount += _symbolCount;
		}
		else
		{
			textTreeTextNode = this;
		}
		if (!(textTreeTextNode.GetNextNode() is TextTreeTextNode textTreeTextNode2))
		{
			return;
		}
		if (textTreeTextNode._positionRefCount == 0 && (textTreeTextNode2._positionRefCount == 0 || textTreeTextNode2._referencedEdge == ElementEdge.AfterEnd))
		{
			textTreeTextNode.Remove();
			textTreeTextNode._parentNode = null;
			textTreeTextNode2.Splay();
			if (textTreeTextNode2._symbolOffsetCache != -1)
			{
				textTreeTextNode2._symbolOffsetCache -= textTreeTextNode._symbolCount;
			}
			textTreeTextNode2._symbolCount += textTreeTextNode._symbolCount;
		}
		else if ((textTreeTextNode._positionRefCount == 0 || textTreeTextNode._referencedEdge == ElementEdge.BeforeStart) && textTreeTextNode2._positionRefCount == 0)
		{
			textTreeTextNode2.Remove();
			textTreeTextNode2._parentNode = null;
			textTreeTextNode.Splay();
			textTreeTextNode._symbolCount += textTreeTextNode2._symbolCount;
		}
	}
}
