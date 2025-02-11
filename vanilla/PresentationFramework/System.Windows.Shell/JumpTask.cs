namespace System.Windows.Shell;

/// <summary>Represents a shortcut to an application in the Windows 7 taskbar Jump List.</summary>
public class JumpTask : JumpItem
{
	/// <summary>Gets or sets the text displayed for the task in the Jump List.</summary>
	/// <returns>The text displayed for the task in the Jump List. The default is null.</returns>
	public string Title { get; set; }

	/// <summary>Gets or sets the text displayed in the tooltip for the task in the Jump List.</summary>
	/// <returns>The text displayed in the tooltip for the task. The default is null.</returns>
	public string Description { get; set; }

	/// <summary>Gets or sets the path to the application.</summary>
	/// <returns>The path to the application. The default is null.</returns>
	public string ApplicationPath { get; set; }

	/// <summary>Gets or sets the arguments passed to the application on startup.</summary>
	/// <returns>The arguments passed to the application on startup. The default is null.</returns>
	public string Arguments { get; set; }

	/// <summary>Gets or sets the working directory of the application on startup.</summary>
	/// <returns>The working directory of the application on startup. The default is null.</returns>
	public string WorkingDirectory { get; set; }

	/// <summary>Gets or sets the path to a resource that contains the icon to display in the Jump List.</summary>
	/// <returns>The path to a resource that contains the icon. The default is null.</returns>
	public string IconResourcePath { get; set; }

	/// <summary>Gets or sets the zero-based index of an icon embedded in a resource.</summary>
	/// <returns>The zero-based index of the icon, or -1 if no icon is used. The default is 0.</returns>
	public int IconResourceIndex { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpTask" /> class.</summary>
	public JumpTask()
	{
	}
}
