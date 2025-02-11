using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Annotations.Storage;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Anchoring;
using MS.Internal.Annotations.Component;
using MS.Internal.Documents;

namespace System.Windows.Annotations;

/// <summary>Provides a <see cref="T:System.Windows.Documents.DocumentPaginator" /> for printing a document together with its associated annotations.</summary>
public sealed class AnnotationDocumentPaginator : DocumentPaginator
{
	private class AnnotatedDocumentPage : DocumentPage, IContentHost
	{
		private IContentHost _basePage;

		public IEnumerator<IInputElement> HostedElements
		{
			get
			{
				if (_basePage != null)
				{
					return _basePage.HostedElements;
				}
				return new HostedElements(new ReadOnlyCollection<TextSegment>(new List<TextSegment>(0)));
			}
		}

		public AnnotatedDocumentPage(DocumentPage basePage, Visual visual, Size pageSize, Rect bleedBox, Rect contentBox)
			: base(visual, pageSize, bleedBox, contentBox)
		{
			_basePage = basePage as IContentHost;
		}

		public ReadOnlyCollection<Rect> GetRectangles(ContentElement child)
		{
			if (_basePage != null)
			{
				return _basePage.GetRectangles(child);
			}
			return new ReadOnlyCollection<Rect>(new List<Rect>(0));
		}

		public IInputElement InputHitTest(Point point)
		{
			if (_basePage != null)
			{
				return _basePage.InputHitTest(point);
			}
			return null;
		}

		public void OnChildDesiredSizeChanged(UIElement child)
		{
			if (_basePage != null)
			{
				_basePage.OnChildDesiredSizeChanged(child);
			}
		}
	}

	private AnnotationStore _annotationStore;

	private DocumentPaginator _originalPaginator;

	private LocatorManager _locatorManager;

	private bool _isFixedContent;

	private FlowDirection _flowDirection;

	/// <summary>Gets a value that indicates whether <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.PageCount" /> is the total number of pages.</summary>
	/// <returns>true if pagination is complete and <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.PageCount" /> represents the total number of pages; otherwise, false if pagination is in process and <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.PageCount" /> represents the number of pages currently formatted. </returns>
	public override bool IsPageCountValid => _originalPaginator.IsPageCountValid;

	/// <summary>Gets a value that indicates the number of pages currently formatted.</summary>
	/// <returns>If <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.IsPageCountValid" /> is true, the total number of annotation pages; otherwise if <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.IsPageCountValid" /> is false, the number of pages currently formatted (pagination in process).</returns>
	public override int PageCount => _originalPaginator.PageCount;

	/// <summary>Gets or sets the suggested width and height of each page.</summary>
	/// <returns>The suggested width and height for formatting pages.</returns>
	public override Size PageSize
	{
		get
		{
			return _originalPaginator.PageSize;
		}
		set
		{
			_originalPaginator.PageSize = value;
		}
	}

