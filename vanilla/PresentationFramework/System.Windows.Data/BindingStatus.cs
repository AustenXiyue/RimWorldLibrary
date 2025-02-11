namespace System.Windows.Data;

/// <summary>Describes the status of a binding.</summary>
public enum BindingStatus
{
	/// <summary>The binding has not yet been attached to its target property.</summary>
	Unattached,
	/// <summary>The binding has not been activated.</summary>
	Inactive,
	/// <summary>The binding has been successfully activated. This means that the binding has been attached to its binding target (target) property and has located the binding source (source), resolved the Path and/or XPath, and begun transferring values.</summary>
	Active,
	/// <summary>The binding has been detached from its target property.</summary>
	Detached,
	/// <summary>The binding is waiting for an asynchronous operation to complete.</summary>
	AsyncRequestPending,
	/// <summary>The binding was unable to resolve the source path.</summary>
	PathError,
	/// <summary>The binding could not successfully return a source value to update the target value. For more information, see the remarks on <see cref="P:System.Windows.Data.BindingBase.FallbackValue" />.</summary>
	UpdateTargetError,
	/// <summary>The binding was unable to send the value to the source property.</summary>
	UpdateSourceError
}
