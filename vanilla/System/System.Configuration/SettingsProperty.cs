namespace System.Configuration;

/// <summary>Used internally as the class that represents metadata about an individual configuration property.</summary>
/// <filterpriority>2</filterpriority>
public class SettingsProperty
{
	private string name;

	private Type propertyType;

	private SettingsProvider provider;

	private bool isReadOnly;

	private object defaultValue;

	private SettingsSerializeAs serializeAs;

	private SettingsAttributeDictionary attributes;

	private bool throwOnErrorDeserializing;

	private bool throwOnErrorSerializing;

	/// <summary>Gets a <see cref="T:System.Configuration.SettingsAttributeDictionary" /> object containing the attributes of the <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsAttributeDictionary" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsAttributeDictionary Attributes => attributes;

	/// <summary>Gets or sets the default value of the <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>An object containing the default value of the <see cref="T:System.Configuration.SettingsProperty" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual object DefaultValue
	{
		get
		{
			return defaultValue;
		}
		set
		{
			defaultValue = value;
		}
	}

	/// <summary>Gets or sets a value specifying whether a <see cref="T:System.Configuration.SettingsProperty" /> object is read-only. </summary>
	/// <returns>true if the <see cref="T:System.Configuration.SettingsProperty" /> is read-only; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual bool IsReadOnly
	{
		get
		{
			return isReadOnly;
		}
		set
		{
			isReadOnly = value;
		}
	}

	/// <summary>Gets or sets the name of the <see cref="T:System.Configuration.SettingsProperty" />.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.SettingsProperty" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets or sets the type for the <see cref="T:System.Configuration.SettingsProperty" />.</summary>
	/// <returns>The type for the <see cref="T:System.Configuration.SettingsProperty" />.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual Type PropertyType
	{
		get
		{
			return propertyType;
		}
		set
		{
			propertyType = value;
		}
	}

	/// <summary>Gets or sets the provider for the <see cref="T:System.Configuration.SettingsProperty" />.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsProvider" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsProvider Provider
	{
		get
		{
			return provider;
		}
		set
		{
			provider = value;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Configuration.SettingsSerializeAs" /> object for the <see cref="T:System.Configuration.SettingsProperty" />.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsSerializeAs" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsSerializeAs SerializeAs
	{
		get
		{
			return serializeAs;
		}
		set
		{
			serializeAs = value;
		}
	}

	/// <summary>Gets or sets a value specifying whether an error will be thrown when the property is unsuccessfully deserialized.</summary>
	/// <returns>true if the error will be thrown when the property is unsuccessfully deserialized; otherwise, false.</returns>
	public bool ThrowOnErrorDeserializing
	{
		get
		{
			return throwOnErrorDeserializing;
		}
		set
		{
			throwOnErrorDeserializing = value;
		}
	}

	/// <summary>Gets or sets a value specifying whether an error will be thrown when the property is unsuccessfully serialized.</summary>
	/// <returns>true if the error will be thrown when the property is unsuccessfully serialized; otherwise, false.</returns>
	public bool ThrowOnErrorSerializing
	{
		get
		{
			return throwOnErrorSerializing;
		}
		set
		{
			throwOnErrorSerializing = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsProperty" /> class, based on the supplied parameter.</summary>
	/// <param name="propertyToCopy">Specifies a copy of an existing <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	public SettingsProperty(SettingsProperty propertyToCopy)
		: this(propertyToCopy.Name, propertyToCopy.PropertyType, propertyToCopy.Provider, propertyToCopy.IsReadOnly, propertyToCopy.DefaultValue, propertyToCopy.SerializeAs, new SettingsAttributeDictionary(propertyToCopy.Attributes), propertyToCopy.ThrowOnErrorDeserializing, propertyToCopy.ThrowOnErrorSerializing)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsProperty" /> class. based on the supplied parameter.</summary>
	/// <param name="name">Specifies the name of an existing <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	public SettingsProperty(string name)
		: this(name, null, null, isReadOnly: false, null, SettingsSerializeAs.String, new SettingsAttributeDictionary(), throwOnErrorDeserializing: false, throwOnErrorSerializing: false)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Configuration.SettingsProperty" /> class based on the supplied parameters.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <param name="propertyType">The type of <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <param name="provider">A <see cref="T:System.Configuration.SettingsProvider" /> object to use for persistence.</param>
	/// <param name="isReadOnly">A <see cref="T:System.Boolean" /> value specifying whether the <see cref="T:System.Configuration.SettingsProperty" /> object is read-only.</param>
	/// <param name="defaultValue">The default value of the <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <param name="serializeAs">A <see cref="T:System.Configuration.SettingsSerializeAs" /> object. This object is an enumeration used to set the serialization scheme for storing application settings.</param>
	/// <param name="attributes">A <see cref="T:System.Configuration.SettingsAttributeDictionary" /> object.</param>
	/// <param name="throwOnErrorDeserializing">A Boolean value specifying whether an error will be thrown when the property is unsuccessfully deserialized.</param>
	/// <param name="throwOnErrorSerializing">A Boolean value specifying whether an error will be thrown when the property is unsuccessfully serialized.</param>
	public SettingsProperty(string name, Type propertyType, SettingsProvider provider, bool isReadOnly, object defaultValue, SettingsSerializeAs serializeAs, SettingsAttributeDictionary attributes, bool throwOnErrorDeserializing, bool throwOnErrorSerializing)
	{
		this.name = name;
		this.propertyType = propertyType;
		this.provider = provider;
		this.isReadOnly = isReadOnly;
		this.defaultValue = defaultValue;
		this.serializeAs = serializeAs;
		this.attributes = attributes;
		this.throwOnErrorDeserializing = throwOnErrorDeserializing;
		this.throwOnErrorSerializing = throwOnErrorSerializing;
	}
}
