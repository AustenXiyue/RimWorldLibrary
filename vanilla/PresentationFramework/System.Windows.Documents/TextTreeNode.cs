using MS.Internal;

namespace System.Windows.Documents;

internal abstract class TextTreeNode : SplayTreeNode
{
	internal abstract bool BeforeStartReferenceCount { get; set; }

	internal abstract bool AfterStartReferenceCount { get; set; }

	internal abstract bool BeforeEndReferenceCount { get; set; }

	internal abstract bool AfterEndReferenceCount { get; set; }

	internal abstract TextTreeNode Clone();

	internal TextContainer GetTextTree()
	{
		SplayTreeNode splayTreeNode = this;
		while (true)
		{
			SplayTreeNode containingNode = splayTreeNode.GetContainingNode();
			if (containingNode == null)
			{
				break;
			}
			splayTreeNode = containingNode;
		}
		return ((TextTreeRootNode)splayTreeNode).TextContainer;
	}

	internal DependencyObject GetDependencyParent()
	{
		SplayTreeNode splayTreeNode = this;
		DependencyObject dependencyObject;
		while (true)
		{
			if (splayTreeNode is TextTreeTextElementNode textTreeTextElementNode)
			{
				dependencyObject = textTreeTextElementNode.TextElement;
				Invariant.Assert(dependencyObject != null, "TextElementNode has null TextElement!");
				break;
			}
			SplayTreeNode containingNode = splayTreeNode.GetContainingNode();
			if (containingNode == null)
			{
				dependencyObject = ((TextTreeRootNode)splayTreeNode).TextContainer.Parent;
				break;
			}
			splayTreeNode = containingNode;
		}
		return dependencyObject;
	}

	internal DependencyObject GetLogicalTreeNode()
	{
		if (this is TextTreeObjectNode textTreeObjectNode && textTreeObjectNode.EmbeddedElement is FrameworkElement)
		{
			return textTreeObjectNode.EmbeddedElement;
		}
		SplayTreeNode splayTreeNode = this;
		while (true)
		{
			if (splayTreeNode is TextTreeTextElementNode textTreeTextElementNode)
			{
				return textTreeTextElementNode.TextElement;
			}
			SplayTreeNode containingNode = splayTreeNode.GetContainingNode();
			if (containingNode == null)
			{
				break;
			}
			splayTreeNode = containingNode;
		}
		return ((TextTreeRootNode)splayTreeNode).TextContainer.Parent;
	}

	internal abstract TextPointerContext GetPointerContext(LogicalDirection direction);

	internal TextTreeNode IncrementReferenceCount(ElementEdge edge)
	{
		return IncrementReferenceCount(edge, 1);
	}

	internal virtual TextTreeNode IncrementReferenceCount(ElementEdge edge, bool delta)
	{
		return IncrementReferenceCount(edge, delta ? 1 : 0);
	}

	internal virtual TextTreeNode IncrementReferenceCount(ElementEdge edge, int delta)
	{
		Invariant.Assert(delta >= 0);
		if (delta > 0)
		{
			switch (edge)
			{
			case ElementEdge.BeforeStart:
				BeforeStartReferenceCount = true;
				break;
			case ElementEdge.AfterStart:
				AfterStartReferenceCount = true;
				break;
			case ElementEdge.BeforeEnd:
				BeforeEndReferenceCount = true;
				break;
			case ElementEdge.AfterEnd:
				AfterEndReferenceCount = true;
				break;
			default:
				Invariant.Assert(condition: false, "Bad ElementEdge value!");
				break;
			}
		}
		return this;
	}

	internal virtual void DecrementReferenceCount(ElementEdge edge)
	{
	}

	internal void InsertAtPosition(TextPointer position)
	{
		InsertAtNode(position.Node, position.Edge);
	}

	internal ElementEdge GetEdgeFromOffsetNoBias(int nodeOffset)
	{
		return GetEdgeFromOffset(nodeOffset, LogicalDirection.Forward);
	}

	internal ElementEdge GetEdgeFromOffset(int nodeOffset, LogicalDirection bias)
	{
		if (SymbolCount == 0)
		{
			return (bias != LogicalDirection.Forward) ? ElementEdge.BeforeStart : ElementEdge.AfterEnd;
		}
		if (nodeOffset == 0)
		{
			return ElementEdge.BeforeStart;
		}
		if (nodeOffset == SymbolCount)
		{
			return ElementEdge.AfterEnd;
		}
		if (nodeOffset == 1)
		{
			return ElementEdge.AfterStart;
		}
		Invariant.Assert(nodeOffset == SymbolCount - 1);
		return ElementEdge.BeforeEnd;
	}

	internal int GetOffsetFromEdge(ElementEdge edge)
	{
		int result;
		switch (edge)
		{
		case ElementEdge.BeforeStart:
			result = 0;
			break;
		case ElementEdge.AfterStart:
			result = 1;
			break;
		case ElementEdge.BeforeEnd:
			result = SymbolCount - 1;
			break;
		case ElementEdge.AfterEnd:
			result = SymbolCount;
			break;
		default:
			result = 0;
			Invariant.Assert(condition: false, "Bad ElementEdge value!");
			break;
		}
		return result;
	}
}
