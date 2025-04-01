namespace System.ComponentModel.Design;

/// <summary>Provides an interface for adding and removing extender providers at design time.</summary>
public interface IExtenderProviderService
{
	/// <summary>Adds the specified extender provider.</summary>
	/// <param name="provider">The extender provider to add. </param>
	void AddExtenderProvider(IExtenderProvider provider);

	/// <summary>Removes the specified extender provider.</summary>
	/// <param name="provider">The extender provider to remove. </param>
	void RemoveExtenderProvider(IExtenderProvider provider);
}
