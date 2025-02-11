using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Abstract class that describes a 2-D drawing. This class cannot be inherited by your code.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class Drawing : Animatable, IDrawingContent, DUCE.IResource
{
	/// <summary> Gets the axis-aligned bounds of the drawing's contents. </summary>
	/// <returns>The axis-aligned bounds of the drawing's contents.</returns>
	public Rect Bounds
	{
		get
		{
			ReadPreamble();
			return GetBounds();
		}
	}

	internal Drawing()
	{
	}

	internal abstract void WalkCurrentValue(DrawingContextWalker ctx);

	Rect IDrawingContent.GetContentBounds(BoundsDrawingContextWalker ctx)
	{
		WalkCurrentValue(ctx);
		return ctx.Bounds;
	}

	void IDrawingContent.WalkContent(DrawingContextWalker ctx)
	{
		WalkCurrentValue(ctx);
	}

	bool IDrawingContent.HitTestPoint(Point point)
	{
		return DrawingServices.HitTestPoint(this, point);
	}

	IntersectionDetail IDrawingContent.HitTestGeometry(PathGeometry geometry)
	{
		return DrawingServices.HitTestGeometry(this, geometry);
	}

	void IDrawingContent.PropagateChangedHandler(EventHandler handler, bool adding)
	{
		if (!base.IsFrozen)
		{
			if (adding)
			{
				Changed += handler;
			}
			else
			{
				Changed -= handler;
			}
		}
	}

	internal Rect GetBounds()
	{
		BoundsDrawingContextWalker boundsDrawingContextWalker = new BoundsDrawingContextWalker();
		WalkCurrentValue(boundsDrawingContextWalker);
		return boundsDrawingContextWalker.Bounds;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Drawing" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Drawing Clone()
	{
		return (Drawing)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Drawing" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Drawing CloneCurrentValue()
	{
		return (Drawing)base.CloneCurrentValue();
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
