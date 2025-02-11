using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Defines a geometric shape, described using a <see cref="T:System.Windows.Media.StreamGeometryContext" />. This geometry is light-weight alternative to <see cref="T:System.Windows.Media.PathGeometry" />: it does not support data binding, animation, or modification.</summary>
[TypeConverter(typeof(GeometryConverter))]
public sealed class StreamGeometry : Geometry
{
	private byte[] _data;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.StreamGeometry.FillRule" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.StreamGeometry.FillRule" /> dependency property.</returns>
	public static readonly DependencyProperty FillRuleProperty;

	internal const FillRule c_FillRule = FillRule.EvenOdd;

	/// <summary>Gets a <see cref="T:System.Windows.Rect" /> that is exactly large enough to contain this <see cref="T:System.Windows.Media.StreamGeometry" />.</summary>
	/// <returns>The bounding box of this <see cref="T:System.Windows.Media.StreamGeometry" />. </returns>
	public override Rect Bounds
	{
		get
		{
			ReadPreamble();
			if (IsEmpty())
			{
				return Rect.Empty;
			}
			MilRectD bounds = default(MilRectD);
			if (!AreBoundsValid(ref bounds))
			{
				bounds = PathGeometry.GetPathBoundsAsRB(GetPathGeometryData(), null, Matrix.Identity, Geometry.StandardFlatteningTolerance, ToleranceType.Absolute, skipHollows: false);
				CacheBounds(ref bounds);
			}
			return bounds.AsRect;
		}
	}

	/// <summary>Gets or sets a value that determines how the intersecting areas contained in this <see cref="T:System.Windows.Media.StreamGeometry" /> are combined.  </summary>
	/// <returns>Indicates how the intersecting areas of this <see cref="T:System.Windows.Media.StreamGeometry" /> are combined.  The default value is EvenOdd.</returns>
	public FillRule FillRule
	{
		get
		{
			return (FillRule)GetValue(FillRuleProperty);
		}
		set
		{
			SetValueInternal(FillRuleProperty, FillRuleBoxes.Box(value));
		}
	}

	/// <summary>Initializes a new, empty instance of the <see cref="T:System.Windows.Media.StreamGeometry" /> class.</summary>
	public StreamGeometry()
	{
	}

	/// <summary>Opens a <see cref="T:System.Windows.Media.StreamGeometryContext" /> that can be used to describe this <see cref="T:System.Windows.Media.StreamGeometry" /> object's contents.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.StreamGeometryContext" /> that can be used to describe this <see cref="T:System.Windows.Media.StreamGeometry" /> object's contents.</returns>
	public StreamGeometryContext Open()
	{
		WritePreamble();
		return new StreamGeometryCallbackContext(this);
	}

	/// <summary>Removes all geometric information from this <see cref="T:System.Windows.Media.StreamGeometry" />.</summary>
	public void Clear()
	{
		WritePreamble();
		_data = null;
		SetDirty();
		RegisterForAsyncUpdateResource();
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.StreamGeometry" /> describes a geometric shape.</summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.StreamGeometry" /> describes a geometry shape; otherwise, false.</returns>
	public unsafe override bool IsEmpty()
	{
		ReadPreamble();
		if (_data == null || _data.Length == 0)
		{
			return true;
		}
		Invariant.Assert(_data != null && _data.Length >= sizeof(MIL_PATHGEOMETRY));
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			return ptr->FigureCount == 0;
		}
	}

