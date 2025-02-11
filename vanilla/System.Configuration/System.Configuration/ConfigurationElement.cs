using System.Collections;
using System.Xml;
using Unity;

namespace System.Configuration;

/// <summary>Represents a configuration element within a configuration file.</summary>
public abstract class ConfigurationElement
{
	private class SaveContext
	{
		public readonly ConfigurationElement Element;

		public readonly ConfigurationElement Parent;

		public readonly ConfigurationSaveMode Mode;

		public SaveContext(ConfigurationElement element, ConfigurationElement parent, ConfigurationSaveMode mode)
		{
			Element = element;
			Parent = parent;
			Mode = mode;
		}

		public bool HasValues()
		{
			if (Mode == ConfigurationSaveMode.Full)
			{
				return true;
			}
			return Element.HasValues(Parent, Mode);
		}

		public bool HasValue(PropertyInformation prop)
		{
			if (Mode == ConfigurationSaveMode.Full)
			{
				return true;
			}
			return Element.HasValue(Parent, prop, Mode);
		}
	}

	private string rawXml;

	private bool modified;

	private ElementMap map;

	private ConfigurationPropertyCollection keyProps;

	private ConfigurationElementCollection defaultCollection;

	private bool readOnly;

	private ElementInformation elementInfo;

	private ConfigurationElementProperty elementProperty;

	private Configuration _configuration;

	private bool elementPresent;

	private ConfigurationLockCollection lockAllAttributesExcept;

	private ConfigurationLockCollection lockAllElementsExcept;

	private ConfigurationLockCollection lockAttributes;

	private ConfigurationLockCollection lockElements;

	private bool lockItem;

	private SaveContext saveContext;

