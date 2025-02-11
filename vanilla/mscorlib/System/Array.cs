using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace System;

/// <summary>Provides methods for creating, manipulating, searching, and sorting arrays, thereby serving as the base class for all arrays in the common language runtime.</summary>
/// <filterpriority>1</filterpriority>
public abstract class Array : ICollection, IEnumerable, IList, IStructuralComparable, IStructuralEquatable, ICloneable
{
	private sealed class ArrayEnumerator : IEnumerator, ICloneable
	{
		private Array _array;

		private int _index;

		private int _endIndex;

		public object Current
		{
			get
			{
				if (_index < 0)
				{
					throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
				}
				if (_index >= _endIndex)
				{
					throw new InvalidOperationException("Enumeration already finished.");
				}
				if (_index == 0 && _array.GetType().GetElementType().IsPointer)
				{
					throw new NotSupportedException("Type is not supported.");
				}
				return _array.GetValueImpl(_index);
			}
		}

		internal ArrayEnumerator(Array array)
		{
			_array = array;
			_index = -1;
			_endIndex = array.Length;
		}

		public bool MoveNext()
		{
			if (_index < _endIndex)
			{
				_index++;
				return _index < _endIndex;
			}
			return false;
		}

		public void Reset()
		{
			_index = -1;
		}

		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	internal struct InternalEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private const int NOT_STARTED = -2;

		private const int FINISHED = -1;

		private readonly Array array;

		private int idx;

		public T Current
		{
			get
			{
				if (idx == -2)
				{
					throw new InvalidOperationException("Enumeration has not started. Call MoveNext");
				}
				if (idx == -1)
				{
					throw new InvalidOperationException("Enumeration already finished");
				}
				return array.InternalArray__get_Item<T>(array.Length - 1 - idx);
			}
		}

		object IEnumerator.Current => Current;

		internal InternalEnumerator(Array array)
		{
			this.array = array;
			idx = -2;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (idx == -2)
			{
				idx = array.Length;
			}
			if (idx != -1)
			{
				return --idx != -1;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			idx = -2;
		}
	}

	internal class EmptyInternalEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		public static readonly EmptyInternalEnumerator<T> Value = new EmptyInternalEnumerator<T>();

		public T Current
		{
			get
			{
				throw new InvalidOperationException("Enumeration has not started. Call MoveNext");
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		void IEnumerator.Reset()
		{
		}
	}

	internal sealed class FunctorComparer<T> : IComparer<T>
	{
		private Comparison<T> comparison;

		public FunctorComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}
	}

	private struct SorterObjectArray
	{
		private object[] keys;

		private object[] items;

		private IComparer comparer;

		internal SorterObjectArray(object[] keys, object[] items, IComparer comparer)
		{
			if (comparer == null)
			{
				comparer = Comparer.Default;
			}
			this.keys = keys;
			this.items = items;
			this.comparer = comparer;
		}

		internal void SwapIfGreaterWithItems(int a, int b)
		{
			if (a != b && comparer.Compare(keys[a], keys[b]) > 0)
			{
				object obj = keys[a];
				keys[a] = keys[b];
				keys[b] = obj;
				if (items != null)
				{
					object obj2 = items[a];
					items[a] = items[b];
					items[b] = obj2;
				}
			}
		}

		private void Swap(int i, int j)
		{
			object obj = keys[i];
			keys[i] = keys[j];
			keys[j] = obj;
			if (items != null)
			{
				object obj2 = items[i];
				items[i] = items[j];
				items[j] = obj2;
			}
		}

		internal void Sort(int left, int length)
		{
			IntrospectiveSort(left, length);
		}

		private void IntrospectiveSort(int left, int length)
		{
			if (length < 2)
			{
				return;
			}
			try
			{
				IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2(keys.Length));
			}
			catch (IndexOutOfRangeException)
			{
				IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException("Failed to compare two elements in the array.", innerException);
			}
		}

		private void IntroSort(int lo, int hi, int depthLimit)
		{
			while (hi > lo)
			{
				int num = hi - lo + 1;
				if (num <= 16)
				{
					switch (num)
					{
					case 1:
						break;
					case 2:
						SwapIfGreaterWithItems(lo, hi);
						break;
					case 3:
						SwapIfGreaterWithItems(lo, hi - 1);
						SwapIfGreaterWithItems(lo, hi);
						SwapIfGreaterWithItems(hi - 1, hi);
						break;
					default:
						InsertionSort(lo, hi);
						break;
					}
					break;
				}
				if (depthLimit == 0)
				{
					Heapsort(lo, hi);
					break;
				}
				depthLimit--;
				int num2 = PickPivotAndPartition(lo, hi);
				IntroSort(num2 + 1, hi, depthLimit);
				hi = num2 - 1;
			}
		}

		private int PickPivotAndPartition(int lo, int hi)
		{
			int num = lo + (hi - lo) / 2;
			SwapIfGreaterWithItems(lo, num);
			SwapIfGreaterWithItems(lo, hi);
			SwapIfGreaterWithItems(num, hi);
			object obj = keys[num];
			Swap(num, hi - 1);
			int num2 = lo;
			int num3 = hi - 1;
			while (num2 < num3)
			{
				while (comparer.Compare(keys[++num2], obj) < 0)
				{
				}
				while (comparer.Compare(obj, keys[--num3]) < 0)
				{
				}
				if (num2 >= num3)
				{
					break;
				}
				Swap(num2, num3);
			}
			Swap(num2, hi - 1);
			return num2;
		}

		private void Heapsort(int lo, int hi)
		{
			int num = hi - lo + 1;
			for (int num2 = num / 2; num2 >= 1; num2--)
			{
				DownHeap(num2, num, lo);
			}
			for (int num3 = num; num3 > 1; num3--)
			{
				Swap(lo, lo + num3 - 1);
				DownHeap(1, num3 - 1, lo);
			}
		}

		private void DownHeap(int i, int n, int lo)
		{
			object obj = keys[lo + i - 1];
			object obj2 = ((items != null) ? items[lo + i - 1] : null);
			while (i <= n / 2)
			{
				int num = 2 * i;
				if (num < n && comparer.Compare(keys[lo + num - 1], keys[lo + num]) < 0)
				{
					num++;
				}
				if (comparer.Compare(obj, keys[lo + num - 1]) >= 0)
				{
					break;
				}
				keys[lo + i - 1] = keys[lo + num - 1];
				if (items != null)
				{
					items[lo + i - 1] = items[lo + num - 1];
				}
				i = num;
			}
			keys[lo + i - 1] = obj;
			if (items != null)
			{
				items[lo + i - 1] = obj2;
			}
		}

		private void InsertionSort(int lo, int hi)
		{
			for (int i = lo; i < hi; i++)
			{
				int num = i;
				object obj = keys[i + 1];
				object obj2 = ((items != null) ? items[i + 1] : null);
				while (num >= lo && comparer.Compare(obj, keys[num]) < 0)
				{
					keys[num + 1] = keys[num];
					if (items != null)
					{
						items[num + 1] = items[num];
					}
					num--;
				}
				keys[num + 1] = obj;
				if (items != null)
				{
					items[num + 1] = obj2;
				}
			}
		}
	}

	private struct SorterGenericArray
	{
		private Array keys;

		private Array items;

		private IComparer comparer;

		internal SorterGenericArray(Array keys, Array items, IComparer comparer)
		{
			if (comparer == null)
			{
				comparer = Comparer.Default;
			}
			this.keys = keys;
			this.items = items;
			this.comparer = comparer;
		}

		internal void SwapIfGreaterWithItems(int a, int b)
		{
			if (a != b && comparer.Compare(keys.GetValue(a), keys.GetValue(b)) > 0)
			{
				object value = keys.GetValue(a);
				keys.SetValue(keys.GetValue(b), a);
				keys.SetValue(value, b);
				if (items != null)
				{
					object value2 = items.GetValue(a);
					items.SetValue(items.GetValue(b), a);
					items.SetValue(value2, b);
				}
			}
		}

		private void Swap(int i, int j)
		{
			object value = keys.GetValue(i);
			keys.SetValue(keys.GetValue(j), i);
			keys.SetValue(value, j);
			if (items != null)
			{
				object value2 = items.GetValue(i);
				items.SetValue(items.GetValue(j), i);
				items.SetValue(value2, j);
			}
		}

		internal void Sort(int left, int length)
		{
			IntrospectiveSort(left, length);
		}

