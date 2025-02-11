using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal sealed class DependencyObjectProvider : TypeDescriptionProvider
{
	private static readonly UncommonField<IDictionary> _cacheSlot = new UncommonField<IDictionary>(null);

	private static Dictionary<PropertyKey, DependencyObjectPropertyDescriptor> _propertyMap = new Dictionary<PropertyKey, DependencyObjectPropertyDescriptor>();

	private static Dictionary<PropertyKey, DependencyPropertyKind> _propertyKindMap = new Dictionary<PropertyKey, DependencyPropertyKind>();

	private static Hashtable _attachInfoMap = new Hashtable();

	public DependencyObjectProvider()
		: base(TypeDescriptor.GetProvider(typeof(DependencyObject)))
	{
		TypeDescriptor.Refreshed += delegate(RefreshEventArgs args)
		{
			if (args.TypeChanged != null && typeof(DependencyObject).IsAssignableFrom(args.TypeChanged))
			{
				ClearCache();
				DependencyObjectPropertyDescriptor.ClearCache();
				DPCustomTypeDescriptor.ClearCache();
				DependencyPropertyDescriptor.ClearCache();
			}
		};
	}

	public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
	{
		return new DPCustomTypeDescriptor(base.GetTypeDescriptor(objectType, instance), objectType, instance);
	}

	public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
	{
		ICustomTypeDescriptor customTypeDescriptor = base.GetExtendedTypeDescriptor(instance);
		if (instance != null && !(instance is Type))
		{
			customTypeDescriptor = new APCustomTypeDescriptor(customTypeDescriptor, instance);
		}
		return customTypeDescriptor;
	}

	public override IDictionary GetCache(object instance)
	{
		if (!(instance is DependencyObject dependencyObject))
		{
			return base.GetCache(instance);
		}
		IDictionary dictionary = _cacheSlot.GetValue(dependencyObject);
		if (dictionary == null && !dependencyObject.IsSealed)
		{
			dictionary = new Hashtable();
			_cacheSlot.SetValue(dependencyObject, dictionary);
		}
		return dictionary;
	}

	private static void ClearCache()
	{
		lock (_propertyMap)
		{
			_propertyMap.Clear();
		}
		lock (_propertyKindMap)
		{
			_propertyKindMap.Clear();
		}
		lock (_attachInfoMap)
		{
			_attachInfoMap.Clear();
		}
	}

	internal static AttachInfo GetAttachInfo(DependencyProperty dp)
	{
		AttachInfo attachInfo = (AttachInfo)_attachInfoMap[dp];
		if (attachInfo == null)
		{
			attachInfo = new AttachInfo(dp);
			lock (_attachInfoMap)
			{
				_attachInfoMap[dp] = attachInfo;
			}
		}
		return attachInfo;
	}

	internal static DependencyObjectPropertyDescriptor GetAttachedPropertyDescriptor(DependencyProperty dp, Type targetType)
	{
		PropertyKey key = new PropertyKey(targetType, dp);
		DependencyObjectPropertyDescriptor value;
		lock (_propertyMap)
		{
			if (!_propertyMap.TryGetValue(key, out value))
			{
				value = new DependencyObjectPropertyDescriptor(dp, targetType);
				_propertyMap[key] = value;
			}
		}
		return value;
	}

	internal static DependencyPropertyKind GetDependencyPropertyKind(DependencyProperty dp, Type targetType)
	{
		PropertyKey key = new PropertyKey(targetType, dp);
		DependencyPropertyKind value;
		lock (_propertyKindMap)
		{
			if (!_propertyKindMap.TryGetValue(key, out value))
			{
				value = new DependencyPropertyKind(dp, targetType);
				_propertyKindMap[key] = value;
			}
		}
		return value;
	}
}
