using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using MS.Internal;
using MS.Utility;

namespace System.Windows.Media.Animation;

/// <summary>A container timeline that provides object and property targeting information for its child animations. </summary>
public class Storyboard : ParallelTimeline
{
	private class ObjectPropertyPair
	{
		private DependencyObject _object;

		private DependencyProperty _property;

		public DependencyObject DependencyObject => _object;

		public DependencyProperty DependencyProperty => _property;

		public ObjectPropertyPair(DependencyObject o, DependencyProperty p)
		{
			_object = o;
			_property = p;
		}

		public override int GetHashCode()
		{
			return _object.GetHashCode() ^ _property.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o != null && o is ObjectPropertyPair)
			{
				return Equals((ObjectPropertyPair)o);
			}
			return false;
		}

		public bool Equals(ObjectPropertyPair key)
		{
			if (_object.Equals(key._object))
			{
				return _property == key._property;
			}
			return false;
		}
	}

	private class CloneCacheEntry
	{
		internal object Source;

		internal object Clone;

		internal CloneCacheEntry(object source, object clone)
		{
			Source = source;
			Clone = clone;
		}
	}

	internal class ChangeListener
	{
		private DependencyObject _target;

		private DependencyProperty _property;

		private Freezable _clone;

		private Freezable _original;

		internal ChangeListener(DependencyObject target, Freezable clone, DependencyProperty property, Freezable original)
		{
			_target = target;
			_property = property;
			_clone = clone;
			_original = original;
		}

		internal void InvalidatePropertyOnCloneChange(object source, EventArgs e)
		{
			CloneCacheEntry complexPathClone = GetComplexPathClone(_target, _property);
			if (complexPathClone != null && complexPathClone.Clone == _clone)
			{
				_target.InvalidateSubProperty(_property);
			}
			else
			{
				Cleanup();
			}
		}

		internal void InvalidatePropertyOnOriginalChange(object source, EventArgs e)
		{
			_target.InvalidateProperty(_property);
			Cleanup();
		}

		internal static void ListenToChangesOnFreezable(DependencyObject target, Freezable clone, DependencyProperty dp, Freezable original)
		{
			new ChangeListener(target, clone, dp, original).Setup();
		}

		private void Setup()
		{
			EventHandler value = InvalidatePropertyOnCloneChange;
			_clone.Changed += value;
			if (_original.IsFrozen)
			{
				_original = null;
				return;
			}
			value = InvalidatePropertyOnOriginalChange;
			_original.Changed += value;
		}

		private void Cleanup()
		{
			EventHandler value = InvalidatePropertyOnCloneChange;
			_clone.Changed -= value;
			if (_original != null)
			{
				value = InvalidatePropertyOnOriginalChange;
				_original.Changed -= value;
			}
			_target = null;
			_property = null;
			_clone = null;
			_original = null;
		}
	}

	internal static class Layers
	{
		internal static long ElementEventTrigger = 1L;

		internal static long StyleOrTemplateEventTrigger = 1L;

		internal static long Code = 1L;

		internal static long PropertyTriggerStartLayer = 2L;
	}

	private enum InteractiveOperation : ushort
	{
		Unknown,
		Pause,
		Remove,
		Resume,
		Seek,
		SeekAlignedToLastTick,
		SetSpeedRatio,
		SkipToFill,
		Stop
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Storyboard.Target" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Storyboard.Target" /> attached property.</returns>
	public static readonly DependencyProperty TargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> attached property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> attached property.</returns>
	public static readonly DependencyProperty TargetNameProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetProperty" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetProperty" /> attached property.</returns>
	public static readonly DependencyProperty TargetPropertyProperty;

	private static readonly UncommonField<HybridDictionary> StoryboardClockTreesField;

	private static readonly UncommonField<FrugalMap> ComplexPathCloneField;

	static Storyboard()
	{
		TargetNameProperty = DependencyProperty.RegisterAttached("TargetName", typeof(string), typeof(Storyboard));
		TargetPropertyProperty = DependencyProperty.RegisterAttached("TargetProperty", typeof(PropertyPath), typeof(Storyboard));
		StoryboardClockTreesField = new UncommonField<HybridDictionary>();
		ComplexPathCloneField = new UncommonField<FrugalMap>();
		PropertyMetadata defaultMetadata = new PropertyMetadata
		{
			FreezeValueCallback = TargetFreezeValueCallback
		};
		TargetProperty = DependencyProperty.RegisterAttached("Target", typeof(DependencyObject), typeof(Storyboard), defaultMetadata);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Storyboard" /> class. </summary>
	public Storyboard()
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.Storyboard" /> class.  </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.Storyboard" /> instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new Storyboard();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Storyboard" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new Storyboard Clone()
	{
		return (Storyboard)base.Clone();
	}

	/// <summary>Makes the specified <see cref="T:System.Windows.Media.Animation.Timeline" /> target the dependency object. </summary>
	/// <param name="element">The <see cref="T:System.Windows.Media.Animation.Timeline" /> that should target the specified dependency object.</param>
	/// <param name="value">The dependency object to target.</param>
	public static void SetTarget(DependencyObject element, DependencyObject value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TargetProperty, value);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Storyboard.Target" /> value of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />.</summary>
	/// <returns>The dependency object targeted by <paramref name="element" />.</returns>
	/// <param name="element">The timeline from which to retrieve the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" />.</param>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static DependencyObject GetTarget(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (DependencyObject)element.GetValue(TargetProperty);
	}

	private static bool TargetFreezeValueCallback(DependencyObject d, DependencyProperty dp, EntryIndex entryIndex, PropertyMetadata metadata, bool isChecking)
	{
		return true;
	}

	/// <summary>Makes the specified <see cref="T:System.Windows.Media.Animation.Timeline" /> target the dependency object with the specified name. </summary>
	/// <param name="element">The <see cref="T:System.Windows.Media.Animation.Timeline" /> that should target the specified dependency object.</param>
	/// <param name="name">The name of the dependency object to target.</param>
	public static void SetTargetName(DependencyObject element, string name)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		element.SetValue(TargetNameProperty, name);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> value of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>The name of the dependency object targeted by <paramref name="element" />.</returns>
	/// <param name="element">The timeline from which to retrieve the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" />. </param>
	public static string GetTargetName(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (string)element.GetValue(TargetNameProperty);
	}

	/// <summary>Makes the specified <see cref="T:System.Windows.Media.Animation.Timeline" /> target the specified dependency property.</summary>
	/// <param name="element">The <see cref="T:System.Windows.Media.Animation.Timeline" /> with which to associate the specified dependency property. </param>
	/// <param name="path">A path that describe the dependency property to be animated.</param>
	public static void SetTargetProperty(DependencyObject element, PropertyPath path)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		element.SetValue(TargetPropertyProperty, path);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetProperty" /> value of the specified <see cref="T:System.Windows.Media.Animation.Timeline" />. </summary>
	/// <returns>The property targeted by <paramref name="element" />.</returns>
	/// <param name="element">The dependency object from which to get the <see cref="P:System.Windows.Media.Animation.Storyboard.TargetProperty" />.</param>
	public static PropertyPath GetTargetProperty(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (PropertyPath)element.GetValue(TargetPropertyProperty);
	}

	internal static DependencyObject ResolveTargetName(string targetName, INameScope nameScope, DependencyObject element)
	{
		object obj = null;
		object obj2 = null;
		FrameworkElement frameworkElement = element as FrameworkElement;
		FrameworkContentElement frameworkContentElement = element as FrameworkContentElement;
		if (frameworkElement != null)
		{
			if (nameScope != null)
			{
				obj2 = ((FrameworkTemplate)nameScope).FindName(targetName, frameworkElement);
				obj = nameScope;
			}
			else
			{
				obj2 = frameworkElement.FindName(targetName);
				obj = frameworkElement;
			}
		}
		else
		{
			if (frameworkContentElement == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_NoNameScope, targetName));
			}
			obj2 = frameworkContentElement.FindName(targetName);
			obj = frameworkContentElement;
		}
		if (obj2 == null)
		{
			throw new InvalidOperationException(SR.Format(SR.Storyboard_NameNotFound, targetName, obj.GetType().ToString()));
		}
		return (obj2 as DependencyObject) ?? throw new InvalidOperationException(SR.Format(SR.Storyboard_TargetNameNotDependencyObject, targetName));
	}

	internal static BeginStoryboard ResolveBeginStoryboardName(string targetName, INameScope nameScope, FrameworkElement fe, FrameworkContentElement fce)
	{
		object obj = null;
		if (nameScope != null)
		{
			obj = nameScope.FindName(targetName);
			if (obj == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_NameNotFound, targetName, nameScope.GetType().ToString()));
			}
		}
		else if (fe != null)
		{
			obj = fe.FindName(targetName);
			if (obj == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_NameNotFound, targetName, fe.GetType().ToString()));
			}
		}
		else
		{
			if (fce == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_NoNameScope, targetName));
			}
			obj = fce.FindName(targetName);
			if (obj == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_NameNotFound, targetName, fce.GetType().ToString()));
			}
		}
		return (obj as BeginStoryboard) ?? throw new InvalidOperationException(SR.Format(SR.Storyboard_BeginStoryboardNameNotFound, targetName));
	}

	private void ClockTreeWalkRecursive(Clock currentClock, DependencyObject containingObject, INameScope nameScope, DependencyObject parentObject, string parentObjectName, PropertyPath parentPropertyPath, HandoffBehavior handoffBehavior, HybridDictionary clockMappings, long layer)
	{
		Timeline timeline = currentClock.Timeline;
		DependencyObject dependencyObject = parentObject;
		string text = parentObjectName;
		PropertyPath propertyPath = parentPropertyPath;
		string text2 = (string)timeline.GetValue(TargetNameProperty);
		if (text2 != null)
		{
			if (nameScope is Style)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_TargetNameNotAllowedInStyle, text2));
			}
			text = text2;
		}
		DependencyObject dependencyObject2 = (DependencyObject)timeline.GetValue(TargetProperty);
		if (dependencyObject2 != null)
		{
			dependencyObject = dependencyObject2;
			text = null;
		}
		PropertyPath propertyPath2 = (PropertyPath)timeline.GetValue(TargetPropertyProperty);
		if (propertyPath2 != null)
		{
			propertyPath = propertyPath2;
		}
		if (currentClock is AnimationClock)
		{
			DependencyProperty dependencyProperty = null;
			AnimationClock animationClock = (AnimationClock)currentClock;
			if (dependencyObject == null)
			{
				if (text != null)
				{
					DependencyObject element = Helper.FindMentor(containingObject);
					dependencyObject = ResolveTargetName(text, nameScope, element);
				}
				else
				{
					dependencyObject = containingObject as FrameworkElement;
					if (dependencyObject == null)
					{
						dependencyObject = containingObject as FrameworkContentElement;
					}
					if (dependencyObject == null)
					{
						throw new InvalidOperationException(SR.Format(SR.Storyboard_NoTarget, timeline.GetType().ToString()));
					}
				}
			}
			if (propertyPath == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_TargetPropertyRequired, timeline.GetType().ToString()));
			}
			using (propertyPath.SetContext(dependencyObject))
			{
				if (propertyPath.Length < 1)
				{
					throw new InvalidOperationException(SR.Storyboard_PropertyPathEmpty);
				}
				VerifyPathIsAnimatable(propertyPath);
				if (propertyPath.Length == 1)
				{
					if (!(propertyPath.GetAccessor(0) is DependencyProperty dependencyProperty2))
					{
						throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathMustPointToDependencyProperty, propertyPath.Path));
					}
					VerifyAnimationIsValid(dependencyProperty2, animationClock);
					ObjectPropertyPair mappingKey = new ObjectPropertyPair(dependencyObject, dependencyProperty2);
					UpdateMappings(clockMappings, mappingKey, animationClock);
				}
				else
				{
					ProcessComplexPath(clockMappings, dependencyObject, propertyPath, animationClock, handoffBehavior, layer);
				}
				return;
			}
		}
		if (currentClock is MediaClock)
		{
			ApplyMediaClock(nameScope, containingObject, dependencyObject, text, (MediaClock)currentClock);
		}
		else if (currentClock is ClockGroup { Children: var children })
		{
			for (int i = 0; i < children.Count; i++)
			{
				ClockTreeWalkRecursive(children[i], containingObject, nameScope, dependencyObject, text, propertyPath, handoffBehavior, clockMappings, layer);
			}
		}
	}

	private static void ApplyMediaClock(INameScope nameScope, DependencyObject containingObject, DependencyObject currentObject, string currentObjectName, MediaClock mediaClock)
	{
		MediaElement mediaElement = null;
		if (currentObjectName == null)
		{
			mediaElement = ((currentObject == null) ? (containingObject as MediaElement) : (currentObject as MediaElement));
		}
		else
		{
			DependencyObject element = Helper.FindMentor(containingObject);
			mediaElement = ResolveTargetName(currentObjectName, nameScope, element) as MediaElement;
			if (mediaElement == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_MediaElementNotFound, currentObjectName));
			}
		}
		if (mediaElement == null)
		{
			throw new InvalidOperationException(SR.Storyboard_MediaElementRequired);
		}
		mediaElement.Clock = mediaClock;
	}

	private static void UpdateMappings(HybridDictionary clockMappings, ObjectPropertyPair mappingKey, AnimationClock animationClock)
	{
		object obj = clockMappings[mappingKey];
		if (obj == null)
		{
			clockMappings[mappingKey] = animationClock;
		}
		else if (obj is AnimationClock)
		{
			List<AnimationClock> list = new List<AnimationClock>();
			list.Add((AnimationClock)obj);
			list.Add(animationClock);
			clockMappings[mappingKey] = list;
		}
		else
		{
			((List<AnimationClock>)obj).Add(animationClock);
		}
	}

	private static void ApplyAnimationClocks(HybridDictionary clockMappings, HandoffBehavior handoffBehavior, long layer)
	{
		foreach (DictionaryEntry clockMapping in clockMappings)
		{
			ObjectPropertyPair objectPropertyPair = (ObjectPropertyPair)clockMapping.Key;
			object value = clockMapping.Value;
			List<AnimationClock> list;
			if (value is AnimationClock)
			{
				list = new List<AnimationClock>(1);
				list.Add((AnimationClock)value);
			}
			else
			{
				list = (List<AnimationClock>)value;
			}
			AnimationStorage.ApplyAnimationClocksToLayer(objectPropertyPair.DependencyObject, objectPropertyPair.DependencyProperty, list, handoffBehavior, layer);
		}
	}

	internal static void VerifyPathIsAnimatable(PropertyPath path)
	{
		object obj = null;
		object obj2 = null;
		bool flag = true;
		Freezable freezable = null;
		for (int i = 0; i < path.Length; i++)
		{
			obj = path.GetItem(i);
			obj2 = path.GetAccessor(i);
			if (obj == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathObjectNotFound, AccessorName(path, i - 1), path.Path));
			}
			if (obj2 == null)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathPropertyNotFound, path.Path));
			}
			if (i == 1)
			{
				if (obj is Freezable { IsFrozen: not false })
				{
					flag = false;
				}
			}
			else if (flag && obj is Freezable { IsFrozen: not false } freezable3)
			{
				if (i > 0)
				{
					throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathFrozenCheckFailed, AccessorName(path, i - 1), path.Path, freezable3.GetType().ToString()));
				}
				throw new InvalidOperationException(SR.Format(SR.Storyboard_ImmutableTargetNotSupported, path.Path));
			}
			if (i == path.Length - 1)
			{
				DependencyObject dependencyObject = obj as DependencyObject;
				DependencyProperty dependencyProperty = obj2 as DependencyProperty;
				if (dependencyObject == null)
				{
					throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathMustPointToDependencyObject, AccessorName(path, i - 1), path.Path));
				}
				if (dependencyProperty == null)
				{
					throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathMustPointToDependencyProperty, path.Path));
				}
				if (flag && dependencyObject.IsSealed)
				{
					throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathSealedCheckFailed, dependencyProperty.Name, path.Path, dependencyObject));
				}
				if (!AnimationStorage.IsPropertyAnimatable(dependencyObject, dependencyProperty))
				{
					throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathIncludesNonAnimatableProperty, path.Path, dependencyProperty.Name));
				}
			}
		}
	}

	private static string AccessorName(PropertyPath path, int index)
	{
		object accessor = path.GetAccessor(index);
		if (accessor is DependencyProperty)
		{
			return ((DependencyProperty)accessor).Name;
		}
		if (accessor is PropertyInfo)
		{
			return ((PropertyInfo)accessor).Name;
		}
		if (accessor is PropertyDescriptor)
		{
			return ((PropertyDescriptor)accessor).Name;
		}
		return "[Unknown]";
	}

	private static void VerifyAnimationIsValid(DependencyProperty targetProperty, AnimationClock animationClock)
	{
		if (!AnimationStorage.IsAnimationClockValid(targetProperty, animationClock))
		{
			throw new InvalidOperationException(SR.Format(SR.Storyboard_AnimationMismatch, animationClock.Timeline.GetType(), targetProperty.Name, targetProperty.PropertyType));
		}
	}

	private void ProcessComplexPath(HybridDictionary clockMappings, DependencyObject targetObject, PropertyPath path, AnimationClock animationClock, HandoffBehavior handoffBehavior, long layer)
	{
		DependencyProperty dependencyProperty = path.GetAccessor(0) as DependencyProperty;
		object value = targetObject.GetValue(dependencyProperty);
		DependencyObject dependencyObject = path.LastItem as DependencyObject;
		DependencyProperty dependencyProperty2 = path.LastAccessor as DependencyProperty;
		if (dependencyObject == null || dependencyProperty2 == null || dependencyProperty == null)
		{
			throw new InvalidOperationException(SR.Format(SR.Storyboard_PropertyPathUnresolved, path.Path));
		}
		VerifyAnimationIsValid(dependencyProperty2, animationClock);
		if (PropertyCloningRequired(value))
		{
			VerifyComplexPathSupport(targetObject);
			Freezable freezable = ((Freezable)value).Clone();
			SetComplexPathClone(targetObject, dependencyProperty, value, freezable);
			targetObject.InvalidateProperty(dependencyProperty);
			if (targetObject.GetValue(dependencyProperty) != freezable)
			{
				throw new InvalidOperationException(SR.Format(SR.Storyboard_ImmutableTargetNotSupported, path.Path));
			}
			using (path.SetContext(targetObject))
			{
				dependencyObject = path.LastItem as DependencyObject;
				dependencyProperty2 = path.LastAccessor as DependencyProperty;
			}
			ChangeListener.ListenToChangesOnFreezable(targetObject, freezable, dependencyProperty, (Freezable)value);
		}
		ObjectPropertyPair mappingKey = new ObjectPropertyPair(dependencyObject, dependencyProperty2);
		UpdateMappings(clockMappings, mappingKey, animationClock);
	}

	private bool PropertyCloningRequired(object targetPropertyValue)
	{
		if (targetPropertyValue is Freezable)
		{
			if (((Freezable)targetPropertyValue).IsFrozen)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void VerifyComplexPathSupport(DependencyObject targetObject)
	{
		if (targetObject is FrameworkElement || targetObject is FrameworkContentElement)
		{
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.Storyboard_ComplexPathNotSupported, targetObject.GetType().ToString()));
	}

	internal static void GetComplexPathValue(DependencyObject targetObject, DependencyProperty targetProperty, ref EffectiveValueEntry entry, PropertyMetadata metadata)
	{
		CloneCacheEntry complexPathClone = GetComplexPathClone(targetObject, targetProperty);
		if (complexPathClone == null)
		{
			return;
		}
		object obj = entry.Value;
		if (obj == DependencyProperty.UnsetValue && complexPathClone.Source == metadata.GetDefaultValue(targetObject, targetProperty))
		{
			entry.BaseValueSourceInternal = BaseValueSourceInternal.Default;
			entry.SetAnimatedValue(complexPathClone.Clone, DependencyProperty.UnsetValue);
			return;
		}
		if (obj is DeferredReference deferredReference)
		{
			obj = (entry.Value = deferredReference.GetValue(entry.BaseValueSourceInternal));
		}
		if (complexPathClone.Source == obj)
		{
			CloneEffectiveValue(ref entry, complexPathClone);
		}
		else
		{
			SetComplexPathClone(targetObject, targetProperty, DependencyProperty.UnsetValue, DependencyProperty.UnsetValue);
		}
	}

	private static void CloneEffectiveValue(ref EffectiveValueEntry entry, CloneCacheEntry cacheEntry)
	{
		object clone = cacheEntry.Clone;
		if (!entry.IsExpression)
		{
			entry.Value = clone;
		}
		else
		{
			entry.ModifiedValue.ExpressionValue = clone;
		}
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	public void Begin(FrameworkElement containingObject)
	{
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them, using the specified <see cref="T:System.Windows.Media.Animation.HandoffBehavior" />.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a specified <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	public void Begin(FrameworkElement containingObject, HandoffBehavior handoffBehavior)
	{
		Begin(containingObject, handoffBehavior, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="isControllable">true if the storyboard should be interactively controllable; otherwise, false.</param>
	public void Begin(FrameworkElement containingObject, bool isControllable)
	{
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, isControllable);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a specified <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	/// <param name="isControllable">Declares whether the animation is controllable (can be paused) once started.</param>
	public void Begin(FrameworkElement containingObject, HandoffBehavior handoffBehavior, bool isControllable)
	{
		BeginCommon(containingObject, null, handoffBehavior, isControllable, Layers.Code);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets within the specified template and initiates them.</summary>
	/// <param name="containingObject">The object to which the specified <paramref name="frameworkTemplate" /> has been applied. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />. </param>
	/// <param name="frameworkTemplate">The template to animate. </param>
	public void Begin(FrameworkElement containingObject, FrameworkTemplate frameworkTemplate)
	{
		Begin(containingObject, frameworkTemplate, HandoffBehavior.SnapshotAndReplace, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets within the specified template and initiates them.</summary>
	/// <param name="containingObject">The object to which the specified <paramref name="frameworkTemplate" /> has been applied. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="frameworkTemplate">The template to animate.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	public void Begin(FrameworkElement containingObject, FrameworkTemplate frameworkTemplate, HandoffBehavior handoffBehavior)
	{
		Begin(containingObject, frameworkTemplate, handoffBehavior, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets within the specified template and initiates them.</summary>
	/// <param name="containingObject">The object to which the specified <paramref name="frameworkTemplate" /> has been applied.  Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="frameworkTemplate">The template to animate.</param>
	/// <param name="isControllable">true if the storyboard should be interactively controllable; otherwise, false.</param>
	public void Begin(FrameworkElement containingObject, FrameworkTemplate frameworkTemplate, bool isControllable)
	{
		Begin(containingObject, frameworkTemplate, HandoffBehavior.SnapshotAndReplace, isControllable);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets within the specified template and initiates them.</summary>
	/// <param name="containingObject">The object to which the specified <paramref name="frameworkTemplate" /> has been applied. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="frameworkTemplate">The template to animate.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	/// <param name="isControllable">true if the storyboard should be interactively controllable; otherwise, false.</param>
	public void Begin(FrameworkElement containingObject, FrameworkTemplate frameworkTemplate, HandoffBehavior handoffBehavior, bool isControllable)
	{
		BeginCommon(containingObject, frameworkTemplate, handoffBehavior, isControllable, Layers.Code);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />. </param>
	public void Begin(FrameworkContentElement containingObject)
	{
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them, using the specified <see cref="T:System.Windows.Media.Animation.HandoffBehavior" />.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	public void Begin(FrameworkContentElement containingObject, HandoffBehavior handoffBehavior)
	{
		Begin(containingObject, handoffBehavior, isControllable: false);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="isControllable">true if the storyboard should be interactively controllable; otherwise, false.</param>
	public void Begin(FrameworkContentElement containingObject, bool isControllable)
	{
		Begin(containingObject, HandoffBehavior.SnapshotAndReplace, isControllable);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them, using the specified <see cref="T:System.Windows.Media.Animation.HandoffBehavior" />. </summary>
	/// <param name="containingObject">An object contained within the same name scope as the targets of this storyboard's animations. Animations without a specified <see cref="P:System.Windows.Media.Animation.Storyboard.TargetName" /> are applied to <paramref name="containingObject" />.</param>
	/// <param name="handoffBehavior">The behavior the new animation should use to interact with any current animations.</param>
	/// <param name="isControllable">Declares whether the animation is controllable (can be paused) once started.</param>
	public void Begin(FrameworkContentElement containingObject, HandoffBehavior handoffBehavior, bool isControllable)
	{
		BeginCommon(containingObject, null, handoffBehavior, isControllable, Layers.Code);
	}

	/// <summary>Applies the animations associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to their targets and initiates them.</summary>
	public void Begin()
	{
		INameScope nameScope = null;
		HandoffBehavior handoffBehavior = HandoffBehavior.SnapshotAndReplace;
		bool isControllable = true;
		long code = Layers.Code;
		BeginCommon(this, nameScope, handoffBehavior, isControllable, code);
	}

	internal void BeginCommon(DependencyObject containingObject, INameScope nameScope, HandoffBehavior handoffBehavior, bool isControllable, long layer)
	{
		if (containingObject == null)
		{
			throw new ArgumentNullException("containingObject");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Storyboard_UnrecognizedHandoffBehavior);
		}
		if (base.BeginTime.HasValue && MediaContext.CurrentMediaContext.TimeManager != null)
		{
			if (TraceAnimation.IsEnabled)
			{
				TraceAnimation.TraceActivityItem(TraceAnimation.StoryboardBegin, this, base.Name, containingObject, nameScope);
			}
			HybridDictionary clockMappings = new HybridDictionary();
			Clock clock = CreateClock(isControllable);
			ClockTreeWalkRecursive(clock, containingObject, nameScope, null, null, null, handoffBehavior, clockMappings, layer);
			ApplyAnimationClocks(clockMappings, handoffBehavior, layer);
			if (isControllable)
			{
				SetStoryboardClock(containingObject, clock);
			}
		}
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentGlobalSpeed" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>The current global speed, or null if the clock is stopped. </returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public double? GetCurrentGlobalSpeed(FrameworkElement containingObject)
	{
		return GetCurrentGlobalSpeedImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentGlobalSpeed" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <returns>The current global speed, or null if the clock is stopped. </returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public double? GetCurrentGlobalSpeed(FrameworkContentElement containingObject)
	{
		return GetCurrentGlobalSpeedImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentGlobalSpeed" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <returns>The current global speed, or 0 if the clock is stopped. </returns>
	public double GetCurrentGlobalSpeed()
	{
		double? currentGlobalSpeedImpl = GetCurrentGlobalSpeedImpl(this);
		if (currentGlobalSpeedImpl.HasValue)
		{
			return currentGlobalSpeedImpl.Value;
		}
		return 0.0;
	}

	private double? GetCurrentGlobalSpeedImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.CurrentGlobalSpeed;
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentIteration" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>This clock's current iteration within its current active period, or null if this clock is stopped.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public int? GetCurrentIteration(FrameworkElement containingObject)
	{
		return GetCurrentIterationImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentIteration" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>This clock's current iteration within its current active period, or null if this clock is stopped.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public int? GetCurrentIteration(FrameworkContentElement containingObject)
	{
		return GetCurrentIterationImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentIteration" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>This clock's current iteration within its current active period, or null if this clock is stopped.</returns>
	public int GetCurrentIteration()
	{
		int? currentIterationImpl = GetCurrentIterationImpl(this);
		if (currentIterationImpl.HasValue)
		{
			return currentIterationImpl.Value;
		}
		return 0;
	}

	private int? GetCurrentIterationImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.CurrentIteration;
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />, or 0.0 if this clock is active and its <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> has a <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of <see cref="P:System.Windows.Duration.Forever" />; otherwise, a value between 0.0 and 1.0 that indicates the current progress of this clock within its current iteration. A value of 0.0 indicates no progress, and a value of 1.0 indicates that the clock is at the end of its current iteration.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public double? GetCurrentProgress(FrameworkElement containingObject)
	{
		return GetCurrentProgressImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />, or 0.0 if this clock is active and its <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> has a <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of <see cref="P:System.Windows.Duration.Forever" />; otherwise, a value between 0.0 and 1.0 that indicates the current progress of this clock within its current iteration. A value of 0.0 indicates no progress, and a value of 1.0 indicates that the clock is at the end of its current iteration.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public double? GetCurrentProgress(FrameworkContentElement containingObject)
	{
		return GetCurrentProgressImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />, or 0.0 if this clock is active and its <see cref="P:System.Windows.Media.Animation.Clock.Timeline" /> has a <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> of <see cref="P:System.Windows.Duration.Forever" />; otherwise, a value between 0.0 and 1.0 that indicates the current progress of this clock within its current iteration. A value of 0.0 indicates no progress, and a value of 1.0 indicates that the clock is at the end of its current iteration.</returns>
	public double GetCurrentProgress()
	{
		double? currentProgressImpl = GetCurrentProgressImpl(this);
		if (currentProgressImpl.HasValue)
		{
			return currentProgressImpl.Value;
		}
		return 0.0;
	}

	private double? GetCurrentProgressImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.CurrentProgress;
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentState" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>The current state of the clock created for this storyboard: <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public ClockState GetCurrentState(FrameworkElement containingObject)
	{
		return GetCurrentStateImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentState" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>The current state of the clock created for this storyboard: <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public ClockState GetCurrentState(FrameworkContentElement containingObject)
	{
		return GetCurrentStateImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentState" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>The current state of the clock created for this storyboard: <see cref="F:System.Windows.Media.Animation.ClockState.Active" />, <see cref="F:System.Windows.Media.Animation.ClockState.Filling" />, or <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />.</returns>
	public ClockState GetCurrentState()
	{
		return GetCurrentStateImpl(this);
	}

	private ClockState GetCurrentStateImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.CurrentState ?? ClockState.Stopped;
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this storyboard's clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />; otherwise, the current time of the storyboard's clock.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public TimeSpan? GetCurrentTime(FrameworkElement containingObject)
	{
		return GetCurrentTimeImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this storyboard's clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />; otherwise, the current time of the storyboard's clock.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public TimeSpan? GetCurrentTime(FrameworkContentElement containingObject)
	{
		return GetCurrentTimeImpl(containingObject);
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <returns>null if this storyboard's clock is <see cref="F:System.Windows.Media.Animation.ClockState.Stopped" />; otherwise, the current time of the storyboard's clock.</returns>
	public TimeSpan GetCurrentTime()
	{
		TimeSpan? currentTimeImpl = GetCurrentTimeImpl(this);
		if (currentTimeImpl.HasValue)
		{
			return currentTimeImpl.Value;
		}
		return default(TimeSpan);
	}

	private TimeSpan? GetCurrentTimeImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.CurrentTime;
	}

	/// <summary>Retrieves a value that indicates whether the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Animation.Clock" /> created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused; otherwise, false.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public bool GetIsPaused(FrameworkElement containingObject)
	{
		return GetIsPausedImpl(containingObject);
	}

	/// <summary>Retrieves a value that indicates whether the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Animation.Clock" /> created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused; otherwise, false.</returns>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public bool GetIsPaused(FrameworkContentElement containingObject)
	{
		return GetIsPausedImpl(containingObject);
	}

	/// <summary>Retrieves a value that indicates whether the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.Animation.Clock" /> created for this <see cref="T:System.Windows.Media.Animation.Storyboard" /> is paused; otherwise, false.</returns>
	public bool GetIsPaused()
	{
		return GetIsPausedImpl(this);
	}

	private bool GetIsPausedImpl(DependencyObject containingObject)
	{
		return GetStoryboardClock(containingObject)?.IsPaused ?? false;
	}

	/// <summary>Pauses the <see cref="T:System.Windows.Media.Animation.Clock" /> of the specified <see cref="T:System.Windows.FrameworkElement" /> associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Pause(FrameworkElement containingObject)
	{
		PauseImpl(containingObject);
	}

	/// <summary>Pauses the <see cref="T:System.Windows.Media.Animation.Clock" /> of the specified <see cref="T:System.Windows.FrameworkContentElement" /> associated with this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Pause(FrameworkContentElement containingObject)
	{
		PauseImpl(containingObject);
	}

	/// <summary>Pauses the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	public void Pause()
	{
		PauseImpl(this);
	}

	private void PauseImpl(DependencyObject containingObject)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.Pause)?.Controller.Pause();
		if (TraceAnimation.IsEnabled)
		{
			TraceAnimation.TraceActivityItem(TraceAnimation.StoryboardPause, this, base.Name, this);
		}
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. Animations that belong to this <see cref="T:System.Windows.Media.Animation.Storyboard" /> no longer affect the properties they once animated, regardless of their <see cref="P:System.Windows.Media.Animation.Timeline.FillBehavior" /> setting. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Remove(FrameworkElement containingObject)
	{
		RemoveImpl(containingObject);
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. Animations that belong to this <see cref="T:System.Windows.Media.Animation.Storyboard" /> no longer affect the properties they once animated, regardless of their <see cref="P:System.Windows.Media.Animation.Timeline.FillBehavior" /> setting.</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Remove(FrameworkContentElement containingObject)
	{
		RemoveImpl(containingObject);
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. Animations that belong to this <see cref="T:System.Windows.Media.Animation.Storyboard" /> no longer affect the properties they once animated, regardless of their <see cref="P:System.Windows.Media.Animation.Timeline.FillBehavior" /> setting.</summary>
	public void Remove()
	{
		RemoveImpl(this);
	}

	private void RemoveImpl(DependencyObject containingObject)
	{
		Clock storyboardClock = GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.Remove);
		if (storyboardClock != null)
		{
			storyboardClock.Controller.Remove();
			StoryboardClockTreesField.GetValue(containingObject)?.Remove(this);
		}
		if (TraceAnimation.IsEnabled)
		{
			TraceAnimation.TraceActivityItem(TraceAnimation.StoryboardRemove, this, base.Name, containingObject);
		}
	}

	/// <summary>Resumes the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Resume(FrameworkElement containingObject)
	{
		ResumeImpl(containingObject);
	}

	/// <summary>Resumes the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Resume(FrameworkContentElement containingObject)
	{
		ResumeImpl(containingObject);
	}

	/// <summary>Resumes the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	public void Resume()
	{
		ResumeImpl(this);
	}

	private void ResumeImpl(DependencyObject containingObject)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.Resume)?.Controller.Resume();
		if (TraceAnimation.IsEnabled)
		{
			TraceAnimation.TraceActivityItem(TraceAnimation.StoryboardResume, this, base.Name, containingObject);
		}
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to the specified position. The <see cref="T:System.Windows.Media.Animation.Storyboard" /> performs the requested seek when the next clock tick occurs. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void Seek(FrameworkElement containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekImpl(containingObject, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to the specified position. The <see cref="T:System.Windows.Media.Animation.Storyboard" /> performs the requested seek when the next clock tick occurs.</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void Seek(FrameworkContentElement containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekImpl(containingObject, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to the specified position. The <see cref="T:System.Windows.Media.Animation.Storyboard" /> performs the requested seek when the next clock tick occurs.</summary>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void Seek(TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekImpl(this, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to the specified position. The <see cref="T:System.Windows.Media.Animation.Storyboard" /> performs the requested seek when the next clock tick occurs.</summary>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward. </param>
	public void Seek(TimeSpan offset)
	{
		SeekImpl(this, offset, TimeSeekOrigin.BeginTime);
	}

	private void SeekImpl(DependencyObject containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.Seek)?.Controller.Seek(offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to a new position immediately (synchronously).</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void SeekAlignedToLastTick(FrameworkElement containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekAlignedToLastTickImpl(containingObject, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to a new position immediately (synchronously).</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void SeekAlignedToLastTick(FrameworkContentElement containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekAlignedToLastTickImpl(containingObject, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to a new position immediately (synchronously).</summary>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward from the specified <paramref name="origin" />.</param>
	/// <param name="origin">The position from which <paramref name="offset" /> is applied.</param>
	public void SeekAlignedToLastTick(TimeSpan offset, TimeSeekOrigin origin)
	{
		SeekAlignedToLastTickImpl(this, offset, origin);
	}

	/// <summary>Seeks this <see cref="T:System.Windows.Media.Animation.Storyboard" /> to a new position immediately (synchronously).</summary>
	/// <param name="offset">A positive or negative value that describes the amount by which the timeline should move forward or backward.</param>
	public void SeekAlignedToLastTick(TimeSpan offset)
	{
		SeekAlignedToLastTickImpl(this, offset, TimeSeekOrigin.BeginTime);
	}

	private void SeekAlignedToLastTickImpl(DependencyObject containingObject, TimeSpan offset, TimeSeekOrigin origin)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.SeekAlignedToLastTick)?.Controller.SeekAlignedToLastTick(offset, origin);
	}

	/// <summary>Sets the interactive speed ratio of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="speedRatio">A finite value greater than zero that is the new interactive speed ratio of the storyboard. This value is multiplied against the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> value to determine the storyboard's effective speed. This value does not overwrite the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> property. For example, calling this method and specifying an interactive speed ratio of 3 on a storyboard with a <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> of 0.5 gives the storyboard an effective speed of 1.5. </param>
	public void SetSpeedRatio(FrameworkElement containingObject, double speedRatio)
	{
		SetSpeedRatioImpl(containingObject, speedRatio);
	}

	/// <summary>Sets the interactive speed ratio of the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	/// <param name="speedRatio">A finite value greater than zero that is the new interactive speed ratio of the storyboard. This value is multiplied against the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> value to determine the storyboard's effective speed. This value does not overwrite the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> property. For example, calling this method and specifying an interactive speed ratio of 3 on a storyboard with a <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> of 0.5 gives the storyboard an effective speed of 1.5.</param>
	public void SetSpeedRatio(FrameworkContentElement containingObject, double speedRatio)
	{
		SetSpeedRatioImpl(containingObject, speedRatio);
	}

	/// <summary>Sets the interactive speed ratio for the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	/// <param name="speedRatio">A finite value greater than zero that is the new interactive speed ratio of the storyboard. This value is multiplied against the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> value to determine the storyboard's effective speed. This value does not overwrite the storyboard's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> property. For example, calling this method and specifying an interactive speed ratio of 3 on a storyboard with a <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> of 0.5 gives the storyboard an effective speed of 1.5.</param>
	public void SetSpeedRatio(double speedRatio)
	{
		SetSpeedRatioImpl(this, speedRatio);
	}

	private void SetSpeedRatioImpl(DependencyObject containingObject, double speedRatio)
	{
		Clock storyboardClock = GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.SetSpeedRatio);
		if (storyboardClock != null)
		{
			storyboardClock.Controller.SpeedRatio = speedRatio;
		}
	}

	/// <summary>Advances the current time of this storyboard's <see cref="T:System.Windows.Media.Animation.Clock" /> to the end of its active period. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void SkipToFill(FrameworkElement containingObject)
	{
		SkipToFillImpl(containingObject);
	}

	/// <summary>Advances the current time of this storyboard's <see cref="T:System.Windows.Media.Animation.Clock" /> to the end of its active period.</summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void SkipToFill(FrameworkContentElement containingObject)
	{
		SkipToFillImpl(containingObject);
	}

	/// <summary>Advances the current time of this storyboard's <see cref="T:System.Windows.Media.Animation.Clock" /> to the end of its active period.</summary>
	public void SkipToFill()
	{
		SkipToFillImpl(this);
	}

	private void SkipToFillImpl(DependencyObject containingObject)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.SkipToFill)?.Controller.SkipToFill();
	}

	/// <summary>Stops the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Stop(FrameworkElement containingObject)
	{
		StopImpl(containingObject);
	}

	/// <summary>Stops the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />. </summary>
	/// <param name="containingObject">The object specified when the <see cref="M:System.Windows.Media.Animation.Storyboard.Begin(System.Windows.FrameworkContentElement,System.Boolean)" /> method was called. This object contains the <see cref="T:System.Windows.Media.Animation.Clock" /> objects that were created for this storyboard and its children.</param>
	public void Stop(FrameworkContentElement containingObject)
	{
		StopImpl(containingObject);
	}

	/// <summary>Stops the <see cref="T:System.Windows.Media.Animation.Clock" /> that was created for this <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
	public void Stop()
	{
		StopImpl(this);
	}

	private void StopImpl(DependencyObject containingObject)
	{
		GetStoryboardClock(containingObject, throwIfNull: false, InteractiveOperation.Stop)?.Controller.Stop();
		if (TraceAnimation.IsEnabled)
		{
			TraceAnimation.TraceActivityItem(TraceAnimation.StoryboardStop, this, base.Name, containingObject);
		}
	}

	private Clock GetStoryboardClock(DependencyObject o)
	{
		return GetStoryboardClock(o, throwIfNull: true, InteractiveOperation.Unknown);
	}

	private Clock GetStoryboardClock(DependencyObject o, bool throwIfNull, InteractiveOperation operation)
	{
		Clock result = null;
		WeakReference weakReference = null;
		HybridDictionary value = StoryboardClockTreesField.GetValue(o);
		if (value != null)
		{
			weakReference = value[this] as WeakReference;
		}
		if (weakReference == null)
		{
			if (throwIfNull)
			{
				throw new InvalidOperationException(SR.Storyboard_NeverApplied);
			}
			if (TraceAnimation.IsEnabledOverride)
			{
				TraceAnimation.Trace(TraceEventType.Warning, TraceAnimation.StoryboardNotApplied, operation, this, o);
			}
		}
		if (weakReference != null)
		{
			result = weakReference.Target as Clock;
		}
		return result;
	}

	private void SetStoryboardClock(DependencyObject o, Clock clock)
	{
		HybridDictionary hybridDictionary = StoryboardClockTreesField.GetValue(o);
		if (hybridDictionary == null)
		{
			hybridDictionary = new HybridDictionary();
			StoryboardClockTreesField.SetValue(o, hybridDictionary);
		}
		hybridDictionary[this] = new WeakReference(clock);
	}

	private static CloneCacheEntry GetComplexPathClone(DependencyObject o, DependencyProperty dp)
	{
		FrugalMap value = ComplexPathCloneField.GetValue(o);
		if (value[dp.GlobalIndex] != DependencyProperty.UnsetValue)
		{
			return (CloneCacheEntry)value[dp.GlobalIndex];
		}
		return null;
	}

	private static void SetComplexPathClone(DependencyObject o, DependencyProperty dp, object source, object clone)
	{
		FrugalMap value = ComplexPathCloneField.GetValue(o);
		if (clone != DependencyProperty.UnsetValue)
		{
			value[dp.GlobalIndex] = new CloneCacheEntry(source, clone);
		}
		else
		{
			value[dp.GlobalIndex] = DependencyProperty.UnsetValue;
		}
		ComplexPathCloneField.SetValue(o, value);
	}
}
