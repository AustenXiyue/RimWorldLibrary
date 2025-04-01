using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition;

/// <summary>When applied to a <see cref="T:System.Reflection.Assembly" /> object, enables an <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> object to discover custom <see cref="T:System.Reflection.ReflectionContext" /> objects.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class CatalogReflectionContextAttribute : Attribute
{
	private Type _reflectionContextType;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AssemblyCatalog" /> class with the specified <see cref="T:System.Reflection.ReflectionContext" /> type.</summary>
	/// <param name="reflectionContextType">The type of the reflection context.</param>
	public CatalogReflectionContextAttribute(Type reflectionContextType)
	{
		Requires.NotNull(reflectionContextType, "reflectionContextType");
		_reflectionContextType = reflectionContextType;
	}

	/// <summary>Creates an instance of the custom <see cref="T:System.Reflection.ReflectionContext" /> object.</summary>
	/// <returns>An instance of the custom reflection context.</returns>
	public ReflectionContext CreateReflectionContext()
	{
		Assumes.NotNull(_reflectionContextType);
		ReflectionContext reflectionContext = null;
		try
		{
			return (ReflectionContext)Activator.CreateInstance(_reflectionContextType);
		}
		catch (InvalidCastException innerException)
		{
			throw new InvalidOperationException(Strings.ReflectionContext_Type_Required, innerException);
		}
		catch (MissingMethodException inner)
		{
			throw new MissingMethodException(Strings.ReflectionContext_Requires_DefaultConstructor, inner);
		}
	}
}
