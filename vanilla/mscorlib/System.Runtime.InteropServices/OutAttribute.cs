using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that data should be marshaled from callee back to caller.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ComVisible(true)]
public sealed class OutAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeParameterInfo parameter)
	{
		if (!parameter.IsOut)
		{
			return null;
		}
		return new OutAttribute();
	}

	internal static bool IsDefined(RuntimeParameterInfo parameter)
	{
		return parameter.IsOut;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.OutAttribute" /> class.</summary>
	public OutAttribute()
	{
	}
}
