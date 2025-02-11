using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides pixel format conversion functionality for a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. </summary>
public sealed class FormatConvertedBitmap : BitmapSource, ISupportInitialize
{
	private BitmapSource _source;

	private PixelFormat _destinationFormat;

	private BitmapPalette _destinationPalette;

	private double _alphaThreshold;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.Source" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormat" /> dependency property. </returns>
	public static readonly DependencyProperty DestinationFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationPalette" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationPalette" /> dependency property.</returns>
	public static readonly DependencyProperty DestinationPaletteProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.AlphaThreshold" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.AlphaThreshold" /> dependency property.</returns>
	public static readonly DependencyProperty AlphaThresholdProperty;

	internal static BitmapSource s_Source;

	internal static PixelFormat s_DestinationFormat;

	internal static BitmapPalette s_DestinationPalette;

	internal const double c_AlphaThreshold = 0.0;

	/// <summary>Gets or sets the source for the bitmap.   </summary>
	/// <returns>The source for the bitmap. The default value is null. </returns>
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

	/// <summary>Gets or sets the pixel format to convert the bitmap to.  </summary>
	/// <returns>The pixel format to apply to the bitmap. The default value is <see cref="P:System.Windows.Media.PixelFormats.Pbgra32" />.</returns>
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

	/// <summary>Gets or sets the palette to apply to the bitmap if the format is indexed.  </summary>
	/// <returns>The destination palette to apply to the bitmap. The default value is null. </returns>
	public BitmapPalette DestinationPalette
	{
		get
		{
			return (BitmapPalette)GetValue(DestinationPaletteProperty);
		}
		set
		{
			SetValueInternal(DestinationPaletteProperty, value);
		}
	}

