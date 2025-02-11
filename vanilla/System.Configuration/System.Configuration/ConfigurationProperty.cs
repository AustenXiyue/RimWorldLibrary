using System.ComponentModel;
using Unity;

namespace System.Configuration;

/// <summary>Represents an attribute or a child of a configuration element. This class cannot be inherited.</summary>
public sealed class ConfigurationProperty
{
	internal static readonly object NoDefaultValue = new object();

	private string name;

	private Type type;

	private object default_value;

	private TypeConverter converter;

	private ConfigurationValidatorBase validation;

	private ConfigurationPropertyOptions flags;

	private string description;

	private ConfigurationCollectionAttribute collectionAttribute;

	/// <summary>Gets the <see cref="T:System.ComponentModel.TypeConverter" /> used to convert this <see cref="T:System.Configuration.ConfigurationProperty" /> into an XML representation for writing to the configuration file.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> used to convert this <see cref="T:System.Configuration.ConfigurationProperty" /> into an XML representation for writing to the configuration file.</returns>
	/// <exception cref="T:System.Exception">This <see cref="T:System.Configuration.ConfigurationProperty" /> cannot be converted. </exception>
	public TypeConverter Converter => converter;

	/// <summary>Gets the default value for this <see cref="T:System.Configuration.ConfigurationProperty" /> property.</summary>
	/// <returns>An <see cref="T:System.Object" /> that can be cast to the type specified by the <see cref="P:System.Configuration.ConfigurationProperty.Type" /> property.</returns>
	public object DefaultValue => default_value;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Configuration.ConfigurationProperty" /> is the key for the containing <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	/// <returns>true if this <see cref="T:System.Configuration.ConfigurationProperty" /> object is the key for the containing element; otherwise, false. The default is false.</returns>
	public bool IsKey => (flags & ConfigurationPropertyOptions.IsKey) != 0;

	/// <summary>Gets a value indicating whether this <see cref="T:System.Configuration.ConfigurationProperty" /> is required.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.ConfigurationProperty" /> is required; otherwise, false. The default is false.</returns>
	public bool IsRequired => (flags & ConfigurationPropertyOptions.IsRequired) != 0;

	/// <summary>Gets a value that indicates whether the property is the default collection of an element. </summary>
	/// <returns>true if the property is the default collection of an element; otherwise, false.</returns>
	public bool IsDefaultCollection => (flags & ConfigurationPropertyOptions.IsDefaultCollection) != 0;

	/// <summary>Gets the name of this <see cref="T:System.Configuration.ConfigurationProperty" />.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.ConfigurationProperty" />.</returns>
	public string Name => name;

	/// <summary>Gets the description associated with the <see cref="T:System.Configuration.ConfigurationProperty" />.</summary>
	/// <returns>A string value that describes the property.</returns>
	public string Description => description;

	/// <summary>Gets the type of this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</summary>
	/// <returns>A <see cref="T:System.Type" /> representing the type of this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</returns>
	public Type Type => type;

	/// <summary>Gets the <see cref="T:System.Configuration.ConfigurationValidatorAttribute" />, which is used to validate this <see cref="T:System.Configuration.ConfigurationProperty" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationValidatorBase" /> validator, which is used to validate this <see cref="T:System.Configuration.ConfigurationProperty" />.</returns>
	public ConfigurationValidatorBase Validator => validation;

	internal bool IsElement => typeof(ConfigurationElement).IsAssignableFrom(type);

	internal ConfigurationCollectionAttribute CollectionAttribute
	{
		get
		{
			return collectionAttribute;
		}
		set
		{
			collectionAttribute = value;
		}
	}

