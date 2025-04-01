namespace System.Globalization;

/// <summary>Provides information about the version of Unicode used to compare and order strings.</summary>
[Serializable]
public sealed class SortVersion : IEquatable<SortVersion>
{
	private int m_NlsVersion;

	private Guid m_SortId;

	/// <summary>Gets the full version number of the <see cref="T:System.Globalization.SortVersion" /> object.</summary>
	/// <returns>The version number of this <see cref="T:System.Globalization.SortVersion" /> object.</returns>
	public int FullVersion => m_NlsVersion;

	/// <summary>Gets a globally unique identifier for this <see cref="T:System.Globalization.SortVersion" /> object.</summary>
	/// <returns>A globally unique identifier for this <see cref="T:System.Globalization.SortVersion" /> object.</returns>
	public Guid SortId => m_SortId;

	/// <summary>Creates a new instance of the <see cref="T:System.Globalization.SortVersion" /> class.</summary>
	/// <param name="fullVersion">A version number.</param>
	/// <param name="sortId">A sort ID.</param>
	public SortVersion(int fullVersion, Guid sortId)
	{
		m_SortId = sortId;
		m_NlsVersion = fullVersion;
	}

	internal SortVersion(int nlsVersion, int effectiveId, Guid customVersion)
	{
		m_NlsVersion = nlsVersion;
		if (customVersion == Guid.Empty)
		{
			byte h = (byte)((uint)effectiveId >> 24);
			byte i = (byte)((effectiveId & 0xFF0000) >> 16);
			byte j = (byte)((effectiveId & 0xFF00) >> 8);
			byte k = (byte)(effectiveId & 0xFF);
			customVersion = new Guid(0, 0, 0, 0, 0, 0, 0, h, i, j, k);
		}
		m_SortId = customVersion;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Globalization.SortVersion" /> instance is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.Globalization.SortVersion" /> object that represents the same version as this instance; otherwise, false.</returns>
	/// <param name="obj">An object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		SortVersion sortVersion = obj as SortVersion;
		if (sortVersion != null)
		{
			return Equals(sortVersion);
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Globalization.SortVersion" /> instance is equal to a specified <see cref="T:System.Globalization.SortVersion" /> object.</summary>
	/// <returns>true if <paramref name="other" /> represents the same version as this instance; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	public bool Equals(SortVersion other)
	{
		if (other == null)
		{
			return false;
		}
		if (m_NlsVersion == other.m_NlsVersion)
		{
			return m_SortId == other.m_SortId;
		}
		return false;
	}

	/// <summary>Returns a hash code for this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return (m_NlsVersion * 7) | m_SortId.GetHashCode();
	}

	/// <summary>Indicates whether two <see cref="T:System.Globalization.SortVersion" /> instances are equal.</summary>
	/// <returns>true if the values of <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	/// <param name="left">The first instance to compare.</param>
	/// <param name="right">The second instance to compare.</param>
	public static bool operator ==(SortVersion left, SortVersion right)
	{
		return left?.Equals(right) ?? right?.Equals(left) ?? true;
	}

	/// <summary>Indicates whether two <see cref="T:System.Globalization.SortVersion" /> instances are not equal.</summary>
	/// <returns>true if the values of <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first instance to compare.</param>
	/// <param name="right">The second instance to compare.</param>
	public static bool operator !=(SortVersion left, SortVersion right)
	{
		return !(left == right);
	}
}
