using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

/// <summary>Exposes the enumerator, which supports a simple iteration over a collection of a specified type.</summary>
/// <typeparam name="T">The type of objects to enumerate.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>1</filterpriority>
[TypeDependency("System.SZArrayHelper")]
public interface IEnumerable<out T> : IEnumerable
{
	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
	/// <filterpriority>1</filterpriority>
	new IEnumerator<T> GetEnumerator();
}
