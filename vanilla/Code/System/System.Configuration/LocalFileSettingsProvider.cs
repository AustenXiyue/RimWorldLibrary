using System.Collections.Specialized;
using System.Security.Permissions;

namespace System.Configuration;

/// <summary>Provides persistence for application settings classes.</summary>
/// <filterpriority>2</filterpriority>
public class LocalFileSettingsProvider : SettingsProvider, IApplicationSettingsProvider
{
	private CustomizableFileSettingsProvider impl;

	/// <summary>Gets or sets the name of the currently running application.</summary>
	/// <returns>A string that contains the application's display name.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ApplicationName
	{
		get
		{
			return impl.ApplicationName;
		}
		set
		{
			impl.ApplicationName = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.LocalFileSettingsProvider" /> class.</summary>
	public LocalFileSettingsProvider()
	{
		impl = new CustomizableFileSettingsProvider();
	}

	/// <summary>Returns the value of the named settings property for the previous version of the same application. </summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyValue" /> representing the application setting if found; otherwise, null.</returns>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> that describes where the application settings property is used.</param>
	/// <param name="property">The <see cref="T:System.Configuration.SettingsProperty" /> whose value is to be returned.</param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	[System.MonoTODO]
	[FileIOPermission(SecurityAction.Assert, AllFiles = (FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery))]
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
	{
		return impl.GetPreviousVersion(context, property);
	}

	/// <summary>Returns the collection of setting property values for the specified application instance and settings property group.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> containing the values for the specified settings property group.</returns>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application usage.</param>
	/// <param name="properties">A <see cref="T:System.Configuration.SettingsPropertyCollection" /> containing the settings property group whose values are to be retrieved.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A user-scoped setting was encountered but the current configuration only supports application-scoped settings.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
	{
		return impl.GetPropertyValues(context, properties);
	}

	/// <param name="name">The name of the provider.</param>
	/// <param name="values">The values for initialization.</param>
	/// <filterpriority>2</filterpriority>
	public override void Initialize(string name, NameValueCollection values)
	{
		if (name == null)
		{
			name = "LocalFileSettingsProvider";
		}
		if (values != null)
		{
			impl.ApplicationName = values["applicationName"];
		}
		base.Initialize(name, values);
	}

	/// <summary>Resets all application settings properties associated with the specified application to their default values.</summary>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application usage.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A user-scoped setting was encountered but the current configuration only supports application-scoped settings.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void Reset(SettingsContext context)
	{
		impl.Reset(context);
	}

	/// <summary>Sets the values of the specified group of property settings.</summary>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application usage.</param>
	/// <param name="values">A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> representing the group of property settings to set.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A user-scoped setting was encountered but the current configuration only supports application-scoped settings.-or-There was a general failure saving the settings to the configuration file.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	[System.MonoTODO]
	public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
	{
		impl.SetPropertyValues(context, values);
	}

	/// <summary>Attempts to migrate previous user-scoped settings from a previous version of the same application.</summary>
	/// <param name="context">A <see cref="T:System.Configuration.SettingsContext" /> describing the current application usage. </param>
	/// <param name="properties">A <see cref="T:System.Configuration.SettingsPropertyCollection" /> containing the settings property group whose values are to be retrieved. </param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A user-scoped setting was encountered but the current configuration only supports application-scoped settings.-or-The previous version of the configuration file could not be accessed.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPrincipal" />
	/// </PermissionSet>
	[System.MonoTODO]
	public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
	{
		impl.Upgrade(context, properties);
	}
}
