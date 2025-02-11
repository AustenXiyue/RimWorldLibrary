using System.Runtime.InteropServices;

namespace System.Resources;

/// <summary>Informs the resource manager of an app's default culture. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class NeutralResourcesLanguageAttribute : Attribute
{
	private string _culture;

	private UltimateResourceFallbackLocation _fallbackLoc;

	/// <summary>Gets the culture name.</summary>
	/// <returns>The name of the default culture for the main assembly.</returns>
	public string CultureName => _culture;

	/// <summary>Gets the location for the <see cref="T:System.Resources.ResourceManager" /> class to use to retrieve neutral resources by using the resource fallback process.</summary>
	/// <returns>One of the enumeration values that indicates the location (main assembly or satellite) from which to retrieve neutral resources.</returns>
	public UltimateResourceFallbackLocation Location => _fallbackLoc;

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.NeutralResourcesLanguageAttribute" /> class.</summary>
	/// <param name="cultureName">The name of the culture that the current assembly's neutral resources were written in. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="cultureName" /> parameter is null. </exception>
	public NeutralResourcesLanguageAttribute(string cultureName)
	{
		if (cultureName == null)
		{
			throw new ArgumentNullException("cultureName");
		}
		_culture = cultureName;
		_fallbackLoc = UltimateResourceFallbackLocation.MainAssembly;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.NeutralResourcesLanguageAttribute" /> class with the specified ultimate resource fallback location.</summary>
	/// <param name="cultureName">The name of the culture that the current assembly's neutral resources were written in.</param>
	/// <param name="location">One of the enumeration values that indicates the location from which to retrieve neutral fallback resources.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cultureName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="location" /> is not a member of <see cref="T:System.Resources.UltimateResourceFallbackLocation" />.</exception>
	public NeutralResourcesLanguageAttribute(string cultureName, UltimateResourceFallbackLocation location)
	{
		if (cultureName == null)
		{
			throw new ArgumentNullException("cultureName");
		}
		if (!Enum.IsDefined(typeof(UltimateResourceFallbackLocation), location))
		{
			throw new ArgumentException(Environment.GetResourceString("The NeutralResourcesLanguageAttribute specifies an invalid or unrecognized ultimate resource fallback location: \"{0}\".", location));
		}
		_culture = cultureName;
		_fallbackLoc = location;
	}
}
