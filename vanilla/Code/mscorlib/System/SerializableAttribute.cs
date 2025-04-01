using System.Reflection;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Indicates that a class can be serialized. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
public sealed class SerializableAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeType type)
	{
		if ((type.Attributes & TypeAttributes.Serializable) != TypeAttributes.Serializable)
		{
			return null;
		}
		return new SerializableAttribute();
	}

	internal static bool IsDefined(RuntimeType type)
	{
		return type.IsSerializable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.SerializableAttribute" /> class.</summary>
	public SerializableAttribute()
	{
	}
}
