using System.Runtime.InteropServices;

namespace System.Reflection;

/// <summary>Provides a remotable version of the AssemblyName.</summary>
[ComVisible(true)]
public class AssemblyNameProxy : MarshalByRefObject
{
	/// <summary>Gets the AssemblyName for a given file.</summary>
	/// <returns>An AssemblyName object representing the given file.</returns>
	/// <param name="assemblyFile">The assembly file for which to get the AssemblyName. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="assemblyFile" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="assemblyFile" /> is empty. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="assemblyFile" /> is not found. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.BadImageFormatException">
	///   <paramref name="assemblyFile" /> is not a valid assembly. </exception>
	public AssemblyName GetAssemblyName(string assemblyFile)
	{
		return AssemblyName.GetAssemblyName(assemblyFile);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.AssemblyNameProxy" /> class.</summary>
	public AssemblyNameProxy()
	{
	}
}