	/// <summary>Gets the source document that is being paginated.</summary>
	/// <returns>The source document that is being paginated.</returns>
	public override IDocumentPaginatorSource Source => _originalPaginator.Source;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationDocumentPaginator" /> class based on a specified <see cref="T:System.Windows.Documents.DocumentPaginator" /> and annotation storage <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="originalPaginator">The document to add the printed annotations to.</param>
	/// <param name="annotationStore">The annotation storage stream to retrieve the annotations from.</param>
	public AnnotationDocumentPaginator(DocumentPaginator originalPaginator, Stream annotationStore)
		: this(originalPaginator, new XmlStreamStore(annotationStore), FlowDirection.LeftToRight)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationDocumentPaginator" /> class based on a specified <see cref="T:System.Windows.Documents.DocumentPaginator" />, annotation storage <see cref="T:System.IO.Stream" />, and text <see cref="T:System.Windows.FlowDirection" />.</summary>
	/// <param name="originalPaginator">The document to add the printed annotations to.</param>
	/// <param name="annotationStore">The annotation storage stream to retrieve the annotations from.</param>
	/// <param name="flowDirection">The text flow direction, <see cref="F:System.Windows.FlowDirection.LeftToRight" /> or <see cref="F:System.Windows.FlowDirection.RightToLeft" />.</param>
	public AnnotationDocumentPaginator(DocumentPaginator originalPaginator, Stream annotationStore, FlowDirection flowDirection)
		: this(originalPaginator, new XmlStreamStore(annotationStore), flowDirection)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationDocumentPaginator" /> class based on a specified <see cref="T:System.Windows.Documents.DocumentPaginator" /> and <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" />.</summary>
	/// <param name="originalPaginator">The document to add the printed annotations to.</param>
	/// <param name="annotationStore">The store to retrieve the annotations from.</param>
	public AnnotationDocumentPaginator(DocumentPaginator originalPaginator, AnnotationStore annotationStore)
		: this(originalPaginator, annotationStore, FlowDirection.LeftToRight)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.AnnotationDocumentPaginator" /> class based on a specified <see cref="T:System.Windows.Documents.DocumentPaginator" />, <see cref="T:System.Windows.Annotations.Storage.AnnotationStore" />, and text <see cref="T:System.Windows.FlowDirection" />.</summary>
	/// <param name="originalPaginator">The document to add the printed annotations to.</param>
	/// <param name="annotationStore">The store to retrieve the annotations from.</param>
	/// <param name="flowDirection">The text flow direction, <see cref="F:System.Windows.FlowDirection.LeftToRight" /> or <see cref="F:System.Windows.FlowDirection.RightToLeft" />.</param>
	public AnnotationDocumentPaginator(DocumentPaginator originalPaginator, AnnotationStore annotationStore, FlowDirection flowDirection)
	{
		_isFixedContent = originalPaginator is FixedDocumentPaginator || originalPaginator is FixedDocumentSequencePaginator;
		if (!_isFixedContent && !(originalPaginator is FlowDocumentPaginator))
		{
			throw new ArgumentException(SR.OnlyFlowAndFixedSupported);
		}
		_originalPaginator = originalPaginator;
		_annotationStore = annotationStore;
		_locatorManager = new LocatorManager(_annotationStore);
		_flowDirection = flowDirection;
		_originalPaginator.GetPageCompleted += HandleGetPageCompleted;
		_originalPaginator.ComputePageCountCompleted += HandleComputePageCountCompleted;
		_originalPaginator.PagesChanged += HandlePagesChanged;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Documents.DocumentPage" /> together with associated user-annotations for a specified page number.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.DocumentPage" /> for the specified <paramref name="pageNumber" />; or <see cref="F:System.Windows.Documents.DocumentPage.Missing" />, if the specified <paramref name="pageNumber" /> does not exist.</returns>
	/// <param name="pageNumber">The zero-based page number of the <see cref="T:System.Windows.Documents.DocumentPage" /> to return.</param>
	public override DocumentPage GetPage(int pageNumber)
	{
		DocumentPage documentPage = _originalPaginator.GetPage(pageNumber);
		if (documentPage != DocumentPage.Missing)
		{
			documentPage = ComposePageWithAnnotationVisuals(pageNumber, documentPage);
		}
		return documentPage;
	}

	/// <summary>asynchronously returns a <see cref="T:System.Windows.Documents.DocumentPage" /> together with associated user-annotations for a specified page number.</summary>
	/// <param name="pageNumber">The zero-based page number of the <see cref="T:System.Windows.Documents.DocumentPage" /> to retrieve.</param>
	/// <param name="userState">An application-defined object that is used to identify the asynchronous operation.</param>
	public override void GetPageAsync(int pageNumber, object userState)
	{
		_originalPaginator.GetPageAsync(pageNumber, userState);
	}

	/// <summary>Forces a pagination of the content, updates <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.PageCount" /> with the new total, and sets <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.IsPageCountValid" /> to true.</summary>
	public override void ComputePageCount()
	{
		_originalPaginator.ComputePageCount();
	}

	/// <summary>Starts an asynchronous pagination of the content, updates <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.PageCount" /> with the new total, and sets <see cref="P:System.Windows.Annotations.AnnotationDocumentPaginator.IsPageCountValid" /> to true when it is finished.</summary>
	/// <param name="userState">An application-defined object for identifying the asynchronous operation.</param>
	public override void ComputePageCountAsync(object userState)
	{
		_originalPaginator.ComputePageCountAsync(userState);
	}

	/// <summary>Cancels all asynchronous operations initiated with a given <paramref name="userState" /> object.</summary>
	/// <param name="userState">The unique application-defined identifier passed in the call to start the asynchronous operation.</param>
	public override void CancelAsync(object userState)
	{
		_originalPaginator.CancelAsync(userState);
	}

	private void HandleGetPageCompleted(object sender, GetPageCompletedEventArgs e)
	{
		if (!e.Cancelled && e.Error == null && e.DocumentPage != DocumentPage.Missing)
		{
			e = new GetPageCompletedEventArgs(ComposePageWithAnnotationVisuals(e.PageNumber, e.DocumentPage), e.PageNumber, e.Error, e.Cancelled, e.UserState);
		}
		OnGetPageCompleted(e);
	}

	private void HandleComputePageCountCompleted(object sender, AsyncCompletedEventArgs e)
	{
		OnComputePageCountCompleted(e);
	}

	private void HandlePagesChanged(object sender, PagesChangedEventArgs e)
	{
		OnPagesChanged(e);
	}

	private DocumentPage ComposePageWithAnnotationVisuals(int pageNumber, DocumentPage page)
	{
		Size size = page.Size;
		AdornerDecorator adornerDecorator = new AdornerDecorator();
		adornerDecorator.FlowDirection = _flowDirection;
		DocumentPageView documentPageView = new DocumentPageView();
		documentPageView.UseAsynchronousGetPage = false;
		documentPageView.DocumentPaginator = _originalPaginator;
		documentPageView.PageNumber = pageNumber;
		adornerDecorator.Child = documentPageView;
		adornerDecorator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		adornerDecorator.Arrange(new Rect(adornerDecorator.DesiredSize));
		adornerDecorator.UpdateLayout();
		AnnotationComponentManager annotationComponentManager = new AnnotationComponentManager(null);
		if (_isFixedContent)
		{
			AnnotationService.SetSubTreeProcessorId(adornerDecorator, FixedPageProcessor.Id);
			_locatorManager.RegisterSelectionProcessor(new FixedTextSelectionProcessor(), typeof(TextRange));
		}
		else
		{
			AnnotationService.SetDataId(adornerDecorator, "FlowDocument");
			_locatorManager.RegisterSelectionProcessor(new TextViewSelectionProcessor(), typeof(DocumentPageView));
			TextSelectionProcessor textSelectionProcessor = new TextSelectionProcessor();
			textSelectionProcessor.SetTargetDocumentPageView(documentPageView);
			_locatorManager.RegisterSelectionProcessor(textSelectionProcessor, typeof(TextRange));
		}
		foreach (IAttachedAnnotation item in ProcessAnnotations(documentPageView))
		{
			if (item.AttachmentLevel != 0 && item.AttachmentLevel != AttachmentLevel.Incomplete)
			{
				annotationComponentManager.AddAttachedAnnotation(item, reorder: false);
			}
		}
		adornerDecorator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		adornerDecorator.Arrange(new Rect(adornerDecorator.DesiredSize));
		adornerDecorator.UpdateLayout();
		return new AnnotatedDocumentPage(page, adornerDecorator, size, new Rect(size), new Rect(size));
	}

	private IList<IAttachedAnnotation> ProcessAnnotations(DocumentPageView dpv)
	{
		if (dpv == null)
		{
			throw new ArgumentNullException("dpv");
		}
		IList<IAttachedAnnotation> list = new List<IAttachedAnnotation>();
		IList<ContentLocatorBase> list2 = _locatorManager.GenerateLocators(dpv);
		if (list2.Count > 0)
		{
			ContentLocator[] array = new ContentLocator[list2.Count];
			ContentLocatorBase[] array2 = array;
			list2.CopyTo(array2, 0);
			IList<Annotation> annotations = _annotationStore.GetAnnotations(array[0]);
			foreach (ContentLocator item in list2)
			{
				if (item.Parts[item.Parts.Count - 1].NameValuePairs.ContainsKey("IncludeOverlaps"))
				{
					item.Parts.RemoveAt(item.Parts.Count - 1);
				}
			}
			foreach (Annotation item2 in annotations)
			{
				foreach (AnnotationResource anchor in item2.Anchors)
				{
					foreach (ContentLocatorBase contentLocator in anchor.ContentLocators)
					{
						AttachmentLevel attachmentLevel;
						object attachedAnchor = _locatorManager.FindAttachedAnchor(dpv, array, contentLocator, out attachmentLevel);
						if (attachmentLevel != 0)
						{
							Invariant.Assert(VisualTreeHelper.GetChildrenCount(dpv) == 1, "DocumentPageView has no visual children.");
							DependencyObject child = VisualTreeHelper.GetChild(dpv, 0);
							list.Add(new AttachedAnnotation(_locatorManager, item2, anchor, attachedAnchor, attachmentLevel, child as DocumentPageHost));
							break;
						}
					}
				}
			}
		}
		return list;
	}
}
