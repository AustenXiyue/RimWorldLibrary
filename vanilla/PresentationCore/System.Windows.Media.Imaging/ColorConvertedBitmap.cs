using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Changes the color space of a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
public sealed class ColorConvertedBitmap : BitmapSource, ISupportInitialize
{
	private BitmapSource _source;

	private ColorContext _sourceColorContext;

	private ColorContext _destinationColorContext;

	private PixelFormat _destinationFormat;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.Source" />  dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.Source" />  dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.SourceColorContext" />  dependency property. </summary>
	public static readonly DependencyProperty SourceColorContextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.DestinationColorContext" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.DestinationColorContext" /> dependency property.</returns>
	public static readonly DependencyProperty DestinationColorContextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.DestinationFormat" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.DestinationFormat" /> dependency property.</returns>
	public static readonly DependencyProperty DestinationFormatProperty;

	internal static BitmapSource s_Source;

	internal static ColorContext s_SourceColorContext;

	internal static ColorContext s_DestinationColorContext;

	internal static PixelFormat s_DestinationFormat;

	/// <summary>Gets or sets a value that identifies the source bitmap that is converted.  </summary>
	/// <returns>A value that identifies the source bitmap that is converted.</returns>
	public BitmapSource Source
	{
		get
		{
			return (BitmapSource)GetValue(SourceProperty);
		}
		set
		{
			SetValueInternal(SourceProperty, value);
		}
	}

	/// <summary>Gets or sets a value that identifies the color profile of the source bitmap.  </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.ColorContext" />.</returns>
	public ColorContext SourceColorContext
	{
		get
		{
			return (ColorContext)GetValue(SourceColorContextProperty);
		}
		set
		{
			SetValueInternal(SourceColorContextProperty, value);
		}
	}

	/// <summary>Gets or sets a value that identifies the color profile, as defined by the <see cref="T:System.Windows.Media.ColorContext" /> class, of the converted bitmap.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.ColorContext" />.</returns>
	public ColorContext DestinationColorContext
	{
		get
		{
			return (ColorContext)GetValue(DestinationColorContextProperty);
		}
		set
		{
			SetValueInternal(DestinationColorContextProperty, value);
		}
	}

