namespace System.Reflection;

/// <summary>Represents an object that provides a custom type.</summary>
public interface ICustomTypeProvider
{
	/// <summary>Gets the custom type provided by this object.</summary>
	/// <returns>The custom type. </returns>
	Type GetCustomType();
}
