using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal.Documents;

namespace System.Windows.Documents;

/// <summary>Hosts one or more <see cref="T:System.Windows.Documents.DocumentReference" /> elements that define a sequence of fixed documents. </summary>
[ContentProperty("References")]
public class FixedDocumentSequence : FrameworkContentElement, IDocumentPaginatorSource, IAddChildInternal, IAddChild, IServiceProvider, IFixedNavigate, IUriContext
{
	private struct RequestedPage
	{
		internal DynamicDocumentPaginator ChildPaginator;

		internal int ChildPageNumber;

		internal int PageNumber;

		internal RequestedPage(int pageNumber)
		{
			PageNumber = pageNumber;
			ChildPageNumber = 0;
			ChildPaginator = null;
		}

		public override int GetHashCode()
		{
			return PageNumber;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is RequestedPage))
			{
				return false;
			}
			return Equals((RequestedPage)obj);
		}

		public bool Equals(RequestedPage obj)
		{
			return PageNumber == obj.PageNumber;
		}

		public static bool operator ==(RequestedPage obj1, RequestedPage obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(RequestedPage obj1, RequestedPage obj2)
		{
			return !obj1.Equals(obj2);
		}
	}

	private class GetPageAsyncRequest
	{
		internal RequestedPage Page;

		internal object UserState;

		internal bool Cancelled;

		internal GetPageAsyncRequest(RequestedPage page, object userState)
		{
			Page = page;
			UserState = userState;
			Cancelled = false;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedDocumentSequence.PrintTicket" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedDocumentSequence.PrintTicket" /> dependency property.</returns>
	public static readonly DependencyProperty PrintTicketProperty;

	private DocumentReferenceCollection _references;

	private DocumentReference _partialRef;

	private FixedDocumentSequencePaginator _paginator;

	private IDictionary<object, GetPageAsyncRequest> _asyncOps;

	private IList<RequestedPage> _pendingPages;

	private Size _pageSize;

	private bool _navigateAfterPagination;

	private string _navigateFragment;

	private DocumentSequenceTextContainer _textContainer;

	private RubberbandSelector _rubberbandSelector;

	/// <summary>Gets an enumerator for accessing the document sequence's <see cref="T:System.Windows.Documents.DocumentReference" /> child elements. </summary>
	/// <returns>An enumerator for accessing the document sequence's <see cref="T:System.Windows.Documents.DocumentReference" /> child elements.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			DocumentReference[] array = new DocumentReference[_references.Count];
			_references.CopyTo(array, 0);
			return array.GetEnumerator();
		}
	}

	/// <summary>Gets the paginator for the <see cref="T:System.Windows.Documents.FixedDocument" /> that provides page-oriented services such as getting a particular page and repaginating in response to changes. </summary>
	/// <returns>An object of a class derived from <see cref="T:System.Windows.Documents.DocumentPaginator" /> that provides pagination services</returns>
	public DocumentPaginator DocumentPaginator => _paginator;

	internal bool IsPageCountValid
	{
		get
		{
			bool result = true;
			if (base.IsInitialized)
			{
				foreach (DocumentReference reference in References)
				{
					DynamicDocumentPaginator paginator = GetPaginator(reference);
					if (paginator == null || !paginator.IsPageCountValid)
					{
						result = false;
						break;
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}
	}

	internal int PageCount
	{
		get
		{
			int num = 0;
			foreach (DocumentReference reference in References)
			{
				DynamicDocumentPaginator paginator = GetPaginator(reference);
				if (paginator != null)
				{
					num += paginator.PageCount;
					if (!paginator.IsPageCountValid)
					{
						break;
					}
				}
			}
			return num;
		}
	}

	internal Size PageSize
	{
		get
		{
			return _pageSize;
		}
		set
		{
			_pageSize = value;
		}
	}

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

	/// <summary>Gets a collection of the document sequence's <see cref="T:System.Windows.Documents.DocumentReference" /> child elements. </summary>
	/// <returns>A collection of the document sequence's <see cref="T:System.Windows.Documents.DocumentReference" /> child elements.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[CLSCompliant(false)]
	public DocumentReferenceCollection References => _references;

	/// <summary>Gets or sets the <see cref="T:System.Printing.PrintTicket" /> that is associated with this document sequence. </summary>
	/// <returns>The <see cref="T:System.Printing.PrintTicket" /> for this sequence.</returns>
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

	internal DocumentSequenceTextContainer TextContainer
	{
		get
		{
			if (_textContainer == null)
			{
				_textContainer = new DocumentSequenceTextContainer(this);
			}
			return _textContainer;
		}
	}

	static FixedDocumentSequence()
	{
		PrintTicketProperty = DependencyProperty.RegisterAttached("PrintTicket", typeof(object), typeof(FixedDocumentSequence), new FrameworkPropertyMetadata((object)null));
		ContentElement.FocusableProperty.OverrideMetadata(typeof(FixedDocumentSequence), new FrameworkPropertyMetadata(true));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> class.</summary>
	public FixedDocumentSequence()
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
			return TextContainer;
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
		if (!(value is DocumentReference documentReference))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(DocumentReference)), "value");
		}
		if (documentReference.IsInitialized)
		{
			_references.Add(documentReference);
			return;
		}
		if (_partialRef == null)
		{
			_partialRef = documentReference;
			_partialRef.Initialized += _OnDocumentReferenceInitialized;
			return;
		}
		throw new InvalidOperationException(SR.PrevoiusUninitializedDocumentReferenceOutstanding);
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
			int pageNumber = Convert.ToInt32(elementID, CultureInfo.InvariantCulture) - 1;
			if (TranslatePageNumber(pageNumber, out var childPaginator, out var childPageNumber) && childPaginator.Source is FixedDocument fixedDocument)
			{
				uIElement = fixedDocument.GetFixedPage(childPageNumber);
			}
		}
		else
		{
			foreach (DocumentReference reference in References)
			{
				DynamicDocumentPaginator childPaginator = GetPaginator(reference);
				if (childPaginator.Source is FixedDocument fixedDocument2)
				{
					uIElement = ((IFixedNavigate)fixedDocument2).FindElementByID(elementID, out rootFixedPage);
					if (uIElement != null)
					{
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
		DocumentPage documentPage = null;
		if (TranslatePageNumber(pageNumber, out var childPaginator, out var childPageNumber))
		{
			documentPage = childPaginator.GetPage(childPageNumber);
			return new FixedDocumentSequenceDocumentPage(this, childPaginator, documentPage);
		}
		return DocumentPage.Missing;
	}

	internal DocumentPage GetPage(FixedDocument document, int fixedDocPageNumber)
	{
		if (fixedDocPageNumber < 0)
		{
			throw new ArgumentOutOfRangeException("fixedDocPageNumber", SR.IDPNegativePageNumber);
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		DocumentPage page = document.GetPage(fixedDocPageNumber);
		return new FixedDocumentSequenceDocumentPage(this, document.DocumentPaginator as DynamicDocumentPaginator, page);
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
		GetPageAsyncRequest getPageAsyncRequest = new GetPageAsyncRequest(new RequestedPage(pageNumber), userState);
		_asyncOps[userState] = getPageAsyncRequest;
		DispatcherOperationCallback method = _GetPageAsyncDelegate;
		base.Dispatcher.BeginInvoke(DispatcherPriority.Background, method, getPageAsyncRequest);
	}

	internal int GetPageNumber(ContentPosition contentPosition)
	{
		if (contentPosition == null)
		{
			throw new ArgumentNullException("contentPosition");
		}
		DynamicDocumentPaginator dynamicDocumentPaginator = null;
		ContentPosition contentPosition2 = null;
		if (contentPosition is DocumentSequenceTextPointer)
		{
			DocumentSequenceTextPointer documentSequenceTextPointer = (DocumentSequenceTextPointer)contentPosition;
			dynamicDocumentPaginator = GetPaginator(documentSequenceTextPointer.ChildBlock.DocRef);
			contentPosition2 = documentSequenceTextPointer.ChildPointer as ContentPosition;
		}
		if (contentPosition2 == null)
		{
			throw new ArgumentException(SR.IDPInvalidContentPosition);
		}
		int pageNumber = dynamicDocumentPaginator.GetPageNumber(contentPosition2);
		_SynthesizeGlobalPageNumber(dynamicDocumentPaginator, pageNumber, out var pageNumber2);
		return pageNumber2;
	}

	internal void CancelAsync(object userState)
	{
		if (userState == null)
		{
			throw new ArgumentNullException("userState");
		}
		if (!_asyncOps.ContainsKey(userState))
		{
			return;
		}
		GetPageAsyncRequest getPageAsyncRequest = _asyncOps[userState];
		if (getPageAsyncRequest != null)
		{
			getPageAsyncRequest.Cancelled = true;
			if (getPageAsyncRequest.Page.ChildPaginator != null)
			{
				getPageAsyncRequest.Page.ChildPaginator.CancelAsync(getPageAsyncRequest);
			}
		}
	}

	internal ContentPosition GetObjectPosition(object o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		foreach (DocumentReference reference in References)
		{
			DynamicDocumentPaginator paginator = GetPaginator(reference);
			if (paginator != null)
			{
				ContentPosition objectPosition = paginator.GetObjectPosition(o);
				if (objectPosition != ContentPosition.Missing && objectPosition is ITextPointer)
				{
					return new DocumentSequenceTextPointer(new ChildDocumentBlock(TextContainer, reference), (ITextPointer)objectPosition);
				}
			}
		}
		return ContentPosition.Missing;
	}

	internal ContentPosition GetPagePosition(DocumentPage page)
	{
		if (!(page is FixedDocumentSequenceDocumentPage fixedDocumentSequenceDocumentPage))
		{
			return ContentPosition.Missing;
		}
		return fixedDocumentSequenceDocumentPage.ContentPosition;
	}

	/// <summary>Creates an automation peer for the sequence. </summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.DocumentAutomationPeer" /> that exposes the <see cref="T:System.Windows.Documents.FixedDocumentSequence" /> to Microsoft UI Automation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentAutomationPeer(this);
	}

	internal DynamicDocumentPaginator GetPaginator(DocumentReference docRef)
	{
		DynamicDocumentPaginator dynamicDocumentPaginator = null;
		IDocumentPaginatorSource currentlyLoadedDoc = docRef.CurrentlyLoadedDoc;
		if (currentlyLoadedDoc != null)
		{
			dynamicDocumentPaginator = currentlyLoadedDoc.DocumentPaginator as DynamicDocumentPaginator;
		}
		else
		{
			currentlyLoadedDoc = docRef.GetDocument(forceReload: false);
			if (currentlyLoadedDoc != null)
			{
				dynamicDocumentPaginator = currentlyLoadedDoc.DocumentPaginator as DynamicDocumentPaginator;
				dynamicDocumentPaginator.PaginationCompleted += _OnChildPaginationCompleted;
				dynamicDocumentPaginator.PaginationProgress += _OnChildPaginationProgress;
				dynamicDocumentPaginator.PagesChanged += _OnChildPagesChanged;
			}
		}
		return dynamicDocumentPaginator;
	}

	internal bool TranslatePageNumber(int pageNumber, out DynamicDocumentPaginator childPaginator, out int childPageNumber)
	{
		childPaginator = null;
		childPageNumber = 0;
		foreach (DocumentReference reference in References)
		{
			childPaginator = GetPaginator(reference);
			if (childPaginator != null)
			{
				childPageNumber = pageNumber;
				if (childPaginator.PageCount > childPageNumber)
				{
					return true;
				}
				if (!childPaginator.IsPageCountValid)
				{
					break;
				}
				pageNumber -= childPaginator.PageCount;
			}
		}
		return false;
	}

	private void _Init()
	{
		_paginator = new FixedDocumentSequencePaginator(this);
		_references = new DocumentReferenceCollection();
		_references.CollectionChanged += _OnCollectionChanged;
		_asyncOps = new Dictionary<object, GetPageAsyncRequest>();
		_pendingPages = new List<RequestedPage>();
		_pageSize = new Size(816.0, 1056.0);
		base.Initialized += OnInitialized;
	}

	private void OnInitialized(object sender, EventArgs e)
	{
		bool flag = true;
		foreach (DocumentReference reference in References)
		{
			DynamicDocumentPaginator paginator = GetPaginator(reference);
			if (paginator == null || !paginator.IsPageCountValid)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			_paginator.NotifyPaginationCompleted(EventArgs.Empty);
		}
		if (PageCount > 0)
		{
			DocumentPage page = GetPage(0);
			if (page != null && page.Visual is FixedPage fixedPage)
			{
				base.Language = fixedPage.Language;
			}
		}
	}

	private void _OnDocumentReferenceInitialized(object sender, EventArgs e)
	{
		DocumentReference documentReference = (DocumentReference)sender;
		if (documentReference == _partialRef)
		{
			_partialRef.Initialized -= _OnDocumentReferenceInitialized;
			_partialRef = null;
			_references.Add(documentReference);
		}
	}

	private void _OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.Action == NotifyCollectionChangedAction.Add)
		{
			if (args.NewItems.Count != 1)
			{
				throw new NotSupportedException(SR.RangeActionsNotSupported);
			}
			object obj = args.NewItems[0];
			AddLogicalChild(obj);
			int pageCount = PageCount;
			int pageCount2 = (GetPaginator((DocumentReference)obj) ?? throw new ApplicationException(SR.DocumentReferenceHasInvalidDocument)).PageCount;
			int start = pageCount - pageCount2;
			if (pageCount2 > 0)
			{
				_paginator.NotifyPaginationProgress(new PaginationProgressEventArgs(start, pageCount2));
				_paginator.NotifyPagesChanged(new PagesChangedEventArgs(start, pageCount2));
			}
			return;
		}
		throw new NotSupportedException(SR.Format(SR.UnexpectedCollectionChangeAction, args.Action));
	}

	private bool _SynthesizeGlobalPageNumber(DynamicDocumentPaginator childPaginator, int childPageNumber, out int pageNumber)
	{
		pageNumber = 0;
		foreach (DocumentReference reference in References)
		{
			DynamicDocumentPaginator paginator = GetPaginator(reference);
			if (paginator != null)
			{
				if (paginator == childPaginator)
				{
					pageNumber += childPageNumber;
					return true;
				}
				pageNumber += paginator.PageCount;
			}
		}
		return false;
	}

	private void _OnChildPaginationCompleted(object sender, EventArgs args)
	{
		if (IsPageCountValid)
		{
			_paginator.NotifyPaginationCompleted(EventArgs.Empty);
			if (_navigateAfterPagination)
			{
				FixedHyperLink.NavigateToElement(this, _navigateFragment);
				_navigateAfterPagination = false;
			}
		}
	}

	private void _OnChildPaginationProgress(object sender, PaginationProgressEventArgs args)
	{
		if (_SynthesizeGlobalPageNumber((DynamicDocumentPaginator)sender, args.Start, out var pageNumber))
		{
			_paginator.NotifyPaginationProgress(new PaginationProgressEventArgs(pageNumber, args.Count));
		}
	}

	private void _OnChildPagesChanged(object sender, PagesChangedEventArgs args)
	{
		if (_SynthesizeGlobalPageNumber((DynamicDocumentPaginator)sender, args.Start, out var pageNumber))
		{
			_paginator.NotifyPagesChanged(new PagesChangedEventArgs(pageNumber, args.Count));
		}
		else
		{
			_paginator.NotifyPagesChanged(new PagesChangedEventArgs(PageCount, int.MaxValue));
		}
	}

	private object _GetPageAsyncDelegate(object arg)
	{
		GetPageAsyncRequest getPageAsyncRequest = (GetPageAsyncRequest)arg;
		int pageNumber = getPageAsyncRequest.Page.PageNumber;
		if (getPageAsyncRequest.Cancelled || !TranslatePageNumber(pageNumber, out getPageAsyncRequest.Page.ChildPaginator, out getPageAsyncRequest.Page.ChildPageNumber) || getPageAsyncRequest.Cancelled)
		{
			_NotifyGetPageAsyncCompleted(DocumentPage.Missing, pageNumber, null, cancelled: true, getPageAsyncRequest.UserState);
			_asyncOps.Remove(getPageAsyncRequest.UserState);
			return null;
		}
		if (!_pendingPages.Contains(getPageAsyncRequest.Page))
		{
			_pendingPages.Add(getPageAsyncRequest.Page);
			getPageAsyncRequest.Page.ChildPaginator.GetPageCompleted += _OnGetPageCompleted;
			getPageAsyncRequest.Page.ChildPaginator.GetPageAsync(getPageAsyncRequest.Page.ChildPageNumber, getPageAsyncRequest);
		}
		return null;
	}

	private void _OnGetPageCompleted(object sender, GetPageCompletedEventArgs args)
	{
		GetPageAsyncRequest getPageAsyncRequest = (GetPageAsyncRequest)args.UserState;
		_pendingPages.Remove(getPageAsyncRequest.Page);
		DocumentPage page = DocumentPage.Missing;
		int pageNumber = getPageAsyncRequest.Page.PageNumber;
		if (!args.Cancelled && args.Error == null && args.DocumentPage != DocumentPage.Missing)
		{
			page = new FixedDocumentSequenceDocumentPage(this, (DynamicDocumentPaginator)sender, args.DocumentPage);
			_SynthesizeGlobalPageNumber((DynamicDocumentPaginator)sender, args.PageNumber, out pageNumber);
		}
		if (args.Cancelled)
		{
			return;
		}
		ArrayList arrayList = new ArrayList();
		IEnumerator<KeyValuePair<object, GetPageAsyncRequest>> enumerator = _asyncOps.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GetPageAsyncRequest value = enumerator.Current.Value;
				if (getPageAsyncRequest.Page.Equals(value.Page))
				{
					arrayList.Add(enumerator.Current.Key);
					_NotifyGetPageAsyncCompleted(page, pageNumber, args.Error, value.Cancelled, value.UserState);
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