	/// <summary>Gets or sets a value that represents the <see cref="T:System.Windows.Media.PixelFormat" /> of the converted bitmap.   </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Media.PixelFormat" />.</returns>
	public PixelFormat DestinationFormat
	{
		get
		{
			return (PixelFormat)GetValue(DestinationFormatProperty);
		}
		set
		{
			SetValueInternal(DestinationFormatProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> class.</summary>
	public ColorConvertedBitmap()
		: base(useVirtuals: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> class by using the specified values.</summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is converted.</param>
	/// <param name="sourceColorContext">The <see cref="T:System.Windows.Media.ColorContext" /> of the source bitmap.</param>
	/// <param name="destinationColorContext">The <see cref="T:System.Windows.Media.ColorContext" /> of the converted bitmap.</param>
	/// <param name="format">The <see cref="T:System.Windows.Media.PixelFormat" /> of the converted bitmap.</param>
	public ColorConvertedBitmap(BitmapSource source, ColorContext sourceColorContext, ColorContext destinationColorContext, PixelFormat format)
		: base(useVirtuals: true)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sourceColorContext == null)
		{
			throw new ArgumentNullException("sourceColorContext");
		}
		if (destinationColorContext == null)
		{
			throw new ArgumentNullException("destinationColorContext");
		}
		_bitmapInit.BeginInit();
		Source = source;
		SourceColorContext = sourceColorContext;
		DestinationColorContext = destinationColorContext;
		DestinationFormat = format;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> is currently being initialized. <see cref="M:System.Windows.Media.Imaging.ColorConvertedBitmap.BeginInit" /> has already been called.-or-The <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> has already been initialized.</exception>
	public void BeginInit()
	{
		WritePreamble();
		_bitmapInit.BeginInit();
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.Source" />, <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.SourceColorContext" />, or <see cref="P:System.Windows.Media.Imaging.ColorConvertedBitmap.DestinationColorContext" /> property is null.-or-The <see cref="M:System.Windows.Media.Imaging.ColorConvertedBitmap.EndInit" /> method is called without first calling <see cref="M:System.Windows.Media.Imaging.ColorConvertedBitmap.BeginInit" />.</exception>
	public void EndInit()
	{
		WritePreamble();
		_bitmapInit.EndInit();
		IsValidForFinalizeCreation(throwIfInvalid: true);
		FinalizeCreation();
	}

	private void ClonePrequel(ColorConvertedBitmap otherColorConvertedBitmap)
	{
		BeginInit();
	}

	private void ClonePostscript(ColorConvertedBitmap otherColorConvertedBitmap)
	{
		EndInit();
	}

	internal override void FinalizeCreation()
	{
		_bitmapInit.EnsureInitializedComplete();
		BitmapSourceSafeMILHandle ppWICColorTransform = null;
		HRESULT.Check(UnsafeNativeMethods.WICCodec.CreateColorTransform(out ppWICColorTransform));
		lock (_syncObject)
		{
			Guid pixelFmtDest = DestinationFormat.Guid;
			HRESULT.Check(UnsafeNativeMethods.WICColorTransform.Initialize(ppWICColorTransform, Source.WicSourceHandle, SourceColorContext.ColorContextHandle, DestinationColorContext.ColorContextHandle, ref pixelFmtDest));
		}
		base.WicSourceHandle = ppWICColorTransform;
		_isSourceCached = Source.IsSourceCached;
		base.CreationCompleted = true;
		UpdateCachedSettings();
	}

	private void SourcePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			BitmapSource bitmapSource = (_source = e.NewValue as BitmapSource);
			RegisterDownloadEventSource(_source);
			_syncObject = ((bitmapSource != null) ? bitmapSource.SyncObject : _bitmapInit);
		}
	}

	internal override bool IsValidForFinalizeCreation(bool throwIfInvalid)
	{
		if (Source == null)
		{
			if (throwIfInvalid)
			{
				throw new InvalidOperationException(SR.Format(SR.Image_NoArgument, "Source"));
			}
			return false;
		}
		if (SourceColorContext == null)
		{
			if (throwIfInvalid)
			{
				throw new InvalidOperationException(SR.Color_NullColorContext);
			}
			return false;
		}
		if (DestinationColorContext == null)
		{
			if (throwIfInvalid)
			{
				throw new InvalidOperationException(SR.Format(SR.Image_NoArgument, "DestinationColorContext"));
			}
			return false;
		}
		return true;
	}

	private void SourceColorContextPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_sourceColorContext = e.NewValue as ColorContext;
		}
	}

