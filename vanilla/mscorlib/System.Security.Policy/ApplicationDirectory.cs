using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Policy;

/// <summary>Provides the application directory as evidence for policy evaluation. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class ApplicationDirectory : EvidenceBase, IBuiltInEvidence
{
	private string directory;

	/// <summary>Gets the path of the application directory.</summary>
	/// <returns>The path of the application directory.</returns>
	public string Directory => directory;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.ApplicationDirectory" /> class.</summary>
	/// <param name="name">The path of the application directory. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	public ApplicationDirectory(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length < 1)
		{
			throw new FormatException(Locale.GetText("Empty"));
		}
		directory = name;
	}

	/// <summary>Creates a new copy of the <see cref="T:System.Security.Policy.ApplicationDirectory" />.</summary>
	/// <returns>A new, identical copy of the <see cref="T:System.Security.Policy.ApplicationDirectory" />.</returns>
	public object Copy()
	{
		return new ApplicationDirectory(Directory);
	}

	/// <summary>Determines whether instances of the same type of an evidence object are equivalent.</summary>
	/// <returns>true if the two instances are equivalent; otherwise, false.</returns>
	/// <param name="o">An object of same type as the current evidence object. </param>
	public override bool Equals(object o)
	{
		if (o is ApplicationDirectory applicationDirectory)
		{
			ThrowOnInvalid(applicationDirectory.directory);
			return directory == applicationDirectory.directory;
		}
		return false;
	}

	/// <summary>Gets the hash code of the current application directory.</summary>
	/// <returns>The hash code of the current application directory.</returns>
	public override int GetHashCode()
	{
		return Directory.GetHashCode();
	}

	/// <summary>Gets a string representation of the state of the <see cref="T:System.Security.Policy.ApplicationDirectory" /> evidence object.</summary>
	/// <returns>A representation of the state of the <see cref="T:System.Security.Policy.ApplicationDirectory" /> evidence object.</returns>
	public override string ToString()
	{
		ThrowOnInvalid(Directory);
		SecurityElement securityElement = new SecurityElement("System.Security.Policy.ApplicationDirectory");
		securityElement.AddAttribute("version", "1");
		securityElement.AddChild(new SecurityElement("Directory", directory));
		return securityElement.ToString();
	}

	int IBuiltInEvidence.GetRequiredSize(bool verbose)
	{
		return ((!verbose) ? 1 : 3) + directory.Length;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer(char[] buffer, int position)
	{
		return 0;
	}

	[MonoTODO("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer(char[] buffer, int position, bool verbose)
	{
		return 0;
	}

	private void ThrowOnInvalid(string appdir)
	{
		if (appdir.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException(string.Format(Locale.GetText("Invalid character(s) in directory {0}"), appdir), "other");
		}
	}
}
