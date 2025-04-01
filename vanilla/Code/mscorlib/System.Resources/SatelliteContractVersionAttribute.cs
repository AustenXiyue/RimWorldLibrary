using System.Runtime.InteropServices;

namespace System.Resources;

/// <summary>Instructs a <see cref="T:System.Resources.ResourceManager" /> object to ask for a particular version of a satellite assembly.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
[ComVisible(true)]
public sealed class SatelliteContractVersionAttribute : Attribute
{
	private string _version;

	/// <summary>Gets the version of the satellite assemblies with the required resources.</summary>
	/// <returns>A string that contains the version of the satellite assemblies with the required resources.</returns>
	public string Version => _version;

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.SatelliteContractVersionAttribute" /> class.</summary>
	/// <param name="version">A string that specifies the version of the satellite assemblies to load. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="version" /> parameter is null. </exception>
	public SatelliteContractVersionAttribute(string version)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		_version = version;
	}
}
