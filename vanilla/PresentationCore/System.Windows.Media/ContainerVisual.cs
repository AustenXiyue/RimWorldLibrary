using System.ComponentModel;
using System.Windows.Media.Effects;

namespace System.Windows.Media;

/// <summary>Manages a collection of <see cref="T:System.Windows.Media.Visual" /> objects.</summary>
public class ContainerVisual : Visual
{
	private readonly VisualCollection _children;

	/// <summary>Gets the child collection of the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.VisualCollection" /> that contains the children of the <see cref="T:System.Windows.Media.ContainerVisual" />.</returns>
	public VisualCollection Children
	{
		get
		{
			VerifyAPIReadOnly();
			return _children;
		}
	}

	/// <summary>Gets the parent <see cref="T:System.Windows.Media.Visual" /> for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The parent of the visual.</returns>
	public DependencyObject Parent => base.VisualParent;

	/// <summary>Gets or sets the clipping region of the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> that defines the clipping region.</returns>
	public Geometry Clip
	{
		get
		{
			return base.VisualClip;
		}
		set
		{
			base.VisualClip = value;
		}
	}

	/// <summary>Gets or sets the opacity of the <see cref="T:System.Windows.Media.ContainerVisual" />, based on 0=transparent, 1=opaque.</summary>
	/// <returns>A value from 0 through 1 that specifies a range from fully transparent to fully opaque. A value of 0 indicates that the <see cref="T:System.Windows.Media.ContainerVisual" /> is completely transparent, while a value of 1 indicates that the <see cref="T:System.Windows.Media.ContainerVisual" /> is completely opaque. A value 0.5 indicates 50 percent opaque, a value of 0.725 indicates 72.5 percent opaque, and so on. Values less than 0 are treated as 0, while values greater than 1 are treated as 1.</returns>
	public double Opacity
	{
		get
		{
			return base.VisualOpacity;
		}
		set
		{
			base.VisualOpacity = value;
		}
	}

	/// <summary>Gets or sets a brush that specifies a possible opacity mask for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.Brush" /> that represents the opacity mask value of the <see cref="T:System.Windows.Media.ContainerVisual" />.</returns>
	public Brush OpacityMask
	{
		get
		{
			return base.VisualOpacityMask;
		}
		set
		{
			base.VisualOpacityMask = value;
		}
	}

	/// <summary>Gets or sets a cached representation of the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.CacheMode" /> that holds a cached representation of the <see cref="T:System.Windows.Media.ContainerVisual" />.</returns>
	public CacheMode CacheMode
	{
		get
		{
			return base.VisualCacheMode;
		}
		set
		{
			base.VisualCacheMode = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> value for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The bitmap effect for this visual object.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffect BitmapEffect
	{
		get
		{
			return base.VisualBitmapEffect;
		}
		set
		{
			base.VisualBitmapEffect = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> value for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The bitmap effect input value for this visual object.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffectInput BitmapEffectInput
	{
		get
		{
			return base.VisualBitmapEffectInput;
		}
		set
		{
			base.VisualBitmapEffectInput = value;
		}
	}

	/// <summary>Gets or sets the bitmap effect to apply to the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Effects.Effect" /> that represents the bitmap effect.</returns>
	public Effect Effect
	{
		get
		{
			return base.VisualEffect;
		}
		set
		{
			base.VisualEffect = value;
		}
	}

	/// <summary>Gets or sets the X (horizontal) guideline for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The horizontal guideline.</returns>
	[DefaultValue(null)]
	public DoubleCollection XSnappingGuidelines
	{
		get
		{
			return base.VisualXSnappingGuidelines;
		}
		set
		{
			base.VisualXSnappingGuidelines = value;
		}
	}

	/// <summary>Gets or sets the Y (vertical) guideline for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The vertical guideline.</returns>
	[DefaultValue(null)]
	public DoubleCollection YSnappingGuidelines
	{
		get
		{
			return base.VisualYSnappingGuidelines;
		}
		set
		{
			base.VisualYSnappingGuidelines = value;
		}
	}

	/// <summary>Gets the bounding box for the contents of the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that specifies the bounding box.</returns>
	public Rect ContentBounds => base.VisualContentBounds;

	/// <summary>Gets or sets the transform that is applied to the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The transform value.</returns>
	public Transform Transform
	{
		get
		{
			return base.VisualTransform;
		}
		set
		{
			base.VisualTransform = value;
		}
	}

	/// <summary>Gets or sets the offset value of the <see cref="T:System.Windows.Media.ContainerVisual" /> from its reference point.</summary>
	/// <returns>A <see cref="T:System.Windows.Vector" /> that represents the offset value of the <see cref="T:System.Windows.Media.ContainerVisual" />.</returns>
	public Vector Offset
	{
		get
		{
			return base.VisualOffset;
		}
		set
		{
			base.VisualOffset = value;
		}
	}

	/// <summary>Gets the union of all the content bounding boxes for all of the descendants of the <see cref="T:System.Windows.Media.ContainerVisual" />, but not including the contents of the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that specifies the combination bounding box.</returns>
	public Rect DescendantBounds => base.VisualDescendantBounds;

	/// <summary>Gets the number of children for the <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The number of children in the <see cref="T:System.Windows.Media.VisualCollection" /> of the <see cref="T:System.Windows.Media.ContainerVisual" />.</returns>
	protected sealed override int VisualChildrenCount => _children.Count;

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.ContainerVisual" /> class.</summary>
	public ContainerVisual()
	{
		_children = new VisualCollection(this);
	}

	/// <summary>Returns the top-most visual object of a hit test by specifying a <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>The hit test result of the visual returned as a <see cref="T:System.Windows.Media.HitTestResult" /> type.</returns>
	/// <param name="point">The point value to hit test.</param>
	public new HitTestResult HitTest(Point point)
	{
		return base.HitTest(point);
	}

	/// <summary>Initiates a hit test on the <see cref="T:System.Windows.Media.ContainerVisual" /> by using the <see cref="T:System.Windows.Media.HitTestFilterCallback" /> and <see cref="T:System.Windows.Media.HitTestResultCallback" /> objects.</summary>
	/// <param name="filterCallback">The delegate that allows you to ignore parts of the visual tree that you are not interested in processing in your hit test results.</param>
	/// <param name="resultCallback">The delegate that is used to control the return of hit test information.</param>
	/// <param name="hitTestParameters">Defines the set of parameters for a hit test.</param>
	public new void HitTest(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters hitTestParameters)
	{
		base.HitTest(filterCallback, resultCallback, hitTestParameters);
	}

	/// <summary>Returns a specified child <see cref="T:System.Windows.Media.Visual" /> for the parent <see cref="T:System.Windows.Media.ContainerVisual" />.</summary>
	/// <returns>The child <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="index">A 32-bit signed integer that represents the index value of the child <see cref="T:System.Windows.Media.Visual" />. The value of <paramref name="index" /> must be between 0 and <see cref="P:System.Windows.Media.ContainerVisual.VisualChildrenCount" /> - 1.</param>
	protected sealed override Visual GetVisualChild(int index)
	{
		return _children[index];
	}
}
