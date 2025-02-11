using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal abstract class SelectionProcessor
{
	public abstract bool MergeSelections(object selection1, object selection2, out object newSelection);

	public abstract IList<DependencyObject> GetSelectedNodes(object selection);

	public abstract UIElement GetParent(object selection);

	public abstract Point GetAnchorPoint(object selection);

	public abstract IList<ContentLocatorPart> GenerateLocatorParts(object selection, DependencyObject startNode);

	public abstract object ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out AttachmentLevel attachmentLevel);

	public abstract XmlQualifiedName[] GetLocatorPartTypes();
}
