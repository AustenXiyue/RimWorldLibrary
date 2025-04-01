using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that data should be marshaled from the caller to the callee, but not back to the caller.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class InAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeParameterInfo parameter)
	{
		if (!parameter.IsIn)
		{
			return null;
		}
		return new InAttribute();
	}

	internal static bool IsDefined(RuntimeParameterInfo parameter)
	{
		return parameter.IsIn;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.InAttribute" /> class.</summary>
	public InAttribute()
	{
	}
}
