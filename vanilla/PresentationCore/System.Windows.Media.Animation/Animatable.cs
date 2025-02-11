using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Animation;

/// <summary>Abstract class that provides animation support. </summary>
public abstract class Animatable : Freezable, IAnimatable, DUCE.IResource
{
	private static readonly UncommonField<WeakReference> StoredWeakReferenceField = new UncommonField<WeakReference>();

	/// <summary>Gets a value that indicates whether one or more <see cref="T:System.Windows.Media.Animation.AnimationClock" /> objects is associated with any of this object's dependency properties.</summary>
	/// <returns>true if one or more <see cref="T:System.Windows.Media.Animation.AnimationClock" /> objects is associated with any of this object's dependency properties; otherwise, false.</returns>
	public bool HasAnimatedProperties
	{
		get
		{
			VerifyAccess();
			return base.IAnimatable_HasAnimatedProperties;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Animatable" /> class.</summary>
	protected Animatable()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Animatable" />, making deep copies of this object's values. When copying this object's dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values. </summary>
	/// <returns>A modifiable clone of this instance. The returned clone is effectively a deep copy of the current object. The clone's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new Animatable Clone()
	{
		return (Animatable)base.Clone();
	}

	internal void PropertyChanged(DependencyProperty dp)
	{
		if (AnimationStorage.GetStorage(this, dp) is IndependentAnimationStorage independentAnimationStorage)
		{
			independentAnimationStorage.InvalidateResource();
		}
		else
		{
			RegisterForAsyncUpdateResource();
		}
	}

	internal virtual void AddRefOnChannelAnimations(DUCE.Channel channel)
	{
		if (!base.IAnimatable_HasAnimatedProperties)
		{
			return;
		}
		FrugalMap animatedPropertiesMap = AnimationStorage.GetAnimatedPropertiesMap(this);
		for (int i = 0; i < animatedPropertiesMap.Count; i++)
		{
			animatedPropertiesMap.GetKeyValuePair(i, out var _, out var value);
			if (value is DUCE.IResource resource)
			{
				resource.AddRefOnChannel(channel);
			}
		}
	}

	internal virtual void ReleaseOnChannelAnimations(DUCE.Channel channel)
	{
		if (!base.IAnimatable_HasAnimatedProperties)
		{
			return;
		}
		FrugalMap animatedPropertiesMap = AnimationStorage.GetAnimatedPropertiesMap(this);
		for (int i = 0; i < animatedPropertiesMap.Count; i++)
		{
			animatedPropertiesMap.GetKeyValuePair(i, out var _, out var value);
			if (value is DUCE.IResource resource)
			{
				resource.ReleaseOnChannel(channel);
			}
		}
	}

	internal static DependencyProperty RegisterProperty(string name, Type propertyType, Type ownerType, object defaultValue, PropertyChangedCallback changed, ValidateValueCallback validate, bool isIndependentlyAnimated, CoerceValueCallback coerced)
	{
		UIPropertyMetadata uIPropertyMetadata = ((!isIndependentlyAnimated) ? new UIPropertyMetadata(defaultValue) : new IndependentlyAnimatedPropertyMetadata(defaultValue));
		uIPropertyMetadata.PropertyChangedCallback = changed;
		if (coerced != null)
		{
			uIPropertyMetadata.CoerceValueCallback = coerced;
		}
		return DependencyProperty.Register(name, propertyType, ownerType, uIPropertyMetadata, validate);
	}

	internal void AddRefResource(DUCE.IResource resource, DUCE.Channel channel)
	{
		resource?.AddRefOnChannel(channel);
	}

	internal void ReleaseResource(DUCE.IResource resource, DUCE.Channel channel)
	{
		resource?.ReleaseOnChannel(channel);
	}

	/// <summary>Makes this <see cref="T:System.Windows.Media.Animation.Animatable" /> object unmodifiable or determines whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this <see cref="T:System.Windows.Media.Animation.Animatable" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the if this <see cref="T:System.Windows.Media.Animation.Animatable" /> is now unmodifiable, or false if it cannot be made unmodifiable, with the side effect of having begun to change the frozen status of this object.</returns>
	/// <param name="isChecking">true if this method should simply determine whether this instance can be frozen. false if this instance should actually freeze itself when this method is called.</param>
	protected override bool FreezeCore(bool isChecking)
	{
		if (base.IAnimatable_HasAnimatedProperties)
		{
			if (TraceFreezable.IsEnabled)
			{
				TraceFreezable.Trace(TraceEventType.Warning, TraceFreezable.UnableToFreezeAnimatedProperties, this);
			}
			return false;
		}
		return base.FreezeCore(isChecking);
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		return DUCE.ResourceHandle.Null;
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		return DUCE.ResourceHandle.Null;
	}

	int DUCE.IResource.GetChannelCount()
	{
		return 0;
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return null;
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	internal DUCE.ResourceHandle GetAnimationResourceHandle(DependencyProperty dp, DUCE.Channel channel)
	{
		if (channel != null && base.IAnimatable_HasAnimatedProperties)
		{
			return IndependentAnimationStorage.GetResourceHandle(this, dp, channel);
		}
		return DUCE.ResourceHandle.Null;
	}

	internal WeakReference GetWeakReference()
	{
		object obj = StoredWeakReferenceField.GetValue(this);
		if (obj == null)
		{
			obj = new WeakReference(this);
			StoredWeakReferenceField.SetValue(this, (WeakReference)obj);
		}
		return (WeakReference)obj;
	}

	internal bool IsBaseValueDefault(DependencyProperty dp)
	{
		return ReadLocalValue(dp) == DependencyProperty.UnsetValue;
	}

	internal void RegisterForAsyncUpdateResource()
	{
		if (this != null && base.Dispatcher != null && base.Animatable_IsResourceInvalidationNecessary)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			if (!((DUCE.IResource)this).GetHandle(mediaContext.Channel).IsNull)
			{
				mediaContext.ResourcesUpdated += UpdateResource;
				base.Animatable_IsResourceInvalidationNecessary = false;
			}
		}
	}

	internal virtual void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		base.Animatable_IsResourceInvalidationNecessary = true;
	}

	internal void InternalWritePreamble()
	{
		WritePreamble();
	}

	/// <summary>Specifies whether a dependency object should be serialized.</summary>
	/// <returns>true to serialize <paramref name="target" />; otherwise, false. The default is false.</returns>
	/// <param name="target">Represents an object that participates in the dependency property system.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool ShouldSerializeStoredWeakReference(DependencyObject target)
	{
		return false;
	}

	/// <summary>Applies an <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to the specified <see cref="T:System.Windows.DependencyProperty" />. If the property is already animated, the <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" /> handoff behavior is used.</summary>
	/// <param name="dp">The property to animate.</param>
	/// <param name="clock">The clock with which to animate the specified property. If <paramref name="clock" /> is null, all animations will be removed from the specified property (but not stopped). </param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
	{
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Applies an <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to the specified <see cref="T:System.Windows.DependencyProperty" />. If the property is already animated, the specified <see cref="T:System.Windows.Media.Animation.HandoffBehavior" /> is used.</summary>
	/// <param name="dp">The property to animate.</param>
	/// <param name="clock">The clock with which to animate the specified property. If <paramref name="handoffBehavior" /> is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" /> and <paramref name="clock" /> is null, all animations will be removed from the specified property (but not stopped). If <paramref name="handoffBehavior" /> is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.Compose" /> and clock is null, this method has no effect.</param>
	/// <param name="handoffBehavior">A value that specifies how the new animation should interact with any current animations already affecting the property value.</param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (clock != null && !AnimationStorage.IsAnimationValid(dp, clock.Timeline))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, clock.Timeline.GetType(), dp.Name, dp.PropertyType), "clock");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.ApplyAnimationClock(this, dp, clock, handoffBehavior);
	}

	/// <summary>Applies an animation to the specified <see cref="T:System.Windows.DependencyProperty" />. The animation is started when the next frame is rendered. If the specified property is already animated, the <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" /> handoff behavior is used.</summary>
	/// <param name="dp">The property to animate.</param>
	/// <param name="animation">The animation used to animate the specified property.If the animation's <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> is null, any current animations will be removed and the current value of the property will be held.If <paramref name="animation" /> is null, all animations will be removed from the property and the property value will revert back to its base value.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
	{
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Applies an animation to the specified <see cref="T:System.Windows.DependencyProperty" />. The animation is started when the next frame is rendered. If the specified property is already animated, the specified <see cref="T:System.Windows.Media.Animation.HandoffBehavior" /> is used. </summary>
	/// <param name="dp">The property to animate.</param>
	/// <param name="animation">The animation used to animate the specified property.If <paramref name="handoffBehavior" /> is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" /> and the animation's <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> is null, any current animations will be removed and the current value of the property will be held. If <paramref name="handoffBehavior" /> is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" /> and <paramref name="animation" /> is a null reference, all animations will be removed from the property and the property value will revert back to its base value.If <paramref name="handoffBehavior" /> is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.Compose" />, this method will have no effect if the animation or its <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> is null.</param>
	/// <param name="handoffBehavior">A value that specifies how the new animation should interact with any current animations already affecting the property value.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (animation != null && !AnimationStorage.IsAnimationValid(dp, animation))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, animation.GetType(), dp.Name, dp.PropertyType), "animation");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.BeginAnimation(this, dp, animation, handoffBehavior);
	}

	/// <summary>Returns the non-animated value of the specified <see cref="T:System.Windows.DependencyProperty" />.</summary>
	/// <returns>The value that would be returned if the specified property were not animated. </returns>
	/// <param name="dp">Identifies the property whose base (non-animated) value should be retrieved. </param>
	public object GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return GetValueEntry(LookupEntry(dp.GlobalIndex), dp, null, RequestFlags.AnimationBaseValue).Value;
	}

	internal sealed override void EvaluateAnimatedValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry entry)
	{
		if (base.IAnimatable_HasAnimatedProperties)
		{
			AnimationStorage.GetStorage(this, dp)?.EvaluateAnimatedValue(metadata, ref entry);
		}
	}
}
