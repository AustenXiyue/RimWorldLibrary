using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Utility;

namespace System.Windows.Diagnostics;

public static class ResourceDictionaryDiagnostics
{
	internal class LookupResult
	{
		public StaticResourceExtension Requester { get; set; }

		public object Key { get; set; }

		public ResourceDictionary Dictionary { get; set; }

		public LookupResult(StaticResourceExtension requester)
		{
			Requester = requester;
		}
	}

	private static Dictionary<Uri, List<WeakReference<ResourceDictionary>>> _dictionariesFromUri;

	private static readonly object _dictionariesFromUriLock;

	private static List<object> IgnorableProperties;

	[ThreadStatic]
	private static Stack<LookupResult> _lookupResultStack;

	[ThreadStatic]
	private static Dictionary<WeakReferenceKey<StaticResourceExtension>, WeakReference<ResourceDictionary>> _resultCache;

	[ThreadStatic]
	private static DispatcherOperation _cleanupOperation;

	private static readonly ReadOnlyCollection<ResourceDictionaryInfo> EmptyResourceDictionaryInfos;

	public static IEnumerable<ResourceDictionaryInfo> ThemedResourceDictionaries
	{
		get
		{
			if (!IsEnabled)
			{
				return EmptyResourceDictionaryInfos;
			}
			return SystemResources.ThemedResourceDictionaries;
		}
	}

	public static IEnumerable<ResourceDictionaryInfo> GenericResourceDictionaries
	{
		get
		{
			if (!IsEnabled)
			{
				return EmptyResourceDictionaryInfos;
			}
			return SystemResources.GenericResourceDictionaries;
		}
	}

	private static IReadOnlyCollection<ResourceDictionary> EmptyResourceDictionaries => (IReadOnlyCollection<ResourceDictionary>)(object)Array.Empty<ResourceDictionary>();

	private static IReadOnlyCollection<FrameworkElement> EmptyFrameworkElementList => (IReadOnlyCollection<FrameworkElement>)(object)Array.Empty<FrameworkElement>();

	private static IReadOnlyCollection<FrameworkContentElement> EmptyFrameworkContentElementList => (IReadOnlyCollection<FrameworkContentElement>)(object)Array.Empty<FrameworkContentElement>();

	private static IReadOnlyCollection<Application> EmptyApplicationList => (IReadOnlyCollection<Application>)(object)Array.Empty<Application>();

	internal static bool HasStaticResourceResolvedListeners
	{
		get
		{
			if (IsEnabled)
			{
				return ResourceDictionaryDiagnostics.StaticResourceResolved != null;
			}
			return false;
		}
	}

	internal static bool IsEnabled { get; private set; }

	public static event EventHandler<ResourceDictionaryLoadedEventArgs> ThemedResourceDictionaryLoaded
	{
		add
		{
			if (IsEnabled)
			{
				SystemResources.ThemedDictionaryLoaded += value;
			}
		}
		remove
		{
			SystemResources.ThemedDictionaryLoaded -= value;
		}
	}

	public static event EventHandler<ResourceDictionaryUnloadedEventArgs> ThemedResourceDictionaryUnloaded
	{
		add
		{
			if (IsEnabled)
			{
				SystemResources.ThemedDictionaryUnloaded += value;
			}
		}
		remove
		{
			SystemResources.ThemedDictionaryUnloaded -= value;
		}
	}

	public static event EventHandler<ResourceDictionaryLoadedEventArgs> GenericResourceDictionaryLoaded
	{
		add
		{
			if (IsEnabled)
			{
				SystemResources.GenericDictionaryLoaded += value;
			}
		}
		remove
		{
			SystemResources.GenericDictionaryLoaded -= value;
		}
	}

	public static event EventHandler<StaticResourceResolvedEventArgs> StaticResourceResolved;

	static ResourceDictionaryDiagnostics()
	{
		_dictionariesFromUriLock = new object();
		IgnorableProperties = new List<object>();
		EmptyResourceDictionaryInfos = new List<ResourceDictionaryInfo>().AsReadOnly();
		IsEnabled = VisualDiagnostics.IsEnabled && VisualDiagnostics.IsEnvironmentVariableSet(null, "ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO");
		IgnorableProperties.Add(typeof(ResourceDictionary).GetProperty("DeferrableContent"));
	}

