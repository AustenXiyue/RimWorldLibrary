using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Supports a value type that can be assigned null. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[ComVisible(true)]
public static class Nullable
{
	/// <summary>Compares the relative values of two <see cref="T:System.Nullable`1" /> objects.</summary>
	/// <returns>An integer that indicates the relative values of the <paramref name="n1" /> and <paramref name="n2" /> parameters.Return ValueDescriptionLess than zeroThe <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n1" /> is false, and the <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n2" /> is true.-or-The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are true, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is less than the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.ZeroThe <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are false.-or-The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are true, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is equal to the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.Greater than zeroThe <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n1" /> is true, and the <see cref="P:System.Nullable`1.HasValue" /> property for <paramref name="n2" /> is false.-or-The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are true, and the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n1" /> is greater than the value of the <see cref="P:System.Nullable`1.Value" /> property for <paramref name="n2" />.</returns>
	/// <param name="n1">A <see cref="T:System.Nullable`1" /> object.</param>
	/// <param name="n2">A <see cref="T:System.Nullable`1" /> object.</param>
	/// <typeparam name="T">The underlying value type of the <paramref name="n1" /> and <paramref name="n2" /> parameters.</typeparam>
	public static int Compare<T>(T? n1, T? n2) where T : struct
	{
		if (n1.has_value)
		{
			if (!n2.has_value)
			{
				return 1;
			}
			return Comparer<T>.Default.Compare(n1.value, n2.value);
		}
		if (!n2.has_value)
		{
			return 0;
		}
		return -1;
	}

	/// <summary>Indicates whether two specified <see cref="T:System.Nullable`1" /> objects are equal.</summary>
	/// <returns>true if the <paramref name="n1" /> parameter is equal to the <paramref name="n2" /> parameter; otherwise, false. The return value depends on the <see cref="P:System.Nullable`1.HasValue" /> and <see cref="P:System.Nullable`1.Value" /> properties of the two parameters that are compared.Return ValueDescriptiontrueThe <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are false. -or-The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are true, and the <see cref="P:System.Nullable`1.Value" /> properties of the parameters are equal.falseThe <see cref="P:System.Nullable`1.HasValue" /> property is true for one parameter and false for the other parameter.-or-The <see cref="P:System.Nullable`1.HasValue" /> properties for <paramref name="n1" /> and <paramref name="n2" /> are true, and the <see cref="P:System.Nullable`1.Value" /> properties of the parameters are unequal.</returns>
	/// <param name="n1">A <see cref="T:System.Nullable`1" /> object.</param>
	/// <param name="n2">A <see cref="T:System.Nullable`1" /> object.</param>
	/// <typeparam name="T">The underlying value type of the <paramref name="n1" /> and <paramref name="n2" /> parameters.</typeparam>
	public static bool Equals<T>(T? n1, T? n2) where T : struct
	{
		if (n1.has_value != n2.has_value)
		{
			return false;
		}
		if (!n1.has_value)
		{
			return true;
		}
		return EqualityComparer<T>.Default.Equals(n1.value, n2.value);
	}

	/// <summary>Returns the underlying type argument of the specified nullable type.</summary>
	/// <returns>The type argument of the <paramref name="nullableType" /> parameter, if the <paramref name="nullableType" /> parameter is a closed generic nullable type; otherwise, null. </returns>
	/// <param name="nullableType">A <see cref="T:System.Type" /> object that describes a closed generic nullable type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="nullableType" /> is null.</exception>
	public static Type GetUnderlyingType(Type nullableType)
	{
		if (nullableType == null)
		{
			throw new ArgumentNullException("nullableType");
		}
		if (!nullableType.IsGenericType || nullableType.IsGenericTypeDefinition || !(nullableType.GetGenericTypeDefinition() == typeof(Nullable<>)))
		{
			return null;
		}
		return nullableType.GetGenericArguments()[0];
	}
}
/// <summary>Represents a value type that can be assigned null.</summary>
/// <typeparam name="T">The underlying value type of the <see cref="T:System.Nullable`1" /> generic type.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerStepThrough]
public struct Nullable<T> where T : struct
{
	internal T value;

