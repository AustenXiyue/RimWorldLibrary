using MS.Internal;

namespace System.Windows.Documents;

internal abstract class SplayTreeNode
{
	internal abstract SplayTreeNode ParentNode { get; set; }

	internal abstract SplayTreeNode ContainedNode { get; set; }

	internal abstract SplayTreeNode LeftChildNode { get; set; }

	internal abstract SplayTreeNode RightChildNode { get; set; }

	internal abstract int SymbolCount { get; set; }

	internal abstract int IMECharCount { get; set; }

	internal abstract int LeftSymbolCount { get; set; }

	internal abstract int LeftCharCount { get; set; }

	internal abstract uint Generation { get; set; }

	internal abstract int SymbolOffsetCache { get; set; }

	internal SplayTreeNodeRole Role
	{
		get
		{
			SplayTreeNode parentNode = ParentNode;
			if (parentNode == null || parentNode.ContainedNode == this)
			{
				return SplayTreeNodeRole.LocalRoot;
			}
			if (parentNode.LeftChildNode == this)
			{
				return SplayTreeNodeRole.LeftChild;
			}
			Invariant.Assert(parentNode.RightChildNode == this, "Node has no relation to parent!");
			return SplayTreeNodeRole.RightChild;
		}
	}

	internal SplayTreeNode GetSiblingAtOffset(int offset, out int nodeOffset)
	{
		SplayTreeNode splayTreeNode = this;
		nodeOffset = 0;
		int leftSymbolCount;
		while (true)
		{
			leftSymbolCount = splayTreeNode.LeftSymbolCount;
			if (offset < nodeOffset + leftSymbolCount)
			{
				splayTreeNode = splayTreeNode.LeftChildNode;
				continue;
			}
			int symbolCount = splayTreeNode.SymbolCount;
			if (offset <= nodeOffset + leftSymbolCount + symbolCount)
			{
				break;
			}
			nodeOffset += leftSymbolCount + symbolCount;
			splayTreeNode = splayTreeNode.RightChildNode;
		}
		nodeOffset += leftSymbolCount;
		splayTreeNode.Splay();
		return splayTreeNode;
	}

	internal SplayTreeNode GetSiblingAtCharOffset(int charOffset, out int nodeCharOffset)
	{
		SplayTreeNode splayTreeNode = this;
		nodeCharOffset = 0;
		int leftCharCount;
		while (true)
		{
			leftCharCount = splayTreeNode.LeftCharCount;
			if (charOffset < nodeCharOffset + leftCharCount)
			{
				splayTreeNode = splayTreeNode.LeftChildNode;
				continue;
			}
			if (charOffset == nodeCharOffset + leftCharCount && charOffset > 0)
			{
				splayTreeNode = splayTreeNode.LeftChildNode;
				continue;
			}
			int iMECharCount = splayTreeNode.IMECharCount;
			if (iMECharCount > 0 && charOffset <= nodeCharOffset + leftCharCount + iMECharCount)
			{
				break;
			}
			nodeCharOffset += leftCharCount + iMECharCount;
			splayTreeNode = splayTreeNode.RightChildNode;
		}
		nodeCharOffset += leftCharCount;
		splayTreeNode.Splay();
		return splayTreeNode;
	}

	internal SplayTreeNode GetFirstContainedNode()
	{
		return ContainedNode?.GetMinSibling();
	}

	internal SplayTreeNode GetLastContainedNode()
	{
		return ContainedNode?.GetMaxSibling();
	}

	internal SplayTreeNode GetContainingNode()
	{
		Splay();
		return ParentNode;
	}

	internal SplayTreeNode GetPreviousNode()
	{
		SplayTreeNode splayTreeNode = LeftChildNode;
		if (splayTreeNode != null)
		{
			while (true)
			{
				SplayTreeNode rightChildNode = splayTreeNode.RightChildNode;
				if (rightChildNode == null)
				{
					break;
				}
				splayTreeNode = rightChildNode;
			}
		}
		else
		{
			SplayTreeNodeRole role = Role;
			splayTreeNode = ParentNode;
			while (true)
			{
				switch (role)
				{
				case SplayTreeNodeRole.LocalRoot:
					splayTreeNode = null;
					break;
				default:
					goto IL_0031;
				case SplayTreeNodeRole.RightChild:
					break;
				}
				break;
				IL_0031:
				role = splayTreeNode.Role;
				splayTreeNode = splayTreeNode.ParentNode;
			}
		}
		splayTreeNode?.Splay();
		return splayTreeNode;
	}

