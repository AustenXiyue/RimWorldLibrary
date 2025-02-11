using System.Reflection;

namespace System.Windows;

/// <summary>Specifies the location in which theme dictionaries are stored for an assembly. </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ThemeInfoAttribute : Attribute
{
	private ResourceDictionaryLocation _themeDictionaryLocation;

	private ResourceDictionaryLocation _genericDictionaryLocation;

	/// <summary>The location of theme specific resources. </summary>
	/// <returns>The <see cref="T:System.Windows.ResourceDictionaryLocation" /> of the theme specific <see cref="T:System.Windows.ResourceDictionary" />.</returns>
	public ResourceDictionaryLocation ThemeDictionaryLocation => _themeDictionaryLocation;

	/// <summary>The location of generic, not theme specific, resources. </summary>
	/// <returns>The <see cref="T:System.Windows.ResourceDictionaryLocation" /> of the generic <see cref="T:System.Windows.ResourceDictionary" />.</returns>
	public ResourceDictionaryLocation GenericDictionaryLocation => _genericDictionaryLocation;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ThemeInfoAttribute" /> class and creates an attribute that defines theme dictionary locations for types in an assembly.</summary>
	/// <param name="themeDictionaryLocation">The location of theme-specific resources.</param>
	/// <param name="genericDictionaryLocation">The location of generic, not theme-specific, resources.</param>
	public ThemeInfoAttribute(ResourceDictionaryLocation themeDictionaryLocation, ResourceDictionaryLocation genericDictionaryLocation)
	{
		_themeDictionaryLocation = themeDictionaryLocation;
		_genericDictionaryLocation = genericDictionaryLocation;
	}

	internal static ThemeInfoAttribute FromAssembly(Assembly assembly)
	{
		return Attribute.GetCustomAttribute(assembly, typeof(ThemeInfoAttribute)) as ThemeInfoAttribute;
	}
}
