using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Scales and rotates a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />. </summary>
public sealed class TransformedBitmap : BitmapSource, ISupportInitialize
{
	private BitmapSource _source;

	private Transform _transform;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Source" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Transform" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Transform" /> dependency property.</returns>
	public static readonly DependencyProperty TransformProperty;

	internal static BitmapSource s_Source;

	internal static Transform s_Transform;

	/// <summary>Gets or sets the source for the bitmap.  </summary>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Transform" />, which specifies the scale or rotation of the bitmap.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Transform" />, which specifies the scale or rotation of the bitmap. The default value is <see cref="P:System.Windows.Media.Transform.Identity" />.</returns>
	public Transform Transform
	{
		get
		{
			return (Transform)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> class.</summary>
	public TransformedBitmap()
		: base(useVirtuals: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> class that has the specified <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Source" /> and <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Transform" />. </summary>
	/// <param name="source">The <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Source" /> of the new <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> instance.</param>
	/// <param name="newTransform">The <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Transform" /> of the new <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="source" /> parameter is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="newTransform" /> parameter is null.-or-The transform is not an orthogonal transform.</exception>
	public TransformedBitmap(BitmapSource source, Transform newTransform)
		: base(useVirtuals: true)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (newTransform == null)
		{
			throw new InvalidOperationException(SR.Format(SR.Image_NoArgument, "Transform"));
		}
		if (!CheckTransform(newTransform))
		{
			throw new InvalidOperationException(SR.Image_OnlyOrthogonal);
		}
		_bitmapInit.BeginInit();
		Source = source;
		Transform = newTransform;
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> is currently being initialized. <see cref="M:System.Windows.Media.Imaging.TransformedBitmap.BeginInit" /> has already been called.-or-The <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> has already been initialized.</exception>
	public void BeginInit()
	{
		WritePreamble();
		_bitmapInit.BeginInit();
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Media.Imaging.BitmapImage" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Source" /> or <see cref="P:System.Windows.Media.Imaging.TransformedBitmap.Transform" /> properties are null.-or-The transform is not an orthogonal transform.-or-The <see cref="M:System.Windows.Media.Imaging.TransformedBitmap.EndInit" /> method is called without first calling <see cref="M:System.Windows.Media.Imaging.TransformedBitmap.BeginInit" />.</exception>
	public void EndInit()
	{
		WritePreamble();
		_bitmapInit.EndInit();
		IsValidForFinalizeCreation(throwIfInvalid: true);
		FinalizeCreation();
	}

	private void ClonePrequel(TransformedBitmap otherTransformedBitmap)
	{
		BeginInit();
	}

	private void ClonePostscript(TransformedBitmap otherTransformedBitmap)
	{
		EndInit();
	}

	internal bool CheckTransform(Transform newTransform)
	{
		Matrix value = newTransform.Value;
		bool result = false;
		if ((DoubleUtil.IsZero(value.M11) && DoubleUtil.IsZero(value.M22)) || (DoubleUtil.IsZero(value.M12) && DoubleUtil.IsZero(value.M21)))
		{
			result = true;
		}
		return result;
	}

	internal void GetParamsFromTransform(Transform newTransform, out double scaleX, out double scaleY, out WICBitmapTransformOptions options)
	{
		Matrix value = newTransform.Value;
		if (DoubleUtil.IsZero(value.M12) && DoubleUtil.IsZero(value.M21))
		{
			scaleX = Math.Abs(value.M11);
			scaleY = Math.Abs(value.M22);
			options = WICBitmapTransformOptions.WICBitmapTransformRotate0;
			if (value.M11 < 0.0)
			{
				options |= WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal;
			}
			if (value.M22 < 0.0)
			{
				options |= WICBitmapTransformOptions.WICBitmapTransformFlipVertical;
			}
		}
		else
		{
			scaleX = Math.Abs(value.M12);
			scaleY = Math.Abs(value.M21);
			options = WICBitmapTransformOptions.WICBitmapTransformRotate90;
			if (value.M12 < 0.0)
			{
				options |= WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal;
			}
			if (value.M21 >= 0.0)
			{
				options |= WICBitmapTransformOptions.WICBitmapTransformFlipVertical;
			}
		}
	}

	internal override void FinalizeCreation()
	{
		_bitmapInit.EnsureInitializedComplete();
		BitmapSourceSafeMILHandle bitmapSourceSafeMILHandle = null;
		GetParamsFromTransform(Transform, out var scaleX, out var scaleY, out var options);
		using (FactoryMaker factoryMaker = new FactoryMaker())
		{
			try
			{
				nint imagingFactoryPtr = factoryMaker.ImagingFactoryPtr;
				bitmapSourceSafeMILHandle = _source.WicSourceHandle;
				if (!DoubleUtil.IsOne(scaleX) || !DoubleUtil.IsOne(scaleY))
				{
					uint width = Math.Max(1u, (uint)(scaleX * (double)_source.PixelWidth + 0.5));
					uint height = Math.Max(1u, (uint)(scaleY * (double)_source.PixelHeight + 0.5));
					HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapScaler(imagingFactoryPtr, out bitmapSourceSafeMILHandle));
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICBitmapScaler.Initialize(bitmapSourceSafeMILHandle, _source.WicSourceHandle, width, height, WICInterpolationMode.Fant));
					}
				}
				if (options != 0)
				{
					bitmapSourceSafeMILHandle = BitmapSource.CreateCachedBitmap(null, bitmapSourceSafeMILHandle, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default, _source.Palette);
					BitmapSourceSafeMILHandle ppBitmapFlipRotator = null;
					HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFlipRotator(imagingFactoryPtr, out ppBitmapFlipRotator));
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICBitmapFlipRotator.Initialize(ppBitmapFlipRotator, bitmapSourceSafeMILHandle, options));
					}
					bitmapSourceSafeMILHandle = ppBitmapFlipRotator;
				}
				if (options == WICBitmapTransformOptions.WICBitmapTransformRotate0 && DoubleUtil.IsOne(scaleX) && DoubleUtil.IsOne(scaleY))
				{
					HRESULT.Check(UnsafeNativeMethods.WICImagingFactory.CreateBitmapFlipRotator(imagingFactoryPtr, out bitmapSourceSafeMILHandle));
					lock (_syncObject)
					{
						HRESULT.Check(UnsafeNativeMethods.WICBitmapFlipRotator.Initialize(bitmapSourceSafeMILHandle, _source.WicSourceHandle, WICBitmapTransformOptions.WICBitmapTransformRotate0));
					}
				}
				base.WicSourceHandle = bitmapSourceSafeMILHandle;
				_isSourceCached = _source.IsSourceCached;
			}
			catch
			{
				_bitmapInit.Reset();
				throw;
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
		Transform transform = Transform;
		if (transform == null)
		{
			if (throwIfInvalid)
			{
				throw new InvalidOperationException(SR.Format(SR.Image_NoArgument, "Transform"));
			}
			return false;
		}
		if (!CheckTransform(transform))
		{
			if (throwIfInvalid)
			{
				throw new InvalidOperationException(SR.Image_OnlyOrthogonal);
			}
			return false;
		}
		return true;
	}

