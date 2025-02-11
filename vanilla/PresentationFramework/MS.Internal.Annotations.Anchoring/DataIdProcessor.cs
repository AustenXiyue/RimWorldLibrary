using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Annotations;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal sealed class DataIdProcessor : SubTreeProcessor
{
	public const string Id = "Id";

	public static readonly DependencyProperty DataIdProperty = DependencyProperty.RegisterAttached("DataId", typeof(string), typeof(DataIdProcessor), new PropertyMetadata(null, OnDataIdPropertyChanged, CoerceDataId));

	public static readonly DependencyProperty FetchAnnotationsAsBatchProperty = DependencyProperty.RegisterAttached("FetchAnnotationsAsBatch", typeof(bool), typeof(DataIdProcessor), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	private static readonly XmlQualifiedName DataIdElementName = new XmlQualifiedName("DataId", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	private const string ValueAttributeName = "Value";

	private static readonly XmlQualifiedName[] LocatorPartTypeNames = new XmlQualifiedName[1] { DataIdElementName };

	public DataIdProcessor(LocatorManager manager)
		: base(manager)
	{
	}

	public override IList<IAttachedAnnotation> PreProcessNode(DependencyObject node, out bool calledProcessAnnotations)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		object obj = node.ReadLocalValue(DataIdProperty);
		if ((bool)node.GetValue(FetchAnnotationsAsBatchProperty) && obj != DependencyProperty.UnsetValue)
		{
			calledProcessAnnotations = true;
			return base.Manager.ProcessAnnotations(node);
		}
		calledProcessAnnotations = false;
		return null;
	}

	public override IList<IAttachedAnnotation> PostProcessNode(DependencyObject node, bool childrenCalledProcessAnnotations, out bool calledProcessAnnotations)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		object obj = node.ReadLocalValue(DataIdProperty);
		if (!(bool)node.GetValue(FetchAnnotationsAsBatchProperty) && !childrenCalledProcessAnnotations && obj != DependencyProperty.UnsetValue)
		{
			FrameworkElement frameworkElement = null;
			if (node is FrameworkElement frameworkElement2)
			{
				frameworkElement = frameworkElement2.Parent as FrameworkElement;
			}
			AnnotationService service = AnnotationService.GetService(node);
			if (service != null && (service.Root == node || (frameworkElement != null && service.Root == frameworkElement.TemplatedParent)))
			{
				calledProcessAnnotations = true;
				return base.Manager.ProcessAnnotations(node);
			}
		}
		calledProcessAnnotations = false;
		return null;
	}

	public override ContentLocator GenerateLocator(PathNode node, out bool continueGenerating)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		continueGenerating = true;
		ContentLocator contentLocator = null;
		ContentLocatorPart contentLocatorPart = CreateLocatorPart(node.Node);
		if (contentLocatorPart != null)
		{
			contentLocator = new ContentLocator();
			contentLocator.Parts.Add(contentLocatorPart);
		}
		return contentLocator;
	}

	public override DependencyObject ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out bool continueResolving)
	{
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		if (DataIdElementName != locatorPart.PartType)
		{
			throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
		}
		continueResolving = true;
		string text = locatorPart.NameValuePairs["Value"];
		if (text == null)
		{
			throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
		}
		string nodeId = GetNodeId(startNode);
		if (nodeId != null)
		{
			if (nodeId.Equals(text))
			{
				return startNode;
			}
			continueResolving = false;
		}
		return null;
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}

	public static void SetDataId(DependencyObject d, string id)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		d.SetValue(DataIdProperty, id);
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static string GetDataId(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return d.GetValue(DataIdProperty) as string;
	}

	public static void SetFetchAnnotationsAsBatch(DependencyObject d, bool id)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		d.SetValue(FetchAnnotationsAsBatchProperty, id);
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static bool GetFetchAnnotationsAsBatch(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return (bool)d.GetValue(FetchAnnotationsAsBatchProperty);
	}

	private static void OnDataIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		string a = (string)e.OldValue;
		string b = (string)e.NewValue;
		if (!string.Equals(a, b))
		{
			AnnotationService service = AnnotationService.GetService(d);
			if (service != null && service.IsEnabled)
			{
				service.UnloadAnnotations(d);
				service.LoadAnnotations(d);
			}
		}
	}

	private static object CoerceDataId(DependencyObject d, object value)
	{
		string text = (string)value;
		if (text == null || text.Length != 0)
		{
			return value;
		}
		return null;
	}

	private ContentLocatorPart CreateLocatorPart(DependencyObject node)
	{
		string nodeId = GetNodeId(node);
		if (nodeId == null || nodeId.Length == 0)
		{
			return null;
		}
		return new ContentLocatorPart(DataIdElementName)
		{
			NameValuePairs = { { "Value", nodeId } }
		};
	}

	internal string GetNodeId(DependencyObject d)
	{
		string text = d.GetValue(DataIdProperty) as string;
		if (string.IsNullOrEmpty(text))
		{
			text = null;
		}
		return text;
	}
}
