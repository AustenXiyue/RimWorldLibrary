using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using MS.Win32;

namespace System.Windows;

/// <summary>Implements a markup extension that enables application authors to customize control styles based on the current system theme.</summary>
[MarkupExtensionReturnType(typeof(Uri))]
public class ThemeDictionaryExtension : MarkupExtension
{
	private class ThemeDictionaryInfo
	{
		public WeakReference DictionaryReference;

		public string AssemblyName;
	}

	private string _assemblyName;

	private static PropertyInfo _sourceProperty;

	[ThreadStatic]
	private static List<ThemeDictionaryInfo> _themeDictionaryInfos;

	/// <summary>Gets or sets a string setting a particular naming convention to identify which dictionary applies for a particular theme. </summary>
	/// <returns>The assembly name string.</returns>
	public string AssemblyName
	{
		get
		{
			return _assemblyName;
		}
		set
		{
			_assemblyName = value;
		}
	}

	private static PropertyInfo SourceProperty
	{
		get
		{
			if (_sourceProperty == null)
			{
				_sourceProperty = typeof(ResourceDictionary).GetProperty("Source");
			}
			return _sourceProperty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ThemeDictionaryExtension" /> class.</summary>
	public ThemeDictionaryExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ThemeDictionaryExtension" /> class, using the specified assembly name.</summary>
	/// <param name="assemblyName">The assembly name string.</param>
	public ThemeDictionaryExtension(string assemblyName)
	{
		if (assemblyName != null)
		{
			_assemblyName = assemblyName;
			return;
		}
		throw new ArgumentNullException("assemblyName");
	}

	/// <summary>Returns an object that should be set on the property where this extension is applied. For <see cref="T:System.Windows.ThemeDictionaryExtension" />, this is the URI value for a particular theme dictionary extension.</summary>
	/// <returns>The object value to set on the property where the extension is applied. </returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension. This service is expected to provide results for <see cref="T:System.Windows.Markup.IXamlTypeResolver" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.ThemeDictionaryExtension.AssemblyName" /> property is null. You must set this value during construction or before using the <see cref="M:System.Windows.ThemeDictionaryExtension.ProvideValue(System.IServiceProvider)" />  method.-or-<paramref name="serviceProvide" />r is null or does not provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver" />.-or-<paramref name="serviceProvider" /> specifies a target type that does not match <see cref="P:System.Windows.ResourceDictionary.Source" />.</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (string.IsNullOrEmpty(AssemblyName))
		{
			throw new InvalidOperationException(SR.ThemeDictionaryExtension_Name);
		}
		IProvideValueTarget obj = (serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget) ?? throw new InvalidOperationException(SR.Format(SR.MarkupExtensionNoContext, GetType().Name, "IProvideValueTarget"));
		object targetObject = obj.TargetObject;
		object targetProperty = obj.TargetProperty;
		ResourceDictionary obj2 = targetObject as ResourceDictionary;
		PropertyInfo propertyInfo = targetProperty as PropertyInfo;
		if (obj2 == null || (targetProperty != null && propertyInfo != SourceProperty))
		{
			throw new InvalidOperationException(SR.ThemeDictionaryExtension_Source);
		}
		Register(obj2, _assemblyName);
		obj2.IsSourcedFromThemeDictionary = true;
		return GenerateUri(_assemblyName, SystemResources.ResourceDictionaries.ThemedResourceName, UxThemeWrapper.ThemeName);
	}

	private static Uri GenerateUri(string assemblyName, string resourceName, string themeName)
	{
		StringBuilder stringBuilder = new StringBuilder(assemblyName.Length + 50);
		stringBuilder.Append('/');
		stringBuilder.Append(assemblyName);
		if (assemblyName.Equals("PresentationFramework", StringComparison.OrdinalIgnoreCase))
		{
			stringBuilder.Append('.');
			stringBuilder.Append(themeName);
		}
		stringBuilder.Append(";component/");
		stringBuilder.Append(resourceName);
		stringBuilder.Append(".xaml");
		return new Uri(stringBuilder.ToString(), UriKind.RelativeOrAbsolute);
	}

	internal static Uri GenerateFallbackUri(ResourceDictionary dictionary, string resourceName)
	{
		for (int i = 0; i < _themeDictionaryInfos.Count; i++)
		{
			ThemeDictionaryInfo themeDictionaryInfo = _themeDictionaryInfos[i];
			if (!themeDictionaryInfo.DictionaryReference.IsAlive)
			{
				_themeDictionaryInfos.RemoveAt(i);
				i--;
			}
			else if ((ResourceDictionary)themeDictionaryInfo.DictionaryReference.Target == dictionary)
			{
				string themeName = resourceName.Split('/')[1];
				return GenerateUri(themeDictionaryInfo.AssemblyName, resourceName, themeName);
			}
		}
		return null;
	}

	private static void Register(ResourceDictionary dictionary, string assemblyName)
	{
		if (_themeDictionaryInfos == null)
		{
			_themeDictionaryInfos = new List<ThemeDictionaryInfo>();
		}
		ThemeDictionaryInfo themeDictionaryInfo;
		for (int i = 0; i < _themeDictionaryInfos.Count; i++)
		{
			themeDictionaryInfo = _themeDictionaryInfos[i];
			if (!themeDictionaryInfo.DictionaryReference.IsAlive)
			{
				_themeDictionaryInfos.RemoveAt(i);
				i--;
			}
			else if (themeDictionaryInfo.DictionaryReference.Target == dictionary)
			{
				themeDictionaryInfo.AssemblyName = assemblyName;
				return;
			}
		}
		themeDictionaryInfo = new ThemeDictionaryInfo();
		themeDictionaryInfo.DictionaryReference = new WeakReference(dictionary);
		themeDictionaryInfo.AssemblyName = assemblyName;
		_themeDictionaryInfos.Add(themeDictionaryInfo);
	}

	internal static void OnThemeChanged()
	{
		if (_themeDictionaryInfos == null)
		{
			return;
		}
		for (int i = 0; i < _themeDictionaryInfos.Count; i++)
		{
			ThemeDictionaryInfo themeDictionaryInfo = _themeDictionaryInfos[i];
			if (!themeDictionaryInfo.DictionaryReference.IsAlive)
			{
				_themeDictionaryInfos.RemoveAt(i);
				i--;
			}
			else
			{
				((ResourceDictionary)themeDictionaryInfo.DictionaryReference.Target).Source = GenerateUri(themeDictionaryInfo.AssemblyName, SystemResources.ResourceDictionaries.ThemedResourceName, UxThemeWrapper.ThemeName);
			}
		}
	}
}
