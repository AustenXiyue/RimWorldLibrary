using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal abstract class SubTreeProcessor
{
	private LocatorManager _manager;

	protected LocatorManager Manager => _manager;

	protected SubTreeProcessor(LocatorManager manager)
	{
		if (manager == null)
		{
			throw new ArgumentNullException("manager");
		}
		_manager = manager;
	}

	public abstract IList<IAttachedAnnotation> PreProcessNode(DependencyObject node, out bool calledProcessAnnotations);

	public virtual IList<IAttachedAnnotation> PostProcessNode(DependencyObject node, bool childrenCalledProcessAnnotations, out bool calledProcessAnnotations)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		calledProcessAnnotations = false;
		return null;
	}

	public abstract ContentLocator GenerateLocator(PathNode node, out bool continueGenerating);

	public abstract DependencyObject ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out bool continueResolving);

	public abstract XmlQualifiedName[] GetLocatorPartTypes();
}
