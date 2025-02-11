using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Represents a 2-D geometric shape defined by the combination of two <see cref="T:System.Windows.Media.Geometry" /> objects. </summary>
public sealed class CombinedGeometry : Geometry
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.CombinedGeometry.GeometryCombineMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.CombinedGeometry.GeometryCombineMode" /> dependency property.</returns>
	public static readonly DependencyProperty GeometryCombineModeProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.CombinedGeometry.Geometry1" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.CombinedGeometry.Geometry1" /> dependency property.</returns>
	public static readonly DependencyProperty Geometry1Property;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.CombinedGeometry.Geometry2" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.CombinedGeometry.Geometry2" /> dependency property.</returns>
	public static readonly DependencyProperty Geometry2Property;

	internal DUCE.MultiChannelResource _duceResource;

	internal const GeometryCombineMode c_GeometryCombineMode = GeometryCombineMode.Union;

	internal static Geometry s_Geometry1;

	internal static Geometry s_Geometry2;

	/// <summary> Gets a <see cref="T:System.Windows.Rect" /> that specifies the bounding box of this <see cref="T:System.Windows.Media.CombinedGeometry" /> object.   Note: This method does not take any pens into account.    </summary>
	/// <returns>The bounding box of this <see cref="T:System.Windows.Media.CombinedGeometry" />.</returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			return GetAsPathGeometry().Bounds;
		}
	}

	/// <summary>Gets or sets the method by which the two geometries (specified by the <see cref="P:System.Windows.Media.CombinedGeometry.Geometry1" /> and <see cref="P:System.Windows.Media.CombinedGeometry.Geometry2" /> properties) are combined. </summary>
	/// <returns>The method by which <see cref="P:System.Windows.Media.CombinedGeometry.Geometry1" /> and <see cref="P:System.Windows.Media.CombinedGeometry.Geometry2" /> are combined. The default value is <see cref="F:System.Windows.Media.GeometryCombineMode.Union" />.</returns>
	public GeometryCombineMode GeometryCombineMode
	{
		get
		{
			return (GeometryCombineMode)GetValue(GeometryCombineModeProperty);
		}
		set
		{
			SetValueInternal(GeometryCombineModeProperty, value);
		}
	}

	/// <summary> Gets or sets the first <see cref="T:System.Windows.Media.Geometry" /> object of this <see cref="T:System.Windows.Media.CombinedGeometry" /> object. </summary>
	/// <returns>The first <see cref="T:System.Windows.Media.Geometry" /> object to combine.</returns>
	public Geometry Geometry1
	{
		get
		{
			return (Geometry)GetValue(Geometry1Property);
		}
		set
		{
			SetValueInternal(Geometry1Property, value);
		}
	}

	/// <summary> Gets or sets the second <see cref="T:System.Windows.Media.Geometry" /> object of this <see cref="T:System.Windows.Media.CombinedGeometry" /> object. </summary>
	/// <returns>The second <see cref="T:System.Windows.Media.Geometry" /> object.</returns>
	public Geometry Geometry2
	{
		get
		{
			return (Geometry)GetValue(Geometry2Property);
		}
		set
		{
			SetValueInternal(Geometry2Property, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.CombinedGeometry" /> class. </summary>
	public CombinedGeometry()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.CombinedGeometry" /> class with the specified <see cref="T:System.Windows.Media.Geometry" /> objects. </summary>
	/// <param name="geometry1">The first <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	/// <param name="geometry2">The second <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	public CombinedGeometry(Geometry geometry1, Geometry geometry2)
	{
		Geometry1 = geometry1;
		Geometry2 = geometry2;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.CombinedGeometry" /> class with the specified <see cref="T:System.Windows.Media.Geometry" /> objects and <see cref="P:System.Windows.Media.CombinedGeometry.GeometryCombineMode" />.</summary>
	/// <param name="geometryCombineMode">The method by which <paramref name="geometry1" /> and <paramref name="geometry2" /> are combined.</param>
	/// <param name="geometry1">The first <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	/// <param name="geometry2">The second <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	public CombinedGeometry(GeometryCombineMode geometryCombineMode, Geometry geometry1, Geometry geometry2)
	{
		GeometryCombineMode = geometryCombineMode;
		Geometry1 = geometry1;
		Geometry2 = geometry2;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.CombinedGeometry" /> class with the specified <see cref="T:System.Windows.Media.Geometry" /> objects, <see cref="P:System.Windows.Media.CombinedGeometry.GeometryCombineMode" />, and <see cref="P:System.Windows.Media.Geometry.Transform" />.</summary>
	/// <param name="geometryCombineMode">The method by which <paramref name="geometry1" /> and <paramref name="geometry2" /> are combined.</param>
	/// <param name="geometry1">The first <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	/// <param name="geometry2">The second <see cref="T:System.Windows.Media.Geometry" /> to combine.</param>
	/// <param name="transform">The <see cref="P:System.Windows.Media.Geometry.Transform" /> applied to the <see cref="T:System.Windows.Media.CombinedGeometry" />.</param>
	public CombinedGeometry(GeometryCombineMode geometryCombineMode, Geometry geometry1, Geometry geometry2, Transform transform)
	{
		GeometryCombineMode = geometryCombineMode;
		Geometry1 = geometry1;
		Geometry2 = geometry2;
		base.Transform = transform;
	}

	internal override Rect GetBoundsInternal(Pen pen, Matrix matrix, double tolerance, ToleranceType type)
	{
		if (IsObviouslyEmpty())
		{
			return Rect.Empty;
		}
		return GetAsPathGeometry().GetBoundsInternal(pen, matrix, tolerance, type);
	}

	internal override bool ContainsInternal(Pen pen, Point hitPoint, double tolerance, ToleranceType type)
	{
		if (pen == null)
		{
			ReadPreamble();
			bool flag = false;
			bool flag2 = false;
			Transform transform = base.Transform;
			if (transform != null && !transform.IsIdentity)
			{
				Matrix value = transform.Value;
				if (!value.HasInverse)
				{
					return false;
				}
				value.Invert();
				hitPoint *= value;
			}
			Geometry geometry = Geometry1;
			Geometry geometry2 = Geometry2;
			if (geometry != null)
			{
				flag = geometry.ContainsInternal(pen, hitPoint, tolerance, type);
			}
			if (geometry2 != null)
			{
				flag2 = geometry2.ContainsInternal(pen, hitPoint, tolerance, type);
			}
			switch (GeometryCombineMode)
			{
			case GeometryCombineMode.Union:
				return flag || flag2;
			case GeometryCombineMode.Intersect:
				return flag && flag2;
			case GeometryCombineMode.Exclude:
				if (flag)
				{
					return !flag2;
				}
				return false;
			case GeometryCombineMode.Xor:
				return flag != flag2;
			default:
				return false;
			}
		}
		return base.ContainsInternal(pen, hitPoint, tolerance, type);
	}

	/// <summary> Gets the area of the filled region. </summary>
	/// <returns>The area of the filled region of this combined geometry.</returns>
	/// <param name="tolerance">The computational tolerance of error.</param>
	/// <param name="type">Specifies how the error tolerance will be interpreted.</param>
	public override double GetArea(double tolerance, ToleranceType type)
	{
		ReadPreamble();
		return GetAsPathGeometry().GetArea(tolerance, type);
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		return GetAsPathGeometry().GetTransformedFigureCollection(transform);
	}

	internal override PathGeometryData GetPathGeometryData()
	{
		if (IsObviouslyEmpty())
		{
			return Geometry.GetEmptyPathGeometryData();
		}
		return GetAsPathGeometry().GetPathGeometryData();
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		Geometry geometry = Geometry1;
		Geometry geometry2 = Geometry2;
		PathGeometry geometry3 = ((geometry == null) ? new PathGeometry() : geometry.GetAsPathGeometry());
		Geometry geometry4 = ((geometry2 == null) ? new PathGeometry() : geometry2.GetAsPathGeometry());
		return Geometry.Combine(geometry3, geometry4, GeometryCombineMode, base.Transform);
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.CombinedGeometry" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.CombinedGeometry" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		return GetAsPathGeometry().IsEmpty();
	}

	internal override bool IsObviouslyEmpty()
	{
		Geometry geometry = Geometry1;
		Geometry geometry2 = Geometry2;
		bool flag = geometry?.IsObviouslyEmpty() ?? true;
		bool flag2 = geometry2?.IsObviouslyEmpty() ?? true;
		if (GeometryCombineMode == GeometryCombineMode.Intersect)
		{
			return flag || flag2;
		}
		if (GeometryCombineMode == GeometryCombineMode.Exclude)
		{
			return flag;
		}
		return flag && flag2;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.CombinedGeometry" /> object may have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.CombinedGeometry" /> object may have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		Geometry geometry = Geometry1;
		Geometry geometry2 = Geometry2;
		if (geometry == null || !geometry.MayHaveCurves())
		{
			return geometry2?.MayHaveCurves() ?? false;
		}
		return true;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.CombinedGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CombinedGeometry Clone()
	{
		return (CombinedGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.CombinedGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new CombinedGeometry CloneCurrentValue()
	{
		return (CombinedGeometry)base.CloneCurrentValue();
	}

	private static void GeometryCombineModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CombinedGeometry)d).PropertyChanged(GeometryCombineModeProperty);
	}

	private static void Geometry1PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		CombinedGeometry combinedGeometry = (CombinedGeometry)d;
		Geometry resource = (Geometry)e.OldValue;
		Geometry resource2 = (Geometry)e.NewValue;
		if (combinedGeometry.Dispatcher != null)
		{
			DUCE.IResource resource3 = combinedGeometry;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					combinedGeometry.ReleaseResource(resource, channel);
					combinedGeometry.AddRefResource(resource2, channel);
				}
			}
		}
		combinedGeometry.PropertyChanged(Geometry1Property);
	}

	private static void Geometry2PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		CombinedGeometry combinedGeometry = (CombinedGeometry)d;
		Geometry resource = (Geometry)e.OldValue;
		Geometry resource2 = (Geometry)e.NewValue;
		if (combinedGeometry.Dispatcher != null)
		{
			DUCE.IResource resource3 = combinedGeometry;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					combinedGeometry.ReleaseResource(resource, channel);
					combinedGeometry.AddRefResource(resource2, channel);
				}
			}
		}
		combinedGeometry.PropertyChanged(Geometry2Property);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new CombinedGeometry();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Geometry geometry = Geometry1;
			Geometry geometry2 = Geometry2;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hGeometry = ((DUCE.IResource)geometry)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hGeometry2 = ((DUCE.IResource)geometry2)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_COMBINEDGEOMETRY mILCMD_COMBINEDGEOMETRY = default(DUCE.MILCMD_COMBINEDGEOMETRY);
			mILCMD_COMBINEDGEOMETRY.Type = MILCMD.MilCmdCombinedGeometry;
			mILCMD_COMBINEDGEOMETRY.Handle = _duceResource.GetHandle(channel);
			mILCMD_COMBINEDGEOMETRY.hTransform = hTransform;
			mILCMD_COMBINEDGEOMETRY.GeometryCombineMode = GeometryCombineMode;
			mILCMD_COMBINEDGEOMETRY.hGeometry1 = hGeometry;
			mILCMD_COMBINEDGEOMETRY.hGeometry2 = hGeometry2;
			channel.SendCommand((byte*)(&mILCMD_COMBINEDGEOMETRY), sizeof(DUCE.MILCMD_COMBINEDGEOMETRY));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_COMBINEDGEOMETRY))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)Geometry1)?.AddRefOnChannel(channel);
			((DUCE.IResource)Geometry2)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)Geometry1)?.ReleaseOnChannel(channel);
			((DUCE.IResource)Geometry2)?.ReleaseOnChannel(channel);
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static CombinedGeometry()
	{
		s_Geometry1 = Geometry.Empty;
		s_Geometry2 = Geometry.Empty;
		Type typeFromHandle = typeof(CombinedGeometry);
		GeometryCombineModeProperty = Animatable.RegisterProperty("GeometryCombineMode", typeof(GeometryCombineMode), typeFromHandle, GeometryCombineMode.Union, GeometryCombineModePropertyChanged, ValidateEnums.IsGeometryCombineModeValid, isIndependentlyAnimated: false, null);
		Geometry1Property = Animatable.RegisterProperty("Geometry1", typeof(Geometry), typeFromHandle, Geometry.Empty, Geometry1PropertyChanged, null, isIndependentlyAnimated: false, null);
		Geometry2Property = Animatable.RegisterProperty("Geometry2", typeof(Geometry), typeFromHandle, Geometry.Empty, Geometry2PropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
