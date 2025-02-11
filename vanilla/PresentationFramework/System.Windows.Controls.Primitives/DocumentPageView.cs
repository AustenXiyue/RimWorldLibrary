using System.Windows.Automation.Peers;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a viewport for a paginated <see cref="T:System.Windows.Documents.DocumentPage" />.    </summary>
public class DocumentPageView : FrameworkElement, IServiceProvider, IDisposable
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.PageNumber" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.PageNumber" /> dependency property.</returns>
	public static readonly DependencyProperty PageNumberProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.Stretch" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.Stretch" /> dependency property.</returns>
	public static readonly DependencyProperty StretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.StretchDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.StretchDirection" /> dependency property.</returns>
	public static readonly DependencyProperty StretchDirectionProperty;

	private DocumentPaginator _documentPaginator;

	private double _pageZoom;

	private DocumentPage _documentPage;

	private DocumentPage _documentPageAsync;

	private DocumentPageTextView _textView;

	private DocumentPageHost _pageHost;

	private Visual _pageVisualClone;

	private Size _visualCloneSize;

	private bool _useAsynchronous = true;

	private bool _suspendLayout;

	private bool _disposed;

	private bool _newPageConnected;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Documents.DocumentPaginator" /> used to retrieve content pages for this view.</summary>
	/// <returns>The paginator that retrieves content pages for this view.</returns>
	public DocumentPaginator DocumentPaginator
	{
		get
		{
			return _documentPaginator;
		}
		set
		{
			CheckDisposed();
			if (_documentPaginator != value)
			{
				if (_documentPaginator != null)
				{
					_documentPaginator.GetPageCompleted -= HandleGetPageCompleted;
					_documentPaginator.PagesChanged -= HandlePagesChanged;
					DisposeCurrentPage();
					DisposeAsyncPage();
				}
				Invariant.Assert(_documentPage == null);
				Invariant.Assert(_documentPageAsync == null);
				_documentPaginator = value;
				_textView = null;
				if (_documentPaginator != null)
				{
					_documentPaginator.GetPageCompleted += HandleGetPageCompleted;
					_documentPaginator.PagesChanged += HandlePagesChanged;
				}
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Documents.DocumentPage" /> associated with this view.</summary>
	/// <returns>The document page associated with this view.</returns>
	public DocumentPage DocumentPage
	{
		get
		{
			if (_documentPage != null)
			{
				return _documentPage;
			}
			return DocumentPage.Missing;
		}
	}

	/// <summary>Gets or sets the page number of the current page displayed. </summary>
	/// <returns>The zero-based page number of the current page displayed.  The default is 0.</returns>
	public int PageNumber
	{
		get
		{
			return (int)GetValue(PageNumberProperty);
		}
		set
		{
			SetValue(PageNumberProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Stretch" /> enumeration that specifies how content should be stretched to fill the display page. </summary>
	/// <returns>The enumeration value that specifies how content should be stretched to fill the display page.  The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
	public Stretch Stretch
	{
		get
		{
			return (Stretch)GetValue(StretchProperty);
		}
		set
		{
			SetValue(StretchProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.StretchDirection" /> enumeration that specifies in what scaling directions <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.Stretch" /> should be applied. </summary>
	/// <returns>The enumeration value that specifies in what scaling directions <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.Stretch" /> should be applied.  The default is <see cref="F:System.Windows.Controls.StretchDirection.DownOnly" />.</returns>
	public StretchDirection StretchDirection
	{
		get
		{
			return (StretchDirection)GetValue(StretchDirectionProperty);
		}
		set
		{
			SetValue(StretchDirectionProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether <see cref="M:System.Windows.Controls.Primitives.DocumentPageView.Dispose" /> has been called for this instance.</summary>
	/// <returns>true if <see cref="M:System.Windows.Controls.Primitives.DocumentPageView.Dispose" /> has been called for this <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" />; otherwise, false.</returns>
	protected bool IsDisposed => _disposed;

	/// <summary>Gets the number of visual children contained in this view.</summary>
	/// <returns>The number of visual children contained in this view.</returns>
	protected override int VisualChildrenCount => (_pageHost != null) ? 1 : 0;

	internal bool UseAsynchronousGetPage
	{
		get
		{
			return _useAsynchronous;
		}
		set
		{
			_useAsynchronous = value;
		}
	}

	internal DocumentPage DocumentPageInternal => _documentPage;

	/// <summary>Occurs when a <see cref="T:System.Windows.Media.Visual" /> element of the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.DocumentPage" /> is connected.</summary>
	public event EventHandler PageConnected;

	/// <summary>Occurs when a <see cref="T:System.Windows.Media.Visual" /> element of the <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.DocumentPage" /> is disconnected.</summary>
	public event EventHandler PageDisconnected;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" /> class.</summary>
	public DocumentPageView()
	{
		_pageZoom = 1.0;
	}

	static DocumentPageView()
	{
		PageNumberProperty = DependencyProperty.Register("PageNumber", typeof(int), typeof(DocumentPageView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPageNumberChanged));
		StretchProperty = Viewbox.StretchProperty.AddOwner(typeof(DocumentPageView), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));
		StretchDirectionProperty = Viewbox.StretchDirectionProperty.AddOwner(typeof(DocumentPageView), new FrameworkPropertyMetadata(StretchDirection.DownOnly, FrameworkPropertyMetadataOptions.AffectsMeasure));
		UIElement.ClipToBoundsProperty.OverrideMetadata(typeof(DocumentPageView), new PropertyMetadata(BooleanBoxes.TrueBox));
	}

	protected override void OnDpiChanged(DpiScale oldDpiScaleInfo, DpiScale newDpiScaleInfo)
	{
		DisposeCurrentPage();
		DisposeAsyncPage();
	}

	/// <summary>Returns the available viewport size that can be given to display the current <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.DocumentPage" />.</summary>
	/// <returns>The actual desired size.</returns>
	/// <param name="availableSize">The maximum available size.</param>
	protected sealed override Size MeasureOverride(Size availableSize)
	{
		Size result = default(Size);
		CheckDisposed();
		if (_suspendLayout)
		{
			return base.DesiredSize;
		}
		if (_documentPaginator != null)
		{
			Size size;
			if (ShouldReflowContent() && (!double.IsInfinity(availableSize.Width) || !double.IsInfinity(availableSize.Height)))
			{
				size = _documentPaginator.PageSize;
				Size size2;
				if (double.IsInfinity(availableSize.Width))
				{
					size2 = default(Size);
					size2.Height = availableSize.Height / _pageZoom;
					size2.Width = size2.Height * (size.Width / size.Height);
				}
				else if (double.IsInfinity(availableSize.Height))
				{
					size2 = default(Size);
					size2.Width = availableSize.Width / _pageZoom;
					size2.Height = size2.Width * (size.Height / size.Width);
				}
				else
				{
					size2 = new Size(availableSize.Width / _pageZoom, availableSize.Height / _pageZoom);
				}
				if (!DoubleUtil.AreClose(size, size2))
				{
					_documentPaginator.PageSize = size2;
				}
			}
			if (_documentPage == null && _documentPageAsync == null)
			{
				if (PageNumber >= 0)
				{
					if (_useAsynchronous)
					{
						_documentPaginator.GetPageAsync(PageNumber, this);
					}
					else
					{
						_documentPageAsync = _documentPaginator.GetPage(PageNumber);
						if (_documentPageAsync == null)
						{
							_documentPageAsync = DocumentPage.Missing;
						}
					}
				}
				else
				{
					_documentPage = DocumentPage.Missing;
				}
			}
			if (_documentPageAsync != null)
			{
				DisposeCurrentPage();
				if (_documentPageAsync == null)
				{
					_documentPageAsync = DocumentPage.Missing;
				}
				if (_pageVisualClone != null)
				{
					RemoveDuplicateVisual();
				}
				_documentPage = _documentPageAsync;
				if (_documentPage != DocumentPage.Missing)
				{
					_documentPage.PageDestroyed += HandlePageDestroyed;
					_documentPageAsync.PageDestroyed -= HandleAsyncPageDestroyed;
				}
				_documentPageAsync = null;
				_newPageConnected = true;
			}
			if (_documentPage != null && _documentPage != DocumentPage.Missing)
			{
				size = new Size(_documentPage.Size.Width * _pageZoom, _documentPage.Size.Height * _pageZoom);
				Size size3 = Viewbox.ComputeScaleFactor(availableSize, size, Stretch, StretchDirection);
				result = new Size(size.Width * size3.Width, size.Height * size3.Height);
			}
			if (_pageVisualClone != null)
			{
				return _visualCloneSize;
			}
		}
		return result;
	}

	/// <summary>Arranges the content to fit a specified view size.</summary>
	/// <returns>The actual size that the page view used to arrange itself and its children.</returns>
	/// <param name="finalSize">The maximum size that the page view should use to arrange itself and its children.</param>
	protected sealed override Size ArrangeOverride(Size finalSize)
	{
		CheckDisposed();
		if (_pageVisualClone == null)
		{
			if (_pageHost == null)
			{
				_pageHost = new DocumentPageHost();
				AddVisualChild(_pageHost);
			}
			Invariant.Assert(_pageHost != null);
			Visual visual = ((_documentPage == null) ? null : _documentPage.Visual);
			if (visual == null)
			{
				_pageHost.PageVisual = null;
				_pageHost.CachedOffset = default(Point);
				_pageHost.RenderTransform = null;
				_pageHost.Arrange(new Rect(_pageHost.CachedOffset, finalSize));
			}
			else
			{
				if (_pageHost.PageVisual != visual)
				{
					DocumentPageHost.DisconnectPageVisual(visual);
					_pageHost.PageVisual = visual;
				}
				Size contentSize = _documentPage.Size;
				Transform transform = Transform.Identity;
				if (base.FlowDirection == FlowDirection.RightToLeft)
				{
					transform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, contentSize.Width, 0.0);
				}
				if (!DoubleUtil.IsOne(_pageZoom))
				{
					ScaleTransform scaleTransform = new ScaleTransform(_pageZoom, _pageZoom);
					transform = ((transform != Transform.Identity) ? ((Transform)new MatrixTransform(transform.Value * scaleTransform.Value)) : ((Transform)scaleTransform));
					contentSize = new Size(contentSize.Width * _pageZoom, contentSize.Height * _pageZoom);
				}
				Size size = Viewbox.ComputeScaleFactor(finalSize, contentSize, Stretch, StretchDirection);
				if (!DoubleUtil.IsOne(size.Width) || !DoubleUtil.IsOne(size.Height))
				{
					ScaleTransform scaleTransform = new ScaleTransform(size.Width, size.Height);
					transform = ((transform != Transform.Identity) ? ((Transform)new MatrixTransform(transform.Value * scaleTransform.Value)) : ((Transform)scaleTransform));
					contentSize = new Size(contentSize.Width * size.Width, contentSize.Height * size.Height);
				}
				_pageHost.CachedOffset = new Point((finalSize.Width - contentSize.Width) / 2.0, (finalSize.Height - contentSize.Height) / 2.0);
				_pageHost.RenderTransform = transform;
				_pageHost.Arrange(new Rect(_pageHost.CachedOffset, _documentPage.Size));
			}
			if (_newPageConnected)
			{
				OnPageConnected();
			}
			OnTransformChangedAsync();
		}
		else if (_pageHost.PageVisual != _pageVisualClone)
		{
			_pageHost.PageVisual = _pageVisualClone;
			_pageHost.Arrange(new Rect(_pageHost.CachedOffset, finalSize));
		}
		return base.ArrangeOverride(finalSize);
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Visual" /> child at a specified index.</summary>
	/// <returns>The visual child at the specified index.</returns>
	/// <param name="index">The index of the visual child to return.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero, or greater than or equal to <see cref="P:System.Windows.Controls.Primitives.DocumentPageView.VisualChildrenCount" />.</exception>
	protected override Visual GetVisualChild(int index)
	{
		if (index != 0 || _pageHost == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _pageHost;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" />.</summary>
	protected void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_documentPaginator != null)
			{
				_documentPaginator.GetPageCompleted -= HandleGetPageCompleted;
				_documentPaginator.PagesChanged -= HandlePagesChanged;
				_documentPaginator.CancelAsync(this);
				DisposeCurrentPage();
				DisposeAsyncPage();
			}
			Invariant.Assert(_documentPage == null);
			Invariant.Assert(_documentPageAsync == null);
			_documentPaginator = null;
			_textView = null;
		}
	}

	/// <summary>Gets the service object of the specified type.</summary>
	/// <returns>A service object of type <paramref name="serviceType" />, or null if there is no service object of type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType">The type of service object to get. </param>
	protected object GetService(Type serviceType)
	{
		object result = null;
		if (serviceType == null)
		{
			throw new ArgumentNullException("serviceType");
		}
		CheckDisposed();
		if (_documentPaginator != null && _documentPaginator is IServiceProvider)
		{
			if (serviceType == typeof(ITextView))
			{
				if (_textView == null && ((IServiceProvider)_documentPaginator).GetService(typeof(ITextContainer)) is ITextContainer textContainer)
				{
					_textView = new DocumentPageTextView(this, textContainer);
				}
				result = _textView;
			}
			else if (serviceType == typeof(TextContainer) || serviceType == typeof(ITextContainer))
			{
				result = ((IServiceProvider)_documentPaginator).GetService(serviceType);
			}
		}
		return result;
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.Primitives.DocumentPageView" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Automation.Peers.DocumentPageViewAutomationPeer" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DocumentPageViewAutomationPeer(this);
	}

	internal void SetPageZoom(double pageZoom)
	{
		Invariant.Assert(!DoubleUtil.LessThanOrClose(pageZoom, 0.0) && !double.IsInfinity(pageZoom));
		Invariant.Assert(!_disposed);
		if (!DoubleUtil.AreClose(_pageZoom, pageZoom))
		{
			_pageZoom = pageZoom;
			InvalidateMeasure();
		}
	}

	internal void SuspendLayout()
	{
		_suspendLayout = true;
		_pageVisualClone = DuplicatePageVisual();
		_visualCloneSize = base.DesiredSize;
	}

	internal void ResumeLayout()
	{
		_suspendLayout = false;
		_pageVisualClone = null;
		InvalidateMeasure();
	}

	internal void DuplicateVisual()
	{
		if (_documentPage != null && _pageVisualClone == null)
		{
			_pageVisualClone = DuplicatePageVisual();
			_visualCloneSize = base.DesiredSize;
			InvalidateArrange();
		}
	}

	internal void RemoveDuplicateVisual()
	{
		if (_pageVisualClone != null)
		{
			_pageVisualClone = null;
			InvalidateArrange();
		}
	}

	private void HandlePageDestroyed(object sender, EventArgs e)
	{
		if (!_disposed)
		{
			InvalidateMeasure();
			DisposeCurrentPage();
		}
	}

	private void HandleAsyncPageDestroyed(object sender, EventArgs e)
	{
		if (!_disposed)
		{
			DisposeAsyncPage();
		}
	}

	private void HandleGetPageCompleted(object sender, GetPageCompletedEventArgs e)
	{
		if (!_disposed && e != null && !e.Cancelled && e.Error == null && e.PageNumber == PageNumber && e.UserState == this)
		{
			if (_documentPageAsync != null && _documentPageAsync != DocumentPage.Missing)
			{
				_documentPageAsync.PageDestroyed -= HandleAsyncPageDestroyed;
			}
			_documentPageAsync = e.DocumentPage;
			if (_documentPageAsync == null)
			{
				_documentPageAsync = DocumentPage.Missing;
			}
			if (_documentPageAsync != DocumentPage.Missing)
			{
				_documentPageAsync.PageDestroyed += HandleAsyncPageDestroyed;
			}
			InvalidateMeasure();
		}
	}

	private void HandlePagesChanged(object sender, PagesChangedEventArgs e)
	{
		if (!_disposed && e != null && PageNumber >= e.Start && (e.Count == int.MaxValue || PageNumber <= e.Start + e.Count))
		{
			OnPageContentChanged();
		}
	}

	private void OnTransformChangedAsync()
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(OnTransformChanged), null);
	}

	private object OnTransformChanged(object arg)
	{
		if (_textView != null && _documentPage != null)
		{
			_textView.OnTransformChanged();
		}
		return null;
	}

	private void OnPageConnected()
	{
		_newPageConnected = false;
		if (_textView != null)
		{
			_textView.OnPageConnected();
		}
		if (this.PageConnected != null && _documentPage != null)
		{
			this.PageConnected(this, EventArgs.Empty);
		}
	}

	private void OnPageDisconnected()
	{
		if (_textView != null)
		{
			_textView.OnPageDisconnected();
		}
		if (this.PageDisconnected != null)
		{
			this.PageDisconnected(this, EventArgs.Empty);
		}
	}

	private void OnPageContentChanged()
	{
		InvalidateMeasure();
		DisposeCurrentPage();
		DisposeAsyncPage();
	}

	private void DisposeCurrentPage()
	{
		if (_documentPage != null)
		{
			if (_pageHost != null)
			{
				_pageHost.PageVisual = null;
			}
			if (_documentPage != DocumentPage.Missing)
			{
				_documentPage.PageDestroyed -= HandlePageDestroyed;
			}
			if (_documentPage != null)
			{
				((IDisposable)_documentPage).Dispose();
			}
			_documentPage = null;
			OnPageDisconnected();
		}
	}

	private void DisposeAsyncPage()
	{
		if (_documentPageAsync != null)
		{
			if (_documentPageAsync != DocumentPage.Missing)
			{
				_documentPageAsync.PageDestroyed -= HandleAsyncPageDestroyed;
			}
			if (_documentPageAsync != null)
			{
				((IDisposable)_documentPageAsync).Dispose();
			}
			_documentPageAsync = null;
		}
	}

	private void CheckDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(typeof(DocumentPageView).ToString());
		}
	}

	private bool ShouldReflowContent()
	{
		bool result = false;
		if (DocumentViewerBase.GetIsMasterPage(this))
		{
			DocumentViewerBase hostViewer = GetHostViewer();
			if (hostViewer != null)
			{
				result = hostViewer.IsMasterPageView(this);
			}
		}
		return result;
	}

	private DocumentViewerBase GetHostViewer()
	{
		DocumentViewerBase result = null;
		if (base.TemplatedParent is DocumentViewerBase)
		{
			result = (DocumentViewerBase)base.TemplatedParent;
		}
		else
		{
			for (Visual visual = VisualTreeHelper.GetParent(this) as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
			{
				if (visual is DocumentViewerBase)
				{
					result = (DocumentViewerBase)visual;
					break;
				}
			}
		}
		return result;
	}

	private static void OnPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Invariant.Assert(d != null && d is DocumentPageView);
		((DocumentPageView)d).OnPageContentChanged();
	}

	private DrawingVisual DuplicatePageVisual()
	{
		DrawingVisual drawingVisual = null;
		if (_pageHost != null && _pageHost.PageVisual != null && _documentPage.Size != Size.Empty)
		{
			Rect rectangle = new Rect(_documentPage.Size);
			rectangle.Width = Math.Min(rectangle.Width, 4096.0);
			rectangle.Height = Math.Min(rectangle.Height, 4096.0);
			drawingVisual = new DrawingVisual();
			try
			{
				if (rectangle.Width > 1.0 && rectangle.Height > 1.0)
				{
					RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)rectangle.Width, (int)rectangle.Height, 96.0, 96.0, PixelFormats.Pbgra32);
					renderTargetBitmap.Render(_pageHost.PageVisual);
					ImageBrush brush = new ImageBrush(renderTargetBitmap);
					drawingVisual.Opacity = 0.5;
					using DrawingContext drawingContext = drawingVisual.RenderOpen();
					drawingContext.DrawRectangle(brush, null, rectangle);
				}
			}
			catch (OverflowException)
			{
			}
		}
		return drawingVisual;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Controls.Primitives.DocumentPageView.GetService(System.Type)" /> method instead. </summary>
	/// <returns>A service object of type <paramref name="serviceType" />.</returns>
	/// <param name="serviceType"> An object that specifies the type of service object to get.</param>
	object IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.  Use the type-safe <see cref="M:System.Windows.Controls.Primitives.DocumentPageView.Dispose" /> method instead. </summary>
	void IDisposable.Dispose()
	{
		Dispose();
	}
}
