using System.Collections.Generic;
using System.Windows.Diagnostics;
using System.Windows.Markup;
using System.Xaml;

namespace System.Windows;

/// <summary>Implements a markup extension that supports static (XAML load time) resource references made from XAML. </summary>
[MarkupExtensionReturnType(typeof(object))]
[Localizability(LocalizationCategory.NeverLocalize)]
public class StaticResourceExtension : MarkupExtension
{
	private object _resourceKey;

	/// <summary>Gets or sets the key value passed by this static resource reference. They key is used  to return the object matching that key in resource dictionaries. </summary>
	/// <returns>The resource key for a resource.</returns>
	/// <exception cref="T:System.ArgumentNullException">Specified value as null, either through markup extension usage or explicit construction.</exception>
	[ConstructorArgument("resourceKey")]
	public object ResourceKey
	{
		get
		{
			return _resourceKey;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_resourceKey = value;
		}
	}

	internal virtual DeferredResourceReference PrefetchedValue => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.StaticResourceExtension" /> class.</summary>
	public StaticResourceExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.StaticResourceExtension" /> class, with the provided initial key.</summary>
	/// <param name="resourceKey">The key of the resource that this markup extension references.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceKey" /> parameter is null, either through markup extension usage or explicit construction.</exception>
	public StaticResourceExtension(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		_resourceKey = resourceKey;
	}

