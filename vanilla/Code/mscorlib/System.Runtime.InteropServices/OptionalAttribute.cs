using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that a parameter is optional.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ComVisible(true)]
public sealed class OptionalAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeParameterInfo parameter)
	{
		if (!parameter.IsOptional)
		{
			return null;
		}
		return new OptionalAttribute();
	}

	internal static bool IsDefined(RuntimeParameterInfo parameter)
	{
		return parameter.IsOptional;
	}

	/// <summary>Initializes a new instance of the OptionalAttribute class with default values.</summary>
	public OptionalAttribute()
	{
	}
}