	/// <summary>Indicates whether the assembly name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
	/// <returns>true if the property requires assembly name transformation; otherwise, false.</returns>
	public bool IsAssemblyStringTransformationRequired
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Indicates whether the type name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
	/// <returns>true if the property requires type-name transformation; otherwise, false.</returns>
	public bool IsTypeStringTransformationRequired
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Indicates whether the configuration property's parent configuration section is queried at serialization time to determine whether the configuration property should be serialized into XML.</summary>
	/// <returns>true if the parent configuration section should be queried; otherwise, false.</returns>
	public bool IsVersionCheckRequired
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
	/// <param name="name">The name of the configuration entity. </param>
	/// <param name="type">The type of the configuration entity. </param>
	public ConfigurationProperty(string name, Type type)
		: this(name, type, NoDefaultValue, TypeDescriptor.GetConverter(type), new DefaultValidator(), ConfigurationPropertyOptions.None, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
	/// <param name="name">The name of the configuration entity. </param>
	/// <param name="type">The type of the configuration entity. </param>
	/// <param name="defaultValue">The default value of the configuration entity. </param>
	public ConfigurationProperty(string name, Type type, object defaultValue)
		: this(name, type, defaultValue, TypeDescriptor.GetConverter(type), new DefaultValidator(), ConfigurationPropertyOptions.None, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
	/// <param name="name">The name of the configuration entity. </param>
	/// <param name="type">The type of the configuration entity. </param>
	/// <param name="defaultValue">The default value of the configuration entity. </param>
	/// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values.</param>
	public ConfigurationProperty(string name, Type type, object defaultValue, ConfigurationPropertyOptions options)
		: this(name, type, defaultValue, TypeDescriptor.GetConverter(type), new DefaultValidator(), options, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
	/// <param name="name">The name of the configuration entity. </param>
	/// <param name="type">The type of the configuration entity.</param>
	/// <param name="defaultValue">The default value of the configuration entity. </param>
	/// <param name="typeConverter">The type of the converter to apply.</param>
	/// <param name="validator">The validator to use. </param>
	/// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values. </param>
	public ConfigurationProperty(string name, Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options)
		: this(name, type, defaultValue, typeConverter, validator, options, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationProperty" /> class. </summary>
	/// <param name="name">The name of the configuration entity. </param>
	/// <param name="type">The type of the configuration entity. </param>
	/// <param name="defaultValue">The default value of the configuration entity. </param>
	/// <param name="typeConverter">The type of the converter to apply.</param>
	/// <param name="validator">The validator to use. </param>
	/// <param name="options">One of the <see cref="T:System.Configuration.ConfigurationPropertyOptions" /> enumeration values. </param>
	/// <param name="description">The description of the configuration entity. </param>
	public ConfigurationProperty(string name, Type type, object defaultValue, TypeConverter typeConverter, ConfigurationValidatorBase validator, ConfigurationPropertyOptions options, string description)
	{
		this.name = name;
		converter = ((typeConverter != null) ? typeConverter : TypeDescriptor.GetConverter(type));
		if (defaultValue != null)
		{
			if (defaultValue == NoDefaultValue)
			{
				defaultValue = Type.GetTypeCode(type) switch
				{
					TypeCode.Object => null, 
					TypeCode.String => string.Empty, 
					_ => Activator.CreateInstance(type), 
				};
			}
			else if (!type.IsAssignableFrom(defaultValue.GetType()))
			{
				if (!converter.CanConvertFrom(defaultValue.GetType()))
				{
					throw new ConfigurationErrorsException($"The default value for property '{name}' has a different type than the one of the property itself: expected {type} but was {defaultValue.GetType()}");
				}
				defaultValue = converter.ConvertFrom(defaultValue);
			}
		}
		default_value = defaultValue;
		flags = options;
		this.type = type;
		validation = ((validator != null) ? validator : new DefaultValidator());
		this.description = description;
	}

	internal object ConvertFromString(string value)
	{
		if (converter != null)
		{
			return converter.ConvertFromInvariantString(value);
		}
		throw new NotImplementedException();
	}

	internal string ConvertToString(object value)
	{
		if (converter != null)
		{
			return converter.ConvertToInvariantString(value);
		}
		throw new NotImplementedException();
	}

	internal void Validate(object value)
	{
		if (validation != null)
		{
			validation.Validate(value);
		}
	}
}
