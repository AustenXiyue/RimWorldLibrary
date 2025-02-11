using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal abstract class BamlTreeNode
{
	[Flags]
	private enum BamlTreeNodeState : byte
	{
		None = 0,
		ContentFormatted = 1,
		NodeVisited = 2,
		Unidentifiable = 4
	}

	protected BamlNodeType _nodeType;

	protected List<BamlTreeNode> _children;

	protected BamlTreeNode _parent;

	private BamlTreeNodeState _state;

	internal BamlNodeType NodeType
	{
		get
		{
			return _nodeType;
		}
		set
		{
			_nodeType = value;
		}
	}

	internal List<BamlTreeNode> Children
	{
		get
		{
			return _children;
		}
		set
		{
			_children = value;
		}
	}

	internal BamlTreeNode Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	internal bool Formatted
	{
		get
		{
			return (_state & BamlTreeNodeState.ContentFormatted) != 0;
		}
		set
		{
			if (value)
			{
				_state |= BamlTreeNodeState.ContentFormatted;
			}
			else
			{
				_state &= ~BamlTreeNodeState.ContentFormatted;
			}
		}
	}

	internal bool Visited
	{
		get
		{
			return (_state & BamlTreeNodeState.NodeVisited) != 0;
		}
		set
		{
			if (value)
			{
				_state |= BamlTreeNodeState.NodeVisited;
			}
			else
			{
				_state &= ~BamlTreeNodeState.NodeVisited;
			}
		}
	}

	internal bool Unidentifiable
	{
		get
		{
			return (_state & BamlTreeNodeState.Unidentifiable) != 0;
		}
		set
		{
			if (value)
			{
				_state |= BamlTreeNodeState.Unidentifiable;
			}
			else
			{
				_state &= ~BamlTreeNodeState.Unidentifiable;
			}
		}
	}

	internal BamlTreeNode(BamlNodeType type)
	{
		NodeType = type;
	}

	internal void AddChild(BamlTreeNode child)
	{
		if (_children == null)
		{
			_children = new List<BamlTreeNode>();
		}
		_children.Add(child);
		child.Parent = this;
	}

	internal abstract BamlTreeNode Copy();

	internal abstract void Serialize(BamlWriter writer);
}