	internal SplayTreeNode GetNextNode()
	{
		SplayTreeNode splayTreeNode = RightChildNode;
		if (splayTreeNode != null)
		{
			while (true)
			{
				SplayTreeNode leftChildNode = splayTreeNode.LeftChildNode;
				if (leftChildNode == null)
				{
					break;
				}
				splayTreeNode = leftChildNode;
			}
		}
		else
		{
			SplayTreeNodeRole role = Role;
			splayTreeNode = ParentNode;
			while (true)
			{
				switch (role)
				{
				case SplayTreeNodeRole.LocalRoot:
					splayTreeNode = null;
					break;
				default:
					goto IL_0031;
				case SplayTreeNodeRole.LeftChild:
					break;
				}
				break;
				IL_0031:
				role = splayTreeNode.Role;
				splayTreeNode = splayTreeNode.ParentNode;
			}
		}
		splayTreeNode?.Splay();
		return splayTreeNode;
	}

	internal int GetSymbolOffset(uint treeGeneration)
	{
		int num = 0;
		SplayTreeNode splayTreeNode = this;
		while (splayTreeNode.Generation != treeGeneration || splayTreeNode.SymbolOffsetCache < 0)
		{
			splayTreeNode.Splay();
			num += splayTreeNode.LeftSymbolCount;
			num++;
			splayTreeNode = splayTreeNode.ParentNode;
		}
		num += splayTreeNode.SymbolOffsetCache;
		Generation = treeGeneration;
		SymbolOffsetCache = num;
		return num;
	}

	internal int GetIMECharOffset()
	{
		int num = 0;
		SplayTreeNode splayTreeNode = this;
		while (true)
		{
			splayTreeNode.Splay();
			num += splayTreeNode.LeftCharCount;
			splayTreeNode = splayTreeNode.ParentNode;
			if (splayTreeNode == null)
			{
				break;
			}
			if (splayTreeNode is TextTreeTextElementNode textTreeTextElementNode)
			{
				num += textTreeTextElementNode.IMELeftEdgeCharCount;
			}
		}
		return num;
	}

	internal void InsertAtNode(SplayTreeNode positionNode, ElementEdge edge)
	{
		SplayTreeNode splayTreeNode;
		bool insertBefore;
		switch (edge)
		{
		case ElementEdge.BeforeStart:
		case ElementEdge.AfterEnd:
			InsertAtNode(positionNode, edge == ElementEdge.BeforeStart);
			return;
		case ElementEdge.AfterStart:
			splayTreeNode = positionNode.GetFirstContainedNode();
			insertBefore = true;
			break;
		default:
			splayTreeNode = positionNode.GetLastContainedNode();
			insertBefore = false;
			break;
		}
		if (splayTreeNode == null)
		{
			positionNode.ContainedNode = this;
			ParentNode = positionNode;
			Invariant.Assert(LeftChildNode == null);
			Invariant.Assert(RightChildNode == null);
			Invariant.Assert(LeftSymbolCount == 0);
		}
		else
		{
			InsertAtNode(splayTreeNode, insertBefore);
		}
	}

	internal void InsertAtNode(SplayTreeNode location, bool insertBefore)
	{
		Invariant.Assert(ParentNode == null, "Can't insert child node!");
		Invariant.Assert(LeftChildNode == null, "Can't insert node with left children!");
		Invariant.Assert(RightChildNode == null, "Can't insert node with right children!");
		SplayTreeNode splayTreeNode = (insertBefore ? location.GetPreviousNode() : location);
		SplayTreeNode rightSubTree;
		SplayTreeNode parentNode;
		if (splayTreeNode != null)
		{
			rightSubTree = splayTreeNode.Split();
			parentNode = splayTreeNode.ParentNode;
		}
		else
		{
			rightSubTree = location;
			location.Splay();
			Invariant.Assert(location.Role == SplayTreeNodeRole.LocalRoot, "location should be local root!");
			parentNode = location.ParentNode;
		}
		Join(this, splayTreeNode, rightSubTree);
		ParentNode = parentNode;
		if (parentNode != null)
		{
			parentNode.ContainedNode = this;
		}
	}

