using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace System.Configuration;

/// <summary>Acts as a base class for deriving concrete wrapper classes to implement the application settings feature in Window Forms applications.</summary>
/// <filterpriority>1</filterpriority>
public abstract class ApplicationSettingsBase : SettingsBase, INotifyPropertyChanged
{
	private string settingsKey;

	private SettingsContext context;

	private SettingsPropertyCollection properties;

	private ISettingsProviderService providerService;

	private SettingsPropertyValueCollection propertyValues;

	private SettingsProviderCollection providers;

	/// <summary>Gets the application settings context associated with the settings group.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsContext" /> associated with the settings group.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public override SettingsContext Context
	{
		get
		{
			if (base.IsSynchronized)
			{
				Monitor.Enter(this);
			}
			try
			{
				if (context == null)
				{
					context = new SettingsContext();
					context["SettingsKey"] = "";
					Type type = GetType();
					context["GroupName"] = type.FullName;
					context["SettingsClassType"] = type;
				}
				return context;
			}
			finally
			{
				if (base.IsSynchronized)
				{
					Monitor.Exit(this);
				}
			}
		}
	}

	/// <summary>Gets or sets the value of the specified application settings property.</summary>
	/// <returns>If found, the value of the named settings property; otherwise, null.</returns>
	/// <param name="propertyName">A <see cref="T:System.String" /> containing the name of the property to access.</param>
	/// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">There are no properties associated with the current wrapper or the specified property could not be found.</exception>
	/// <exception cref="T:System.Configuration.SettingsPropertyIsReadOnlyException">An attempt was made to set a read-only property.</exception>
	/// <exception cref="T:System.Configuration.SettingsPropertyWrongTypeException">The value supplied is of a type incompatible with the settings property, during a set operation.</exception>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be parsed.</exception>
	/// <filterpriority>2</filterpriority>
	[System.MonoTODO]
	public override object this[string propertyName]
	{
		get
		{
			if (base.IsSynchronized)
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
			SettingsProperty settingsProperty = Properties[propertyName];
			if (settingsProperty == null)
			{
				throw new SettingsPropertyNotFoundException(propertyName);
			}
			if (settingsProperty.IsReadOnly)
			{
				throw new SettingsPropertyIsReadOnlyException(propertyName);
			}
			if (value != null && !settingsProperty.PropertyType.IsAssignableFrom(value.GetType()))
			{
				throw new SettingsPropertyWrongTypeException(propertyName);
			}
			if (PropertyValues[propertyName] == null)
			{
				CacheValuesByProvider(settingsProperty.Provider);
			}
			SettingChangingEventArgs settingChangingEventArgs = new SettingChangingEventArgs(propertyName, GetType().FullName, settingsKey, value, cancel: false);
			OnSettingChanging(this, settingChangingEventArgs);
			if (!settingChangingEventArgs.Cancel)
			{
				PropertyValues[propertyName].PropertyValue = value;
				OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	/// <summary>Gets the collection of settings properties in the wrapper.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyCollection" /> containing all the <see cref="T:System.Configuration.SettingsProperty" /> objects used in the current wrapper.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The associated settings provider could not be found or its instantiation failed. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Browsable(false)]
	public override SettingsPropertyCollection Properties
	{
		get
		{
			if (base.IsSynchronized)
			{
				Monitor.Enter(this);
			}
			try
			{
				if (properties == null)
				{
					SettingsProvider local_provider = null;
					properties = new SettingsPropertyCollection();
					Type type = GetType();
					SettingsProviderAttribute[] array = (SettingsProviderAttribute[])type.GetCustomAttributes(typeof(SettingsProviderAttribute), inherit: false);
					if (array != null && array.Length != 0)
					{
						SettingsProvider settingsProvider = (SettingsProvider)Activator.CreateInstance(Type.GetType(array[0].ProviderTypeName));
						settingsProvider.Initialize(null, null);
						if (settingsProvider != null && Providers[settingsProvider.Name] == null)
						{
							Providers.Add(settingsProvider);
							local_provider = settingsProvider;
						}
					}
					PropertyInfo[] array2 = type.GetProperties();
					foreach (PropertyInfo propertyInfo in array2)
					{
						SettingAttribute[] array3 = (SettingAttribute[])propertyInfo.GetCustomAttributes(typeof(SettingAttribute), inherit: false);
						if (array3 != null && array3.Length != 0)
						{
							CreateSettingsProperty(propertyInfo, properties, ref local_provider);
						}
					}
				}
				return properties;
			}
			finally
			{
				if (base.IsSynchronized)
				{
					Monitor.Exit(this);
				}
			}
		}
	}

	/// <summary>Gets a collection of property values.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> of property values.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public override SettingsPropertyValueCollection PropertyValues
	{
		get
		{
			if (base.IsSynchronized)
			{
				Monitor.Enter(this);
			}
			try
			{
				if (propertyValues == null)
				{
					propertyValues = new SettingsPropertyValueCollection();
				}
				return propertyValues;
			}
			finally
			{
				if (base.IsSynchronized)
				{
					Monitor.Exit(this);
				}
			}
		}
	}

	/// <summary>Gets the collection of application settings providers used by the wrapper.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsProviderCollection" /> containing all the <see cref="T:System.Configuration.SettingsProvider" /> objects used by the settings properties of the current settings wrapper.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[Browsable(false)]
	public override SettingsProviderCollection Providers
	{
		get
		{
			if (base.IsSynchronized)
			{
				Monitor.Enter(this);
			}
			try
			{
				if (providers == null)
				{
					providers = new SettingsProviderCollection();
				}
				return providers;
			}
			finally
			{
				if (base.IsSynchronized)
				{
					Monitor.Exit(this);
				}
			}
		}
	}

	/// <summary>Gets or sets the settings key for the application settings group.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the settings key for the current settings group.</returns>
	/// <filterpriority>2</filterpriority>
	[Browsable(false)]
	public string SettingsKey
	{
		get
		{
			return settingsKey;
		}
		set
		{
			settingsKey = value;
		}
	}

	/// <summary>Occurs after the value of an application settings property is changed.</summary>
	/// <filterpriority>2</filterpriority>
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Occurs before the value of an application settings property is changed.</summary>
	/// <filterpriority>2</filterpriority>
	public event SettingChangingEventHandler SettingChanging;

	/// <summary>Occurs after the application settings are retrieved from storage.</summary>
	public event SettingsLoadedEventHandler SettingsLoaded;

	/// <summary>Occurs before values are saved to the data store.</summary>
	/// <filterpriority>2</filterpriority>
	public event SettingsSavingEventHandler SettingsSaving;

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.ApplicationSettingsBase" /> class to its default state.</summary>
	protected ApplicationSettingsBase()
	{
		Initialize(Context, Properties, Providers);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.ApplicationSettingsBase" /> class using the supplied owner component.</summary>
	/// <param name="owner">The component that will act as the owner of the application settings object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="owner" /> is null.</exception>
	protected ApplicationSettingsBase(IComponent owner)
		: this(owner, string.Empty)
	{
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.ApplicationSettingsBase" /> class using the supplied settings key.</summary>
	/// <param name="settingsKey">A <see cref="T:System.String" /> that uniquely identifies separate instances of the wrapper class.</param>
	protected ApplicationSettingsBase(string settingsKey)
	{
		this.settingsKey = settingsKey;
		Initialize(Context, Properties, Providers);
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Configuration.ApplicationSettingsBase" /> class using the supplied owner component and settings key.</summary>
	/// <param name="owner">The component that will act as the owner of the application settings object.</param>
	/// <param name="settingsKey">A <see cref="T:System.String" /> that uniquely identifies separate instances of the wrapper class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="owner" /> is null.</exception>
	protected ApplicationSettingsBase(IComponent owner, string settingsKey)
	{
		if (owner == null)
		{
			throw new ArgumentNullException();
		}
		providerService = (ISettingsProviderService)owner.Site.GetService(typeof(ISettingsProviderService));
		this.settingsKey = settingsKey;
		Initialize(Context, Properties, Providers);
	}

	/// <summary>Returns the value of the named settings property for the previous version of the same application.</summary>
	/// <returns>An <see cref="T:System.Object" /> containing the value of the specified <see cref="T:System.Configuration.SettingsProperty" /> if found; otherwise, null.</returns>
	/// <param name="propertyName">A <see cref="T:System.String" /> containing the name of the settings property whose value is to be returned.</param>
	/// <exception cref="T:System.Configuration.SettingsPropertyNotFoundException">The property does not exist. The property count is zero or the property cannot be found in the data store.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	public object GetPreviousVersion(string propertyName)
	{
		throw new NotImplementedException();
	}

	/// <summary>Refreshes the application settings property values from persistent storage.</summary>
	/// <filterpriority>1</filterpriority>
	public void Reload()
	{
		foreach (SettingsProvider provider in Providers)
		{
			CacheValuesByProvider(provider);
		}
	}

	/// <summary>Restores the persisted application settings values to their corresponding default properties.</summary>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be parsed.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Reset()
	{
		Reload();
		foreach (SettingsPropertyValue propertyValue in PropertyValues)
		{
			propertyValue.PropertyValue = propertyValue.Reset();
		}
	}

	/// <summary>Stores the current values of the application settings properties.</summary>
	/// <filterpriority>1</filterpriority>
	public override void Save()
	{
		Context.CurrentSettings = this;
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
		Context.CurrentSettings = null;
	}

	/// <summary>Updates application settings to reflect a more recent installation of the application.</summary>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be parsed.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public virtual void Upgrade()
	{
	}

	/// <summary>Raises the <see cref="E:System.Configuration.ApplicationSettingsBase.PropertyChanged" /> event.</summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> that contains the event data.</param>
	protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(sender, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Configuration.ApplicationSettingsBase.SettingChanging" /> event.</summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="T:System.Configuration.SettingChangingEventArgs" /> that contains the event data.</param>
	protected virtual void OnSettingChanging(object sender, SettingChangingEventArgs e)
	{
		if (this.SettingChanging != null)
		{
			this.SettingChanging(sender, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Configuration.ApplicationSettingsBase.SettingsLoaded" /> event.</summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="T:System.Configuration.SettingsLoadedEventArgs" /> that contains the event data.</param>
	protected virtual void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
	{
		if (this.SettingsLoaded != null)
		{
			this.SettingsLoaded(sender, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Configuration.ApplicationSettingsBase.SettingsSaving" /> event.</summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
	protected virtual void OnSettingsSaving(object sender, CancelEventArgs e)
	{
		if (this.SettingsSaving != null)
		{
			this.SettingsSaving(sender, e);
		}
	}

	private void CacheValuesByProvider(SettingsProvider provider)
	{
		SettingsPropertyCollection settingsPropertyCollection = new SettingsPropertyCollection();
		foreach (SettingsProperty property in Properties)
		{
			if (property.Provider == provider)
			{
				settingsPropertyCollection.Add(property);
			}
		}
		if (settingsPropertyCollection.Count > 0)
		{
			foreach (SettingsPropertyValue propertyValue in provider.GetPropertyValues(Context, settingsPropertyCollection))
			{
				if (PropertyValues[propertyValue.Name] != null)
				{
					PropertyValues[propertyValue.Name].PropertyValue = propertyValue.PropertyValue;
				}
				else
				{
					PropertyValues.Add(propertyValue);
				}
			}
		}
		OnSettingsLoaded(this, new SettingsLoadedEventArgs(provider));
	}

	private void InitializeSettings(SettingsPropertyCollection settings)
	{
	}

	private object GetPropertyValue(string propertyName)
	{
		SettingsProperty settingsProperty = Properties[propertyName];
		if (settingsProperty == null)
		{
			throw new SettingsPropertyNotFoundException(propertyName);
		}
		if (propertyValues == null)
		{
			InitializeSettings(Properties);
		}
		if (PropertyValues[propertyName] == null)
		{
			CacheValuesByProvider(settingsProperty.Provider);
		}
		return PropertyValues[propertyName].PropertyValue;
	}

	private void CreateSettingsProperty(PropertyInfo prop, SettingsPropertyCollection properties, ref SettingsProvider local_provider)
	{
		SettingsAttributeDictionary settingsAttributeDictionary = new SettingsAttributeDictionary();
		SettingsProvider settingsProvider = null;
		object defaultValue = null;
		SettingsSerializeAs serializeAs = SettingsSerializeAs.String;
		bool flag = false;
		object[] customAttributes = prop.GetCustomAttributes(inherit: false);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			Attribute attribute = (Attribute)customAttributes[i];
			if (attribute is SettingsProviderAttribute)
			{
				settingsProvider = (SettingsProvider)Activator.CreateInstance(Type.GetType(((SettingsProviderAttribute)attribute).ProviderTypeName));
				settingsProvider.Initialize(null, null);
			}
			else if (attribute is DefaultSettingValueAttribute)
			{
				defaultValue = ((DefaultSettingValueAttribute)attribute).Value;
			}
			else if (attribute is SettingsSerializeAsAttribute)
			{
				serializeAs = ((SettingsSerializeAsAttribute)attribute).SerializeAs;
				flag = true;
			}
			else if (attribute is ApplicationScopedSettingAttribute || attribute is UserScopedSettingAttribute)
			{
				settingsAttributeDictionary.Add(attribute.GetType(), attribute);
			}
			else
			{
				settingsAttributeDictionary.Add(attribute.GetType(), attribute);
			}
		}
		if (!flag)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(prop.PropertyType);
			if (converter != null && (!converter.CanConvertFrom(typeof(string)) || !converter.CanConvertTo(typeof(string))))
			{
				serializeAs = SettingsSerializeAs.Xml;
			}
		}
		SettingsProperty settingsProperty = new SettingsProperty(prop.Name, prop.PropertyType, settingsProvider, isReadOnly: false, defaultValue, serializeAs, settingsAttributeDictionary, throwOnErrorDeserializing: false, throwOnErrorSerializing: false);
		if (providerService != null)
		{
			settingsProperty.Provider = providerService.GetSettingsProvider(settingsProperty);
		}
		if (settingsProvider == null)
		{
			if (local_provider == null)
			{
				local_provider = new LocalFileSettingsProvider();
				local_provider.Initialize(null, null);
			}
			settingsProperty.Provider = local_provider;
			settingsProvider = local_provider;
		}
		if (settingsProvider != null)
		{
			SettingsProvider settingsProvider2 = Providers[settingsProvider.Name];
			if (settingsProvider2 != null)
			{
				settingsProperty.Provider = settingsProvider2;
			}
		}
		properties.Add(settingsProperty);
		if (settingsProperty.Provider != null && Providers[settingsProperty.Provider.Name] == null)
		{
			Providers.Add(settingsProperty.Provider);
		}
	}
}
