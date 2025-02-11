using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows;

[TypeConverter(typeof(ResourceReferenceExpressionConverter))]
internal class ResourceReferenceExpression : Expression
{
	[Flags]
	private enum InternalState : byte
	{
		Default = 0,
		HasCachedResourceValue = 1,
		IsMentorCacheValid = 2,
		DisableThrowOnResourceFailure = 4,
		IsListeningForFreezableChanges = 8,
		IsListeningForInflated = 0x10
	}

	private class ResourceReferenceExpressionWeakContainer : WeakReference
	{
		private Freezable _resource;

		public ResourceReferenceExpressionWeakContainer(ResourceReferenceExpression target)
			: base(target)
		{
		}

		private void InvalidateTargetSubProperty(object sender, EventArgs args)
		{
			ResourceReferenceExpression resourceReferenceExpression = (ResourceReferenceExpression)Target;
			if (resourceReferenceExpression != null)
			{
				resourceReferenceExpression.InvalidateTargetSubProperty(sender, args);
			}
			else
			{
				RemoveChangedHandler();
			}
		}

		public void AddChangedHandler(Freezable resource)
		{
			if (_resource != null)
			{
				RemoveChangedHandler();
			}
			_resource = resource;
			_resource.Changed += InvalidateTargetSubProperty;
		}

		public void RemoveChangedHandler()
		{
			if (!_resource.IsFrozen)
			{
				_resource.Changed -= InvalidateTargetSubProperty;
				_resource = null;
			}
		}
	}

	private object _resourceKey;

	private object _cachedResourceValue;

	private DependencyObject _mentorCache;

	private DependencyObject _targetObject;

	private DependencyProperty _targetProperty;

	private InternalState _state;

	private ResourceReferenceExpressionWeakContainer _weakContainerRRE;

	public object ResourceKey => _resourceKey;

	public ResourceReferenceExpression(object resourceKey)
	{
		_resourceKey = resourceKey;
	}

	internal override DependencySource[] GetSources()
	{
		return null;
	}

