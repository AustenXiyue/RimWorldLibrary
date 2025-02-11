using System.Text;

namespace System.Runtime.Versioning;

/// <summary>Represents the name of a version of the .NET Framework.</summary>
[Serializable]
public sealed class FrameworkName : IEquatable<FrameworkName>
{
	private readonly string m_identifier;

	private readonly Version m_version;

	private readonly string m_profile;

	private string m_fullName;

	private const char c_componentSeparator = ',';

	private const char c_keyValueSeparator = '=';

	private const char c_versionValuePrefix = 'v';

	private const string c_versionKey = "Version";

	private const string c_profileKey = "Profile";

	/// <summary>Gets the identifier of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>The identifier of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</returns>
	public string Identifier => m_identifier;

	/// <summary>Gets the version of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>An object that contains version information about this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</returns>
	public Version Version => m_version;

	/// <summary>Gets the profile name of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>The profile name of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</returns>
	public string Profile => m_profile;

	/// <summary>Gets the full name of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>The full name of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</returns>
	public string FullName
	{
		get
		{
			if (m_fullName == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(Identifier);
				stringBuilder.Append(',');
				stringBuilder.Append("Version").Append('=');
				stringBuilder.Append('v');
				stringBuilder.Append(Version);
				if (!string.IsNullOrEmpty(Profile))
				{
					stringBuilder.Append(',');
					stringBuilder.Append("Profile").Append('=');
					stringBuilder.Append(Profile);
				}
				m_fullName = stringBuilder.ToString();
			}
			return m_fullName;
		}
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Runtime.Versioning.FrameworkName" /> instance represents the same .NET Framework version as a specified object.</summary>
	/// <returns>true if every component of the current <see cref="T:System.Runtime.Versioning.FrameworkName" /> object matches the corresponding component of <paramref name="obj" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare to the current instance.</param>
	public override bool Equals(object obj)
	{
		return Equals(obj as FrameworkName);
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Runtime.Versioning.FrameworkName" /> instance represents the same .NET Framework version as a specified <see cref="T:System.Runtime.Versioning.FrameworkName" /> instance.</summary>
	/// <returns>true if every component of the current <see cref="T:System.Runtime.Versioning.FrameworkName" /> object matches the corresponding component of <paramref name="other" />; otherwise, false.</returns>
	/// <param name="other">The object to compare to the current instance.</param>
	public bool Equals(FrameworkName other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (Identifier == other.Identifier && Version == other.Version)
		{
			return Profile == other.Profile;
		}
		return false;
	}

	/// <summary>Returns the hash code for the <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>A 32-bit signed integer that represents the hash code of this instance.</returns>
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^ Version.GetHashCode() ^ Profile.GetHashCode();
	}

	/// <summary>Returns the string representation of this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</summary>
	/// <returns>A string that represents this <see cref="T:System.Runtime.Versioning.FrameworkName" /> object.</returns>
	public override string ToString()
	{
		return FullName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Versioning.FrameworkName" /> class from a string and a <see cref="T:System.Version" /> object that identify a .NET Framework version.</summary>
	/// <param name="identifier">A string that identifies a .NET Framework version. </param>
	/// <param name="version">An object that contains .NET Framework version information.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identifier" /> is <see cref="F:System.String.Empty" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identifier" /> is null.-or-<paramref name="version" /> is null.</exception>
	public FrameworkName(string identifier, Version version)
		: this(identifier, version, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Versioning.FrameworkName" /> class from a string, a <see cref="T:System.Version" /> object that identifies a .NET Framework version, and a profile name.</summary>
	/// <param name="identifier">A string that identifies a .NET Framework version.</param>
	/// <param name="version">An object that contains .NET Framework version information.</param>
	/// <param name="profile">A profile name.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="identifier" /> is <see cref="F:System.String.Empty" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="identifier" /> is null.-or-<paramref name="version" /> is null.</exception>
	public FrameworkName(string identifier, Version version, string profile)
	{
		if (identifier == null)
		{
			throw new ArgumentNullException("identifier");
		}
		if (identifier.Trim().Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "identifier"), "identifier");
		}
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		m_identifier = identifier.Trim();
		m_version = (Version)version.Clone();
		m_profile = ((profile == null) ? string.Empty : profile.Trim());
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Versioning.FrameworkName" /> class from a string that contains information about a version of the .NET Framework.</summary>
	/// <param name="frameworkName">A string that contains .NET Framework version information.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="frameworkName" /> is <see cref="F:System.String.Empty" />.-or-<paramref name="frameworkName" /> has fewer than two components or more than three components.-or-<paramref name="frameworkName" /> does not include a major and minor version number.-or-<paramref name="frameworkName " />does not include a valid version number.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="frameworkName" /> is null.</exception>
	public FrameworkName(string frameworkName)
	{
		if (frameworkName == null)
		{
			throw new ArgumentNullException("frameworkName");
		}
		if (frameworkName.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("The parameter '{0}' cannot be an empty string.", "frameworkName"), "frameworkName");
		}
		string[] array = frameworkName.Split(',');
		if (array.Length < 2 || array.Length > 3)
		{
			throw new ArgumentException(global::SR.GetString("FrameworkName cannot have less than two components or more than three components."), "frameworkName");
		}
		m_identifier = array[0].Trim();
		if (m_identifier.Length == 0)
		{
			throw new ArgumentException(global::SR.GetString("FrameworkName is invalid."), "frameworkName");
		}
		bool flag = false;
		m_profile = string.Empty;
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('=');
			if (array2.Length != 2)
			{
				throw new ArgumentException(global::SR.GetString("FrameworkName is invalid."), "frameworkName");
			}
			string text = array2[0].Trim();
			string text2 = array2[1].Trim();
			if (text.Equals("Version", StringComparison.OrdinalIgnoreCase))
			{
				flag = true;
				if (text2.Length > 0 && (text2[0] == 'v' || text2[0] == 'V'))
				{
					text2 = text2.Substring(1);
				}
				try
				{
					m_version = new Version(text2);
				}
				catch (Exception innerException)
				{
					throw new ArgumentException(global::SR.GetString("FrameworkName version component is invalid."), "frameworkName", innerException);
				}
			}
			else
			{
				if (!text.Equals("Profile", StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException(global::SR.GetString("FrameworkName is invalid."), "frameworkName");
				}
				if (!string.IsNullOrEmpty(text2))
				{
					m_profile = text2;
				}
			}
		}
		if (!flag)
		{
			throw new ArgumentException(global::SR.GetString("FrameworkName version component is missing."), "frameworkName");
		}
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Runtime.Versioning.FrameworkName" /> objects represent the same .NET Framework version.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters represent the same .NET Framework version; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(FrameworkName left, FrameworkName right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>Returns a value that indicates whether two <see cref="T:System.Runtime.Versioning.FrameworkName" /> objects represent different .NET Framework versions.</summary>
	/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters represent different .NET Framework versions; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(FrameworkName left, FrameworkName right)
	{
		return !(left == right);
	}
}
