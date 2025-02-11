using System.Collections;

namespace System.ComponentModel.Design;

/// <summary>Provides support for building a set of related custom designers.</summary>
public interface ITreeDesigner : IDesigner, IDisposable
{
	/// <summary>Gets a collection of child designers.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" />, containing the collection of <see cref="T:System.ComponentModel.Design.IDesigner" /> child objects of the current designer. </returns>
	ICollection Children { get; }

	/// <summary>Gets the parent designer.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.Design.IDesigner" /> representing the parent designer, or null if there is no parent.</returns>
	IDesigner Parent { get; }
}
