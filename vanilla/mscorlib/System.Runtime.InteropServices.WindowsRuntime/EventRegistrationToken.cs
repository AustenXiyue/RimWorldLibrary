namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>A token that is returned when an event handler is added to a Windows Runtime event. The token is used to remove the event handler from the event at a later time. </summary>
public struct EventRegistrationToken
{
	internal ulong m_value;

	internal ulong Value => m_value;

	internal EventRegistrationToken(ulong value)
	{
		m_value = value;
	}

	/// <summary>Indicates whether two <see cref="T:System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken" /> instances are equal. </summary>
	/// <returns>true if the two objects are equal; otherwise, false. </returns>
	/// <param name="left">The first instance to compare. </param>
	/// <param name="right">The second instance to compare. </param>
	public static bool operator ==(EventRegistrationToken left, EventRegistrationToken right)
	{
		return left.Equals(right);
	}

	/// <summary>Indicates whether two <see cref="T:System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken" /> instances are not equal.</summary>
	/// <returns>true if the two instances are not equal; otherwise, false. </returns>
	/// <param name="left">The first instance to compare. </param>
	/// <param name="right">The second instance to compare. </param>
	public static bool operator !=(EventRegistrationToken left, EventRegistrationToken right)
	{
		return !left.Equals(right);
	}

	/// <summary>Returns a value that indicates whether the current object is equal to the specified object. </summary>
	/// <returns>true  if the current object is equal to <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is EventRegistrationToken eventRegistrationToken))
		{
			return false;
		}
		return eventRegistrationToken.Value == Value;
	}

	/// <summary>Returns the hash code for this instance. </summary>
	/// <returns>The hash code for this instance. </returns>
	public override int GetHashCode()
	{
		return m_value.GetHashCode();
	}
}
