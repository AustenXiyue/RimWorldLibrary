namespace System.ComponentModel;

/// <summary>Specifies whether the Visual Studio Custom Action Installer or the Installutil.exe (Installer Tool) should be invoked when the assembly is installed.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class RunInstallerAttribute : Attribute
{
	private bool runInstaller;

	/// <summary>Specifies that the Visual Studio Custom Action Installer or the Installutil.exe (Installer Tool) should be invoked when the assembly is installed. This static field is read-only.</summary>
	public static readonly RunInstallerAttribute Yes = new RunInstallerAttribute(runInstaller: true);

	/// <summary>Specifies that the Visual Studio Custom Action Installer or the Installutil.exe (Installer Tool) should not be invoked when the assembly is installed. This static field is read-only.</summary>
	public static readonly RunInstallerAttribute No = new RunInstallerAttribute(runInstaller: false);

	/// <summary>Specifies the default visiblity, which is <see cref="F:System.ComponentModel.RunInstallerAttribute.No" />. This static field is read-only.</summary>
	public static readonly RunInstallerAttribute Default = No;

	/// <summary>Gets a value indicating whether an installer should be invoked during installation of an assembly.</summary>
	/// <returns>true if an installer should be invoked during installation of an assembly; otherwise, false.</returns>
	public bool RunInstaller => runInstaller;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.RunInstallerAttribute" /> class.</summary>
	/// <param name="runInstaller">true if an installer should be invoked during installation of an assembly; otherwise, false. </param>
	public RunInstallerAttribute(bool runInstaller)
	{
		this.runInstaller = runInstaller;
	}

	/// <summary>Determines whether the value of the specified <see cref="T:System.ComponentModel.RunInstallerAttribute" /> is equivalent to the current <see cref="T:System.ComponentModel.RunInstallerAttribute" />.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.RunInstallerAttribute" /> is equal to the current <see cref="T:System.ComponentModel.RunInstallerAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is RunInstallerAttribute runInstallerAttribute)
		{
			return runInstallerAttribute.RunInstaller == runInstaller;
		}
		return false;
	}

	/// <summary>Generates a hash code for the current <see cref="T:System.ComponentModel.RunInstallerAttribute" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.RunInstallerAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines if this attribute is the default.</summary>
	/// <returns>true if the attribute is the default value for this attribute class; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}
}
