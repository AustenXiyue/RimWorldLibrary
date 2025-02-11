using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;

namespace MS.Internal.Annotations.Anchoring;

internal class TextSelectionHelper
{
	private TextSelectionHelper()
	{
	}

	public static bool MergeSelections(object anchor1, object anchor2, out object newAnchor)
	{
		TextAnchor textAnchor = anchor1 as TextAnchor;
		TextAnchor textAnchor2 = anchor2 as TextAnchor;
		if (anchor1 != null && textAnchor == null)
		{
			throw new ArgumentException(SR.WrongSelectionType, "anchor1: type = " + anchor1.GetType().ToString());
		}
		if (anchor2 != null && textAnchor2 == null)
		{
			throw new ArgumentException(SR.WrongSelectionType, "Anchor2: type = " + anchor2.GetType().ToString());
		}
		if (textAnchor == null)
		{
			newAnchor = textAnchor2;
			return newAnchor != null;
		}
		if (textAnchor2 == null)
		{
			newAnchor = textAnchor;
			return newAnchor != null;
		}
		newAnchor = TextAnchor.ExclusiveUnion(textAnchor, textAnchor2);
		return true;
	}

	public static IList<DependencyObject> GetSelectedNodes(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		ITextPointer start = null;
		ITextPointer end = null;
		CheckSelection(selection, out start, out end, out var _);
		IList<DependencyObject> list = new List<DependencyObject>();
		if (start.CompareTo(end) == 0)
		{
			list.Add(((TextPointer)start).Parent);
			return list;
		}
		TextPointer textPointer = (TextPointer)start.CreatePointer();
		while (((ITextPointer)textPointer).CompareTo(end) < 0)
		{
			DependencyObject parent = textPointer.Parent;
			if (!list.Contains(parent))
			{
				list.Add(parent);
			}
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		return list;
	}

	public static UIElement GetParent(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		ITextPointer start = null;
		ITextPointer end = null;
		CheckSelection(selection, out start, out end, out var _);
		return GetParent(start);
	}

	public static UIElement GetParent(ITextPointer pointer)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		DependencyObject parent = PathNode.GetParent(pointer.TextContainer.Parent);
		if (parent is FlowDocumentScrollViewer flowDocumentScrollViewer)
		{
			return (UIElement)flowDocumentScrollViewer.ScrollViewer.Content;
		}
		if (parent is DocumentViewerBase documentViewerBase)
		{
			GetPointerPage(pointer.CreatePointer(LogicalDirection.Forward), out var pageNumber);
			if (pageNumber >= 0)
			{
				foreach (DocumentPageView pageView in documentViewerBase.PageViews)
				{
					if (pageView.PageNumber == pageNumber)
					{
						Invariant.Assert(VisualTreeHelper.GetChildrenCount(pageView) == 1);
						return VisualTreeHelper.GetChild(pageView, 0) as DocumentPageHost;
					}
				}
				return null;
			}
		}
		return parent as UIElement;
	}

	public static Point GetAnchorPoint(object selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		return GetAnchorPointForPointer(((selection as TextAnchor) ?? throw new ArgumentException(SR.WrongSelectionType, "selection")).Start.CreatePointer(LogicalDirection.Forward));
	}

