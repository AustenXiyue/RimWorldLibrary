using System.ComponentModel;

namespace System.Configuration;

/// <summary>Provides the base class used to support user property settings.</summary>
/// <filterpriority>2</filterpriority>
public abstract class SettingsBase
{
	private bool sync;

	private SettingsContext context;

	private SettingsPropertyCollection properties;

	private SettingsProviderCollection providers;

	private SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

	/// <summary>Gets the associated settings context.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsContext" /> associated with the settings instance.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsContext Context => context;

	/// <summary>Gets a value indicating whether access to the object is synchronized (thread safe). </summary>
	/// <returns>true if access to the <see cref="T:System.Configuration.SettingsBase" /> is synchronized; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public bool IsSynchronized => sync;

	/// <summary>Gets or sets the value of the specified settings property.</summary>
	/// <returns>If found, the value of the named settings property.</returns>
	/// <param name="propertyName">A <see cref="T:System.String" /> containing the name of the property to access.</param>
	/// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">There are no properties associated with the current object, or the specified property could not be found.</exception>
	/// <exception cref="T:System.Configuration.SettingsPropertyIsReadOnlyException">An attempt was made to set a read-only property.</exception>
	/// <exception cref="T:System.Configuration.SettingsPropertyWrongTypeException">The value supplied is of a type incompatible with the settings property, during a set operation.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	public virtual object this[string propertyName]
	{
		get
		{
			if (sync)
			{
				lock (this)
				{
					return GetPropertyValue(propertyName);
				}
			}
			return GetPropertyValue(propertyName);
		}
		set
		{
			if (sync)
			{
				lock (this)
				{
					SetPropertyValue(propertyName, value);
					return;
				}
			}
			SetPropertyValue(propertyName, value);
		}
	}

	/// <summary>Gets the collection of settings properties.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyCollection" /> collection containing all the <see cref="T:System.Configuration.SettingsProperty" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsPropertyCollection Properties => properties;

	/// <summary>Gets a collection of settings property values.</summary>
	/// <returns>A collection of <see cref="T:System.Configuration.SettingsPropertyValue" /> objects representing the actual data values for the properties managed by the <see cref="T:System.Configuration.SettingsBase" /> instance.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsPropertyValueCollection PropertyValues
	{
		get
		{
			if (sync)
			{
				lock (this)
				{
					return values;
				}
			}
			return values;
		}
	}

	/// <summary>Gets a collection of settings providers.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsProviderCollection" /> containing <see cref="T:System.Configuration.SettingsProvider" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public virtual SettingsProviderCollection Providers => providers;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsBase" /> class. </summary>
	protected SettingsBase()
	{
	}

	/// <summary>Initializes internal properties used by <see cref="T:System.Configuration.SettingsBase" /> object.</summary>
	/// <param name="context">The settings context related to the settings properties.</param>
	/// <param name="properties">The settings properties that will be accessible from the <see cref="T:System.Configuration.SettingsBase" /> instance.</param>
	/// <param name="providers">The initialized providers that should be used when loading and saving property values.</param>
	/// <filterpriority>2</filterpriority>
	public void Initialize(SettingsContext context, SettingsPropertyCollection properties, SettingsProviderCollection providers)
	{
		this.context = context;
		this.properties = properties;
		this.providers = providers;
	}

	/// <summary>Stores the current values of the settings properties.</summary>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public virtual void Save()
	{
		if (sync)
		{
			lock (this)
			{
				SaveCore();
				return;
			}
		}
		SaveCore();
	}

	private void SaveCore()
	{
		foreach (SettingsProvider provider in Providers)
		{
			SettingsPropertyValueCollection settingsPropertyValueCollection = new SettingsPropertyValueCollection();
			foreach (SettingsPropertyValue propertyValue in PropertyValues)
			{
				if (propertyValue.Property.Provider == provider)
				{
					settingsPropertyValueCollection.Add(propertyValue);
				}
			}
			if (settingsPropertyValueCollection.Count > 0)
			{
				provider.SetPropertyValues(Context, settingsPropertyValueCollection);
			}
		}
	}

	/// <summary>Provides a <see cref="T:System.Configuration.SettingsBase" /> class that is synchronized (thread safe).</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsBase" /> class that is synchronized.</returns>
	/// <param name="settingsBase">The class used to support user property settings.</param>
	public static SettingsBase Synchronized(SettingsBase settingsBase)
	{
		settingsBase.sync = true;
		return settingsBase;
	}

	private object GetPropertyValue(string propertyName)
	{
		SettingsProperty settingsProperty = null;
		if (Properties == null || (settingsProperty = Properties[propertyName]) == null)
		{
			throw new SettingsPropertyNotFoundException($"The settings property '{propertyName}' was not found");
		}
		if (values[propertyName] == null)
		{
			foreach (SettingsPropertyValue propertyValue in settingsProperty.Provider.GetPropertyValues(Context, Properties))
			{
				values.Add(propertyValue);
			}
		}
		return PropertyValues[propertyName].PropertyValue;
	}

	private void SetPropertyValue(string propertyName, object value)
	{
		SettingsProperty settingsProperty = null;
		if (Properties == null || (settingsProperty = Properties[propertyName]) == null)
		{
			throw new SettingsPropertyNotFoundException($"The settings property '{propertyName}' was not found");
		}
		if (settingsProperty.IsReadOnly)
		{
			throw new SettingsPropertyIsReadOnlyException($"The settings property '{propertyName}' is read only");
		}
		if (settingsProperty.PropertyType != value.GetType())
		{
			throw new SettingsPropertyWrongTypeException($"The value supplied is of a type incompatible with the settings property '{propertyName}'");
		}
		PropertyValues[propertyName].PropertyValue = value;
	}
}
