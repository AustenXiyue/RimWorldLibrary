using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Anchoring;
using MS.Internal.Annotations.Component;
using MS.Utility;

namespace System.Windows.Annotations;

/// <summary>Provides utility methods and commands to create and delete highlight, ink sticky note, and text sticky note annotations.</summary>
public static class AnnotationHelper
{
	/// <summary>Creates a highlight annotation on the current selection of the viewer control associated with the specified <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <returns>The highlight annotation; or null, if there is no selected content to highlight.</returns>
	/// <param name="service">The annotation service to use to create the highlight annotation.</param>
	/// <param name="author">The author of the annotation.</param>
	/// <param name="highlightBrush">The brush to use to draw the highlight over the selected content.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled. -or-<paramref name="highlightBrush" /> in not a <see cref="T:System.Windows.Media.SolidColorBrush" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The viewer control contains no content selection.</exception>
	public static Annotation CreateHighlightForSelection(AnnotationService service, string author, Brush highlightBrush)
	{
		Annotation annotation = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.CreateHighlightBegin);
		try
		{
			annotation = Highlight(service, author, highlightBrush, create: true);
			Invariant.Assert(annotation != null, "Highlight not returned from create call.");
			return annotation;
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.CreateHighlightEnd);
		}
	}

	/// <summary>Creates a text sticky note annotation on the current selection of the viewer control associated with the specified <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <returns>The text sticky note annotation; or null, if there is no selected content to annotate.</returns>
	/// <param name="service">The annotation service to use to create the text sticky note annotation.</param>
	/// <param name="author">The author of the annotation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled.</exception>
	/// <exception cref="T:System.InvalidOperationException">The viewer control contains no content selection.</exception>
	public static Annotation CreateTextStickyNoteForSelection(AnnotationService service, string author)
	{
		return CreateStickyNoteForSelection(service, StickyNoteControl.TextSchemaName, author);
	}

	/// <summary>Creates an ink sticky note annotation on the current selection of the viewer control associated with the specified <see cref="T:System.Windows.Annotations.AnnotationService" />..</summary>
	/// <returns>The ink sticky note annotation; or null, if there is no selected content to annotate.</returns>
	/// <param name="service">The annotation service to use to create the ink sticky note annotation.</param>
	/// <param name="author">The author of the annotation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled.</exception>
	/// <exception cref="T:System.InvalidOperationException">The viewer control contains no content selection.</exception>
	public static Annotation CreateInkStickyNoteForSelection(AnnotationService service, string author)
	{
		return CreateStickyNoteForSelection(service, StickyNoteControl.InkSchemaName, author);
	}

	/// <summary>Clears all highlight annotations from the current selection of the viewer control associated with the given <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <param name="service">The annotation service from which to remove highlight annotations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled.</exception>
	public static void ClearHighlightsForSelection(AnnotationService service)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.ClearHighlightBegin);
		try
		{
			Highlight(service, null, null, create: false);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.ClearHighlightEnd);
		}
	}

	/// <summary>Deletes text sticky note annotations that are wholly contained within the current selection of the viewer control associated with the given <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <param name="service">The annotation service from which to delete text sticky note annotations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled.</exception>
	public static void DeleteTextStickyNotesForSelection(AnnotationService service)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteTextNoteBegin);
		try
		{
			DeleteSpannedAnnotations(service, StickyNoteControl.TextSchemaName);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteTextNoteEnd);
		}
	}

	/// <summary>Deletes ink sticky note annotations that are wholly contained within the current selection of the viewer control associated with the given <see cref="T:System.Windows.Annotations.AnnotationService" />.</summary>
	/// <param name="service">The annotation service from which to delete ink sticky note annotations.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="service" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="service" /> is not enabled.</exception>
	public static void DeleteInkStickyNotesForSelection(AnnotationService service)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteInkNoteBegin);
		try
		{
			DeleteSpannedAnnotations(service, StickyNoteControl.InkSchemaName);
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.DeleteInkNoteEnd);
		}
	}

	/// <summary>Returns an <see cref="T:System.Windows.Annotations.IAnchorInfo" /> object that provides anchoring information, such as the anchor location, about the specified annotation.</summary>
	/// <returns>An <see cref="T:System.Windows.Annotations.IAnchorInfo" /> object that provides anchoring information about the specified annotation, or null if it cannot be resolved.</returns>
	/// <param name="service">The annotation service to use for this operation.</param>
	/// <param name="annotation">The annotation to get anchoring information for.</param>
	public static IAnchorInfo GetAnchorInfo(AnnotationService service, Annotation annotation)
	{
		CheckInputs(service);
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		bool flag = true;
		if (!(service.Root is DocumentViewerBase documentViewerBase))
		{
			if (service.Root is FlowDocumentReader fdr)
			{
				DocumentViewerBase documentViewerBase2 = GetFdrHost(fdr) as DocumentViewerBase;
			}
		}
		else
		{
			flag = documentViewerBase.Document is FlowDocument;
		}
		IList<IAttachedAnnotation> list = null;
		if (flag)
		{
			TextSelectionProcessor textSelectionProcessor = service.LocatorManager.GetSelectionProcessor(typeof(TextRange)) as TextSelectionProcessor;
			TextSelectionProcessor textSelectionProcessor2 = service.LocatorManager.GetSelectionProcessor(typeof(TextAnchor)) as TextSelectionProcessor;
			Invariant.Assert(textSelectionProcessor != null, "TextSelectionProcessor should be available for TextRange if we are processing flow content.");
			Invariant.Assert(textSelectionProcessor2 != null, "TextSelectionProcessor should be available for TextAnchor if we are processing flow content.");
			try
			{
				textSelectionProcessor.Clamping = false;
				textSelectionProcessor2.Clamping = false;
				list = ResolveAnnotations(service, new Annotation[1] { annotation });
			}
			finally
			{
				textSelectionProcessor.Clamping = true;
				textSelectionProcessor2.Clamping = true;
			}
		}
		else
		{
			FixedPageProcessor fixedPageProcessor = service.LocatorManager.GetSubTreeProcessorForLocatorPart(FixedPageProcessor.CreateLocatorPart(0)) as FixedPageProcessor;
			Invariant.Assert(fixedPageProcessor != null, "FixedPageProcessor should be available if we are processing fixed content.");
			try
			{
				fixedPageProcessor.UseLogicalTree = true;
				list = ResolveAnnotations(service, new Annotation[1] { annotation });
			}
			finally
			{
				fixedPageProcessor.UseLogicalTree = false;
			}
		}
		Invariant.Assert(list != null);
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	internal static void OnCreateHighlightCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is DependencyObject d)
		{
			CreateHighlightForSelection(AnnotationService.GetService(d), null, (e.Parameter != null) ? (e.Parameter as Brush) : null);
		}
	}

	internal static void OnCreateTextStickyNoteCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is DependencyObject d)
		{
			CreateTextStickyNoteForSelection(AnnotationService.GetService(d), e.Parameter as string);
		}
	}

	internal static void OnCreateInkStickyNoteCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is DependencyObject d)
		{
			CreateInkStickyNoteForSelection(AnnotationService.GetService(d), e.Parameter as string);
		}
	}

	internal static void OnClearHighlightsCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is DependencyObject d)
		{
			ClearHighlightsForSelection(AnnotationService.GetService(d));
		}
	}

	internal static void OnDeleteStickyNotesCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (sender is DependencyObject d)
		{
			DeleteTextStickyNotesForSelection(AnnotationService.GetService(d));
			DeleteInkStickyNotesForSelection(AnnotationService.GetService(d));
		}
	}

	internal static void OnDeleteAnnotationsCommand(object sender, ExecutedRoutedEventArgs e)
	{
		if (!(sender is FrameworkElement frameworkElement))
		{
			return;
		}
		ITextSelection textSelection = GetTextSelection(frameworkElement);
		if (textSelection != null)
		{
			AnnotationService service = AnnotationService.GetService(frameworkElement);
			DeleteTextStickyNotesForSelection(service);
			DeleteInkStickyNotesForSelection(service);
			if (!textSelection.IsEmpty)
			{
				ClearHighlightsForSelection(service);
			}
		}
	}

	internal static void OnQueryCreateHighlightCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: true);
		e.Handled = true;
	}

	internal static void OnQueryCreateTextStickyNoteCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: true);
		e.Handled = true;
	}

	internal static void OnQueryCreateInkStickyNoteCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: true);
		e.Handled = true;
	}

	internal static void OnQueryClearHighlightsCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: true);
		e.Handled = true;
	}

	internal static void OnQueryDeleteStickyNotesCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: false);
		e.Handled = true;
	}

	internal static void OnQueryDeleteAnnotationsCommand(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = IsCommandEnabled(sender, checkForEmpty: false);
		e.Handled = true;
	}

	internal static DocumentPageView FindView(DocumentViewerBase viewer, int pageNb)
	{
		Invariant.Assert(viewer != null, "viewer is null");
		Invariant.Assert(pageNb >= 0, "negative pageNb");
		foreach (DocumentPageView pageView in viewer.PageViews)
		{
			if (pageView.PageNumber == pageNb)
			{
				return pageView;
			}
		}
		return null;
	}

	private static Annotation CreateStickyNoteForSelection(AnnotationService service, XmlQualifiedName noteType, string author)
	{
		CheckInputs(service);
		ITextSelection textSelection = GetTextSelection((FrameworkElement)service.Root);
		Invariant.Assert(textSelection != null, "TextSelection is null");
		if (textSelection.IsEmpty)
		{
			throw new InvalidOperationException(SR.EmptySelectionNotSupported);
		}
		Annotation annotation = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.CreateStickyNoteBegin);
		try
		{
			annotation = CreateAnnotationForSelection(service, textSelection, noteType, author);
			Invariant.Assert(annotation != null, "CreateAnnotationForSelection returned null.");
			service.Store.AddAnnotation(annotation);
			textSelection.SetCaretToPosition(textSelection.MovingPosition, textSelection.MovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
			return annotation;
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.CreateStickyNoteEnd);
		}
	}

	private static bool AreAllPagesVisible(DocumentViewerBase viewer, int startPage, int endPage)
	{
		Invariant.Assert(viewer != null, "viewer is null.");
		Invariant.Assert(endPage >= startPage, "EndPage is less than StartPage");
		bool result = true;
		if (viewer.PageViews.Count <= endPage - startPage)
		{
			return false;
		}
		for (int i = startPage; i <= endPage; i++)
		{
			if (FindView(viewer, i) == null)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private static IList<IAttachedAnnotation> GetSpannedAnnotations(AnnotationService service)
	{
		CheckInputs(service);
		bool flag = true;
		DocumentViewerBase documentViewerBase = service.Root as DocumentViewerBase;
		if (documentViewerBase == null)
		{
			if (service.Root is FlowDocumentReader fdr)
			{
				documentViewerBase = GetFdrHost(fdr) as DocumentViewerBase;
			}
		}
		else
		{
			flag = documentViewerBase.Document is FlowDocument;
		}
		bool flag2 = true;
		ITextSelection textSelection = GetTextSelection((FrameworkElement)service.Root);
		Invariant.Assert(textSelection != null, "TextSelection is null");
		int pageNumber = 0;
		int pageNumber2 = 0;
		if (documentViewerBase != null)
		{
			TextSelectionHelper.GetPointerPage(textSelection.Start, out pageNumber);
			TextSelectionHelper.GetPointerPage(textSelection.End, out pageNumber2);
			if (pageNumber == -1 || pageNumber2 == -1)
			{
				throw new ArgumentException(SR.InvalidSelectionPages);
			}
			flag2 = AreAllPagesVisible(documentViewerBase, pageNumber, pageNumber2);
		}
		IList<IAttachedAnnotation> list = null;
		list = (flag2 ? service.GetAttachedAnnotations() : ((!flag) ? GetSpannedAnnotationsForFixed(service, pageNumber, pageNumber2) : GetSpannedAnnotationsForFlow(service, textSelection)));
		IList<TextSegment> textSegments = textSelection.TextSegments;
		if (list != null && list.Count > 0 && (flag2 || !flag))
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!(list[num].AttachedAnchor is TextAnchor textAnchor) || !textAnchor.IsOverlapping(textSegments))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	internal static object GetFdrHost(FlowDocumentReader fdr)
	{
		Invariant.Assert(fdr != null, "Null FDR");
		Decorator decorator = null;
		if (fdr.TemplateInternal != null)
		{
			decorator = StyleHelper.FindNameInTemplateContent(fdr, "PART_ContentHost", fdr.TemplateInternal) as Decorator;
		}
		return decorator?.Child;
	}

	private static IList<IAttachedAnnotation> GetSpannedAnnotationsForFlow(AnnotationService service, ITextSelection selection)
	{
		Invariant.Assert(service != null);
		ITextPointer textPointer = selection.Start.CreatePointer();
		ITextPointer textPointer2 = selection.End.CreatePointer();
		textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
		textPointer2.MoveToNextInsertionPosition(LogicalDirection.Forward);
		ITextRange selection2 = new TextRange(textPointer, textPointer2);
		IList<ContentLocatorBase> list = service.LocatorManager.GenerateLocators(selection2);
		Invariant.Assert(list != null && list.Count > 0);
		TextSelectionProcessor textSelectionProcessor = service.LocatorManager.GetSelectionProcessor(typeof(TextRange)) as TextSelectionProcessor;
		TextSelectionProcessor textSelectionProcessor2 = service.LocatorManager.GetSelectionProcessor(typeof(TextAnchor)) as TextSelectionProcessor;
		Invariant.Assert(textSelectionProcessor != null, "TextSelectionProcessor should be available for TextRange if we are processing flow content.");
		Invariant.Assert(textSelectionProcessor2 != null, "TextSelectionProcessor should be available for TextAnchor if we are processing flow content.");
		IList<IAttachedAnnotation> list2 = null;
		IList<Annotation> list3 = null;
		try
		{
			textSelectionProcessor.Clamping = false;
			textSelectionProcessor2.Clamping = false;
			ContentLocator contentLocator = list[0] as ContentLocator;
			Invariant.Assert(contentLocator != null, "Locators for selection in Flow should always be ContentLocators.  ContentLocatorSets not supported.");
			contentLocator.Parts[contentLocator.Parts.Count - 1].NameValuePairs.Add("IncludeOverlaps", bool.TrueString);
			list3 = service.Store.GetAnnotations(contentLocator);
			return ResolveAnnotations(service, list3);
		}
		finally
		{
			textSelectionProcessor.Clamping = true;
			textSelectionProcessor2.Clamping = true;
		}
	}

	private static IList<IAttachedAnnotation> GetSpannedAnnotationsForFixed(AnnotationService service, int startPage, int endPage)
	{
		Invariant.Assert(service != null, "Need non-null service to get spanned annotations for fixed content.");
		FixedPageProcessor fixedPageProcessor = service.LocatorManager.GetSubTreeProcessorForLocatorPart(FixedPageProcessor.CreateLocatorPart(0)) as FixedPageProcessor;
		Invariant.Assert(fixedPageProcessor != null, "FixedPageProcessor should be available if we are processing fixed content.");
		List<IAttachedAnnotation> list = null;
		List<Annotation> annotations = new List<Annotation>();
		try
		{
			fixedPageProcessor.UseLogicalTree = true;
			for (int i = startPage; i <= endPage; i++)
			{
				ContentLocator contentLocator = new ContentLocator();
				contentLocator.Parts.Add(FixedPageProcessor.CreateLocatorPart(i));
				AddRange(annotations, service.Store.GetAnnotations(contentLocator));
			}
			return ResolveAnnotations(service, annotations);
		}
		finally
		{
			fixedPageProcessor.UseLogicalTree = false;
		}
	}

	private static void AddRange(List<Annotation> annotations, IList<Annotation> newAnnotations)
	{
		Invariant.Assert(annotations != null && newAnnotations != null, "annotations or newAnnotations array is null");
		foreach (Annotation newAnnotation in newAnnotations)
		{
			bool flag = true;
			foreach (Annotation annotation in annotations)
			{
				if (annotation.Id.Equals(newAnnotation.Id))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				annotations.Add(newAnnotation);
			}
		}
	}

	private static List<IAttachedAnnotation> ResolveAnnotations(AnnotationService service, IList<Annotation> annotations)
	{
		Invariant.Assert(annotations != null);
		List<IAttachedAnnotation> list = new List<IAttachedAnnotation>(annotations.Count);
		foreach (Annotation annotation in annotations)
		{
			AttachmentLevel attachmentLevel;
			object obj = service.LocatorManager.ResolveLocator(annotation.Anchors[0].ContentLocators[0], 0, service.Root, out attachmentLevel);
			if (attachmentLevel != AttachmentLevel.Incomplete && attachmentLevel != 0 && obj != null)
			{
				list.Add(new AttachedAnnotation(service.LocatorManager, annotation, annotation.Anchors[0], obj, attachmentLevel));
			}
		}
		return list;
	}

	private static void DeleteSpannedAnnotations(AnnotationService service, XmlQualifiedName annotationType)
	{
		CheckInputs(service);
		Invariant.Assert(annotationType != null && (annotationType == HighlightComponent.TypeName || annotationType == StickyNoteControl.TextSchemaName || annotationType == StickyNoteControl.InkSchemaName), "Invalid Annotation Type");
		ITextSelection textSelection = GetTextSelection((FrameworkElement)service.Root);
		Invariant.Assert(textSelection != null, "TextSelection is null");
		foreach (IAttachedAnnotation spannedAnnotation in GetSpannedAnnotations(service))
		{
			if (annotationType.Equals(spannedAnnotation.Annotation.AnnotationType) && spannedAnnotation.AttachedAnchor is TextAnchor textAnchor && ((textSelection.Start.CompareTo(textAnchor.Start) > 0 && textSelection.Start.CompareTo(textAnchor.End) < 0) || (textSelection.End.CompareTo(textAnchor.Start) > 0 && textSelection.End.CompareTo(textAnchor.End) < 0) || (textSelection.Start.CompareTo(textAnchor.Start) <= 0 && textSelection.End.CompareTo(textAnchor.End) >= 0) || CheckCaret(textSelection, textAnchor, annotationType)))
			{
				service.Store.DeleteAnnotation(spannedAnnotation.Annotation.Id);
			}
		}
	}

	private static bool CheckCaret(ITextSelection selection, TextAnchor anchor, XmlQualifiedName type)
	{
		if (!selection.IsEmpty)
		{
			return false;
		}
		if ((anchor.Start.CompareTo(selection.Start) == 0 && selection.Start.LogicalDirection == LogicalDirection.Forward) || (anchor.End.CompareTo(selection.End) == 0 && selection.End.LogicalDirection == LogicalDirection.Backward))
		{
			return true;
		}
		return false;
	}

	private static Annotation CreateAnnotationForSelection(AnnotationService service, ITextRange textSelection, XmlQualifiedName annotationType, string author)
	{
		Invariant.Assert(service != null && textSelection != null, "Parameter 'service' or 'textSelection' is null.");
		Invariant.Assert(annotationType != null && (annotationType == HighlightComponent.TypeName || annotationType == StickyNoteControl.TextSchemaName || annotationType == StickyNoteControl.InkSchemaName), "Invalid Annotation Type");
		Annotation annotation = new Annotation(annotationType);
		SetAnchor(service, annotation, textSelection);
		if (author != null)
		{
			annotation.Authors.Add(author);
		}
		return annotation;
	}

	private static Annotation Highlight(AnnotationService service, string author, Brush highlightBrush, bool create)
	{
		CheckInputs(service);
		ITextSelection textSelection = GetTextSelection((FrameworkElement)service.Root);
		Invariant.Assert(textSelection != null, "TextSelection is null");
		if (textSelection.IsEmpty)
		{
			throw new InvalidOperationException(SR.EmptySelectionNotSupported);
		}
		Color? color = null;
		if (highlightBrush != null)
		{
			if (!(highlightBrush is SolidColorBrush solidColorBrush))
			{
				throw new ArgumentException(SR.InvalidHighlightColor, "highlightBrush");
			}
			byte a = (byte)((!(solidColorBrush.Opacity <= 0.0)) ? ((!(solidColorBrush.Opacity >= 1.0)) ? ((byte)(solidColorBrush.Opacity * (double)(int)solidColorBrush.Color.A)) : solidColorBrush.Color.A) : 0);
			color = Color.FromArgb(a, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
		}
		ITextRange textRange = new TextRange(textSelection.Start, textSelection.End);
		Annotation result = ProcessHighlights(service, textRange, author, color, create);
		textSelection.SetCaretToPosition(textSelection.MovingPosition, textSelection.MovingPosition.LogicalDirection, allowStopAtLineEnd: true, allowStopNearSpace: true);
		return result;
	}

	private static Annotation ProcessHighlights(AnnotationService service, ITextRange textRange, string author, Color? color, bool create)
	{
		Invariant.Assert(textRange != null, "Parameter 'textRange' is null.");
		foreach (IAttachedAnnotation spannedAnnotation in GetSpannedAnnotations(service))
		{
			if (HighlightComponent.TypeName.Equals(spannedAnnotation.Annotation.AnnotationType))
			{
				TextAnchor obj = spannedAnnotation.FullyAttachedAnchor as TextAnchor;
				Invariant.Assert(obj != null, "FullyAttachedAnchor must not be null.");
				TextAnchor anchor = new TextAnchor(obj);
				anchor = TextAnchor.TrimToRelativeComplement(anchor, textRange.TextSegments);
				if (anchor == null || anchor.IsEmpty)
				{
					service.Store.DeleteAnnotation(spannedAnnotation.Annotation.Id);
				}
				else
				{
					SetAnchor(service, spannedAnnotation.Annotation, anchor);
				}
			}
		}
		if (create)
		{
			Annotation annotation = CreateHighlight(service, textRange, author, color);
			service.Store.AddAnnotation(annotation);
			return annotation;
		}
		return null;
	}

	private static Annotation CreateHighlight(AnnotationService service, ITextRange textRange, string author, Color? color)
	{
		Invariant.Assert(textRange != null, "textRange is null");
		Annotation annotation = CreateAnnotationForSelection(service, textRange, HighlightComponent.TypeName, author);
		if (color.HasValue)
		{
			ColorConverter colorConverter = new ColorConverter();
			XmlElement xmlElement = new XmlDocument().CreateElement("Colors", "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			xmlElement.SetAttribute("Background", colorConverter.ConvertToInvariantString(color.Value));
			AnnotationResource annotationResource = new AnnotationResource("Highlight");
			annotationResource.Contents.Add(xmlElement);
			annotation.Cargos.Add(annotationResource);
		}
		return annotation;
	}

	private static ITextSelection GetTextSelection(FrameworkElement viewer)
	{
		if (viewer is FlowDocumentReader fdr)
		{
			viewer = GetFdrHost(fdr) as FrameworkElement;
		}
		if (viewer != null)
		{
			return TextEditor.GetTextSelection(viewer);
		}
		return null;
	}

	private static void SetAnchor(AnnotationService service, Annotation annot, object selection)
	{
		Invariant.Assert(annot != null && selection != null, "null input parameter");
		IList<ContentLocatorBase> list = service.LocatorManager.GenerateLocators(selection);
		Invariant.Assert(list != null && list.Count > 0, "No locators generated for selection.");
		AnnotationResource annotationResource = new AnnotationResource();
		foreach (ContentLocatorBase item in list)
		{
			annotationResource.ContentLocators.Add(item);
		}
		annot.Anchors.Clear();
		annot.Anchors.Add(annotationResource);
	}

	private static void CheckInputs(AnnotationService service)
	{
		if (service == null)
		{
			throw new ArgumentNullException("service");
		}
		if (!service.IsEnabled)
		{
			throw new ArgumentException(SR.AnnotationServiceNotEnabled, "service");
		}
		if (!(service.Root is DocumentViewerBase documentViewerBase))
		{
			FlowDocumentScrollViewer obj = service.Root as FlowDocumentScrollViewer;
			FlowDocumentReader flowDocumentReader = service.Root as FlowDocumentReader;
			Invariant.Assert(obj != null || flowDocumentReader != null, "Service's Root must be either a FlowDocumentReader, DocumentViewerBase or a FlowDocumentScrollViewer.");
		}
		else if (documentViewerBase.Document == null)
		{
			throw new InvalidOperationException(SR.OnlyFlowFixedSupported);
		}
	}

	private static bool IsCommandEnabled(object sender, bool checkForEmpty)
	{
		Invariant.Assert(sender != null, "Parameter 'sender' is null.");
		if (sender is FrameworkElement frameworkElement)
		{
			FrameworkElement frameworkElement2 = frameworkElement.Parent as FrameworkElement;
			AnnotationService service = AnnotationService.GetService(frameworkElement);
			if (service != null && service.IsEnabled && (service.Root == frameworkElement || (frameworkElement2 != null && service.Root == frameworkElement2.TemplatedParent)))
			{
				ITextSelection textSelection = GetTextSelection(frameworkElement);
				if (textSelection != null)
				{
					if (checkForEmpty)
					{
						return !textSelection.IsEmpty;
					}
					return true;
				}
			}
		}
		return false;
	}
}
