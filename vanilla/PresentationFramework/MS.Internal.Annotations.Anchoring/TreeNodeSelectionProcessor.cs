using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Media;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal sealed class TreeNodeSelectionProcessor : SelectionProcessor
{
	private static readonly XmlQualifiedName[] LocatorPartTypeNames = Array.Empty<XmlQualifiedName>();

	public override bool MergeSelections(object selection1, object selection2, out object newSelection)
	{
		if (selection1 == null)
		{
			throw new ArgumentNullException("selection1");
		}
		if (selection2 == null)
		{
			throw new ArgumentNullException("selection2");
		}
		newSelection = null;
		return false;
	}

	public override IList<DependencyObject> GetSelectedNodes(object selection)
	{
		return new DependencyObject[1] { GetParent(selection) };
	}

	public override UIElement GetParent(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		return (selection as UIElement) ?? throw new ArgumentException(SR.WrongSelectionType, "selection");
	}

	public override Point GetAnchorPoint(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		Rect visualContentBounds = ((selection as Visual) ?? throw new ArgumentException(SR.WrongSelectionType, "selection")).VisualContentBounds;
		return new Point(visualContentBounds.Left, visualContentBounds.Top);
	}

	public override IList<ContentLocatorPart> GenerateLocatorParts(object selection, DependencyObject startNode)
	{
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		return new List<ContentLocatorPart>(0);
	}

	public override object ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out AttachmentLevel attachmentLevel)
	{
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		attachmentLevel = AttachmentLevel.Full;
		return startNode;
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}
}
