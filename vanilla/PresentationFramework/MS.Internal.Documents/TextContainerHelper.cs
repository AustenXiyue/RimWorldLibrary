using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal static class TextContainerHelper
{
	internal static int ElementEdgeCharacterLength = 1;

	internal static int EmbeddedObjectLength => 1;

	internal static List<AutomationPeer> GetAutomationPeersFromRange(ITextPointer start, ITextPointer end, ITextPointer ownerContentStart)
	{
		AutomationPeer automationPeer = null;
		List<AutomationPeer> list = new List<AutomationPeer>();
		start = start.CreatePointer();
		while (start.CompareTo(end) < 0)
		{
			bool flag = false;
			if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
			{
				object adjacentElement = start.GetAdjacentElement(LogicalDirection.Forward);
				if (adjacentElement is ContentElement)
				{
					automationPeer = ContentElementAutomationPeer.CreatePeerForElement((ContentElement)adjacentElement);
					if (automationPeer != null)
					{
						if (ownerContentStart == null || IsImmediateAutomationChild(start, ownerContentStart))
						{
							list.Add(automationPeer);
						}
						start.MoveToNextContextPosition(LogicalDirection.Forward);
						start.MoveToElementEdge(ElementEdge.AfterEnd);
						flag = true;
					}
				}
			}
			else if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement)
			{
				object adjacentElement = start.GetAdjacentElement(LogicalDirection.Forward);
				if (adjacentElement is UIElement)
				{
					if (ownerContentStart == null || IsImmediateAutomationChild(start, ownerContentStart))
					{
						automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)adjacentElement);
						if (automationPeer != null)
						{
							list.Add(automationPeer);
						}
						else
						{
							iterate((Visual)adjacentElement, list);
						}
					}
				}
				else if (adjacentElement is ContentElement)
				{
					automationPeer = ContentElementAutomationPeer.CreatePeerForElement((ContentElement)adjacentElement);
					if (automationPeer != null && (ownerContentStart == null || IsImmediateAutomationChild(start, ownerContentStart)))
					{
						list.Add(automationPeer);
					}
				}
			}
			if (!flag && !start.MoveToNextContextPosition(LogicalDirection.Forward))
			{
				break;
			}
		}
		return list;
	}

	internal static bool IsImmediateAutomationChild(ITextPointer elementStart, ITextPointer ownerContentStart)
	{
		Invariant.Assert(elementStart.CompareTo(ownerContentStart) >= 0);
		bool result = true;
		ITextPointer textPointer = elementStart.CreatePointer();
		while (typeof(TextElement).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
			if (textPointer.CompareTo(ownerContentStart) <= 0)
			{
				break;
			}
			AutomationPeer automationPeer = null;
			object adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Forward);
			if (adjacentElement is UIElement)
			{
				automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)adjacentElement);
			}
			else if (adjacentElement is ContentElement)
			{
				automationPeer = ContentElementAutomationPeer.CreatePeerForElement((ContentElement)adjacentElement);
			}
			if (automationPeer != null)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal static AutomationPeer GetEnclosingAutomationPeer(ITextPointer start, ITextPointer end, out ITextPointer elementStart, out ITextPointer elementEnd)
	{
		List<object> list = new List<object>();
		List<ITextPointer> list2 = new List<ITextPointer>();
		ITextPointer textPointer = start.CreatePointer();
		while (typeof(TextElement).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.BeforeStart);
			object adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Forward);
			if (adjacentElement != null)
			{
				list.Insert(0, adjacentElement);
				list2.Insert(0, textPointer.CreatePointer(LogicalDirection.Forward));
			}
		}
		List<object> list3 = new List<object>();
		List<ITextPointer> list4 = new List<ITextPointer>();
		textPointer = end.CreatePointer();
		while (typeof(TextElement).IsAssignableFrom(textPointer.ParentType))
		{
			textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
			object adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Backward);
			if (adjacentElement != null)
			{
				list3.Insert(0, adjacentElement);
				list4.Insert(0, textPointer.CreatePointer(LogicalDirection.Backward));
			}
		}
		AutomationPeer automationPeer = null;
		elementStart = (elementEnd = null);
		for (int num = Math.Min(list.Count, list3.Count); num > 0; num--)
		{
			if (list[num - 1] == list3[num - 1])
			{
				object adjacentElement = list[num - 1];
				if (adjacentElement is UIElement)
				{
					automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)adjacentElement);
				}
				else if (adjacentElement is ContentElement)
				{
					automationPeer = ContentElementAutomationPeer.CreatePeerForElement((ContentElement)adjacentElement);
				}
				if (automationPeer != null)
				{
					elementStart = list2[num - 1];
					elementEnd = list4[num - 1];
					break;
				}
			}
		}
		return automationPeer;
	}

	internal static TextContentRange GetTextContentRangeForTextElement(TextElement textElement)
	{
		ITextContainer textContainer = textElement.TextContainer;
		int elementStartOffset = textElement.ElementStartOffset;
		int elementEndOffset = textElement.ElementEndOffset;
		return new TextContentRange(elementStartOffset, elementEndOffset, textContainer);
	}

	internal static TextContentRange GetTextContentRangeForTextElementEdge(TextElement textElement, ElementEdge edge)
	{
		Invariant.Assert(edge == ElementEdge.BeforeStart || edge == ElementEdge.AfterEnd);
		ITextContainer textContainer = textElement.TextContainer;
		int cpFirst;
		int cpLast;
		if (edge == ElementEdge.AfterEnd)
		{
			cpFirst = textElement.ContentEndOffset;
			cpLast = textElement.ElementEndOffset;
		}
		else
		{
			cpFirst = textElement.ElementStartOffset;
			cpLast = textElement.ContentStartOffset;
		}
		return new TextContentRange(cpFirst, cpLast, textContainer);
	}

	internal static ITextPointer GetContentStart(ITextContainer textContainer, DependencyObject element)
	{
		if (element is TextElement)
		{
			return ((TextElement)element).ContentStart;
		}
		Invariant.Assert(element is TextBlock || element is FlowDocument || element is TextBox, "Cannot retrive ContentStart position for EmbeddedObject.");
		return textContainer.CreatePointerAtOffset(0, LogicalDirection.Forward);
	}

	internal static int GetElementLength(ITextContainer textContainer, DependencyObject element)
	{
		if (element is TextElement)
		{
			return ((TextElement)element).SymbolCount;
		}
		Invariant.Assert(element is TextBlock || element is FlowDocument || element is TextBox, "Cannot retrive length for EmbeddedObject.");
		return textContainer.SymbolCount;
	}

	internal static ITextPointer GetTextPointerFromCP(ITextContainer textContainer, int cp, LogicalDirection direction)
	{
		return textContainer.CreatePointerAtOffset(cp, direction);
	}

	internal static StaticTextPointer GetStaticTextPointerFromCP(ITextContainer textContainer, int cp)
	{
		return textContainer.CreateStaticPointerAtOffset(cp);
	}

	internal static ITextPointer GetTextPointerForEmbeddedObject(FrameworkElement embeddedObject)
	{
		if (embeddedObject.Parent is TextElement textElement)
		{
			return textElement.ContentStart;
		}
		Invariant.Assert(condition: false, "Embedded object needs to have InlineUIContainer or BlockUIContainer as parent.");
		return null;
	}

	internal static int GetCPFromElement(ITextContainer textContainer, DependencyObject element, ElementEdge edge)
	{
		if (element is TextElement textElement)
		{
			if (!textElement.IsInTree || textElement.TextContainer != textContainer)
			{
				return textContainer.SymbolCount;
			}
			switch (edge)
			{
			case ElementEdge.BeforeStart:
				return textElement.ElementStartOffset;
			case ElementEdge.AfterStart:
				return textElement.ContentStartOffset;
			case ElementEdge.BeforeEnd:
				return textElement.ContentEndOffset;
			case ElementEdge.AfterEnd:
				return textElement.ElementEndOffset;
			default:
				Invariant.Assert(condition: false, "Unknown ElementEdge.");
				return 0;
			}
		}
		Invariant.Assert(element is TextBlock || element is FlowDocument || element is TextBox, "Cannot retrive length for EmbeddedObject.");
		return (edge != ElementEdge.BeforeStart && edge != ElementEdge.AfterStart) ? textContainer.SymbolCount : 0;
	}

	internal static int GetCchFromElement(ITextContainer textContainer, DependencyObject element)
	{
		if (!(element is TextElement { SymbolCount: var symbolCount }))
		{
			return textContainer.SymbolCount;
		}
		return symbolCount;
	}

	internal static int GetCPFromEmbeddedObject(UIElement embeddedObject, ElementEdge edge)
	{
		Invariant.Assert(edge == ElementEdge.BeforeStart || edge == ElementEdge.AfterEnd, "Cannot retrieve CP from the content of embedded object.");
		int result = -1;
		if (embeddedObject is FrameworkElement)
		{
			FrameworkElement frameworkElement = (FrameworkElement)embeddedObject;
			if (frameworkElement.Parent is TextElement)
			{
				TextElement textElement = (TextElement)frameworkElement.Parent;
				result = ((edge == ElementEdge.BeforeStart) ? textElement.ContentStartOffset : textElement.ContentEndOffset);
			}
		}
		return result;
	}

	private static void iterate(Visual parent, List<AutomationPeer> peers)
	{
		AutomationPeer automationPeer = null;
		int internalVisualChildrenCount = parent.InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			Visual visual = parent.InternalGetVisualChild(i);
			if (visual != null && visual.CheckFlagsAnd(VisualFlags.IsUIElement) && (automationPeer = UIElementAutomationPeer.CreatePeerForElement((UIElement)visual)) != null)
			{
				peers.Add(automationPeer);
			}
			else
			{
				iterate(visual, peers);
			}
		}
	}
}