	private void DestinationColorContextPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_destinationColorContext = e.NewValue as ColorContext;
		}
	}

	private void DestinationFormatPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_destinationFormat = (PixelFormat)e.NewValue;
		}
	}

	private static object CoerceSource(DependencyObject d, object value)
	{
		ColorConvertedBitmap colorConvertedBitmap = (ColorConvertedBitmap)d;
		if (!colorConvertedBitmap._bitmapInit.IsInInit)
		{
			return colorConvertedBitmap._source;
		}
		return value;
	}

	private static object CoerceSourceColorContext(DependencyObject d, object value)
	{
		ColorConvertedBitmap colorConvertedBitmap = (ColorConvertedBitmap)d;
		if (!colorConvertedBitmap._bitmapInit.IsInInit)
		{
			return colorConvertedBitmap._sourceColorContext;
		}
		return value;
	}

	private static object CoerceDestinationColorContext(DependencyObject d, object value)
	{
		ColorConvertedBitmap colorConvertedBitmap = (ColorConvertedBitmap)d;
		if (!colorConvertedBitmap._bitmapInit.IsInInit)
		{
			return colorConvertedBitmap._destinationColorContext;
		}
		return value;
	}

	private static object CoerceDestinationFormat(DependencyObject d, object value)
	{
		ColorConvertedBitmap colorConvertedBitmap = (ColorConvertedBitmap)d;
		if (!colorConvertedBitmap._bitmapInit.IsInInit)
		{
			return colorConvertedBitmap._destinationFormat;
		}
		return value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ColorConvertedBitmap Clone()
	{
		return (ColorConvertedBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.ColorConvertedBitmap" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ColorConvertedBitmap CloneCurrentValue()
	{
		return (ColorConvertedBitmap)base.CloneCurrentValue();
	}

	private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ColorConvertedBitmap colorConvertedBitmap = (ColorConvertedBitmap)d;
		colorConvertedBitmap.SourcePropertyChangedHook(e);
		if (!e.IsASubPropertyChange || e.OldValueSource != e.NewValueSource)
		{
			colorConvertedBitmap.PropertyChanged(SourceProperty);
		}
	}

	private static void SourceColorContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ColorConvertedBitmap obj = (ColorConvertedBitmap)d;
		obj.SourceColorContextPropertyChangedHook(e);
		obj.PropertyChanged(SourceColorContextProperty);
	}

	private static void DestinationColorContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ColorConvertedBitmap obj = (ColorConvertedBitmap)d;
		obj.DestinationColorContextPropertyChangedHook(e);
		obj.PropertyChanged(DestinationColorContextProperty);
	}

	private static void DestinationFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ColorConvertedBitmap obj = (ColorConvertedBitmap)d;
		obj.DestinationFormatPropertyChangedHook(e);
		obj.PropertyChanged(DestinationFormatProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ColorConvertedBitmap();
	}

	protected override void CloneCore(Freezable source)
	{
		ColorConvertedBitmap otherColorConvertedBitmap = (ColorConvertedBitmap)source;
		ClonePrequel(otherColorConvertedBitmap);
		base.CloneCore(source);
		ClonePostscript(otherColorConvertedBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		ColorConvertedBitmap otherColorConvertedBitmap = (ColorConvertedBitmap)source;
		ClonePrequel(otherColorConvertedBitmap);
		base.CloneCurrentValueCore(source);
		ClonePostscript(otherColorConvertedBitmap);
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		ColorConvertedBitmap otherColorConvertedBitmap = (ColorConvertedBitmap)source;
		ClonePrequel(otherColorConvertedBitmap);
		base.GetAsFrozenCore(source);
		ClonePostscript(otherColorConvertedBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		ColorConvertedBitmap otherColorConvertedBitmap = (ColorConvertedBitmap)source;
		ClonePrequel(otherColorConvertedBitmap);
		base.GetCurrentValueAsFrozenCore(source);
		ClonePostscript(otherColorConvertedBitmap);
	}

	static ColorConvertedBitmap()
	{
		s_Source = null;
		s_SourceColorContext = null;
		s_DestinationColorContext = null;
		s_DestinationFormat = PixelFormats.Pbgra32;
		Type typeFromHandle = typeof(ColorConvertedBitmap);
		SourceProperty = Animatable.RegisterProperty("Source", typeof(BitmapSource), typeFromHandle, null, SourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceSource);
		SourceColorContextProperty = Animatable.RegisterProperty("SourceColorContext", typeof(ColorContext), typeFromHandle, null, SourceColorContextPropertyChanged, null, isIndependentlyAnimated: false, CoerceSourceColorContext);
		DestinationColorContextProperty = Animatable.RegisterProperty("DestinationColorContext", typeof(ColorContext), typeFromHandle, null, DestinationColorContextPropertyChanged, null, isIndependentlyAnimated: false, CoerceDestinationColorContext);
		DestinationFormatProperty = Animatable.RegisterProperty("DestinationFormat", typeof(PixelFormat), typeFromHandle, PixelFormats.Pbgra32, DestinationFormatPropertyChanged, null, isIndependentlyAnimated: false, CoerceDestinationFormat);
	}
}