	public static Point GetAnchorPointForPointer(ITextPointer pointer)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		Rect anchorRectangle = GetAnchorRectangle(pointer);
		if (anchorRectangle != Rect.Empty)
		{
			return new Point(anchorRectangle.Left, anchorRectangle.Top + anchorRectangle.Height);
		}
		return new Point(0.0, 0.0);
	}

	public static Point GetPointForPointer(ITextPointer pointer)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		Rect anchorRectangle = GetAnchorRectangle(pointer);
		if (anchorRectangle != Rect.Empty)
		{
			return new Point(anchorRectangle.Left, anchorRectangle.Top + anchorRectangle.Height / 2.0);
		}
		return new Point(0.0, 0.0);
	}

	public static Rect GetAnchorRectangle(ITextPointer pointer)
	{
		if (pointer == null)
		{
			throw new ArgumentNullException("pointer");
		}
		bool flag = false;
		ITextView documentPageTextView = GetDocumentPageTextView(pointer);
		if (pointer.CompareTo(pointer.TextContainer.End) == 0)
		{
			Point point = new Point(double.MaxValue, double.MaxValue);
			pointer = documentPageTextView.GetTextPositionFromPoint(point, snapToText: true);
			flag = true;
		}
		if (documentPageTextView != null && documentPageTextView.IsValid && TextDocumentView.Contains(pointer, documentPageTextView.TextSegments))
		{
			Rect rectangleFromTextPosition = documentPageTextView.GetRectangleFromTextPosition(pointer);
			if (flag && rectangleFromTextPosition != Rect.Empty)
			{
				rectangleFromTextPosition.X += rectangleFromTextPosition.Height / 2.0;
			}
			return rectangleFromTextPosition;
		}
		return Rect.Empty;
	}

	public static IDocumentPaginatorSource GetPointerPage(ITextPointer pointer, out int pageNumber)
	{
		Invariant.Assert(pointer != null, "unknown pointer");
		IDocumentPaginatorSource documentPaginatorSource = pointer.TextContainer.Parent as IDocumentPaginatorSource;
		if (documentPaginatorSource is FixedDocument { Parent: FixedDocumentSequence parent })
		{
			documentPaginatorSource = parent;
		}
		Invariant.Assert(documentPaginatorSource != null);
		pageNumber = (documentPaginatorSource.DocumentPaginator as DynamicDocumentPaginator)?.GetPageNumber((ContentPosition)pointer) ?? (-1);
		return documentPaginatorSource;
	}

	internal static void CheckSelection(object selection, out ITextPointer start, out ITextPointer end, out IList<TextSegment> segments)
	{
		if (selection is ITextRange textRange)
		{
			start = textRange.Start;
			end = textRange.End;
			segments = textRange.TextSegments;
			return;
		}
		if (!(selection is TextAnchor textAnchor))
		{
			throw new ArgumentException(SR.WrongSelectionType, "selection");
		}
		start = textAnchor.Start;
		end = textAnchor.End;
		segments = textAnchor.TextSegments;
	}

	internal static ITextView GetDocumentPageTextView(ITextPointer pointer)
	{
		DependencyObject parent = pointer.TextContainer.Parent;
		if (parent != null && PathNode.GetParent(parent) is FlowDocumentScrollViewer flowDocumentScrollViewer)
		{
			IServiceProvider obj = flowDocumentScrollViewer.ScrollViewer.Content as IServiceProvider;
			Invariant.Assert(obj != null, "FlowDocumentScrollViewer should be an IServiceProvider.");
			return obj.GetService(typeof(ITextView)) as ITextView;
		}
		int pageNumber;
		IDocumentPaginatorSource pointerPage = GetPointerPage(pointer, out pageNumber);
		if (pointerPage != null && pageNumber >= 0 && pointerPage.DocumentPaginator.GetPage(pageNumber) is IServiceProvider serviceProvider)
		{
			return serviceProvider.GetService(typeof(ITextView)) as ITextView;
		}
		return null;
	}

	internal static List<ITextView> GetDocumentPageTextViews(TextSegment segment)
	{
		List<ITextView> list = null;
		ITextPointer textPointer = segment.Start.CreatePointer(LogicalDirection.Forward);
		ITextPointer textPointer2 = segment.End.CreatePointer(LogicalDirection.Backward);
		DependencyObject parent = textPointer.TextContainer.Parent;
		if (parent != null && PathNode.GetParent(parent) is FlowDocumentScrollViewer flowDocumentScrollViewer)
		{
			IServiceProvider serviceProvider = flowDocumentScrollViewer.ScrollViewer.Content as IServiceProvider;
			Invariant.Assert(serviceProvider != null, "FlowDocumentScrollViewer should be an IServiceProvider.");
			list = new List<ITextView>(1);
			list.Add(serviceProvider.GetService(typeof(ITextView)) as ITextView);
			return list;
		}
		int pageNumber;
		IDocumentPaginatorSource pointerPage = GetPointerPage(textPointer, out pageNumber);
		int num = ((pointerPage.DocumentPaginator is DynamicDocumentPaginator dynamicDocumentPaginator) ? dynamicDocumentPaginator.GetPageNumber((ContentPosition)textPointer2) : (-1));
		if (pageNumber == -1 || num == -1)
		{
			return new List<ITextView>(0);
		}
		if (pageNumber == num)
		{
			return ProcessSinglePage(pointerPage, pageNumber);
		}
		return ProcessMultiplePages(pointerPage, pageNumber, num);
	}

	private static List<ITextView> ProcessSinglePage(IDocumentPaginatorSource idp, int pageNumber)
	{
		Invariant.Assert(idp != null, "IDocumentPaginatorSource is null");
		IServiceProvider serviceProvider = idp.DocumentPaginator.GetPage(pageNumber) as IServiceProvider;
		List<ITextView> list = null;
		if (serviceProvider != null)
		{
			list = new List<ITextView>(1);
			if (serviceProvider.GetService(typeof(ITextView)) is ITextView item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static List<ITextView> ProcessMultiplePages(IDocumentPaginatorSource idp, int startPageNumber, int endPageNumber)
	{
		Invariant.Assert(idp != null, "IDocumentPaginatorSource is null");
		DocumentViewerBase documentViewerBase = PathNode.GetParent(idp as DependencyObject) as DocumentViewerBase;
		Invariant.Assert(documentViewerBase != null, "DocumentViewer not found");
		if (endPageNumber < startPageNumber)
		{
			int num = endPageNumber;
			endPageNumber = startPageNumber;
			startPageNumber = num;
		}
		List<ITextView> list = null;
		if (idp != null && startPageNumber >= 0 && endPageNumber >= startPageNumber)
		{
			list = new List<ITextView>(endPageNumber - startPageNumber + 1);
			for (int i = startPageNumber; i <= endPageNumber; i++)
			{
				DocumentPageView documentPageView = AnnotationHelper.FindView(documentViewerBase, i);
				if (documentPageView != null && documentPageView.DocumentPage is IServiceProvider serviceProvider && serviceProvider.GetService(typeof(ITextView)) is ITextView item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
