using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Baml2006;
using System.Windows.Controls.Primitives;
using System.Windows.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xaml;
using System.Xaml.Permissions;
using MS.Internal;
using MS.Internal.Ink;
using MS.Internal.Interop;
using MS.Internal.PresentationFramework;
using MS.Internal.WindowsBase;
using MS.Utility;
using MS.Win32;

namespace System.Windows;

internal static class SystemResources
{
	internal class ResourceDictionaries
	{
		private ResourceDictionary _genericDictionary;

		private ResourceDictionary _themedDictionary;

		private bool _genericLoaded;

		private bool _themedLoaded;

		private bool _preventReEnter;

		private bool _locationsLoaded;

		private string _assemblyName;

		private Assembly _assembly;

		private ResourceDictionaryLocation _genericLocation;

		private ResourceDictionaryLocation _themedLocation;

		private static string _themedResourceName;

		private Assembly _themedDictionaryAssembly;

		private Assembly _genericDictionaryAssembly;

		private Uri _themedDictionarySourceUri;

		private Uri _genericDictionarySourceUri;

		internal static string ThemedResourceName
		{
			get
			{
				string text = _themedResourceName;
				while (text == null)
				{
					text = UxThemeWrapper.ThemedResourceName;
					string text2 = Interlocked.CompareExchange(ref _themedResourceName, text, null);
					if (text2 != null && text2 != text)
					{
						_themedResourceName = null;
						text = null;
					}
				}
				return text;
			}
		}

		internal ResourceDictionaryInfo GenericDictionaryInfo => new ResourceDictionaryInfo(_assembly, _genericDictionaryAssembly, _genericDictionary, _genericDictionarySourceUri);

		internal ResourceDictionaryInfo ThemedDictionaryInfo => new ResourceDictionaryInfo(_assembly, _themedDictionaryAssembly, _themedDictionary, _themedDictionarySourceUri);

		internal ResourceDictionaries(Assembly assembly)
		{
			_assembly = assembly;
			_themedDictionaryAssembly = null;
			_themedDictionarySourceUri = null;
			_genericDictionaryAssembly = null;
			_genericDictionarySourceUri = null;
			if (assembly == PresentationFramework)
			{
				_assemblyName = "PresentationFramework";
				_genericDictionary = null;
				_genericLoaded = true;
				_genericLocation = ResourceDictionaryLocation.None;
				_themedLocation = ResourceDictionaryLocation.ExternalAssembly;
				_locationsLoaded = true;
			}
			else
			{
				_assemblyName = MS.Internal.PresentationFramework.SafeSecurityHelper.GetAssemblyPartialName(assembly);
			}
		}

		internal void ClearThemedDictionary()
		{
			ResourceDictionaryInfo themedDictionaryInfo = ThemedDictionaryInfo;
			_themedLoaded = false;
			_themedDictionary = null;
			_themedDictionaryAssembly = null;
			_themedDictionarySourceUri = null;
			if (themedDictionaryInfo.ResourceDictionary != null)
			{
				SystemResources.ThemedDictionaryUnloaded?.Invoke(null, new ResourceDictionaryUnloadedEventArgs(themedDictionaryInfo));
			}
		}

