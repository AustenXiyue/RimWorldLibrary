using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System;

/// <summary>Represents the version number of an assembly, operating system, or the common language runtime. This class cannot be inherited.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
{
	internal enum ParseFailureKind
	{
		ArgumentNullException,
		ArgumentException,
		ArgumentOutOfRangeException,
		FormatException
	}

	internal struct VersionResult
	{
		internal Version m_parsedVersion;

		internal ParseFailureKind m_failure;

		internal string m_exceptionArgument;

		internal string m_argumentName;

		internal bool m_canThrow;

		internal void Init(string argumentName, bool canThrow)
		{
			m_canThrow = canThrow;
			m_argumentName = argumentName;
		}

		internal void SetFailure(ParseFailureKind failure)
		{
			SetFailure(failure, string.Empty);
		}

		internal void SetFailure(ParseFailureKind failure, string argument)
		{
			m_failure = failure;
			m_exceptionArgument = argument;
			if (m_canThrow)
			{
				throw GetVersionParseException();
			}
		}

		internal Exception GetVersionParseException()
		{
			switch (m_failure)
			{
			case ParseFailureKind.ArgumentNullException:
				return new ArgumentNullException(m_argumentName);
			case ParseFailureKind.ArgumentException:
				return new ArgumentException(Environment.GetResourceString("Version string portion was too short or too long."));
			case ParseFailureKind.ArgumentOutOfRangeException:
				return new ArgumentOutOfRangeException(m_exceptionArgument, Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
			case ParseFailureKind.FormatException:
				try
				{
					int.Parse(m_exceptionArgument, CultureInfo.InvariantCulture);
				}
				catch (FormatException result)
				{
					return result;
				}
				catch (OverflowException result2)
				{
					return result2;
				}
				return new FormatException(Environment.GetResourceString("Input string was not in a correct format."));
			default:
				return new ArgumentException(Environment.GetResourceString("Version string portion was too short or too long."));
			}
		}
	}

	private int _Major;

	private int _Minor;

	private int _Build = -1;

	private int _Revision = -1;

	private static readonly char[] SeparatorsArray = new char[1] { '.' };

	private const int ZERO_CHAR_VALUE = 48;

	/// <summary>Gets the value of the major component of the version number for the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>The major version number.</returns>
	/// <filterpriority>1</filterpriority>
	public int Major => _Major;

	/// <summary>Gets the value of the minor component of the version number for the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>The minor version number.</returns>
	/// <filterpriority>1</filterpriority>
	public int Minor => _Minor;

	/// <summary>Gets the value of the build component of the version number for the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>The build number, or -1 if the build number is undefined.</returns>
	/// <filterpriority>1</filterpriority>
	public int Build => _Build;

	/// <summary>Gets the value of the revision component of the version number for the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>The revision number, or -1 if the revision number is undefined.</returns>
	/// <filterpriority>1</filterpriority>
	public int Revision => _Revision;

	/// <summary>Gets the high 16 bits of the revision number.</summary>
	/// <returns>A 16-bit signed integer.</returns>
	public short MajorRevision => (short)(_Revision >> 16);

	/// <summary>Gets the low 16 bits of the revision number.</summary>
	/// <returns>A 16-bit signed integer.</returns>
	public short MinorRevision => (short)(_Revision & 0xFFFF);

	/// <summary>Initializes a new instance of the <see cref="T:System.Version" /> class with the specified major, minor, build, and revision numbers.</summary>
	/// <param name="major">The major version number. </param>
	/// <param name="minor">The minor version number. </param>
	/// <param name="build">The build number. </param>
	/// <param name="revision">The revision number. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="major" />, <paramref name="minor" />, <paramref name="build" />, or <paramref name="revision" /> is less than zero. </exception>
	public Version(int major, int minor, int build, int revision)
	{
		if (major < 0)
		{
			throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (minor < 0)
		{
			throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (build < 0)
		{
			throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (revision < 0)
		{
			throw new ArgumentOutOfRangeException("revision", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		_Major = major;
		_Minor = minor;
		_Build = build;
		_Revision = revision;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Version" /> class using the specified major, minor, and build values.</summary>
	/// <param name="major">The major version number. </param>
	/// <param name="minor">The minor version number. </param>
	/// <param name="build">The build number. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="major" />, <paramref name="minor" />, or <paramref name="build" /> is less than zero. </exception>
	public Version(int major, int minor, int build)
	{
		if (major < 0)
		{
			throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (minor < 0)
		{
			throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (build < 0)
		{
			throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		_Major = major;
		_Minor = minor;
		_Build = build;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Version" /> class using the specified major and minor values.</summary>
	/// <param name="major">The major version number. </param>
	/// <param name="minor">The minor version number. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="major" /> or <paramref name="minor" /> is less than zero. </exception>
	public Version(int major, int minor)
	{
		if (major < 0)
		{
			throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		if (minor < 0)
		{
			throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("Version's parameters must be greater than or equal to zero."));
		}
		_Major = major;
		_Minor = minor;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Version" /> class using the specified string.</summary>
	/// <param name="version">A string containing the major, minor, build, and revision numbers, where each number is delimited with a period character ('.'). </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="version" /> has fewer than two components or more than four components. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="version" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">A major, minor, build, or revision component is less than zero. </exception>
	/// <exception cref="T:System.FormatException">At least one component of <paramref name="version" /> does not parse to an integer. </exception>
	/// <exception cref="T:System.OverflowException">At least one component of <paramref name="version" /> represents a number greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public Version(string version)
	{
		Version version2 = Parse(version);
		_Major = version2.Major;
		_Minor = version2.Minor;
		_Build = version2.Build;
		_Revision = version2.Revision;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Version" /> class.</summary>
	public Version()
	{
		_Major = 0;
		_Minor = 0;
	}

	/// <summary>Returns a new <see cref="T:System.Version" /> object whose value is the same as the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>A new <see cref="T:System.Object" /> whose values are a copy of the current <see cref="T:System.Version" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new Version
		{
			_Major = _Major,
			_Minor = _Minor,
			_Build = _Build,
			_Revision = _Revision
		};
	}

	/// <summary>Compares the current <see cref="T:System.Version" /> object to a specified object and returns an indication of their relative values.</summary>
	/// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.Return value Meaning Less than zero The current <see cref="T:System.Version" /> object is a version before <paramref name="version" />. Zero The current <see cref="T:System.Version" /> object is the same version as <paramref name="version" />. Greater than zero The current <see cref="T:System.Version" /> object is a version subsequent to <paramref name="version" />.-or- <paramref name="version" /> is null. </returns>
	/// <param name="version">An object to compare, or null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="version" /> is not of type <see cref="T:System.Version" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int CompareTo(object version)
	{
		if (version == null)
		{
			return 1;
		}
		Version version2 = version as Version;
		if (version2 == null)
		{
			throw new ArgumentException(Environment.GetResourceString("Object must be of type Version."));
		}
		if (_Major != version2._Major)
		{
			if (_Major > version2._Major)
			{
				return 1;
			}
			return -1;
		}
		if (_Minor != version2._Minor)
		{
			if (_Minor > version2._Minor)
			{
				return 1;
			}
			return -1;
		}
		if (_Build != version2._Build)
		{
			if (_Build > version2._Build)
			{
				return 1;
			}
			return -1;
		}
		if (_Revision != version2._Revision)
		{
			if (_Revision > version2._Revision)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	/// <summary>Compares the current <see cref="T:System.Version" /> object to a specified <see cref="T:System.Version" /> object and returns an indication of their relative values.</summary>
	/// <returns>A signed integer that indicates the relative values of the two objects, as shown in the following table.Return value Meaning Less than zero The current <see cref="T:System.Version" /> object is a version before <paramref name="value" />. Zero The current <see cref="T:System.Version" /> object is the same version as <paramref name="value" />. Greater than zero The current <see cref="T:System.Version" /> object is a version subsequent to <paramref name="value" />. -or-<paramref name="value" /> is null.</returns>
	/// <param name="value">A <see cref="T:System.Version" /> object to compare to the current <see cref="T:System.Version" /> object, or null.</param>
	/// <filterpriority>1</filterpriority>
	public int CompareTo(Version value)
	{
		if (value == null)
		{
			return 1;
		}
		if (_Major != value._Major)
		{
			if (_Major > value._Major)
			{
				return 1;
			}
			return -1;
		}
		if (_Minor != value._Minor)
		{
			if (_Minor > value._Minor)
			{
				return 1;
			}
			return -1;
		}
		if (_Build != value._Build)
		{
			if (_Build > value._Build)
			{
				return 1;
			}
			return -1;
		}
		if (_Revision != value._Revision)
		{
			if (_Revision > value._Revision)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	/// <summary>Returns a value indicating whether the current <see cref="T:System.Version" /> object is equal to a specified object.</summary>
	/// <returns>true if the current <see cref="T:System.Version" /> object and <paramref name="obj" /> are both <see cref="T:System.Version" /> objects, and every component of the current <see cref="T:System.Version" /> object matches the corresponding component of <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">An object to compare with the current <see cref="T:System.Version" /> object, or null. </param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object obj)
	{
		Version version = obj as Version;
		if (version == null)
		{
			return false;
		}
		if (_Major != version._Major || _Minor != version._Minor || _Build != version._Build || _Revision != version._Revision)
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns a value indicating whether the current <see cref="T:System.Version" /> object and a specified <see cref="T:System.Version" /> object represent the same value.</summary>
	/// <returns>true if every component of the current <see cref="T:System.Version" /> object matches the corresponding component of the <paramref name="obj" /> parameter; otherwise, false.</returns>
	/// <param name="obj">A <see cref="T:System.Version" /> object to compare to the current <see cref="T:System.Version" /> object, or null.</param>
	/// <filterpriority>1</filterpriority>
	public bool Equals(Version obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (_Major != obj._Major || _Minor != obj._Minor || _Build != obj._Build || _Revision != obj._Revision)
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns a hash code for the current <see cref="T:System.Version" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return 0 | ((_Major & 0xF) << 28) | ((_Minor & 0xFF) << 20) | ((_Build & 0xFF) << 12) | (_Revision & 0xFFF);
	}

	/// <summary>Converts the value of the current <see cref="T:System.Version" /> object to its equivalent <see cref="T:System.String" /> representation.</summary>
	/// <returns>The <see cref="T:System.String" /> representation of the values of the major, minor, build, and revision components of the current <see cref="T:System.Version" /> object, as depicted in the following format. Each component is separated by a period character ('.'). Square brackets ('[' and ']') indicate a component that will not appear in the return value if the component is not defined: major.minor[.build[.revision]] For example, if you create a <see cref="T:System.Version" /> object using the constructor Version(1,1), the returned string is "1.1". If you create a <see cref="T:System.Version" /> object using the constructor Version(1,3,4,2), the returned string is "1.3.4.2".</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		if (_Build == -1)
		{
			return ToString(2);
		}
		if (_Revision == -1)
		{
			return ToString(3);
		}
		return ToString(4);
	}

	/// <summary>Converts the value of the current <see cref="T:System.Version" /> object to its equivalent <see cref="T:System.String" /> representation. A specified count indicates the number of components to return.</summary>
	/// <returns>The <see cref="T:System.String" /> representation of the values of the major, minor, build, and revision components of the current <see cref="T:System.Version" /> object, each separated by a period character ('.'). The <paramref name="fieldCount" /> parameter determines how many components are returned.fieldCount Return Value 0 An empty string (""). 1 major 2 major.minor 3 major.minor.build 4 major.minor.build.revision For example, if you create <see cref="T:System.Version" /> object using the constructor Version(1,3,5), ToString(2) returns "1.3" and ToString(4) throws an exception.</returns>
	/// <param name="fieldCount">The number of components to return. The <paramref name="fieldCount" /> ranges from 0 to 4. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fieldCount" /> is less than 0, or more than 4.-or- <paramref name="fieldCount" /> is more than the number of components defined in the current <see cref="T:System.Version" /> object. </exception>
	/// <filterpriority>1</filterpriority>
	public string ToString(int fieldCount)
	{
		switch (fieldCount)
		{
		case 0:
			return string.Empty;
		case 1:
			return _Major.ToString();
		case 2:
		{
			StringBuilder stringBuilder = StringBuilderCache.Acquire();
			AppendPositiveNumber(_Major, stringBuilder);
			stringBuilder.Append('.');
			AppendPositiveNumber(_Minor, stringBuilder);
			return StringBuilderCache.GetStringAndRelease(stringBuilder);
		}
		default:
			if (_Build == -1)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument must be between {0} and {1}.", "0", "2"), "fieldCount");
			}
			if (fieldCount == 3)
			{
				StringBuilder stringBuilder = StringBuilderCache.Acquire();
				AppendPositiveNumber(_Major, stringBuilder);
				stringBuilder.Append('.');
				AppendPositiveNumber(_Minor, stringBuilder);
				stringBuilder.Append('.');
				AppendPositiveNumber(_Build, stringBuilder);
				return StringBuilderCache.GetStringAndRelease(stringBuilder);
			}
			if (_Revision == -1)
			{
				throw new ArgumentException(Environment.GetResourceString("Argument must be between {0} and {1}.", "0", "3"), "fieldCount");
			}
			if (fieldCount == 4)
			{
				StringBuilder stringBuilder = StringBuilderCache.Acquire();
				AppendPositiveNumber(_Major, stringBuilder);
				stringBuilder.Append('.');
				AppendPositiveNumber(_Minor, stringBuilder);
				stringBuilder.Append('.');
				AppendPositiveNumber(_Build, stringBuilder);
				stringBuilder.Append('.');
				AppendPositiveNumber(_Revision, stringBuilder);
				return StringBuilderCache.GetStringAndRelease(stringBuilder);
			}
			throw new ArgumentException(Environment.GetResourceString("Argument must be between {0} and {1}.", "0", "4"), "fieldCount");
		}
	}

	private static void AppendPositiveNumber(int num, StringBuilder sb)
	{
		int length = sb.Length;
		do
		{
			int num2 = num % 10;
			num /= 10;
			sb.Insert(length, (char)(48 + num2));
		}
		while (num > 0);
	}

	/// <summary>Converts the string representation of a version number to an equivalent <see cref="T:System.Version" /> object.</summary>
	/// <returns>An object that is equivalent to the version number specified in the <paramref name="input" /> parameter.</returns>
	/// <param name="input">A string that contains a version number to convert.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="input" /> has fewer than two or more than four version components.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">At least one component in <paramref name="input" /> is less than zero.</exception>
	/// <exception cref="T:System.FormatException">At least one component in <paramref name="input" /> is not an integer.</exception>
	/// <exception cref="T:System.OverflowException">At least one component in <paramref name="input" /> represents a number that is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	public static Version Parse(string input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		VersionResult result = default(VersionResult);
		result.Init("input", canThrow: true);
		if (!TryParseVersion(input, ref result))
		{
			throw result.GetVersionParseException();
		}
		return result.m_parsedVersion;
	}

	/// <summary>Tries to convert the string representation of a version number to an equivalent <see cref="T:System.Version" /> object, and returns a value that indicates whether the conversion succeeded.</summary>
	/// <returns>true if the <paramref name="input" /> parameter was converted successfully; otherwise, false.</returns>
	/// <param name="input">A string that contains a version number to convert.</param>
	/// <param name="result">When this method returns, contains the <see cref="T:System.Version" /> equivalent of the number that is contained in <paramref name="input" />, if the conversion succeeded, or a <see cref="T:System.Version" /> object whose major and minor version numbers are 0 if the conversion failed.</param>
	public static bool TryParse(string input, out Version result)
	{
		VersionResult result2 = default(VersionResult);
		result2.Init("input", canThrow: false);
		bool result3 = TryParseVersion(input, ref result2);
		result = result2.m_parsedVersion;
		return result3;
	}

	private static bool TryParseVersion(string version, ref VersionResult result)
	{
		if (version == null)
		{
			result.SetFailure(ParseFailureKind.ArgumentNullException);
			return false;
		}
		string[] array = version.Split(SeparatorsArray);
		int num = array.Length;
		if (num < 2 || num > 4)
		{
			result.SetFailure(ParseFailureKind.ArgumentException);
			return false;
		}
		if (!TryParseComponent(array[0], "version", ref result, out var parsedComponent))
		{
			return false;
		}
		if (!TryParseComponent(array[1], "version", ref result, out var parsedComponent2))
		{
			return false;
		}
		num -= 2;
		if (num > 0)
		{
			if (!TryParseComponent(array[2], "build", ref result, out var parsedComponent3))
			{
				return false;
			}
			num--;
			if (num > 0)
			{
				if (!TryParseComponent(array[3], "revision", ref result, out var parsedComponent4))
				{
					return false;
				}
				result.m_parsedVersion = new Version(parsedComponent, parsedComponent2, parsedComponent3, parsedComponent4);
			}
			else
			{
				result.m_parsedVersion = new Version(parsedComponent, parsedComponent2, parsedComponent3);
			}
		}
		else
		{
			result.m_parsedVersion = new Version(parsedComponent, parsedComponent2);
		}
		return true;
	}

	private static bool TryParseComponent(string component, string componentName, ref VersionResult result, out int parsedComponent)
	{
		if (!int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent))
		{
			result.SetFailure(ParseFailureKind.FormatException, component);
			return false;
		}
		if (parsedComponent < 0)
		{
			result.SetFailure(ParseFailureKind.ArgumentOutOfRangeException, componentName);
			return false;
		}
		return true;
	}

	/// <summary>Determines whether two specified <see cref="T:System.Version" /> objects are equal.</summary>
	/// <returns>true if <paramref name="v1" /> equals <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(Version v1, Version v2)
	{
		return v1?.Equals(v2) ?? ((object)v2 == null);
	}

	/// <summary>Determines whether two specified <see cref="T:System.Version" /> objects are not equal.</summary>
	/// <returns>true if <paramref name="v1" /> does not equal <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(Version v1, Version v2)
	{
		return !(v1 == v2);
	}

	/// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is less than the second specified <see cref="T:System.Version" /> object.</summary>
	/// <returns>true if <paramref name="v1" /> is less than <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="v1" /> is null. </exception>
	/// <filterpriority>3</filterpriority>
	public static bool operator <(Version v1, Version v2)
	{
		if ((object)v1 == null)
		{
			throw new ArgumentNullException("v1");
		}
		return v1.CompareTo(v2) < 0;
	}

	/// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is less than or equal to the second <see cref="T:System.Version" /> object.</summary>
	/// <returns>true if <paramref name="v1" /> is less than or equal to <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="v1" /> is null. </exception>
	/// <filterpriority>3</filterpriority>
	public static bool operator <=(Version v1, Version v2)
	{
		if ((object)v1 == null)
		{
			throw new ArgumentNullException("v1");
		}
		return v1.CompareTo(v2) <= 0;
	}

	/// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is greater than the second specified <see cref="T:System.Version" /> object.</summary>
	/// <returns>true if <paramref name="v1" /> is greater than <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >(Version v1, Version v2)
	{
		return v2 < v1;
	}

	/// <summary>Determines whether the first specified <see cref="T:System.Version" /> object is greater than or equal to the second specified <see cref="T:System.Version" /> object.</summary>
	/// <returns>true if <paramref name="v1" /> is greater than or equal to <paramref name="v2" />; otherwise, false.</returns>
	/// <param name="v1">The first <see cref="T:System.Version" /> object. </param>
	/// <param name="v2">The second <see cref="T:System.Version" /> object. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator >=(Version v1, Version v2)
	{
		return v2 <= v1;
	}
}
