namespace System.Runtime.InteropServices;

/// <summary>Indicates that the attributed assembly is a primary interop assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
[ComVisible(true)]
public sealed class PrimaryInteropAssemblyAttribute : Attribute
{
	internal int _major;

	internal int _minor;

	/// <summary>Gets the major version number of the type library for which this assembly is the primary interop assembly.</summary>
	/// <returns>The major version number of the type library for which this assembly is the primary interop assembly.</returns>
	public int MajorVersion => _major;

	/// <summary>Gets the minor version number of the type library for which this assembly is the primary interop assembly.</summary>
	/// <returns>The minor version number of the type library for which this assembly is the primary interop assembly.</returns>
	public int MinorVersion => _minor;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute" /> class with the major and minor version numbers of the type library for which this assembly is the primary interop assembly.</summary>
	/// <param name="major">The major version of the type library for which this assembly is the primary interop assembly. </param>
	/// <param name="minor">The minor version of the type library for which this assembly is the primary interop assembly. </param>
	public PrimaryInteropAssemblyAttribute(int major, int minor)
	{
		_major = major;
		_minor = minor;
	}
}