		internal ResourceDictionary LoadThemedDictionary(bool isTraceEnabled)
		{
			if (!_themedLoaded)
			{
				LoadDictionaryLocations();
				if (_preventReEnter || _themedLocation == ResourceDictionaryLocation.None)
				{
					return null;
				}
				IsSystemResourcesParsing = true;
				_preventReEnter = true;
				try
				{
					ResourceDictionary resourceDictionary = null;
					bool flag = _themedLocation == ResourceDictionaryLocation.ExternalAssembly;
					string assemblyName;
					if (flag)
					{
						LoadExternalAssembly(classic: false, generic: false, out _themedDictionaryAssembly, out assemblyName);
					}
					else
					{
						_themedDictionaryAssembly = _assembly;
						assemblyName = _assemblyName;
					}
					if (_themedDictionaryAssembly != null)
					{
						resourceDictionary = LoadDictionary(_themedDictionaryAssembly, assemblyName, ThemedResourceName, isTraceEnabled, out _themedDictionarySourceUri);
						if (resourceDictionary == null && !flag)
						{
							LoadExternalAssembly(classic: false, generic: false, out _themedDictionaryAssembly, out assemblyName);
							if (_themedDictionaryAssembly != null)
							{
								resourceDictionary = LoadDictionary(_themedDictionaryAssembly, assemblyName, ThemedResourceName, isTraceEnabled, out _themedDictionarySourceUri);
							}
						}
					}
					if (resourceDictionary == null && UxThemeWrapper.IsActive)
					{
						if (flag)
						{
							LoadExternalAssembly(classic: true, generic: false, out _themedDictionaryAssembly, out assemblyName);
						}
						else
						{
							_themedDictionaryAssembly = _assembly;
							assemblyName = _assemblyName;
						}
						if (_themedDictionaryAssembly != null)
						{
							resourceDictionary = LoadDictionary(_themedDictionaryAssembly, assemblyName, "themes/classic", isTraceEnabled, out _themedDictionarySourceUri);
						}
					}
					_themedDictionary = resourceDictionary;
					_themedLoaded = true;
					if (_themedDictionary != null)
					{
						SystemResources.ThemedDictionaryLoaded?.Invoke(null, new ResourceDictionaryLoadedEventArgs(ThemedDictionaryInfo));
					}
				}
				finally
				{
					_preventReEnter = false;
					IsSystemResourcesParsing = false;
				}
			}
			return _themedDictionary;
		}

		internal ResourceDictionary LoadGenericDictionary(bool isTraceEnabled)
		{
			if (!_genericLoaded)
			{
				LoadDictionaryLocations();
				if (_preventReEnter || _genericLocation == ResourceDictionaryLocation.None)
				{
					return null;
				}
				IsSystemResourcesParsing = true;
				_preventReEnter = true;
				try
				{
					ResourceDictionary genericDictionary = null;
					string assemblyName;
					if (_genericLocation == ResourceDictionaryLocation.ExternalAssembly)
					{
						LoadExternalAssembly(classic: false, generic: true, out _genericDictionaryAssembly, out assemblyName);
					}
					else
					{
						_genericDictionaryAssembly = _assembly;
						assemblyName = _assemblyName;
					}
					if (_genericDictionaryAssembly != null)
					{
						genericDictionary = LoadDictionary(_genericDictionaryAssembly, assemblyName, "themes/generic", isTraceEnabled, out _genericDictionarySourceUri);
					}
					_genericDictionary = genericDictionary;
					_genericLoaded = true;
					if (_genericDictionary != null)
					{
						SystemResources.GenericDictionaryLoaded?.Invoke(null, new ResourceDictionaryLoadedEventArgs(GenericDictionaryInfo));
					}
				}
				finally
				{
					_preventReEnter = false;
					IsSystemResourcesParsing = false;
				}
			}
			return _genericDictionary;
		}

		private void LoadDictionaryLocations()
		{
			if (!_locationsLoaded)
			{
				ThemeInfoAttribute themeInfoAttribute = ThemeInfoAttribute.FromAssembly(_assembly);
				if (themeInfoAttribute != null)
				{
					_themedLocation = themeInfoAttribute.ThemeDictionaryLocation;
					_genericLocation = themeInfoAttribute.GenericDictionaryLocation;
				}
				else
				{
					_themedLocation = ResourceDictionaryLocation.None;
					_genericLocation = ResourceDictionaryLocation.None;
				}
				_locationsLoaded = true;
			}
		}

		private void LoadExternalAssembly(bool classic, bool generic, out Assembly assembly, out string assemblyName)
		{
			StringBuilder stringBuilder = new StringBuilder(_assemblyName.Length + 10);
			stringBuilder.Append(_assemblyName);
			stringBuilder.Append('.');
			if (generic)
			{
				stringBuilder.Append("generic");
			}
			else if (classic)
			{
				stringBuilder.Append("classic");
			}
			else
			{
				stringBuilder.Append(UxThemeWrapper.ThemeName);
			}
			assemblyName = stringBuilder.ToString();
			string fullAssemblyNameFromPartialName = MS.Internal.PresentationFramework.SafeSecurityHelper.GetFullAssemblyNameFromPartialName(_assembly, assemblyName);
			assembly = null;
			try
			{
				assembly = Assembly.Load(fullAssemblyNameFromPartialName);
			}
			catch (FileNotFoundException)
			{
			}
			catch (BadImageFormatException)
			{
			}
			if (_assemblyName == "PresentationFramework" && assembly != null)
			{
				Type type = assembly.GetType("Microsoft.Windows.Themes.KnownTypeHelper");
				if (type != null)
				{
					MS.Internal.WindowsBase.SecurityHelper.RunClassConstructor(type);
				}
			}
		}

