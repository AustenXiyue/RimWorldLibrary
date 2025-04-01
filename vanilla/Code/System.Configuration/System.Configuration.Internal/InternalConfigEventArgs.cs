namespace System.Configuration.Internal;

/// <summary>Defines a class that allows the .NET Framework infrastructure to specify event arguments for configuration events.</summary>
public sealed class InternalConfigEventArgs : EventArgs
{
	private string configPath;

	/// <summary>Gets or sets the configuration path related to the <see cref="T:System.Configuration.Internal.InternalConfigEventArgs" /> object.</summary>
	/// <returns>A string value specifying the configuration path.</returns>
	public string ConfigPath
	{
		get
		{
			return configPath;
		}
		set
		{
			configPath = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Internal.InternalConfigEventArgs" /> class.</summary>
	/// <param name="configPath">A configuration path.</param>
	public InternalConfigEventArgs(string configPath)
	{
		this.configPath = configPath;
	}
}
