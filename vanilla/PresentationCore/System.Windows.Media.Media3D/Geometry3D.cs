using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.Media3D;

namespace System.Windows.Media.Media3D;

/// <summary>Classes that derive from this abstract base class define 3D geometric shapes. The <see cref="T:System.Windows.Media.Media3D.Geometry3D" /> class of objects can be used for hit-testing and rendering 3D graphic data.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Geometry3D : Animatable, DUCE.IResource
{
	/// <summary> Gets a <see cref="T:System.Windows.Media.Media3D.Rect3D" /> that specifies the axis-aligned bounding box of this <see cref="T:System.Windows.Media.Media3D.Geometry3D" />. </summary>
	/// <returns>Bounding <see cref="T:System.Windows.Media.Media3D.Rect3D" /> for the <see cref="T:System.Windows.Media.Media3D.Geometry3D" />.</returns>
	public abstract Rect3D Bounds { get; }

	internal Geometry3D()
	{
	}

	internal void RayHitTest(RayHitTestParameters rayParams, FaceType facesToHit)
	{
		Rect3D box = Bounds;
		if (!box.IsEmpty)
		{
			rayParams.GetLocalLine(out var origin, out var direction);
			if (LineUtil.ComputeLineBoxIntersection(ref origin, ref direction, ref box, rayParams.IsRay))
			{
				RayHitTestCore(rayParams, facesToHit);
			}
		}
	}

	internal abstract void RayHitTestCore(RayHitTestParameters rayParams, FaceType hitTestableFaces);

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Geometry3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Geometry3D Clone()
	{
		return (Geometry3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Geometry3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Geometry3D CloneCurrentValue()
	{
		return (Geometry3D)base.CloneCurrentValue();
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}
}
