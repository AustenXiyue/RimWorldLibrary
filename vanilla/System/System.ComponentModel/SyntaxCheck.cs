using System.IO;
using System.Security.Permissions;

namespace System.ComponentModel;

/// <summary>Provides methods to verify the machine name and path conform to a specific syntax. This class cannot be inherited.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public static class SyntaxCheck
{
	/// <summary>Checks the syntax of the machine name to confirm that it does not contain "\".</summary>
	/// <returns>true if <paramref name="value" /> matches the proper machine name format; otherwise, false.</returns>
	/// <param name="value">A string containing the machine name to check. </param>
	public static bool CheckMachineName(string value)
	{
		if (value == null)
		{
			return false;
		}
		value = value.Trim();
		if (value.Equals(string.Empty))
		{
			return false;
		}
		return value.IndexOf('\\') == -1;
	}

	/// <summary>Checks the syntax of the path to see whether it starts with "\\".</summary>
	/// <returns>true if <paramref name="value" /> matches the proper path format; otherwise, false.</returns>
	/// <param name="value">A string containing the path to check. </param>
	public static bool CheckPath(string value)
	{
		if (value == null)
		{
			return false;
		}
		value = value.Trim();
		if (value.Equals(string.Empty))
		{
			return false;
		}
		return value.StartsWith("\\\\");
	}

	/// <summary>Checks the syntax of the path to see if it starts with "\" or drive letter "C:".</summary>
	/// <returns>true if <paramref name="value" /> matches the proper path format; otherwise, false.</returns>
	/// <param name="value">A string containing the path to check. </param>
	public static bool CheckRootedPath(string value)
	{
		if (value == null)
		{
			return false;
		}
		value = value.Trim();
		if (value.Equals(string.Empty))
		{
			return false;
		}
		return Path.IsPathRooted(value);
	}
}
