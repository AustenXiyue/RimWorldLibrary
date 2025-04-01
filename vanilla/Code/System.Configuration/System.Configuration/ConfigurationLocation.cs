using System.IO;
using System.Xml;

namespace System.Configuration;

/// <summary>Represents a location element within a configuration file.</summary>
public class ConfigurationLocation
{
	private static readonly char[] pathTrimChars = new char[1] { '/' };

	private string path;

	private Configuration configuration;

	private Configuration parent;

	private string xmlContent;

	private bool parentResolved;

	private bool allowOverride;

	/// <summary>Gets the relative path to the resource whose configuration settings are represented by this <see cref="T:System.Configuration.ConfigurationLocation" /> object.</summary>
	/// <returns>The relative path to the resource whose configuration settings are represented by this <see cref="T:System.Configuration.ConfigurationLocation" />.</returns>
	public string Path => path;

	internal bool AllowOverride => allowOverride;

	internal string XmlContent => xmlContent;

	internal Configuration OpenedConfiguration => configuration;

	internal ConfigurationLocation()
	{
	}

	internal ConfigurationLocation(string path, string xmlContent, Configuration parent, bool allowOverride)
	{
		if (!string.IsNullOrEmpty(path))
		{
			switch (path[0])
			{
			case ' ':
			case '.':
			case '/':
			case '\\':
				throw new ConfigurationErrorsException("<location> path attribute must be a relative virtual path.  It cannot start with any of ' ' '.' '/' or '\\'.");
			}
			path = path.TrimEnd(pathTrimChars);
		}
		this.path = path;
		this.xmlContent = xmlContent;
		this.parent = parent;
		this.allowOverride = allowOverride;
	}

	/// <summary>Creates an instance of a Configuration object.</summary>
	/// <returns>A Configuration object.</returns>
	public Configuration OpenConfiguration()
	{
		if (configuration == null)
		{
			if (!parentResolved)
			{
				Configuration parentWithFile = parent.GetParentWithFile();
				if (parentWithFile != null)
				{
					string configPathFromLocationSubPath = parent.ConfigHost.GetConfigPathFromLocationSubPath(parent.LocationConfigPath, path);
					parent = parentWithFile.FindLocationConfiguration(configPathFromLocationSubPath, parent);
				}
			}
			configuration = new Configuration(parent, path);
			using (XmlTextReader reader = new ConfigXmlTextReader(new StringReader(xmlContent), path))
			{
				configuration.ReadData(reader, allowOverride);
			}
			xmlContent = null;
		}
		return configuration;
	}

	internal void SetParentConfiguration(Configuration parent)
	{
		if (!parentResolved)
		{
			parentResolved = true;
			this.parent = parent;
			if (configuration != null)
			{
				configuration.Parent = parent;
			}
		}
	}
}
