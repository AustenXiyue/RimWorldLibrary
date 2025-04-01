namespace System.ComponentModel;

/// <summary>Provides functionality for nested containers, which logically contain zero or more other components and are owned by a parent component.</summary>
public interface INestedContainer : IContainer, IDisposable
{
	/// <summary>Gets the owning component for the nested container.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.IComponent" /> that owns the nested container.</returns>
	IComponent Owner { get; }
}