	private unsafe bool AreBoundsValid(ref MilRectD bounds)
	{
		if (IsEmpty())
		{
			return false;
		}
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			bool num = (ptr->Flags & MilPathGeometryFlags.BoundsValid) != 0;
			if (num)
			{
				bounds = ptr->Bounds;
			}
			return num;
		}
	}

	private unsafe void CacheBounds(ref MilRectD bounds)
	{
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			ptr->Flags |= MilPathGeometryFlags.BoundsValid;
			ptr->Bounds = bounds;
		}
	}

	internal unsafe void SetDirty()
	{
		if (!IsEmpty())
		{
			fixed (byte* data = _data)
			{
				MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
				ptr->Flags &= ~MilPathGeometryFlags.BoundsValid;
			}
		}
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.StreamGeometry" /> contains a curved segment.</summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.StreamGeometry" /> object has a curved segment; otherwise, false.</returns>
	public unsafe override bool MayHaveCurves()
	{
		if (IsEmpty())
		{
			return false;
		}
		Invariant.Assert(_data != null && _data.Length >= sizeof(MIL_PATHGEOMETRY));
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			return (ptr->Flags & MilPathGeometryFlags.HasCurves) != 0;
		}
	}

	internal unsafe bool HasHollows()
	{
		if (IsEmpty())
		{
			return false;
		}
		Invariant.Assert(_data != null && _data.Length >= sizeof(MIL_PATHGEOMETRY));
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			return (ptr->Flags & MilPathGeometryFlags.HasHollows) != 0;
		}
	}

	internal unsafe bool HasGaps()
	{
		if (IsEmpty())
		{
			return false;
		}
		Invariant.Assert(_data != null && _data.Length >= sizeof(MIL_PATHGEOMETRY));
		fixed (byte* data = _data)
		{
			MIL_PATHGEOMETRY* ptr = (MIL_PATHGEOMETRY*)data;
			return (ptr->Flags & MilPathGeometryFlags.HasGaps) != 0;
		}
	}

	internal void Close(byte[] _buffer)
	{
		SetDirty();
		_data = _buffer;
		RegisterForAsyncUpdateResource();
	}

	protected override void OnChanged()
	{
		SetDirty();
		base.OnChanged();
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		PathStreamGeometryContext pathStreamGeometryContext = new PathStreamGeometryContext(FillRule, base.Transform);
		PathGeometry.ParsePathGeometryData(GetPathGeometryData(), pathStreamGeometryContext);
		return pathStreamGeometryContext.GetPathGeometry();
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		return GetAsPathGeometry()?.GetTransformedFigureCollection(transform);
	}

	internal override bool CanSerializeToString()
	{
		Transform transform = base.Transform;
		if ((transform == null || transform.IsIdentity) && !HasHollows())
		{
			return !HasGaps();
		}
		return false;
	}

	internal override string ConvertToString(string format, IFormatProvider provider)
	{
		return GetAsPathGeometry().ConvertToString(format, provider);
	}

	private void InvalidateResourceFigures(object sender, EventArgs args)
	{
		SetDirty();
		RegisterForAsyncUpdateResource();
	}

	internal override PathGeometryData GetPathGeometryData()
	{
		if (IsEmpty())
		{
			return Geometry.GetEmptyPathGeometryData();
		}
		PathGeometryData result = default(PathGeometryData);
		result.FillRule = FillRule;
		result.Matrix = CompositionResourceManager.TransformToMilMatrix3x2D(base.Transform);
		result.SerializedData = _data;
		return result;
	}

	internal override void TransformPropertyChangedHook(DependencyPropertyChangedEventArgs e)
	{
		SetDirty();
	}

	private unsafe int GetFigureSize(byte* pbPathData)
	{
		if (pbPathData != null)
		{
			return (int)((MIL_PATHGEOMETRY*)pbPathData)->Size;
		}
		return 0;
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		checked
		{
			if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
			{
				Transform transform = base.Transform;
				DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
				DUCE.MILCMD_PATHGEOMETRY mILCMD_PATHGEOMETRY = default(DUCE.MILCMD_PATHGEOMETRY);
				mILCMD_PATHGEOMETRY.Type = MILCMD.MilCmdPathGeometry;
				mILCMD_PATHGEOMETRY.Handle = _duceResource.GetHandle(channel);
				mILCMD_PATHGEOMETRY.hTransform = hTransform;
				mILCMD_PATHGEOMETRY.FillRule = FillRule;
				fixed (byte* ptr = ((_data == null) ? Geometry.GetEmptyPathGeometryData().SerializedData : _data))
				{
					mILCMD_PATHGEOMETRY.FiguresSize = (uint)GetFigureSize(ptr);
					channel.BeginCommand(unchecked((byte*)(&mILCMD_PATHGEOMETRY)), sizeof(DUCE.MILCMD_PATHGEOMETRY), (int)mILCMD_PATHGEOMETRY.FiguresSize);
					channel.AppendCommandData(ptr, (int)mILCMD_PATHGEOMETRY.FiguresSize);
				}
				channel.EndCommand();
			}
			base.UpdateResource(channel, skipOnChannelCheck);
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_PATHGEOMETRY))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
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

	protected override void CloneCore(Freezable source)
	{
		base.CloneCore(source);
		StreamGeometry streamGeometry = (StreamGeometry)source;
		if (streamGeometry._data != null && streamGeometry._data.Length != 0)
		{
			_data = new byte[streamGeometry._data.Length];
			streamGeometry._data.CopyTo(_data, 0);
		}
	}

	protected override void CloneCurrentValueCore(Freezable source)
	{
		base.CloneCurrentValueCore(source);
		StreamGeometry streamGeometry = (StreamGeometry)source;
		if (streamGeometry._data != null && streamGeometry._data.Length != 0)
		{
			_data = new byte[streamGeometry._data.Length];
			streamGeometry._data.CopyTo(_data, 0);
		}
	}

	protected override void GetAsFrozenCore(Freezable source)
	{
		base.GetAsFrozenCore(source);
		StreamGeometry streamGeometry = (StreamGeometry)source;
		if (streamGeometry._data != null && streamGeometry._data.Length != 0)
		{
			_data = new byte[streamGeometry._data.Length];
			streamGeometry._data.CopyTo(_data, 0);
		}
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		base.GetCurrentValueAsFrozenCore(source);
		StreamGeometry streamGeometry = (StreamGeometry)source;
		if (streamGeometry._data != null && streamGeometry._data.Length != 0)
		{
			_data = new byte[streamGeometry._data.Length];
			streamGeometry._data.CopyTo(_data, 0);
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.StreamGeometry" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new StreamGeometry Clone()
	{
		return (StreamGeometry)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.StreamGeometry" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new StreamGeometry CloneCurrentValue()
	{
		return (StreamGeometry)base.CloneCurrentValue();
	}

	private static void FillRulePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((StreamGeometry)d).PropertyChanged(FillRuleProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new StreamGeometry();
	}

	static StreamGeometry()
	{
		Type typeFromHandle = typeof(StreamGeometry);
		FillRuleProperty = Animatable.RegisterProperty("FillRule", typeof(FillRule), typeFromHandle, FillRule.EvenOdd, FillRulePropertyChanged, ValidateEnums.IsFillRuleValid, isIndependentlyAnimated: false, null);
	}
}
