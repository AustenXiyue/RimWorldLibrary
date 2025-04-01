using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq;

/// <summary>Represents an <see cref="T:System.Collections.IEnumerable" /> as an <see cref="T:System.Linq.EnumerableQuery" /> data source. </summary>
public abstract class EnumerableQuery
{
	internal abstract Expression Expression { get; }

	internal abstract IEnumerable Enumerable { get; }

	internal static IQueryable Create(Type elementType, IEnumerable sequence)
	{
		return (IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(elementType), sequence);
	}

	internal static IQueryable Create(Type elementType, Expression expression)
	{
		return (IQueryable)Activator.CreateInstance(typeof(EnumerableQuery<>).MakeGenericType(elementType), expression);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableQuery" /> class.</summary>
	protected EnumerableQuery()
	{
	}
}
/// <summary>Represents an <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection as an <see cref="T:System.Linq.IQueryable`1" /> data source.</summary>
/// <typeparam name="T">The type of the data in the collection.</typeparam>
public class EnumerableQuery<T> : EnumerableQuery, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IOrderedQueryable, IQueryProvider
{
	private readonly Expression _expression;

	private IEnumerable<T> _enumerable;

	/// <summary>Gets the query provider that is associated with this instance.</summary>
	/// <returns>The query provider that is associated with this instance.</returns>
	IQueryProvider IQueryable.Provider => this;

	internal override Expression Expression => _expression;

	internal override IEnumerable Enumerable => _enumerable;

	/// <summary>Gets the expression tree that is associated with or that represents this instance.</summary>
	/// <returns>The expression tree that is associated with or that represents this instance.</returns>
	Expression IQueryable.Expression => _expression;

	/// <summary>Gets the type of the data in the collection that this instance represents.</summary>
	/// <returns>The type of the data in the collection that this instance represents.</returns>
	Type IQueryable.ElementType => typeof(T);

	/// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableQuery`1" /> class and associates it with an <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection.</summary>
	/// <param name="enumerable">A collection to associate with the new instance.</param>
	public EnumerableQuery(IEnumerable<T> enumerable)
	{
		_enumerable = enumerable;
		_expression = Expression.Constant(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Linq.EnumerableQuery`1" /> class and associates the instance with an expression tree.</summary>
	/// <param name="expression">An expression tree to associate with the new instance.</param>
	public EnumerableQuery(Expression expression)
	{
		_expression = expression;
	}

	/// <summary>Constructs a new <see cref="T:System.Linq.EnumerableQuery`1" /> object and associates it with a specified expression tree that represents an <see cref="T:System.Linq.IQueryable" /> collection of data.</summary>
	/// <returns>An <see cref="T:System.Linq.EnumerableQuery`1" /> object that is associated with <paramref name="expression" />.</returns>
	/// <param name="expression">An expression tree that represents an <see cref="T:System.Linq.IQueryable" /> collection of data.</param>
	IQueryable IQueryProvider.CreateQuery(Expression expression)
	{
		if (expression == null)
		{
			throw Error.ArgumentNull("expression");
		}
		Type type = TypeHelper.FindGenericType(typeof(IQueryable<>), expression.Type);
		if (type == null)
		{
			throw Error.ArgumentNotValid("expression");
		}
		return EnumerableQuery.Create(type.GetGenericArguments()[0], expression);
	}

	/// <summary>Constructs a new <see cref="T:System.Linq.EnumerableQuery`1" /> object and associates it with a specified expression tree that represents an <see cref="T:System.Linq.IQueryable`1" /> collection of data.</summary>
	/// <returns>An EnumerableQuery object that is associated with <paramref name="expression" />.</returns>
	/// <param name="expression">An expression tree to execute.</param>
	/// <typeparam name="S">The type of the data in the collection that <paramref name="expression" /> represents.</typeparam>
	IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
	{
		if (expression == null)
		{
			throw Error.ArgumentNull("expression");
		}
		if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
		{
			throw Error.ArgumentNotValid("expression");
		}
		return new EnumerableQuery<TElement>(expression);
	}

	/// <summary>Executes an expression after rewriting it to call <see cref="T:System.Linq.Enumerable" /> methods instead of <see cref="T:System.Linq.Queryable" /> methods on any enumerable data sources that cannot be queried by <see cref="T:System.Linq.Queryable" /> methods.</summary>
	/// <returns>The value that results from executing <paramref name="expression" />.</returns>
	/// <param name="expression">An expression tree to execute.</param>
	object IQueryProvider.Execute(Expression expression)
	{
		if (expression == null)
		{
			throw Error.ArgumentNull("expression");
		}
		return EnumerableExecutor.Create(expression).ExecuteBoxed();
	}

	/// <summary>Executes an expression after rewriting it to call <see cref="T:System.Linq.Enumerable" /> methods instead of <see cref="T:System.Linq.Queryable" /> methods on any enumerable data sources that cannot be queried by <see cref="T:System.Linq.Queryable" /> methods.</summary>
	/// <returns>The value that results from executing <paramref name="expression" />.</returns>
	/// <param name="expression">An expression tree to execute.</param>
	/// <typeparam name="S">The type of the data in the collection that <paramref name="expression" /> represents.</typeparam>
	TElement IQueryProvider.Execute<TElement>(Expression expression)
	{
		if (expression == null)
		{
			throw Error.ArgumentNull("expression");
		}
		if (!typeof(TElement).IsAssignableFrom(expression.Type))
		{
			throw Error.ArgumentNotValid("expression");
		}
		return new EnumerableExecutor<TElement>(expression).Execute();
	}

	/// <summary>Returns an enumerator that can iterate through the associated <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection, or, if it is null, through the collection that results from rewriting the associated expression tree as a query on an <see cref="T:System.Collections.Generic.IEnumerable`1" /> data source and executing it.</summary>
	/// <returns>An enumerator that can be used to iterate through the associated data source.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Returns an enumerator that can iterate through the associated <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection, or, if it is null, through the collection that results from rewriting the associated expression tree as a query on an <see cref="T:System.Collections.Generic.IEnumerable`1" /> data source and executing it.</summary>
	/// <returns>An enumerator that can be used to iterate through the associated data source.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	private IEnumerator<T> GetEnumerator()
	{
		if (_enumerable == null)
		{
			IEnumerable<T> enumerable = Expression.Lambda<Func<IEnumerable<T>>>(new EnumerableRewriter().Visit(_expression), (IEnumerable<ParameterExpression>)null).Compile()();
			if (enumerable == this)
			{
				throw Error.EnumeratingNullEnumerableExpression();
			}
			_enumerable = enumerable;
		}
		return _enumerable.GetEnumerator();
	}

	/// <summary>Returns a textual representation of the enumerable collection or, if it is null, of the expression tree that is associated with this instance.</summary>
	/// <returns>A textual representation of the enumerable collection or, if it is null, of the expression tree that is associated with this instance.</returns>
	public override string ToString()
	{
		if (_expression is ConstantExpression constantExpression && constantExpression.Value == this)
		{
			if (_enumerable != null)
			{
				return _enumerable.ToString();
			}
			return "null";
		}
		return _expression.ToString();
	}
}
