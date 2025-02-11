using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Annotations.Component;
using MS.Internal.Documents;
using MS.Internal.IO.Packaging;

namespace System.Windows.Documents;

/// <summary>Hosts a portable, high fidelity, fixed-format document with read access for user text selection, keyboard navigation, and search.</summary>
[ContentProperty("Pages")]
public class FixedDocument : FrameworkContentElement, IDocumentPaginatorSource, IAddChildInternal, IAddChild, IServiceProvider, IFixedNavigate, IUriContext
{
	private class GetPageAsyncRequest
	{
		internal PageContent PageContent;

		internal int PageNumber;

		internal object UserState;

		internal bool Cancelled;

		internal GetPageAsyncRequest(PageContent pageContent, int pageNumber, object userState)
		{
			PageContent = pageContent;
			PageNumber = pageNumber;
			UserState = userState;
			Cancelled = false;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedDocument.PrintTicket" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedDocument.PrintTicket" /> dependency property.</returns>
	public static readonly DependencyProperty PrintTicketProperty;

	private IDictionary<object, GetPageAsyncRequest> _asyncOps;

	private IList<PageContent> _pendingPages;

	private PageContentCollection _pages;

	private PageContent _partialPage;

	private Dictionary<FixedPage, ArrayList> _highlights;

	private double _pageWidth = 816.0;

	private double _pageHeight = 1056.0;

	private FixedTextContainer _fixedTextContainer;

	private RubberbandSelector _rubberbandSelector;

	private bool _navigateAfterPagination;

	private string _navigateFragment;

	private FixedDocumentPaginator _paginator;

	private DocumentReference _documentReference;

	private bool _hasExplicitStructure;

	private const string _structureRelationshipName = "http://schemas.microsoft.com/xps/2005/06/documentstructure";

	private const string _storyFragmentsRelationshipName = "http://schemas.microsoft.com/xps/2005/06/storyfragments";

	private static readonly ContentType _storyFragmentsContentType;

	private static readonly ContentType _documentStructureContentType;

	private static DependencyObjectType UIElementType;

	/// <summary>Gets or sets the base URI of the current application context. </summary>
	/// <returns>The base URI of the application context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return (Uri)GetValue(BaseUriHelper.BaseUriProperty);
		}
		set
		{
			SetValue(BaseUriHelper.BaseUriProperty, value);
		}
	}

	internal NavigationService NavigationService
	{
		get
		{
			return (NavigationService)GetValue(NavigationService.NavigationServiceProperty);
		}
		set
		{
			SetValue(NavigationService.NavigationServiceProperty, value);
		}
	}

	/// <summary>Gets an enumerator for accessing the document's <see cref="T:System.Windows.Documents.PageContent" /> child elements.</summary>
	/// <returns>An enumerator for accessing the document's <see cref="T:System.Windows.Documents.PageContent" /> child elements.</returns>
	protected internal override IEnumerator LogicalChildren => Pages.GetEnumerator();

	/// <summary>Gets the paginator for the <see cref="T:System.Windows.Documents.FixedDocument" /> that provides page-oriented services such as getting a particular page and repaginating in response to changes. </summary>
	/// <returns>An object of a class derived from <see cref="T:System.Windows.Documents.DocumentPaginator" /> that provides pagination services.</returns>
	public DocumentPaginator DocumentPaginator => _paginator;

	internal bool IsPageCountValid => base.IsInitialized;

	internal int PageCount => Pages.Count;

	internal Size PageSize
	{
		get
		{
			return new Size(_pageWidth, _pageHeight);
		}
		set
		{
			_pageWidth = value.Width;
			_pageHeight = value.Height;
		}
	}

	internal bool HasExplicitStructure => _hasExplicitStructure;

	/// <summary>Gets a collection of the document's <see cref="T:System.Windows.Documents.PageContent" /> elements. </summary>
	/// <returns>A collection of the document's <see cref="T:System.Windows.Documents.PageContent" /> elements.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public PageContentCollection Pages => _pages;

	/// <summary>Gets or sets the <see cref="T:System.Printing.PrintTicket" /> that is associated with this document. </summary>
	/// <returns>The <see cref="T:System.Printing.PrintTicket" /> for this document.</returns>
	public object PrintTicket
	{
		get
		{
			return GetValue(PrintTicketProperty);
		}
		set
		{
			SetValue(PrintTicketProperty, value);
		}
	}

