using System.Runtime.InteropServices;

namespace System.Reflection.Emit;

/// <summary>Represents the Token returned by the metadata to represent an event.</summary>
[Serializable]
[ComVisible(true)]
public struct EventToken
{
	internal int tokValue;

	/// <summary>The default EventToken with <see cref="P:System.Reflection.Emit.EventToken.Token" /> value 0.</summary>
	public static readonly EventToken Empty;

	/// <summary>Retrieves the metadata token for this event.</summary>
	/// <returns>Read-only. Retrieves the metadata token for this event.</returns>
	public int Token => tokValue;

	internal EventToken(int val)
	{
		tokValue = val;
	}

	/// <summary>Checks if the given object is an instance of EventToken and is equal to this instance.</summary>
	/// <returns>Returns true if <paramref name="obj" /> is an instance of EventToken and equals the current instance; otherwise, false.</returns>
	/// <param name="obj">The object to be compared with this instance. </param>
	public override bool Equals(object obj)
	{
		bool flag = obj is EventToken;
		if (flag)
		{
			EventToken eventToken = (EventToken)obj;
			flag = tokValue == eventToken.tokValue;
		}
		return flag;
	}

	/// <summary>Indicates whether the current instance is equal to the specified <see cref="T:System.Reflection.Emit.EventToken" />.</summary>
	/// <returns>true if the value of <paramref name="obj" /> is equal to the value of the current instance; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.Reflection.Emit.EventToken" /> to compare to the current instance.</param>
	public bool Equals(EventToken obj)
	{
		return tokValue == obj.tokValue;
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.EventToken" /> structures are equal.</summary>
	/// <returns>true if <paramref name="a" /> is equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.EventToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.EventToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator ==(EventToken a, EventToken b)
	{
		return object.Equals(a, b);
	}

	/// <summary>Indicates whether two <see cref="T:System.Reflection.Emit.EventToken" /> structures are not equal.</summary>
	/// <returns>true if <paramref name="a" /> is not equal to <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The <see cref="T:System.Reflection.Emit.EventToken" /> to compare to <paramref name="b" />.</param>
	/// <param name="b">The <see cref="T:System.Reflection.Emit.EventToken" /> to compare to <paramref name="a" />.</param>
	public static bool operator !=(EventToken a, EventToken b)
	{
		return !object.Equals(a, b);
	}

	/// <summary>Generates the hash code for this event.</summary>
	/// <returns>Returns the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return tokValue;
	}
}
