namespace System.Windows.Markup;

/// <summary>Abstract class that provides a means to store parser records for later instantiation. </summary>
public abstract class XamlInstanceCreator
{
	/// <summary>When overridden in a derived class, creates a new object to store parser records.</summary>
	/// <returns>The created object.</returns>
	public abstract object CreateObject();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.XamlInstanceCreator" /> class.</summary>
	protected XamlInstanceCreator()
	{
	}
}
