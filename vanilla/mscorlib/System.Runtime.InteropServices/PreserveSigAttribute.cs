using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Indicates that the HRESULT or retval signature transformation that takes place during COM interop calls should be suppressed.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class PreserveSigAttribute : Attribute
{
	internal static Attribute GetCustomAttribute(RuntimeMethodInfo method)
	{
		if ((method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) == 0)
		{
			return null;
		}
		return new PreserveSigAttribute();
	}

	internal static bool IsDefined(RuntimeMethodInfo method)
	{
		return (method.GetMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.PreserveSigAttribute" /> class.</summary>
	public PreserveSigAttribute()
	{
	}
}
