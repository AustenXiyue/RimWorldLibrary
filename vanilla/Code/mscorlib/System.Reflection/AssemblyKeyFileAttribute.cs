using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Specifies the name of a file containing the key pair used to generate a strong name.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
public sealed class AssemblyKeyFileAttribute : Attribute
{
	private string m_keyFile;

	/// <summary>Gets the name of the file containing the key pair used to generate a strong name for the attributed assembly.</summary>
	/// <returns>A string containing the name of the file that contains the key pair.</returns>
	public string KeyFile => m_keyFile;

	/// <summary>Initializes a new instance of the AssemblyKeyFileAttribute class with the name of the file containing the key pair to generate a strong name for the assembly being attributed.</summary>
	/// <param name="keyFile">The name of the file containing the key pair. </param>
	public AssemblyKeyFileAttribute(string keyFile)
	{
		m_keyFile = keyFile;
	}
}
