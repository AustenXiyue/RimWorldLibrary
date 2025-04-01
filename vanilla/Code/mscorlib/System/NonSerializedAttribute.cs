using System.Reflection;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Indicates that a field of a serializable class should not be serialized. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
[ComVisible(true)]
public sealed class NonSerializedAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeFieldInfo field)
	{
		if ((field.Attributes & FieldAttributes.NotSerialized) == 0)
		{
			return null;
		}
		return new NonSerializedAttribute();
	}

	internal static bool IsDefined(RuntimeFieldInfo field)
	{
		return (field.Attributes & FieldAttributes.NotSerialized) != 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.NonSerializedAttribute" /> class.</summary>
	public NonSerializedAttribute()
	{
	}
}
