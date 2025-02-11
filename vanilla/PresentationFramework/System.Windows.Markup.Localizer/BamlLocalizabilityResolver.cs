namespace System.Windows.Markup.Localizer;

/// <summary>Resolves localizable settings for classes and properties in binary XAML (BAML). </summary>
public abstract class BamlLocalizabilityResolver
{
	/// <summary>Returns a value that indicates whether a specified type of element can be localized and, if so, whether it can be formatted inline. </summary>
	/// <returns>An object that contains the localizability information for the specified assembly and element.</returns>
	/// <param name="assembly">The full name of the assembly that contains BAML to be localized.</param>
	/// <param name="className">The full class name of the element that you want to retrieve localizability information for.</param>
	public abstract ElementLocalizability GetElementLocalizability(string assembly, string className);

	/// <summary>Returns a value that indicates whether a specified property of a specified type of element can be localized. </summary>
	/// <returns>An object that specifies whether and how the property can be localized.</returns>
	/// <param name="assembly">The full name of the assembly that contains BAML to be localized.</param>
	/// <param name="className">The full class name of the element that you want to retrieve localizability information for.</param>
	/// <param name="property">The name of the property that you want to retrieve localizability information for.</param>
	public abstract LocalizabilityAttribute GetPropertyLocalizability(string assembly, string className, string property);

	/// <summary>Returns the full class name of a XAML tag that has not been encountered in BAML.</summary>
	/// <returns>The full class name associated with the tag.</returns>
	/// <param name="formattingTag">The name of the tag.</param>
	public abstract string ResolveFormattingTagToClass(string formattingTag);

	/// <summary>Returns the full name of the assembly that contains the specified class.</summary>
	/// <returns>The full name of the assembly that contains the class.</returns>
	/// <param name="className">The full class name.</param>
	public abstract string ResolveAssemblyFromClass(string className);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizabilityResolver" /> class. </summary>
	protected BamlLocalizabilityResolver()
	{
	}
}
