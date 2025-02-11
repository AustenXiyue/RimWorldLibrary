using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Documents;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal class TextViewSelectionProcessor : SelectionProcessor
{
	private static readonly XmlQualifiedName[] LocatorPartTypeNames = Array.Empty<XmlQualifiedName>();

	public override bool MergeSelections(object selection1, object selection2, out object newSelection)
	{
		newSelection = null;
		return false;
	}

	public override IList<DependencyObject> GetSelectedNodes(object selection)
	{
		VerifySelection(selection);
		return new DependencyObject[1] { (DependencyObject)selection };
	}

	public override UIElement GetParent(object selection)
	{
		VerifySelection(selection);
		return (UIElement)selection;
	}

	public override Point GetAnchorPoint(object selection)
	{
		VerifySelection(selection);
		return new Point(double.NaN, double.NaN);
	}

	public override IList<ContentLocatorPart> GenerateLocatorParts(object selection, DependencyObject startNode)
	{
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		ITextView textView = VerifySelection(selection);
		List<ContentLocatorPart> list = new List<ContentLocatorPart>(1);
		int startOffset;
		int endOffset;
		if (textView != null && textView.IsValid)
		{
			GetTextViewTextRange(textView, out startOffset, out endOffset);
		}
		else
		{
			startOffset = -1;
			endOffset = -1;
		}
		list.Add(new ContentLocatorPart(TextSelectionProcessor.CharacterRangeElementName)
		{
			NameValuePairs = 
			{
				{
					"Count",
					1.ToString(NumberFormatInfo.InvariantInfo)
				},
				{
					"Segment" + 0.ToString(NumberFormatInfo.InvariantInfo),
					startOffset.ToString(NumberFormatInfo.InvariantInfo) + "," + endOffset.ToString(NumberFormatInfo.InvariantInfo)
				},
				{
					"IncludeOverlaps",
					bool.TrueString
				}
			}
		});
		return list;
	}

	public override object ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out AttachmentLevel attachmentLevel)
	{
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		attachmentLevel = AttachmentLevel.Unresolved;
		return null;
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}

	internal static TextRange GetTextViewTextRange(ITextView textView, out int startOffset, out int endOffset)
	{
		startOffset = int.MinValue;
		endOffset = 0;
		TextRange result = null;
		IList<TextSegment> textSegments = textView.TextSegments;
		if (textSegments != null && textSegments.Count > 0)
		{
			ITextPointer start = textSegments[0].Start;
			ITextPointer end = textSegments[textSegments.Count - 1].End;
			startOffset = end.TextContainer.Start.GetOffsetToPosition(start);
			endOffset = end.TextContainer.Start.GetOffsetToPosition(end);
			result = new TextRange(start, end);
		}
		return result;
	}

	private ITextView VerifySelection(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		return ((selection as IServiceProvider) ?? throw new ArgumentException(SR.SelectionMustBeServiceProvider, "selection")).GetService(typeof(ITextView)) as ITextView;
	}
}
