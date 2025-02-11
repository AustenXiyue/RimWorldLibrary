using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;

namespace System.Windows;

/// <summary>Implements a markup extension that supports dynamic resource references made from XAML. </summary>
[TypeConverter(typeof(DynamicResourceExtensionConverter))]
[MarkupExtensionReturnType(typeof(object))]
public class DynamicResourceExtension : MarkupExtension
{
	private object _resourceKey;

	/// <summary>Gets or sets the key specified by this dynamic resource reference. The key is used to lookup a resource in resource dictionaries, by means of an intermediate expression. </summary>
	/// <returns>The resource key that this dynamic resource reference specifies.</returns>
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DynamicResourceExtension" /> class.</summary>
	public DynamicResourceExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DynamicResourceExtension" /> class, with the provided initial key.</summary>
	/// <param name="resourceKey">The key of the resource that this markup extension references.</param>
	public DynamicResourceExtension(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		_resourceKey = resourceKey;
	}

	/// <summary>Returns an object that should be set on the property where this extension is applied. For <see cref="T:System.Windows.DynamicResourceExtension" />, this is the object found in a resource dictionary in the current parent chain that is keyed by the <see cref="P:System.Windows.DynamicResourceExtension.ResourceKey" />.</summary>
	/// <returns>The object to set on the property where the extension is applied. Rather than the actual value, this will be an expression that will be evaluated at a later time.</returns>
	/// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
	/// <exception cref="T:System.InvalidOperationException">Attempted to provide a value for an extension that did not provide a <paramref name="resourceKey" />.</exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (ResourceKey == null)
		{
			throw new InvalidOperationException(SR.MarkupExtensionResourceKey);
		}
		if (serviceProvider != null)
		{
			Helper.CheckCanReceiveMarkupExtension(this, serviceProvider, out var _, out var _);
		}
		return new ResourceReferenceExpression(ResourceKey);
	}
}
