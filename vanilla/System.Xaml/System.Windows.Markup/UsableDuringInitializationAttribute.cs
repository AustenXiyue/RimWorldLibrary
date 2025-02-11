namespace System.Windows.Markup;

/// <summary>Indicates whether this type is built top-down during XAML object graph creation.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UsableDuringInitializationAttribute : Attribute
{
	private bool _usable;

	/// <summary>Gets a value that indicates whether the associated class is usable during initialization.</summary>
	/// <returns>true if the associated class is usable during initialization; otherwise, false.</returns>
	public bool Usable => _usable;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.UsableDuringInitializationAttribute" /> class.</summary>
	/// <param name="usable">Defines whether the associated class is usable during initialization.</param>
	public UsableDuringInitializationAttribute(bool usable)
	{
		_usable = usable;
	}
}
