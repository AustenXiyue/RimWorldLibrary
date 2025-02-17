using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq;

/// <summary>Provides functionality to evaluate queries against a specific data source wherein the type of the data is not specified.</summary>
/// <filterpriority>2</filterpriority>
public interface IQueryable : IEnumerable
{
	/// <summary>Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
	Expression Expression { get; }

	/// <summary>Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
	Type ElementType { get; }

	/// <summary>Gets the query provider that is associated with this data source.</summary>
	/// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
	IQueryProvider Provider { get; }
}
/// <summary>Provides functionality to evaluate queries against a specific data source wherein the type of the data is known.</summary>
/// <typeparam name="T">The type of the data in the data source.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable
{
}
