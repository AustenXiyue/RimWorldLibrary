using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines the interfaces used by the internal design time API to create a <see cref="T:System.Configuration.Configuration" /> object.</summary>
[ComVisible(false)]
public interface IInternalConfigConfigurationFactory
{
	/// <summary>Creates and initializes a <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.Configuration" /> object.</returns>
	/// <param name="typeConfigHost">The <see cref="T:System.Type" /> of the <see cref="T:System.Configuration.Configuration" /> object to be created.</param>
	/// <param name="hostInitConfigurationParams">A parameter array of <see cref="T:System.Object" /> that contains the parameters to be applied to the created <see cref="T:System.Configuration.Configuration" /> object.</param>
	Configuration Create(Type typeConfigHost, params object[] hostInitConfigurationParams);

	/// <summary>Normalizes a location subpath of a path to a configuration file.</summary>
	/// <returns>A normalized subpath string.</returns>
	/// <param name="subPath">A string representing the path to the configuration file.</param>
	/// <param name="errorInfo">An instance of <see cref="T:System.Configuration.Internal.IConfigErrorInfo" /> or null.</param>
	string NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo);
}
