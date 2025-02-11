using System.Configuration;
using System.Security;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to add known types that are used for serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
public sealed class DeclaredTypeElement : ConfigurationElement
{
	private ConfigurationPropertyCollection properties;

	/// <summary>Gets the collection of known types.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.TypeElementCollection" /> that contains the known types.</returns>
	[ConfigurationProperty("", DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
	public TypeElementCollection KnownTypes => (TypeElementCollection)base[""];

	/// <summary>Gets or sets the name of the declared type that requires a collection of known types. </summary>
	/// <returns>The name of the declared type.</returns>
	[ConfigurationProperty("type", DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
	[DeclaredTypeValidator]
	public string Type
	{
		get
		{
			return (string)base["type"];
		}
		set
		{
			base["type"] = value;
		}
	}

	protected override ConfigurationPropertyCollection Properties
	{
		get
		{
			if (properties == null)
			{
				ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
				configurationPropertyCollection.Add(new ConfigurationProperty("", typeof(TypeElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
				configurationPropertyCollection.Add(new ConfigurationProperty("type", typeof(string), string.Empty, null, new DeclaredTypeValidator(), ConfigurationPropertyOptions.IsKey));
				properties = configurationPropertyCollection;
			}
			return properties;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> class.  </summary>
	public DeclaredTypeElement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElement" /> class with the specified type name.</summary>
	/// <param name="typeName">The name of the type that requires a collection of known types.</param>
	public DeclaredTypeElement(string typeName)
		: this()
	{
		if (string.IsNullOrEmpty(typeName))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
		}
		Type = typeName;
	}

	[SecuritySafeCritical]
	protected override void PostDeserialize()
	{
		if (base.EvaluationContext.IsMachineLevel || PartialTrustHelpers.IsInFullTrust())
		{
			return;
		}
		throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString("Failed to load configuration section for dataContractSerializer.")));
	}
}
