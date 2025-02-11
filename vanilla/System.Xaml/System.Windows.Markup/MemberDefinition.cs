namespace System.Windows.Markup;

/// <summary>Provides the base class that is used for a markup technique of defining members of a class in declarative XAML.</summary>
public abstract class MemberDefinition
{
	/// <summary>When implemented in a derived class, gets or sets the name of the member to define.</summary>
	/// <returns>The name of the member to define.</returns>
	public abstract string Name { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.MemberDefinition" /> class. </summary>
	protected MemberDefinition()
	{
	}
}