	internal override object GetValue(DependencyObject d, DependencyProperty dp)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (ReadInternalState(InternalState.HasCachedResourceValue))
		{
			return _cachedResourceValue;
		}
		object source;
		return GetRawValue(d, out source, dp);
	}

	internal override Expression Copy(DependencyObject targetObject, DependencyProperty targetDP)
	{
		return new ResourceReferenceExpression(ResourceKey);
	}

	internal object GetRawValue(DependencyObject d, out object source, DependencyProperty dp)
	{
		if (!ReadInternalState(InternalState.IsMentorCacheValid))
		{
			_mentorCache = Helper.FindMentor(d);
			WriteInternalState(InternalState.IsMentorCacheValid, set: true);
			if (_mentorCache != null && _mentorCache != _targetObject)
			{
				Helper.DowncastToFEorFCE(_mentorCache, out var fe, out var fce, throwIfNeither: true);
				if (fe != null)
				{
					fe.ResourcesChanged += InvalidateExpressionValue;
				}
				else
				{
					fce.ResourcesChanged += InvalidateExpressionValue;
				}
			}
		}
		object obj;
		if (_mentorCache != null)
		{
			Helper.DowncastToFEorFCE(_mentorCache, out var fe2, out var fce2, throwIfNeither: true);
			obj = FrameworkElement.FindResourceInternal(fe2, fce2, dp, _resourceKey, null, allowDeferredResourceReference: true, mustReturnDeferredResourceReference: false, null, isImplicitStyleLookup: false, out source);
		}
		else
		{
			obj = FrameworkElement.FindResourceFromAppOrSystem(_resourceKey, out source, disableThrowOnResourceNotFound: false, allowDeferredResourceReference: true, mustReturnDeferredResourceReference: false);
		}
		if (obj == null)
		{
			obj = DependencyProperty.UnsetValue;
		}
		_cachedResourceValue = obj;
		WriteInternalState(InternalState.HasCachedResourceValue, set: true);
		object resource = obj;
		if (obj is DeferredResourceReference deferredResourceReference)
		{
			if (deferredResourceReference.IsInflated)
			{
				resource = deferredResourceReference.Value as Freezable;
			}
			else if (!ReadInternalState(InternalState.IsListeningForInflated))
			{
				deferredResourceReference.AddInflatedListener(this);
				WriteInternalState(InternalState.IsListeningForInflated, set: true);
			}
		}
		ListenForFreezableChanges(resource);
		return obj;
	}

	internal override bool SetValue(DependencyObject d, DependencyProperty dp, object value)
	{
		return false;
	}

	internal override void OnAttach(DependencyObject d, DependencyProperty dp)
	{
		_targetObject = d;
		_targetProperty = dp;
		FrameworkObject frameworkObject = new FrameworkObject(_targetObject);
		frameworkObject.HasResourceReference = true;
		if (!frameworkObject.IsValid)
		{
			_targetObject.InheritanceContextChanged += InvalidateExpressionValue;
		}
	}

	internal override void OnDetach(DependencyObject d, DependencyProperty dp)
	{
		InvalidateMentorCache();
		if (!(_targetObject is FrameworkElement) && !(_targetObject is FrameworkContentElement))
		{
			_targetObject.InheritanceContextChanged -= InvalidateExpressionValue;
		}
		_targetObject = null;
		_targetProperty = null;
		_weakContainerRRE = null;
	}

	private void InvalidateCacheValue()
	{
		object resource = _cachedResourceValue;
		if (_cachedResourceValue is DeferredResourceReference deferredResourceReference)
		{
			if (deferredResourceReference.IsInflated)
			{
				resource = deferredResourceReference.Value;
			}
			else if (ReadInternalState(InternalState.IsListeningForInflated))
			{
				deferredResourceReference.RemoveInflatedListener(this);
				WriteInternalState(InternalState.IsListeningForInflated, set: false);
			}
			deferredResourceReference.RemoveFromDictionary();
		}
		StopListeningForFreezableChanges(resource);
		_cachedResourceValue = null;
		WriteInternalState(InternalState.HasCachedResourceValue, set: false);
	}

	private void InvalidateMentorCache()
	{
		if (ReadInternalState(InternalState.IsMentorCacheValid))
		{
			if (_mentorCache != null)
			{
				if (_mentorCache != _targetObject)
				{
					Helper.DowncastToFEorFCE(_mentorCache, out var fe, out var fce, throwIfNeither: true);
					if (fe != null)
					{
						fe.ResourcesChanged -= InvalidateExpressionValue;
					}
					else
					{
						fce.ResourcesChanged -= InvalidateExpressionValue;
					}
				}
				_mentorCache = null;
			}
			WriteInternalState(InternalState.IsMentorCacheValid, set: false);
		}
		InvalidateCacheValue();
	}

	internal void InvalidateExpressionValue(object sender, EventArgs e)
	{
		if (_targetObject == null)
		{
			return;
		}
		if (e is ResourcesChangedEventArgs { Info: var info })
		{
			if (!info.IsTreeChange)
			{
				InvalidateCacheValue();
			}
			else
			{
				InvalidateMentorCache();
			}
		}
		else
		{
			InvalidateMentorCache();
		}
		InvalidateTargetProperty(sender, e);
	}

	private void InvalidateTargetProperty(object sender, EventArgs e)
	{
		_targetObject.InvalidateProperty(_targetProperty);
	}

	private void InvalidateTargetSubProperty(object sender, EventArgs e)
	{
		_targetObject.NotifySubPropertyChange(_targetProperty);
	}

	private void ListenForFreezableChanges(object resource)
	{
		if (!ReadInternalState(InternalState.IsListeningForFreezableChanges) && resource is Freezable { IsFrozen: false } freezable)
		{
			if (_weakContainerRRE == null)
			{
				_weakContainerRRE = new ResourceReferenceExpressionWeakContainer(this);
			}
			_weakContainerRRE.AddChangedHandler(freezable);
			WriteInternalState(InternalState.IsListeningForFreezableChanges, set: true);
		}
	}

	private void StopListeningForFreezableChanges(object resource)
	{
		if (!ReadInternalState(InternalState.IsListeningForFreezableChanges))
		{
			return;
		}
		if (resource is Freezable freezable && _weakContainerRRE != null)
		{
			if (!freezable.IsFrozen)
			{
				_weakContainerRRE.RemoveChangedHandler();
			}
			else
			{
				_weakContainerRRE = null;
			}
		}
		WriteInternalState(InternalState.IsListeningForFreezableChanges, set: false);
	}

	internal void OnDeferredResourceInflated(DeferredResourceReference deferredResourceReference)
	{
		if (ReadInternalState(InternalState.IsListeningForInflated))
		{
			deferredResourceReference.RemoveInflatedListener(this);
			WriteInternalState(InternalState.IsListeningForInflated, set: false);
		}
		ListenForFreezableChanges(deferredResourceReference.Value);
	}

	private bool ReadInternalState(InternalState reqFlag)
	{
		return (_state & reqFlag) != 0;
	}

	private void WriteInternalState(InternalState reqFlag, bool set)
	{
		if (set)
		{
			_state |= reqFlag;
		}
		else
		{
			_state &= (InternalState)(byte)(~(int)reqFlag);
		}
	}
}