		private void IntrospectiveSort(int left, int length)
		{
			if (length < 2)
			{
				return;
			}
			try
			{
				IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2(keys.Length));
			}
			catch (IndexOutOfRangeException)
			{
				IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
			}
			catch (Exception innerException)
			{
				throw new InvalidOperationException("Failed to compare two elements in the array.", innerException);
			}
		}

		private void IntroSort(int lo, int hi, int depthLimit)
		{
			while (hi > lo)
			{
				int num = hi - lo + 1;
				if (num <= 16)
				{
					switch (num)
					{
					case 1:
						break;
					case 2:
						SwapIfGreaterWithItems(lo, hi);
						break;
					case 3:
						SwapIfGreaterWithItems(lo, hi - 1);
						SwapIfGreaterWithItems(lo, hi);
						SwapIfGreaterWithItems(hi - 1, hi);
						break;
					default:
						InsertionSort(lo, hi);
						break;
					}
					break;
				}
				if (depthLimit == 0)
				{
					Heapsort(lo, hi);
					break;
				}
				depthLimit--;
				int num2 = PickPivotAndPartition(lo, hi);
				IntroSort(num2 + 1, hi, depthLimit);
				hi = num2 - 1;
			}
		}

		private int PickPivotAndPartition(int lo, int hi)
		{
			int num = lo + (hi - lo) / 2;
			SwapIfGreaterWithItems(lo, num);
			SwapIfGreaterWithItems(lo, hi);
			SwapIfGreaterWithItems(num, hi);
			object value = keys.GetValue(num);
			Swap(num, hi - 1);
			int num2 = lo;
			int num3 = hi - 1;
			while (num2 < num3)
			{
				while (comparer.Compare(keys.GetValue(++num2), value) < 0)
				{
				}
				while (comparer.Compare(value, keys.GetValue(--num3)) < 0)
				{
				}
				if (num2 >= num3)
				{
					break;
				}
				Swap(num2, num3);
			}
			Swap(num2, hi - 1);
			return num2;
		}

		private void Heapsort(int lo, int hi)
		{
			int num = hi - lo + 1;
			for (int num2 = num / 2; num2 >= 1; num2--)
			{
				DownHeap(num2, num, lo);
			}
			for (int num3 = num; num3 > 1; num3--)
			{
				Swap(lo, lo + num3 - 1);
				DownHeap(1, num3 - 1, lo);
			}
		}

		private void DownHeap(int i, int n, int lo)
		{
			object value = keys.GetValue(lo + i - 1);
			object value2 = ((items != null) ? items.GetValue(lo + i - 1) : null);
			while (i <= n / 2)
			{
				int num = 2 * i;
				if (num < n && comparer.Compare(keys.GetValue(lo + num - 1), keys.GetValue(lo + num)) < 0)
				{
					num++;
				}
				if (comparer.Compare(value, keys.GetValue(lo + num - 1)) >= 0)
				{
					break;
				}
				keys.SetValue(keys.GetValue(lo + num - 1), lo + i - 1);
				if (items != null)
				{
					items.SetValue(items.GetValue(lo + num - 1), lo + i - 1);
				}
				i = num;
			}
			keys.SetValue(value, lo + i - 1);
			if (items != null)
			{
				items.SetValue(value2, lo + i - 1);
			}
		}

