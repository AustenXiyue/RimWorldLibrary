using System.Collections.Generic;
using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media.Animation;

internal class AnimationStorage
{
	private static readonly UncommonField<FrugalMap> AnimatedPropertyMapField = new UncommonField<FrugalMap>();

	protected WeakReference _dependencyObject;

	protected DependencyProperty _dependencyProperty;

	protected FrugalObjectList<AnimationClock> _animationClocks;

	private SortedList<long, AnimationLayer> _propertyTriggerLayers;

	private EventHandler _currentTimeInvalidatedHandler;

	private EventHandler _removeRequestedHandler;

	private object _snapshotValue = DependencyProperty.UnsetValue;

	private bool _hasStickySnapshotValue;

	private bool _hadValidationError;

	internal object _baseValue = DependencyProperty.UnsetValue;

	internal bool IsEmpty
	{
		get
		{
			if (_animationClocks == null && _propertyTriggerLayers == null)
			{
				return _snapshotValue == DependencyProperty.UnsetValue;
			}
			return false;
		}
	}

	protected AnimationStorage()
	{
		_currentTimeInvalidatedHandler = OnCurrentTimeInvalidated;
		_removeRequestedHandler = OnRemoveRequested;
	}

	internal void AttachAnimationClock(AnimationClock animationClock, EventHandler removeRequestedHandler)
	{
		animationClock.CurrentTimeInvalidated += _currentTimeInvalidatedHandler;
		if (animationClock.HasControllableRoot)
		{
			animationClock.RemoveRequested += removeRequestedHandler;
		}
	}

	internal void DetachAnimationClock(AnimationClock animationClock, EventHandler removeRequestedHandler)
	{
		animationClock.CurrentTimeInvalidated -= _currentTimeInvalidatedHandler;
		if (animationClock.HasControllableRoot)
		{
			animationClock.RemoveRequested -= removeRequestedHandler;
		}
	}

	internal void Initialize(DependencyObject d, DependencyProperty dp)
	{
		if (d is Animatable animatable)
		{
			_dependencyObject = animatable.GetWeakReference();
		}
		else
		{
			_dependencyObject = new WeakReference(d);
		}
		_dependencyProperty = dp;
	}

	internal void RemoveLayer(AnimationLayer layer)
	{
		int index = _propertyTriggerLayers.IndexOfValue(layer);
		_propertyTriggerLayers.RemoveAt(index);
		if (_propertyTriggerLayers.Count == 0)
		{
			_propertyTriggerLayers = null;
		}
	}

