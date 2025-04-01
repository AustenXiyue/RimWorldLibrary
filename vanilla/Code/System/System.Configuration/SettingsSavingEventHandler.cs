using System.ComponentModel;

namespace System.Configuration;

/// <summary>Represents the method that will handle the <see cref="E:System.Configuration.ApplicationSettingsBase.SettingsSaving" /> event. </summary>
/// <param name="sender">The source of the event, typically a data container or data-bound collection.</param>
/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
/// <filterpriority>2</filterpriority>
public delegate void SettingsSavingEventHandler(object sender, CancelEventArgs e);