	internal void Remove()
	{
		Splay();
		Invariant.Assert(Role == SplayTreeNodeRole.LocalRoot);
		SplayTreeNode parentNode = ParentNode;
		SplayTreeNode leftChildNode = LeftChildNode;
		SplayTreeNode rightChildNode = RightChildNode;
		if (leftChildNode != null)
		{
			leftChildNode.ParentNode = null;
		}
		if (rightChildNode != null)
		{
			rightChildNode.ParentNode = null;
		}
		SplayTreeNode splayTreeNode = Join(leftChildNode, rightChildNode);
		if (parentNode != null)
		{
			parentNode.ContainedNode = splayTreeNode;
		}
		if (splayTreeNode != null)
		{
			splayTreeNode.ParentNode = parentNode;
		}
		ParentNode = null;
		LeftChildNode = null;
		RightChildNode = null;
	}

	internal static void Join(SplayTreeNode root, SplayTreeNode leftSubTree, SplayTreeNode rightSubTree)
	{
		root.LeftChildNode = leftSubTree;
		root.RightChildNode = rightSubTree;
		Invariant.Assert(root.Role == SplayTreeNodeRole.LocalRoot);
		if (leftSubTree != null)
		{
			leftSubTree.ParentNode = root;
			root.LeftSymbolCount = leftSubTree.LeftSymbolCount + leftSubTree.SymbolCount;
			root.LeftCharCount = leftSubTree.LeftCharCount + leftSubTree.IMECharCount;
		}
		else
		{
			root.LeftSymbolCount = 0;
			root.LeftCharCount = 0;
		}
		if (rightSubTree != null)
		{
			rightSubTree.ParentNode = root;
		}
	}

	internal static SplayTreeNode Join(SplayTreeNode leftSubTree, SplayTreeNode rightSubTree)
	{
		Invariant.Assert(leftSubTree == null || leftSubTree.ParentNode == null);
		Invariant.Assert(rightSubTree == null || rightSubTree.ParentNode == null);
		SplayTreeNode splayTreeNode;
		if (leftSubTree != null)
		{
			splayTreeNode = leftSubTree.GetMaxSibling();
			splayTreeNode.Splay();
			Invariant.Assert(splayTreeNode.Role == SplayTreeNodeRole.LocalRoot);
			Invariant.Assert(splayTreeNode.RightChildNode == null);
			splayTreeNode.RightChildNode = rightSubTree;
			if (rightSubTree != null)
			{
				rightSubTree.ParentNode = splayTreeNode;
			}
		}
		else if (rightSubTree != null)
		{
			splayTreeNode = rightSubTree;
			Invariant.Assert(splayTreeNode.Role == SplayTreeNodeRole.LocalRoot);
		}
		else
		{
			splayTreeNode = null;
		}
		return splayTreeNode;
	}

	internal SplayTreeNode Split()
	{
		Splay();
		Invariant.Assert(Role == SplayTreeNodeRole.LocalRoot, "location should be local root!");
		SplayTreeNode rightChildNode = RightChildNode;
		if (rightChildNode != null)
		{
			rightChildNode.ParentNode = null;
			RightChildNode = null;
		}
		return rightChildNode;
	}

	internal SplayTreeNode GetMinSibling()
	{
		SplayTreeNode splayTreeNode = this;
		while (true)
		{
			SplayTreeNode leftChildNode = splayTreeNode.LeftChildNode;
			if (leftChildNode == null)
			{
				break;
			}
			splayTreeNode = leftChildNode;
		}
		splayTreeNode.Splay();
		return splayTreeNode;
	}

