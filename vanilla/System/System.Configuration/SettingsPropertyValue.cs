using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace System.Configuration;

/// <summary>Contains the value of a settings property that can be loaded and stored by an instance of <see cref="T:System.Configuration.SettingsBase" />.</summary>
/// <filterpriority>2</filterpriority>
public class SettingsPropertyValue
{
	private readonly SettingsProperty property;

	private object propertyValue;

	private object serializedValue;

	private bool needSerializedValue;

	private bool needPropertyValue;

	private bool dirty;

	private bool defaulted;

	private bool deserialized;

	/// <summary>Gets or sets whether the value of a <see cref="T:System.Configuration.SettingsProperty" /> object has been deserialized. </summary>
	/// <returns>true if the value of a <see cref="T:System.Configuration.SettingsProperty" /> object has been deserialized; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool Deserialized
	{
		get
		{
			return deserialized;
		}
		set
		{
			deserialized = value;
		}
	}

	/// <summary>Gets or sets whether the value of a <see cref="T:System.Configuration.SettingsProperty" /> object has changed. </summary>
	/// <returns>true if the value of a <see cref="T:System.Configuration.SettingsProperty" /> object has changed; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsDirty
	{
		get
		{
			return dirty;
		}
		set
		{
			dirty = value;
		}
	}

	/// <summary>Gets the name of the property from the associated <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>The name of the <see cref="T:System.Configuration.SettingsProperty" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name => property.Name;

	/// <summary>Gets the <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.SettingsProperty" /> object that describes the <see cref="T:System.Configuration.SettingsPropertyValue" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public SettingsProperty Property => property;

	/// <summary>Gets or sets the value of the <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>The value of the <see cref="T:System.Configuration.SettingsProperty" /> object. When this value is set, the <see cref="P:System.Configuration.SettingsPropertyValue.IsDirty" /> property is set to true and <see cref="P:System.Configuration.SettingsPropertyValue.UsingDefaultValue" /> is set to false.When a value is first accessed from the <see cref="P:System.Configuration.SettingsPropertyValue.PropertyValue" /> property, and if the value was initially stored into the <see cref="T:System.Configuration.SettingsPropertyValue" /> object as a serialized representation using the <see cref="P:System.Configuration.SettingsPropertyValue.SerializedValue" /> property, the <see cref="P:System.Configuration.SettingsPropertyValue.PropertyValue" /> property will trigger deserialization of the underlying value.  As a side effect, the <see cref="P:System.Configuration.SettingsPropertyValue.Deserialized" /> property will be set to true.If this chain of events occurs in ASP.NET, and if an error occurs during the deserialization process, the error is logged using the health-monitoring feature of ASP.NET. By default, this means that deserialization errors will show up in the Application Event Log when running under ASP.NET. If this process occurs outside of ASP.NET, and if an error occurs during deserialization, the error is suppressed, and the remainder of the logic during deserialization occurs. If there is no serialized value to deserialize when the deserialization is attempted, then <see cref="T:System.Configuration.SettingsPropertyValue" /> object will instead attempt to return a default value if one was configured as defined on the associated <see cref="T:System.Configuration.SettingsProperty" /> instance. In this case, if the <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property was set to either null, or to the string "[null]", then the <see cref="T:System.Configuration.SettingsPropertyValue" /> object will initialize the <see cref="P:System.Configuration.SettingsPropertyValue.PropertyValue" /> property to either null for reference types, or to the default value for the associated value type.  On the other hand, if <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property holds a valid object reference or string value (other than "[null]"), then the <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property is returned instead.If there is no serialized value to deserialize when the deserialization is attempted, and no default value was specified, then an empty string will be returned for string types. For all other types, a default instance will be returned by calling <see cref="M:System.Activator.CreateInstance(System.Type)" /> — for reference types this means an attempt will be made to create an object instance using the default constructor.  If this attempt fails, then null is returned.</returns>
	/// <exception cref="T:System.ArgumentException">While attempting to use the default value from the <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property, an error occurred.  Either the attempt to convert <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property to a valid type failed, or the resulting value was not compatible with the type defined by <see cref="P:System.Configuration.SettingsProperty.PropertyType" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object PropertyValue
	{
		get
		{
			if (needPropertyValue)
			{
				propertyValue = GetDeserializedValue(serializedValue);
				if (propertyValue == null)
				{
					propertyValue = GetDeserializedDefaultValue();
					defaulted = true;
				}
				needPropertyValue = false;
			}
			if (propertyValue != null && !(propertyValue is string) && !(propertyValue is DateTime) && !property.PropertyType.IsPrimitive)
			{
				dirty = true;
			}
			return propertyValue;
		}
		set
		{
			propertyValue = value;
			dirty = true;
			needPropertyValue = false;
			needSerializedValue = true;
			defaulted = false;
		}
	}

