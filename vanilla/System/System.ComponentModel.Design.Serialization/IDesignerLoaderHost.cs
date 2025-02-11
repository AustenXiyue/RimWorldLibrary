using System.Collections;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides an interface that can extend a designer host to support loading from a serialized state.</summary>
public interface IDesignerLoaderHost : IDesignerHost, IServiceContainer, IServiceProvider
{
	/// <summary>Ends the designer loading operation.</summary>
	/// <param name="baseClassName">The fully qualified name of the base class of the document that this designer is designing. </param>
	/// <param name="successful">true if the designer is successfully loaded; otherwise, false. </param>
	/// <param name="errorCollection">A collection containing the errors encountered during load, if any. If no errors were encountered, pass either an empty collection or null. </param>
	void EndLoad(string baseClassName, bool successful, ICollection errorCollection);

	/// <summary>Reloads the design document.</summary>
	void Reload();
}
