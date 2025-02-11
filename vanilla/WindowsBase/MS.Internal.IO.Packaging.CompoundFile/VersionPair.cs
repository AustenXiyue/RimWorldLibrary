using System;
using System.Runtime.CompilerServices;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class VersionPair : IComparable
{
	private short _major;

	private short _minor;

	public short Major => _major;

	public short Minor => _minor;

	internal VersionPair(short major, short minor)
	{
		if (major < 0)
		{
			throw new ArgumentOutOfRangeException("major", SR.VersionNumberComponentNegative);
		}
		if (minor < 0)
		{
			throw new ArgumentOutOfRangeException("minor", SR.VersionNumberComponentNegative);
		}
		_major = major;
		_minor = minor;
	}

	public override string ToString()
	{
		IFormatProvider formatProvider = null;
		IFormatProvider provider = formatProvider;
		Span<char> initialBuffer = stackalloc char[64];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(3, 2, formatProvider, initialBuffer);
		handler.AppendLiteral("(");
		handler.AppendFormatted(_major);
		handler.AppendLiteral(",");
		handler.AppendFormatted(_minor);
		handler.AppendLiteral(")");
		return string.Create(provider, initialBuffer, ref handler);
	}

	public static bool operator ==(VersionPair v1, VersionPair v2)
	{
		bool result = false;
		if ((object)v1 == null && (object)v2 == null)
		{
			result = true;
		}
		else if ((object)v1 != null && (object)v2 != null && v1.Major == v2.Major && v1.Minor == v2.Minor)
		{
			result = true;
		}
		return result;
	}

	public static bool operator !=(VersionPair v1, VersionPair v2)
	{
		return !(v1 == v2);
	}

	public static bool operator <(VersionPair v1, VersionPair v2)
	{
		bool result = false;
		if ((object)v1 == null && (object)v2 != null)
		{
			result = true;
		}
		else if ((object)v1 != null && (object)v2 != null && (v1.Major < v2.Major || (v1.Major == v2.Major && v1.Minor < v2.Minor)))
		{
			result = true;
		}
		return result;
	}

	public static bool operator >(VersionPair v1, VersionPair v2)
	{
		bool result = false;
		if ((object)v1 != null && (object)v2 == null)
		{
			result = true;
		}
		else if ((object)v1 != null && (object)v2 != null && !(v1 < v2) && v1 != v2)
		{
			return true;
		}
		return result;
	}

	public static bool operator <=(VersionPair v1, VersionPair v2)
	{
		if (!(v1 > v2))
		{
			return true;
		}
		return false;
	}

	public static bool operator >=(VersionPair v1, VersionPair v2)
	{
		if (!(v1 < v2))
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		VersionPair versionPair = (VersionPair)obj;
		if (this != versionPair)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return _major << 16 + _minor;
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (obj.GetType() != GetType())
		{
			throw new ArgumentException(SR.ExpectedVersionPairObject);
		}
		VersionPair versionPair = (VersionPair)obj;
		if (Equals(obj))
		{
			return 0;
		}
		if (this < versionPair)
		{
			return -1;
		}
		return 1;
	}
}
