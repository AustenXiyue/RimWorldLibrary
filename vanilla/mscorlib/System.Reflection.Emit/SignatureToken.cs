using System.Runtime.InteropServices;

namespace System.Reflection.Emit;

/// <summary>Represents the Token returned by the metadata to represent a signature.</summary>
[ComVisible(true)]
public struct SignatureToken
{
	internal int tokValue;

	/// <summary>The default SignatureToken with <see cref="P:System.Reflection.Emit.SignatureToken.Token" /> value 0.</summary>
	public static readonly SignatureToken Empty;

	/// <summary>Retrieves the metadata token for the local variable signature for this method.</summary>
	/// <returns>Read-only. Retrieves the metadata token of this signature.</returns>
	public int Token => tokValue;

	internal SignatureToken(int val)
	{
		tokValue = val;
	}

	/// <summary>Checks if the given object is an instance of SignatureToken and is equal to this instance.</summary>
	/// <returns>true if <paramref name="obj" /> is an instance of SignatureToken and is equal to this object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this SignatureToken. </param>
	public override bool Equals(object obj)
	{
		bool flag = obj is SignatureToken;
		if (flag)
		{
			SignatureToken signatureToken = (SignatureToken)obj;
			flag = tokValue == signatureToken.tokValue;
		}
		return flag;
	}

	/// <summary>Indicates whether the current instance is equal to the specified <see cref="T:System.Reflection.Emit.SignatureToken" />.</summary>
	/// <returns>true if the value of <paramref name="obj" /> is equal to the value of the current instance; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Reflection.Emit.SignatureToken" /> to compare to the current instance.</param>
	public bool Equals(SignatureToken obj)
	{
		return tokValue == obj.tokValue;
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.SignatureToken" /> structures are equal.</summary>
	/// <returns>true if <paramref name="a" /> is equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.SignatureToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.SignatureToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator ==(SignatureToken a, SignatureToken b)
	{
		return object.Equals(a, b);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.SignatureToken" /> structures are not equal.</summary>
	/// <returns>true if <paramref name="a" /> is not equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.SignatureToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.SignatureToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator !=(SignatureToken a, SignatureToken b)
	{
		return !object.Equals(a, b);
	}

	/// <summary>Generates the hash code for this signature.</summary>
	/// <returns>Returns the hash code for this signature.</returns>
	public override int GetHashCode()
	{
		return tokValue;
	}
}