	internal Configuration Configuration
	{
		get
		{
			return _configuration;
		}
		set
		{
			_configuration = value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Configuration.ElementInformation" /> object that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationElement" /> object. </summary>
	/// <returns>An <see cref="T:System.Configuration.ElementInformation" /> that contains the non-customizable information and functionality of the <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
	public ElementInformation ElementInformation
	{
		get
		{
			if (elementInfo == null)
			{
				elementInfo = new ElementInformation(this, null);
			}
			return elementInfo;
		}
	}

	internal string RawXml
	{
		get
		{
			return rawXml;
		}
		set
		{
			if (rawXml == null || value != null)
			{
				rawXml = value;
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Configuration.ConfigurationElementProperty" /> object that represents the <see cref="T:System.Configuration.ConfigurationElement" /> object itself.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationElementProperty" /> that represents the <see cref="T:System.Configuration.ConfigurationElement" /> itself.</returns>
	protected internal virtual ConfigurationElementProperty ElementProperty
	{
		get
		{
			if (elementProperty == null)
			{
				elementProperty = new ConfigurationElementProperty(ElementInformation.Validator);
			}
			return elementProperty;
		}
	}

	/// <summary>Gets the <see cref="T:System.Configuration.ContextInformation" /> object for the <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ContextInformation" /> for the <see cref="T:System.Configuration.ConfigurationElement" />.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The current element is not associated with a context.</exception>
	protected ContextInformation EvaluationContext
	{
		get
		{
			if (Configuration != null)
			{
				return Configuration.EvaluationContext;
			}
			throw new ConfigurationErrorsException("This element is not currently associated with any context.");
		}
	}

	/// <summary>Gets the collection of locked attributes.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked attributes (properties) for the element.</returns>
	public ConfigurationLockCollection LockAllAttributesExcept
	{
		get
		{
			if (lockAllAttributesExcept == null)
			{
				lockAllAttributesExcept = new ConfigurationLockCollection(this, ConfigurationLockType.Attribute | ConfigurationLockType.Exclude);
			}
			return lockAllAttributesExcept;
		}
	}

	/// <summary>Gets the collection of locked elements.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked elements.</returns>
	public ConfigurationLockCollection LockAllElementsExcept
	{
		get
		{
			if (lockAllElementsExcept == null)
			{
				lockAllElementsExcept = new ConfigurationLockCollection(this, ConfigurationLockType.Element | ConfigurationLockType.Exclude);
			}
			return lockAllElementsExcept;
		}
	}

	/// <summary>Gets the collection of locked attributes </summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked attributes (properties) for the element.</returns>
	public ConfigurationLockCollection LockAttributes
	{
		get
		{
			if (lockAttributes == null)
			{
				lockAttributes = new ConfigurationLockCollection(this, ConfigurationLockType.Attribute);
			}
			return lockAttributes;
		}
	}

	/// <summary>Gets the collection of locked elements.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationLockCollection" /> of locked elements.</returns>
	public ConfigurationLockCollection LockElements
	{
		get
		{
			if (lockElements == null)
			{
				lockElements = new ConfigurationLockCollection(this, ConfigurationLockType.Element);
			}
			return lockElements;
		}
	}

	/// <summary>Gets or sets a value indicating whether the element is locked.</summary>
	/// <returns>true if the element is locked; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element has already been locked at a higher configuration level.</exception>
	public bool LockItem
	{
		get
		{
			return lockItem;
		}
		set
		{
			lockItem = value;
		}
	}

	/// <summary>Gets or sets a property or attribute of this configuration element.</summary>
	/// <returns>The specified property, attribute, or child element.</returns>
	/// <param name="prop">The property to access. </param>
	/// <exception cref="T:System.Configuration.ConfigurationException">
	///   <paramref name="prop" /> is null or does not exist within the element.</exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="prop" /> is read only or locked.</exception>
	protected internal object this[ConfigurationProperty prop]
	{
		get
		{
			return this[prop.Name];
		}
		set
		{
			this[prop.Name] = value;
		}
	}

	/// <summary>Gets or sets a property, attribute, or child element of this configuration element.</summary>
	/// <returns>The specified property, attribute, or child element</returns>
	/// <param name="propertyName">The name of the <see cref="T:System.Configuration.ConfigurationProperty" /> to access.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">
	///   <paramref name="prop" /> is read-only or locked.</exception>
	protected internal object this[string propertyName]
	{
		get
		{
			return (ElementInformation.Properties[propertyName] ?? throw new InvalidOperationException("Property '" + propertyName + "' not found in configuration element")).Value;
		}
		set
		{
			PropertyInformation propertyInformation = ElementInformation.Properties[propertyName];
			if (propertyInformation == null)
			{
				throw new InvalidOperationException("Property '" + propertyName + "' not found in configuration element");
			}
			SetPropertyValue(propertyInformation.Property, value, ignoreLocks: false);
			propertyInformation.Value = value;
			modified = true;
		}
	}

	/// <summary>Gets the collection of properties.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> of properties for the element.</returns>
	protected internal virtual ConfigurationPropertyCollection Properties
	{
		get
		{
			if (map == null)
			{
				map = ElementMap.GetMap(GetType());
			}
			return map.Properties;
		}
	}

	internal bool IsElementPresent => elementPresent;

	/// <summary>Gets a reference to the top-level <see cref="T:System.Configuration.Configuration" /> instance that represents the configuration hierarchy that the current <see cref="T:System.Configuration.ConfigurationElement" /> instance belongs to.</summary>
	/// <returns>The top-level <see cref="T:System.Configuration.Configuration" /> instance that the current <see cref="T:System.Configuration.ConfigurationElement" /> instance belongs to.</returns>
	public Configuration CurrentConfiguration
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="P:System.Configuration.ConfigurationElement.CurrentConfiguration" /> property is null.</summary>
	/// <returns>false if the <see cref="P:System.Configuration.ConfigurationElement.CurrentConfiguration" /> property is null; otherwise, true.</returns>
	protected bool HasContext
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationElement" /> class. </summary>
	protected ConfigurationElement()
	{
	}

	internal virtual void InitFromProperty(PropertyInformation propertyInfo)
	{
		elementInfo = new ElementInformation(this, propertyInfo);
		Init();
	}

	/// <summary>Sets the <see cref="T:System.Configuration.ConfigurationElement" /> object to its initial state.</summary>
	protected internal virtual void Init()
	{
	}

	/// <summary>Adds the invalid-property errors in this <see cref="T:System.Configuration.ConfigurationElement" /> object, and in all subelements, to the passed list.</summary>
	/// <param name="errorList">An object that implements the <see cref="T:System.Collections.IList" /> interface.</param>
	[System.MonoTODO]
	protected virtual void ListErrors(IList errorList)
	{
		throw new NotImplementedException();
	}

	/// <summary>Sets a property to the specified value.</summary>
	/// <param name="prop">The element property to set. </param>
	/// <param name="value">The value to assign to the property.</param>
	/// <param name="ignoreLocks">true if the locks on the property should be ignored; otherwise, false.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">Occurs if the element is read-only or <paramref name="ignoreLocks" /> is true but the locks cannot be ignored.</exception>
	[System.MonoTODO]
	protected void SetPropertyValue(ConfigurationProperty prop, object value, bool ignoreLocks)
	{
		try
		{
			if (value != null)
			{
				prop.Validate(value);
			}
		}
		catch (Exception inner)
		{
			throw new ConfigurationErrorsException($"The value for the property '{prop.Name}' on type {ElementInformation.Type} is not valid.", inner);
		}
	}

	internal ConfigurationPropertyCollection GetKeyProperties()
	{
		if (keyProps != null)
		{
			return keyProps;
		}
		ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
		foreach (ConfigurationProperty property in Properties)
		{
			if (property.IsKey)
			{
				configurationPropertyCollection.Add(property);
			}
		}
		return keyProps = configurationPropertyCollection;
	}

	internal ConfigurationElementCollection GetDefaultCollection()
	{
		if (defaultCollection != null)
		{
			return defaultCollection;
		}
		ConfigurationProperty configurationProperty = null;
		foreach (ConfigurationProperty property in Properties)
		{
			if (property.IsDefaultCollection)
			{
				configurationProperty = property;
				break;
			}
		}
		if (configurationProperty != null)
		{
			defaultCollection = this[configurationProperty] as ConfigurationElementCollection;
		}
		return defaultCollection;
	}

	/// <summary>Compares the current <see cref="T:System.Configuration.ConfigurationElement" /> instance to the specified object.</summary>
	/// <returns>true if the object to compare with is equal to the current <see cref="T:System.Configuration.ConfigurationElement" /> instance; otherwise, false. The default is false. </returns>
	/// <param name="compareTo">The object to compare with.</param>
	public override bool Equals(object compareTo)
	{
		if (!(compareTo is ConfigurationElement configurationElement))
		{
			return false;
		}
		if (GetType() != configurationElement.GetType())
		{
			return false;
		}
		foreach (ConfigurationProperty property in Properties)
		{
			if (!object.Equals(this[property], configurationElement[property]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Gets a unique value representing the current <see cref="T:System.Configuration.ConfigurationElement" /> instance.</summary>
	/// <returns>A unique value representing the current <see cref="T:System.Configuration.ConfigurationElement" /> instance.</returns>
	public override int GetHashCode()
	{
		int num = 0;
		foreach (ConfigurationProperty property in Properties)
		{
			object obj = this[property];
			if (obj != null)
			{
				num += obj.GetHashCode();
			}
		}
		return num;
	}

	internal virtual bool HasLocalModifications()
	{
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (property.ValueOrigin == PropertyValueOrigin.SetHere && property.IsModified)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Reads XML from the configuration file.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> that reads from the configuration file.</param>
	/// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element to read is locked.- or -An attribute of the current node is not recognized.- or -The lock status of the current node cannot be determined.  </exception>
	protected internal virtual void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		Hashtable hashtable = new Hashtable();
		reader.MoveToContent();
		elementPresent = true;
		while (reader.MoveToNextAttribute())
		{
			PropertyInformation propertyInformation = ElementInformation.Properties[reader.LocalName];
			if (propertyInformation == null || (serializeCollectionKey && !propertyInformation.IsKey))
			{
				if (reader.LocalName == "lockAllAttributesExcept")
				{
					LockAllAttributesExcept.SetFromList(reader.Value);
				}
				else if (reader.LocalName == "lockAllElementsExcept")
				{
					LockAllElementsExcept.SetFromList(reader.Value);
				}
				else if (reader.LocalName == "lockAttributes")
				{
					LockAttributes.SetFromList(reader.Value);
				}
				else if (reader.LocalName == "lockElements")
				{
					LockElements.SetFromList(reader.Value);
				}
				else if (reader.LocalName == "lockItem")
				{
					LockItem = reader.Value.ToLowerInvariant() == "true";
				}
				else if (!(reader.LocalName == "xmlns") && (!(this is ConfigurationSection) || !(reader.LocalName == "configSource")) && !OnDeserializeUnrecognizedAttribute(reader.LocalName, reader.Value))
				{
					throw new ConfigurationErrorsException("Unrecognized attribute '" + reader.LocalName + "'.", reader);
				}
				continue;
			}
			if (hashtable.ContainsKey(propertyInformation))
			{
				throw new ConfigurationErrorsException("The attribute '" + propertyInformation.Name + "' may only appear once in this element.", reader);
			}
			string text = null;
			try
			{
				text = reader.Value;
				ValidateValue(propertyInformation.Property, text);
				propertyInformation.SetStringValue(text);
			}
			catch (ConfigurationErrorsException)
			{
				throw;
			}
			catch (ConfigurationException)
			{
				throw;
			}
			catch (Exception ex3)
			{
				throw new ConfigurationErrorsException($"The value for the property '{propertyInformation.Name}' is not valid. The error is: {ex3.Message}", reader);
			}
			hashtable[propertyInformation] = propertyInformation.Name;
			if (reader is ConfigXmlTextReader configXmlTextReader)
			{
				propertyInformation.Source = configXmlTextReader.Filename;
				propertyInformation.LineNumber = configXmlTextReader.LineNumber;
			}
		}
		reader.MoveToElement();
		if (reader.IsEmptyElement)
		{
			reader.Skip();
		}
		else
		{
			int depth = reader.Depth;
			reader.ReadStartElement();
			reader.MoveToContent();
			do
			{
				if (reader.NodeType != XmlNodeType.Element)
				{
					reader.Skip();
					continue;
				}
				PropertyInformation propertyInformation2 = ElementInformation.Properties[reader.LocalName];
				if (propertyInformation2 == null || (serializeCollectionKey && !propertyInformation2.IsKey))
				{
					if (OnDeserializeUnrecognizedElement(reader.LocalName, reader))
					{
						continue;
					}
					if (propertyInformation2 == null)
					{
						ConfigurationElementCollection configurationElementCollection = GetDefaultCollection();
						if (configurationElementCollection != null && configurationElementCollection.OnDeserializeUnrecognizedElement(reader.LocalName, reader))
						{
							continue;
						}
					}
					throw new ConfigurationErrorsException("Unrecognized element '" + reader.LocalName + "'.", reader);
				}
				if (!propertyInformation2.IsElement)
				{
					throw new ConfigurationErrorsException("Property '" + propertyInformation2.Name + "' is not a ConfigurationElement.");
				}
				if (hashtable.Contains(propertyInformation2))
				{
					throw new ConfigurationErrorsException("The element <" + propertyInformation2.Name + "> may only appear once in this section.", reader);
				}
				((ConfigurationElement)propertyInformation2.Value).DeserializeElement(reader, serializeCollectionKey);
				hashtable[propertyInformation2] = propertyInformation2.Name;
				if (depth == reader.Depth)
				{
					reader.Read();
				}
			}
			while (depth < reader.Depth);
		}
		modified = false;
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (!string.IsNullOrEmpty(property.Name) && property.IsRequired && !hashtable.ContainsKey(property) && ElementInformation.Properties[property.Name] == null)
			{
				object obj = OnRequiredPropertyNotFound(property.Name);
				if (!object.Equals(obj, property.DefaultValue))
				{
					property.Value = obj;
					property.IsModified = false;
				}
			}
		}
		PostDeserialize();
	}

	/// <summary>Gets a value indicating whether an unknown attribute is encountered during deserialization.</summary>
	/// <returns>true when an unknown attribute is encountered while deserializing; otherwise, false.</returns>
	/// <param name="name">The name of the unrecognized attribute.</param>
	/// <param name="value">The value of the unrecognized attribute.</param>
	protected virtual bool OnDeserializeUnrecognizedAttribute(string name, string value)
	{
		return false;
	}

	/// <summary>Gets a value indicating whether an unknown element is encountered during deserialization.</summary>
	/// <returns>true when an unknown element is encountered while deserializing; otherwise, false.</returns>
	/// <param name="elementName">The name of the unknown subelement.</param>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> being used for deserialization.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName" /> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName" /> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
	protected virtual bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
	{
		return false;
	}

	/// <summary>Throws an exception when a required property is not found.</summary>
	/// <returns>None.</returns>
	/// <param name="name">The name of the required attribute that was not found.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">In all cases.</exception>
	protected virtual object OnRequiredPropertyNotFound(string name)
	{
		throw new ConfigurationErrorsException("Required attribute '" + name + "' not found.");
	}

	/// <summary>Called before serialization.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that will be used to serialize the <see cref="T:System.Configuration.ConfigurationElement" />.</param>
	protected virtual void PreSerialize(XmlWriter writer)
	{
	}

	/// <summary>Called after deserialization.</summary>
	protected virtual void PostDeserialize()
	{
	}

	/// <summary>Used to initialize a default set of values for the <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	protected internal virtual void InitializeDefault()
	{
	}

	/// <summary>Indicates whether this configuration element has been modified since it was last saved or loaded, when implemented in a derived class.</summary>
	/// <returns>true if the element has been modified; otherwise, false. </returns>
	protected internal virtual bool IsModified()
	{
		if (modified)
		{
			return true;
		}
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (property.IsElement && property.Value is ConfigurationElement configurationElement && configurationElement.IsModified())
			{
				modified = true;
				break;
			}
		}
		return modified;
	}

	/// <summary>Sets the <see cref="M:System.Configuration.ConfigurationElement.IsReadOnly" /> property for the <see cref="T:System.Configuration.ConfigurationElement" /> object and all subelements.</summary>
	protected internal virtual void SetReadOnly()
	{
		readOnly = true;
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.ConfigurationElement" /> object is read-only; otherwise, false.</returns>
	public virtual bool IsReadOnly()
	{
		return readOnly;
	}

	/// <summary>Resets the internal state of the <see cref="T:System.Configuration.ConfigurationElement" /> object, including the locks and the properties collections.</summary>
	/// <param name="parentElement">The parent node of the configuration element.</param>
	protected internal virtual void Reset(ConfigurationElement parentElement)
	{
		elementPresent = false;
		if (parentElement != null)
		{
			ElementInformation.Reset(parentElement.ElementInformation);
		}
		else
		{
			InitializeDefault();
		}
	}

	/// <summary>Resets the value of the <see cref="M:System.Configuration.ConfigurationElement.IsModified" /> method to false when implemented in a derived class.</summary>
	protected internal virtual void ResetModified()
	{
		modified = false;
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			property.IsModified = false;
			if (property.Value is ConfigurationElement configurationElement)
			{
				configurationElement.ResetModified();
			}
		}
	}

	/// <summary>Writes the contents of this configuration element to the configuration file when implemented in a derived class.</summary>
	/// <returns>true if any data was actually serialized; otherwise, false.</returns>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that writes to the configuration file. </param>
	/// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The current attribute is locked at a higher configuration level.</exception>
	protected internal virtual bool SerializeElement(XmlWriter writer, bool serializeCollectionKey)
	{
		PreSerialize(writer);
		if (serializeCollectionKey)
		{
			ConfigurationPropertyCollection keyProperties = GetKeyProperties();
			foreach (ConfigurationProperty item in keyProperties)
			{
				writer.WriteAttributeString(item.Name, item.ConvertToString(this[item.Name]));
			}
			return keyProperties.Count > 0;
		}
		bool flag = false;
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (!property.IsElement)
			{
				if (saveContext == null)
				{
					throw new InvalidOperationException();
				}
				if (saveContext.HasValue(property))
				{
					writer.WriteAttributeString(property.Name, property.GetStringValue());
					flag = true;
				}
			}
		}
		foreach (PropertyInformation property2 in ElementInformation.Properties)
		{
			if (property2.IsElement)
			{
				ConfigurationElement configurationElement = (ConfigurationElement)property2.Value;
				if (configurationElement != null)
				{
					flag = configurationElement.SerializeToXmlElement(writer, property2.Name) || flag;
				}
			}
		}
		return flag;
	}

	/// <summary>Writes the outer tags of this configuration element to the configuration file when implemented in a derived class.</summary>
	/// <returns>true if writing was successful; otherwise, false.</returns>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> that writes to the configuration file. </param>
	/// <param name="elementName">The name of the <see cref="T:System.Configuration.ConfigurationElement" /> to be written. </param>
	/// <exception cref="T:System.Exception">The element has multiple child elements. </exception>
	protected internal virtual bool SerializeToXmlElement(XmlWriter writer, string elementName)
	{
		if (saveContext == null)
		{
			throw new InvalidOperationException();
		}
		if (!saveContext.HasValues())
		{
			return false;
		}
		if (elementName != null && elementName != "")
		{
			writer.WriteStartElement(elementName);
		}
		bool result = SerializeElement(writer, serializeCollectionKey: false);
		if (elementName != null && elementName != "")
		{
			writer.WriteEndElement();
		}
		return result;
	}

	/// <summary>Modifies the <see cref="T:System.Configuration.ConfigurationElement" /> object to remove all values that should not be saved. </summary>
	/// <param name="sourceElement">A <see cref="T:System.Configuration.ConfigurationElement" /> at the current level containing a merged view of the properties.</param>
	/// <param name="parentElement">The parent <see cref="T:System.Configuration.ConfigurationElement" />, or null if this is the top level.</param>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> that determines which property values to include.</param>
	protected internal virtual void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
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
			PropertyInformation propertyInformation2 = ElementInformation.Properties[property.Name];
			object value = property.Value;
			if (parentElement == null || !parentElement.HasValue(property.Name))
			{
				propertyInformation2.Value = value;
			}
			else
			{
				if (value == null)
				{
					continue;
				}
				object obj = parentElement[property.Name];
				if (!property.IsElement)
				{
					if (!object.Equals(value, obj) || saveMode == ConfigurationSaveMode.Full || (saveMode == ConfigurationSaveMode.Modified && property.ValueOrigin == PropertyValueOrigin.SetHere))
					{
						propertyInformation2.Value = value;
					}
					continue;
				}
				ConfigurationElement configurationElement = (ConfigurationElement)value;
				if (!flag || configurationElement.IsModified())
				{
					if (obj == null)
					{
						propertyInformation2.Value = value;
						continue;
					}
					ConfigurationElement parentElement2 = (ConfigurationElement)obj;
					((ConfigurationElement)propertyInformation2.Value).Unmerge(configurationElement, parentElement2, saveMode);
				}
			}
		}
	}

	internal bool HasValue(string propName)
	{
		PropertyInformation propertyInformation = ElementInformation.Properties[propName];
		if (propertyInformation != null)
		{
			return propertyInformation.ValueOrigin != PropertyValueOrigin.Default;
		}
		return false;
	}

	internal bool IsReadFromConfig(string propName)
	{
		PropertyInformation propertyInformation = ElementInformation.Properties[propName];
		if (propertyInformation != null)
		{
			return propertyInformation.ValueOrigin == PropertyValueOrigin.SetHere;
		}
		return false;
	}

	private void ValidateValue(ConfigurationProperty p, string value)
	{
		ConfigurationValidatorBase validator;
		if (p != null && (validator = p.Validator) != null)
		{
			if (!validator.CanValidate(p.Type))
			{
				throw new ConfigurationErrorsException($"Validator does not support type {p.Type}");
			}
			validator.Validate(p.ConvertFromString(value));
		}
	}

	internal bool HasValue(ConfigurationElement parent, PropertyInformation prop, ConfigurationSaveMode mode)
	{
		if (prop.ValueOrigin == PropertyValueOrigin.Default)
		{
			return false;
		}
		if (mode == ConfigurationSaveMode.Modified && prop.ValueOrigin == PropertyValueOrigin.SetHere && prop.IsModified)
		{
			return true;
		}
		object obj = ((parent != null && parent.HasValue(prop.Name)) ? parent[prop.Name] : prop.DefaultValue);
		if (!prop.IsElement)
		{
			return !object.Equals(prop.Value, obj);
		}
		ConfigurationElement obj2 = (ConfigurationElement)prop.Value;
		ConfigurationElement parent2 = (ConfigurationElement)obj;
		return obj2.HasValues(parent2, mode);
	}

	internal virtual bool HasValues(ConfigurationElement parent, ConfigurationSaveMode mode)
	{
		if (mode == ConfigurationSaveMode.Full)
		{
			return true;
		}
		if (modified && mode == ConfigurationSaveMode.Modified)
		{
			return true;
		}
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (HasValue(parent, property, mode))
			{
				return true;
			}
		}
		return false;
	}

	internal virtual void PrepareSave(ConfigurationElement parent, ConfigurationSaveMode mode)
	{
		saveContext = new SaveContext(this, parent, mode);
		foreach (PropertyInformation property in ElementInformation.Properties)
		{
			if (property.IsElement)
			{
				ConfigurationElement configurationElement = (ConfigurationElement)property.Value;
				if (parent == null || !parent.HasValue(property.Name))
				{
					configurationElement.PrepareSave(null, mode);
					continue;
				}
				ConfigurationElement parent2 = (ConfigurationElement)parent[property.Name];
				configurationElement.PrepareSave(parent2, mode);
			}
		}
	}

	/// <summary>Returns the transformed version of the specified assembly name. </summary>
	/// <returns>The transformed version of the assembly name. If no transformer is available, the <paramref name="assemblyName" /> parameter value is returned unchanged. The <see cref="P:System.Configuration.Configuration.TypeStringTransformer" /> property is null if no transformer is available.</returns>
	/// <param name="assemblyName">The name of the assembly.</param>
	protected virtual string GetTransformedAssemblyString(string assemblyName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Returns the transformed version of the specified type name.</summary>
	/// <returns>The transformed version of the specified type name. If no transformer is available, the <paramref name="typeName" /> parameter value is returned unchanged. The <see cref="P:System.Configuration.Configuration.TypeStringTransformer" /> property is null if no transformer is available.</returns>
	/// <param name="typeName">The name of the type.</param>
	protected virtual string GetTransformedTypeString(string typeName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