	internal SplayTreeNode GetMaxSibling()
	{
		SplayTreeNode splayTreeNode = this;
		while (true)
		{
			SplayTreeNode rightChildNode = splayTreeNode.RightChildNode;
			if (rightChildNode == null)
			{
				break;
			}
			splayTreeNode = rightChildNode;
		}
		splayTreeNode.Splay();
		return splayTreeNode;
	}

	internal void Splay()
	{
		while (true)
		{
			SplayTreeNodeRole role = Role;
			if (role == SplayTreeNodeRole.LocalRoot)
			{
				break;
			}
			SplayTreeNode parentNode = ParentNode;
			SplayTreeNodeRole role2 = parentNode.Role;
			if (role2 == SplayTreeNodeRole.LocalRoot)
			{
				if (role == SplayTreeNodeRole.LeftChild)
				{
					parentNode.RotateRight();
				}
				else
				{
					parentNode.RotateLeft();
				}
				break;
			}
			SplayTreeNode parentNode2 = parentNode.ParentNode;
			if (role == role2)
			{
				if (role == SplayTreeNodeRole.LeftChild)
				{
					parentNode2.RotateRight();
					parentNode.RotateRight();
				}
				else
				{
					parentNode2.RotateLeft();
					parentNode.RotateLeft();
				}
			}
			else if (role == SplayTreeNodeRole.LeftChild)
			{
				parentNode.RotateRight();
				parentNode2.RotateLeft();
			}
			else
			{
				parentNode.RotateLeft();
				parentNode2.RotateRight();
			}
		}
		Invariant.Assert(Role == SplayTreeNodeRole.LocalRoot, "Splay didn't move node to root!");
	}

	internal bool IsChildOfNode(SplayTreeNode parentNode)
	{
		if (parentNode.LeftChildNode != this && parentNode.RightChildNode != this)
		{
			return parentNode.ContainedNode == this;
		}
		return true;
	}

	private void RotateLeft()
	{
		Invariant.Assert(RightChildNode != null, "Can't rotate left with null right child!");
		SplayTreeNode rightChildNode = RightChildNode;
		RightChildNode = rightChildNode.LeftChildNode;
		if (rightChildNode.LeftChildNode != null)
		{
			rightChildNode.LeftChildNode.ParentNode = this;
		}
		SplayTreeNode splayTreeNode = (rightChildNode.ParentNode = ParentNode);
		if (splayTreeNode != null)
		{
			if (splayTreeNode.ContainedNode == this)
			{
				splayTreeNode.ContainedNode = rightChildNode;
			}
			else if (Role == SplayTreeNodeRole.LeftChild)
			{
				splayTreeNode.LeftChildNode = rightChildNode;
			}
			else
			{
				splayTreeNode.RightChildNode = rightChildNode;
			}
		}
		rightChildNode.LeftChildNode = this;
		ParentNode = rightChildNode;
		_ = RightChildNode;
		rightChildNode.LeftSymbolCount += LeftSymbolCount + SymbolCount;
		rightChildNode.LeftCharCount += LeftCharCount + IMECharCount;
	}

	private void RotateRight()
	{
		Invariant.Assert(LeftChildNode != null, "Can't rotate right with null left child!");
		SplayTreeNode leftChildNode = LeftChildNode;
		LeftChildNode = leftChildNode.RightChildNode;
		if (leftChildNode.RightChildNode != null)
		{
			leftChildNode.RightChildNode.ParentNode = this;
		}
		SplayTreeNode splayTreeNode = (leftChildNode.ParentNode = ParentNode);
		if (splayTreeNode != null)
		{
			if (splayTreeNode.ContainedNode == this)
			{
				splayTreeNode.ContainedNode = leftChildNode;
			}
			else if (Role == SplayTreeNodeRole.LeftChild)
			{
				splayTreeNode.LeftChildNode = leftChildNode;
			}
			else
			{
				splayTreeNode.RightChildNode = leftChildNode;
			}
		}
		leftChildNode.RightChildNode = this;
		ParentNode = leftChildNode;
		_ = LeftChildNode;
		LeftSymbolCount -= leftChildNode.LeftSymbolCount + leftChildNode.SymbolCount;
		LeftCharCount -= leftChildNode.LeftCharCount + leftChildNode.IMECharCount;
	}
}
