using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal sealed class TextSelectionProcessor : SelectionProcessor
{
	internal const string SegmentAttribute = "Segment";

	internal const string CountAttribute = "Count";

	internal const string IncludeOverlaps = "IncludeOverlaps";

	internal const char Separator = ',';

	internal static readonly XmlQualifiedName CharacterRangeElementName = new XmlQualifiedName("CharacterRange", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	private static readonly XmlQualifiedName[] LocatorPartTypeNames = new XmlQualifiedName[1] { CharacterRangeElementName };

	private DocumentPageView _targetPage;

	private bool _clamping = true;

	internal bool Clamping
	{
		set
		{
			_clamping = value;
		}
	}

	public override bool MergeSelections(object anchor1, object anchor2, out object newAnchor)
	{
		return TextSelectionHelper.MergeSelections(anchor1, anchor2, out newAnchor);
	}

	public override IList<DependencyObject> GetSelectedNodes(object selection)
	{
		return TextSelectionHelper.GetSelectedNodes(selection);
	}

	public override UIElement GetParent(object selection)
	{
		return TextSelectionHelper.GetParent(selection);
	}

	public override Point GetAnchorPoint(object selection)
	{
		return TextSelectionHelper.GetAnchorPoint(selection);
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
		IList<TextSegment> segments = null;
		TextSelectionHelper.CheckSelection(selection, out var start, out var end, out segments);
		if (!(start is TextPointer))
		{
			throw new ArgumentException(SR.WrongSelectionType, "selection");
		}
		if (!GetNodesStartAndEnd(startNode, out var start2, out var end2))
		{
			return null;
		}
		if (start2.CompareTo(end) > 0)
		{
			throw new ArgumentException(SR.InvalidStartNodeForTextSelection, "startNode");
		}
		if (end2.CompareTo(start) < 0)
		{
			throw new ArgumentException(SR.InvalidStartNodeForTextSelection, "startNode");
		}
		ContentLocatorPart contentLocatorPart = new ContentLocatorPart(CharacterRangeElementName);
		int startOffset = 0;
		int endOffset = 0;
		for (int i = 0; i < segments.Count; i++)
		{
			GetTextSegmentValues(segments[i], start2, end2, out startOffset, out endOffset);
			contentLocatorPart.NameValuePairs.Add("Segment" + i.ToString(NumberFormatInfo.InvariantInfo), startOffset.ToString(NumberFormatInfo.InvariantInfo) + "," + endOffset.ToString(NumberFormatInfo.InvariantInfo));
		}
		contentLocatorPart.NameValuePairs.Add("Count", segments.Count.ToString(NumberFormatInfo.InvariantInfo));
		return new List<ContentLocatorPart>(1) { contentLocatorPart };
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
		if (CharacterRangeElementName != locatorPart.PartType)
		{
			throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
		}
		int startOffset = 0;
		int endOffset = 0;
		int num = int.Parse(locatorPart.NameValuePairs["Count"] ?? throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Count")), NumberFormatInfo.InvariantInfo);
		TextAnchor textAnchor = new TextAnchor();
		attachmentLevel = AttachmentLevel.Unresolved;
		for (int i = 0; i < num; i++)
		{
			GetLocatorPartSegmentValues(locatorPart, i, out startOffset, out endOffset);
			if (!GetNodesStartAndEnd(startNode, out var start, out var end))
			{
				return null;
			}
			int offsetToPosition = start.GetOffsetToPosition(end);
			if (startOffset > offsetToPosition)
			{
				return null;
			}
			ITextPointer textPointer = start.CreatePointer(startOffset);
			ITextPointer textPointer2 = ((offsetToPosition <= endOffset) ? end.CreatePointer() : start.CreatePointer(endOffset));
			if (textPointer.CompareTo(textPointer2) >= 0)
			{
				return null;
			}
			textAnchor.AddTextSegment(textPointer, textPointer2);
		}
		if (textAnchor.IsEmpty)
		{
			throw new ArgumentException(SR.IncorrectAnchorLength, "locatorPart");
		}
		attachmentLevel = AttachmentLevel.Full;
		if (_clamping)
		{
			ITextPointer start2 = textAnchor.Start;
			ITextPointer end2 = textAnchor.End;
			IServiceProvider serviceProvider = null;
			ITextView textView = null;
			serviceProvider = ((_targetPage == null) ? (PathNode.GetParent(start2.TextContainer.Parent as FlowDocument) as IServiceProvider) : _targetPage);
			Invariant.Assert(serviceProvider != null, "No ServiceProvider found to get TextView from.");
			textView = serviceProvider.GetService(typeof(ITextView)) as ITextView;
			Invariant.Assert(textView != null, "Null TextView provided by ServiceProvider.");
			textAnchor = TextAnchor.TrimToIntersectionWith(textAnchor, textView.TextSegments);
			if (textAnchor == null)
			{
				attachmentLevel = AttachmentLevel.Unresolved;
			}
			else
			{
				if (textAnchor.Start.CompareTo(start2) != 0)
				{
					attachmentLevel &= ~AttachmentLevel.StartPortion;
				}
				if (textAnchor.End.CompareTo(end2) != 0)
				{
					attachmentLevel &= ~AttachmentLevel.EndPortion;
				}
			}
		}
		return textAnchor;
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}

	internal static void GetMaxMinLocatorPartValues(ContentLocatorPart locatorPart, out int startOffset, out int endOffset)
	{
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		int num = int.Parse(locatorPart.NameValuePairs["Count"] ?? throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Count")), NumberFormatInfo.InvariantInfo);
		startOffset = int.MaxValue;
		endOffset = 0;
		for (int i = 0; i < num; i++)
		{
			GetLocatorPartSegmentValues(locatorPart, i, out var startOffset2, out var endOffset2);
			if (startOffset2 < startOffset)
			{
				startOffset = startOffset2;
			}
			if (endOffset2 > endOffset)
			{
				endOffset = endOffset2;
			}
		}
	}

	internal void SetTargetDocumentPageView(DocumentPageView target)
	{
		_targetPage = target;
	}

	private static void GetLocatorPartSegmentValues(ContentLocatorPart locatorPart, int segmentNumber, out int startOffset, out int endOffset)
	{
		if (segmentNumber < 0)
		{
			throw new ArgumentException("segmentNumber");
		}
		string[] array = locatorPart.NameValuePairs["Segment" + segmentNumber.ToString(NumberFormatInfo.InvariantInfo)].Split(',');
		if (array.Length != 2)
		{
			throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Segment" + segmentNumber.ToString(NumberFormatInfo.InvariantInfo)));
		}
		startOffset = int.Parse(array[0], NumberFormatInfo.InvariantInfo);
		endOffset = int.Parse(array[1], NumberFormatInfo.InvariantInfo);
	}

	private ITextContainer GetTextContainer(DependencyObject startNode)
	{
		ITextContainer textContainer = null;
		if (startNode is IServiceProvider serviceProvider)
		{
			textContainer = serviceProvider.GetService(typeof(ITextContainer)) as ITextContainer;
		}
		if (textContainer == null && startNode is TextBoxBase textBoxBase)
		{
			textContainer = textBoxBase.TextContainer;
		}
		return textContainer;
	}

	private bool GetNodesStartAndEnd(DependencyObject startNode, out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		ITextContainer textContainer = GetTextContainer(startNode);
		if (textContainer != null)
		{
			start = textContainer.Start;
			end = textContainer.End;
		}
		else
		{
			if (!(startNode is TextElement textElement))
			{
				return false;
			}
			start = textElement.ContentStart;
			end = textElement.ContentEnd;
		}
		return true;
	}

	private void GetTextSegmentValues(TextSegment segment, ITextPointer elementStart, ITextPointer elementEnd, out int startOffset, out int endOffset)
	{
		startOffset = 0;
		endOffset = 0;
		if (elementStart.CompareTo(segment.Start) >= 0)
		{
			startOffset = 0;
		}
		else
		{
			startOffset = elementStart.GetOffsetToPosition(segment.Start);
		}
		if (elementEnd.CompareTo(segment.End) >= 0)
		{
			endOffset = elementStart.GetOffsetToPosition(segment.End);
		}
		else
		{
			endOffset = elementStart.GetOffsetToPosition(elementEnd);
		}
	}
}
