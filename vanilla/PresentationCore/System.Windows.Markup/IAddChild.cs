namespace System.Windows.Markup;

/// <summary>Provides a means to parse elements that permit mixtures of child elements or text.</summary>
public interface IAddChild
{
	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void AddChild(object value);

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void AddText(string text);
}
