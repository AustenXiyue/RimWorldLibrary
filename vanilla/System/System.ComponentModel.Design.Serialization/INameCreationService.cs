namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides a service that can generate unique names for objects.</summary>
public interface INameCreationService
{
	/// <summary>Creates a new name that is unique to all components in the specified container.</summary>
	/// <returns>A unique name for the data type.</returns>
	/// <param name="container">The container where the new object is added. </param>
	/// <param name="dataType">The data type of the object that receives the name. </param>
	string CreateName(IContainer container, Type dataType);

	/// <summary>Gets a value indicating whether the specified name is valid.</summary>
	/// <returns>true if the name is valid; otherwise, false.</returns>
	/// <param name="name">The name to validate. </param>
	bool IsValidName(string name);

	/// <summary>Gets a value indicating whether the specified name is valid.</summary>
	/// <param name="name">The name to validate. </param>
	void ValidateName(string name);
}
