using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Effects;

/// <summary>Provides a custom bitmap effect. </summary>
public abstract class Effect : Animatable, DUCE.IResource
{
	private class UnitSpaceCoercingGeneralTransform : GeneralTransform
	{
		private readonly Rect _worldBounds;

		private readonly GeneralTransform _innerTransform;

		private GeneralTransform _innerTransformInverse;

		private bool _isInverse;

		private UnitSpaceCoercingGeneralTransform _inverseTransform;

		public override GeneralTransform Inverse
		{
			get
			{
				if (_inverseTransform == null)
				{
					_inverseTransform = (UnitSpaceCoercingGeneralTransform)Clone();
					_inverseTransform._isInverse = !_isInverse;
				}
				return _inverseTransform;
			}
		}

		public UnitSpaceCoercingGeneralTransform(Rect worldBounds, GeneralTransform innerTransform)
		{
			_worldBounds = worldBounds;
			_innerTransform = innerTransform;
			_isInverse = false;
		}

		public override Rect TransformBounds(Rect rect)
		{
			Point result = default(Point);
			Point result2 = default(Point);
			if (!TryTransform(rect.TopLeft, out result) || !TryTransform(rect.BottomRight, out result2))
			{
				return Rect.Empty;
			}
			return new Rect(result, result2);
		}

		public override bool TryTransform(Point inPoint, out Point result)
		{
			bool result2 = false;
			result = default(Point);
			Point? point = WorldToUnit(inPoint, _worldBounds);
			if (point.HasValue && GetCorrectInnerTransform().TryTransform(point.Value, out var result3))
			{
				Point? point2 = UnitToWorld(result3, _worldBounds);
				if (point2.HasValue)
				{
					result = point2.Value;
					result2 = true;
				}
			}
			return result2;
		}

		protected override Freezable CreateInstanceCore()
		{
			return new UnitSpaceCoercingGeneralTransform(_worldBounds, _innerTransform)
			{
				_isInverse = _isInverse
			};
		}

		private GeneralTransform GetCorrectInnerTransform()
		{
			if (_isInverse)
			{
				if (_innerTransformInverse == null)
				{
					_innerTransformInverse = _innerTransform.Inverse;
				}
				return _innerTransformInverse;
			}
			return _innerTransform;
		}
	}

	private Rect _mruWorldBounds = Rect.Empty;

	private GeneralTransform _mruInnerGeneralTransform;

	private GeneralTransform _mruWorldSpaceGeneralTransform;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Brush" /> that, when it is used as an input for an <see cref="T:System.Windows.Media.Effects.Effect" />, causes the bitmap of the <see cref="T:System.Windows.UIElement" /> that the <see cref="T:System.Windows.Media.Effects.Effect" /> is applied to be that input. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that acts as the input.</returns>
	[Browsable(false)]
	public static Brush ImplicitInput { get; private set; }

	/// <summary>When overridden in a derived class, transforms mouse input and coordinate systems through the effect. </summary>
	/// <returns>The transform to apply. The default is the identity transform.</returns>
	protected internal virtual GeneralTransform EffectMapping => Transform.Identity;

	static Effect()
	{
		ImplicitInput = new ImplicitInputBrush();
		ImplicitInput.Freeze();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.Effect" /> class. </summary>
	protected Effect()
	{
	}

	internal abstract Rect GetRenderBounds(Rect contentBounds);

	internal GeneralTransform CoerceToUnitSpaceGeneralTransform(GeneralTransform gt, Rect worldBounds)
	{
		if (gt == Transform.Identity)
		{
			return Transform.Identity;
		}
		if (_mruWorldBounds != worldBounds || _mruInnerGeneralTransform != gt)
		{
			_mruWorldBounds = worldBounds;
			_mruInnerGeneralTransform = gt;
			_mruWorldSpaceGeneralTransform = new UnitSpaceCoercingGeneralTransform(worldBounds, gt);
		}
		return _mruWorldSpaceGeneralTransform;
	}

	private static Point UnitToWorldUnsafe(Point unitPoint, Rect worldBounds)
	{
		return new Point(worldBounds.Left + unitPoint.X * worldBounds.Width, worldBounds.Top + unitPoint.Y * worldBounds.Height);
	}

	internal static Point? UnitToWorld(Point unitPoint, Rect worldBounds)
	{
		if (!worldBounds.IsEmpty)
		{
			return UnitToWorldUnsafe(unitPoint, worldBounds);
		}
		return null;
	}

	internal static Point? WorldToUnit(Point worldPoint, Rect worldBounds)
	{
		if (worldBounds.Width == 0.0 || worldBounds.Height == 0.0)
		{
			return null;
		}
		return new Point((worldPoint.X - worldBounds.Left) / worldBounds.Width, (worldPoint.Y - worldBounds.Top) / worldBounds.Height);
	}

	internal static Rect UnitToWorld(Rect unitRect, Rect worldBounds)
	{
		if (!worldBounds.IsEmpty)
		{
			return new Rect(UnitToWorldUnsafe(unitRect.TopLeft, worldBounds), UnitToWorldUnsafe(unitRect.BottomRight, worldBounds));
		}
		return Rect.Empty;
	}

	internal static Rect? WorldToUnit(Rect worldRect, Rect worldBounds)
	{
		Point? point = WorldToUnit(worldRect.TopLeft, worldBounds);
		Point? point2 = WorldToUnit(worldRect.BottomRight, worldBounds);
		if (!point.HasValue || !point2.HasValue)
		{
			return null;
		}
		return new Rect(point.Value, point2.Value);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values.  </summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new Effect Clone()
	{
		return (Effect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.Effect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied.  </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Effect CloneCurrentValue()
	{
		return (Effect)base.CloneCurrentValue();
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
