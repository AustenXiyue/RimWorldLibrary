using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Provides a multilevel switch to control tracing and debug output without recompiling your code.</summary>
/// <filterpriority>2</filterpriority>
[SwitchLevel(typeof(TraceLevel))]
public class TraceSwitch : Switch
{
	/// <summary>Gets or sets the trace level that determines the messages the switch allows.</summary>
	/// <returns>One of the <see cref="T:System.Diagnostics.TraceLevel" /> values that that specifies the level of messages that are allowed by the switch.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Diagnostics.TraceSwitch.Level" /> is set to a value that is not one of the <see cref="T:System.Diagnostics.TraceLevel" /> values. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public TraceLevel Level
	{
		get
		{
			return (TraceLevel)base.SwitchSetting;
		}
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		set
		{
			if (value < TraceLevel.Off || value > TraceLevel.Verbose)
			{
				throw new ArgumentException(global::SR.GetString("The Level must be set to a value in the enumeration TraceLevel."));
			}
			base.SwitchSetting = (int)value;
		}
	}

	/// <summary>Gets a value indicating whether the switch allows error-handling messages.</summary>
	/// <returns>true if the <see cref="P:System.Diagnostics.TraceSwitch.Level" /> property is set to <see cref="F:System.Diagnostics.TraceLevel.Error" />, <see cref="F:System.Diagnostics.TraceLevel.Warning" />, <see cref="F:System.Diagnostics.TraceLevel.Info" />, or <see cref="F:System.Diagnostics.TraceLevel.Verbose" />; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool TraceError => Level >= TraceLevel.Error;

	/// <summary>Gets a value indicating whether the switch allows warning messages.</summary>
	/// <returns>true if the <see cref="P:System.Diagnostics.TraceSwitch.Level" /> property is set to <see cref="F:System.Diagnostics.TraceLevel.Warning" />, <see cref="F:System.Diagnostics.TraceLevel.Info" />, or <see cref="F:System.Diagnostics.TraceLevel.Verbose" />; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool TraceWarning => Level >= TraceLevel.Warning;

	/// <summary>Gets a value indicating whether the switch allows informational messages.</summary>
	/// <returns>true if the <see cref="P:System.Diagnostics.TraceSwitch.Level" /> property is set to <see cref="F:System.Diagnostics.TraceLevel.Info" /> or <see cref="F:System.Diagnostics.TraceLevel.Verbose" />; otherwise, false.  </returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool TraceInfo => Level >= TraceLevel.Info;

	/// <summary>Gets a value indicating whether the switch allows all messages.</summary>
	/// <returns>true if the <see cref="P:System.Diagnostics.TraceSwitch.Level" /> property is set to <see cref="F:System.Diagnostics.TraceLevel.Verbose" />; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool TraceVerbose => Level == TraceLevel.Verbose;

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceSwitch" /> class, using the specified display name and description.</summary>
	/// <param name="displayName">The name to display on a user interface. </param>
	/// <param name="description">The description of the switch. </param>
	public TraceSwitch(string displayName, string description)
		: base(displayName, description)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.TraceSwitch" /> class, using the specified display name, description, and default value for the switch. </summary>
	/// <param name="displayName">The name to display on a user interface. </param>
	/// <param name="description">The description of the switch. </param>
	/// <param name="defaultSwitchValue">The default value of the switch.</param>
	public TraceSwitch(string displayName, string description, string defaultSwitchValue)
		: base(displayName, description, defaultSwitchValue)
	{
	}

	/// <summary>Updates and corrects the level for this switch.</summary>
	protected override void OnSwitchSettingChanged()
	{
		int num = base.SwitchSetting;
		if (num < 0)
		{
			base.SwitchSetting = 0;
		}
		else if (num > 4)
		{
			base.SwitchSetting = 4;
		}
	}

	/// <summary>Sets the <see cref="P:System.Diagnostics.Switch.SwitchSetting" /> property to the integer equivalent of the <see cref="P:System.Diagnostics.Switch.Value" /> property.</summary>
	protected override void OnValueChanged()
	{
		base.SwitchSetting = (int)Enum.Parse(typeof(TraceLevel), base.Value, ignoreCase: true);
	}
}
