using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that displays an image.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class Image : FrameworkElement, IUriContext, IProvidePropertyFallback
{
	private class DownloadCompletedEventManager : WeakEventManager
	{
		private static DownloadCompletedEventManager CurrentManager
		{
			get
			{
				Type typeFromHandle = typeof(DownloadCompletedEventManager);
				DownloadCompletedEventManager downloadCompletedEventManager = (DownloadCompletedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
				if (downloadCompletedEventManager == null)
				{
					downloadCompletedEventManager = new DownloadCompletedEventManager();
					WeakEventManager.SetCurrentManager(typeFromHandle, downloadCompletedEventManager);
				}
				return downloadCompletedEventManager;
			}
		}

		private DownloadCompletedEventManager()
		{
		}

		public static void AddHandler(BitmapSource source, EventHandler<EventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedAddHandler(source, handler);
		}

		public static void RemoveHandler(BitmapSource source, EventHandler<EventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedRemoveHandler(source, handler);
		}

		protected override ListenerList NewListenerList()
		{
			return new ListenerList<EventArgs>();
		}

		protected override void StartListening(object source)
		{
			((BitmapSource)source).DownloadCompleted += OnDownloadCompleted;
		}

		protected override void StopListening(object source)
		{
			BitmapSource bitmapSource = (BitmapSource)source;
			if (bitmapSource.CheckAccess() && !bitmapSource.IsFrozen)
			{
				bitmapSource.DownloadCompleted -= OnDownloadCompleted;
			}
		}

		private void OnDownloadCompleted(object sender, EventArgs args)
		{
			DeliverEvent(sender, args);
		}
	}

	private class DownloadFailedEventManager : WeakEventManager
	{
		private static DownloadFailedEventManager CurrentManager
		{
			get
			{
				Type typeFromHandle = typeof(DownloadFailedEventManager);
				DownloadFailedEventManager downloadFailedEventManager = (DownloadFailedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
				if (downloadFailedEventManager == null)
				{
					downloadFailedEventManager = new DownloadFailedEventManager();
					WeakEventManager.SetCurrentManager(typeFromHandle, downloadFailedEventManager);
				}
				return downloadFailedEventManager;
			}
		}

		private DownloadFailedEventManager()
		{
		}

		public static void AddHandler(BitmapSource source, EventHandler<ExceptionEventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedAddHandler(source, handler);
		}

		public static void RemoveHandler(BitmapSource source, EventHandler<ExceptionEventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedRemoveHandler(source, handler);
		}

		protected override ListenerList NewListenerList()
		{
			return new ListenerList<ExceptionEventArgs>();
		}

		protected override void StartListening(object source)
		{
			((BitmapSource)source).DownloadFailed += OnDownloadFailed;
		}

		protected override void StopListening(object source)
		{
			BitmapSource bitmapSource = (BitmapSource)source;
			if (bitmapSource.CheckAccess() && !bitmapSource.IsFrozen)
			{
				bitmapSource.DownloadFailed -= OnDownloadFailed;
			}
		}

		private void OnDownloadFailed(object sender, ExceptionEventArgs args)
		{
			DeliverEvent(sender, args);
		}
	}

	private class DecodeFailedEventManager : WeakEventManager
	{
		private static DecodeFailedEventManager CurrentManager
		{
			get
			{
				Type typeFromHandle = typeof(DecodeFailedEventManager);
				DecodeFailedEventManager decodeFailedEventManager = (DecodeFailedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
				if (decodeFailedEventManager == null)
				{
					decodeFailedEventManager = new DecodeFailedEventManager();
					WeakEventManager.SetCurrentManager(typeFromHandle, decodeFailedEventManager);
				}
				return decodeFailedEventManager;
			}
		}

		private DecodeFailedEventManager()
		{
		}

		public static void AddHandler(BitmapSource source, EventHandler<ExceptionEventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedAddHandler(source, handler);
		}

		public static void RemoveHandler(BitmapSource source, EventHandler<ExceptionEventArgs> handler)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			CurrentManager.ProtectedRemoveHandler(source, handler);
		}

		protected override ListenerList NewListenerList()
		{
			return new ListenerList<ExceptionEventArgs>();
		}

		protected override void StartListening(object source)
		{
			((BitmapSource)source).DecodeFailed += OnDecodeFailed;
		}

		protected override void StopListening(object source)
		{
			BitmapSource bitmapSource = (BitmapSource)source;
			if (bitmapSource.CheckAccess() && !bitmapSource.IsFrozen)
			{
				bitmapSource.DecodeFailed -= OnDecodeFailed;
			}
		}

		private void OnDecodeFailed(object sender, ExceptionEventArgs args)
		{
			DeliverEvent(sender, args);
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Image.Source" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Image.Source" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty SourceProperty;

	public static readonly RoutedEvent DpiChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Image.Stretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Image.Stretch" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty StretchProperty;

	/// <summary>Identifies the <see cref="T:System.Windows.Controls.StretchDirection" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="T:System.Windows.Controls.StretchDirection" /> dependency property.</returns>
	public static readonly DependencyProperty StretchDirectionProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Image.ImageFailed" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Image.ImageFailed" /> routed event.</returns>
	public static readonly RoutedEvent ImageFailedEvent;

	private BitmapSource _bitmapSource;

	private bool _hasDpiChangedEverFired;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.ImageSource" /> for the image.  </summary>
	/// <returns>The source of the drawn image. The default value is null.</returns>
	public ImageSource Source
	{
		get
		{
			return (ImageSource)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets or sets a value that describes how an <see cref="T:System.Windows.Controls.Image" /> should be stretched to fill the destination rectangle.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.Stretch" /> values. The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
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

	/// <summary>Gets or sets a value that indicates how the image is scaled.  </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.StretchDirection" /> values. The default is <see cref="F:System.Windows.Controls.StretchDirection.Both" />.</returns>
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

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return BaseUri;
		}
		set
		{
			BaseUri = value;
		}
	}

	/// <summary>Gets or sets the base uniform resource identifier (URI) for the <see cref="T:System.Windows.Controls.Image" />.</summary>
	/// <returns>A base URI for the <see cref="T:System.Windows.Controls.Image" />.</returns>
	protected virtual Uri BaseUri
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

	internal override int EffectiveValuesInitialSize => 19;

	/// <summary>Occurs when there is a failure in the image.</summary>
	public event EventHandler<ExceptionRoutedEventArgs> ImageFailed
	{
		add
		{
			AddHandler(ImageFailedEvent, value);
		}
		remove
		{
			RemoveHandler(ImageFailedEvent, value);
		}
	}

	public event DpiChangedEventHandler DpiChanged
	{
		add
		{
			AddHandler(DpiChangedEvent, value);
		}
		remove
		{
			RemoveHandler(DpiChangedEvent, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Image" /> class. </summary>
	public Image()
	{
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Image" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.Image" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ImageAutomationPeer(this);
	}

	protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
	{
		_hasDpiChangedEverFired = true;
		RaiseEvent(new DpiChangedEventArgs(oldDpi, newDpi, DpiChangedEvent, this));
	}

	/// <summary>Updates the <see cref="P:System.Windows.UIElement.DesiredSize" /> of the image. This method is called by the parent <see cref="T:System.Windows.UIElement" /> and is the first pass of layout. </summary>
	/// <returns>The image's desired size.</returns>
	/// <param name="constraint">The size that the image should not exceed.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		if (!_hasDpiChangedEverFired)
		{
			_hasDpiChangedEverFired = true;
			DpiScale dpi = GetDpi();
			OnDpiChanged(dpi, dpi);
		}
		return MeasureArrangeHelper(constraint);
	}

	/// <summary>Arranges and sizes an image control.</summary>
	/// <returns>The size of the control.</returns>
	/// <param name="arrangeSize">The size used to arrange the control.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		return MeasureArrangeHelper(arrangeSize);
	}

	/// <summary>Renders the contents of an <see cref="T:System.Windows.Controls.Image" />.</summary>
	/// <param name="dc">An instance of <see cref="T:System.Windows.Media.DrawingContext" /> used to render the control.</param>
	protected override void OnRender(DrawingContext dc)
	{
		ImageSource source = Source;
		if (source != null)
		{
			dc.DrawImage(source, new Rect(default(Point), base.RenderSize));
		}
	}

	private Size MeasureArrangeHelper(Size inputSize)
	{
		ImageSource source = Source;
		Size size = default(Size);
		if (source == null)
		{
			return size;
		}
		try
		{
			UpdateBaseUri(this, source);
			size = source.Size;
		}
		catch (Exception errorException)
		{
			SetCurrentValue(SourceProperty, null);
			RaiseEvent(new ExceptionRoutedEventArgs(ImageFailedEvent, this, errorException));
		}
		Size size2 = Viewbox.ComputeScaleFactor(inputSize, size, Stretch, StretchDirection);
		return new Size(size.Width * size2.Width, size.Height * size2.Height);
	}

	static Image()
	{
		SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), typeof(Image), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged, null), null);
		StretchProperty = Viewbox.StretchProperty.AddOwner(typeof(Image));
		StretchDirectionProperty = Viewbox.StretchDirectionProperty.AddOwner(typeof(Image));
		ImageFailedEvent = EventManager.RegisterRoutedEvent("ImageFailed", RoutingStrategy.Bubble, typeof(EventHandler<ExceptionRoutedEventArgs>), typeof(Image));
		Style defaultValue = CreateDefaultStyles();
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(Image), new FrameworkPropertyMetadata(defaultValue));
		StretchProperty.OverrideMetadata(typeof(Image), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));
		StretchDirectionProperty.OverrideMetadata(typeof(Image), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));
		DpiChangedEvent = Window.DpiChangedEvent.AddOwner(typeof(Image));
		ControlsTraceLogger.AddControl(TelemetryControls.Image);
	}

	private static Style CreateDefaultStyles()
	{
		Style style = new Style(typeof(Image), null);
		style.Setters.Add(new Setter(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight));
		style.Seal();
		return style;
	}

	private void OnSourceDownloaded(object sender, EventArgs e)
	{
		DetachBitmapSourceEvents();
		InvalidateMeasure();
		InvalidateVisual();
	}

	private void OnSourceFailed(object sender, ExceptionEventArgs e)
	{
		DetachBitmapSourceEvents();
		SetCurrentValue(SourceProperty, null);
		RaiseEvent(new ExceptionRoutedEventArgs(ImageFailedEvent, this, e.ErrorException));
	}

	private void AttachBitmapSourceEvents(BitmapSource bitmapSource)
	{
		DownloadCompletedEventManager.AddHandler(bitmapSource, OnSourceDownloaded);
		DownloadFailedEventManager.AddHandler(bitmapSource, OnSourceFailed);
		DecodeFailedEventManager.AddHandler(bitmapSource, OnSourceFailed);
		_bitmapSource = bitmapSource;
	}

	private void DetachBitmapSourceEvents()
	{
		if (_bitmapSource != null)
		{
			DownloadCompletedEventManager.RemoveHandler(_bitmapSource, OnSourceDownloaded);
			DownloadFailedEventManager.RemoveHandler(_bitmapSource, OnSourceFailed);
			DecodeFailedEventManager.RemoveHandler(_bitmapSource, OnSourceFailed);
			_bitmapSource = null;
		}
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			Image image = (Image)d;
			_ = (ImageSource)e.OldValue;
			ImageSource imageSource = (ImageSource)e.NewValue;
			UpdateBaseUri(d, imageSource);
			image.DetachBitmapSourceEvents();
			if (imageSource is BitmapSource bitmapSource && bitmapSource.CheckAccess() && !bitmapSource.IsFrozen)
			{
				image.AttachBitmapSourceEvents(bitmapSource);
			}
		}
	}

	private static void UpdateBaseUri(DependencyObject d, ImageSource source)
	{
		if (source is IUriContext && !source.IsFrozen && ((IUriContext)source).BaseUri == null && BaseUriHelper.GetBaseUriCore(d) != null)
		{
			((IUriContext)source).BaseUri = BaseUriHelper.GetBaseUriCore(d);
		}
	}

	bool IProvidePropertyFallback.CanProvidePropertyFallback(string property)
	{
		if (string.CompareOrdinal(property, "Source") == 0)
		{
			return true;
		}
		return false;
	}

	object IProvidePropertyFallback.ProvidePropertyFallback(string property, Exception cause)
	{
		if (string.CompareOrdinal(property, "Source") == 0)
		{
			RaiseEvent(new ExceptionRoutedEventArgs(ImageFailedEvent, this, cause));
		}
		return null;
	}
}