	/// <summary>Gets or sets the alpha channel threshold of a bitmap when converting to palletized formats that recognizes an alpha color.  </summary>
	/// <returns>The alpha channel threshold for this <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" />. The default value is 0.0.</returns>
	public double AlphaThreshold
	{
		get
		{
			return (double)GetValue(AlphaThresholdProperty);
		}
		set
		{
			SetValueInternal(AlphaThresholdProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> class.</summary>
	public FormatConvertedBitmap()
		: base(useVirtuals: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> class that has the specified <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.Source" />, <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormat" />, <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationPalette" />, and <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.AlphaThreshold" />.</summary>
	/// <param name="source">The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.Source" /> of the new <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> instance.</param>
	/// <param name="destinationFormat">The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormat" /> of the new <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> instance.</param>
	/// <param name="destinationPalette">The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationPalette" /> of the new <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> instance if <paramref name="destinationFormat" /> is an indexed format.</param>
	/// <param name="alphaThreshold">The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.AlphaThreshold" /> of the new <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> instance.</param>
	public FormatConvertedBitmap(BitmapSource source, PixelFormat destinationFormat, BitmapPalette destinationPalette, double alphaThreshold)
		: base(useVirtuals: true)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (alphaThreshold < 0.0 || alphaThreshold > 100.0)
		{
			throw new ArgumentException(SR.Image_AlphaThresholdOutOfRange);
		}
		_bitmapInit.BeginInit();
		Source = source;
		DestinationFormat = destinationFormat;
		DestinationPalette = destinationPalette;
		AlphaThreshold = alphaThreshold;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> initialization. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> is currently being initialized. <see cref="M:System.Windows.Media.Imaging.FormatConvertedBitmap.BeginInit" /> has already been called.-or-The <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> has already been initialized.</exception>
	public void BeginInit()
	{
		WritePreamble();
		_bitmapInit.BeginInit();
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> initialization. </summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.Source" /> property is null.-or-The <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationFormat" /> property is an indexed format and the <see cref="P:System.Windows.Media.Imaging.FormatConvertedBitmap.DestinationPalette" /> property is null.-or-The palette colors do not match the destination format.-or-The <see cref="M:System.Windows.Media.Imaging.FormatConvertedBitmap.EndInit" /> method is called without first calling <see cref="M:System.Windows.Media.Imaging.FormatConvertedBitmap.BeginInit" />.</exception>
	public void EndInit()
	{
		WritePreamble();
		_bitmapInit.EndInit();
		IsValidForFinalizeCreation(throwIfInvalid: true);
		FinalizeCreation();
	}

	private void ClonePrequel(FormatConvertedBitmap otherFormatConvertedBitmap)
	{
		BeginInit();
	}

	private void ClonePostscript(FormatConvertedBitmap otherFormatConvertedBitmap)
	{
		EndInit();
	}

	internal override void FinalizeCreation()
	{
		_bitmapInit.EnsureInitializedComplete();
		BitmapSourceSafeMILHandle ppFormatConverter = null;
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			try
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateFormatConverter(factoryMaker.ImagingFactoryPtr, out ppFormatConverter));
				SafeMILHandle bitmapPalette = ((DestinationPalette == null) ? new SafeMILHandle() : DestinationPalette.InternalPalette);
				Guid dstFormat = DestinationFormat.Guid;
				lock (_syncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICFormatConverter.Initialize(ppFormatConverter, Source.WicSourceHandle, ref dstFormat, DitherType.DitherTypeErrorDiffusion, bitmapPalette, AlphaThreshold, WICPaletteType.WICPaletteTypeOptimal));
				}
				base.WicSourceHandle = ppFormatConverter;
				_isSourceCached = false;
			}
			catch
			{
				_bitmapInit.Reset();
				throw;
			}
			finally
			{
				ppFormatConverter?.Close();
			}
		}
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
		if (DestinationFormat.Palettized)
		{
			if (DestinationPalette == null)
			{
				if (throwIfInvalid)
				{
					throw new InvalidOperationException(SR.Image_IndexedPixelFormatRequiresPalette);
				}
				return false;
			}
			if (1 << DestinationFormat.BitsPerPixel < DestinationPalette.Colors.Count)
			{
				if (throwIfInvalid)
				{
					throw new InvalidOperationException(SR.Image_PaletteColorsDoNotMatchFormat);
				}
				return false;
			}
		}
		return true;
	}

	private void DestinationFormatPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_destinationFormat = (PixelFormat)e.NewValue;
		}
	}

	private void DestinationPalettePropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_destinationPalette = e.NewValue as BitmapPalette;
		}
	}

	private void AlphaThresholdPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_alphaThreshold = (double)e.NewValue;
		}
	}

	private static object CoerceSource(DependencyObject d, object value)
	{
		FormatConvertedBitmap formatConvertedBitmap = (FormatConvertedBitmap)d;
		if (!formatConvertedBitmap._bitmapInit.IsInInit)
		{
			return formatConvertedBitmap._source;
		}
		return value;
	}

	private static object CoerceDestinationFormat(DependencyObject d, object value)
	{
		FormatConvertedBitmap formatConvertedBitmap = (FormatConvertedBitmap)d;
		if (!formatConvertedBitmap._bitmapInit.IsInInit)
		{
			return formatConvertedBitmap._destinationFormat;
		}
		if (((PixelFormat)value).Format == PixelFormatEnum.Default)
		{
			if (formatConvertedBitmap.Source != null)
			{
				return formatConvertedBitmap.Source.Format;
			}
			return formatConvertedBitmap._destinationFormat;
		}
		return value;
	}

	private static object CoerceDestinationPalette(DependencyObject d, object value)
	{
		FormatConvertedBitmap formatConvertedBitmap = (FormatConvertedBitmap)d;
		if (!formatConvertedBitmap._bitmapInit.IsInInit)
		{
			return formatConvertedBitmap._destinationPalette;
		}
		return value;
	}

	private static object CoerceAlphaThreshold(DependencyObject d, object value)
	{
		FormatConvertedBitmap formatConvertedBitmap = (FormatConvertedBitmap)d;
		if (!formatConvertedBitmap._bitmapInit.IsInInit)
		{
			return formatConvertedBitmap._alphaThreshold;
		}
		return value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new FormatConvertedBitmap Clone()
	{
		return (FormatConvertedBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.FormatConvertedBitmap" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new FormatConvertedBitmap CloneCurrentValue()
	{
		return (FormatConvertedBitmap)base.CloneCurrentValue();
	}

	private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FormatConvertedBitmap formatConvertedBitmap = (FormatConvertedBitmap)d;
		formatConvertedBitmap.SourcePropertyChangedHook(e);
		if (!e.IsASubPropertyChange || e.OldValueSource != e.NewValueSource)
		{
			formatConvertedBitmap.PropertyChanged(SourceProperty);
		}
	}

	private static void DestinationFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FormatConvertedBitmap obj = (FormatConvertedBitmap)d;
		obj.DestinationFormatPropertyChangedHook(e);
		obj.PropertyChanged(DestinationFormatProperty);
	}

	private static void DestinationPalettePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FormatConvertedBitmap obj = (FormatConvertedBitmap)d;
		obj.DestinationPalettePropertyChangedHook(e);
		obj.PropertyChanged(DestinationPaletteProperty);
	}

	private static void AlphaThresholdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FormatConvertedBitmap obj = (FormatConvertedBitmap)d;
		obj.AlphaThresholdPropertyChangedHook(e);
		obj.PropertyChanged(AlphaThresholdProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new FormatConvertedBitmap();
	}

	protected override void CloneCore(Freezable source)
	{
		FormatConvertedBitmap otherFormatConvertedBitmap = (FormatConvertedBitmap)source;
		ClonePrequel(otherFormatConvertedBitmap);
		base.CloneCore(source);
		ClonePostscript(otherFormatConvertedBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		FormatConvertedBitmap otherFormatConvertedBitmap = (FormatConvertedBitmap)source;
		ClonePrequel(otherFormatConvertedBitmap);
		base.CloneCurrentValueCore(source);
		ClonePostscript(otherFormatConvertedBitmap);
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		FormatConvertedBitmap otherFormatConvertedBitmap = (FormatConvertedBitmap)source;
		ClonePrequel(otherFormatConvertedBitmap);
		base.GetAsFrozenCore(source);
		ClonePostscript(otherFormatConvertedBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		FormatConvertedBitmap otherFormatConvertedBitmap = (FormatConvertedBitmap)source;
		ClonePrequel(otherFormatConvertedBitmap);
		base.GetCurrentValueAsFrozenCore(source);
		ClonePostscript(otherFormatConvertedBitmap);
	}

	static FormatConvertedBitmap()
	{
		s_Source = null;
		s_DestinationFormat = PixelFormats.Pbgra32;
		s_DestinationPalette = null;
		Type typeFromHandle = typeof(FormatConvertedBitmap);
		SourceProperty = Animatable.RegisterProperty("Source", typeof(BitmapSource), typeFromHandle, null, SourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceSource);
		DestinationFormatProperty = Animatable.RegisterProperty("DestinationFormat", typeof(PixelFormat), typeFromHandle, PixelFormats.Pbgra32, DestinationFormatPropertyChanged, null, isIndependentlyAnimated: false, CoerceDestinationFormat);
		DestinationPaletteProperty = Animatable.RegisterProperty("DestinationPalette", typeof(BitmapPalette), typeFromHandle, null, DestinationPalettePropertyChanged, null, isIndependentlyAnimated: false, CoerceDestinationPalette);
		AlphaThresholdProperty = Animatable.RegisterProperty("AlphaThreshold", typeof(double), typeFromHandle, 0.0, AlphaThresholdPropertyChanged, null, isIndependentlyAnimated: false, CoerceAlphaThreshold);
	}
}