	internal bool has_value;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Nullable`1" /> object has a value.</summary>
	/// <returns>true if the current <see cref="T:System.Nullable`1" /> object has a value; false if the current <see cref="T:System.Nullable`1" /> object has no value.</returns>
	public bool HasValue => has_value;

	/// <summary>Gets the value of the current <see cref="T:System.Nullable`1" /> value.</summary>
	/// <returns>The value of the current <see cref="T:System.Nullable`1" /> object if the <see cref="P:System.Nullable`1.HasValue" /> property is true. An exception is thrown if the <see cref="P:System.Nullable`1.HasValue" /> property is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Nullable`1.HasValue" /> property is false.</exception>
	public T Value
	{
		get
		{
			if (!has_value)
			{
				throw new InvalidOperationException("Nullable object must have a value.");
			}
			return value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Nullable`1" /> structure to the specified value. </summary>
	/// <param name="value">A value type.</param>
	public Nullable(T value)
	{
		has_value = true;
		this.value = value;
	}

	/// <summary>Indicates whether the current <see cref="T:System.Nullable`1" /> object is equal to a specified object.</summary>
	/// <returns>true if the <paramref name="other" /> parameter is equal to the current <see cref="T:System.Nullable`1" /> object; otherwise, false. This table describes how equality is defined for the compared values: Return ValueDescriptiontrueThe <see cref="P:System.Nullable`1.HasValue" /> property is false, and the <paramref name="other" /> parameter is null. That is, two null values are equal by definition.-or-The <see cref="P:System.Nullable`1.HasValue" /> property is true, and the value returned by the <see cref="P:System.Nullable`1.Value" /> property is equal to the <paramref name="other" /> parameter.falseThe <see cref="P:System.Nullable`1.HasValue" /> property for the current <see cref="T:System.Nullable`1" /> structure is true, and the <paramref name="other" /> parameter is null.-or-The <see cref="P:System.Nullable`1.HasValue" /> property for the current <see cref="T:System.Nullable`1" /> structure is false, and the <paramref name="other" /> parameter is not null.-or-The <see cref="P:System.Nullable`1.HasValue" /> property for the current <see cref="T:System.Nullable`1" /> structure is true, and the value returned by the <see cref="P:System.Nullable`1.Value" /> property is not equal to the <paramref name="other" /> parameter.</returns>
	/// <param name="other">An object.</param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object other)
	{
		if (other == null)
		{
			return !has_value;
		}
		if (!(other is T?))
		{
			return false;
		}
		return Equals((T?)other);
	}

	private bool Equals(T? other)
	{
		if (other.has_value != has_value)
		{
			return false;
		}
		if (!has_value)
		{
			return true;
		}
		return other.value.Equals(value);
	}

	/// <summary>Retrieves the hash code of the object returned by the <see cref="P:System.Nullable`1.Value" /> property.</summary>
	/// <returns>The hash code of the object returned by the <see cref="P:System.Nullable`1.Value" /> property if the <see cref="P:System.Nullable`1.HasValue" /> property is true, or zero if the <see cref="P:System.Nullable`1.HasValue" /> property is false. </returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		if (!has_value)
		{
			return 0;
		}
		return value.GetHashCode();
	}

	/// <summary>Retrieves the value of the current <see cref="T:System.Nullable`1" /> object, or the object's default value.</summary>
	/// <returns>The value of the <see cref="P:System.Nullable`1.Value" /> property if the  <see cref="P:System.Nullable`1.HasValue" /> property is true; otherwise, the default value of the current <see cref="T:System.Nullable`1" /> object. The type of the default value is the type argument of the current <see cref="T:System.Nullable`1" /> object, and the value of the default value consists solely of binary zeroes.</returns>
	public T GetValueOrDefault()
	{
		return value;
	}

	/// <summary>Retrieves the value of the current <see cref="T:System.Nullable`1" /> object, or the specified default value.</summary>
	/// <returns>The value of the <see cref="P:System.Nullable`1.Value" /> property if the <see cref="P:System.Nullable`1.HasValue" /> property is true; otherwise, the <paramref name="defaultValue" /> parameter.</returns>
	/// <param name="defaultValue">A value to return if the <see cref="P:System.Nullable`1.HasValue" /> property is false.</param>
	public T GetValueOrDefault(T defaultValue)
	{
		if (!has_value)
		{
			return defaultValue;
		}
		return value;
	}

	/// <summary>Returns the text representation of the value of the current <see cref="T:System.Nullable`1" /> object.</summary>
	/// <returns>The text representation of the value of the current <see cref="T:System.Nullable`1" /> object if the <see cref="P:System.Nullable`1.HasValue" /> property is true, or an empty string ("") if the <see cref="P:System.Nullable`1.HasValue" /> property is false.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		if (has_value)
		{
			return value.ToString();
		}
		return string.Empty;
	}

	/// <summary>Creates a new <see cref="T:System.Nullable`1" /> object initialized to a specified value. </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> object whose <see cref="P:System.Nullable`1.Value" /> property is initialized with the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A value type.</param>
	public static implicit operator T?(T value)
	{
		return value;
	}

	/// <summary>Returns the value of a specified <see cref="T:System.Nullable`1" /> value.</summary>
	/// <returns>The value of the <see cref="P:System.Nullable`1.Value" /> property for the <paramref name="value" /> parameter.</returns>
	/// <param name="value">A <see cref="T:System.Nullable`1" /> value.</param>
	public static explicit operator T(T? value)
	{
		return value.Value;
	}

	private static object Box(T? o)
	{
		if (!o.has_value)
		{
			return null;
		}
		return o.value;
	}

	private static T? Unbox(object o)
	{
		if (o == null)
		{
			return null;
		}
		return (T)o;
	}
}
