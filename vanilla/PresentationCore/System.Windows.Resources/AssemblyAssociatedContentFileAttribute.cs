namespace System.Windows.Resources;

/// <summary>This attribute is interpreted during the Extensible Application Markup Language (XAML) compilation process to associate loose content with a Windows Presentation Foundation (WPF) application.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AssemblyAssociatedContentFileAttribute : Attribute
{
	private string _path;

	/// <summary>Gets the path to the associated content.</summary>
	/// <returns>The path, as declared in the attribute.</returns>
	public string RelativeContentFilePath => _path;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Resources.AssemblyAssociatedContentFileAttribute" /> class.</summary>
	/// <param name="relativeContentFilePath">The path to the associated content.</param>
	public AssemblyAssociatedContentFileAttribute(string relativeContentFilePath)
	{
		_path = relativeContentFilePath;
	}
}
