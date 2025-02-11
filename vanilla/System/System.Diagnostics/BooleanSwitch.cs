using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a simple on/off switch that controls debugging and tracing output.</summary>
/// <filterpriority>2</filterpriority>
[SwitchLevel(typeof(bool))]
public class BooleanSwitch : Switch
{
	/// <summary>Gets or sets a value indicating whether the switch is enabled or disabled.</summary>
	/// <returns>true if the switch is enabled; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the correct permission.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public bool Enabled
	{
		get
		{
			if (base.SwitchSetting != 0)
			{
				return true;
			}
			return false;
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		set
		{
			base.SwitchSetting = (value ? 1 : 0);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.BooleanSwitch" /> class with the specified display name and description.</summary>
	/// <param name="displayName">The name to display on a user interface. </param>
	/// <param name="description">The description of the switch. </param>
	public BooleanSwitch(string displayName, string description)
		: base(displayName, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.BooleanSwitch" /> class with the specified display name, description, and default switch value. </summary>
	/// <param name="displayName">The name to display on the user interface. </param>
	/// <param name="description">The description of the switch. </param>
	/// <param name="defaultSwitchValue">The default value of the switch.</param>
	public BooleanSwitch(string displayName, string description, string defaultSwitchValue)
		: base(displayName, description, defaultSwitchValue)
	{
	}

	/// <summary>Determines whether the new value of the <see cref="P:System.Diagnostics.Switch.Value" /> property can be parsed as a Boolean value.</summary>
	protected override void OnValueChanged()
	{
		if (bool.TryParse(base.Value, out var result))
		{
			base.SwitchSetting = (result ? 1 : 0);
		}
		else
		{
			base.OnValueChanged();
		}
	}
}