	internal void WritePostscript()
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject == null)
		{
			return;
		}
		FrugalMap value = AnimatedPropertyMapField.GetValue(dependencyObject);
		if (value.Count == 0 || value[_dependencyProperty.GlobalIndex] == DependencyProperty.UnsetValue)
		{
			if (!IsEmpty)
			{
				value[_dependencyProperty.GlobalIndex] = this;
				AnimatedPropertyMapField.SetValue(dependencyObject, value);
				if (value.Count == 1)
				{
					dependencyObject.IAnimatable_HasAnimatedProperties = true;
				}
				if (dependencyObject is Animatable animatable)
				{
					animatable.RegisterForAsyncUpdateResource();
				}
				if (this is DUCE.IResource resource && dependencyObject is DUCE.IResource resource2)
				{
					using (CompositionEngineLock.Acquire())
					{
						int channelCount = resource2.GetChannelCount();
						for (int i = 0; i < channelCount; i++)
						{
							DUCE.Channel channel = resource2.GetChannel(i);
							if (!resource2.GetHandle(channel).IsNull)
							{
								resource.AddRefOnChannel(channel);
							}
						}
					}
				}
			}
		}
		else if (IsEmpty)
		{
			if (this is DUCE.IResource resource3 && dependencyObject is DUCE.IResource resource4)
			{
				using (CompositionEngineLock.Acquire())
				{
					int channelCount2 = resource4.GetChannelCount();
					for (int j = 0; j < channelCount2; j++)
					{
						DUCE.Channel channel2 = resource4.GetChannel(j);
						if (!resource4.GetHandle(channel2).IsNull)
						{
							resource3.ReleaseOnChannel(channel2);
						}
					}
				}
			}
			if (dependencyObject is Animatable animatable2)
			{
				animatable2.RegisterForAsyncUpdateResource();
			}
			value[_dependencyProperty.GlobalIndex] = DependencyProperty.UnsetValue;
			if (value.Count == 0)
			{
				AnimatedPropertyMapField.ClearValue(dependencyObject);
				dependencyObject.IAnimatable_HasAnimatedProperties = false;
			}
			else
			{
				AnimatedPropertyMapField.SetValue(dependencyObject, value);
			}
			if (_baseValue != DependencyProperty.UnsetValue)
			{
				dependencyObject.SetValue(_dependencyProperty, _baseValue);
			}
		}
		dependencyObject.InvalidateProperty(_dependencyProperty);
	}

	internal void EvaluateAnimatedValue(PropertyMetadata metadata, ref EffectiveValueEntry entry)
	{
		DependencyObject dependencyObject = (DependencyObject)_dependencyObject.Target;
		if (dependencyObject != null)
		{
			object value = entry.GetFlattenedEntry(RequestFlags.FullyResolved).Value;
			if (entry.IsDeferredReference)
			{
				value = ((DeferredReference)value).GetValue(entry.BaseValueSourceInternal);
				entry.SetAnimationBaseValue(value);
			}
			object currentPropertyValue = GetCurrentPropertyValue(this, dependencyObject, _dependencyProperty, metadata, value);
			if (!_dependencyProperty.IsValidValueInternal(currentPropertyValue))
			{
				throw new InvalidOperationException(SR.Format(SR.Animation_CalculatedValueIsInvalidForProperty, _dependencyProperty.Name, null));
			}
			entry.SetAnimatedValue(currentPropertyValue, value);
		}
	}

	private void OnCurrentTimeInvalidated(object sender, EventArgs args)
	{
		object target = _dependencyObject.Target;
		if (target == null)
		{
			DetachAnimationClock((AnimationClock)sender, _removeRequestedHandler);
			return;
		}
		try
		{
			DependencyObject dependencyObject = (DependencyObject)target;
			EffectiveValueEntry valueEntry = dependencyObject.GetValueEntry(dependencyObject.LookupEntry(_dependencyProperty.GlobalIndex), _dependencyProperty, null, RequestFlags.RawEntry);
			EffectiveValueEntry newEntry;
			object obj;
			if (!valueEntry.HasModifiers)
			{
				newEntry = valueEntry;
				obj = newEntry.Value;
				if (newEntry.IsDeferredReference)
				{
					obj = (newEntry.Value = ((DeferredReference)obj).GetValue(newEntry.BaseValueSourceInternal));
				}
			}
			else
			{
				newEntry = default(EffectiveValueEntry);
				newEntry.BaseValueSourceInternal = valueEntry.BaseValueSourceInternal;
				newEntry.PropertyIndex = valueEntry.PropertyIndex;
				newEntry.HasExpressionMarker = valueEntry.HasExpressionMarker;
				obj = valueEntry.ModifiedValue.BaseValue;
				if (valueEntry.IsDeferredReference && obj is DeferredReference deferredReference)
				{
					obj = deferredReference.GetValue(newEntry.BaseValueSourceInternal);
				}
				newEntry.Value = obj;
				if (valueEntry.IsExpression)
				{
					obj = valueEntry.ModifiedValue.ExpressionValue;
					if (valueEntry.IsDeferredReference && obj is DeferredReference deferredReference2)
					{
						obj = deferredReference2.GetValue(newEntry.BaseValueSourceInternal);
					}
					newEntry.SetExpressionValue(obj, newEntry.Value);
				}
			}
			PropertyMetadata metadata = _dependencyProperty.GetMetadata(dependencyObject.DependencyObjectType);
			object currentPropertyValue = GetCurrentPropertyValue(this, dependencyObject, _dependencyProperty, metadata, obj);
			if (_dependencyProperty.IsValidValueInternal(currentPropertyValue))
			{
				newEntry.SetAnimatedValue(currentPropertyValue, obj);
				dependencyObject.UpdateEffectiveValue(dependencyObject.LookupEntry(_dependencyProperty.GlobalIndex), _dependencyProperty, metadata, valueEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
				if (_hadValidationError && TraceAnimation.IsEnabled)
				{
					TraceAnimation.TraceActivityItem(TraceAnimation.AnimateStorageValidationNoLongerFailing, this, currentPropertyValue, target, _dependencyProperty);
					_hadValidationError = false;
				}
			}
			else if (!_hadValidationError)
			{
				if (TraceAnimation.IsEnabled)
				{
					TraceAnimation.TraceActivityItem(TraceAnimation.AnimateStorageValidationFailed, this, currentPropertyValue, target, _dependencyProperty);
				}
				_hadValidationError = true;
			}
		}
		catch (Exception innerException)
		{
			throw new AnimationException((AnimationClock)sender, _dependencyProperty, (IAnimatable)target, SR.Format(SR.Animation_Exception, _dependencyProperty.Name, target.GetType().FullName, ((AnimationClock)sender).Timeline.GetType().FullName), innerException);
		}
	}

	private void OnRemoveRequested(object sender, EventArgs args)
	{
		AnimationClock animationClock = (AnimationClock)sender;
		int num = _animationClocks.IndexOf(animationClock);
		_animationClocks.RemoveAt(num);
		if (_hasStickySnapshotValue && num == 0)
		{
			_hasStickySnapshotValue = false;
			animationClock.CurrentStateInvalidated -= OnCurrentStateInvalidated;
		}
		if (_animationClocks.Count == 0)
		{
			_animationClocks = null;
			_snapshotValue = DependencyProperty.UnsetValue;
		}
		DetachAnimationClock(animationClock, _removeRequestedHandler);
		WritePostscript();
	}

	private void OnCurrentStateInvalidated(object sender, EventArgs args)
	{
		_hasStickySnapshotValue = false;
		((AnimationClock)sender).CurrentStateInvalidated -= OnCurrentStateInvalidated;
	}

	private void ClearAnimations()
	{
		if (_animationClocks != null)
		{
			for (int i = 0; i < _animationClocks.Count; i++)
			{
				DetachAnimationClock(_animationClocks[i], _removeRequestedHandler);
			}
			_animationClocks = null;
		}
	}

	internal static void ApplyAnimationClock(DependencyObject d, DependencyProperty dp, AnimationClock animationClock, HandoffBehavior handoffBehavior)
	{
		if (animationClock == null)
		{
			BeginAnimation(d, dp, null, handoffBehavior);
			return;
		}
		ApplyAnimationClocks(d, dp, new AnimationClock[1] { animationClock }, handoffBehavior);
	}

	[FriendAccessAllowed]
	internal static void ApplyAnimationClocks(DependencyObject d, DependencyProperty dp, IList<AnimationClock> animationClocks, HandoffBehavior handoffBehavior)
	{
		AnimationStorage animationStorage = GetStorage(d, dp);
		if (handoffBehavior == HandoffBehavior.SnapshotAndReplace || animationStorage == null || animationStorage._animationClocks == null)
		{
			if (animationStorage != null)
			{
				EventHandler value = animationStorage.OnCurrentStateInvalidated;
				if (animationStorage._hasStickySnapshotValue)
				{
					animationStorage._animationClocks[0].CurrentStateInvalidated -= value;
				}
				else
				{
					animationStorage._snapshotValue = d.GetValue(dp);
				}
				if (animationClocks[0].CurrentState == ClockState.Stopped)
				{
					animationStorage._hasStickySnapshotValue = true;
					animationClocks[0].CurrentStateInvalidated += animationStorage.OnCurrentStateInvalidated;
				}
				else
				{
					animationStorage._hasStickySnapshotValue = false;
				}
				animationStorage.ClearAnimations();
			}
			else
			{
				animationStorage = CreateStorage(d, dp);
			}
			animationStorage._animationClocks = new FrugalObjectList<AnimationClock>(animationClocks.Count);
			for (int i = 0; i < animationClocks.Count; i++)
			{
				animationStorage._animationClocks.Add(animationClocks[i]);
				animationStorage.AttachAnimationClock(animationClocks[i], animationStorage._removeRequestedHandler);
			}
		}
		else
		{
			FrugalObjectList<AnimationClock> frugalObjectList = new FrugalObjectList<AnimationClock>(animationStorage._animationClocks.Count + animationClocks.Count);
			for (int j = 0; j < animationStorage._animationClocks.Count; j++)
			{
				frugalObjectList.Add(animationStorage._animationClocks[j]);
			}
			animationStorage._animationClocks = frugalObjectList;
			for (int k = 0; k < animationClocks.Count; k++)
			{
				frugalObjectList.Add(animationClocks[k]);
				animationStorage.AttachAnimationClock(animationClocks[k], animationStorage._removeRequestedHandler);
			}
		}
		animationStorage.WritePostscript();
	}

	[FriendAccessAllowed]
	internal static void ApplyAnimationClocksToLayer(DependencyObject d, DependencyProperty dp, IList<AnimationClock> animationClocks, HandoffBehavior handoffBehavior, long propertyTriggerLayerIndex)
	{
		if (propertyTriggerLayerIndex == 1)
		{
			ApplyAnimationClocks(d, dp, animationClocks, handoffBehavior);
			return;
		}
		AnimationStorage animationStorage = GetStorage(d, dp);
		if (animationStorage == null)
		{
			animationStorage = CreateStorage(d, dp);
		}
		SortedList<long, AnimationLayer> sortedList = animationStorage._propertyTriggerLayers;
		if (sortedList == null)
		{
			sortedList = (animationStorage._propertyTriggerLayers = new SortedList<long, AnimationLayer>(1));
		}
		AnimationLayer animationLayer2 = (sortedList.ContainsKey(propertyTriggerLayerIndex) ? sortedList[propertyTriggerLayerIndex] : (sortedList[propertyTriggerLayerIndex] = new AnimationLayer(animationStorage)));
		object defaultDestinationValue = DependencyProperty.UnsetValue;
		if (handoffBehavior == HandoffBehavior.SnapshotAndReplace)
		{
			defaultDestinationValue = ((IAnimatable)d).GetAnimationBaseValue(dp);
			int count = sortedList.Count;
			if (count > 1)
			{
				IList<long> keys = sortedList.Keys;
				for (int i = 0; i < count && keys[i] < propertyTriggerLayerIndex; i++)
				{
					sortedList.TryGetValue(keys[i], out var value);
					defaultDestinationValue = value.GetCurrentValue(defaultDestinationValue);
				}
			}
		}
		animationLayer2.ApplyAnimationClocks(animationClocks, handoffBehavior, defaultDestinationValue);
		animationStorage.WritePostscript();
	}

	internal static void BeginAnimation(DependencyObject d, DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
	{
		AnimationStorage storage = GetStorage(d, dp);
		if (animation == null)
		{
			if (storage == null || handoffBehavior == HandoffBehavior.Compose)
			{
				return;
			}
			if (storage._hasStickySnapshotValue)
			{
				storage._hasStickySnapshotValue = false;
				storage._animationClocks[0].CurrentStateInvalidated -= storage.OnCurrentStateInvalidated;
			}
			storage._snapshotValue = DependencyProperty.UnsetValue;
			storage.ClearAnimations();
		}
		else
		{
			if (animation.BeginTime.HasValue)
			{
				AnimationClock animationClock = animation.CreateClock();
				ApplyAnimationClocks(d, dp, new AnimationClock[1] { animationClock }, handoffBehavior);
				return;
			}
			if (storage == null)
			{
				return;
			}
			if (handoffBehavior == HandoffBehavior.SnapshotAndReplace)
			{
				if (storage._hasStickySnapshotValue)
				{
					storage._hasStickySnapshotValue = false;
					storage._animationClocks[0].CurrentStateInvalidated -= storage.OnCurrentStateInvalidated;
				}
				else
				{
					storage._snapshotValue = d.GetValue(dp);
				}
				storage.ClearAnimations();
			}
		}
		storage.WritePostscript();
	}

	internal static AnimationStorage EnsureStorage(DependencyObject d, DependencyProperty dp)
	{
		object obj = AnimatedPropertyMapField.GetValue(d)[dp.GlobalIndex];
		if (obj == DependencyProperty.UnsetValue)
		{
			return CreateStorage(d, dp);
		}
		return (AnimationStorage)obj;
	}

	internal static object GetCurrentPropertyValue(AnimationStorage storage, DependencyObject d, DependencyProperty dp, PropertyMetadata metadata, object baseValue)
	{
		if (storage._hasStickySnapshotValue && storage._animationClocks[0].CurrentState == ClockState.Stopped)
		{
			return storage._snapshotValue;
		}
		if (storage._animationClocks == null && storage._propertyTriggerLayers == null)
		{
			return storage._snapshotValue;
		}
		object obj = baseValue;
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = metadata.GetDefaultValue(d, dp);
		}
		if (storage._propertyTriggerLayers != null)
		{
			int count = storage._propertyTriggerLayers.Count;
			IList<AnimationLayer> values = storage._propertyTriggerLayers.Values;
			for (int i = 0; i < count; i++)
			{
				obj = values[i].GetCurrentValue(obj);
			}
		}
		if (storage._animationClocks != null)
		{
			FrugalObjectList<AnimationClock> animationClocks = storage._animationClocks;
			int count2 = animationClocks.Count;
			bool flag = false;
			object defaultDestinationValue = obj;
			object obj2 = obj;
			if (storage._snapshotValue != DependencyProperty.UnsetValue)
			{
				obj2 = storage._snapshotValue;
			}
			for (int j = 0; j < count2; j++)
			{
				if (animationClocks[j].CurrentState != ClockState.Stopped)
				{
					flag = true;
					obj2 = animationClocks[j].GetCurrentValue(obj2, defaultDestinationValue);
					if (obj2 == DependencyProperty.UnsetValue)
					{
						throw new InvalidOperationException(SR.Format(SR.Animation_ReturnedUnsetValueInstance, animationClocks[j].Timeline.GetType().FullName, dp.Name, d.GetType().FullName));
					}
				}
			}
			if (flag)
			{
				obj = obj2;
			}
		}
		if (DependencyProperty.IsValidType(obj, dp.PropertyType))
		{
			return obj;
		}
		throw new InvalidOperationException(SR.Format(SR.Animation_CalculatedValueIsInvalidForProperty, dp.Name, (obj == null) ? "null" : obj.ToString()));
	}

	[FriendAccessAllowed]
	internal static bool IsPropertyAnimatable(DependencyObject d, DependencyProperty dp)
	{
		if (dp.PropertyType != typeof(Visual3DCollection) && dp.ReadOnly)
		{
			return false;
		}
		if (dp.GetMetadata(d.DependencyObjectType) is UIPropertyMetadata { IsAnimationProhibited: not false })
		{
			return false;
		}
		return true;
	}

	internal static bool IsAnimationValid(DependencyProperty dp, AnimationTimeline animation)
	{
		if (!dp.PropertyType.IsAssignableFrom(animation.TargetPropertyType))
		{
			return animation.TargetPropertyType == typeof(object);
		}
		return true;
	}

	[FriendAccessAllowed]
	internal static bool IsAnimationClockValid(DependencyProperty dp, AnimationClock animation)
	{
		return IsAnimationValid(dp, animation.Timeline);
	}

	internal static FrugalMap GetAnimatedPropertiesMap(DependencyObject d)
	{
		return AnimatedPropertyMapField.GetValue(d);
	}

	internal static AnimationStorage GetStorage(DependencyObject d, DependencyProperty dp)
	{
		return AnimatedPropertyMapField.GetValue(d)[dp.GlobalIndex] as AnimationStorage;
	}

	private static AnimationStorage CreateStorage(DependencyObject d, DependencyProperty dp)
	{
		AnimationStorage animationStorage = ((!(dp.GetMetadata(d.DependencyObjectType) is IndependentlyAnimatedPropertyMetadata)) ? new AnimationStorage() : CreateIndependentAnimationStorageForType(dp.PropertyType));
		animationStorage.Initialize(d, dp);
		return animationStorage;
	}

	private static IndependentAnimationStorage CreateIndependentAnimationStorageForType(Type type)
	{
		if (type == typeof(double))
		{
			return new DoubleIndependentAnimationStorage();
		}
		if (type == typeof(Color))
		{
			return new ColorIndependentAnimationStorage();
		}
		if (type == typeof(Matrix))
		{
			return new MatrixIndependentAnimationStorage();
		}
		if (type == typeof(Point3D))
		{
			return new Point3DIndependentAnimationStorage();
		}
		if (type == typeof(Point))
		{
			return new PointIndependentAnimationStorage();
		}
		if (type == typeof(Quaternion))
		{
			return new QuaternionIndependentAnimationStorage();
		}
		if (type == typeof(Rect))
		{
			return new RectIndependentAnimationStorage();
		}
		if (type == typeof(Size))
		{
			return new SizeIndependentAnimationStorage();
		}
		return new Vector3DIndependentAnimationStorage();
	}
}