		private ResourceDictionary LoadDictionary(Assembly assembly, string assemblyName, string resourceName, bool isTraceEnabled, out Uri dictionarySourceUri)
		{
			ResourceDictionary resourceDictionary = null;
			dictionarySourceUri = null;
			ResourceManager resourceManager = new ResourceManager(assemblyName + ".g", assembly);
			resourceName += ".baml";
			Stream stream = null;
			try
			{
				stream = resourceManager.GetStream(resourceName, CultureInfo.CurrentUICulture);
			}
			catch (MissingManifestResourceException)
			{
			}
			catch (MissingSatelliteAssemblyException)
			{
			}
			catch (InvalidOperationException)
			{
			}
			if (stream != null)
			{
				Baml2006ReaderSettings baml2006ReaderSettings = new Baml2006ReaderSettings();
				baml2006ReaderSettings.OwnsStream = true;
				baml2006ReaderSettings.LocalAssembly = assembly;
				Baml2006ReaderInternal baml2006ReaderInternal = new Baml2006ReaderInternal(stream, new Baml2006SchemaContext(baml2006ReaderSettings.LocalAssembly), baml2006ReaderSettings);
				XamlObjectWriterSettings xamlObjectWriterSettings = System.Windows.Markup.XamlReader.CreateObjectWriterSettingsForBaml();
				if (assembly != null)
				{
					xamlObjectWriterSettings.AccessLevel = XamlAccessLevel.AssemblyAccessTo(assembly);
					AssemblyName assemblyName2 = new AssemblyName(assembly.FullName);
					Uri result = null;
					if (Uri.TryCreate($"pack://application:,,,/{assemblyName2.Name};v{assemblyName2.Version.ToString()};component/{resourceName}", UriKind.Absolute, out result))
					{
						if (XamlSourceInfoHelper.IsXamlSourceInfoEnabled)
						{
							xamlObjectWriterSettings.SourceBamlUri = result;
						}
						dictionarySourceUri = result;
					}
				}
				XamlObjectWriter xamlObjectWriter = new XamlObjectWriter(baml2006ReaderInternal.SchemaContext, xamlObjectWriterSettings);
				XamlServices.Transform(baml2006ReaderInternal, xamlObjectWriter);
				resourceDictionary = (ResourceDictionary)xamlObjectWriter.Result;
				if (isTraceEnabled && resourceDictionary != null)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceBamlAssembly, EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, assemblyName);
				}
			}
			return resourceDictionary;
		}

		internal static void OnThemeChanged()
		{
			_themedResourceName = null;
		}
	}

	[ThreadStatic]
	private static int _parsing;

	[ThreadStatic]
	private static List<DpiUtil.HwndDpiInfo> _dpiAwarenessContextAndDpis;

	[ThreadStatic]
	private static Dictionary<DpiUtil.HwndDpiInfo, SecurityCriticalDataClass<HwndWrapper>> _hwndNotify;

	[ThreadStatic]
	private static Dictionary<DpiUtil.HwndDpiInfo, HwndWrapperHook> _hwndNotifyHook;

	private static Hashtable _resourceCache = new Hashtable();

	private static DTypeMap _themeStyleCache = new DTypeMap(100);

	private static Dictionary<Assembly, ResourceDictionaries> _dictionaries;

	private static object _specialNull = new object();

	internal const string GenericResourceName = "themes/generic";

	internal const string ClassicResourceName = "themes/classic";

	private static Assembly _mscorlib;

	private static Assembly _presentationFramework;

	private static Assembly _presentationCore;

	private static Assembly _windowsBase;

	internal const string PresentationFrameworkName = "PresentationFramework";

	internal static bool SystemResourcesHaveChanged;

	[ThreadStatic]
	internal static bool SystemResourcesAreChanging;

	internal static ReadOnlyCollection<ResourceDictionaryInfo> ThemedResourceDictionaries
	{
		get
		{
			List<ResourceDictionaryInfo> list = new List<ResourceDictionaryInfo>();
			if (_dictionaries != null)
			{
				lock (ThemeDictionaryLock)
				{
					if (_dictionaries != null)
					{
						foreach (KeyValuePair<Assembly, ResourceDictionaries> dictionary in _dictionaries)
						{
							ResourceDictionaryInfo themedDictionaryInfo = dictionary.Value.ThemedDictionaryInfo;
							if (themedDictionaryInfo.ResourceDictionary != null)
							{
								list.Add(themedDictionaryInfo);
							}
						}
					}
				}
			}
			return list.AsReadOnly();
		}
	}

	internal static ReadOnlyCollection<ResourceDictionaryInfo> GenericResourceDictionaries
	{
		get
		{
			List<ResourceDictionaryInfo> list = new List<ResourceDictionaryInfo>();
			if (_dictionaries != null)
			{
				lock (ThemeDictionaryLock)
				{
					if (_dictionaries != null)
					{
						foreach (KeyValuePair<Assembly, ResourceDictionaries> dictionary in _dictionaries)
						{
							ResourceDictionaryInfo genericDictionaryInfo = dictionary.Value.GenericDictionaryInfo;
							if (genericDictionaryInfo.ResourceDictionary != null)
							{
								list.Add(genericDictionaryInfo);
							}
						}
					}
				}
			}
			return list.AsReadOnly();
		}
	}

	private static Assembly MsCorLib
	{
		get
		{
			if (_mscorlib == null)
			{
				_mscorlib = typeof(string).Assembly;
			}
			return _mscorlib;
		}
	}

	private static Assembly PresentationFramework
	{
		get
		{
			if (_presentationFramework == null)
			{
				_presentationFramework = typeof(FrameworkElement).Assembly;
			}
			return _presentationFramework;
		}
	}

	private static Assembly PresentationCore
	{
		get
		{
			if (_presentationCore == null)
			{
				_presentationCore = typeof(UIElement).Assembly;
			}
			return _presentationCore;
		}
	}

	private static Assembly WindowsBase
	{
		get
		{
			if (_windowsBase == null)
			{
				_windowsBase = typeof(DependencyObject).Assembly;
			}
			return _windowsBase;
		}
	}

	internal static bool IsSystemResourcesParsing
	{
		get
		{
			return _parsing > 0;
		}
		set
		{
			if (value)
			{
				_parsing++;
			}
			else
			{
				_parsing--;
			}
		}
	}

	internal static object ThemeDictionaryLock => _resourceCache.SyncRoot;

	private static DpiAwarenessContextValue ProcessDpiAwarenessContextValue
	{
		get
		{
			if (HwndTarget.IsProcessUnaware == true)
			{
				return DpiAwarenessContextValue.Unaware;
			}
			if (HwndTarget.IsProcessSystemAware == true)
			{
				return DpiAwarenessContextValue.SystemAware;
			}
			if (HwndTarget.IsProcessPerMonitorDpiAware == true)
			{
				return DpiAwarenessContextValue.PerMonitorAware;
			}
			return DpiUtil.GetProcessDpiAwarenessContextValue(IntPtr.Zero);
		}
	}

	private static bool IsPerMonitorDpiScalingActive
	{
		get
		{
			if (HwndTarget.IsPerMonitorDpiScalingEnabled)
			{
				if (ProcessDpiAwarenessContextValue != DpiAwarenessContextValue.PerMonitorAware)
				{
					return ProcessDpiAwarenessContextValue == DpiAwarenessContextValue.PerMonitorAwareVersion2;
				}
				return true;
			}
			return false;
		}
	}

	private static HwndWrapper Hwnd
	{
		get
		{
			EnsureResourceChangeListener();
			DpiUtil.HwndDpiInfo key = _hwndNotify.Keys.FirstOrDefault((DpiUtil.HwndDpiInfo hwndDpiContext) => hwndDpiContext.DpiAwarenessContextValue == ProcessDpiAwarenessContextValue);
			return _hwndNotify[key].Value;
		}
	}

	internal static event EventHandler<ResourceDictionaryLoadedEventArgs> ThemedDictionaryLoaded;

	internal static event EventHandler<ResourceDictionaryUnloadedEventArgs> ThemedDictionaryUnloaded;

	internal static event EventHandler<ResourceDictionaryLoadedEventArgs> GenericDictionaryLoaded;

	internal static object FindThemeStyle(DependencyObjectType key)
	{
		object obj = _themeStyleCache[key];
		if (obj != null)
		{
			if (obj == _specialNull)
			{
				return null;
			}
			return obj;
		}
		obj = FindResourceInternal(key.SystemType);
		lock (ThemeDictionaryLock)
		{
			if (obj != null)
			{
				_themeStyleCache[key] = obj;
			}
			else
			{
				_themeStyleCache[key] = _specialNull;
			}
		}
		return obj;
	}

	internal static object FindResourceInternal(object key)
	{
		return FindResourceInternal(key, allowDeferredResourceReference: false, mustReturnDeferredResourceReference: false);
	}

	internal static object FindResourceInternal(object key, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		EnsureResourceChangeListener();
		object resource = null;
		Type type = null;
		ResourceKey resourceKey = null;
		bool flag = EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
		type = key as Type;
		resourceKey = ((type == null) ? (key as ResourceKey) : null);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceFindBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, (key == null) ? "null" : key.ToString());
		}
		if (type == null && resourceKey == null)
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceFindEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
			}
			return null;
		}
		if (!FindCachedResource(key, ref resource, mustReturnDeferredResourceReference))
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceCacheMiss, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
			}
			lock (ThemeDictionaryLock)
			{
				bool canCache = true;
				SystemResourceKey systemResourceKey = ((resourceKey != null) ? (resourceKey as SystemResourceKey) : null);
				if (systemResourceKey != null)
				{
					resource = (mustReturnDeferredResourceReference ? new DeferredResourceReferenceHolder(systemResourceKey, systemResourceKey.Resource) : systemResourceKey.Resource);
					if (flag)
					{
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceStock, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, systemResourceKey.ToString());
					}
				}
				else
				{
					resource = FindDictionaryResource(key, type, resourceKey, flag, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
				}
				if ((canCache && !allowDeferredResourceReference) || resource == null)
				{
					CacheResource(key, resource, flag);
				}
			}
		}
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceFindEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
		}
		return resource;
	}

	internal static void CacheResource(object key, object resource, bool isTraceEnabled)
	{
		if (resource != null)
		{
			_resourceCache[key] = resource;
			if (isTraceEnabled)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceCacheValue, EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
			}
		}
		else
		{
			_resourceCache[key] = _specialNull;
			if (isTraceEnabled)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientResourceCacheNull, EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose);
			}
		}
	}

	private static bool FindCachedResource(object key, ref object resource, bool mustReturnDeferredResourceReference)
	{
		resource = _resourceCache[key];
		bool num = resource != null;
		if (resource == _specialNull)
		{
			resource = null;
		}
		else if (resource is DispatcherObject dispatcherObject)
		{
			dispatcherObject.VerifyAccess();
		}
		if (num && mustReturnDeferredResourceReference)
		{
			resource = new DeferredResourceReferenceHolder(key, resource);
		}
		return num;
	}

	private static object FindDictionaryResource(object key, Type typeKey, ResourceKey resourceKey, bool isTraceEnabled, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, out bool canCache)
	{
		canCache = true;
		object obj = null;
		Assembly assembly = ((typeKey != null) ? typeKey.Assembly : resourceKey.Assembly);
		if (assembly == null || IgnoreAssembly(assembly))
		{
			return null;
		}
		ResourceDictionaries resourceDictionaries = EnsureDictionarySlot(assembly);
		ResourceDictionary resourceDictionary = resourceDictionaries.LoadThemedDictionary(isTraceEnabled);
		if (resourceDictionary != null)
		{
			obj = LookupResourceInDictionary(resourceDictionary, key, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		if (obj == null)
		{
			resourceDictionary = resourceDictionaries.LoadGenericDictionary(isTraceEnabled);
			if (resourceDictionary != null)
			{
				obj = LookupResourceInDictionary(resourceDictionary, key, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
			}
		}
		if (obj != null)
		{
			Freeze(obj);
		}
		return obj;
	}

	private static object LookupResourceInDictionary(ResourceDictionary dictionary, object key, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, out bool canCache)
	{
		object obj = null;
		IsSystemResourcesParsing = true;
		try
		{
			return dictionary.FetchResource(key, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		finally
		{
			IsSystemResourcesParsing = false;
		}
	}

	private static void Freeze(object resource)
	{
		if (resource is Freezable { IsFrozen: false } freezable)
		{
			freezable.Freeze();
		}
	}

	private static ResourceDictionaries EnsureDictionarySlot(Assembly assembly)
	{
		ResourceDictionaries value = null;
		if (_dictionaries != null)
		{
			_dictionaries.TryGetValue(assembly, out value);
		}
		else
		{
			_dictionaries = new Dictionary<Assembly, ResourceDictionaries>(1);
		}
		if (value == null)
		{
			value = new ResourceDictionaries(assembly);
			_dictionaries.Add(assembly, value);
		}
		return value;
	}

	private static bool IgnoreAssembly(Assembly assembly)
	{
		if (!(assembly == MsCorLib) && !(assembly == PresentationCore))
		{
			return assembly == WindowsBase;
		}
		return true;
	}

	private static void EnsureResourceChangeListener()
	{
		if (_hwndNotify == null || _hwndNotifyHook == null || _hwndNotify.Count == 0 || _hwndNotify.Keys.FirstOrDefault((DpiUtil.HwndDpiInfo hwndDpiContext) => hwndDpiContext.DpiAwarenessContextValue == ProcessDpiAwarenessContextValue) == null)
		{
			_hwndNotify = new Dictionary<DpiUtil.HwndDpiInfo, SecurityCriticalDataClass<HwndWrapper>>();
			_hwndNotifyHook = new Dictionary<DpiUtil.HwndDpiInfo, HwndWrapperHook>();
			_dpiAwarenessContextAndDpis = new List<DpiUtil.HwndDpiInfo>();
			DpiUtil.HwndDpiInfo item = CreateResourceChangeListenerWindow(ProcessDpiAwarenessContextValue, 0, 0, "EnsureResourceChangeListener");
			_dpiAwarenessContextAndDpis.Add(item);
		}
	}

	private static bool EnsureResourceChangeListener(DpiUtil.HwndDpiInfo hwndDpiInfo)
	{
		EnsureResourceChangeListener();
		if (hwndDpiInfo.DpiAwarenessContextValue == DpiAwarenessContextValue.Invalid)
		{
			return false;
		}
		if (!_hwndNotify.ContainsKey(hwndDpiInfo) && CreateResourceChangeListenerWindow(hwndDpiInfo.DpiAwarenessContextValue, hwndDpiInfo.ContainingMonitorScreenRect.left, hwndDpiInfo.ContainingMonitorScreenRect.top, "EnsureResourceChangeListener") == hwndDpiInfo && !_dpiAwarenessContextAndDpis.Contains(hwndDpiInfo))
		{
			_dpiAwarenessContextAndDpis.Add(hwndDpiInfo);
		}
		return _hwndNotify.ContainsKey(hwndDpiInfo);
	}

	private static DpiUtil.HwndDpiInfo CreateResourceChangeListenerWindow(DpiAwarenessContextValue dpiContextValue, int x = 0, int y = 0, [CallerMemberName] string callerName = "")
	{
		using (DpiUtil.WithDpiAwarenessContext(dpiContextValue))
		{
			HwndWrapper hwndWrapper = new HwndWrapper(0, -2013265920, 0, x, y, 0, 0, "SystemResourceNotifyWindow", IntPtr.Zero, null);
			DpiUtil.HwndDpiInfo hwndDpiInfo = (IsPerMonitorDpiScalingActive ? DpiUtil.GetExtendedDpiInfoForWindow(hwndWrapper.Handle) : new DpiUtil.HwndDpiInfo(dpiContextValue, GetDpiScaleForUnawareOrSystemAwareContext(dpiContextValue)));
			_hwndNotify[hwndDpiInfo] = new SecurityCriticalDataClass<HwndWrapper>(hwndWrapper);
			_hwndNotify[hwndDpiInfo].Value.Dispatcher.ShutdownFinished += OnShutdownFinished;
			_hwndNotifyHook[hwndDpiInfo] = SystemThemeFilterMessage;
			_hwndNotify[hwndDpiInfo].Value.AddHook(_hwndNotifyHook[hwndDpiInfo]);
			return hwndDpiInfo;
		}
	}

	private static void OnShutdownFinished(object sender, EventArgs args)
	{
		if (_hwndNotify != null && _hwndNotify.Count != 0)
		{
			foreach (DpiUtil.HwndDpiInfo dpiAwarenessContextAndDpi in _dpiAwarenessContextAndDpis)
			{
				_hwndNotify[dpiAwarenessContextAndDpi].Value.Dispose();
				_hwndNotifyHook[dpiAwarenessContextAndDpi] = null;
			}
		}
		_hwndNotify?.Clear();
		_hwndNotify = null;
		_hwndNotifyHook?.Clear();
		_hwndNotifyHook = null;
	}

	private static DpiScale2 GetDpiScaleForUnawareOrSystemAwareContext(DpiAwarenessContextValue dpiContextValue)
	{
		DpiScale2 dpiScale = null;
		if (dpiContextValue != DpiAwarenessContextValue.SystemAware && dpiContextValue == DpiAwarenessContextValue.Unaware)
		{
			return DpiScale2.FromPixelsPerInch(96.0, 96.0);
		}
		return DpiUtil.GetSystemDpi();
	}

	private static void OnThemeChanged()
	{
		ResourceDictionaries.OnThemeChanged();
		UxThemeWrapper.OnThemeChanged();
		ThemeDictionaryExtension.OnThemeChanged();
		lock (ThemeDictionaryLock)
		{
			_resourceCache.Clear();
			_themeStyleCache.Clear();
			if (_dictionaries == null)
			{
				return;
			}
			foreach (ResourceDictionaries value in _dictionaries.Values)
			{
				value.ClearThemedDictionary();
			}
		}
	}

	private static void OnSystemValueChanged()
	{
		lock (ThemeDictionaryLock)
		{
			List<SystemResourceKey> list = new List<SystemResourceKey>();
			foreach (object key in _resourceCache.Keys)
			{
				if (key is SystemResourceKey item)
				{
					list.Add(item);
				}
			}
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				_resourceCache.Remove(list[i]);
			}
		}
	}

	private static object InvalidateTreeResources(object args)
	{
		object[] array = (object[])args;
		PresentationSource presentationSource = (PresentationSource)array[0];
		if (!presentationSource.IsDisposed && presentationSource.RootVisual is FrameworkElement frameworkElement)
		{
			if ((bool)array[1])
			{
				TreeWalkHelper.InvalidateOnResourcesChange(frameworkElement, null, ResourcesChangeInfo.SysColorsOrSettingsChangeInfo);
			}
			else
			{
				TreeWalkHelper.InvalidateOnResourcesChange(frameworkElement, null, ResourcesChangeInfo.ThemeChangeInfo);
			}
			KeyboardNavigation.AlwaysShowFocusVisual = SystemParameters.KeyboardCues;
			frameworkElement.CoerceValue(KeyboardNavigation.ShowKeyboardCuesProperty);
			SystemResourcesAreChanging = true;
			frameworkElement.CoerceValue(TextElement.FontFamilyProperty);
			frameworkElement.CoerceValue(TextElement.FontSizeProperty);
			frameworkElement.CoerceValue(TextElement.FontStyleProperty);
			frameworkElement.CoerceValue(TextElement.FontWeightProperty);
			SystemResourcesAreChanging = false;
			if (frameworkElement is PopupRoot { Parent: not null } popupRoot)
			{
				popupRoot.Parent.CoerceValue(Popup.HasDropShadowProperty);
			}
		}
		return null;
	}

	private static void InvalidateTabletDevices(WindowMessage msg, nint wParam, nint lParam)
	{
		if (StylusLogic.IsStylusAndTouchSupportEnabled && StylusLogic.IsInstantiated && _hwndNotify != null && _hwndNotify.Count != 0 && Hwnd.Dispatcher?.InputManager != null)
		{
			StylusLogic.CurrentStylusLogic.HandleMessage(msg, wParam, lParam);
		}
	}

	private static void InvalidateResources(bool isSysColorsOrSettingsChange)
	{
		SystemResourcesHaveChanged = true;
		Dispatcher dispatcher = (isSysColorsOrSettingsChange ? null : Dispatcher.FromThread(Thread.CurrentThread));
		if (!(dispatcher != null || isSysColorsOrSettingsChange))
		{
			return;
		}
		WeakReferenceListEnumerator enumerator = PresentationSource.CriticalCurrentSources.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PresentationSource presentationSource = (PresentationSource)enumerator.Current;
			if (!presentationSource.IsDisposed && (isSysColorsOrSettingsChange || presentationSource.Dispatcher == dispatcher))
			{
				presentationSource.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(InvalidateTreeResources), new object[2] { presentationSource, isSysColorsOrSettingsChange });
			}
		}
	}

	private static nint SystemThemeFilterMessage(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		switch (msg)
		{
		case 537:
			InvalidateTabletDevices((WindowMessage)msg, wParam, lParam);
			if (SystemParameters.InvalidateDeviceDependentCache())
			{
				OnSystemValueChanged();
				InvalidateResources(isSysColorsOrSettingsChange: true);
			}
			break;
		case 126:
			InvalidateTabletDevices((WindowMessage)msg, wParam, lParam);
			if (SystemParameters.InvalidateDisplayDependentCache())
			{
				OnSystemValueChanged();
				InvalidateResources(isSysColorsOrSettingsChange: true);
			}
			break;
		case 536:
			if (MS.Win32.NativeMethods.IntPtrToInt32(wParam) == 10 && SystemParameters.InvalidatePowerDependentCache())
			{
				OnSystemValueChanged();
				InvalidateResources(isSysColorsOrSettingsChange: true);
			}
			break;
		case 794:
			SystemColors.InvalidateCache();
			SystemParameters.InvalidateCache();
			SystemParameters.InvalidateDerivedThemeRelatedProperties();
			OnThemeChanged();
			InvalidateResources(isSysColorsOrSettingsChange: false);
			break;
		case 21:
			if (SystemColors.InvalidateCache())
			{
				OnSystemValueChanged();
				InvalidateResources(isSysColorsOrSettingsChange: true);
			}
			break;
		case 26:
			InvalidateTabletDevices((WindowMessage)msg, wParam, lParam);
			if (SystemParameters.InvalidateCache((int)wParam))
			{
				OnSystemValueChanged();
				InvalidateResources(isSysColorsOrSettingsChange: true);
				HighContrastHelper.OnSettingChanged();
			}
			SystemParameters.InvalidateWindowFrameThicknessProperties();
			break;
		case 712:
			InvalidateTabletDevices((WindowMessage)msg, wParam, lParam);
			break;
		case 713:
			InvalidateTabletDevices((WindowMessage)msg, wParam, lParam);
			break;
		case 798:
		case 799:
			SystemParameters.InvalidateIsGlassEnabled();
			break;
		case 800:
			SystemParameters.InvalidateWindowGlassColorizationProperties();
			break;
		}
		return IntPtr.Zero;
	}

	internal static bool ClearBitArray(BitArray cacheValid)
	{
		bool result = false;
		for (int i = 0; i < cacheValid.Count; i++)
		{
			if (ClearSlot(cacheValid, i))
			{
				result = true;
			}
		}
		return result;
	}

	internal static bool ClearSlot(BitArray cacheValid, int slot)
	{
		if (cacheValid[slot])
		{
			cacheValid[slot] = false;
			return true;
		}
		return false;
	}

	internal static HwndWrapper GetDpiAwarenessCompatibleNotificationWindow(HandleRef hwnd)
	{
		DpiAwarenessContextValue processDpiAwarenessContextValue = ProcessDpiAwarenessContextValue;
		DpiUtil.HwndDpiInfo hwndDpiInfo = (IsPerMonitorDpiScalingActive ? DpiUtil.GetExtendedDpiInfoForWindow(hwnd.Handle, fallbackToNearestMonitorHeuristic: true) : new DpiUtil.HwndDpiInfo(processDpiAwarenessContextValue, GetDpiScaleForUnawareOrSystemAwareContext(processDpiAwarenessContextValue)));
		if (EnsureResourceChangeListener(hwndDpiInfo))
		{
			return _hwndNotify[hwndDpiInfo].Value;
		}
		return null;
	}

	internal static void DelayHwndShutdown()
	{
		if (_hwndNotify != null && _hwndNotify.Count != 0)
		{
			Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
			currentDispatcher.ShutdownFinished -= OnShutdownFinished;
			currentDispatcher.ShutdownFinished += OnShutdownFinished;
		}
	}
}
