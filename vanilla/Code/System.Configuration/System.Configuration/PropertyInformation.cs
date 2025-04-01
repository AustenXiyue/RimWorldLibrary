using System.ComponentModel;
using Unity;

namespace System.Configuration;

/// <summary>Contains meta-information on an individual property within the configuration. This type cannot be inherited.</summary>
public sealed class PropertyInformation
{
	private bool isLocked;

	private bool isModified;

	private int lineNumber;

	private string source;

	private object val;

	private PropertyValueOrigin origin;

	private readonly ConfigurationElement owner;

	private readonly ConfigurationProperty property;

	/// <summary>Gets the <see cref="T:System.ComponentModel.TypeConverter" /> object related to the configuration attribute.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.TypeConverter" /> object.</returns>
	public TypeConverter Converter => property.Converter;

	/// <summary>Gets an object containing the default value related to a configuration attribute.</summary>
	/// <returns>An object containing the default value of the configuration attribute.</returns>
	public object DefaultValue => property.DefaultValue;

	/// <summary>Gets the description of the object that corresponds to a configuration attribute.</summary>
	/// <returns>The description of the configuration attribute.</returns>
	public string Description => property.Description;

	/// <summary>Gets a value specifying whether the configuration attribute is a key.</summary>
	/// <returns>true if the configuration attribute is a key; otherwise, false.</returns>
	public bool IsKey => property.IsKey;

	/// <summary>Gets a value specifying whether the configuration attribute is locked.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.PropertyInformation" /> object is locked; otherwise, false.</returns>
	[System.MonoTODO]
	public bool IsLocked
	{
		get
		{
			return isLocked;
		}
		internal set
		{
			isLocked = value;
		}
	}

	/// <summary>Gets a value specifying whether the configuration attribute has been modified.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.PropertyInformation" /> object has been modified; otherwise, false.</returns>
	public bool IsModified
	{
		get
		{
			return isModified;
		}
		internal set
		{
			isModified = value;
		}
	}

	/// <summary>Gets a value specifying whether the configuration attribute is required.</summary>
	/// <returns>true if the <see cref="T:System.Configuration.PropertyInformation" /> object is required; otherwise, false.</returns>
	public bool IsRequired => property.IsRequired;

	/// <summary>Gets the line number in the configuration file related to the configuration attribute.</summary>
	/// <returns>A line number of the configuration file.</returns>
	public int LineNumber
	{
		get
		{
			return lineNumber;
		}
		internal set
		{
			lineNumber = value;
		}
	}

	/// <summary>Gets the name of the object that corresponds to a configuration attribute.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.PropertyInformation" /> object.</returns>
	public string Name => property.Name;

	/// <summary>Gets the source file that corresponds to a configuration attribute.</summary>
	/// <returns>The source file of the <see cref="T:System.Configuration.PropertyInformation" /> object.</returns>
	public string Source
	{
		get
		{
			return source;
		}
		internal set
		{
			source = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Type" /> of the object that corresponds to a configuration attribute.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the <see cref="T:System.Configuration.PropertyInformation" /> object.</returns>
	public Type Type => property.Type;

	/// <summary>Gets a <see cref="T:System.Configuration.ConfigurationValidatorBase" /> object related to the configuration attribute.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationValidatorBase" /> object.</returns>
	public ConfigurationValidatorBase Validator => property.Validator;

	/// <summary>Gets or sets an object containing the value related to a configuration attribute.</summary>
	/// <returns>An object containing the value for the <see cref="T:System.Configuration.PropertyInformation" /> object.</returns>
	public object Value
	{
		get
		{
			if (origin == PropertyValueOrigin.Default)
			{
				if (!property.IsElement)
				{
					return DefaultValue;
				}
				ConfigurationElement configurationElement = (ConfigurationElement)Activator.CreateInstance(Type, nonPublic: true);
				configurationElement.InitFromProperty(this);
				if (owner != null && owner.IsReadOnly())
				{
					configurationElement.SetReadOnly();
				}
				val = configurationElement;
				origin = PropertyValueOrigin.Inherited;
			}
			return val;
		}
		set
		{
			val = value;
			isModified = true;
			origin = PropertyValueOrigin.SetHere;
		}
	}

	internal bool IsElement => property.IsElement;

	/// <summary>Gets a <see cref="T:System.Configuration.PropertyValueOrigin" /> object related to the configuration attribute. </summary>
	/// <returns>A <see cref="T:System.Configuration.PropertyValueOrigin" /> object.</returns>
	public PropertyValueOrigin ValueOrigin => origin;

	internal ConfigurationProperty Property => property;

	internal PropertyInformation(ConfigurationElement owner, ConfigurationProperty property)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		this.owner = owner;
		this.property = property;
	}

	internal void Reset(PropertyInformation parentProperty)
	{
		if (parentProperty != null)
		{
			if (property.IsElement)
			{
				((ConfigurationElement)Value).Reset((ConfigurationElement)parentProperty.Value);
				return;
			}
			val = parentProperty.Value;
			origin = PropertyValueOrigin.Inherited;
		}
		else
		{
			origin = PropertyValueOrigin.Default;
		}
	}

	internal string GetStringValue()
	{
		return property.ConvertToString(Value);
	}

	internal void SetStringValue(string value)
	{
		val = property.ConvertFromString(value);
		if (!object.Equals(val, DefaultValue))
		{
			origin = PropertyValueOrigin.SetHere;
		}
	}

	internal PropertyInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
