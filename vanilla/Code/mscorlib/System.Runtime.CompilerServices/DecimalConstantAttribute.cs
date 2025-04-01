using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

/// <summary>Stores the value of a <see cref="T:System.Decimal" /> constant in metadata. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
[ComVisible(true)]
public sealed class DecimalConstantAttribute : Attribute
{
	private decimal dec;

	/// <summary>Gets the decimal constant stored in this attribute.</summary>
	/// <returns>The decimal constant stored in this attribute.</returns>
	public decimal Value => dec;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DecimalConstantAttribute" /> class with the specified unsigned integer values.</summary>
	/// <param name="scale">The power of 10 scaling factor that indicates the number of digits to the right of the decimal point. Valid values are 0 through 28 inclusive. </param>
	/// <param name="sign">A value of 0 indicates a positive value, and a value of 1 indicates a negative value. </param>
	/// <param name="hi">The high 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />. </param>
	/// <param name="mid">The middle 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />. </param>
	/// <param name="low">The low 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="scale" /> &gt; 28. </exception>
	[CLSCompliant(false)]
	public DecimalConstantAttribute(byte scale, byte sign, uint hi, uint mid, uint low)
	{
		dec = new decimal((int)low, (int)mid, (int)hi, sign != 0, scale);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.DecimalConstantAttribute" /> class with the specified signed integer values. </summary>
	/// <param name="scale">The power of 10 scaling factor that indicates the number of digits to the right of the decimal point. Valid values are 0 through 28 inclusive.</param>
	/// <param name="sign">A value of 0 indicates a positive value, and a value of 1 indicates a negative value.</param>
	/// <param name="hi">The high 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />.</param>
	/// <param name="mid">The middle 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />.</param>
	/// <param name="low">The low 32 bits of the 96-bit <see cref="P:System.Runtime.CompilerServices.DecimalConstantAttribute.Value" />.</param>
	public DecimalConstantAttribute(byte scale, byte sign, int hi, int mid, int low)
	{
		dec = new decimal(low, mid, hi, sign != 0, scale);
	}

	internal static decimal GetRawDecimalConstant(CustomAttributeData attr)
	{
		foreach (CustomAttributeNamedArgument namedArgument in attr.NamedArguments)
		{
			if (namedArgument.MemberInfo.Name.Equals("Value"))
			{
				return (decimal)namedArgument.TypedValue.Value;
			}
		}
		ParameterInfo[] parameters = attr.Constructor.GetParameters();
		IList<CustomAttributeTypedArgument> constructorArguments = attr.ConstructorArguments;
		if (parameters[2].ParameterType == typeof(uint))
		{
			int lo = (int)(uint)constructorArguments[4].Value;
			int mid = (int)(uint)constructorArguments[3].Value;
			int hi = (int)(uint)constructorArguments[2].Value;
			byte b = (byte)constructorArguments[1].Value;
			byte scale = (byte)constructorArguments[0].Value;
			return new decimal(lo, mid, hi, b != 0, scale);
		}
		int lo2 = (int)constructorArguments[4].Value;
		int mid2 = (int)constructorArguments[3].Value;
		int hi2 = (int)constructorArguments[2].Value;
		byte b2 = (byte)constructorArguments[1].Value;
		byte scale2 = (byte)constructorArguments[0].Value;
		return new decimal(lo2, mid2, hi2, b2 != 0, scale2);
	}
}
