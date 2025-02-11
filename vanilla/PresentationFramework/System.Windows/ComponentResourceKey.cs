using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Defines or references resource keys based on class names in external assemblies, as well as an additional identifier.</summary>
[TypeConverter(typeof(ComponentResourceKeyConverter))]
public class ComponentResourceKey : ResourceKey
{
	private Type _typeInTargetAssembly;

	private bool _typeInTargetAssemblyInitialized;

	private object _resourceId;

	private bool _resourceIdInitialized;

	/// <summary>Gets or sets the <see cref="T:System.Type" /> that defines the resource key.</summary>
	/// <returns>The type that defines the resource key.</returns>
	public Type TypeInTargetAssembly
	{
		get
		{
			return _typeInTargetAssembly;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (_typeInTargetAssemblyInitialized)
			{
				throw new InvalidOperationException(SR.ChangingTypeNotAllowed);
			}
			_typeInTargetAssembly = value;
			_typeInTargetAssemblyInitialized = true;
		}
	}

	/// <summary>Gets the assembly object that indicates which assembly's dictionary to look in for the value associated with this key.</summary>
	/// <returns>The retrieved assembly, as a reflection class.</returns>
	public override Assembly Assembly
	{
		get
		{
			if (!(_typeInTargetAssembly != null))
			{
				return null;
			}
			return _typeInTargetAssembly.Assembly;
		}
	}

	/// <summary>Gets or sets a unique identifier to differentiate this key from others associated with this type.</summary>
	/// <returns>A unique identifier. Typically this is a string.</returns>
	public object ResourceId
	{
		get
		{
			return _resourceId;
		}
		set
		{
			if (_resourceIdInitialized)
			{
				throw new InvalidOperationException(SR.ChangingIdNotAllowed);
			}
			_resourceId = value;
			_resourceIdInitialized = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ComponentResourceKey" /> class.</summary>
	public ComponentResourceKey()
	{
	}

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.ComponentResourceKey" /> , specifying the <see cref="T:System.Type" /> that defines the key, and an object to use as an additional resource identifier.</summary>
	/// <param name="typeInTargetAssembly">The type that defines the resource key.</param>
	/// <param name="resourceId">A unique identifier to differentiate this <see cref="T:System.Windows.ComponentResourceKey" /> from others associated with the <paramref name="typeInTargetAssembly" /> type.</param>
	public ComponentResourceKey(Type typeInTargetAssembly, object resourceId)
	{
		if (typeInTargetAssembly == null)
		{
			throw new ArgumentNullException("typeInTargetAssembly");
		}
		if (resourceId == null)
		{
			throw new ArgumentNullException("resourceId");
		}
		_typeInTargetAssembly = typeInTargetAssembly;
		_typeInTargetAssemblyInitialized = true;
		_resourceId = resourceId;
		_resourceIdInitialized = true;
	}

	/// <summary>Determines whether the provided object is equal to the current <see cref="T:System.Windows.ComponentResourceKey" />. </summary>
	/// <returns>true if the objects are equal; otherwise, false.</returns>
	/// <param name="o">Object to compare with the current <see cref="T:System.Windows.ComponentResourceKey" />.</param>
	public override bool Equals(object o)
	{
		if (o is ComponentResourceKey componentResourceKey)
		{
			if ((componentResourceKey._typeInTargetAssembly != null) ? componentResourceKey._typeInTargetAssembly.Equals(_typeInTargetAssembly) : (_typeInTargetAssembly == null))
			{
				if (componentResourceKey._resourceId == null)
				{
					return _resourceId == null;
				}
				return componentResourceKey._resourceId.Equals(_resourceId);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Windows.ComponentResourceKey" />. </summary>
	/// <returns>A signed 32-bit integer value.</returns>
	public override int GetHashCode()
	{
		return ((_typeInTargetAssembly != null) ? _typeInTargetAssembly.GetHashCode() : 0) ^ ((_resourceId != null) ? _resourceId.GetHashCode() : 0);
	}

	/// <summary>Gets the string representation of a <see cref="T:System.Windows.ComponentResourceKey" />. </summary>
	/// <returns>The string representation.</returns>
	public override string ToString()
	{
		IFormatProvider formatProvider = null;
		IFormatProvider provider = formatProvider;
		Span<char> initialBuffer = stackalloc char[256];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(15, 2, formatProvider, initialBuffer);
		handler.AppendLiteral("TargetType=");
		handler.AppendFormatted((_typeInTargetAssembly != null) ? _typeInTargetAssembly.FullName : "null");
		handler.AppendLiteral(" ID=");
		handler.AppendFormatted((_resourceId != null) ? _resourceId.ToString() : "null");
		return string.Create(provider, initialBuffer, ref handler);
	}
}
