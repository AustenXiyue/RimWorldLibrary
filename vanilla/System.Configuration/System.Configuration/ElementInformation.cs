using System.Collections;
using Unity;

namespace System.Configuration;

/// <summary>Contains meta-information about an individual element within the configuration. This class cannot be inherited.</summary>
public sealed class ElementInformation
{
	private readonly PropertyInformation propertyInfo;

	private readonly ConfigurationElement owner;

	private readonly PropertyInformationCollection properties;

	/// <summary>Gets the errors for the associated element and subelements</summary>
	/// <returns>The collection containing the errors for the associated element and subelements</returns>
	[System.MonoTODO]
	public ICollection Errors
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets a value indicating whether the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is a <see cref="T:System.Configuration.ConfigurationElementCollection" /> collection.</summary>
	/// <returns>true if the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is a <see cref="T:System.Configuration.ConfigurationElementCollection" /> collection; otherwise, false.</returns>
	public bool IsCollection => owner is ConfigurationElementCollection;

	/// <summary>Gets a value that indicates whether the associated <see cref="T:System.Configuration.ConfigurationElement" /> object cannot be modified.</summary>
	/// <returns>true if the associated <see cref="T:System.Configuration.ConfigurationElement" /> object cannot be modified; otherwise, false.</returns>
	public bool IsLocked
	{
		get
		{
			if (propertyInfo == null)
			{
				return false;
			}
			return propertyInfo.IsLocked;
		}
	}

	/// <summary>Gets a value indicating whether the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is in the configuration file.</summary>
	/// <returns>true if the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is in the configuration file; otherwise, false.</returns>
	[System.MonoTODO("Support multiple levels of inheritance")]
	public bool IsPresent => owner.IsElementPresent;

	/// <summary>Gets the line number in the configuration file where the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is defined.</summary>
	/// <returns>The line number in the configuration file where the associated <see cref="T:System.Configuration.ConfigurationElement" /> object is defined.</returns>
	public int LineNumber
	{
		get
		{
			if (propertyInfo == null)
			{
				return 0;
			}
			return propertyInfo.LineNumber;
		}
	}

	/// <summary>Gets the source file where the associated <see cref="T:System.Configuration.ConfigurationElement" /> object originated.</summary>
	/// <returns>The source file where the associated <see cref="T:System.Configuration.ConfigurationElement" /> object originated.</returns>
	public string Source
	{
		get
		{
			if (propertyInfo == null)
			{
				return null;
			}
			return propertyInfo.Source;
		}
	}

	/// <summary>Gets the type of the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	/// <returns>The type of the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</returns>
	public Type Type
	{
		get
		{
			if (propertyInfo == null)
			{
				return owner.GetType();
			}
			return propertyInfo.Type;
		}
	}

	/// <summary>Gets the object used to validate the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	/// <returns>The object used to validate the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</returns>
	public ConfigurationValidatorBase Validator
	{
		get
		{
			if (propertyInfo == null)
			{
				return new DefaultValidator();
			}
			return propertyInfo.Validator;
		}
	}

	/// <summary>Gets a <see cref="T:System.Configuration.PropertyInformationCollection" /> collection of the properties in the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.PropertyInformationCollection" /> collection of the properties in the associated <see cref="T:System.Configuration.ConfigurationElement" /> object.</returns>
	public PropertyInformationCollection Properties => properties;

	internal ElementInformation(ConfigurationElement owner, PropertyInformation propertyInfo)
	{
		this.propertyInfo = propertyInfo;
		this.owner = owner;
		properties = new PropertyInformationCollection();
		foreach (ConfigurationProperty property in owner.Properties)
		{
			properties.Add(new PropertyInformation(owner, property));
		}
	}

	internal void Reset(ElementInformation parentInfo)
	{
		foreach (PropertyInformation property in Properties)
		{
			PropertyInformation parentProperty = parentInfo.Properties[property.Name];
			property.Reset(parentProperty);
		}
	}

	internal ElementInformation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
