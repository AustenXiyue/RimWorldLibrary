using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Defines a constant value that a compiler can persist for a field or method parameter.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
public abstract class CustomConstantAttribute : Attribute
{
	/// <summary>Gets the constant value stored by this attribute.</summary>
	/// <returns>The constant value stored by this attribute.</returns>
	public abstract object Value { get; }

	internal static object GetRawConstant(CustomAttributeData attr)
	{
		foreach (CustomAttributeNamedArgument namedArgument in attr.NamedArguments)
		{
			if (namedArgument.MemberInfo.Name.Equals("Value"))
			{
				return namedArgument.TypedValue.Value;
			}
		}
		return DBNull.Value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CustomConstantAttribute" /> class. </summary>
	protected CustomConstantAttribute()
	{
	}
}
