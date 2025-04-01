using System.Collections;

namespace System.ComponentModel;

/// <summary>Defines members that data entity classes can implement to provide custom synchronous and asynchronous validation support.</summary>
public interface INotifyDataErrorInfo
{
	/// <summary>Gets a value that indicates whether the entity has validation errors. </summary>
	/// <returns>true if the entity currently has validation errors; otherwise, false.</returns>
	bool HasErrors { get; }

	/// <summary>Occurs when the validation errors have changed for a property or for the entire entity. </summary>
	event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

	/// <summary>Gets the validation errors for a specified property or for the entire entity.</summary>
	/// <returns>The validation errors for the property or entity.</returns>
	/// <param name="propertyName">The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to retrieve entity-level errors.</param>
	IEnumerable GetErrors(string propertyName);
}
