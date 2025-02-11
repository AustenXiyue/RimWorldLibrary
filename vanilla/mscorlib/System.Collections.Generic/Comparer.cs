using System.Runtime.CompilerServices;
using System.Security;

namespace System.Collections.Generic;

/// <summary>Provides a base class for implementations of the <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
/// <typeparam name="T">The type of objects to compare.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[TypeDependency("System.Collections.Generic.ObjectComparer`1")]
public abstract class Comparer<T> : IComparer, IComparer<T>
{
	private static volatile Comparer<T> defaultComparer;

	/// <summary>Returns a default sort order comparer for the type specified by the generic argument.</summary>
	/// <returns>An object that inherits <see cref="T:System.Collections.Generic.Comparer`1" /> and serves as a sort order comparer for type <paramref name="T" />.</returns>
	public static Comparer<T> Default
	{
		get
		{
			Comparer<T> comparer = defaultComparer;
			if (comparer == null)
			{
				comparer = (defaultComparer = CreateComparer());
			}
			return comparer;
		}
	}

	/// <summary>Creates a comparer by using the specified comparison.</summary>
	/// <returns>The new comparer.</returns>
	/// <param name="comparison">The comparison to use.</param>
	public static Comparer<T> Create(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		return new ComparisonComparer<T>(comparison);
	}

	[SecuritySafeCritical]
	private static Comparer<T> CreateComparer()
	{
		RuntimeType runtimeType = (RuntimeType)typeof(T);
		if (typeof(IComparable<T>).IsAssignableFrom(runtimeType))
		{
			return (Comparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter(typeof(GenericComparer<>), runtimeType);
		}
		if (runtimeType.IsGenericType && runtimeType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			RuntimeType runtimeType2 = (RuntimeType)runtimeType.GetGenericArguments()[0];
			if (typeof(IComparable<>).MakeGenericType(runtimeType2).IsAssignableFrom(runtimeType2))
			{
				return (Comparer<T>)RuntimeType.CreateInstanceForAnotherGenericParameter(typeof(NullableComparer<>), runtimeType2);
			}
		}
		return new ObjectComparer<T>();
	}

	/// <summary>When overridden in a derived class, performs a comparison of two objects of the same type and returns a value indicating whether one object is less than, equal to, or greater than the other.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />.Zero <paramref name="x" /> equals <paramref name="y" />.Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.</returns>
	/// <param name="x">The first object to compare.</param>
	/// <param name="y">The second object to compare.</param>
	/// <exception cref="T:System.ArgumentException">Type <paramref name="T" /> does not implement either the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface.</exception>
	public abstract int Compare(T x, T y);

	/// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
	/// <param name="x">The first object to compare.</param>
	/// <param name="y">The second object to compare.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="x" /> or <paramref name="y" /> is of a type that cannot be cast to type <paramref name="T" />.-or-<paramref name="x" /> and <paramref name="y" /> do not implement either the <see cref="T:System.IComparable`1" /> generic interface or the <see cref="T:System.IComparable" /> interface.</exception>
	int IComparer.Compare(object x, object y)
	{
		if (x == null)
		{
			if (y != null)
			{
				return -1;
			}
			return 0;
		}
		if (y == null)
		{
			return 1;
		}
		if (x is T && y is T)
		{
			return Compare((T)x, (T)y);
		}
		ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
		return 0;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Comparer`1" /> class.</summary>
	protected Comparer()
	{
	}
}
