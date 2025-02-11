using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Utility;
using MS.Utility;

namespace System.Windows.Documents;

/// <summary>Provides information about the <see cref="T:System.Windows.Documents.FixedPage" /> elements within a <see cref="T:System.Windows.Documents.FixedDocument" />.</summary>
[ContentProperty("Child")]
public sealed class PageContent : FrameworkElement, IAddChildInternal, IAddChild, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.PageContent.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.PageContent.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(PageContent), new FrameworkPropertyMetadata(null, OnSourceChanged));

	private WeakReference _pageRef;

	private FixedPage _child;

	private PageContentAsyncResult _asyncOp;

	private HybridDictionary _pendingStreams;

	private LinkTargetCollection _linkTargets;

	/// <summary>Gets or sets the uniform resource identifier (URI) to the <see cref="T:System.Windows.Documents.FixedPage" /> content data stream.  </summary>
	/// <returns>The <see cref="T:System.Uri" /> of the corresponding <see cref="T:System.Windows.Documents.FixedPage" />.</returns>
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Documents.LinkTarget" /> elements that identify the hyperlink-addressable locations on the page. </summary>
	/// <returns>The <see cref="T:System.Windows.Documents.LinkTargetCollection" /> of <see cref="T:System.Windows.Documents.LinkTarget" /> elements that identify the hyperlink-addressable locations on the page.</returns>
	public LinkTargetCollection LinkTargets
	{
		get
		{
			if (_linkTargets == null)
			{
				_linkTargets = new LinkTargetCollection();
			}
			return _linkTargets;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Documents.FixedPage" /> associated with this <see cref="T:System.Windows.Documents.PageContent" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.FixedPage" /> associated with this <see cref="T:System.Windows.Documents.PageContent" />, or null when the <see cref="T:System.Windows.Documents.FixedPage" /> is set by the <see cref="P:System.Windows.Documents.PageContent.Source" /> property. </returns>
	[DefaultValue(null)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public FixedPage Child
	{
		get
		{
			return _child;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_child != null)
			{
				throw new InvalidOperationException(SR.Format(SR.CanOnlyHaveOneChild, typeof(PageContent), value));
			}
			_pageRef = null;
			_child = value;
			LogicalTreeHelper.AddLogicalChild(this, _child);
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Windows.Markup.IUriContext.BaseUri" />.</summary>
	/// <returns>The base URI of the current context. </returns>
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

	internal FixedPage PageStream => _child;

	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			FixedPage fixedPage = _child;
			if (fixedPage == null)
			{
				fixedPage = _GetLoadedPage();
			}
			FixedPage[] array = ((fixedPage != null) ? new FixedPage[1] { fixedPage } : Array.Empty<FixedPage>());
			return array.GetEnumerator();
		}
	}

	/// <summary>Occurs when <see cref="M:System.Windows.Documents.PageContent.GetPageRootAsync(System.Boolean)" /> has completed.</summary>
	public event GetPageRootCompletedEventHandler GetPageRootCompleted;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.PageContent" /> class.</summary>
	public PageContent()
	{
		_Init();
	}

	/// <summary>Loads and returns the <see cref="T:System.Windows.Documents.FixedPage" /> content element. </summary>
	/// <returns>The root element of the visual tree for this page.</returns>
	/// <param name="forceReload">true to always reload the <see cref="T:System.Windows.Documents.FixedPage" /> even if it has been previously loaded and cached; false to load the <see cref="T:System.Windows.Documents.FixedPage" /> only if there is no cached version.</param>
	public FixedPage GetPageRoot(bool forceReload)
	{
		if (_asyncOp != null)
		{
			_asyncOp.Wait();
		}
		FixedPage fixedPage = null;
		if (!forceReload)
		{
			fixedPage = _GetLoadedPage();
		}
		if (fixedPage == null)
		{
			fixedPage = _LoadPage();
		}
		return fixedPage;
	}

	/// <summary>Asynchronously loads and returns the <see cref="T:System.Windows.Documents.FixedPage" /> content element. </summary>
	/// <param name="forceReload">true to always reload the <see cref="T:System.Windows.Documents.FixedPage" /> even if it has been previously loaded and cached; false to load the <see cref="T:System.Windows.Documents.FixedPage" /> only if there is no cached version.</param>
	public void GetPageRootAsync(bool forceReload)
	{
		if (_asyncOp != null)
		{
			return;
		}
		FixedPage fixedPage = null;
		if (!forceReload)
		{
			fixedPage = _GetLoadedPage();
		}
		if (fixedPage != null)
		{
			_NotifyPageCompleted(fixedPage, null, cancelled: false, null);
			return;
		}
		Dispatcher dispatcher = base.Dispatcher;
		Uri uri = _ResolveUri();
		if (uri != null || _child != null)
		{
			_asyncOp = new PageContentAsyncResult(_RequestPageCallback, null, dispatcher, uri, uri, _child);
			_asyncOp.DispatcherOperation = dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(_asyncOp.Dispatch), null);
		}
	}

	/// <summary>Cancels any current <see cref="M:System.Windows.Documents.PageContent.GetPageRootAsync(System.Boolean)" /> operation in progress.</summary>
	public void GetPageRootAsyncCancel()
	{
		if (_asyncOp != null)
		{
			_asyncOp.Cancel();
			_asyncOp = null;
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddChild(System.Object)" />.</summary>
	/// <param name="value">The child <see cref="T:System.Object" /> to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is FixedPage child))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(FixedPage)), "value");
		}
		if (_child != null)
		{
			throw new InvalidOperationException(SR.Format(SR.CanOnlyHaveOneChild, typeof(PageContent), value));
		}
		_pageRef = null;
		_child = child;
		LogicalTreeHelper.AddLogicalChild(this, _child);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddText(System.String)" />.</summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PageContent)d)._pageRef = null;
	}

	/// <summary>Gets a value indicating whether the value of the <see cref="P:System.Windows.Documents.PageContent.Child" /> property should be serialized when this <see cref="T:System.Windows.Documents.PageContent" /> is serialized.</summary>
	/// <returns>true if <paramref name="manager" /> is not null and it does not have an XmlWriter; otherwise, false. The default is false.</returns>
	/// <param name="manager">The serialization services provider.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeChild(XamlDesignerSerializationManager manager)
	{
		bool result = false;
		if (manager != null)
		{
			result = manager.XmlWriter == null;
		}
		return result;
	}

	internal bool IsOwnerOf(FixedPage pageVisual)
	{
		if (pageVisual == null)
		{
			throw new ArgumentNullException("pageVisual");
		}
		if (_child == pageVisual)
		{
			return true;
		}
		if (_pageRef != null && (FixedPage)_pageRef.Target == pageVisual)
		{
			return true;
		}
		return false;
	}

	internal Stream GetPageStream()
	{
		Uri uri = _ResolveUri();
		Stream stream = null;
		if (uri != null)
		{
			stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(uri);
			if (stream == null)
			{
				throw new ApplicationException(SR.PageContentNotFound);
			}
		}
		return stream;
	}

	internal bool ContainsID(string elementID)
	{
		bool result = false;
		foreach (LinkTarget linkTarget in LinkTargets)
		{
			if (elementID.Equals(linkTarget.Name))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void _Init()
	{
		base.InheritanceBehavior = InheritanceBehavior.SkipToAppNow;
		_pendingStreams = new HybridDictionary();
	}

	private void _NotifyPageCompleted(FixedPage result, Exception error, bool cancelled, object userToken)
	{
		if (this.GetPageRootCompleted != null)
		{
			GetPageRootCompletedEventArgs e = new GetPageRootCompletedEventArgs(result, error, cancelled, userToken);
			this.GetPageRootCompleted(this, e);
		}
	}

	private Uri _ResolveUri()
	{
		Uri uri = Source;
		if (uri != null)
		{
			uri = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(this, ((IUriContext)this).BaseUri, uri);
		}
		return uri;
	}

	private void _RequestPageCallback(IAsyncResult ar)
	{
		PageContentAsyncResult pageContentAsyncResult = (PageContentAsyncResult)ar;
		if (pageContentAsyncResult == _asyncOp && pageContentAsyncResult.Result != null)
		{
			LogicalTreeHelper.AddLogicalChild(this, pageContentAsyncResult.Result);
			_pageRef = new WeakReference(pageContentAsyncResult.Result);
		}
		_asyncOp = null;
		_NotifyPageCompleted(pageContentAsyncResult.Result, pageContentAsyncResult.Exception, pageContentAsyncResult.IsCancelled, pageContentAsyncResult.AsyncState);
	}

	private FixedPage _LoadPage()
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXGetPageBegin);
		FixedPage fixedPage = null;
		try
		{
			if (_child != null)
			{
				fixedPage = _child;
			}
			else
			{
				Uri uri = _ResolveUri();
				if (uri != null)
				{
					_LoadPageImpl(((IUriContext)this).BaseUri, uri, out fixedPage, out var pageStream);
					if (fixedPage == null || fixedPage.IsInitialized)
					{
						pageStream.Close();
					}
					else
					{
						_pendingStreams.Add(fixedPage, pageStream);
						fixedPage.Initialized += _OnPaserFinished;
					}
				}
			}
			if (fixedPage != null)
			{
				LogicalTreeHelper.AddLogicalChild(this, fixedPage);
				_pageRef = new WeakReference(fixedPage);
			}
			else
			{
				_pageRef = null;
			}
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXPS, EventTrace.Event.WClientDRXGetPageEnd);
		}
		return fixedPage;
	}

	private FixedPage _GetLoadedPage()
	{
		FixedPage result = null;
		if (_pageRef != null)
		{
			result = (FixedPage)_pageRef.Target;
		}
		return result;
	}

	private void _OnPaserFinished(object sender, EventArgs args)
	{
		if (_pendingStreams.Contains(sender))
		{
			((Stream)_pendingStreams[sender]).Close();
			_pendingStreams.Remove(sender);
		}
	}

	internal static void _LoadPageImpl(Uri baseUri, Uri uriToLoad, out FixedPage fixedPage, out Stream pageStream)
	{
		pageStream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(uriToLoad, out var contentType);
		object obj = null;
		if (pageStream == null)
		{
			throw new ApplicationException(SR.PageContentNotFound);
		}
		ParserContext parserContext = new ParserContext();
		parserContext.BaseUri = uriToLoad;
		if (MS.Internal.Utility.BindUriHelper.IsXamlMimeType(contentType))
		{
			obj = new XpsValidatingLoader().Load(pageStream, baseUri, parserContext, contentType);
		}
		else
		{
			if (!MimeTypeMapper.BamlMime.AreTypeAndSubTypeEqual(contentType))
			{
				throw new ApplicationException(SR.PageContentUnsupportedMimeType);
			}
			obj = XamlReader.LoadBaml(pageStream, parserContext, null, closeStream: true);
		}
		if (obj != null && !(obj is FixedPage))
		{
			throw new ApplicationException(SR.Format(SR.PageContentUnsupportedPageType, obj.GetType()));
		}
		fixedPage = (FixedPage)obj;
	}
}
