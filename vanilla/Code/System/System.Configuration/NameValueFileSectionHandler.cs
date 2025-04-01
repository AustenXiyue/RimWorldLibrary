using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace System.Configuration;

/// <summary>Provides access to a configuration file. This type supports the .NET Framework configuration infrastructure and is not intended to be used directly from your code.</summary>
/// <filterpriority>2</filterpriority>
public class NameValueFileSectionHandler : IConfigurationSectionHandler
{
	/// <summary>Creates a new configuration handler and adds it to the section-handler collection based on the specified parameters.</summary>
	/// <returns>A configuration object.</returns>
	/// <param name="parent">The parent object.</param>
	/// <param name="configContext">The configuration context object.</param>
	/// <param name="section">The section XML node.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The file specified in the file attribute of <paramref name="section" /> exists but cannot be loaded.- or -The name attribute of <paramref name="section" /> does not match the root element of the file specified in the file attribute.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public object Create(object parent, object configContext, XmlNode section)
	{
		XmlNode xmlNode = null;
		if (section.Attributes != null)
		{
			xmlNode = section.Attributes.RemoveNamedItem("file");
		}
		NameValueCollection nameValueCollection = ConfigHelper.GetNameValueCollection(parent as NameValueCollection, section, "key", "value");
		if (xmlNode != null && xmlNode.Value != string.Empty)
		{
			string text = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(((System.Configuration.IConfigXmlNode)section).Filename)), xmlNode.Value);
			if (!File.Exists(text))
			{
				return nameValueCollection;
			}
			ConfigXmlDocument configXmlDocument = new ConfigXmlDocument();
			configXmlDocument.Load(text);
			if (configXmlDocument.DocumentElement.Name != section.Name)
			{
				throw new ConfigurationException("Invalid root element", configXmlDocument.DocumentElement);
			}
			nameValueCollection = ConfigHelper.GetNameValueCollection(nameValueCollection, configXmlDocument.DocumentElement, "key", "value");
		}
		return nameValueCollection;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.NameValueFileSectionHandler" /> class.</summary>
	public NameValueFileSectionHandler()
	{
	}
}
