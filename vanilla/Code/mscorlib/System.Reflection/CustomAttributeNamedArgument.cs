using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Represents a named argument of a custom attribute in the reflection-only context.</summary>
[Serializable]
[ComVisible(true)]
public struct CustomAttributeNamedArgument
{
	private CustomAttributeTypedArgument typedArgument;

	private MemberInfo memberInfo;

	/// <summary>Gets the attribute member that would be used to set the named argument.</summary>
	/// <returns>The attribute member that would be used to set the named argument.</returns>
	public MemberInfo MemberInfo => memberInfo;

	/// <summary>Gets a <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure that can be used to obtain the type and value of the current named argument.</summary>
	/// <returns>A structure that can be used to obtain the type and value of the current named argument.</returns>
	public CustomAttributeTypedArgument TypedValue => typedArgument;

	/// <summary>Gets a value that indicates whether the named argument is a field.</summary>
	/// <returns>true if the named argument is a field; otherwise, false.</returns>
	public bool IsField => memberInfo.MemberType == MemberTypes.Field;

	/// <summary>Gets the name of the attribute member that would be used to set the named argument.</summary>
	/// <returns>The name of the attribute member that would be used to set the named argument.</returns>
	public string MemberName => memberInfo.Name;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> class, which represents the specified field or property of the custom attribute, and specifies the value of the field or property.</summary>
	/// <param name="memberInfo">A field or property of the custom attribute. The new <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> object represents this member and its value.</param>
	/// <param name="value">The value of the field or property of the custom attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="memberInfo" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="memberInfo" /> is not a field or property of the custom attribute.</exception>
	public CustomAttributeNamedArgument(MemberInfo memberInfo, object value)
	{
		this.memberInfo = memberInfo;
		typedArgument = (CustomAttributeTypedArgument)value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> class, which represents the specified field or property of the custom attribute, and specifies a <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> object that describes the type and value of the field or property.</summary>
	/// <param name="memberInfo">A field or property of the custom attribute. The new <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> object represents this member and its value.</param>
	/// <param name="typedArgument">An object that describes the type and value of the field or property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="memberInfo" /> is null.</exception>
	public CustomAttributeNamedArgument(MemberInfo memberInfo, CustomAttributeTypedArgument typedArgument)
	{
		this.memberInfo = memberInfo;
		this.typedArgument = typedArgument;
	}

	/// <summary>Returns a string that consists of the argument name, the equal sign, and a string representation of the argument value.</summary>
	/// <returns>A string that consists of the argument name, the equal sign, and a string representation of the argument value.</returns>
	public override string ToString()
	{
		return memberInfo.Name + " = " + typedArgument.ToString();
	}

	/// <summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance, or null.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is CustomAttributeNamedArgument customAttributeNamedArgument))
		{
			return false;
		}
		if (customAttributeNamedArgument.memberInfo == memberInfo)
		{
			return typedArgument.Equals(customAttributeNamedArgument.typedArgument);
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return (memberInfo.GetHashCode() << 16) + typedArgument.GetHashCode();
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are equivalent.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The structure to the left of the equality operator.</param>
	/// <param name="right">The structure to the right of the equality operator.</param>
	public static bool operator ==(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
	{
		return left.Equals(right);
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are different.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeNamedArgument" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The structure to the left of the inequality operator.</param>
	/// <param name="right">The structure to the right of the inequality operator.</param>
	public static bool operator !=(CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
	{
		return !left.Equals(right);
	}
}