	public static IEnumerable<ResourceDictionary> GetResourceDictionariesForSource(Uri uri)
	{
		if (!IsEnabled || _dictionariesFromUri == null)
		{
			return EmptyResourceDictionaries;
		}
		lock (_dictionariesFromUriLock)
		{
			if (!_dictionariesFromUri.TryGetValue(uri, out var value) || value.Count == 0)
			{
				return EmptyResourceDictionaries;
			}
			List<ResourceDictionary> list = new List<ResourceDictionary>(value.Count);
			List<WeakReference<ResourceDictionary>> list2 = null;
			foreach (WeakReference<ResourceDictionary> item in value)
			{
				if (item.TryGetTarget(out var target))
				{
					list.Add(target);
					continue;
				}
				if (list2 == null)
				{
					list2 = new List<WeakReference<ResourceDictionary>>();
				}
				list2.Add(item);
			}
			if (list2 != null)
			{
				RemoveEntries(uri, value, list2);
			}
			return list.AsReadOnly();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void AddResourceDictionaryForUri(Uri uri, ResourceDictionary rd)
	{
		if (uri != null && IsEnabled)
		{
			AddResourceDictionaryForUriImpl(uri, rd);
		}
	}

	private static void AddResourceDictionaryForUriImpl(Uri uri, ResourceDictionary rd)
	{
		lock (_dictionariesFromUriLock)
		{
			if (_dictionariesFromUri == null)
			{
				_dictionariesFromUri = new Dictionary<Uri, List<WeakReference<ResourceDictionary>>>();
			}
			if (!_dictionariesFromUri.TryGetValue(uri, out var value))
			{
				value = new List<WeakReference<ResourceDictionary>>(1);
				_dictionariesFromUri.Add(uri, value);
			}
			value.Add(new WeakReference<ResourceDictionary>(rd));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void RemoveResourceDictionaryForUri(Uri uri, ResourceDictionary rd)
	{
		if (uri != null && IsEnabled)
		{
			RemoveResourceDictionaryForUriImpl(uri, rd);
		}
	}

	private static void RemoveResourceDictionaryForUriImpl(Uri uri, ResourceDictionary rd)
	{
		lock (_dictionariesFromUriLock)
		{
			if (_dictionariesFromUri == null || !_dictionariesFromUri.TryGetValue(uri, out var value))
			{
				return;
			}
			List<WeakReference<ResourceDictionary>> list = new List<WeakReference<ResourceDictionary>>();
			foreach (WeakReference<ResourceDictionary> item in value)
			{
				if (!item.TryGetTarget(out var target) || target == rd)
				{
					list.Add(item);
				}
			}
			RemoveEntries(uri, value, list);
		}
	}

	private static void RemoveEntries(Uri uri, List<WeakReference<ResourceDictionary>> list, List<WeakReference<ResourceDictionary>> toRemove)
	{
		foreach (WeakReference<ResourceDictionary> item in toRemove)
		{
			list.Remove(item);
		}
		if (list.Count == 0)
		{
			_dictionariesFromUri.Remove(uri);
		}
	}

	public static IEnumerable<FrameworkElement> GetFrameworkElementOwners(ResourceDictionary dictionary)
	{
		return GetOwners(dictionary.FrameworkElementOwners, EmptyFrameworkElementList);
	}

	public static IEnumerable<FrameworkContentElement> GetFrameworkContentElementOwners(ResourceDictionary dictionary)
	{
		return GetOwners(dictionary.FrameworkContentElementOwners, EmptyFrameworkContentElementList);
	}

	public static IEnumerable<Application> GetApplicationOwners(ResourceDictionary dictionary)
	{
		return GetOwners(dictionary.ApplicationOwners, EmptyApplicationList);
	}

	private static IEnumerable<T> GetOwners<T>(WeakReferenceList list, IEnumerable<T> emptyList) where T : DispatcherObject
	{
		if (!IsEnabled || list == null || list.Count == 0)
		{
			return emptyList;
		}
		List<T> list2 = new List<T>(list.Count);
		WeakReferenceListEnumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is T item)
			{
				list2.Add(item);
			}
		}
		return list2.AsReadOnly();
	}

	internal static bool ShouldIgnoreProperty(object targetProperty)
	{
		return IgnorableProperties.Contains(targetProperty);
	}

	internal static LookupResult RequestLookupResult(StaticResourceExtension requester)
	{
		if (_lookupResultStack == null)
		{
			_lookupResultStack = new Stack<LookupResult>();
		}
		LookupResult lookupResult = new LookupResult(requester);
		_lookupResultStack.Push(lookupResult);
		return lookupResult;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void RecordLookupResult(object key, ResourceDictionary rd)
	{
		if (IsEnabled && _lookupResultStack != null)
		{
			RecordLookupResultImpl(key, rd);
		}
	}

	private static void RecordLookupResultImpl(object key, ResourceDictionary rd)
	{
		if (_lookupResultStack.Count <= 0)
		{
			return;
		}
		LookupResult lookupResult = _lookupResultStack.Peek();
		if (object.Equals(lookupResult.Requester.ResourceKey, key))
		{
			if (lookupResult.Requester.GetType() == typeof(StaticResourceExtension))
			{
				lookupResult.Key = key;
				lookupResult.Dictionary = rd;
			}
			else
			{
				lookupResult.Key = key;
				lookupResult.Dictionary = rd;
			}
		}
	}

	internal static void RevertRequest(StaticResourceExtension requester, bool success)
	{
		LookupResult lookupResult = _lookupResultStack.Pop();
		if (success && !(lookupResult.Requester.GetType() == typeof(StaticResourceExtension)))
		{
			if (_resultCache == null)
			{
				_resultCache = new Dictionary<WeakReferenceKey<StaticResourceExtension>, WeakReference<ResourceDictionary>>();
			}
			WeakReferenceKey<StaticResourceExtension> key = new WeakReferenceKey<StaticResourceExtension>(requester);
			ResourceDictionary target = null;
			if (_resultCache.TryGetValue(key, out var value))
			{
				value.TryGetTarget(out target);
			}
			if (lookupResult.Dictionary != null)
			{
				_resultCache[key] = new WeakReference<ResourceDictionary>(lookupResult.Dictionary);
				return;
			}
			lookupResult.Key = requester.ResourceKey;
			lookupResult.Dictionary = target;
		}
	}

	internal static void OnStaticResourceResolved(object targetObject, object targetProperty, LookupResult result)
	{
		EventHandler<StaticResourceResolvedEventArgs> staticResourceResolved = ResourceDictionaryDiagnostics.StaticResourceResolved;
		if (staticResourceResolved != null && result.Dictionary != null)
		{
			staticResourceResolved(null, new StaticResourceResolvedEventArgs(targetObject, targetProperty, result.Dictionary, result.Key));
		}
		RequestCacheCleanup(targetObject);
	}

	private static void RequestCacheCleanup(object targetObject)
	{
		Dispatcher dispatcher;
		if (_resultCache != null && _cleanupOperation == null && targetObject is DispatcherObject dispatcherObject && (dispatcher = dispatcherObject.Dispatcher) != null)
		{
			_cleanupOperation = dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(DoCleanup));
		}
	}

	private static void DoCleanup()
	{
		_cleanupOperation = null;
		List<WeakReferenceKey<StaticResourceExtension>> list = null;
		foreach (KeyValuePair<WeakReferenceKey<StaticResourceExtension>, WeakReference<ResourceDictionary>> item in _resultCache)
		{
			if (item.Key.Item == null || !item.Value.TryGetTarget(out var _))
			{
				if (list == null)
				{
					list = new List<WeakReferenceKey<StaticResourceExtension>>();
				}
				list.Add(item.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (WeakReferenceKey<StaticResourceExtension> item2 in list)
		{
			_resultCache.Remove(item2);
		}
	}
}