		private void InsertionSort(int lo, int hi)
		{
			for (int i = lo; i < hi; i++)
			{
				int num = i;
				object value = keys.GetValue(i + 1);
				object value2 = ((items != null) ? items.GetValue(i + 1) : null);
				while (num >= lo && comparer.Compare(value, keys.GetValue(num)) < 0)
				{
					keys.SetValue(keys.GetValue(num), num + 1);
					if (items != null)
					{
						items.SetValue(items.GetValue(num), num + 1);
					}
					num--;
				}
				keys.SetValue(value, num + 1);
				if (items != null)
				{
					items.SetValue(value2, num + 1);
				}
			}
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="T:System.Array" />.</summary>
	/// <returns>The number of elements contained in the T:System.Collections.ICollection.</returns>
	int ICollection.Count => Length;

	bool IList.IsReadOnly => false;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	/// <param name="index">The index of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ICollection.Count" />.</exception>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
	object IList.this[int index]
	{
		get
		{
			return GetValue(index);
		}
		set
		{
			SetValue(value, index);
		}
	}

	/// <summary>Gets a 64-bit integer that represents the total number of elements in all the dimensions of the <see cref="T:System.Array" />.</summary>
	/// <returns>A 64-bit integer that represents the total number of elements in all the dimensions of the <see cref="T:System.Array" />.</returns>
	/// <filterpriority>2</filterpriority>
	public long LongLength
	{
		get
		{
			long num = GetLength(0);
			for (int i = 1; i < Rank; i++)
			{
				num *= GetLength(i);
			}
			return num;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Array" /> has a fixed size.</summary>
	/// <returns>This property is always true for all arrays.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsFixedSize => true;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Array" /> is read-only.</summary>
	/// <returns>This property is always false for all arrays.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Array" /> is synchronized (thread safe).</summary>
	/// <returns>This property is always false for all arrays.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Array" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Array" />.</returns>
	/// <filterpriority>2</filterpriority>
	public object SyncRoot => this;

	/// <summary>Gets a 32-bit integer that represents the total number of elements in all the dimensions of the <see cref="T:System.Array" />.</summary>
	/// <returns>A 32-bit integer that represents the total number of elements in all the dimensions of the <see cref="T:System.Array" />; zero if there are no elements in the array.</returns>
	/// <exception cref="T:System.OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
	/// <filterpriority>1</filterpriority>
	public int Length
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			int num = GetLength(0);
			for (int i = 1; i < Rank; i++)
			{
				num *= GetLength(i);
			}
			return num;
		}
	}

	/// <summary>Gets the rank (number of dimensions) of the <see cref="T:System.Array" />. For example, a one-dimensional array returns 1, a two-dimensional array returns 2, and so on. </summary>
	/// <returns>The rank (number of dimensions) of the <see cref="T:System.Array" />.</returns>
	/// <filterpriority>1</filterpriority>
	public int Rank
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return GetRank();
		}
	}

	/// <summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 64-bit integers.</summary>
	/// <returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="lengths">An array of 64-bit integers that represent the size of each dimension of the <see cref="T:System.Array" /> to create. Each integer in the array must be between zero and <see cref="F:System.Int32.MaxValue" />, inclusive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.-or-<paramref name="lengths" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.-or-The <paramref name="lengths" /> array contains less than one element.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, params long[] lengths)
	{
		if (lengths == null)
		{
			throw new ArgumentNullException("lengths");
		}
		if (lengths.Length == 0)
		{
			throw new ArgumentException("Must provide at least one rank.");
		}
		int[] array = new int[lengths.Length];
		for (int i = 0; i < lengths.Length; i++)
		{
			long num = lengths[i];
			if (num > int.MaxValue || num < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("len", "Arrays larger than 2GB are not supported.");
			}
			array[i] = (int)num;
		}
		return CreateInstance(elementType, array);
	}

	/// <summary>Returns a read-only wrapper for the specified array.</summary>
	/// <returns>A read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> wrapper for the specified array.</returns>
	/// <param name="array">The one-dimensional, zero-based array to wrap in a read-only <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />  wrapper. </param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return new ReadOnlyCollection<T>(array);
	}

	/// <summary>Changes the number of elements of a one-dimensional array to the specified new size.</summary>
	/// <param name="array">The one-dimensional, zero-based array to resize, or null to create a new array with the specified size.</param>
	/// <param name="newSize">The size of the new array.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="newSize" /> is less than zero.</exception>
	public static void Resize<T>(ref T[] array, int newSize)
	{
		if (newSize < 0)
		{
			throw new ArgumentOutOfRangeException("newSize", "Non-negative number required.");
		}
		T[] array2 = array;
		if (array2 == null)
		{
			array = new T[newSize];
		}
		else if (array2.Length != newSize)
		{
			T[] array3 = new T[newSize];
			Copy(array2, 0, array3, 0, (array2.Length > newSize) ? newSize : array2.Length);
			array = array3;
		}
	}

	/// <summary>Adds an item to <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</returns>
	/// <param name="value">The object to be added to the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
	int IList.Add(object value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	/// <summary>Determines whether an element is in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>true if <paramref name="value" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
	/// <param name="value">The object to locate in the current list. The element to locate can be null for reference types.</param>
	bool IList.Contains(object value)
	{
		return IndexOf(this, value) >= 0;
	}

	/// <summary>Removes all items from the <see cref="T:System.Collections.IList" />.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.</exception>
	void IList.Clear()
	{
		Clear(this, GetLowerBound(0), Length);
	}

	/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.</summary>
	/// <returns>The index of value if found in the list; otherwise, -1.</returns>
	/// <param name="value">The object to locate in the current list.</param>
	int IList.IndexOf(object value)
	{
		return IndexOf(this, value);
	}

	/// <summary>Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.</summary>
	/// <param name="index">The index at which <paramref name="value" /> should be inserted.</param>
	/// <param name="value">The object to insert.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size. </exception>
	/// <exception cref="T:System.NullReferenceException">
	///   <paramref name="value" /> is null reference in the <see cref="T:System.Collections.IList" />.</exception>
	void IList.Insert(int index, object value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.</summary>
	/// <param name="value">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size. </exception>
	void IList.Remove(object value)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	/// <summary>Removes the <see cref="T:System.Collections.IList" /> item at the specified index.</summary>
	/// <param name="index">The index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.IList" />. </exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException("Collection was of a fixed size.");
	}

	/// <summary>Copies all the elements of the current one-dimensional <see cref="T:System.Array" /> to the specified one-dimensional <see cref="T:System.Array" /> starting at the specified destination <see cref="T:System.Array" /> index. The index is specified as a 32-bit integer.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current <see cref="T:System.Array" />.</param>
	/// <param name="index">A 32-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source <see cref="T:System.Array" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">The type of the source <see cref="T:System.Array" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">The source <see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in the source <see cref="T:System.Array" /> cannot be cast to the type of destination <paramref name="array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(Array array, int index)
	{
		if (array != null && array.Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");
		}
		Copy(this, GetLowerBound(0), array, index, Length);
	}

	/// <summary>Creates a shallow copy of the <see cref="T:System.Array" />.</summary>
	/// <returns>A shallow copy of the <see cref="T:System.Array" />.</returns>
	/// <filterpriority>1</filterpriority>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the current collection object precedes, occurs in the same position as, or follows another object in the sort order.</summary>
	/// <returns>An integer that indicates the relationship of the current collection object to other, as shown in the following table.Return valueDescription-1The current instance precedes <paramref name="other" />.0The current instance and <paramref name="other" /> are equal.1The current instance follows <paramref name="other" />.</returns>
	/// <param name="other">The object to compare with the current instance.</param>
	/// <param name="comparer">An object that compares the current object and <paramref name="other" />.</param>
	int IStructuralComparable.CompareTo(object other, IComparer comparer)
	{
		if (other == null)
		{
			return 1;
		}
		if (!(other is Array array) || Length != array.Length)
		{
			throw new ArgumentException("Object is not a array with the same number of elements as the array to compare it to.", "other");
		}
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			if (num != 0)
			{
				break;
			}
			object value = GetValue(i);
			object value2 = array.GetValue(i);
			num = comparer.Compare(value, value2);
		}
		return num;
	}

	/// <summary>Determines whether an object is equal to the current instance.</summary>
	/// <returns>true if the two objects are equal; otherwise, false.</returns>
	/// <param name="other">The object to compare with the current instance.</param>
	/// <param name="comparer">An object that determines whether the current instance and <paramref name="other" /> are equal. </param>
	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (!(other is Array array) || array.Length != Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			object value = GetValue(i);
			object value2 = array.GetValue(i);
			if (!comparer.Equals(value, value2))
			{
				return false;
			}
		}
		return true;
	}

	internal static int CombineHashCodes(int h1, int h2)
	{
		return ((h1 << 5) + h1) ^ h2;
	}

	/// <summary>Returns a hash code for the current instance.</summary>
	/// <returns>The hash code for the current instance.</returns>
	/// <param name="comparer">An object that computes the hash code of the current object.</param>
	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		int num = 0;
		for (int i = ((Length >= 8) ? (Length - 8) : 0); i < Length; i++)
		{
			num = CombineHashCodes(num, comparer.GetHashCode(GetValue(i)));
		}
		return num;
	}

	/// <summary>Searches an entire one-dimensional sorted <see cref="T:System.Array" /> for a specific element, using the <see cref="T:System.IComparable" /> interface implemented by each element of the <see cref="T:System.Array" /> and by the specified object.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static int BinarySearch(Array array, object value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return BinarySearch(array, array.GetLowerBound(0), array.Length, value, null);
	}

	/// <summary>Converts an array of one type to an array of another type.</summary>
	/// <returns>An array of the target type containing the converted elements from the source array.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to convert to a target type.</param>
	/// <param name="converter">A <see cref="T:System.Converter`2" /> that converts each element from one type to another type.</param>
	/// <typeparam name="TInput">The type of the elements of the source array.</typeparam>
	/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="converter" /> is null.</exception>
	public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (converter == null)
		{
			throw new ArgumentNullException("converter");
		}
		TOutput[] array2 = new TOutput[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = converter(array[i]);
		}
		return array2;
	}

	/// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 64-bit integer.</summary>
	/// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
	/// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
	/// <param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue" />, inclusive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Copy(Array sourceArray, Array destinationArray, long length)
	{
		if (length > int.MaxValue || length < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("length", "Arrays larger than 2GB are not supported.");
		}
		Copy(sourceArray, destinationArray, (int)length);
	}

	/// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 64-bit integers.</summary>
	/// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
	/// <param name="sourceIndex">A 64-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
	/// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
	/// <param name="destinationIndex">A 64-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
	/// <param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue" />, inclusive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" /> is outside the range of valid indexes for the <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is outside the range of valid indexes for the <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
	{
		if (sourceIndex > int.MaxValue || sourceIndex < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", "Arrays larger than 2GB are not supported.");
		}
		if (destinationIndex > int.MaxValue || destinationIndex < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", "Arrays larger than 2GB are not supported.");
		}
		if (length > int.MaxValue || length < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("length", "Arrays larger than 2GB are not supported.");
		}
		Copy(sourceArray, (int)sourceIndex, destinationArray, (int)destinationIndex, (int)length);
	}

	/// <summary>Copies all the elements of the current one-dimensional <see cref="T:System.Array" /> to the specified one-dimensional <see cref="T:System.Array" /> starting at the specified destination <see cref="T:System.Array" /> index. The index is specified as a 64-bit integer.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current <see cref="T:System.Array" />.</param>
	/// <param name="index">A 64-bit integer that represents the index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-The number of elements in the source <see cref="T:System.Array" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">The type of the source <see cref="T:System.Array" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">The source <see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in the source <see cref="T:System.Array" /> cannot be cast to the type of destination <paramref name="array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(Array array, long index)
	{
		if (index > int.MaxValue || index < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
		}
		CopyTo(array, (int)index);
	}

	/// <summary>Performs the specified action on each element of the specified array.</summary>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> on whose elements the action is to be performed.</param>
	/// <param name="action">The <see cref="T:System.Action`1" /> to perform on each element of <paramref name="array" />.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="action" /> is null.</exception>
	public static void ForEach<T>(T[] array, Action<T> action)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		for (int i = 0; i < array.Length; i++)
		{
			action(array[i]);
		}
	}

	/// <summary>Gets a 64-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
	/// <returns>A 64-bit integer that represents the number of elements in the specified dimension.</returns>
	/// <param name="dimension">A zero-based dimension of the <see cref="T:System.Array" /> whose length needs to be determined.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="dimension" /> is less than zero.-or-<paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
	/// <filterpriority>2</filterpriority>
	public long GetLongLength(int dimension)
	{
		return GetLength(dimension);
	}

	/// <summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
	/// <returns>The value at the specified position in the one-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index">A 64-bit integer that represents the position of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(long index)
	{
		if (index > int.MaxValue || index < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
		}
		return GetValue((int)index);
	}

	/// <summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	/// <returns>The value at the specified position in the two-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(long index1, long index2)
	{
		if (index1 > int.MaxValue || index1 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index1", "Arrays larger than 2GB are not supported.");
		}
		if (index2 > int.MaxValue || index2 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index2", "Arrays larger than 2GB are not supported.");
		}
		return GetValue((int)index1, (int)index2);
	}

	/// <summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	/// <returns>The value at the specified position in the three-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index3">A 64-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(long index1, long index2, long index3)
	{
		if (index1 > int.MaxValue || index1 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index1", "Arrays larger than 2GB are not supported.");
		}
		if (index2 > int.MaxValue || index2 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index2", "Arrays larger than 2GB are not supported.");
		}
		if (index3 > int.MaxValue || index3 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index3", "Arrays larger than 2GB are not supported.");
		}
		return GetValue((int)index1, (int)index2, (int)index3);
	}

	/// <summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
	/// <returns>The value at the specified position in the multidimensional <see cref="T:System.Array" />.</returns>
	/// <param name="indices">A one-dimensional array of 64-bit integers that represent the indexes specifying the position of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="indices" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(params long[] indices)
	{
		if (indices == null)
		{
			throw new ArgumentNullException("indices");
		}
		if (Rank != indices.Length)
		{
			throw new ArgumentException("Indices length does not match the array rank.");
		}
		int[] array = new int[indices.Length];
		for (int i = 0; i < indices.Length; i++)
		{
			long num = indices[i];
			if (num > int.MaxValue || num < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
			}
			array[i] = (int)num;
		}
		return GetValue(array);
	}

	/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="T:System.Array" /> for a value, using the <see cref="T:System.IComparable" /> interface implemented by each element of the <see cref="T:System.Array" /> and by the specified value.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="index">The starting index of the range to search.</param>
	/// <param name="length">The length of the range to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static int BinarySearch(Array array, int index, int length, object value)
	{
		return BinarySearch(array, index, length, value, null);
	}

	/// <summary>Searches an entire one-dimensional sorted <see cref="T:System.Array" /> for a value using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static int BinarySearch(Array array, object value, IComparer comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return BinarySearch(array, array.GetLowerBound(0), array.Length, value, comparer);
	}

	/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="T:System.Array" /> for a value, using the specified <see cref="T:System.Collections.IComparer" /> interface.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="index">The starting index of the range to search.</param>
	/// <param name="length">The length of the range to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or- null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="T:System.IComparable" /> interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || length < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", "Non-negative number required.");
		}
		if (array.Length - index < length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (array.Rank != 1)
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		if (comparer == null)
		{
			comparer = LowLevelComparer.Default;
		}
		int num = index;
		int num2 = index + length - 1;
		if (array is object[] array2)
		{
			while (num <= num2)
			{
				int median = GetMedian(num, num2);
				int num3;
				try
				{
					num3 = comparer.Compare(array2[median], value);
				}
				catch (Exception innerException)
				{
					throw new InvalidOperationException("Failed to compare two elements in the array.", innerException);
				}
				if (num3 == 0)
				{
					return median;
				}
				if (num3 < 0)
				{
					num = median + 1;
				}
				else
				{
					num2 = median - 1;
				}
			}
		}
		else
		{
			while (num <= num2)
			{
				int median2 = GetMedian(num, num2);
				int num4;
				try
				{
					num4 = comparer.Compare(array.GetValue(median2), value);
				}
				catch (Exception innerException2)
				{
					throw new InvalidOperationException("Failed to compare two elements in the array.", innerException2);
				}
				if (num4 == 0)
				{
					return median2;
				}
				if (num4 < 0)
				{
					num = median2 + 1;
				}
				else
				{
					num2 = median2 - 1;
				}
			}
		}
		return ~num;
	}

	private static int GetMedian(int low, int hi)
	{
		return low + (hi - low >> 1);
	}

	/// <summary>Searches an entire one-dimensional sorted <see cref="T:System.Array" /> for a specific element, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified object.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search. </param>
	/// <param name="value">The object to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static int BinarySearch<T>(T[] array, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return BinarySearch(array, 0, array.Length, value, null);
	}

	/// <summary>Searches an entire one-dimensional sorted <see cref="T:System.Array" /> for a value using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search.  </param>
	/// <param name="value">The object to search for.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.-or- null to use the <see cref="T:System.IComparable`1" /> implementation of each element.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static int BinarySearch<T>(T[] array, T value, IComparer<T> comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return BinarySearch(array, 0, array.Length, value, comparer);
	}

	/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="T:System.Array" /> for a value, using the <see cref="T:System.IComparable`1" /> generic interface implemented by each element of the <see cref="T:System.Array" /> and by the specified value.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search. </param>
	/// <param name="index">The starting index of the range to search.</param>
	/// <param name="length">The length of the range to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="value" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static int BinarySearch<T>(T[] array, int index, int length, T value)
	{
		return BinarySearch(array, index, length, value, null);
	}

	/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="T:System.Array" /> for a value, using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
	/// <param name="array">The sorted one-dimensional, zero-based <see cref="T:System.Array" /> to search. </param>
	/// <param name="index">The starting index of the range to search.</param>
	/// <param name="length">The length of the range to search.</param>
	/// <param name="value">The object to search for.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing elements.-or- null to use the <see cref="T:System.IComparable`1" /> implementation of each element.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="T:System.IComparable`1" /> generic interface, and the search encounters an element that does not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T> comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || length < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", "Non-negative number required.");
		}
		if (array.Length - index < length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		return ArraySortHelper<T>.BinarySearch(array, index, length, value, comparer);
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
	/// <returns>The index of the first occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int IndexOf(Array array, object value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return IndexOf(array, value, array.GetLowerBound(0), array.Length);
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	/// <returns>The index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from <paramref name="startIndex" /> to the last element, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int IndexOf(Array array, object value, int startIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int lowerBound = array.GetLowerBound(0);
		return IndexOf(array, value, startIndex, array.Length - startIndex + lowerBound);
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified in <paramref name="count" />, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The starting index of the search. 0 (zero) is valid in an empty array.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int IndexOf(Array array, object value, int startIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		int lowerBound = array.GetLowerBound(0);
		if (startIndex < lowerBound || startIndex > array.Length + lowerBound)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || count > array.Length - startIndex + lowerBound)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		object[] array2 = array as object[];
		int num = startIndex + count;
		if (array2 != null)
		{
			if (value == null)
			{
				for (int i = startIndex; i < num; i++)
				{
					if (array2[i] == null)
					{
						return i;
					}
				}
			}
			else
			{
				for (int j = startIndex; j < num; j++)
				{
					object obj = array2[j];
					if (obj != null && obj.Equals(value))
					{
						return j;
					}
				}
			}
		}
		else
		{
			for (int k = startIndex; k < num; k++)
			{
				object value2 = array.GetValue(k);
				if (value2 == null)
				{
					if (value == null)
					{
						return k;
					}
				}
				else if (value2.Equals(value))
				{
					return k;
				}
			}
		}
		return lowerBound - 1;
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	public static int IndexOf<T>(T[] array, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return IndexOfImpl(array, value, 0, array.Length);
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from <paramref name="startIndex" /> to the last element, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	public static int IndexOf<T>(T[] array, T value, int startIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return IndexOf(array, value, startIndex, array.Length - startIndex);
	}

	/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified in <paramref name="count" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (startIndex < 0 || startIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || count > array.Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		return IndexOfImpl(array, value, startIndex, count);
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the entire one-dimensional <see cref="T:System.Array" />.</summary>
	/// <returns>The index of the last occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int LastIndexOf(Array array, object value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return LastIndexOf(array, value, array.Length - 1, array.Length);
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	/// <returns>The index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The starting index of the backward search.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int LastIndexOf(Array array, object value, int startIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return LastIndexOf(array, value, startIndex, startIndex + 1);
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the one-dimensional <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that contains the number of elements specified in <paramref name="count" /> and ends at <paramref name="startIndex" />, if found; otherwise, the lower bound of the array minus 1.</returns>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <filterpriority>1</filterpriority>
	public static int LastIndexOf(Array array, object value, int startIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Length == 0)
		{
			return -1;
		}
		if (startIndex < 0 || startIndex >= array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		if (count > startIndex + 1)
		{
			throw new ArgumentOutOfRangeException("endIndex", "endIndex cannot be greater than startIndex.");
		}
		if (array.Rank != 1)
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		object[] array2 = array as object[];
		int num = startIndex - count + 1;
		if (array2 != null)
		{
			if (value == null)
			{
				for (int num2 = startIndex; num2 >= num; num2--)
				{
					if (array2[num2] == null)
					{
						return num2;
					}
				}
			}
			else
			{
				for (int num3 = startIndex; num3 >= num; num3--)
				{
					object obj = array2[num3];
					if (obj != null && obj.Equals(value))
					{
						return num3;
					}
				}
			}
		}
		else
		{
			for (int num4 = startIndex; num4 >= num; num4--)
			{
				object value2 = array.GetValue(num4);
				if (value2 == null)
				{
					if (value == null)
					{
						return num4;
					}
				}
				else if (value2.Equals(value))
				{
					return num4;
				}
			}
		}
		return -1;
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	public static int LastIndexOf<T>(T[] array, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return LastIndexOf(array, value, array.Length - 1, array.Length);
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	public static int LastIndexOf<T>(T[] array, T value, int startIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return LastIndexOf(array, value, startIndex, (array.Length != 0) ? (startIndex + 1) : 0);
	}

	/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that contains the number of elements specified in <paramref name="count" /> and ends at <paramref name="startIndex" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="value">The object to locate in <paramref name="array" />.</param>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Length == 0)
		{
			if (startIndex != -1 && startIndex != 0)
			{
				throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			if (count != 0)
			{
				throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
			}
			return -1;
		}
		if (startIndex < 0 || startIndex >= array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		return LastIndexOfImpl(array, value, startIndex, count);
	}

	/// <summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to reverse.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Reverse(Array array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Reverse(array, array.GetLowerBound(0), array.Length);
	}

	/// <summary>Reverses the sequence of the elements in a range of elements in the one-dimensional <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to reverse.</param>
	/// <param name="index">The starting index of the section to reverse.</param>
	/// <param name="length">The number of elements in the section to reverse.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Reverse(Array array, int index, int length)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int lowerBound = array.GetLowerBound(0);
		if (index < lowerBound || length < 0)
		{
			throw new ArgumentOutOfRangeException((index < lowerBound) ? "index" : "length", "Non-negative number required.");
		}
		if (array.Length - (index - lowerBound) < length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (array.Rank != 1)
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		int num = index;
		int num2 = index + length - 1;
		if (array is object[] array2)
		{
			while (num < num2)
			{
				object obj = array2[num];
				array2[num] = array2[num2];
				array2[num2] = obj;
				num++;
				num2--;
			}
		}
		else
		{
			while (num < num2)
			{
				object value = array.GetValue(num);
				array.SetValue(array.GetValue(num2), num);
				array.SetValue(value, num2);
				num++;
				num2--;
			}
		}
	}

	public static void Reverse<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Reverse(array, 0, array.Length);
	}

	public static void Reverse<T>(T[] array, int index, int length)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || length < 0)
		{
			throw new ArgumentOutOfRangeException((index < 0) ? "index" : "length", "Non-negative number required.");
		}
		if (array.Length - index < length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		int num = index;
		int num2 = index + length - 1;
		while (num < num2)
		{
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
			num++;
			num2--;
		}
	}

	/// <summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 64-bit integer.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index">A 64-bit integer that represents the position of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, long index)
	{
		if (index > int.MaxValue || index < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
		}
		SetValue(value, (int)index);
	}

	/// <summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, long index1, long index2)
	{
		if (index1 > int.MaxValue || index1 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index1", "Arrays larger than 2GB are not supported.");
		}
		if (index2 > int.MaxValue || index2 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index2", "Arrays larger than 2GB are not supported.");
		}
		SetValue(value, (int)index1, (int)index2);
	}

	/// <summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 64-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index1">A 64-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index2">A 64-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index3">A 64-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, long index1, long index2, long index3)
	{
		if (index1 > int.MaxValue || index1 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index1", "Arrays larger than 2GB are not supported.");
		}
		if (index2 > int.MaxValue || index2 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index2", "Arrays larger than 2GB are not supported.");
		}
		if (index3 > int.MaxValue || index3 < int.MinValue)
		{
			throw new ArgumentOutOfRangeException("index3", "Arrays larger than 2GB are not supported.");
		}
		SetValue(value, (int)index1, (int)index2, (int)index3);
	}

	/// <summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 64-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="indices">A one-dimensional array of 64-bit integers that represent the indexes specifying the position of the element to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="indices" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, params long[] indices)
	{
		if (indices == null)
		{
			throw new ArgumentNullException("indices");
		}
		if (Rank != indices.Length)
		{
			throw new ArgumentException("Indices length does not match the array rank.");
		}
		int[] array = new int[indices.Length];
		for (int i = 0; i < indices.Length; i++)
		{
			long num = indices[i];
			if (num > int.MaxValue || num < int.MinValue)
			{
				throw new ArgumentOutOfRangeException("index", "Arrays larger than 2GB are not supported.");
			}
			array[i] = (int)num;
		}
		SetValue(value, array);
	}

	/// <summary>Sorts the elements in an entire one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Sort(array, null, array.GetLowerBound(0), array.Length, null);
	}

	/// <summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each element of the <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array array, int index, int length)
	{
		Sort(array, null, index, length, null);
	}

	/// <summary>Sorts the elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or-null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <exception cref="T:System.ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array array, IComparer comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Sort(array, null, array.GetLowerBound(0), array.Length, comparer);
	}

	/// <summary>Sorts the elements in a range of elements in a one-dimensional <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> to sort.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or-null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array array, int index, int length, IComparer comparer)
	{
		Sort(array, null, index, length, comparer);
	}

	/// <summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	/// <param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
	/// <param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />.-or-null to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional.-or-The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array keys, Array items)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
	}

	/// <summary>Sorts a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
	/// <param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />.-or-null to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or-null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional.-or-The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array keys, Array items, IComparer comparer)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
	}

	/// <summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable" /> implementation of each key.</summary>
	/// <param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
	/// <param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />.-or-null to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional.-or-The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="keys" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.-or-<paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />.-or-<paramref name="items" /> is not null, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array keys, Array items, int index, int length)
	{
		Sort(keys, items, index, length, null);
	}

	/// <summary>Sorts a range of elements in a pair of one-dimensional <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.IComparer" />.</summary>
	/// <param name="keys">The one-dimensional <see cref="T:System.Array" /> that contains the keys to sort.</param>
	/// <param name="items">The one-dimensional <see cref="T:System.Array" /> that contains the items that correspond to each of the keys in the <paramref name="keys" /><see cref="T:System.Array" />.-or-null to sort only the <paramref name="keys" /><see cref="T:System.Array" />.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.IComparer" /> implementation to use when comparing elements.-or-null to use the <see cref="T:System.IComparable" /> implementation of each element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.RankException">The <paramref name="keys" /><see cref="T:System.Array" /> is multidimensional.-or-The <paramref name="items" /><see cref="T:System.Array" /> is multidimensional.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="keys" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />.-or-<paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.-or-<paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />.-or-<paramref name="items" /> is not null, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public static void Sort(Array keys, Array items, int index, int length, IComparer comparer)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		if (keys.Rank != 1 || (items != null && items.Rank != 1))
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		int lowerBound = keys.GetLowerBound(0);
		if (items != null && lowerBound != items.GetLowerBound(0))
		{
			throw new ArgumentException("The arrays' lower bounds must be identical.");
		}
		if (index < lowerBound || length < 0)
		{
			throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", "Non-negative number required.");
		}
		if (keys.Length - (index - lowerBound) < length || (items != null && index - lowerBound > items.Length - length))
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (length > 1)
		{
			SortImpl(keys, items, index, length, comparer);
		}
	}

	/// <summary>Sorts the elements in an entire <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<T>(T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Sort(array, 0, array.Length, null);
	}

	/// <summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each element of the <see cref="T:System.Array" />.</summary>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<T>(T[] array, int index, int length)
	{
		Sort(array, index, length, null);
	}

	/// <summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <param name="array">The one-dimensional, zero-base <see cref="T:System.Array" /> to sort</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or null to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	/// <exception cref="T:System.ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	public static void Sort<T>(T[] array, IComparer<T> comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Sort(array, 0, array.Length, comparer);
	}

	/// <summary>Sorts the elements in a range of elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or null to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || length < 0)
		{
			throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", "Non-negative number required.");
		}
		if (array.Length - index < length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (length > 1)
		{
			ArraySortHelper<T>.Sort(array, index, length, comparer);
		}
	}

	/// <summary>Sorts the elements in an <see cref="T:System.Array" /> using the specified <see cref="T:System.Comparison`1" />.</summary>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to sort</param>
	/// <param name="comparison">The <see cref="T:System.Comparison`1" /> to use when comparing elements.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="comparison" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The implementation of <paramref name="comparison" /> caused an error during the sort. For example, <paramref name="comparison" /> might not return 0 when comparing an item with itself.</exception>
	public static void Sort<T>(T[] array, Comparison<T> comparison)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		ArraySortHelper<T>.Sort(array, 0, array.Length, comparison);
	}

	/// <summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	/// <param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort. </param>
	/// <param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or null to sort only <paramref name="keys" />.</param>
	/// <typeparam name="TKey">The type of the elements of the key array.</typeparam>
	/// <typeparam name="TValue">The type of the elements of the items array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />.-or-<paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		Sort(keys, items, 0, keys.Length, null);
	}

	/// <summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the <see cref="T:System.IComparable`1" /> generic interface implementation of each key.</summary>
	/// <param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort. </param>
	/// <param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or null to sort only <paramref name="keys" />.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <typeparam name="TKey">The type of the elements of the key array.</typeparam>
	/// <typeparam name="TValue">The type of the elements of the items array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="keys" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />.-or-<paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.-or-<paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />.-or-<paramref name="items" /> is not null, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">One or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
	{
		Sort(keys, items, index, length, null);
	}

	/// <summary>Sorts a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort. </param>
	/// <param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or null to sort only <paramref name="keys" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or null to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
	/// <typeparam name="TKey">The type of the elements of the key array.</typeparam>
	/// <typeparam name="TValue">The type of the elements of the items array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />.-or-<paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.-or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		Sort(keys, items, 0, keys.Length, comparer);
	}

	/// <summary>Sorts a range of elements in a pair of <see cref="T:System.Array" /> objects (one contains the keys and the other contains the corresponding items) based on the keys in the first <see cref="T:System.Array" /> using the specified <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface.</summary>
	/// <param name="keys">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the keys to sort. </param>
	/// <param name="items">The one-dimensional, zero-based <see cref="T:System.Array" /> that contains the items that correspond to the keys in <paramref name="keys" />, or null to sort only <paramref name="keys" />.</param>
	/// <param name="index">The starting index of the range to sort.</param>
	/// <param name="length">The number of elements in the range to sort.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> generic interface implementation to use when comparing elements, or null to use the <see cref="T:System.IComparable`1" /> generic interface implementation of each element.</param>
	/// <typeparam name="TKey">The type of the elements of the key array.</typeparam>
	/// <typeparam name="TValue">The type of the elements of the items array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keys" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="keys" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="items" /> is not null, and the lower bound of <paramref name="keys" /> does not match the lower bound of <paramref name="items" />.-or-<paramref name="items" /> is not null, and the length of <paramref name="keys" /> is greater than the length of <paramref name="items" />.-or-<paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="keys" /><see cref="T:System.Array" />.-or-<paramref name="items" /> is not null, and <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in the <paramref name="items" /><see cref="T:System.Array" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="comparer" /> is null, and one or more elements in the <paramref name="keys" /><see cref="T:System.Array" /> do not implement the <see cref="T:System.IComparable`1" /> generic interface.</exception>
	public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		if (index < 0 || length < 0)
		{
			throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index", "Non-negative number required.");
		}
		if (keys.Length - index < length || (items != null && index > items.Length - length))
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		if (length > 1)
		{
			if (items == null)
			{
				Sort(keys, index, length, comparer);
			}
			else
			{
				ArraySortHelper<TKey, TValue>.Default.Sort(keys, items, index, length, comparer);
			}
		}
	}

	/// <summary>Determines whether the specified array contains elements that match the conditions defined by the specified predicate.</summary>
	/// <returns>true if <paramref name="array" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the elements to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static bool Exists<T>(T[] array, Predicate<T> match)
	{
		return FindIndex(array, match) != -1;
	}

	public static void Fill<T>(T[] array, T value)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static void Fill<T>(T[] array, T value, int startIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (startIndex < 0 || startIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || startIndex > array.Length - count)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			array[i] = value;
		}
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static T Find<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (match(array[i]))
			{
				return array[i];
			}
		}
		return default(T);
	}

	/// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
	/// <returns>An <see cref="T:System.Array" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="T:System.Array" />.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the elements to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static T[] FindAll<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		int num = 0;
		T[] array2 = Empty<T>();
		for (int i = 0; i < array.Length; i++)
		{
			if (match(array[i]))
			{
				if (num == array2.Length)
				{
					Resize(ref array2, Math.Min((num == 0) ? 4 : (num * 2), array.Length));
				}
				array2[num++] = array[i];
			}
		}
		if (num != array2.Length)
		{
			Resize(ref array2, num);
		}
		return array2;
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static int FindIndex<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return FindIndex(array, 0, array.Length, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the specified index to the last element.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return FindIndex(array, startIndex, array.Length - startIndex, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="T:System.Array" /> that starts at the specified index and contains the specified number of elements.</summary>
	/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="startIndex">The zero-based starting index of the search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (startIndex < 0 || startIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || startIndex > array.Length - count)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(array[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static T FindLast<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (match(array[num]))
			{
				return array[num];
			}
		}
		return default(T);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="T:System.Array" />.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static int FindLastIndex<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return FindLastIndex(array, array.Length - 1, array.Length, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that extends from the first element to the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
	public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		return FindLastIndex(array, startIndex, startIndex + 1, match);
	}

	/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="T:System.Array" /> that contains the specified number of elements and ends at the specified index.</summary>
	/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to search.</param>
	/// <param name="startIndex">The zero-based starting index of the backward search.</param>
	/// <param name="count">The number of elements in the section to search.</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions of the element to search for.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
	public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		if (array.Length == 0)
		{
			if (startIndex != -1)
			{
				throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
		}
		else if (startIndex < 0 || startIndex >= array.Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		int num = startIndex - count;
		for (int num2 = startIndex; num2 > num; num2--)
		{
			if (match(array[num2]))
			{
				return num2;
			}
		}
		return -1;
	}

	/// <summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
	/// <returns>true if every element in <paramref name="array" /> matches the conditions defined by the specified predicate; otherwise, false. If there are no elements in the array, the return value is true.</returns>
	/// <param name="array">The one-dimensional, zero-based <see cref="T:System.Array" /> to check against the conditions</param>
	/// <param name="match">The <see cref="T:System.Predicate`1" /> that defines the conditions to check against the elements.</param>
	/// <typeparam name="T">The type of the elements of the array.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
	public static bool TrueForAll<T>(T[] array, Predicate<T> match)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (match == null)
		{
			throw new ArgumentNullException("match");
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!match(array[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> for the <see cref="T:System.Array" />.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator GetEnumerator()
	{
		return new ArrayEnumerator(this);
	}

	private Array()
	{
	}

	internal int InternalArray__ICollection_get_Count()
	{
		return Length;
	}

	internal bool InternalArray__ICollection_get_IsReadOnly()
	{
		return true;
	}

	internal IEnumerator<T> InternalArray__IEnumerable_GetEnumerator<T>()
	{
		if (Length == 0)
		{
			return EmptyInternalEnumerator<T>.Value;
		}
		return new InternalEnumerator<T>(this);
	}

	internal void InternalArray__ICollection_Clear()
	{
		throw new NotSupportedException("Collection is read-only");
	}

	internal void InternalArray__ICollection_Add<T>(T item)
	{
		throw new NotSupportedException("Collection is of a fixed size");
	}

	internal bool InternalArray__ICollection_Remove<T>(T item)
	{
		throw new NotSupportedException("Collection is of a fixed size");
	}

	internal bool InternalArray__ICollection_Contains<T>(T item)
	{
		if (Rank > 1)
		{
			throw new RankException(Locale.GetText("Only single dimension arrays are supported."));
		}
		int length = Length;
		for (int i = 0; i < length; i++)
		{
			GetGenericValueImpl<T>(i, out var value);
			if (item == null)
			{
				if (value == null)
				{
					return true;
				}
			}
			else if (item.Equals(value))
			{
				return true;
			}
		}
		return false;
	}

	internal void InternalArray__ICollection_CopyTo<T>(T[] array, int arrayIndex)
	{
		Copy(this, GetLowerBound(0), array, arrayIndex, Length);
	}

	internal T InternalArray__IReadOnlyList_get_Item<T>(int index)
	{
		if ((uint)index >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		GetGenericValueImpl<T>(index, out var value);
		return value;
	}

	internal int InternalArray__IReadOnlyCollection_get_Count()
	{
		return Length;
	}

	internal void InternalArray__Insert<T>(int index, T item)
	{
		throw new NotSupportedException("Collection is of a fixed size");
	}

	internal void InternalArray__RemoveAt(int index)
	{
		throw new NotSupportedException("Collection is of a fixed size");
	}

	internal int InternalArray__IndexOf<T>(T item)
	{
		if (Rank > 1)
		{
			throw new RankException(Locale.GetText("Only single dimension arrays are supported."));
		}
		int length = Length;
		for (int i = 0; i < length; i++)
		{
			GetGenericValueImpl<T>(i, out var value);
			if (item == null)
			{
				if (value == null)
				{
					return i + GetLowerBound(0);
				}
			}
			else if (value.Equals(item))
			{
				return i + GetLowerBound(0);
			}
		}
		return GetLowerBound(0) - 1;
	}

	internal T InternalArray__get_Item<T>(int index)
	{
		if ((uint)index >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		GetGenericValueImpl<T>(index, out var value);
		return value;
	}

	internal void InternalArray__set_Item<T>(int index, T item)
	{
		if ((uint)index >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (this is object[] array)
		{
			array[index] = item;
		}
		else
		{
			SetGenericValueImpl(index, ref item);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void GetGenericValueImpl<T>(int pos, out T value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void SetGenericValueImpl<T>(int pos, ref T value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int GetRank();

	/// <summary>Gets a 32-bit integer that represents the number of elements in the specified dimension of the <see cref="T:System.Array" />.</summary>
	/// <returns>A 32-bit integer that represents the number of elements in the specified dimension.</returns>
	/// <param name="dimension">A zero-based dimension of the <see cref="T:System.Array" /> whose length needs to be determined.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="dimension" /> is less than zero.-or-<paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetLength(int dimension);

	/// <summary>Gets the index of the first element of the specified dimension in the array.</summary>
	/// <returns>The index of the first element of the specified dimension in the array.</returns>
	/// <param name="dimension">A zero-based dimension of the array whose starting index needs to be determined.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="dimension" /> is less than zero.-or-<paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public extern int GetLowerBound(int dimension);

	/// <summary>Gets the value at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
	/// <returns>The value at the specified position in the multidimensional <see cref="T:System.Array" />.</returns>
	/// <param name="indices">A one-dimensional array of 32-bit integers that represent the indexes specifying the position of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="indices" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern object GetValue(params int[] indices);

	/// <summary>Sets a value to the element at the specified position in the multidimensional <see cref="T:System.Array" />. The indexes are specified as an array of 32-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="indices">A one-dimensional array of 32-bit integers that represent the indexes specifying the position of the element to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="indices" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The number of dimensions in the current <see cref="T:System.Array" /> is not equal to the number of elements in <paramref name="indices" />.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Any element in <paramref name="indices" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetValue(object value, params int[] indices);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern object GetValueImpl(int pos);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void SetValueImpl(object value, int pos);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool FastCopy(Array source, int source_idx, Array dest, int dest_idx, int length);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern Array CreateInstanceImpl(Type elementType, int[] lengths, int[] bounds);

	/// <summary>Gets the index of the last element of the specified dimension in the array.</summary>
	/// <returns>The index of the last element of the specified dimension in the array, or -1 if the specified dimension is empty. </returns>
	/// <param name="dimension">A zero-based dimension of the array whose upper bound needs to be determined.</param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="dimension" /> is less than zero.-or-<paramref name="dimension" /> is equal to or greater than <see cref="P:System.Array.Rank" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public int GetUpperBound(int dimension)
	{
		return GetLowerBound(dimension) + GetLength(dimension) - 1;
	}

	/// <summary>Gets the value at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
	/// <returns>The value at the specified position in the one-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index">A 32-bit integer that represents the position of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(int index)
	{
		if (Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");
		}
		int lowerBound = GetLowerBound(0);
		if (index < lowerBound || index > GetUpperBound(0))
		{
			throw new IndexOutOfRangeException(Locale.GetText("Index has to be between upper and lower bound of the array."));
		}
		if (GetType().GetElementType().IsPointer)
		{
			throw new NotSupportedException("Type is not supported.");
		}
		return GetValueImpl(index - lowerBound);
	}

	/// <summary>Gets the value at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	/// <returns>The value at the specified position in the two-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(int index1, int index2)
	{
		int[] indices = new int[2] { index1, index2 };
		return GetValue(indices);
	}

	/// <summary>Gets the value at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	/// <returns>The value at the specified position in the three-dimensional <see cref="T:System.Array" />.</returns>
	/// <param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <param name="index3">A 32-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to get.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>2</filterpriority>
	public object GetValue(int index1, int index2, int index3)
	{
		int[] indices = new int[3] { index1, index2, index3 };
		return GetValue(indices);
	}

	/// <summary>Sets a value to the element at the specified position in the one-dimensional <see cref="T:System.Array" />. The index is specified as a 32-bit integer.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index">A 32-bit integer that represents the position of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly one dimension.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is outside the range of valid indexes for the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, int index)
	{
		if (Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.");
		}
		int lowerBound = GetLowerBound(0);
		if (index < lowerBound || index > GetUpperBound(0))
		{
			throw new IndexOutOfRangeException(Locale.GetText("Index has to be >= lower bound and <= upper bound of the array."));
		}
		if (GetType().GetElementType().IsPointer)
		{
			throw new NotSupportedException("Type is not supported.");
		}
		SetValueImpl(value, index - lowerBound);
	}

	/// <summary>Sets a value to the element at the specified position in the two-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly two dimensions.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">Either <paramref name="index1" /> or <paramref name="index2" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, int index1, int index2)
	{
		int[] indices = new int[2] { index1, index2 };
		SetValue(value, indices);
	}

	/// <summary>Sets a value to the element at the specified position in the three-dimensional <see cref="T:System.Array" />. The indexes are specified as 32-bit integers.</summary>
	/// <param name="value">The new value for the specified element.</param>
	/// <param name="index1">A 32-bit integer that represents the first-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index2">A 32-bit integer that represents the second-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <param name="index3">A 32-bit integer that represents the third-dimension index of the <see cref="T:System.Array" /> element to set.</param>
	/// <exception cref="T:System.ArgumentException">The current <see cref="T:System.Array" /> does not have exactly three dimensions.</exception>
	/// <exception cref="T:System.InvalidCastException">
	///   <paramref name="value" /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index1" /> or <paramref name="index2" /> or <paramref name="index3" /> is outside the range of valid indexes for the corresponding dimension of the current <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetValue(object value, int index1, int index2, int index3)
	{
		int[] indices = new int[3] { index1, index2, index3 };
		SetValue(value, indices);
	}

	internal static Array UnsafeCreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
	{
		return CreateInstance(elementType, lengths, lowerBounds);
	}

	internal static Array UnsafeCreateInstance(Type elementType, int length1, int length2)
	{
		return CreateInstance(elementType, length1, length2);
	}

	internal static Array UnsafeCreateInstance(Type elementType, params int[] lengths)
	{
		return CreateInstance(elementType, lengths);
	}

	/// <summary>Creates a one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and length, with zero-based indexing.</summary>
	/// <returns>A new one-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length, using zero-based indexing.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length">The size of the <see cref="T:System.Array" /> to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported.-or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, int length)
	{
		int[] lengths = new int[1] { length };
		return CreateInstance(elementType, lengths);
	}

	/// <summary>Creates a two-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
	/// <returns>A new two-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length1">The size of the first dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length2">The size of the second dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length1" /> is less than zero.-or-<paramref name="length2" /> is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, int length1, int length2)
	{
		int[] lengths = new int[2] { length1, length2 };
		return CreateInstance(elementType, lengths);
	}

	/// <summary>Creates a three-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing.</summary>
	/// <returns>A new three-dimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length1">The size of the first dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length2">The size of the second dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="length3">The size of the third dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length1" /> is less than zero.-or-<paramref name="length2" /> is less than zero.-or-<paramref name="length3" /> is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, int length1, int length2, int length3)
	{
		int[] lengths = new int[3] { length1, length2, length3 };
		return CreateInstance(elementType, lengths);
	}

	/// <summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with zero-based indexing. The dimension lengths are specified in an array of 32-bit integers.</summary>
	/// <returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length for each dimension, using zero-based indexing.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="lengths">An array of 32-bit integers that represent the size of each dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.-or-<paramref name="lengths" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.-or-The <paramref name="lengths" /> array contains less than one element.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, params int[] lengths)
	{
		if (elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		if (lengths == null)
		{
			throw new ArgumentNullException("lengths");
		}
		if (lengths.Length > 255)
		{
			throw new TypeLoadException();
		}
		int[] bounds = null;
		elementType = elementType.UnderlyingSystemType as RuntimeType;
		if (elementType == null)
		{
			throw new ArgumentException("Type must be a type provided by the runtime.", "elementType");
		}
		if (elementType.Equals(typeof(void)))
		{
			throw new NotSupportedException("Array type can not be void");
		}
		if (elementType.ContainsGenericParameters)
		{
			throw new NotSupportedException("Array type can not be an open generic type");
		}
		return CreateInstanceImpl(elementType, lengths, bounds);
	}

	/// <summary>Creates a multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> and dimension lengths, with the specified lower bounds.</summary>
	/// <returns>A new multidimensional <see cref="T:System.Array" /> of the specified <see cref="T:System.Type" /> with the specified length and lower bound for each dimension.</returns>
	/// <param name="elementType">The <see cref="T:System.Type" /> of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="lengths">A one-dimensional array that contains the size of each dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <param name="lowerBounds">A one-dimensional array that contains the lower bound (starting index) of each dimension of the <see cref="T:System.Array" /> to create.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elementType" /> is null.-or-<paramref name="lengths" /> is null.-or-<paramref name="lowerBounds" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="elementType" /> is not a valid <see cref="T:System.Type" />.-or-The <paramref name="lengths" /> array contains less than one element.-or-The <paramref name="lengths" /> and <paramref name="lowerBounds" /> arrays do not contain the same number of elements.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="elementType" /> is not supported. For example, <see cref="T:System.Void" /> is not supported. -or-<paramref name="elementType" /> is an open generic type.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Any value in <paramref name="lengths" /> is less than zero.-or-Any value in <paramref name="lowerBounds" /> is very large, such that the sum of a dimension's lower bound and length is greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	/// <filterpriority>1</filterpriority>
	public static Array CreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
	{
		if (elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		if (lengths == null)
		{
			throw new ArgumentNullException("lengths");
		}
		if (lowerBounds == null)
		{
			throw new ArgumentNullException("lowerBounds");
		}
		elementType = elementType.UnderlyingSystemType as RuntimeType;
		if (elementType == null)
		{
			throw new ArgumentException("Type must be a type provided by the runtime.", "elementType");
		}
		if (elementType.Equals(typeof(void)))
		{
			throw new NotSupportedException("Array type can not be void");
		}
		if (elementType.ContainsGenericParameters)
		{
			throw new NotSupportedException("Array type can not be an open generic type");
		}
		if (lengths.Length < 1)
		{
			throw new ArgumentException(Locale.GetText("Arrays must contain >= 1 elements."));
		}
		if (lengths.Length != lowerBounds.Length)
		{
			throw new ArgumentException(Locale.GetText("Arrays must be of same size."));
		}
		for (int i = 0; i < lowerBounds.Length; i++)
		{
			if (lengths[i] < 0)
			{
				throw new ArgumentOutOfRangeException("lengths", Locale.GetText("Each value has to be >= 0."));
			}
			if ((long)lowerBounds[i] + (long)lengths[i] > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("lengths", Locale.GetText("Length + bound must not exceed Int32.MaxValue."));
			}
		}
		if (lengths.Length > 255)
		{
			throw new TypeLoadException();
		}
		return CreateInstanceImpl(elementType, lengths, lowerBounds);
	}

	/// <summary>Sets a range of elements in the <see cref="T:System.Array" /> to zero, to false, or to null, depending on the element type.</summary>
	/// <param name="array">The <see cref="T:System.Array" /> whose elements need to be cleared.</param>
	/// <param name="index">The starting index of the range of elements to clear.</param>
	/// <param name="length">The number of elements to clear.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.-or-The sum of <paramref name="index" /> and <paramref name="length" /> is greater than the size of the <see cref="T:System.Array" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void Clear(Array array, int index, int length)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (length < 0)
		{
			throw new IndexOutOfRangeException("length < 0");
		}
		int lowerBound = array.GetLowerBound(0);
		if (index < lowerBound)
		{
			throw new IndexOutOfRangeException("index < lower bound");
		}
		index -= lowerBound;
		if (index > array.Length - length)
		{
			throw new IndexOutOfRangeException("index + length > size");
		}
		ClearInternal(array, index, length);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ClearInternal(Array a, int index, int count);

	/// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
	/// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
	/// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
	/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public static void Copy(Array sourceArray, Array destinationArray, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length);
	}

	/// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 32-bit integers.</summary>
	/// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
	/// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
	/// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
	/// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
	/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
	public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", Locale.GetText("Value has to be >= 0."));
		}
		if (sourceArray.Rank != destinationArray.Rank)
		{
			throw new RankException("Only single dimension arrays are supported here.");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", Locale.GetText("Value has to be >= 0."));
		}
		if (destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", Locale.GetText("Value has to be >= 0."));
		}
		if (FastCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length))
		{
			return;
		}
		int num = sourceIndex - sourceArray.GetLowerBound(0);
		int num2 = destinationIndex - destinationArray.GetLowerBound(0);
		if (num2 < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", "Index was less than the array's lower bound in the first dimension.");
		}
		if (num > sourceArray.Length - length)
		{
			throw new ArgumentException("length");
		}
		if (num2 > destinationArray.Length - length)
		{
			throw new ArgumentException("Destination array was not long enough. Check destIndex and length, and the array's lower bounds", string.Empty);
		}
		Type elementType = sourceArray.GetType().GetElementType();
		Type elementType2 = destinationArray.GetType().GetElementType();
		if (sourceArray != destinationArray || num > num2)
		{
			for (int i = 0; i < length; i++)
			{
				object valueImpl = sourceArray.GetValueImpl(num + i);
				try
				{
					destinationArray.SetValueImpl(valueImpl, num2 + i);
				}
				catch (ArgumentException)
				{
					throw CreateArrayTypeMismatchException();
				}
				catch
				{
					if (CanAssignArrayElement(elementType, elementType2))
					{
						throw;
					}
					throw CreateArrayTypeMismatchException();
				}
			}
			return;
		}
		for (int num3 = length - 1; num3 >= 0; num3--)
		{
			object valueImpl2 = sourceArray.GetValueImpl(num + num3);
			try
			{
				destinationArray.SetValueImpl(valueImpl2, num2 + num3);
			}
			catch (ArgumentException)
			{
				throw CreateArrayTypeMismatchException();
			}
			catch
			{
				if (CanAssignArrayElement(elementType, elementType2))
				{
					throw;
				}
				throw CreateArrayTypeMismatchException();
			}
		}
	}

	private static Exception CreateArrayTypeMismatchException()
	{
		return new ArrayTypeMismatchException();
	}

	private static bool CanAssignArrayElement(Type source, Type target)
	{
		if (source.IsValueType)
		{
			return source.IsAssignableFrom(target);
		}
		if (source.IsInterface)
		{
			return !target.IsValueType;
		}
		if (target.IsInterface)
		{
			return !source.IsValueType;
		}
		if (!source.IsAssignableFrom(target))
		{
			return target.IsAssignableFrom(source);
		}
		return true;
	}

	/// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index.  Guarantees that all changes are undone if the copy does not succeed completely.</summary>
	/// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
	/// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
	/// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
	/// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
	/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
	/// <exception cref="T:System.RankException">
	///   <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
	/// <exception cref="T:System.ArrayTypeMismatchException">The <paramref name="sourceArray" /> type is neither the same as nor derived from the <paramref name="destinationArray" /> type.</exception>
	/// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void ConstrainedCopy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
	{
		Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
	}

	public static T[] Empty<T>()
	{
		return EmptyArray<T>.Value;
	}

	/// <summary>Initializes every element of the value-type <see cref="T:System.Array" /> by calling the default constructor of the value type.</summary>
	/// <filterpriority>2</filterpriority>
	public void Initialize()
	{
	}

	private static int IndexOfImpl<T>(T[] array, T value, int startIndex, int count)
	{
		return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
	}

	private static int LastIndexOfImpl<T>(T[] array, T value, int startIndex, int count)
	{
		return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
	}

	private static void SortImpl(Array keys, Array items, int index, int length, IComparer comparer)
	{
		object[] array = keys as object[];
		object[] array2 = null;
		if (array != null)
		{
			array2 = items as object[];
		}
		if (array != null && (items == null || array2 != null))
		{
			new SorterObjectArray(array, array2, comparer).Sort(index, length);
		}
		else
		{
			new SorterGenericArray(keys, items, comparer).Sort(index, length);
		}
	}

	internal static T UnsafeLoad<T>(T[] array, int index)
	{
		return array[index];
	}

	internal static void UnsafeStore<T>(T[] array, int index, T value)
	{
		array[index] = value;
	}

	internal static R UnsafeMov<S, R>(S instance)
	{
		return (R)(object)instance;
	}
}
