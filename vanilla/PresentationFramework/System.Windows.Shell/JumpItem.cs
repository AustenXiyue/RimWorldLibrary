namespace System.Windows.Shell;

/// <summary>Represents the base class for the <see cref="T:System.Windows.Shell.JumpPath" /> and <see cref="T:System.Windows.Shell.JumpTask" /> classes.</summary>
public abstract class JumpItem
{
	/// <summary>Gets or sets the name of the category the <see cref="T:System.Windows.Shell.JumpItem" /> is grouped with in the Windows 7 taskbar Jump List.</summary>
	/// <returns>The name of the category the <see cref="T:System.Windows.Shell.JumpItem" /> is grouped with. The default is null.</returns>
	public string CustomCategory { get; set; }

	internal JumpItem()
	{
	}
}
