using System.Runtime.InteropServices;

namespace System.Reflection.Emit;

/// <summary>The PropertyToken struct is an opaque representation of the Token returned by the metadata to represent a property.</summary>
[Serializable]
[ComVisible(true)]
public struct PropertyToken
{
	internal int tokValue;

	/// <summary>The default PropertyToken with <see cref="P:System.Reflection.Emit.PropertyToken.Token" /> value 0.</summary>
	public static readonly PropertyToken Empty;

	/// <summary>Retrieves the metadata token for this property.</summary>
	/// <returns>Read-only. Retrieves the metadata token for this instance.</returns>
	public int Token => tokValue;

	internal PropertyToken(int val)
	{
		tokValue = val;
	}

	/// <summary>Checks if the given object is an instance of PropertyToken and is equal to this instance.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of PropertyToken and equals the current instance; otherwise, false.</returns>
	/// <param name="obj">The object to this object. </param>
	public override bool Equals(object obj)
	{
		bool flag = obj is PropertyToken;
		if (flag)
		{
			PropertyToken propertyToken = (PropertyToken)obj;
			flag = tokValue == propertyToken.tokValue;
		}
		return flag;
	}

	/// <summary>Indicates whether the current instance is equal to the specified <see cref="T:System.Reflection.Emit.PropertyToken" />.</summary>
	/// <returns>true if the value of <paramref name="obj" /> is equal to the value of the current instance; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Reflection.Emit.PropertyToken" /> to compare to the current instance.</param>
	public bool Equals(PropertyToken obj)
	{
		return tokValue == obj.tokValue;
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.PropertyToken" /> structures are equal.</summary>
	/// <returns>true if <paramref name="a" /> is equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.PropertyToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.PropertyToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator ==(PropertyToken a, PropertyToken b)
	{
		return object.Equals(a, b);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.PropertyToken" /> structures are not equal.</summary>
	/// <returns>true if <paramref name="a" /> is not equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.PropertyToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.PropertyToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator !=(PropertyToken a, PropertyToken b)
	{
		return !object.Equals(a, b);
	}

	/// <summary>Generates the hash code for this property.</summary>
	/// <returns>Returns the hash code for this property.</returns>
	public override int GetHashCode()
	{
		return tokValue;
	}
}
