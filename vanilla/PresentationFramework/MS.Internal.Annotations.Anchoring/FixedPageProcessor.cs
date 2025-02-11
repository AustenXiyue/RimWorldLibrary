using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal class FixedPageProcessor : SubTreeProcessor
{
	public static readonly string Id = "FixedPage";

	private static readonly string ValueAttributeName = "Value";

	private static readonly XmlQualifiedName PageNumberElementName = new XmlQualifiedName("PageNumber", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	private static readonly XmlQualifiedName[] LocatorPartTypeNames = new XmlQualifiedName[1] { PageNumberElementName };

	private bool _useLogicalTree;

	internal bool UseLogicalTree
	{
		set
		{
			_useLogicalTree = value;
		}
	}

	public FixedPageProcessor(LocatorManager manager)
		: base(manager)
	{
	}

	public override IList<IAttachedAnnotation> PreProcessNode(DependencyObject node, out bool calledProcessAnnotations)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (node is DocumentPageView documentPageView && (documentPageView.DocumentPage is FixedDocumentPage || documentPageView.DocumentPage is FixedDocumentSequenceDocumentPage))
		{
			calledProcessAnnotations = true;
			return base.Manager.ProcessAnnotations(documentPageView);
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
		DocumentPageView documentPageView = node.Node as DocumentPageView;
		int num = -1;
		if (documentPageView != null)
		{
			if (documentPageView.DocumentPage is FixedDocumentPage || documentPageView.DocumentPage is FixedDocumentSequenceDocumentPage)
			{
				num = documentPageView.PageNumber;
			}
		}
		else if (node.Node is FixedTextSelectionProcessor.FixedPageProxy fixedPageProxy)
		{
			num = fixedPageProxy.Page;
		}
		if (num >= 0)
		{
			contentLocator = new ContentLocator();
			ContentLocatorPart item = CreateLocatorPart(num);
			contentLocator.Parts.Add(item);
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
		if (PageNumberElementName != locatorPart.PartType)
		{
			throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
		}
		continueResolving = true;
		int num = 0;
		string text = locatorPart.NameValuePairs[ValueAttributeName];
		if (text != null)
		{
			num = int.Parse(text, NumberFormatInfo.InvariantInfo);
			FixedDocumentPage fixedDocumentPage = null;
			IDocumentPaginatorSource documentPaginatorSource = null;
			DocumentPageView documentPageView = null;
			if (_useLogicalTree)
			{
				documentPaginatorSource = startNode as FixedDocument;
				if (documentPaginatorSource != null)
				{
					fixedDocumentPage = documentPaginatorSource.DocumentPaginator.GetPage(num) as FixedDocumentPage;
				}
				else
				{
					documentPaginatorSource = startNode as FixedDocumentSequence;
					if (documentPaginatorSource != null && documentPaginatorSource.DocumentPaginator.GetPage(num) is FixedDocumentSequenceDocumentPage fixedDocumentSequenceDocumentPage)
					{
						fixedDocumentPage = fixedDocumentSequenceDocumentPage.ChildDocumentPage as FixedDocumentPage;
					}
				}
			}
			else if (startNode is DocumentPageView documentPageView2)
			{
				fixedDocumentPage = documentPageView2.DocumentPage as FixedDocumentPage;
				if (fixedDocumentPage == null && documentPageView2.DocumentPage is FixedDocumentSequenceDocumentPage fixedDocumentSequenceDocumentPage2)
				{
					fixedDocumentPage = fixedDocumentSequenceDocumentPage2.ChildDocumentPage as FixedDocumentPage;
				}
				if (fixedDocumentPage != null && documentPageView2.PageNumber != num)
				{
					continueResolving = false;
					fixedDocumentPage = null;
				}
			}
			return fixedDocumentPage?.FixedPage;
		}
		throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}

	internal static ContentLocatorPart CreateLocatorPart(int page)
	{
		return new ContentLocatorPart(PageNumberElementName)
		{
			NameValuePairs = { 
			{
				ValueAttributeName,
				page.ToString(NumberFormatInfo.InvariantInfo)
			} }
		};
	}
}
