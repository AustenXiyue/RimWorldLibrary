using System.Collections.Generic;

namespace MS.Internal.Globalization;

internal sealed class BamlTree
{
	private BamlTreeNode _root;

	private List<BamlTreeNode> _nodeList;

	internal BamlTreeNode Root => _root;

	internal int Size => _nodeList.Count;

	internal BamlTreeNode this[int i] => _nodeList[i];

	internal BamlTree()
	{
	}

	internal BamlTree(BamlTreeNode root, int size)
	{
		_root = root;
		_nodeList = new List<BamlTreeNode>(size);
		CreateInternalIndex(ref _root, ref _nodeList, toCopy: false);
	}

	internal BamlTree Copy()
	{
		BamlTreeNode parent = _root;
		List<BamlTreeNode> nodeList = new List<BamlTreeNode>(Size);
		CreateInternalIndex(ref parent, ref nodeList, toCopy: true);
		return new BamlTree
		{
			_root = parent,
			_nodeList = nodeList
		};
	}

	internal void AddTreeNode(BamlTreeNode node)
	{
		_nodeList.Add(node);
	}

	private void CreateInternalIndex(ref BamlTreeNode parent, ref List<BamlTreeNode> nodeList, bool toCopy)
	{
		List<BamlTreeNode> children = parent.Children;
		if (toCopy)
		{
			parent = parent.Copy();
			if (children != null)
			{
				parent.Children = new List<BamlTreeNode>(children.Count);
			}
		}
		nodeList.Add(parent);
		if (children == null)
		{
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			BamlTreeNode parent2 = children[i];
			CreateInternalIndex(ref parent2, ref nodeList, toCopy);
			if (toCopy)
			{
				parent2.Parent = parent;
				parent.Children.Add(parent2);
			}
		}
	}
}
