using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Describes a way to paint a region by using one or more tiles. </summary>
public abstract class TileBrush : Brush
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.ViewportUnits" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.ViewportUnits" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportUnitsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.ViewboxUnits" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.ViewboxUnits" /> dependency property.</returns>
	public static readonly DependencyProperty ViewboxUnitsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.Viewport" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.Viewport" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.Viewbox" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.Viewbox" /> dependency property.</returns>
	public static readonly DependencyProperty ViewboxProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.Stretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.Stretch" /> dependency property.</returns>
	public static readonly DependencyProperty StretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.TileMode" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.TileMode" /> dependency property.</returns>
	public static readonly DependencyProperty TileModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.AlignmentX" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.AlignmentX" /> dependency property.</returns>
	public static readonly DependencyProperty AlignmentXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TileBrush.AlignmentY" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TileBrush.AlignmentY" /> dependency property.</returns>
	public static readonly DependencyProperty AlignmentYProperty;

	internal const BrushMappingMode c_ViewportUnits = BrushMappingMode.RelativeToBoundingBox;

	internal const BrushMappingMode c_ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;

	internal static Rect s_Viewport;

	internal static Rect s_Viewbox;

	internal const Stretch c_Stretch = Stretch.Fill;

	internal const TileMode c_TileMode = TileMode.None;

	internal const AlignmentX c_AlignmentX = AlignmentX.Center;

	internal const AlignmentY c_AlignmentY = AlignmentY.Center;

	internal const CachingHint c_CachingHint = CachingHint.Unspecified;

	internal const double c_CacheInvalidationThresholdMinimum = 0.707;

	internal const double c_CacheInvalidationThresholdMaximum = 1.414;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.BrushMappingMode" /> enumeration that specifies whether the value of the <see cref="P:System.Windows.Media.TileBrush.Viewport" />, which indicates the size and position of the <see cref="T:System.Windows.Media.TileBrush" /> base tile, is relative to the size of the output area.  </summary>
	/// <returns>Indicates whether the value of the <see cref="P:System.Windows.Media.TileBrush.Viewport" />, which describes the size and position of the <see cref="T:System.Windows.Media.TileBrush" /> tiles, is relative to the size of the whole output area. The default value is <see cref="F:System.Windows.Media.BrushMappingMode.RelativeToBoundingBox" />.</returns>
	public BrushMappingMode ViewportUnits
	{
		get
		{
			return (BrushMappingMode)GetValue(ViewportUnitsProperty);
		}
		set
		{
			SetValueInternal(ViewportUnitsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies whether the <see cref="P:System.Windows.Media.TileBrush.Viewbox" /> value is relative to the bounding box of the <see cref="T:System.Windows.Media.TileBrush" /> contents or whether the value is absolute.  </summary>
	/// <returns>A value that indicates whether the <see cref="P:System.Windows.Media.TileBrush.Viewbox" /> value is relative to the bounding box of the <see cref="T:System.Windows.Media.TileBrush" /> contents or whether it is an absolute value. The default value is <see cref="F:System.Windows.Media.BrushMappingMode.RelativeToBoundingBox" />.</returns>
	public BrushMappingMode ViewboxUnits
	{
		get
		{
			return (BrushMappingMode)GetValue(ViewboxUnitsProperty);
		}
		set
		{
			SetValueInternal(ViewboxUnitsProperty, value);
		}
	}

	/// <summary>Gets or sets the position and dimensions of the base tile for a <see cref="T:System.Windows.Media.TileBrush" />.  </summary>
	/// <returns>The position and dimensions of the base tile for a <see cref="T:System.Windows.Media.TileBrush" />. The default value is a rectangle (<see cref="T:System.Windows.Rect" />) with a <see cref="P:System.Windows.Rect.TopLeft" /> of (0,0) and a <see cref="P:System.Windows.Rect.Width" /> and <see cref="P:System.Windows.Rect.Height" /> of 1. </returns>
	public Rect Viewport
	{
		get
		{
			return (Rect)GetValue(ViewportProperty);
		}
		set
		{
			SetValueInternal(ViewportProperty, value);
		}
	}

	/// <summary>Gets or sets the position and dimensions of the content in a <see cref="T:System.Windows.Media.TileBrush" /> tile.  </summary>
	/// <returns>The position and dimensions of the <see cref="T:System.Windows.Media.TileBrush" /> content. The default value is a rectangle (<see cref="T:System.Windows.Rect" />) that has a <see cref="P:System.Windows.Rect.TopLeft" /> of (0,0), and a <see cref="P:System.Windows.Rect.Width" /> and <see cref="P:System.Windows.Rect.Height" /> of 1.</returns>
	public Rect Viewbox
	{
		get
		{
			return (Rect)GetValue(ViewboxProperty);
		}
		set
		{
			SetValueInternal(ViewboxProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies how the content of this <see cref="T:System.Windows.Media.TileBrush" /> stretches to fit its tiles.  </summary>
	/// <returns>A value that specifies how this <see cref="T:System.Windows.Media.TileBrush" /> content is projected onto its base tile. The default value is <see cref="F:System.Windows.Media.Stretch.Fill" />.</returns>
	public Stretch Stretch
	{
		get
		{
			return (Stretch)GetValue(StretchProperty);
		}
		set
		{
			SetValueInternal(StretchProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies how a <see cref="T:System.Windows.Media.TileBrush" /> fills the area that you are painting if the base tile is smaller than the output area.  </summary>
	/// <returns>A value that specifies how the <see cref="T:System.Windows.Media.TileBrush" /> tiles fill the output area when the base tile, which is specified by the <see cref="P:System.Windows.Media.TileBrush.Viewport" /> property, is smaller than the output area. The default value is <see cref="F:System.Windows.Media.TileMode.None" />.</returns>
	public TileMode TileMode
	{
		get
		{
			return (TileMode)GetValue(TileModeProperty);
		}
		set
		{
			SetValueInternal(TileModeProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal alignment of content in the <see cref="T:System.Windows.Media.TileBrush" /> base tile.  </summary>
	/// <returns>A value that specifies the horizontal position of <see cref="T:System.Windows.Media.TileBrush" /> content in its base tile. The default value is <see cref="F:System.Windows.HorizontalAlignment.Center" />.</returns>
	public AlignmentX AlignmentX
	{
		get
		{
			return (AlignmentX)GetValue(AlignmentXProperty);
		}
		set
		{
			SetValueInternal(AlignmentXProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical alignment of content in the <see cref="T:System.Windows.Media.TileBrush" /> base tile.  </summary>
	/// <returns>A value that specifies the vertical position of <see cref="T:System.Windows.Media.TileBrush" /> content in its base tile. The default value is <see cref="F:System.Windows.Media.AlignmentY.Center" />.</returns>
	public AlignmentY AlignmentY
	{
		get
		{
			return (AlignmentY)GetValue(AlignmentYProperty);
		}
		set
		{
			SetValueInternal(AlignmentYProperty, value);
		}
	}

	/// <summary>Provides initialization for base class values when called by the constructor of a derived class.  </summary>
	protected TileBrush()
	{
	}

	/// <summary>Obtains the current bounds of the <see cref="T:System.Windows.Media.TileBrush" /> content </summary>
	/// <param name="contentBounds">The output bounds of the <see cref="T:System.Windows.Media.TileBrush" /> content.</param>
	protected abstract void GetContentBounds(out Rect contentBounds);

	internal unsafe void GetTileBrushMapping(Rect shapeFillBounds, out Matrix tileBrushMapping)
	{
		Rect contentBounds = Rect.Empty;
		BrushMappingMode viewboxUnits = ViewboxUnits;
		bool flag = false;
		tileBrushMapping = Matrix.Identity;
		if (viewboxUnits == BrushMappingMode.RelativeToBoundingBox)
		{
			GetContentBounds(out contentBounds);
			if (contentBounds == Rect.Empty)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			Rect viewport = Viewport;
			Rect viewbox = Viewbox;
			Transform.GetTransformValue(base.Transform, out var currentTransformValue);
			Transform.GetTransformValue(base.RelativeTransform, out var currentTransformValue2);
			D3DMATRIX d3DMATRIX = default(D3DMATRIX);
			MILUtilities.ConvertToD3DMATRIX(&currentTransformValue, &d3DMATRIX);
			D3DMATRIX d3DMATRIX2 = default(D3DMATRIX);
			MILUtilities.ConvertToD3DMATRIX(&currentTransformValue2, &d3DMATRIX2);
			UnsafeNativeMethods.MilCoreApi.MilUtility_GetTileBrushMapping(&d3DMATRIX, &d3DMATRIX2, Stretch, AlignmentX, AlignmentY, ViewportUnits, viewboxUnits, &shapeFillBounds, &contentBounds, ref viewport, ref viewbox, out var contentToShape, out var brushIsEmpty);
			if (brushIsEmpty == 0)
			{
				Matrix matrix = default(Matrix);
				MILUtilities.ConvertFromD3DMATRIX(&contentToShape, &matrix);
				tileBrushMapping = matrix;
			}
		}
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TileBrush" /> by making deep copies of its values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new TileBrush Clone()
	{
		return (TileBrush)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TileBrush" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values.</summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> prope-rty of the source is true.</returns>
	public new TileBrush CloneCurrentValue()
	{
		return (TileBrush)base.CloneCurrentValue();
	}

	private static void ViewportUnitsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(ViewportUnitsProperty);
	}

	private static void ViewboxUnitsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(ViewboxUnitsProperty);
	}

	private static void ViewportPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(ViewportProperty);
	}

	private static void ViewboxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(ViewboxProperty);
	}

	private static void StretchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(StretchProperty);
	}

	private static void TileModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(TileModeProperty);
	}

	private static void AlignmentXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(AlignmentXProperty);
	}

	private static void AlignmentYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(AlignmentYProperty);
	}

	private static void CachingHintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(RenderOptions.CachingHintProperty);
	}

	private static void CacheInvalidationThresholdMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(RenderOptions.CacheInvalidationThresholdMinimumProperty);
	}

	private static void CacheInvalidationThresholdMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TileBrush)d).PropertyChanged(RenderOptions.CacheInvalidationThresholdMaximumProperty);
	}

	static TileBrush()
	{
		s_Viewport = new Rect(0.0, 0.0, 1.0, 1.0);
		s_Viewbox = new Rect(0.0, 0.0, 1.0, 1.0);
		RenderOptions.CachingHintProperty.OverrideMetadata(typeof(TileBrush), new UIPropertyMetadata(CachingHint.Unspecified, CachingHintPropertyChanged));
		RenderOptions.CacheInvalidationThresholdMinimumProperty.OverrideMetadata(typeof(TileBrush), new UIPropertyMetadata(0.707, CacheInvalidationThresholdMinimumPropertyChanged));
		RenderOptions.CacheInvalidationThresholdMaximumProperty.OverrideMetadata(typeof(TileBrush), new UIPropertyMetadata(1.414, CacheInvalidationThresholdMaximumPropertyChanged));
		Type typeFromHandle = typeof(TileBrush);
		ViewportUnitsProperty = Animatable.RegisterProperty("ViewportUnits", typeof(BrushMappingMode), typeFromHandle, BrushMappingMode.RelativeToBoundingBox, ViewportUnitsPropertyChanged, ValidateEnums.IsBrushMappingModeValid, isIndependentlyAnimated: false, null);
		ViewboxUnitsProperty = Animatable.RegisterProperty("ViewboxUnits", typeof(BrushMappingMode), typeFromHandle, BrushMappingMode.RelativeToBoundingBox, ViewboxUnitsPropertyChanged, ValidateEnums.IsBrushMappingModeValid, isIndependentlyAnimated: false, null);
		ViewportProperty = Animatable.RegisterProperty("Viewport", typeof(Rect), typeFromHandle, new Rect(0.0, 0.0, 1.0, 1.0), ViewportPropertyChanged, null, isIndependentlyAnimated: true, null);
		ViewboxProperty = Animatable.RegisterProperty("Viewbox", typeof(Rect), typeFromHandle, new Rect(0.0, 0.0, 1.0, 1.0), ViewboxPropertyChanged, null, isIndependentlyAnimated: true, null);
		StretchProperty = Animatable.RegisterProperty("Stretch", typeof(Stretch), typeFromHandle, Stretch.Fill, StretchPropertyChanged, ValidateEnums.IsStretchValid, isIndependentlyAnimated: false, null);
		TileModeProperty = Animatable.RegisterProperty("TileMode", typeof(TileMode), typeFromHandle, TileMode.None, TileModePropertyChanged, ValidateEnums.IsTileModeValid, isIndependentlyAnimated: false, null);
		AlignmentXProperty = Animatable.RegisterProperty("AlignmentX", typeof(AlignmentX), typeFromHandle, AlignmentX.Center, AlignmentXPropertyChanged, ValidateEnums.IsAlignmentXValid, isIndependentlyAnimated: false, null);
		AlignmentYProperty = Animatable.RegisterProperty("AlignmentY", typeof(AlignmentY), typeFromHandle, AlignmentY.Center, AlignmentYPropertyChanged, ValidateEnums.IsAlignmentYValid, isIndependentlyAnimated: false, null);
	}
}
