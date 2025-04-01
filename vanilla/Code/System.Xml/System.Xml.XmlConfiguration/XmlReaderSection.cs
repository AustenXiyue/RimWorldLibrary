using System.ComponentModel;
using System.Configuration;

namespace System.Xml.XmlConfiguration;

/// <summary>Represents an XML reader section.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class XmlReaderSection : ConfigurationSection
{
	/// <summary>Gets or sets the string that represents the prohibit default resolver.</summary>
	/// <returns>A <see cref="T:System.String" /> that represents the prohibit default resolver.</returns>
	[ConfigurationProperty("prohibitDefaultResolver", DefaultValue = "false")]
	public string ProhibitDefaultResolverString
	{
		get
		{
			return (string)base["prohibitDefaultResolver"];
		}
		set
		{
			base["prohibitDefaultResolver"] = value;
		}
	}

	private bool _ProhibitDefaultResolver
	{
		get
		{
			XmlConvert.TryToBoolean(ProhibitDefaultResolverString, out var result);
			return result;
		}
	}

	internal static bool ProhibitDefaultUrlResolver
	{
		get
		{
			if (!(ConfigurationManager.GetSection(XmlConfigurationString.XmlReaderSectionPath) is XmlReaderSection xmlReaderSection))
			{
				return false;
			}
			return xmlReaderSection._ProhibitDefaultResolver;
		}
	}

	[ConfigurationProperty("CollapseWhiteSpaceIntoEmptyString", DefaultValue = "false")]
	public string CollapseWhiteSpaceIntoEmptyStringString
	{
		get
		{
			return (string)base["CollapseWhiteSpaceIntoEmptyString"];
		}
		set
		{
			base["CollapseWhiteSpaceIntoEmptyString"] = value;
		}
	}

	private bool _CollapseWhiteSpaceIntoEmptyString
	{
		get
		{
			XmlConvert.TryToBoolean(CollapseWhiteSpaceIntoEmptyStringString, out var result);
			return result;
		}
	}

	internal static bool CollapseWhiteSpaceIntoEmptyString
	{
		get
		{
			if (!(ConfigurationManager.GetSection(XmlConfigurationString.XmlReaderSectionPath) is XmlReaderSection xmlReaderSection))
			{
				return false;
			}
			return xmlReaderSection._CollapseWhiteSpaceIntoEmptyString;
		}
	}

	internal static XmlResolver CreateDefaultResolver()
	{
		if (ProhibitDefaultUrlResolver)
		{
			return null;
		}
		return new XmlUrlResolver();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlConfiguration.XmlReaderSection" /> class.</summary>
	public XmlReaderSection()
	{
	}
}
