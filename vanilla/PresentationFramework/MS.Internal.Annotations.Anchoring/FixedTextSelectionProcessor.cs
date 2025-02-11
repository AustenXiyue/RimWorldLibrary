using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal class FixedTextSelectionProcessor : SelectionProcessor
{
	internal sealed class FixedPageProxy : DependencyObject
	{
		private int _page;

		private IList<PointSegment> _segments = new List<PointSegment>(1);

		public int Page => _page;

		public IList<PointSegment> Segments => _segments;

		public FixedPageProxy(DependencyObject parent, int page)
		{
			SetValue(PathNode.HiddenParentProperty, parent);
			_page = page;
		}
	}

	internal sealed class PointSegment
	{
		public static readonly Point NotAPoint = new Point(double.NaN, double.NaN);

		private Point _start;

		private Point _end;

		public Point Start => _start;

		public Point End => _end;

		internal PointSegment(Point start, Point end)
		{
			_start = start;
			_end = end;
		}
	}

	private static readonly XmlQualifiedName FixedTextElementName = new XmlQualifiedName("FixedTextRange", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	private static readonly XmlQualifiedName[] LocatorPartTypeNames = new XmlQualifiedName[1] { FixedTextElementName };

	public override bool MergeSelections(object anchor1, object anchor2, out object newAnchor)
	{
		return TextSelectionHelper.MergeSelections(anchor1, anchor2, out newAnchor);
	}

	public override IList<DependencyObject> GetSelectedNodes(object selection)
	{
		IList<TextSegment> list = CheckSelection(selection);
		IList<DependencyObject> list2 = new List<DependencyObject>();
		foreach (TextSegment item in list)
		{
			int pageNumber = int.MinValue;
			ITextPointer pointer = item.Start.CreatePointer(LogicalDirection.Forward);
			TextSelectionHelper.GetPointerPage(pointer, out pageNumber);
			Point pointForPointer = TextSelectionHelper.GetPointForPointer(pointer);
			if (pageNumber == int.MinValue)
			{
				throw new ArgumentException(SR.Format(SR.SelectionDoesNotResolveToAPage, "start"), "selection");
			}
			int pageNumber2 = int.MinValue;
			ITextPointer pointer2 = item.End.CreatePointer(LogicalDirection.Backward);
			TextSelectionHelper.GetPointerPage(pointer2, out pageNumber2);
			Point pointForPointer2 = TextSelectionHelper.GetPointForPointer(pointer2);
			if (pageNumber2 == int.MinValue)
			{
				throw new ArgumentException(SR.Format(SR.SelectionDoesNotResolveToAPage, "end"), "selection");
			}
			int num = list2.Count;
			int num2 = pageNumber2 - pageNumber;
			int i = 0;
			if (list2.Count > 0 && ((FixedPageProxy)list2[list2.Count - 1]).Page == pageNumber)
			{
				num--;
				i++;
			}
			for (; i <= num2; i++)
			{
				list2.Add(new FixedPageProxy(item.Start.TextContainer.Parent, pageNumber + i));
			}
			if (num2 == 0)
			{
				((FixedPageProxy)list2[num]).Segments.Add(new PointSegment(pointForPointer, pointForPointer2));
				continue;
			}
			((FixedPageProxy)list2[num]).Segments.Add(new PointSegment(pointForPointer, PointSegment.NotAPoint));
			((FixedPageProxy)list2[num + num2]).Segments.Add(new PointSegment(PointSegment.NotAPoint, pointForPointer2));
		}
		return list2;
	}

	public override UIElement GetParent(object selection)
	{
		CheckAnchor(selection);
		return TextSelectionHelper.GetParent(selection);
	}

	public override Point GetAnchorPoint(object selection)
	{
		CheckAnchor(selection);
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
		CheckSelection(selection);
		if (!(startNode is FixedPageProxy fixedPageProxy))
		{
			throw new ArgumentException(SR.StartNodeMustBeFixedPageProxy, "startNode");
		}
		ContentLocatorPart contentLocatorPart = new ContentLocatorPart(FixedTextElementName);
		if (fixedPageProxy.Segments.Count == 0)
		{
			contentLocatorPart.NameValuePairs.Add("Count", 1.ToString(NumberFormatInfo.InvariantInfo));
			contentLocatorPart.NameValuePairs.Add("Segment" + 0.ToString(NumberFormatInfo.InvariantInfo), ",,,");
		}
		else
		{
			contentLocatorPart.NameValuePairs.Add("Count", fixedPageProxy.Segments.Count.ToString(NumberFormatInfo.InvariantInfo));
			for (int i = 0; i < fixedPageProxy.Segments.Count; i++)
			{
				string text = "";
				text = (double.IsNaN(fixedPageProxy.Segments[i].Start.X) ? (text + ",") : (text + fixedPageProxy.Segments[i].Start.X.ToString(NumberFormatInfo.InvariantInfo) + "," + fixedPageProxy.Segments[i].Start.Y.ToString(NumberFormatInfo.InvariantInfo)));
				text += ",";
				text = (double.IsNaN(fixedPageProxy.Segments[i].End.X) ? (text + ",") : (text + fixedPageProxy.Segments[i].End.X.ToString(NumberFormatInfo.InvariantInfo) + "," + fixedPageProxy.Segments[i].End.Y.ToString(NumberFormatInfo.InvariantInfo)));
				contentLocatorPart.NameValuePairs.Add("Segment" + i.ToString(NumberFormatInfo.InvariantInfo), text);
			}
		}
		return new List<ContentLocatorPart>(1) { contentLocatorPart };
	}

	public override object ResolveLocatorPart(ContentLocatorPart locatorPart, DependencyObject startNode, out AttachmentLevel attachmentLevel)
	{
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		DocumentPage documentPage = null;
		if (startNode is FixedPage page)
		{
			documentPage = GetDocumentPage(page);
		}
		else if (startNode is DocumentPageView documentPageView)
		{
			documentPage = documentPageView.DocumentPage as FixedDocumentPage;
			if (documentPage == null)
			{
				documentPage = documentPageView.DocumentPage as FixedDocumentSequenceDocumentPage;
			}
		}
		if (documentPage == null)
		{
			throw new ArgumentException(SR.StartNodeMustBeDocumentPageViewOrFixedPage, "startNode");
		}
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		attachmentLevel = AttachmentLevel.Unresolved;
		ITextView textView = (ITextView)((IServiceProvider)documentPage).GetService(typeof(ITextView));
		ReadOnlyCollection<TextSegment> textSegments = textView.TextSegments;
		if (textSegments == null || textSegments.Count <= 0)
		{
			return null;
		}
		TextAnchor textAnchor = new TextAnchor();
		if (documentPage != null)
		{
			int num = int.Parse(locatorPart.NameValuePairs["Count"] ?? throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Count")), NumberFormatInfo.InvariantInfo);
			for (int i = 0; i < num; i++)
			{
				GetLocatorPartSegmentValues(locatorPart, i, out var start, out var end);
				ITextPointer textPointer = ((!double.IsNaN(start.X) && !double.IsNaN(start.Y)) ? textView.GetTextPositionFromPoint(start, snapToText: true) : FindStartVisibleTextPointer(documentPage));
				if (textPointer != null)
				{
					ITextPointer textPointer2 = ((!double.IsNaN(end.X) && !double.IsNaN(end.Y)) ? textView.GetTextPositionFromPoint(end, snapToText: true) : FindEndVisibleTextPointer(documentPage));
					Invariant.Assert(textPointer2 != null, "end TP is null when start TP is not");
					attachmentLevel = AttachmentLevel.Full;
					textAnchor.AddTextSegment(textPointer, textPointer2);
				}
			}
		}
		if (textAnchor.TextSegments.Count > 0)
		{
			return textAnchor;
		}
		return null;
	}

	public override XmlQualifiedName[] GetLocatorPartTypes()
	{
		return (XmlQualifiedName[])LocatorPartTypeNames.Clone();
	}

	private DocumentPage GetDocumentPage(FixedPage page)
	{
		Invariant.Assert(page != null);
		DocumentPage result = null;
		if (page.Parent is PageContent pageContent)
		{
			FixedDocument fixedDocument = pageContent.Parent as FixedDocument;
			result = ((!(fixedDocument.Parent is FixedDocumentSequence fixedDocumentSequence)) ? fixedDocument.GetPage(fixedDocument.GetIndexOfPage(page)) : fixedDocumentSequence.GetPage(fixedDocument, fixedDocument.GetIndexOfPage(page)));
		}
		return result;
	}

	private IList<TextSegment> CheckSelection(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		IList<TextSegment> list = null;
		ITextPointer textPointer = null;
		if (selection is ITextRange textRange)
		{
			textPointer = textRange.Start;
			list = textRange.TextSegments;
		}
		else
		{
			if (!(selection is TextAnchor textAnchor))
			{
				throw new ArgumentException(SR.WrongSelectionType, "selection: type=" + selection.GetType().ToString());
			}
			textPointer = textAnchor.Start;
			list = textAnchor.TextSegments;
		}
		if (!(textPointer.TextContainer is FixedTextContainer) && !(textPointer.TextContainer is DocumentSequenceTextContainer))
		{
			throw new ArgumentException(SR.WrongSelectionType, "selection: type=" + selection.GetType().ToString());
		}
		return list;
	}

	private TextAnchor CheckAnchor(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		if (!(selection is TextAnchor textAnchor) || (!(textAnchor.Start.TextContainer is FixedTextContainer) && !(textAnchor.Start.TextContainer is DocumentSequenceTextContainer)))
		{
			throw new ArgumentException(SR.WrongSelectionType, "selection: type=" + selection.GetType().ToString());
		}
		return textAnchor;
	}

	private void GetLocatorPartSegmentValues(ContentLocatorPart locatorPart, int segmentNumber, out Point start, out Point end)
	{
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		if (FixedTextElementName != locatorPart.PartType)
		{
			throw new ArgumentException(SR.Format(SR.IncorrectLocatorPartType, locatorPart.PartType.Namespace + ":" + locatorPart.PartType.Name), "locatorPart");
		}
		string[] array = (locatorPart.NameValuePairs["Segment" + segmentNumber.ToString(NumberFormatInfo.InvariantInfo)] ?? throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Segment" + segmentNumber.ToString(NumberFormatInfo.InvariantInfo)))).Split(',');
		if (array.Length != 4)
		{
			throw new ArgumentException(SR.Format(SR.InvalidLocatorPart, "Segment" + segmentNumber.ToString(NumberFormatInfo.InvariantInfo)));
		}
		start = GetPoint(array[0], array[1]);
		end = GetPoint(array[2], array[3]);
	}

	private Point GetPoint(string xstr, string ystr)
	{
		Point result;
		if (xstr != null && !string.IsNullOrEmpty(xstr.Trim()) && ystr != null && !string.IsNullOrEmpty(ystr.Trim()))
		{
			double x = double.Parse(xstr, NumberFormatInfo.InvariantInfo);
			double y = double.Parse(ystr, NumberFormatInfo.InvariantInfo);
			result = new Point(x, y);
		}
		else
		{
			result = new Point(double.NaN, double.NaN);
		}
		return result;
	}

	private static ITextPointer FindStartVisibleTextPointer(DocumentPage documentPage)
	{
		if (!GetTextViewRange(documentPage, out var start, out var end))
		{
			return null;
		}
		if (!start.IsAtInsertionPosition && !start.MoveToNextInsertionPosition(LogicalDirection.Forward))
		{
			return null;
		}
		if (start.CompareTo(end) > 0)
		{
			return null;
		}
		return start;
	}

	private static ITextPointer FindEndVisibleTextPointer(DocumentPage documentPage)
	{
		if (!GetTextViewRange(documentPage, out var start, out var end))
		{
			return null;
		}
		if (!end.IsAtInsertionPosition && !end.MoveToNextInsertionPosition(LogicalDirection.Backward))
		{
			return null;
		}
		if (start.CompareTo(end) > 0)
		{
			return null;
		}
		return end;
	}

	private static bool GetTextViewRange(DocumentPage documentPage, out ITextPointer start, out ITextPointer end)
	{
		start = (end = null);
		Invariant.Assert(documentPage != DocumentPage.Missing);
		ITextView textView = ((IServiceProvider)documentPage).GetService(typeof(ITextView)) as ITextView;
		Invariant.Assert(textView != null, "DocumentPage didn't provide a TextView.");
		if (textView.TextSegments == null || textView.TextSegments.Count == 0)
		{
			return false;
		}
		start = textView.TextSegments[0].Start.CreatePointer(LogicalDirection.Forward);
		end = textView.TextSegments[textView.TextSegments.Count - 1].End.CreatePointer(LogicalDirection.Backward);
		return true;
	}
}