	/// <summary>Returns an object that should be set on the property where this extension is applied. For <see cref="T:System.Windows.StaticResourceExtension" />, this is the object found in a resource dictionary, where the object to find is identified by the <see cref="P:System.Windows.StaticResourceExtension.ResourceKey" />.</summary>
	/// <returns>The object value to set on the property where the markup extension provided value is evaluated.</returns>
	/// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="serviceProvider" /> was null, or failed to implement a required service.</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (ResourceKey is SystemResourceKey)
		{
			return (ResourceKey as SystemResourceKey).Resource;
		}
		return ProvideValueInternal(serviceProvider, allowDeferredReference: false);
	}

	internal object ProvideValueInternal(IServiceProvider serviceProvider, bool allowDeferredReference)
	{
		object obj = TryProvideValueInternal(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference: false);
		if (obj == DependencyProperty.UnsetValue)
		{
			throw new Exception(SR.Format(SR.ParserNoResource, ResourceKey.ToString()));
		}
		return obj;
	}

	internal object TryProvideValueInternal(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		if (!ResourceDictionaryDiagnostics.HasStaticResourceResolvedListeners)
		{
			return TryProvideValueImpl(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
		}
		return TryProvideValueWithDiagnosticEvent(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
	}

	private object TryProvideValueWithDiagnosticEvent(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: not null, TargetProperty: not null } provideValueTarget) || ResourceDictionaryDiagnostics.ShouldIgnoreProperty(provideValueTarget.TargetProperty))
		{
			return TryProvideValueImpl(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
		}
		bool flag = false;
		ResourceDictionaryDiagnostics.LookupResult result;
		object obj;
		try
		{
			result = ResourceDictionaryDiagnostics.RequestLookupResult(this);
			obj = TryProvideValueImpl(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
			if (obj is DeferredResourceReference deferredResourceReference)
			{
				flag = true;
				ResourceDictionary dictionary = deferredResourceReference.Dictionary;
				if (dictionary != null)
				{
					ResourceDictionaryDiagnostics.RecordLookupResult(ResourceKey, dictionary);
				}
			}
			else
			{
				flag = obj != DependencyProperty.UnsetValue;
			}
		}
		finally
		{
			ResourceDictionaryDiagnostics.RevertRequest(this, flag);
		}
		if (flag)
		{
			ResourceDictionaryDiagnostics.OnStaticResourceResolved(provideValueTarget.TargetObject, provideValueTarget.TargetProperty, result);
		}
		return obj;
	}

	private object TryProvideValueImpl(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		DeferredResourceReference prefetchedValue = PrefetchedValue;
		object obj;
		if (prefetchedValue == null)
		{
			obj = FindResourceInEnviroment(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
		}
		else
		{
			obj = FindResourceInDeferredContent(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
			if (obj == DependencyProperty.UnsetValue)
			{
				obj = (allowDeferredReference ? prefetchedValue : prefetchedValue.GetValue(BaseValueSourceInternal.Unknown));
			}
		}
		return obj;
	}

	private ResourceDictionary FindTheResourceDictionary(IServiceProvider serviceProvider, bool isDeferredContentSearch)
	{
		IXamlSchemaContextProvider obj = (serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider) ?? throw new InvalidOperationException(SR.Format(SR.MarkupExtensionNoContext, GetType().Name, "IXamlSchemaContextProvider"));
		if (!(serviceProvider.GetService(typeof(IAmbientProvider)) is IAmbientProvider ambientProvider))
		{
			throw new InvalidOperationException(SR.Format(SR.MarkupExtensionNoContext, GetType().Name, "IAmbientProvider"));
		}
		XamlSchemaContext schemaContext = obj.SchemaContext;
		XamlType xamlType = schemaContext.GetXamlType(typeof(FrameworkElement));
		XamlType xamlType2 = schemaContext.GetXamlType(typeof(Style));
		XamlType xamlType3 = schemaContext.GetXamlType(typeof(FrameworkTemplate));
		XamlType xamlType4 = schemaContext.GetXamlType(typeof(Application));
		XamlMember member = schemaContext.GetXamlType(typeof(FrameworkContentElement)).GetMember("Resources");
		XamlMember member2 = xamlType.GetMember("Resources");
		XamlMember member3 = xamlType2.GetMember("Resources");
		XamlMember member4 = xamlType2.GetMember("BasedOn");
		XamlMember member5 = xamlType3.GetMember("Resources");
		XamlMember member6 = xamlType4.GetMember("Resources");
		XamlType[] types = new XamlType[1] { schemaContext.GetXamlType(typeof(ResourceDictionary)) };
		List<AmbientPropertyValue> list = ambientProvider.GetAllAmbientValues(null, isDeferredContentSearch, types, member, member2, member3, member4, member5, member6) as List<AmbientPropertyValue>;
		for (int i = 0; i < list.Count; i++)
		{
			AmbientPropertyValue ambientPropertyValue = list[i];
			if (ambientPropertyValue.Value is ResourceDictionary)
			{
				ResourceDictionary resourceDictionary = (ResourceDictionary)ambientPropertyValue.Value;
				if (resourceDictionary.Contains(ResourceKey))
				{
					return resourceDictionary;
				}
			}
			if (ambientPropertyValue.Value is Style)
			{
				ResourceDictionary resourceDictionary2 = ((Style)ambientPropertyValue.Value).FindResourceDictionary(ResourceKey);
				if (resourceDictionary2 != null)
				{
					return resourceDictionary2;
				}
			}
		}
		return null;
	}

	internal object FindResourceInDeferredContent(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		ResourceDictionary resourceDictionary = FindTheResourceDictionary(serviceProvider, isDeferredContentSearch: true);
		object obj = DependencyProperty.UnsetValue;
		if (resourceDictionary != null)
		{
			obj = resourceDictionary.Lookup(ResourceKey, allowDeferredReference, mustReturnDeferredResourceReference, canCacheAsThemeResource: false);
		}
		if (mustReturnDeferredResourceReference && obj == DependencyProperty.UnsetValue)
		{
			obj = new DeferredResourceReferenceHolder(ResourceKey, obj);
		}
		return obj;
	}

	private object FindResourceInAppOrSystem(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		object source;
		if (!SystemResources.IsSystemResourcesParsing)
		{
			return FrameworkElement.FindResourceFromAppOrSystem(ResourceKey, out source, disableThrowOnResourceNotFound: false, allowDeferredReference, mustReturnDeferredResourceReference);
		}
		return SystemResources.FindResourceInternal(ResourceKey, allowDeferredReference, mustReturnDeferredResourceReference);
	}

	private object FindResourceInEnviroment(IServiceProvider serviceProvider, bool allowDeferredReference, bool mustReturnDeferredResourceReference)
	{
		ResourceDictionary resourceDictionary = FindTheResourceDictionary(serviceProvider, isDeferredContentSearch: false);
		if (resourceDictionary != null)
		{
			return resourceDictionary.Lookup(ResourceKey, allowDeferredReference, mustReturnDeferredResourceReference, canCacheAsThemeResource: false);
		}
		object obj = FindResourceInAppOrSystem(serviceProvider, allowDeferredReference, mustReturnDeferredResourceReference);
		if (obj == null)
		{
			obj = DependencyProperty.UnsetValue;
		}
		if (mustReturnDeferredResourceReference && !(obj is DeferredResourceReference))
		{
			obj = new DeferredResourceReferenceHolder(ResourceKey, obj);
		}
		return obj;
	}
}