	/// <summary>Gets or sets the serialized value of the <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>The serialized value of a <see cref="T:System.Configuration.SettingsProperty" /> object.</returns>
	/// <exception cref="T:System.ArgumentException">The serialization options for the property indicated the use of a string type converter, but a type converter was not available.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	public object SerializedValue
	{
		get
		{
			if (needSerializedValue)
			{
				needSerializedValue = false;
				switch (property.SerializeAs)
				{
				case SettingsSerializeAs.String:
					serializedValue = TypeDescriptor.GetConverter(property.PropertyType).ConvertToInvariantString(propertyValue);
					break;
				case SettingsSerializeAs.Xml:
					if (propertyValue != null)
					{
						XmlSerializer xmlSerializer = new XmlSerializer(propertyValue.GetType());
						StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
						xmlSerializer.Serialize(stringWriter, propertyValue);
						serializedValue = stringWriter.ToString();
					}
					else
					{
						serializedValue = null;
					}
					break;
				case SettingsSerializeAs.Binary:
					if (propertyValue != null)
					{
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						MemoryStream memoryStream = new MemoryStream();
						binaryFormatter.Serialize(memoryStream, propertyValue);
						serializedValue = memoryStream.ToArray();
					}
					else
					{
						serializedValue = null;
					}
					break;
				default:
					serializedValue = null;
					break;
				}
			}
			return serializedValue;
		}
		set
		{
			serializedValue = value;
			needPropertyValue = true;
		}
	}

	/// <summary>Gets a Boolean value specifying whether the value of the <see cref="T:System.Configuration.SettingsPropertyValue" /> object is the default value as defined by the <see cref="P:System.Configuration.SettingsProperty.DefaultValue" /> property value on the associated <see cref="T:System.Configuration.SettingsProperty" /> object.</summary>
	/// <returns>true if the value of the <see cref="T:System.Configuration.SettingsProperty" /> object is the default value; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool UsingDefaultValue => defaulted;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsPropertyValue" /> class, based on supplied parameters.</summary>
	/// <param name="property">Specifies a <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	public SettingsPropertyValue(SettingsProperty property)
	{
		this.property = property;
		needPropertyValue = true;
	}

	internal object Reset()
	{
		propertyValue = GetDeserializedDefaultValue();
		dirty = true;
		defaulted = true;
		needPropertyValue = true;
		return propertyValue;
	}

	private object GetDeserializedDefaultValue()
	{
		if (property.DefaultValue == null)
		{
			if (property.PropertyType != null && property.PropertyType.IsValueType)
			{
				return Activator.CreateInstance(property.PropertyType);
			}
			return null;
		}
		if (property.DefaultValue is string && ((string)property.DefaultValue).Length == 0)
		{
			if (property.PropertyType != typeof(string))
			{
				return Activator.CreateInstance(property.PropertyType);
			}
			return string.Empty;
		}
		if (property.DefaultValue is string && ((string)property.DefaultValue).Length > 0)
		{
			return GetDeserializedValue(property.DefaultValue);
		}
		if (!property.PropertyType.IsAssignableFrom(property.DefaultValue.GetType()))
		{
			return TypeDescriptor.GetConverter(property.PropertyType).ConvertFrom(null, CultureInfo.InvariantCulture, property.DefaultValue);
		}
		return property.DefaultValue;
	}

	private object GetDeserializedValue(object serializedValue)
	{
		if (serializedValue == null)
		{
			return null;
		}
		object result = null;
		try
		{
			switch (property.SerializeAs)
			{
			case SettingsSerializeAs.String:
				if (serializedValue is string)
				{
					result = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString((string)serializedValue);
				}
				break;
			case SettingsSerializeAs.Xml:
			{
				XmlSerializer xmlSerializer = new XmlSerializer(property.PropertyType);
				StringReader input = new StringReader((string)serializedValue);
				result = xmlSerializer.Deserialize(XmlReader.Create(input));
				break;
			}
			case SettingsSerializeAs.Binary:
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				MemoryStream serializationStream = ((!(serializedValue is string)) ? new MemoryStream((byte[])serializedValue) : new MemoryStream(Convert.FromBase64String((string)serializedValue)));
				result = binaryFormatter.Deserialize(serializationStream);
				break;
			}
			}
		}
		catch (Exception ex)
		{
			if (property.ThrowOnErrorDeserializing)
			{
				throw ex;
			}
		}
		return result;
	}
}
