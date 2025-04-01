using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that the attributed type was previously defined in COM.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
[ComVisible(true)]
public sealed class ComImportAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeType type)
	{
		if ((type.Attributes & TypeAttributes.Import) == 0)
		{
			return null;
		}
		return new ComImportAttribute();
	}

	internal static bool IsDefined(RuntimeType type)
	{
		return (type.Attributes & TypeAttributes.Import) != 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ComImportAttribute" />.</summary>
	public ComImportAttribute()
	{
	}
}
