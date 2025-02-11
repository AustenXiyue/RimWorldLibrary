using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using MS.Internal.Documents;
using MS.Internal.PtsHost;

namespace MS.Internal;

internal static class LayoutDump
{
	internal delegate bool DumpCustomUIElement(XmlTextWriter writer, UIElement element, bool uiElementsOnly);

	internal delegate void DumpCustomDocumentPage(XmlTextWriter writer, DocumentPage page);

	private static Hashtable _elementToDumpHandler;

	private static Hashtable _documentPageToDumpHandler;

	internal static string DumpLayoutAndVisualTreeToString(string tagName, Visual root)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlTextWriter obj = new XmlTextWriter(stringWriter)
		{
			Formatting = Formatting.Indented,
			Indentation = 2
		};
		DumpLayoutAndVisualTree(obj, tagName, root);
		obj.Flush();
		obj.Close();
		return stringWriter.ToString();
	}

	internal static void DumpLayoutAndVisualTree(XmlTextWriter writer, string tagName, Visual root)
	{
		writer.WriteStartElement(tagName);
		DumpVisual(writer, root, root);
		writer.WriteEndElement();
		writer.WriteRaw("\r\n");
	}

	internal static void DumpLayoutTreeToFile(string tagName, UIElement root, string fileName)
	{
		string value = DumpLayoutTreeToString(tagName, root);
		StreamWriter streamWriter = new StreamWriter(fileName);
		streamWriter.Write(value);
		streamWriter.Flush();
		streamWriter.Close();
	}

	internal static string DumpLayoutTreeToString(string tagName, UIElement root)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlTextWriter obj = new XmlTextWriter(stringWriter)
		{
			Formatting = Formatting.Indented,
			Indentation = 2
		};
		DumpLayoutTree(obj, tagName, root);
		obj.Flush();
		obj.Close();
		return stringWriter.ToString();
	}

	internal static void DumpLayoutTree(XmlTextWriter writer, string tagName, UIElement root)
	{
		writer.WriteStartElement(tagName);
		DumpUIElement(writer, root, root, uiElementsOnly: true);
		writer.WriteEndElement();
		writer.WriteRaw("\r\n");
	}

	internal static void AddUIElementDumpHandler(Type type, DumpCustomUIElement dumper)
	{
		_elementToDumpHandler.Add(type, dumper);
	}

	internal static void AddDocumentPageDumpHandler(Type type, DumpCustomDocumentPage dumper)
	{
		_documentPageToDumpHandler.Add(type, dumper);
	}

	internal static void DumpVisual(XmlTextWriter writer, Visual visual, Visual parent)
	{
		if (visual is UIElement)
		{
			DumpUIElement(writer, (UIElement)visual, parent, uiElementsOnly: false);
			return;
		}
		writer.WriteStartElement(visual.GetType().Name);
		Rect visualContentBounds = visual.VisualContentBounds;
		if (!visualContentBounds.IsEmpty)
		{
			DumpRect(writer, "ContentRect", visualContentBounds);
		}
		Geometry clip = VisualTreeHelper.GetClip(visual);
		if (clip != null)
		{
			DumpRect(writer, "Clip.Bounds", clip.Bounds);
		}
		GeneralTransform generalTransform = visual.TransformToAncestor(parent);
		Point result = new Point(0.0, 0.0);
		generalTransform.TryTransform(result, out result);
		if (result.X != 0.0 || result.Y != 0.0)
		{
			DumpPoint(writer, "Position", result);
		}
		DumpVisualChildren(writer, "Children", visual);
		writer.WriteEndElement();
	}

	private static void DumpUIElement(XmlTextWriter writer, UIElement element, Visual parent, bool uiElementsOnly)
	{
		writer.WriteStartElement(element.GetType().Name);
		DumpSize(writer, "DesiredSize", element.DesiredSize);
		DumpSize(writer, "ComputedSize", element.RenderSize);
		Geometry clip = VisualTreeHelper.GetClip(element);
		if (clip != null)
		{
			DumpRect(writer, "Clip.Bounds", clip.Bounds);
		}
		GeneralTransform generalTransform = element.TransformToAncestor(parent);
		Point result = new Point(0.0, 0.0);
		generalTransform.TryTransform(result, out result);
		if (result.X != 0.0 || result.Y != 0.0)
		{
			DumpPoint(writer, "Position", result);
		}
		bool flag = false;
		Type type = element.GetType();
		DumpCustomUIElement dumpCustomUIElement = null;
		while (dumpCustomUIElement == null && type != null)
		{
			dumpCustomUIElement = _elementToDumpHandler[type] as DumpCustomUIElement;
			type = type.BaseType;
		}
		if (dumpCustomUIElement != null)
		{
			flag = dumpCustomUIElement(writer, element, uiElementsOnly);
		}
		if (!flag)
		{
			if (uiElementsOnly)
			{
				DumpUIElementChildren(writer, "Children", element);
			}
			else
			{
				DumpVisualChildren(writer, "Children", element);
			}
		}
		writer.WriteEndElement();
	}

	internal static void DumpDocumentPage(XmlTextWriter writer, DocumentPage page, Visual parent)
	{
		writer.WriteStartElement("DocumentPage");
		writer.WriteAttributeString("Type", page.GetType().FullName);
		if (page != DocumentPage.Missing)
		{
			DumpSize(writer, "Size", page.Size);
			GeneralTransform generalTransform = page.Visual.TransformToAncestor(parent);
			Point result = new Point(0.0, 0.0);
			generalTransform.TryTransform(result, out result);
			if (result.X != 0.0 || result.Y != 0.0)
			{
				DumpPoint(writer, "Position", result);
			}
			Type type = page.GetType();
			DumpCustomDocumentPage dumpCustomDocumentPage = null;
			while (dumpCustomDocumentPage == null && type != null)
			{
				dumpCustomDocumentPage = _documentPageToDumpHandler[type] as DumpCustomDocumentPage;
				type = type.BaseType;
			}
			dumpCustomDocumentPage?.Invoke(writer, page);
		}
		writer.WriteEndElement();
	}

	private static void DumpVisualChildren(XmlTextWriter writer, string tagName, Visual visualParent)
	{
		int childrenCount = VisualTreeHelper.GetChildrenCount(visualParent);
		if (childrenCount > 0)
		{
			writer.WriteStartElement(tagName);
			writer.WriteAttributeString("Count", childrenCount.ToString(CultureInfo.InvariantCulture));
			for (int i = 0; i < childrenCount; i++)
			{
				DumpVisual(writer, visualParent.InternalGetVisualChild(i), visualParent);
			}
			writer.WriteEndElement();
		}
	}

	internal static void DumpUIElementChildren(XmlTextWriter writer, string tagName, Visual visualParent)
	{
		List<UIElement> list = new List<UIElement>();
		GetUIElementsFromVisual(visualParent, list);
		if (list.Count > 0)
		{
			writer.WriteStartElement(tagName);
			writer.WriteAttributeString("Count", list.Count.ToString(CultureInfo.InvariantCulture));
			for (int i = 0; i < list.Count; i++)
			{
				DumpUIElement(writer, list[i], visualParent, uiElementsOnly: true);
			}
			writer.WriteEndElement();
		}
	}

	internal static void DumpPoint(XmlTextWriter writer, string tagName, Point point)
	{
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("Left", point.X.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Top", point.Y.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteEndElement();
	}

	internal static void DumpSize(XmlTextWriter writer, string tagName, Size size)
	{
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("Width", size.Width.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Height", size.Height.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteEndElement();
	}

	internal static void DumpRect(XmlTextWriter writer, string tagName, Rect rect)
	{
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("Left", rect.Left.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Top", rect.Top.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Width", rect.Width.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Height", rect.Height.ToString("F", CultureInfo.InvariantCulture));
		writer.WriteEndElement();
	}

	internal static void GetUIElementsFromVisual(Visual visual, List<UIElement> uiElements)
	{
		int childrenCount = VisualTreeHelper.GetChildrenCount(visual);
		for (int i = 0; i < childrenCount; i++)
		{
			Visual visual2 = visual.InternalGetVisualChild(i);
			if (visual2 is UIElement)
			{
				uiElements.Add((UIElement)visual2);
			}
			else
			{
				GetUIElementsFromVisual(visual2, uiElements);
			}
		}
	}

	static LayoutDump()
	{
		_elementToDumpHandler = new Hashtable();
		_documentPageToDumpHandler = new Hashtable();
		AddUIElementDumpHandler(typeof(TextBlock), DumpText);
		AddUIElementDumpHandler(typeof(FlowDocumentScrollViewer), DumpFlowDocumentScrollViewer);
		AddUIElementDumpHandler(typeof(FlowDocumentView), DumpFlowDocumentView);
		AddUIElementDumpHandler(typeof(DocumentPageView), DumpDocumentPageView);
		AddDocumentPageDumpHandler(typeof(FlowDocumentPage), DumpFlowDocumentPage);
	}

	private static bool DumpDocumentPageView(XmlTextWriter writer, UIElement element, bool uiElementsOnly)
	{
		DocumentPageView documentPageView = element as DocumentPageView;
		if (documentPageView.DocumentPage != null)
		{
			DumpDocumentPage(writer, documentPageView.DocumentPage, element);
		}
		return false;
	}

	private static bool DumpText(XmlTextWriter writer, UIElement element, bool uiElementsOnly)
	{
		TextBlock textBlock = element as TextBlock;
		if (textBlock.HasComplexContent)
		{
			DumpTextRange(writer, textBlock.ContentStart, textBlock.ContentEnd);
		}
		else
		{
			DumpTextRange(writer, textBlock.Text);
		}
		writer.WriteStartElement("Metrics");
		writer.WriteAttributeString("BaselineOffset", ((double)textBlock.GetValue(TextBlock.BaselineOffsetProperty)).ToString("F", CultureInfo.InvariantCulture));
		writer.WriteEndElement();
		if (textBlock.IsLayoutDataValid)
		{
			ReadOnlyCollection<LineResult> lineResults = textBlock.GetLineResults();
			DumpLineResults(writer, lineResults, element);
		}
		return false;
	}

	private static bool DumpFlowDocumentScrollViewer(XmlTextWriter writer, UIElement element, bool uiElementsOnly)
	{
		FlowDocumentScrollViewer flowDocumentScrollViewer = element as FlowDocumentScrollViewer;
		bool result = false;
		if (flowDocumentScrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden && flowDocumentScrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
		{
			FlowDocumentView flowDocumentView = null;
			if (flowDocumentScrollViewer.ScrollViewer != null && flowDocumentScrollViewer.ScrollViewer.Content is FlowDocumentView element2)
			{
				DumpUIElement(writer, element2, flowDocumentScrollViewer, uiElementsOnly);
				result = true;
			}
		}
		return result;
	}

	private static bool DumpFlowDocumentView(XmlTextWriter writer, UIElement element, bool uiElementsOnly)
	{
		FlowDocumentView flowDocumentView = element as FlowDocumentView;
		IScrollInfo scrollInfo = flowDocumentView;
		if (scrollInfo.ScrollOwner != null)
		{
			Size size = new Size(scrollInfo.ExtentWidth, scrollInfo.ExtentHeight);
			if (DoubleUtil.AreClose(size, element.DesiredSize))
			{
				DumpSize(writer, "Extent", size);
			}
			Point point = new Point(scrollInfo.HorizontalOffset, scrollInfo.VerticalOffset);
			if (!DoubleUtil.IsZero(point.X) || !DoubleUtil.IsZero(point.Y))
			{
				DumpPoint(writer, "Offset", point);
			}
		}
		FlowDocumentPage documentPage = flowDocumentView.Document.BottomlessFormatter.DocumentPage;
		GeneralTransform generalTransform = documentPage.Visual.TransformToAncestor(flowDocumentView);
		Point result = new Point(0.0, 0.0);
		generalTransform.TryTransform(result, out result);
		if (!DoubleUtil.IsZero(result.X) && !DoubleUtil.IsZero(result.Y))
		{
			DumpPoint(writer, "PagePosition", result);
		}
		DumpFlowDocumentPage(writer, documentPage);
		return false;
	}

	private static void DumpFlowDocumentPage(XmlTextWriter writer, DocumentPage page)
	{
		FlowDocumentPage flowDocumentPage = page as FlowDocumentPage;
		writer.WriteStartElement("FormattedLines");
		writer.WriteAttributeString("Count", flowDocumentPage.FormattedLinesCount.ToString(CultureInfo.InvariantCulture));
		writer.WriteEndElement();
		TextDocumentView textDocumentView = (TextDocumentView)((IServiceProvider)flowDocumentPage).GetService(typeof(ITextView));
		if (textDocumentView.IsValid)
		{
			DumpColumnResults(writer, textDocumentView.Columns, page.Visual);
		}
	}

	private static void DumpTextRange(XmlTextWriter writer, string content)
	{
		int num = 0;
		int length = content.Length;
		writer.WriteStartElement("TextRange");
		writer.WriteAttributeString("Start", num.ToString(CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Length", (length - num).ToString(CultureInfo.InvariantCulture));
		writer.WriteEndElement();
	}

	private static void DumpTextRange(XmlTextWriter writer, ITextPointer start, ITextPointer end)
	{
		int offsetToPosition = start.TextContainer.Start.GetOffsetToPosition(start);
		int offsetToPosition2 = end.TextContainer.Start.GetOffsetToPosition(end);
		writer.WriteStartElement("TextRange");
		writer.WriteAttributeString("Start", offsetToPosition.ToString(CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Length", (offsetToPosition2 - offsetToPosition).ToString(CultureInfo.InvariantCulture));
		writer.WriteEndElement();
	}

	private static void DumpLineRange(XmlTextWriter writer, int cpStart, int cpEnd, int cpContentEnd, int cpEllipses)
	{
		writer.WriteStartElement("TextRange");
		writer.WriteAttributeString("Start", cpStart.ToString(CultureInfo.InvariantCulture));
		writer.WriteAttributeString("Length", (cpEnd - cpStart).ToString(CultureInfo.InvariantCulture));
		if (cpEnd != cpContentEnd)
		{
			writer.WriteAttributeString("HiddenLength", (cpEnd - cpContentEnd).ToString(CultureInfo.InvariantCulture));
		}
		if (cpEnd != cpEllipses)
		{
			writer.WriteAttributeString("EllipsesLength", (cpEnd - cpEllipses).ToString(CultureInfo.InvariantCulture));
		}
		writer.WriteEndElement();
	}

	private static void DumpLineResults(XmlTextWriter writer, ReadOnlyCollection<LineResult> lines, Visual visualParent)
	{
		if (lines != null)
		{
			writer.WriteStartElement("Lines");
			writer.WriteAttributeString("Count", lines.Count.ToString(CultureInfo.InvariantCulture));
			for (int i = 0; i < lines.Count; i++)
			{
				writer.WriteStartElement("Line");
				LineResult lineResult = lines[i];
				DumpRect(writer, "LayoutBox", lineResult.LayoutBox);
				DumpLineRange(writer, lineResult.StartPositionCP, lineResult.EndPositionCP, lineResult.GetContentEndPositionCP(), lineResult.GetEllipsesPositionCP());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	private static void DumpParagraphResults(XmlTextWriter writer, string tagName, ReadOnlyCollection<ParagraphResult> paragraphs, Visual visualParent)
	{
		if (paragraphs == null)
		{
			return;
		}
		writer.WriteStartElement(tagName);
		writer.WriteAttributeString("Count", paragraphs.Count.ToString(CultureInfo.InvariantCulture));
		for (int i = 0; i < paragraphs.Count; i++)
		{
			ParagraphResult paragraphResult = paragraphs[i];
			if (paragraphResult is TextParagraphResult)
			{
				DumpTextParagraphResult(writer, (TextParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is ContainerParagraphResult)
			{
				DumpContainerParagraphResult(writer, (ContainerParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is TableParagraphResult)
			{
				DumpTableParagraphResult(writer, (TableParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is FloaterParagraphResult)
			{
				DumpFloaterParagraphResult(writer, (FloaterParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is UIElementParagraphResult)
			{
				DumpUIElementParagraphResult(writer, (UIElementParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is FigureParagraphResult)
			{
				DumpFigureParagraphResult(writer, (FigureParagraphResult)paragraphResult, visualParent);
			}
			else if (paragraphResult is SubpageParagraphResult)
			{
				DumpSubpageParagraphResult(writer, (SubpageParagraphResult)paragraphResult, visualParent);
			}
		}
		writer.WriteEndElement();
	}

	private static void DumpTextParagraphResult(XmlTextWriter writer, TextParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("TextParagraph");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visualParent2 = DumpParagraphOffset(writer, paragraph, visualParent);
		DumpTextRange(writer, paragraph.StartPosition, paragraph.EndPosition);
		DumpLineResults(writer, paragraph.Lines, visualParent2);
		DumpParagraphResults(writer, "Floaters", paragraph.Floaters, visualParent2);
		DumpParagraphResults(writer, "Figures", paragraph.Figures, visualParent2);
		writer.WriteEndElement();
	}

	private static void DumpContainerParagraphResult(XmlTextWriter writer, ContainerParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("ContainerParagraph");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visualParent2 = DumpParagraphOffset(writer, paragraph, visualParent);
		DumpParagraphResults(writer, "Paragraphs", paragraph.Paragraphs, visualParent2);
		writer.WriteEndElement();
	}

	private static void DumpFloaterParagraphResult(XmlTextWriter writer, FloaterParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("Floater");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visualParent2 = DumpParagraphOffset(writer, paragraph, visualParent);
		DumpColumnResults(writer, paragraph.Columns, visualParent2);
		writer.WriteEndElement();
	}

	private static void DumpUIElementParagraphResult(XmlTextWriter writer, UIElementParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("BlockUIContainer");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		DumpParagraphOffset(writer, paragraph, visualParent);
		writer.WriteEndElement();
	}

	private static void DumpFigureParagraphResult(XmlTextWriter writer, FigureParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("Figure");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visualParent2 = DumpParagraphOffset(writer, paragraph, visualParent);
		DumpColumnResults(writer, paragraph.Columns, visualParent2);
		writer.WriteEndElement();
	}

	private static void DumpTableParagraphResult(XmlTextWriter writer, TableParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("TableParagraph");
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visual = DumpParagraphOffset(writer, paragraph, visualParent);
		ReadOnlyCollection<ParagraphResult> paragraphs = paragraph.Paragraphs;
		int childrenCount = VisualTreeHelper.GetChildrenCount(visual);
		for (int i = 0; i < childrenCount; i++)
		{
			Visual visual2 = visual.InternalGetVisualChild(i);
			int childrenCount2 = VisualTreeHelper.GetChildrenCount(visual2);
			ReadOnlyCollection<ParagraphResult> cellParagraphs = ((RowParagraphResult)paragraphs[i]).CellParagraphs;
			for (int j = 0; j < childrenCount2; j++)
			{
				Visual cellVisual = visual2.InternalGetVisualChild(j);
				DumpTableCell(writer, cellParagraphs[j], cellVisual, visual);
			}
		}
		writer.WriteEndElement();
	}

	private static void DumpSubpageParagraphResult(XmlTextWriter writer, SubpageParagraphResult paragraph, Visual visualParent)
	{
		writer.WriteStartElement("SubpageParagraph");
		writer.WriteStartElement("Element");
		writer.WriteAttributeString("Type", paragraph.Element.GetType().FullName);
		writer.WriteEndElement();
		DumpRect(writer, "LayoutBox", paragraph.LayoutBox);
		Visual visualParent2 = DumpParagraphOffset(writer, paragraph, visualParent);
		DumpColumnResults(writer, paragraph.Columns, visualParent2);
		writer.WriteEndElement();
	}

	private static void DumpColumnResults(XmlTextWriter writer, ReadOnlyCollection<ColumnResult> columns, Visual visualParent)
	{
		if (columns != null)
		{
			writer.WriteStartElement("Columns");
			writer.WriteAttributeString("Count", columns.Count.ToString(CultureInfo.InvariantCulture));
			for (int i = 0; i < columns.Count; i++)
			{
				writer.WriteStartElement("Column");
				ColumnResult columnResult = columns[i];
				DumpRect(writer, "LayoutBox", columnResult.LayoutBox);
				DumpTextRange(writer, columnResult.StartPosition, columnResult.EndPosition);
				DumpParagraphResults(writer, "Paragraphs", columnResult.Paragraphs, visualParent);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}

	private static Visual DumpParagraphOffset(XmlTextWriter writer, ParagraphResult paragraph, Visual visualParent)
	{
		object value = paragraph.GetType().GetField("_paraClient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(paragraph);
		Visual visual = (Visual)value.GetType().GetProperty("Visual", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(value, null);
		if (visualParent.IsAncestorOf(visual))
		{
			GeneralTransform generalTransform = visual.TransformToAncestor(visualParent);
			Point result = new Point(0.0, 0.0);
			generalTransform.TryTransform(result, out result);
			if (result.X != 0.0 || result.Y != 0.0)
			{
				DumpPoint(writer, "Origin", result);
			}
		}
		return visual;
	}

	private static void DumpTableCalculatedMetrics(XmlTextWriter writer, object element)
	{
		PropertyInfo property = typeof(Table).GetProperty("ColumnCount");
		if (property != null)
		{
			int num = (int)property.GetValue(element, null);
			writer.WriteStartElement("ColumnCount");
			writer.WriteAttributeString("Count", num.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
	}

	private static void DumpTableCell(XmlTextWriter writer, ParagraphResult paragraph, Visual cellVisual, Visual tableVisual)
	{
		FieldInfo field = paragraph.GetType().GetField("_paraClient", BindingFlags.Instance | BindingFlags.NonPublic);
		if (!(field == null))
		{
			CellParaClient cellParaClient = (CellParaClient)field.GetValue(paragraph);
			TableCell cell = cellParaClient.CellParagraph.Cell;
			writer.WriteStartElement("Cell");
			Type type = cell.GetType();
			PropertyInfo property = type.GetProperty("ColumnIndex", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if (property != null)
			{
				writer.WriteAttributeString("ColumnIndex", ((int)property.GetValue(cell, null)).ToString(CultureInfo.InvariantCulture));
			}
			PropertyInfo property2 = type.GetProperty("RowIndex", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if (property2 != null)
			{
				writer.WriteAttributeString("RowIndex", ((int)property2.GetValue(cell, null)).ToString(CultureInfo.InvariantCulture));
			}
			writer.WriteAttributeString("ColumnSpan", cell.ColumnSpan.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("RowSpan", cell.RowSpan.ToString(CultureInfo.InvariantCulture));
			Rect rect = cellParaClient.Rect.FromTextDpi();
			DumpRect(writer, "LayoutBox", rect);
			DumpParagraphResults(writer, "Paragraphs", cellParaClient.GetColumnResults(out var _)[0].Paragraphs, cellParaClient.Visual);
			writer.WriteEndElement();
		}
	}
}