	internal FixedTextContainer FixedContainer
	{
		get
		{
			if (_fixedTextContainer == null)
			{
				_fixedTextContainer = new FixedTextContainer(this);
				_fixedTextContainer.Highlights.Changed += OnHighlightChanged;
			}
			return _fixedTextContainer;
		}
	}

	internal Dictionary<FixedPage, ArrayList> Highlights => _highlights;

	internal DocumentReference DocumentReference
	{
		get
		{
			return _documentReference;
		}
		set
		{
			_documentReference = value;
		}
	}

	static FixedDocument()
	{
		PrintTicketProperty = DependencyProperty.RegisterAttached("PrintTicket", typeof(object), typeof(FixedDocument), new FrameworkPropertyMetadata((object)null));
		_storyFragmentsContentType = new ContentType("application/vnd.ms-package.xps-storyfragments+xml");
		_documentStructureContentType = new ContentType("application/vnd.ms-package.xps-documentstructure+xml");
		UIElementType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement));
		ContentElement.FocusableProperty.OverrideMetadata(typeof(FixedDocument), new FrameworkPropertyMetadata(true));
		NavigationService.NavigationServiceProperty.OverrideMetadata(typeof(FixedDocument), new FrameworkPropertyMetadata(FixedHyperLink.OnNavigationServiceChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.FixedDocument" /> class. </summary>
	public FixedDocument()
	{
		_Init();
	}

	/// <summary>Gets the service object of the specified type.</summary>
	/// <returns>A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType">An object that specifies the type of service object to get. </param>
	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		if (serviceType == typeof(ITextContainer))
		{
			return FixedContainer;
		}
		if (serviceType == typeof(RubberbandSelector))
		{
			if (_rubberbandSelector == null)
			{
				_rubberbandSelector = new RubberbandSelector();
			}
			return _rubberbandSelector;
		}
		return null;
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is PageContent pageContent))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(PageContent)), "value");
		}
		if (pageContent.IsInitialized)
		{
			_pages.Add(pageContent);
			return;
		}
		if (_partialPage == null)
		{
			_partialPage = pageContent;
			_partialPage.ChangeLogicalParent(this);
			_partialPage.Initialized += OnPageLoaded;
			return;
		}
		throw new InvalidOperationException(SR.PrevoiusPartialPageContentOutstanding);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	void IFixedNavigate.NavigateAsync(string elementID)
	{
		if (IsPageCountValid)
		{
			FixedHyperLink.NavigateToElement(this, elementID);
			return;
		}
		_navigateAfterPagination = true;
		_navigateFragment = elementID;
	}

	UIElement IFixedNavigate.FindElementByID(string elementID, out FixedPage rootFixedPage)
	{
		UIElement uIElement = null;
		rootFixedPage = null;
		if (char.IsDigit(elementID[0]))
		{
			int num = Convert.ToInt32(elementID, CultureInfo.InvariantCulture);
			num--;
			uIElement = GetFixedPage(num);
			rootFixedPage = GetFixedPage(num);
		}
		else
		{
			PageContentCollection pages = Pages;
			int i = 0;
			for (int count = pages.Count; i < count; i++)
			{
				PageContent pageContent = pages[i];
				if (pageContent.PageStream != null)
				{
					FixedPage fixedPage = GetFixedPage(i);
					if (fixedPage != null)
					{
						uIElement = ((IFixedNavigate)fixedPage).FindElementByID(elementID, out rootFixedPage);
						if (uIElement != null)
						{
							break;
						}
					}
				}
				else
				{
					if (!pageContent.ContainsID(elementID))
					{
						continue;
					}
					FixedPage fixedPage = GetFixedPage(i);
					if (fixedPage != null)
					{
						uIElement = ((IFixedNavigate)fixedPage).FindElementByID(elementID, out rootFixedPage);
						if (uIElement == null)
						{
							uIElement = fixedPage;
						}
						break;
					}
				}
			}
		}
		return uIElement;
	}

	internal DocumentPage GetPage(int pageNumber)
	{
		if (pageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("pageNumber", SR.IDPNegativePageNumber);
		}
		if (pageNumber < Pages.Count)
		{
			FixedPage fixedPage = SyncGetPage(pageNumber, forceReload: false);
			if (fixedPage == null)
			{
				return DocumentPage.Missing;
			}
			Size size = ComputePageSize(fixedPage);
			FixedDocumentPage result = new FixedDocumentPage(this, fixedPage, size, pageNumber);
			fixedPage.Measure(size);
			fixedPage.Arrange(new Rect(default(Point), size));
			return result;
		}
		return DocumentPage.Missing;
	}

	internal void GetPageAsync(int pageNumber, object userState)
	{
		if (pageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("pageNumber", SR.IDPNegativePageNumber);
		}
		if (userState == null)
		{
			throw new ArgumentNullException("userState");
		}
		if (pageNumber < Pages.Count)
		{
			GetPageAsyncRequest getPageAsyncRequest = new GetPageAsyncRequest(Pages[pageNumber], pageNumber, userState);
			_asyncOps[userState] = getPageAsyncRequest;
			DispatcherOperationCallback method = GetPageAsyncDelegate;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, method, getPageAsyncRequest);
		}
		else
		{
			_NotifyGetPageAsyncCompleted(DocumentPage.Missing, pageNumber, null, cancelled: false, userState);
		}
	}

	internal int GetPageNumber(ContentPosition contentPosition)
	{
		if (contentPosition == null)
		{
			throw new ArgumentNullException("contentPosition");
		}
		if (!(contentPosition is FixedTextPointer fixedTextPointer))
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition);
		}
		return fixedTextPointer.FixedTextContainer.GetPageNumber(fixedTextPointer);
	}

	internal void CancelAsync(object userState)
	{
		if (userState == null)
		{
			throw new ArgumentNullException("userState");
		}
		if (_asyncOps.TryGetValue(userState, out var value) && value != null)
		{
			value.Cancelled = true;
			value.PageContent.GetPageRootAsyncCancel();
		}
	}

	internal ContentPosition GetObjectPosition(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		if (!(o is DependencyObject dependencyObject))
		{
			throw new ArgumentException(SR.FixedDocumentExpectsDependencyObject);
		}
		FixedPage fixedPage = null;
		int num = -1;
		if (dependencyObject != this)
		{
			DependencyObject dependencyObject2 = dependencyObject;
			while (dependencyObject2 != null)
			{
				fixedPage = dependencyObject2 as FixedPage;
				if (fixedPage != null)
				{
					num = GetIndexOfPage(fixedPage);
					if (num >= 0)
					{
						break;
					}
					dependencyObject2 = fixedPage.Parent;
				}
				else
				{
					dependencyObject2 = LogicalTreeHelper.GetParent(dependencyObject2);
				}
			}
		}
		else if (Pages.Count > 0)
		{
			num = 0;
		}
		FixedTextPointer fixedTextPointer = null;
		if (num >= 0)
		{
			FlowPosition flowPosition = null;
			System.Windows.Shapes.Path path = dependencyObject as System.Windows.Shapes.Path;
			if (dependencyObject is Glyphs || dependencyObject is Image || (path != null && path.Fill is ImageBrush))
			{
				FixedPosition fixedPosition = new FixedPosition(fixedPage.CreateFixedNode(num, (UIElement)dependencyObject), 0);
				flowPosition = FixedContainer.FixedTextBuilder.CreateFlowPosition(fixedPosition);
			}
			if (flowPosition == null)
			{
				flowPosition = FixedContainer.FixedTextBuilder.GetPageStartFlowPosition(num);
			}
			fixedTextPointer = new FixedTextPointer(mutable: true, LogicalDirection.Forward, flowPosition);
		}
		if (fixedTextPointer == null)
		{
			return ContentPosition.Missing;
		}
		return fixedTextPointer;
	}

	internal ContentPosition GetPagePosition(DocumentPage page)
	{
		if (!(page is FixedDocumentPage fixedDocumentPage))
		{
			return ContentPosition.Missing;
		}
		return fixedDocumentPage.ContentPosition;
	}

	/// <summary>Creates an automation peer for the document. </summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> that exposes the <see cref="T:System.Windows.Documents.FixedDocument" /> to Microsoft UI Automation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentAutomationPeer(this);
	}

	internal int GetIndexOfPage(FixedPage p)
	{
		PageContentCollection pages = Pages;
		int i = 0;
		for (int count = pages.Count; i < count; i++)
		{
			if (pages[i].IsOwnerOf(p))
			{
				return i;
			}
		}
		return -1;
	}

	internal bool IsValidPageIndex(int index)
	{
		if (index >= 0)
		{
			return index < Pages.Count;
		}
		return false;
	}

	internal FixedPage SyncGetPageWithCheck(int index)
	{
		if (IsValidPageIndex(index))
		{
			return SyncGetPage(index, forceReload: false);
		}
		return null;
	}

	internal FixedPage SyncGetPage(int index, bool forceReload)
	{
		PageContentCollection pages = Pages;
		try
		{
			return pages[index].GetPageRoot(forceReload);
		}
		catch (Exception ex)
		{
			if (ex is InvalidOperationException || ex is ApplicationException)
			{
				throw new ApplicationException(string.Format(CultureInfo.CurrentCulture, SR.ExceptionInGetPage, index), ex);
			}
			throw;
		}
	}

	internal void OnPageContentAppended(int index)
	{
		FixedContainer.FixedTextBuilder.AddVirtualPage();
		_paginator.NotifyPaginationProgress(new PaginationProgressEventArgs(index, 1));
		if (base.IsInitialized)
		{
			_paginator.NotifyPagesChanged(new PagesChangedEventArgs(index, 1));
		}
	}

	internal void EnsurePageSize(FixedPage fp)
	{
		if (double.IsNaN(fp.Width))
		{
			fp.Width = _pageWidth;
		}
		if (double.IsNaN(fp.Height))
		{
			fp.Height = _pageHeight;
		}
	}

	internal bool GetPageSize(ref Size pageSize, int pageNumber)
	{
		if (pageNumber < Pages.Count)
		{
			FixedPage fp = null;
			if (!_pendingPages.Contains(Pages[pageNumber]))
			{
				fp = SyncGetPage(pageNumber, forceReload: false);
			}
			pageSize = ComputePageSize(fp);
			return true;
		}
		return false;
	}

	internal Size ComputePageSize(FixedPage fp)
	{
		if (fp == null)
		{
			return new Size(_pageWidth, _pageHeight);
		}
		EnsurePageSize(fp);
		return new Size(fp.Width, fp.Height);
	}

	private void _Init()
	{
		_paginator = new FixedDocumentPaginator(this);
		_pages = new PageContentCollection(this);
		_highlights = new Dictionary<FixedPage, ArrayList>();
		_asyncOps = new Dictionary<object, GetPageAsyncRequest>();
		_pendingPages = new List<PageContent>();
		_hasExplicitStructure = false;
		base.Initialized += OnInitialized;
	}

	private void OnInitialized(object sender, EventArgs e)
	{
		if (_navigateAfterPagination)
		{
			FixedHyperLink.NavigateToElement(this, _navigateFragment);
			_navigateAfterPagination = false;
		}
		ValidateDocStructure();
		if (PageCount > 0)
		{
			DocumentPage page = GetPage(0);
			if (page != null && page.Visual is FixedPage fixedPage)
			{
				base.Language = fixedPage.Language;
			}
		}
		_paginator.NotifyPaginationCompleted(e);
	}

	internal void ValidateDocStructure()
	{
		Uri baseUri = BaseUriHelper.GetBaseUri(this);
		if (!baseUri.Scheme.Equals(System.IO.Packaging.PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) || baseUri.Host.Equals(BaseUriHelper.PackAppBaseUri.Host) || baseUri.Host.Equals(BaseUriHelper.SiteOfOriginBaseUri.Host))
		{
			return;
		}
		Uri structureUriFromRelationship = GetStructureUriFromRelationship(baseUri, "http://schemas.microsoft.com/xps/2005/06/documentstructure");
		if (structureUriFromRelationship != null)
		{
			ValidateAndLoadPartFromAbsoluteUri(structureUriFromRelationship, validateOnly: true, "DocumentStructure", out var mimeType);
			if (!_documentStructureContentType.AreTypeAndSubTypeEqual(mimeType))
			{
				throw new FileFormatException(SR.InvalidDSContentType);
			}
			_hasExplicitStructure = true;
		}
	}

	internal static StoryFragments GetStoryFragments(FixedPage fixedPage)
	{
		object obj = null;
		Uri baseUri = BaseUriHelper.GetBaseUri(fixedPage);
		if (baseUri.Scheme.Equals(System.IO.Packaging.PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) && !baseUri.Host.Equals(BaseUriHelper.PackAppBaseUri.Host) && !baseUri.Host.Equals(BaseUriHelper.SiteOfOriginBaseUri.Host))
		{
			Uri structureUriFromRelationship = GetStructureUriFromRelationship(baseUri, "http://schemas.microsoft.com/xps/2005/06/storyfragments");
			if (structureUriFromRelationship != null)
			{
				obj = ValidateAndLoadPartFromAbsoluteUri(structureUriFromRelationship, validateOnly: false, null, out var mimeType);
				if (!_storyFragmentsContentType.AreTypeAndSubTypeEqual(mimeType))
				{
					throw new FileFormatException(SR.InvalidSFContentType);
				}
				if (!(obj is StoryFragments))
				{
					throw new FileFormatException(SR.InvalidStoryFragmentsMarkup);
				}
			}
		}
		return obj as StoryFragments;
	}

	private static object ValidateAndLoadPartFromAbsoluteUri(Uri AbsoluteUriDoc, bool validateOnly, string rootElement, out ContentType mimeType)
	{
		mimeType = null;
		Stream stream = null;
		object result = null;
		try
		{
			stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(AbsoluteUriDoc, out mimeType);
			ParserContext parserContext = new ParserContext();
			parserContext.BaseUri = AbsoluteUriDoc;
			XpsValidatingLoader xpsValidatingLoader = new XpsValidatingLoader();
			if (validateOnly)
			{
				xpsValidatingLoader.Validate(stream, null, parserContext, mimeType, rootElement);
			}
			else
			{
				result = xpsValidatingLoader.Load(stream, null, parserContext, mimeType);
			}
		}
		catch (Exception ex)
		{
			if (!(ex is WebException) && !(ex is InvalidOperationException))
			{
				throw;
			}
		}
		return result;
	}

	private static Uri GetStructureUriFromRelationship(Uri contentUri, string relationshipName)
	{
		Uri result = null;
		if (contentUri != null && relationshipName != null)
		{
			Uri partUri = System.IO.Packaging.PackUriHelper.GetPartUri(contentUri);
			if (partUri != null)
			{
				Uri packageUri = System.IO.Packaging.PackUriHelper.GetPackageUri(contentUri);
				Package package = PreloadedPackages.GetPackage(packageUri);
				if (package == null)
				{
					package = PackageStore.GetPackage(packageUri);
				}
				if (package != null)
				{
					PackageRelationshipCollection relationshipsByType = package.GetPart(partUri).GetRelationshipsByType(relationshipName);
					Uri uri = null;
					foreach (PackageRelationship item in relationshipsByType)
					{
						uri = System.IO.Packaging.PackUriHelper.ResolvePartUri(partUri, item.TargetUri);
					}
					if (uri != null)
					{
						result = System.IO.Packaging.PackUriHelper.Create(packageUri, uri);
					}
				}
			}
		}
		return result;
	}

	private void OnPageLoaded(object sender, EventArgs e)
	{
		if ((PageContent)sender == _partialPage)
		{
			_partialPage.Initialized -= OnPageLoaded;
			_pages.Add(_partialPage);
			_partialPage = null;
		}
	}

	internal FixedPage GetFixedPage(int pageNumber)
	{
		FixedPage result = null;
		if (GetPage(pageNumber) is FixedDocumentPage fixedDocumentPage && fixedDocumentPage != DocumentPage.Missing)
		{
			result = fixedDocumentPage.FixedPage;
		}
		return result;
	}

	private void OnHighlightChanged(object sender, HighlightChangedEventArgs args)
	{
		ITextContainer fixedContainer = FixedContainer;
		Highlights highlights = null;
		highlights = ((!(base.Parent is FixedDocumentSequence fixedDocumentSequence)) ? FixedContainer.Highlights : fixedDocumentSequence.TextContainer.Highlights);
		List<FixedPage> list = new List<FixedPage>();
		foreach (FixedPage key in _highlights.Keys)
		{
			list.Add(key);
		}
		_highlights.Clear();
		StaticTextPointer staticTextPointer = fixedContainer.CreateStaticPointerAtOffset(0);
		while (true)
		{
			if (!highlights.IsContentHighlighted(staticTextPointer, LogicalDirection.Forward))
			{
				staticTextPointer = highlights.GetNextHighlightChangePosition(staticTextPointer, LogicalDirection.Forward);
				if (staticTextPointer.IsNull)
				{
					break;
				}
			}
			object highlightValue = highlights.GetHighlightValue(staticTextPointer, LogicalDirection.Forward, typeof(TextSelection));
			StaticTextPointer textPosition = staticTextPointer;
			FixedHighlightType fixedHighlightType = FixedHighlightType.None;
			Brush foregroundBrush = null;
			Brush backgroundBrush = null;
			if (highlightValue != DependencyProperty.UnsetValue)
			{
				do
				{
					staticTextPointer = highlights.GetNextHighlightChangePosition(staticTextPointer, LogicalDirection.Forward);
				}
				while (highlights.GetHighlightValue(staticTextPointer, LogicalDirection.Forward, typeof(TextSelection)) != DependencyProperty.UnsetValue);
				fixedHighlightType = FixedHighlightType.TextSelection;
				foregroundBrush = null;
				backgroundBrush = null;
			}
			else if (highlights.GetHighlightValue(textPosition, LogicalDirection.Forward, typeof(HighlightComponent)) is AnnotationHighlightLayer.HighlightSegment highlightSegment)
			{
				staticTextPointer = highlights.GetNextHighlightChangePosition(staticTextPointer, LogicalDirection.Forward);
				fixedHighlightType = FixedHighlightType.AnnotationHighlight;
				backgroundBrush = highlightSegment.Fill;
			}
			if (fixedHighlightType != 0)
			{
				FixedContainer.GetMultiHighlights((FixedTextPointer)textPosition.CreateDynamicTextPointer(LogicalDirection.Forward), (FixedTextPointer)staticTextPointer.CreateDynamicTextPointer(LogicalDirection.Forward), _highlights, fixedHighlightType, foregroundBrush, backgroundBrush);
			}
		}
		ArrayList arrayList = new ArrayList();
		IList ranges = args.Ranges;
		for (int i = 0; i < ranges.Count; i++)
		{
			TextSegment textSegment = (TextSegment)ranges[i];
			int pageNumber = FixedContainer.GetPageNumber(textSegment.Start);
			int pageNumber2 = FixedContainer.GetPageNumber(textSegment.End);
			for (int j = pageNumber; j <= pageNumber2; j++)
			{
				if (arrayList.IndexOf(j) < 0)
				{
					arrayList.Add(j);
				}
			}
		}
		ICollection<FixedPage> keys = _highlights.Keys;
		foreach (FixedPage item in list)
		{
			if (!keys.Contains(item))
			{
				int indexOfPage = GetIndexOfPage(item);
				if (indexOfPage >= 0 && indexOfPage < PageCount && arrayList.IndexOf(indexOfPage) < 0)
				{
					arrayList.Add(indexOfPage);
				}
			}
		}
		arrayList.Sort();
		foreach (int item2 in arrayList)
		{
			HighlightVisual.GetHighlightVisual(SyncGetPage(item2, forceReload: false))?.InvalidateHighlights();
		}
	}

	private object GetPageAsyncDelegate(object arg)
	{
		GetPageAsyncRequest getPageAsyncRequest = (GetPageAsyncRequest)arg;
		PageContent pageContent = getPageAsyncRequest.PageContent;
		if (!_pendingPages.Contains(pageContent))
		{
			_pendingPages.Add(pageContent);
			pageContent.GetPageRootCompleted += OnGetPageRootCompleted;
			pageContent.GetPageRootAsync(forceReload: false);
			if (getPageAsyncRequest.Cancelled)
			{
				pageContent.GetPageRootAsyncCancel();
			}
		}
		return null;
	}

	private void OnGetPageRootCompleted(object sender, GetPageRootCompletedEventArgs args)
	{
		PageContent pageContent = (PageContent)sender;
		pageContent.GetPageRootCompleted -= OnGetPageRootCompleted;
		_pendingPages.Remove(pageContent);
		ArrayList arrayList = new ArrayList();
		IEnumerator<KeyValuePair<object, GetPageAsyncRequest>> enumerator = _asyncOps.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GetPageAsyncRequest value = enumerator.Current.Value;
				if (value.PageContent == pageContent)
				{
					arrayList.Add(enumerator.Current.Key);
					DocumentPage page = DocumentPage.Missing;
					if (!value.Cancelled && !args.Cancelled && args.Error == null)
					{
						FixedPage result = args.Result;
						Size fixedSize = ComputePageSize(result);
						page = new FixedDocumentPage(this, result, fixedSize, Pages.IndexOf(pageContent));
					}
					_NotifyGetPageAsyncCompleted(page, value.PageNumber, args.Error, value.Cancelled, value.UserState);
				}
			}
		}
		finally
		{
			foreach (object item in arrayList)
			{
				_asyncOps.Remove(item);
			}
		}
	}

	private void _NotifyGetPageAsyncCompleted(DocumentPage page, int pageNumber, Exception error, bool cancelled, object userState)
	{
		_paginator.NotifyGetPageCompleted(new GetPageCompletedEventArgs(page, pageNumber, error, cancelled, userState));
	}
}
