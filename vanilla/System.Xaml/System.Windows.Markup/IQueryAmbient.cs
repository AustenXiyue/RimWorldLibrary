namespace System.Windows.Markup;

/// <summary>Queries for whether a specified property should be treated as ambient in the current scope.</summary>
public interface IQueryAmbient
{
	/// <summary>Queries for whether a specified named property can be considered ambient in the current scope.</summary>
	/// <returns>true if the requested property can be considered ambient; otherwise, false.</returns>
	/// <param name="propertyName">The name of the property to check for ambience state.</param>
	bool IsAmbientPropertyAvailable(string propertyName);
}
