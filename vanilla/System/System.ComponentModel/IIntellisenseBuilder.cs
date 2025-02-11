namespace System.ComponentModel;

/// <summary>Provides an interface to facilitate the retrieval of the builder's name and to display the builder.</summary>
public interface IIntellisenseBuilder
{
	/// <summary>Gets a localized name.</summary>
	/// <returns>A localized name.</returns>
	string Name { get; }

	/// <summary>Shows the builder.</summary>
	/// <returns>true if the value should be replaced with <paramref name="newValue" />; otherwise, false (if the user cancels, for example).</returns>
	/// <param name="language">The language service that is calling the builder.</param>
	/// <param name="value">The expression being edited.</param>
	/// <param name="newValue">The new value.</param>
	bool Show(string language, string value, ref string newValue);
}
