namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that extends <see cref="T:System.ComponentModel.Design.Serialization.IDesignerLoaderHost" /> to specify whether errors are tolerated while loading a design document.</summary>
public interface IDesignerLoaderHost2 : IDesignerLoaderHost, IDesignerHost, IServiceContainer, IServiceProvider
{
	/// <summary>Gets or sets a value indicating whether errors should be ignored when <see cref="M:System.ComponentModel.Design.Serialization.IDesignerLoaderHost.Reload" /> is called.</summary>
	/// <returns>true if the designer loader will ignore errors when it reloads; otherwise, false. The default is false.</returns>
	bool IgnoreErrorsDuringReload { get; set; }

	/// <summary>Gets or sets a value indicating whether it is possible to reload with errors. </summary>
	/// <returns>true if the designer loader can reload the design document when errors are detected; otherwise, false. The default is false.</returns>
	bool CanReloadWithErrors { get; set; }
}
