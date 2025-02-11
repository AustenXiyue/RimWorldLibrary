using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Identifies the <see cref="T:System.Windows.Markup.ValueSerializer" /> class that a type or property should use when it is serialized.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class ValueSerializerAttribute : Attribute
{
	private Type _valueSerializerType;

	private string _valueSerializerTypeName;

	/// <summary>Gets the type of the <see cref="T:System.Windows.Markup.ValueSerializer" /> class reported by this attribute.</summary>
	/// <returns>The type of the <see cref="T:System.Windows.Markup.ValueSerializer" />.</returns>
	public Type ValueSerializerType
	{
		get
		{
			if (_valueSerializerType == null && _valueSerializerTypeName != null)
			{
				_valueSerializerType = Type.GetType(_valueSerializerTypeName);
			}
			return _valueSerializerType;
		}
	}

	/// <summary>Gets the assembly qualified name of the <see cref="T:System.Windows.Markup.ValueSerializer" /> type for this type or property.</summary>
	/// <returns>The assembly qualified name of the type.</returns>
	public string ValueSerializerTypeName
	{
		get
		{
			if (_valueSerializerType != null)
			{
				return _valueSerializerType.AssemblyQualifiedName;
			}
			return _valueSerializerTypeName;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ValueSerializerAttribute" /> class, using the specified type.</summary>
	/// <param name="valueSerializerType">A type that represents the type of the <see cref="T:System.Windows.Markup.ValueSerializer" /> class.</param>
	public ValueSerializerAttribute(Type valueSerializerType)
	{
		_valueSerializerType = valueSerializerType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ValueSerializerAttribute" /> class, using an assembly qualified type name string.</summary>
	/// <param name="valueSerializerTypeName">The assembly qualified type name string for the <see cref="T:System.Windows.Markup.ValueSerializer" /> class to use.</param>
	public ValueSerializerAttribute(string valueSerializerTypeName)
	{
		_valueSerializerTypeName = valueSerializerTypeName;
	}
}
