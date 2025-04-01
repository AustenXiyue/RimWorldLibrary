using System.Collections;
using System.Xml;

namespace System.Configuration;

/// <summary>Handles configuration sections that are represented by a single XML tag in the .config file.</summary>
/// <filterpriority>2</filterpriority>
public class SingleTagSectionHandler : IConfigurationSectionHandler
{
	/// <summary>Used internally to create a new instance of this object.</summary>
	/// <returns>The created object handler.</returns>
	/// <param name="parent">The parent of this object.</param>
	/// <param name="context">The context of this object.</param>
	/// <param name="section">The <see cref="T:System.Xml.XmlNode" /> object in the configuration.</param>
	/// <filterpriority>2</filterpriority>
	public virtual object Create(object parent, object context, XmlNode section)
	{
		Hashtable hashtable = ((parent != null) ? ((Hashtable)parent) : new Hashtable());
		if (section.HasChildNodes)
		{
			throw new ConfigurationException("Child Nodes not allowed.");
		}
		XmlAttributeCollection attributes = section.Attributes;
		for (int i = 0; i < attributes.Count; i++)
		{
			hashtable.Add(attributes[i].Name, attributes[i].Value);
		}
		return hashtable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SingleTagSectionHandler" /> class. </summary>
	public SingleTagSectionHandler()
	{
	}
}
