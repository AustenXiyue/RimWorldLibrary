using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Crops a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
public sealed class CroppedBitmap : BitmapSource, ISupportInitialize
{
	private BitmapSource _source;

	private Int32Rect _sourceRect;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.Source" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.SourceRect" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.SourceRect" /> dependency property.</returns>
	public static readonly DependencyProperty SourceRectProperty;

	internal static BitmapSource s_Source;

	internal static Int32Rect s_SourceRect;

	/// <summary>Gets or sets the source for the bitmap.   </summary>
	/// <returns>The source for the bitmap. The default value is null.</returns>
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

	/// <summary>Gets or sets the rectangular area that the bitmap is cropped to.  </summary>
	/// <returns>The rectangular area that the bitmap is cropped to. The default is <see cref="P:System.Windows.Int32Rect.Empty" />.</returns>
	public Int32Rect SourceRect
	{
		get
		{
			return (Int32Rect)GetValue(SourceRectProperty);
		}
		set
		{
			SetValueInternal(SourceRectProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> class.</summary>
	public CroppedBitmap()
		: base(useVirtuals: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> class that has the specified <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.Source" /> and <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.SourceRect" />.</summary>
	/// <param name="source">The <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.Source" /> of the new <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> instance.</param>
	/// <param name="sourceRect">The <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.SourceRect" /> of the new <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs when s<paramref name="ource" /> is null.</exception>
	public CroppedBitmap(BitmapSource source, Int32Rect sourceRect)
		: base(useVirtuals: true)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		_bitmapInit.BeginInit();
		Source = source;
		SourceRect = sourceRect;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> is currently being initialized. <see cref="M:System.Windows.Media.Imaging.CroppedBitmap.BeginInit" /> has already been called.-or-The <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> has already been initialized.</exception>
	public void BeginInit()
	{
		WritePreamble();
		_bitmapInit.BeginInit();
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Media.Imaging.CroppedBitmap.Source" /> property is null.-or-The <see cref="M:System.Windows.Media.Imaging.CroppedBitmap.EndInit" /> method is called without first calling <see cref="M:System.Windows.Media.Imaging.CroppedBitmap.BeginInit" />.</exception>
	public void EndInit()
	{
		WritePreamble();
		_bitmapInit.EndInit();
		IsValidForFinalizeCreation(throwIfInvalid: true);
		FinalizeCreation();
	}

	private void ClonePrequel(CroppedBitmap otherCroppedBitmap)
	{
		BeginInit();
	}

	private void ClonePostscript(CroppedBitmap otherCroppedBitmap)
	{
		EndInit();
	}

	internal override void FinalizeCreation()
	{
		_bitmapInit.EnsureInitializedComplete();
		BitmapSourceSafeMILHandle ppBitmapClipper = null;
		Int32Rect prc = SourceRect;
		BitmapSource source = Source;
		if (prc.IsEmpty)
		{
			prc.Width = source.PixelWidth;
			prc.Height = source.PixelHeight;
		}
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			try
			{
				HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapClipper(factoryMaker.ImagingFactoryPtr, out ppBitmapClipper));
				lock (_syncObject)
				{
					HRESULT.Check(UnsafeNativeMethods.WICBitmapClipper.Initialize(ppBitmapClipper, source.WicSourceHandle, ref prc));
				}
				base.WicSourceHandle = ppBitmapClipper;
				_isSourceCached = source.IsSourceCached;
				ppBitmapClipper = null;
			}
			catch
			{
				_bitmapInit.Reset();
				throw;
			}
			finally
			{
				ppBitmapClipper?.Close();
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
		return true;
	}

	private void SourceRectPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_sourceRect = (Int32Rect)e.NewValue;
		}
	}

	private static object CoerceSource(DependencyObject d, object value)
	{
		CroppedBitmap croppedBitmap = (CroppedBitmap)d;
		if (!croppedBitmap._bitmapInit.IsInInit)
		{
			return croppedBitmap._source;
		}
		return value;
	}

	private static object CoerceSourceRect(DependencyObject d, object value)
	{
		CroppedBitmap croppedBitmap = (CroppedBitmap)d;
		if (!croppedBitmap._bitmapInit.IsInInit)
		{
			return croppedBitmap._sourceRect;
		}
		return value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CroppedBitmap Clone()
	{
		return (CroppedBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.CroppedBitmap" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CroppedBitmap CloneCurrentValue()
	{
		return (CroppedBitmap)base.CloneCurrentValue();
	}

	private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CroppedBitmap croppedBitmap = (CroppedBitmap)d;
		croppedBitmap.SourcePropertyChangedHook(e);
		if (!e.IsASubPropertyChange || e.OldValueSource != e.NewValueSource)
		{
			croppedBitmap.PropertyChanged(SourceProperty);
		}
	}

	private static void SourceRectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CroppedBitmap obj = (CroppedBitmap)d;
		obj.SourceRectPropertyChangedHook(e);
		obj.PropertyChanged(SourceRectProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new CroppedBitmap();
	}

	protected override void CloneCore(Freezable source)
	{
		CroppedBitmap otherCroppedBitmap = (CroppedBitmap)source;
		ClonePrequel(otherCroppedBitmap);
		base.CloneCore(source);
		ClonePostscript(otherCroppedBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		CroppedBitmap otherCroppedBitmap = (CroppedBitmap)source;
		ClonePrequel(otherCroppedBitmap);
		base.CloneCurrentValueCore(source);
		ClonePostscript(otherCroppedBitmap);
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		CroppedBitmap otherCroppedBitmap = (CroppedBitmap)source;
		ClonePrequel(otherCroppedBitmap);
		base.GetAsFrozenCore(source);
		ClonePostscript(otherCroppedBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		CroppedBitmap otherCroppedBitmap = (CroppedBitmap)source;
		ClonePrequel(otherCroppedBitmap);
		base.GetCurrentValueAsFrozenCore(source);
		ClonePostscript(otherCroppedBitmap);
	}

	static CroppedBitmap()
	{
		s_Source = null;
		s_SourceRect = Int32Rect.Empty;
		Type typeFromHandle = typeof(CroppedBitmap);
		SourceProperty = Animatable.RegisterProperty("Source", typeof(BitmapSource), typeFromHandle, null, SourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceSource);
		SourceRectProperty = Animatable.RegisterProperty("SourceRect", typeof(Int32Rect), typeFromHandle, Int32Rect.Empty, SourceRectPropertyChanged, null, isIndependentlyAnimated: false, CoerceSourceRect);
	}
}
