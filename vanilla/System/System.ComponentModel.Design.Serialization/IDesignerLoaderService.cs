using System.Collections;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that can extend a designer loader to support asynchronous loading of external components.</summary>
public interface IDesignerLoaderService
{
	/// <summary>Registers an external component as part of the load process managed by this interface.</summary>
	void AddLoadDependency();

	/// <summary>Signals that a dependent load has finished.</summary>
	/// <param name="successful">true if the load of the designer is successful; false if errors prevented the load from finishing. </param>
	/// <param name="errorCollection">A collection of errors that occurred during the load, if any. If no errors occurred, pass either an empty collection or null. </param>
	void DependentLoadComplete(bool successful, ICollection errorCollection);

	/// <summary>Reloads the design document.</summary>
	/// <returns>true if the reload request is accepted, or false if the loader does not allow the reload.</returns>
	bool Reload();
}