	private void TransformPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			_transform = e.NewValue as Transform;
		}
	}

	private static object CoerceSource(DependencyObject d, object value)
	{
		TransformedBitmap transformedBitmap = (TransformedBitmap)d;
		if (!transformedBitmap._bitmapInit.IsInInit)
		{
			return transformedBitmap._source;
		}
		return value;
	}

	private static object CoerceTransform(DependencyObject d, object value)
	{
		TransformedBitmap transformedBitmap = (TransformedBitmap)d;
		if (!transformedBitmap._bitmapInit.IsInInit)
		{
			return transformedBitmap._transform;
		}
		return value;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TransformedBitmap Clone()
	{
		return (TransformedBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.TransformedBitmap" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TransformedBitmap CloneCurrentValue()
	{
		return (TransformedBitmap)base.CloneCurrentValue();
	}

	private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TransformedBitmap transformedBitmap = (TransformedBitmap)d;
		transformedBitmap.SourcePropertyChangedHook(e);
		if (!e.IsASubPropertyChange || e.OldValueSource != e.NewValueSource)
		{
			transformedBitmap.PropertyChanged(SourceProperty);
		}
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TransformedBitmap transformedBitmap = (TransformedBitmap)d;
		transformedBitmap.TransformPropertyChangedHook(e);
		if (!e.IsASubPropertyChange || e.OldValueSource != e.NewValueSource)
		{
			transformedBitmap.PropertyChanged(TransformProperty);
		}
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TransformedBitmap();
	}

	protected override void CloneCore(Freezable source)
	{
		TransformedBitmap otherTransformedBitmap = (TransformedBitmap)source;
		ClonePrequel(otherTransformedBitmap);
		base.CloneCore(source);
		ClonePostscript(otherTransformedBitmap);
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		TransformedBitmap otherTransformedBitmap = (TransformedBitmap)source;
		ClonePrequel(otherTransformedBitmap);
		base.CloneCurrentValueCore(source);
		ClonePostscript(otherTransformedBitmap);
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		TransformedBitmap otherTransformedBitmap = (TransformedBitmap)source;
		ClonePrequel(otherTransformedBitmap);
		base.GetAsFrozenCore(source);
		ClonePostscript(otherTransformedBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		TransformedBitmap otherTransformedBitmap = (TransformedBitmap)source;
		ClonePrequel(otherTransformedBitmap);
		base.GetCurrentValueAsFrozenCore(source);
		ClonePostscript(otherTransformedBitmap);
	}

	static TransformedBitmap()
	{
		s_Source = null;
		s_Transform = Transform.Identity;
		Type typeFromHandle = typeof(TransformedBitmap);
		SourceProperty = Animatable.RegisterProperty("Source", typeof(BitmapSource), typeFromHandle, null, SourcePropertyChanged, null, isIndependentlyAnimated: false, CoerceSource);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform), typeFromHandle, Transform.Identity, TransformPropertyChanged, null, isIndependentlyAnimated: false, CoerceTransform);
	}
}
