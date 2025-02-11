using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Persists an 8-byte <see cref="T:System.DateTime" /> constant for a field or parameter.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
[ComVisible(true)]
public sealed class DateTimeConstantAttribute : CustomConstantAttribute
{
	private DateTime date;

	/// <summary>Gets the number of 100-nanosecond ticks that represent the date and time of this instance.</summary>
	/// <returns>The number of 100-nanosecond ticks that represent the date and time of this instance.</returns>
	public override object Value => date;

	/// <summary>Initializes a new instance of the DateTimeConstantAttribute class with the number of 100-nanosecond ticks that represent the date and time of this instance.</summary>
	/// <param name="ticks">The number of 100-nanosecond ticks that represent the date and time of this instance. </param>
	public DateTimeConstantAttribute(long ticks)
	{
		date = new DateTime(ticks);
	}

	internal static DateTime GetRawDateTimeConstant(CustomAttributeData attr)
	{
		foreach (CustomAttributeNamedArgument namedArgument in attr.NamedArguments)
		{
			if (namedArgument.MemberInfo.Name.Equals("Value"))
			{
				return new DateTime((long)namedArgument.TypedValue.Value);
			}
		}
		return new DateTime((long)attr.ConstructorArguments[0].Value);
	}
}
