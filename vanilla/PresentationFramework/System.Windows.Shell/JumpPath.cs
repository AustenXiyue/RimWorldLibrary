namespace System.Windows.Shell;

/// <summary>Represents a link to a file that is displayed in a Windows 7 taskbar Jump List.</summary>
public class JumpPath : JumpItem
{
	/// <summary>Gets or sets the path to the file to be included in the Jump List.</summary>
	/// <returns>The path to the file to be included in the Jump List.</returns>
	public string Path { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpPath" /> class.</summary>
	public JumpPath()
	{
	}
}
