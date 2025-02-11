using System.Collections.Generic;

namespace System.Xaml;

/// <summary>Represents a service that can return information items about ambient properties or ambient types to type converters and markup extensions.</summary>
public interface IAmbientProvider
{
	/// <summary>Returns a single ambient property information item from the requested set of properties, based on which property is first encountered.</summary>
	/// <returns>A single ambient property information item for the first ambient property value from the <paramref name="properties" /> list that is found. </returns>
	/// <param name="ceilingTypes">Specifies one or more types that should stop the evaluation when they are encountered traversing upward in the object graph. This type holds the desired ambient property.</param>
	/// <param name="properties">Specifies one or more objects that identify the properties to be considered ambient.</param>
	AmbientPropertyValue GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);

	/// <summary>Returns the first matching object that is a possible ambient type for the requested types.</summary>
	/// <returns>The first result object for the requested set.</returns>
	/// <param name="types">The set of types from which to retrieve ambient type information.</param>
	object GetFirstAmbientValue(params XamlType[] types);

	/// <summary>Returns an enumerable set of ambient property information items for the requested set of properties.</summary>
	/// <returns>An enumerable set of ambient property information items for the requested set of properties. The property information for each <see cref="T:System.Xaml.AmbientPropertyValue" /> that is returned will match one of the input <paramref name="properties" />.</returns>
	/// <param name="ceilingTypes">Specifies one or more types that should stop the evaluation when they are encountered traversing upward in the object graph. This type holds the desired ambient property. May be null.</param>
	/// <param name="properties">Specifies one or more property identifier objects that identify the properties to be considered ambient.</param>
	IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties);

	/// <summary>Returns an enumerable set of object instances of possible ambient types for the requested types.</summary>
	/// <returns>An enumerable set of objects that represent the values for the requested set of <see cref="T:System.Xaml.XamlType" /> identifiers.</returns>
	/// <param name="types">The set of types from which to retrieve ambient type information.</param>
	IEnumerable<object> GetAllAmbientValues(params XamlType[] types);

	/// <summary>Returns an enumerable set of ambient property information items for the requested set of types and properties.</summary>
	/// <returns>An enumerable set of ambient property information items for the requested set of types and properties. The property information for each <see cref="T:System.Xaml.AmbientPropertyValue" /> that is returned will match one of the input <paramref name="types" /> or <paramref name="properties" />.</returns>
	/// <param name="ceilingTypes">Specifies one or more types that should stop the evaluation when they are encountered traversing upward in the object graph. This type holds the desired ambient property. May be null.</param>
	/// <param name="searchLiveStackOnly">true to not use a saved context; false to use a saved context. The default is false. See Remarks.</param>
	/// <param name="types">Specifies one or more type identifier objects that identify the types to be considered ambient.</param>
	/// <param name="properties">Specifies one or more property identifier objects that identify the properties to be considered ambient.</param>
	IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties);
}
