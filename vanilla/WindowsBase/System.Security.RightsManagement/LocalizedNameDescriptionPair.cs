namespace System.Security.RightsManagement;

/// <summary>Represents an immutable (read-only) pair of "Name" and "Description" strings.    </summary>
public class LocalizedNameDescriptionPair
{
	private string _name;

	private string _description;

	/// <summary>Gets the locale name.</summary>
	/// <returns>The locale name.</returns>
	public string Name => _name;

	/// <summary>Gets the locale description.</summary>
	/// <returns>The locale description.</returns>
	public string Description => _description;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.LocalizedNameDescriptionPair" /> class.</summary>
	/// <param name="name">The value for the locale <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> property.</param>
	/// <param name="description">The value for the locale <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">Either the <paramref name="name" /> parameter or <paramref name="description" /> parameter is null.</exception>
	public LocalizedNameDescriptionPair(string name, string description)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (description == null)
		{
			throw new ArgumentNullException("description");
		}
		_name = name;
		_description = description;
	}

	/// <summary>Indicates whether the <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> and <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> properties of a given object match those of this <see cref="T:System.Security.RightsManagement.LocalizedNameDescriptionPair" />.</summary>
	/// <returns>true if the <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> and <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> properties of the given object match those of this instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare the <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> and <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> properties of.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != GetType())
		{
			return false;
		}
		LocalizedNameDescriptionPair localizedNameDescriptionPair = obj as LocalizedNameDescriptionPair;
		if (string.CompareOrdinal(localizedNameDescriptionPair.Name, Name) == 0)
		{
			return string.CompareOrdinal(localizedNameDescriptionPair.Description, Description) == 0;
		}
		return false;
	}

	/// <summary>Gets a computed hash code based on the <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> and <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> properties.</summary>
	/// <returns>A computed hash code based on the <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Name" /> and <see cref="P:System.Security.RightsManagement.LocalizedNameDescriptionPair.Description" /> properties of this <see cref="T:System.Security.RightsManagement.LocalizedNameDescriptionPair" />.</returns>
	public override int GetHashCode()
	{
		return Name.GetHashCode() ^ Description.GetHashCode();
	}
}
