using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System;

/// <summary>Provides static methods for creating tuple objects. </summary>
public static class Tuple
{
	/// <summary>Creates a new 1-tuple, or singleton.</summary>
	/// <returns>A tuple whose value is (<paramref name="item1" />).</returns>
	/// <param name="item1">The value of the only component of the tuple.</param>
	/// <typeparam name="T1">The type of the only component of the tuple.</typeparam>
	public static Tuple<T1> Create<T1>(T1 item1)
	{
		return new Tuple<T1>(item1);
	}

	/// <summary>Creates a new 2-tuple, or pair.</summary>
	/// <returns>A 2-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
	{
		return new Tuple<T1, T2>(item1, item2);
	}

	/// <summary>Creates a new 3-tuple, or triple.</summary>
	/// <returns>A 3-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
	{
		return new Tuple<T1, T2, T3>(item1, item2, item3);
	}

	/// <summary>Creates a new 4-tuple, or quadruple.</summary>
	/// <returns>A 4-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <param name="item4">The value of the fourth component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	/// <typeparam name="T4">The type of the fourth component of the tuple.  </typeparam>
	public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
	}

	/// <summary>Creates a new 5-tuple, or quintuple.</summary>
	/// <returns>A 5-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <param name="item4">The value of the fourth component of the tuple.</param>
	/// <param name="item5">The value of the fifth component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
	/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
	public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
	{
		return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
	}

	/// <summary>Creates a new 6-tuple, or sextuple.</summary>
	/// <returns>A 6-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <param name="item4">The value of the fourth component of the tuple.</param>
	/// <param name="item5">The value of the fifth component of the tuple.</param>
	/// <param name="item6">The value of the sixth component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
	/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
	/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
	public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
	{
		return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
	}

	/// <summary>Creates a new 7-tuple, or septuple.</summary>
	/// <returns>A 7-tuple whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />, <paramref name="item7" />).</returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <param name="item4">The value of the fourth component of the tuple.</param>
	/// <param name="item5">The value of the fifth component of the tuple.</param>
	/// <param name="item6">The value of the sixth component of the tuple.</param>
	/// <param name="item7">The value of the seventh component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
	/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
	/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
	/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
	public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
	{
		return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
	}

	/// <summary>Creates a new 8-tuple, or octuple.</summary>
	/// <returns>An 8-tuple (octuple) whose value is (<paramref name="item1" />, <paramref name="item2" />, <paramref name="item3" />, <paramref name="item4" />, <paramref name="item5" />, <paramref name="item6" />, <paramref name="item7" />, <paramref name="item8" />). </returns>
	/// <param name="item1">The value of the first component of the tuple.</param>
	/// <param name="item2">The value of the second component of the tuple.</param>
	/// <param name="item3">The value of the third component of the tuple.</param>
	/// <param name="item4">The value of the fourth component of the tuple.</param>
	/// <param name="item5">The value of the fifth component of the tuple.</param>
	/// <param name="item6">The value of the sixth component of the tuple.</param>
	/// <param name="item7">The value of the seventh component of the tuple.</param>
	/// <param name="item8">The value of the eighth component of the tuple.</param>
	/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
	/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
	/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
	/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
	/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
	/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
	/// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
	public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
	{
		return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(item1, item2, item3, item4, item5, item6, item7, new Tuple<T8>(item8));
	}

	internal static int CombineHashCodes(int h1, int h2)
	{
		return ((h1 << 5) + h1) ^ h2;
	}

	internal static int CombineHashCodes(int h1, int h2, int h3)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2), h3);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
	}
}
/// <summary>Represents a 1-tuple, or singleton. </summary>
/// <typeparam name="T1">The type of the tuple's only component.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	/// <summary>Gets the value of the <see cref="T:System.Tuple`1" /> object's single component. </summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`1" /> object's single component.</returns>
	public T1 Item1 => m_Item1;

	int ITuple.Length => 1;

	object ITuple.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return Item1;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`1" /> class.</summary>
	/// <param name="item1">The value of the tuple's only component.</param>
	public Tuple(T1 item1)
	{
		m_Item1 = item1;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`1" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`1" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1> tuple))
		{
			return false;
		}
		return comparer.Equals(m_Item1, tuple.m_Item1);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`1" /> object to a specified object, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`1" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`1" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`1" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		return comparer.Compare(m_Item1, tuple.m_Item1);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`1" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`1" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`1" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return comparer.GetHashCode(m_Item1);
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`1" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`1" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 2-tuple, or pair. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`2" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`2" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`2" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`2" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	int ITuple.Length => 2;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`2" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	public Tuple(T1 item1, T2 item2)
	{
		m_Item1 = item1;
		m_Item2 = item2;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`2" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`2" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1))
		{
			return comparer.Equals(m_Item2, tuple.m_Item2);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`2" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`2" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`2" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`2" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item2, tuple.m_Item2);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`2" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`2" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`2" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`2" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`2" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 3-tuple, or triple. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`3" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`3" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	int ITuple.Length => 3;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`3" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	public Tuple(T1 item1, T2 item2, T3 item3)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`3" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`3" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2))
		{
			return comparer.Equals(m_Item3, tuple.m_Item3);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`3" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`3" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`3" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`3" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item3, tuple.m_Item3);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`3" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`3" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`3" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`3" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`3" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 4-tuple, or quadruple. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`4" /> object's fourth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`4" /> object's fourth component.</returns>
	public T4 Item4 => m_Item4;

	int ITuple.Length => 4;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		3 => Item4, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`4" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	/// <param name="item4">The value of the tuple's fourth component</param>
	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
		m_Item4 = item4;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`4" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`4" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false. </returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2) && comparer.Equals(m_Item3, tuple.m_Item3))
		{
			return comparer.Equals(m_Item4, tuple.m_Item4);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`4" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`4" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`4" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`4" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item4, tuple.m_Item4);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`4" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`4" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`4" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`4" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`4" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(", ");
		sb.Append(m_Item4);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 5-tuple, or quintuple. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	private readonly T5 m_Item5;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's fourth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's fourth component.</returns>
	public T4 Item4 => m_Item4;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`5" /> object's fifth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`5" /> object's fifth component.</returns>
	public T5 Item5 => m_Item5;

	int ITuple.Length => 5;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		3 => Item4, 
		4 => Item5, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`5" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	/// <param name="item4">The value of the tuple's fourth component</param>
	/// <param name="item5">The value of the tuple's fifth component.</param>
	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
		m_Item4 = item4;
		m_Item5 = item5;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`5" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`5" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2) && comparer.Equals(m_Item3, tuple.m_Item3) && comparer.Equals(m_Item4, tuple.m_Item4))
		{
			return comparer.Equals(m_Item5, tuple.m_Item5);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`5" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`5" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`5" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`5" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item4, tuple.m_Item4);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item5, tuple.m_Item5);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`5" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`5" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`5" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`5" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`5" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(", ");
		sb.Append(m_Item4);
		sb.Append(", ");
		sb.Append(m_Item5);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 6-tuple, or sextuple. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	private readonly T5 m_Item5;

	private readonly T6 m_Item6;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's fourth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's fourth component.</returns>
	public T4 Item4 => m_Item4;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's fifth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's fifth  component.</returns>
	public T5 Item5 => m_Item5;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`6" /> object's sixth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`6" /> object's sixth component.</returns>
	public T6 Item6 => m_Item6;

	int ITuple.Length => 6;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		3 => Item4, 
		4 => Item5, 
		5 => Item6, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`6" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	/// <param name="item4">The value of the tuple's fourth component</param>
	/// <param name="item5">The value of the tuple's fifth component.</param>
	/// <param name="item6">The value of the tuple's sixth component.</param>
	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
		m_Item4 = item4;
		m_Item5 = item5;
		m_Item6 = item6;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`6" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`6" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2) && comparer.Equals(m_Item3, tuple.m_Item3) && comparer.Equals(m_Item4, tuple.m_Item4) && comparer.Equals(m_Item5, tuple.m_Item5))
		{
			return comparer.Equals(m_Item6, tuple.m_Item6);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`6" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`6" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`6" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`6" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item4, tuple.m_Item4);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item5, tuple.m_Item5);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item6, tuple.m_Item6);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`6" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`6" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`6" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`6" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`6" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(", ");
		sb.Append(m_Item4);
		sb.Append(", ");
		sb.Append(m_Item5);
		sb.Append(", ");
		sb.Append(m_Item6);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents a 7-tuple, or septuple. </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
/// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	private readonly T5 m_Item5;

	private readonly T6 m_Item6;

	private readonly T7 m_Item7;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's fourth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's fourth component.</returns>
	public T4 Item4 => m_Item4;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's fifth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's fifth component.</returns>
	public T5 Item5 => m_Item5;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's sixth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's sixth component.</returns>
	public T6 Item6 => m_Item6;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`7" /> object's seventh component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`7" /> object's seventh component.</returns>
	public T7 Item7 => m_Item7;

	int ITuple.Length => 7;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		3 => Item4, 
		4 => Item5, 
		5 => Item6, 
		6 => Item7, 
		_ => throw new IndexOutOfRangeException(), 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`7" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	/// <param name="item4">The value of the tuple's fourth component</param>
	/// <param name="item5">The value of the tuple's fifth component.</param>
	/// <param name="item6">The value of the tuple's sixth component.</param>
	/// <param name="item7">The value of the tuple's seventh component.</param>
	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
	{
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
		m_Item4 = item4;
		m_Item5 = item5;
		m_Item6 = item6;
		m_Item7 = item7;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`7" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`7" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2) && comparer.Equals(m_Item3, tuple.m_Item3) && comparer.Equals(m_Item4, tuple.m_Item4) && comparer.Equals(m_Item5, tuple.m_Item5) && comparer.Equals(m_Item6, tuple.m_Item6))
		{
			return comparer.Equals(m_Item7, tuple.m_Item7);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`7" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`7" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`7" /> object to a specified object by using a specified comparer, and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`7" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item4, tuple.m_Item4);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item5, tuple.m_Item5);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item6, tuple.m_Item6);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Item7, tuple.m_Item7);
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Tuple`7" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`7" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" /> method calculates the hash code of the current <see cref="T:System.Tuple`7" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7));
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`7" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`7" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(", ");
		sb.Append(m_Item4);
		sb.Append(", ");
		sb.Append(m_Item5);
		sb.Append(", ");
		sb.Append(m_Item6);
		sb.Append(", ");
		sb.Append(m_Item7);
		sb.Append(')');
		return sb.ToString();
	}
}
/// <summary>Represents an n-tuple, where n is 8 or greater.</summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
/// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
/// <typeparam name="TRest">Any generic Tuple object that defines the types of the tuple's remaining components.</typeparam>
/// <filterpriority>2</filterpriority>
[Serializable]
public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
{
	private readonly T1 m_Item1;

	private readonly T2 m_Item2;

	private readonly T3 m_Item3;

	private readonly T4 m_Item4;

	private readonly T5 m_Item5;

	private readonly T6 m_Item6;

	private readonly T7 m_Item7;

	private readonly TRest m_Rest;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's first component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's first component.</returns>
	public T1 Item1 => m_Item1;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's second component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's second component.</returns>
	public T2 Item2 => m_Item2;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's third component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's third component.</returns>
	public T3 Item3 => m_Item3;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's fourth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's fourth component.</returns>
	public T4 Item4 => m_Item4;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's fifth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's fifth component.</returns>
	public T5 Item5 => m_Item5;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's sixth component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's sixth component.</returns>
	public T6 Item6 => m_Item6;

	/// <summary>Gets the value of the current <see cref="T:System.Tuple`8" /> object's seventh component.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's seventh component.</returns>
	public T7 Item7 => m_Item7;

	/// <summary>Gets the current <see cref="T:System.Tuple`8" /> object's remaining components.</summary>
	/// <returns>The value of the current <see cref="T:System.Tuple`8" /> object's remaining components.</returns>
	public TRest Rest => m_Rest;

	int ITuple.Length => 7 + ((ITupleInternal)(object)Rest).Length;

	object ITuple.this[int index] => index switch
	{
		0 => Item1, 
		1 => Item2, 
		2 => Item3, 
		3 => Item4, 
		4 => Item5, 
		5 => Item6, 
		6 => Item7, 
		_ => ((ITupleInternal)(object)Rest)[index - 7], 
	};

	/// <summary>Initializes a new instance of the <see cref="T:System.Tuple`8" /> class.</summary>
	/// <param name="item1">The value of the tuple's first component.</param>
	/// <param name="item2">The value of the tuple's second component.</param>
	/// <param name="item3">The value of the tuple's third component.</param>
	/// <param name="item4">The value of the tuple's fourth component</param>
	/// <param name="item5">The value of the tuple's fifth component.</param>
	/// <param name="item6">The value of the tuple's sixth component.</param>
	/// <param name="item7">The value of the tuple's seventh component.</param>
	/// <param name="rest">Any generic Tuple object that contains the values of the tuple's remaining components.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="rest" /> is not a generic Tuple object.</exception>
	public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
	{
		if (!(rest is ITupleInternal))
		{
			throw new ArgumentException("The last element of an eight element tuple must be a Tuple.");
		}
		m_Item1 = item1;
		m_Item2 = item2;
		m_Item3 = item3;
		m_Item4 = item4;
		m_Item5 = item5;
		m_Item6 = item6;
		m_Item7 = item7;
		m_Rest = rest;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`8" /> object is equal to a specified object.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with this instance.</param>
	public override bool Equals(object obj)
	{
		return ((IStructuralEquatable)this).Equals(obj, (IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Tuple`8" /> object is equal to a specified object based on a specified comparison method.</summary>
	/// <returns>true if the current instance is equal to the specified object; otherwise, false.</returns>
	/// <param name="other">The object to compare with this instance.</param>
	/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple))
		{
			return false;
		}
		if (comparer.Equals(m_Item1, tuple.m_Item1) && comparer.Equals(m_Item2, tuple.m_Item2) && comparer.Equals(m_Item3, tuple.m_Item3) && comparer.Equals(m_Item4, tuple.m_Item4) && comparer.Equals(m_Item5, tuple.m_Item5) && comparer.Equals(m_Item6, tuple.m_Item6) && comparer.Equals(m_Item7, tuple.m_Item7))
		{
			return comparer.Equals(m_Rest, tuple.m_Rest);
		}
		return false;
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`8" /> object to a specified object and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="obj" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="obj" />.ZeroThis instance and <paramref name="obj" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="obj" />.</returns>
	/// <param name="obj">An object to compare with the current instance.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="obj" /> is not a <see cref="T:System.Tuple`8" /> object.</exception>
	int IComparable.CompareTo(object obj)
	{
		return ((IStructuralComparable)this).CompareTo(obj, (IComparer)LowLevelComparer.Default);
	}

	/// <summary>Compares the current <see cref="T:System.Tuple`8" /> object to a specified object by using a specified comparer and returns an integer that indicates whether the current object is before, after, or in the same position as the specified object in the sort order.</summary>
	/// <returns>A signed integer that indicates the relative position of this instance and <paramref name="other" /> in the sort order, as shown in the following table.ValueDescriptionA negative integerThis instance precedes <paramref name="other" />.ZeroThis instance and <paramref name="other" /> have the same position in the sort order.A positive integerThis instance follows <paramref name="other" />.</returns>
	/// <param name="other">An object to compare with the current instance.</param>
	/// <param name="comparer">An object that provides custom rules for comparison.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="other" /> is not a <see cref="T:System.Tuple`8" /> object.</exception>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple))
		{
			throw new ArgumentException(SR.Format("Argument must be of type {0}.", GetType().ToString()), "other");
		}
		int num = 0;
		num = comparer.Compare(m_Item1, tuple.m_Item1);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item2, tuple.m_Item2);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item3, tuple.m_Item3);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item4, tuple.m_Item4);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item5, tuple.m_Item5);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item6, tuple.m_Item6);
		if (num != 0)
		{
			return num;
		}
		num = comparer.Compare(m_Item7, tuple.m_Item7);
		if (num != 0)
		{
			return num;
		}
		return comparer.Compare(m_Rest, tuple.m_Rest);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`8" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return ((IStructuralEquatable)this).GetHashCode((IEqualityComparer)ObjectEqualityComparer.Default);
	}

	/// <summary>Calculates the hash code for the current <see cref="T:System.Tuple`8" /> object by using a specified computation method.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <param name="comparer">An object whose <see cref="M:System.Collections.IEqualityComparer.GetHashCode(System.Object)" />  method calculates the hash code of the current <see cref="T:System.Tuple`8" /> object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		ITupleInternal tupleInternal = (ITupleInternal)(object)m_Rest;
		if (tupleInternal.Length >= 8)
		{
			return tupleInternal.GetHashCode(comparer);
		}
		return (8 - tupleInternal.Length) switch
		{
			1 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			2 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			3 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			4 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			5 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			6 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			7 => Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1), comparer.GetHashCode(m_Item2), comparer.GetHashCode(m_Item3), comparer.GetHashCode(m_Item4), comparer.GetHashCode(m_Item5), comparer.GetHashCode(m_Item6), comparer.GetHashCode(m_Item7), tupleInternal.GetHashCode(comparer)), 
			_ => -1, 
		};
	}

	int ITupleInternal.GetHashCode(IEqualityComparer comparer)
	{
		return ((IStructuralEquatable)this).GetHashCode(comparer);
	}

	/// <summary>Returns a string that represents the value of this <see cref="T:System.Tuple`8" /> instance.</summary>
	/// <returns>The string representation of this <see cref="T:System.Tuple`8" /> object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		return ((ITupleInternal)this).ToString(stringBuilder);
	}

	string ITupleInternal.ToString(StringBuilder sb)
	{
		sb.Append(m_Item1);
		sb.Append(", ");
		sb.Append(m_Item2);
		sb.Append(", ");
		sb.Append(m_Item3);
		sb.Append(", ");
		sb.Append(m_Item4);
		sb.Append(", ");
		sb.Append(m_Item5);
		sb.Append(", ");
		sb.Append(m_Item6);
		sb.Append(", ");
		sb.Append(m_Item7);
		sb.Append(", ");
		return ((ITupleInternal)(object)m_Rest).ToString(sb);
	}
}
