using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Represents an argument of a custom attribute in the reflection-only context, or an element of an array argument.</summary>
[Serializable]
[ComVisible(true)]
public struct CustomAttributeTypedArgument
{
	private Type argumentType;

	private object value;

	/// <summary>Gets the type of the argument or of the array argument element.</summary>
	/// <returns>A <see cref="T:System.Type" /> object representing the type of the argument or of the array element.</returns>
	public Type ArgumentType => argumentType;

	/// <summary>Gets the value of the argument for a simple argument or for an element of an array argument; gets a collection of values for an array argument.</summary>
	/// <returns>An object that represents the value of the argument or element, or a generic <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> objects that represent the values of an array-type argument.</returns>
	public object Value => value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> class with the specified type and value.</summary>
	/// <param name="argumentType">The type of the custom attribute argument.</param>
	/// <param name="value">The value of the custom attribute argument.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="argumentType" /> is null.</exception>
	public CustomAttributeTypedArgument(Type argumentType, object value)
	{
		if (argumentType == null)
		{
			throw new ArgumentNullException("argumentType");
		}
		this.argumentType = argumentType;
		this.value = value;
		if (value is Array)
		{
			Array array = (Array)value;
			Type elementType = array.GetType().GetElementType();
			CustomAttributeTypedArgument[] array2 = new CustomAttributeTypedArgument[array.GetLength(0)];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = new CustomAttributeTypedArgument(elementType, array.GetValue(i));
			}
			this.value = new ReadOnlyCollection<CustomAttributeTypedArgument>(array2);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> class with the specified value.</summary>
	/// <param name="value">The value of the custom attribute argument.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public CustomAttributeTypedArgument(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		argumentType = value.GetType();
		this.value = value;
	}

	/// <summary>Returns a string consisting of the argument name, the equal sign, and a string representation of the argument value.</summary>
	/// <returns>A string consisting of the argument name, the equal sign, and a string representation of the argument value.</returns>
	public override string ToString()
	{
		string text = ((value != null) ? value.ToString() : string.Empty);
		if (argumentType == typeof(string))
		{
			return "\"" + text + "\"";
		}
		if (argumentType == typeof(Type))
		{
			return "typeof (" + text + ")";
		}
		if (argumentType.IsEnum)
		{
			return "(" + argumentType.Name + ")" + text;
		}
		return text;
	}

	/// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
	/// <param name="obj">The object to compare with the current instance. </param>
	public override bool Equals(object obj)
	{
		if (!(obj is CustomAttributeTypedArgument customAttributeTypedArgument))
		{
			return false;
		}
		if (!(customAttributeTypedArgument.argumentType == argumentType) || value == null)
		{
			return customAttributeTypedArgument.value == null;
		}
		return value.Equals(customAttributeTypedArgument.value);
	}

	public override int GetHashCode()
	{
		return (argumentType.GetHashCode() << 16) + ((value != null) ? value.GetHashCode() : 0);
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are equivalent.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are equal; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the left of the equality operator.</param>
	/// <param name="right">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the right of the equality operator.</param>
	public static bool operator ==(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
	{
		return left.Equals(right);
	}

	/// <summary>Tests whether two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are different.</summary>
	/// <returns>true if the two <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structures are different; otherwise, false.</returns>
	/// <param name="left">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the left of the inequality operator.</param>
	/// <param name="right">The <see cref="T:System.Reflection.CustomAttributeTypedArgument" /> structure to the right of the inequality operator.</param>
	public static bool operator !=(CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
	{
		return !left.Equals(right);
	}
}
