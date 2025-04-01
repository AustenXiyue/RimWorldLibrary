using System.Reflection;
using System.Xml;

namespace System.Configuration;

/// <summary>Contains the XML representing the serialized value of the setting. This class cannot be inherited.</summary>
/// <filterpriority>2</filterpriority>
public sealed class SettingValueElement : ConfigurationElement
{
	private XmlNode node;

	private XmlNode original;

	[System.MonoTODO]
	protected override ConfigurationPropertyCollection Properties => base.Properties;

	/// <summary>Gets or sets the value of a <see cref="T:System.Configuration.SettingValueElement" /> object by using an <see cref="T:System.Xml.XmlNode" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNode" /> object containing the value of a <see cref="T:System.Configuration.SettingElement" />.</returns>
	/// <filterpriority>2</filterpriority>
	public XmlNode ValueXml
	{
		get
		{
			return node;
		}
		set
		{
			node = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingValueElement" /> class. </summary>
	[System.MonoTODO]
	public SettingValueElement()
	{
	}

	[System.MonoTODO]
	protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		original = new XmlDocument().ReadNode(reader);
		node = original.CloneNode(deep: true);
	}

	/// <summary>Compares the current <see cref="T:System.Configuration.SettingValueElement" /> instance to the specified object.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.SettingValueElement" /> instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="settingValue">The object to compare.</param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object settingValue)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a unique value representing the <see cref="T:System.Configuration.SettingValueElement" /> current instance.</summary>
	/// <returns>A unique value representing the <see cref="T:System.Configuration.SettingValueElement" /> current instance.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}

	protected override bool IsModified()
	{
		return original != node;
	}

	protected override void Reset(ConfigurationElement parentElement)
	{
		node = null;
	}

	protected override void ResetModified()
	{
		node = original;
	}

	protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
	{
		if (node == null)
		{
			return false;
		}
		node.WriteTo(writer);
		return true;
	}

	protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
	{
		if (parentElement != null && sourceElement.GetType() != parentElement.GetType())
		{
			throw new ConfigurationErrorsException("Can't unmerge two elements of different type");
		}
		bool flag = saveMode == ConfigurationSaveMode.Minimal || saveMode == ConfigurationSaveMode.Modified;
		foreach (PropertyInformation property in sourceElement.ElementInformation.Properties)
		{
			if (property.ValueOrigin == PropertyValueOrigin.Default)
			{
				continue;
			}
			PropertyInformation propertyInformation2 = base.ElementInformation.Properties[property.Name];
			object value = property.Value;
			if (parentElement == null || !HasValue(parentElement, property.Name))
			{
				propertyInformation2.Value = value;
			}
			else
			{
				if (value == null)
				{
					continue;
				}
				object item = GetItem(parentElement, property.Name);
				if (!PropertyIsElement(property))
				{
					if (!object.Equals(value, item) || saveMode == ConfigurationSaveMode.Full || (saveMode == ConfigurationSaveMode.Modified && property.ValueOrigin == PropertyValueOrigin.SetHere))
					{
						propertyInformation2.Value = value;
					}
					continue;
				}
				ConfigurationElement configurationElement = (ConfigurationElement)value;
				if (!flag || ElementIsModified(configurationElement))
				{
					if (item == null)
					{
						propertyInformation2.Value = value;
						continue;
					}
					ConfigurationElement parentElement2 = (ConfigurationElement)item;
					ConfigurationElement target = (ConfigurationElement)propertyInformation2.Value;
					ElementUnmerge(target, configurationElement, parentElement2, saveMode);
				}
			}
		}
	}

	private bool HasValue(ConfigurationElement element, string propName)
	{
		PropertyInformation propertyInformation = element.ElementInformation.Properties[propName];
		if (propertyInformation != null)
		{
			return propertyInformation.ValueOrigin != PropertyValueOrigin.Default;
		}
		return false;
	}

	private object GetItem(ConfigurationElement element, string property)
	{
		return (base.ElementInformation.Properties[property] ?? throw new InvalidOperationException("Property '" + property + "' not found in configuration element")).Value;
	}

	private bool PropertyIsElement(PropertyInformation prop)
	{
		return typeof(ConfigurationElement).IsAssignableFrom(prop.Type);
	}

	private bool ElementIsModified(ConfigurationElement element)
	{
		return (bool)element.GetType().GetMethod("IsModified", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(element, new object[0]);
	}

	private void ElementUnmerge(ConfigurationElement target, ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
	{
		target.GetType().GetMethod("Unmerge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, new object[3] { sourceElement, parentElement, saveMode });
	}
}
