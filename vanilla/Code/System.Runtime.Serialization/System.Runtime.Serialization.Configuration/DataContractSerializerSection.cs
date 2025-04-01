using System.Configuration;
using System.Security;
using System.Security.Permissions;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
public sealed class DataContractSerializerSection : ConfigurationSection
{
	private ConfigurationPropertyCollection properties;

	/// <summary>Gets a collection of types added to the <see cref="P:System.Runtime.Serialization.DataContractSerializer.KnownTypes" /> property.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.DeclaredTypeElementCollection" /> that contains the known types.</returns>
	[ConfigurationProperty("declaredTypes", DefaultValue = null)]
	public DeclaredTypeElementCollection DeclaredTypes => (DeclaredTypeElementCollection)base["declaredTypes"];

	protected override ConfigurationPropertyCollection Properties
	{
		get
		{
			if (properties == null)
			{
				ConfigurationPropertyCollection configurationPropertyCollection = new ConfigurationPropertyCollection();
				configurationPropertyCollection.Add(new ConfigurationProperty("declaredTypes", typeof(DeclaredTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
				properties = configurationPropertyCollection;
			}
			return properties;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.DataContractSerializerSection" /> class. </summary>
	public DataContractSerializerSection()
	{
	}

	[SecurityCritical]
	[ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
	internal static DataContractSerializerSection UnsafeGetSection()
	{
		return ((DataContractSerializerSection)ConfigurationManager.GetSection(ConfigurationStrings.DataContractSerializerSectionPath)) ?? throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString("Failed to load configuration section for dataContractSerializer.")));
	}
}
