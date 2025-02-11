using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary>Renders a <see cref="T:System.Windows.Media.Media3D.Geometry3D" /> with the specified <see cref="T:System.Windows.Media.Media3D.Material" />. </summary>
public sealed class GeometryModel3D : Model3D
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.Geometry" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.Geometry" /> dependency property.</returns>
	public static readonly DependencyProperty GeometryProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.Material" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.Material" /> dependency property.</returns>
	public static readonly DependencyProperty MaterialProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.BackMaterial" /> dependency property.</summary>
	/// <returns>The idenfitier for the <see cref="P:System.Windows.Media.Media3D.GeometryModel3D.BackMaterial" /> dependency property.</returns>
	public static readonly DependencyProperty BackMaterialProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Geometry3D" /> that describes the shape of this <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Geometry3D" /> that comprises the model.</returns>
	public Geometry3D Geometry
	{
		get
		{
			return (Geometry3D)GetValue(GeometryProperty);
		}
		set
		{
			SetValueInternal(GeometryProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Material" /> used to render the front of this <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Material" /> applied to the <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />.</returns>
	public Material Material
	{
		get
		{
			return (Material)GetValue(MaterialProperty);
		}
		set
		{
			SetValueInternal(MaterialProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Material" /> used to render the back of this <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.Material" /> applied to the back of the <see cref="T:System.Windows.Media.Media3D.Model3D" />. The default value is null.</returns>
	public Material BackMaterial
	{
		get
		{
			return (Material)GetValue(BackMaterialProperty);
		}
		set
		{
			SetValueInternal(BackMaterialProperty, value);
		}
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />. </summary>
	public GeometryModel3D()
	{
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" /> with the specified Geometry3D and Material. </summary>
	/// <param name="geometry">Geometry of the new mesh primitive.</param>
	/// <param name="material">Material of the new mesh primitive.</param>
	public GeometryModel3D(Geometry3D geometry, Material material)
	{
		Geometry = geometry;
		Material = material;
	}

	internal override Rect3D CalculateSubgraphBoundsInnerSpace()
	{
		return Geometry?.Bounds ?? Rect3D.Empty;
	}

	internal override void RayHitTestCore(RayHitTestParameters rayParams)
	{
		Geometry3D geometry = Geometry;
		if (geometry != null)
		{
			rayParams.CurrentModel = this;
			FaceType faceType = FaceType.None;
			if (Material != null)
			{
				faceType |= FaceType.Front;
			}
			if (BackMaterial != null)
			{
				faceType |= FaceType.Back;
			}
			if (faceType != 0)
			{
				geometry.RayHitTest(rayParams, faceType);
			}
		}
	}

	internal void MaterialPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
	}

	internal void BackMaterialPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		MaterialPropertyChangedHook(e);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryModel3D Clone()
	{
		return (GeometryModel3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeometryModel3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryModel3D CloneCurrentValue()
	{
		return (GeometryModel3D)base.CloneCurrentValue();
	}

	private static void GeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GeometryModel3D geometryModel3D = (GeometryModel3D)d;
		Geometry3D resource = (Geometry3D)e.OldValue;
		Geometry3D resource2 = (Geometry3D)e.NewValue;
		if (geometryModel3D.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryModel3D;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryModel3D.ReleaseResource(resource, channel);
					geometryModel3D.AddRefResource(resource2, channel);
				}
			}
		}
		geometryModel3D.PropertyChanged(GeometryProperty);
	}

	private static void MaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GeometryModel3D geometryModel3D = (GeometryModel3D)d;
		geometryModel3D.MaterialPropertyChangedHook(e);
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Material resource = (Material)e.OldValue;
		Material resource2 = (Material)e.NewValue;
		if (geometryModel3D.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryModel3D;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryModel3D.ReleaseResource(resource, channel);
					geometryModel3D.AddRefResource(resource2, channel);
				}
			}
		}
		geometryModel3D.PropertyChanged(MaterialProperty);
	}

	private static void BackMaterialPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GeometryModel3D geometryModel3D = (GeometryModel3D)d;
		geometryModel3D.BackMaterialPropertyChangedHook(e);
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Material resource = (Material)e.OldValue;
		Material resource2 = (Material)e.NewValue;
		if (geometryModel3D.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryModel3D;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryModel3D.ReleaseResource(resource, channel);
					geometryModel3D.AddRefResource(resource2, channel);
				}
			}
		}
		geometryModel3D.PropertyChanged(BackMaterialProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeometryModel3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			Geometry3D geometry = Geometry;
			Material material = Material;
			Material backMaterial = BackMaterial;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hgeometry = ((DUCE.IResource)geometry)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hmaterial = ((DUCE.IResource)material)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hbackMaterial = ((DUCE.IResource)backMaterial)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_GEOMETRYMODEL3D mILCMD_GEOMETRYMODEL3D = default(DUCE.MILCMD_GEOMETRYMODEL3D);
			mILCMD_GEOMETRYMODEL3D.Type = MILCMD.MilCmdGeometryModel3D;
			mILCMD_GEOMETRYMODEL3D.Handle = _duceResource.GetHandle(channel);
			mILCMD_GEOMETRYMODEL3D.htransform = htransform;
			mILCMD_GEOMETRYMODEL3D.hgeometry = hgeometry;
			mILCMD_GEOMETRYMODEL3D.hmaterial = hmaterial;
			mILCMD_GEOMETRYMODEL3D.hbackMaterial = hbackMaterial;
			channel.SendCommand((byte*)(&mILCMD_GEOMETRYMODEL3D), sizeof(DUCE.MILCMD_GEOMETRYMODEL3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GEOMETRYMODEL3D))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)Geometry)?.AddRefOnChannel(channel);
			((DUCE.IResource)Material)?.AddRefOnChannel(channel);
			((DUCE.IResource)BackMaterial)?.AddRefOnChannel(channel);
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
			((DUCE.IResource)Geometry)?.ReleaseOnChannel(channel);
			((DUCE.IResource)Material)?.ReleaseOnChannel(channel);
			((DUCE.IResource)BackMaterial)?.ReleaseOnChannel(channel);
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

	static GeometryModel3D()
	{
		Type typeFromHandle = typeof(GeometryModel3D);
		GeometryProperty = Animatable.RegisterProperty("Geometry", typeof(Geometry3D), typeFromHandle, null, GeometryPropertyChanged, null, isIndependentlyAnimated: false, null);
		MaterialProperty = Animatable.RegisterProperty("Material", typeof(Material), typeFromHandle, null, MaterialPropertyChanged, null, isIndependentlyAnimated: false, null);
		BackMaterialProperty = Animatable.RegisterProperty("BackMaterial", typeof(Material), typeFromHandle, null, BackMaterialPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
