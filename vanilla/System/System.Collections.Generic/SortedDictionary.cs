using System.Diagnostics;

namespace System.Collections.Generic;

/// <summary>Represents a collection of key/value pairs that are sorted on the key. </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerTypeProxy(typeof(IDictionaryDebugView<, >))]
[DebuggerDisplay("Count = {Count}")]
public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
	{
		private SortedSet<KeyValuePair<TKey, TValue>>.Enumerator _treeEnum;

		private int _getEnumeratorRetType;

		internal const int KeyValuePair = 1;

		internal const int DictEntry = 2;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> at the current position of the enumerator.</returns>
		public KeyValuePair<TKey, TValue> Current => _treeEnum.Current;

		internal bool NotStartedOrEnded => _treeEnum.NotStartedOrEnded;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the collection at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IEnumerator.Current
		{
			get
			{
				if (NotStartedOrEnded)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				if (_getEnumeratorRetType == 2)
				{
					return new DictionaryEntry(Current.Key, Current.Value);
				}
				return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);
			}
		}

		/// <summary>Gets the key of the element at the current position of the enumerator.</summary>
		/// <returns>The key of the element in the collection at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IDictionaryEnumerator.Key
		{
			get
			{
				if (NotStartedOrEnded)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return Current.Key;
			}
		}

		/// <summary>Gets the value of the element at the current position of the enumerator.</summary>
		/// <returns>The value of the element in the collection at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IDictionaryEnumerator.Value
		{
			get
			{
				if (NotStartedOrEnded)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return Current.Value;
			}
		}

		/// <summary>Gets the element at the current position of the enumerator as a <see cref="T:System.Collections.DictionaryEntry" /> structure.</summary>
		/// <returns>The element in the collection at the current position of the dictionary, as a <see cref="T:System.Collections.DictionaryEntry" /> structure.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		DictionaryEntry IDictionaryEnumerator.Entry
		{
			get
			{
				if (NotStartedOrEnded)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return new DictionaryEntry(Current.Key, Current.Value);
			}
		}

		internal Enumerator(SortedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
		{
			_treeEnum = dictionary._set.GetEnumerator();
			_getEnumeratorRetType = getEnumeratorRetType;
		}

		/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			return _treeEnum.MoveNext();
		}

		/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.SortedDictionary`2.Enumerator" />.</summary>
		public void Dispose()
		{
			_treeEnum.Dispose();
		}

		internal void Reset()
		{
			_treeEnum.Reset();
		}

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset()
		{
			_treeEnum.Reset();
		}
	}

	/// <summary>Represents the collection of keys in a <see cref="T:System.Collections.Generic.SortedDictionary`2" />. This class cannot be inherited. </summary>
	[Serializable]
	[DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
	{
		/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</summary>
		public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
		{
			private SortedDictionary<TKey, TValue>.Enumerator _dictEnum;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" /> at the current position of the enumerator.</returns>
			public TKey Current => _dictEnum.Current.Key;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the collection at the current position of the enumerator.</returns>
			/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
			object IEnumerator.Current
			{
				get
				{
					if (_dictEnum.NotStartedOrEnded)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return Current;
				}
			}

			internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
			{
				_dictEnum = dictionary.GetEnumerator();
			}

			/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection.Enumerator" />.</summary>
			public void Dispose()
			{
				_dictEnum.Dispose();
			}

			/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			public bool MoveNext()
			{
				return _dictEnum.MoveNext();
			}

			/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			void IEnumerator.Reset()
			{
				_dictEnum.Reset();
			}
		}

		private SortedDictionary<TKey, TValue> _dictionary;

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</returns>
		public int Count => _dictionary.Count;

		bool ICollection<TKey>.IsReadOnly => true;

		/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />, this property always returns false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />, this property always returns the current instance.</returns>
		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" /> class that reflects the keys in the specified <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.SortedDictionary`2" /> whose keys are reflected in the new <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="dictionary" /> is null.</exception>
		public KeyCollection(SortedDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}
			_dictionary = dictionary;
		}

		/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		/// <summary>Copies the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" /> elements to an existing one-dimensional array, starting at the specified array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" />. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(TKey[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			}
			if (array.Length - index < Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			_dictionary._set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
			{
				array[index++] = node.Item.Key;
				return true;
			});
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an array, starting at a particular array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
			}
			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("The lower bound of target array must be zero.", "array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			}
			if (array.Length - index < _dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			if (array is TKey[] array2)
			{
				CopyTo(array2, index);
				return;
			}
			try
			{
				object[] objects = (object[])array;
				_dictionary._set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					object[] array3 = objects;
					int num = index;
					index = num + 1;
					array3[num] = node.Item.Key;
					return true;
				});
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}

		void ICollection<TKey>.Add(TKey item)
		{
			throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
		}

		void ICollection<TKey>.Clear()
		{
			throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
		}

		bool ICollection<TKey>.Contains(TKey item)
		{
			return _dictionary.ContainsKey(item);
		}

		bool ICollection<TKey>.Remove(TKey item)
		{
			throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
		}
	}

	/// <summary>Represents the collection of values in a <see cref="T:System.Collections.Generic.SortedDictionary`2" />. This class cannot be inherited</summary>
	[Serializable]
	[DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
	{
		/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</summary>
		public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
		{
			private SortedDictionary<TKey, TValue>.Enumerator _dictEnum;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" /> at the current position of the enumerator.</returns>
			public TValue Current => _dictEnum.Current.Value;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the collection at the current position of the enumerator.</returns>
			/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
			object IEnumerator.Current
			{
				get
				{
					if (_dictEnum.NotStartedOrEnded)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return Current;
				}
			}

			internal Enumerator(SortedDictionary<TKey, TValue> dictionary)
			{
				_dictEnum = dictionary.GetEnumerator();
			}

			/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection.Enumerator" />.</summary>
			public void Dispose()
			{
				_dictEnum.Dispose();
			}

			/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			public bool MoveNext()
			{
				return _dictEnum.MoveNext();
			}

			/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			void IEnumerator.Reset()
			{
				_dictEnum.Reset();
			}
		}

		private SortedDictionary<TKey, TValue> _dictionary;

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</returns>
		public int Count => _dictionary.Count;

		bool ICollection<TValue>.IsReadOnly => true;

		/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />, this property always returns false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />, this property always returns the current instance.</returns>
		object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" /> class that reflects the values in the specified <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.SortedDictionary`2" /> whose values are reflected in the new <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="dictionary" /> is null.</exception>
		public ValueCollection(SortedDictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}
			_dictionary = dictionary;
		}

		/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(_dictionary);
		}

		/// <summary>Copies the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" /> elements to an existing one-dimensional array, starting at the specified array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" />. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(TValue[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			}
			if (array.Length - index < Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			_dictionary._set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
			{
				array[index++] = node.Item.Value;
				return true;
			});
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an array, starting at a particular array index.</summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.ICollection" />. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
			}
			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("The lower bound of target array must be zero.", "array");
			}
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Non-negative number required.");
			}
			if (array.Length - index < _dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			if (array is TValue[] array2)
			{
				CopyTo(array2, index);
				return;
			}
			try
			{
				object[] objects = (object[])array;
				_dictionary._set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
				{
					object[] array3 = objects;
					int num = index;
					index = num + 1;
					array3[num] = node.Item.Value;
					return true;
				});
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}

		void ICollection<TValue>.Clear()
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}

		bool ICollection<TValue>.Contains(TValue item)
		{
			return _dictionary.ContainsValue(item);
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}
	}

	[Serializable]
	internal sealed class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>>
	{
		internal IComparer<TKey> keyComparer;

		public KeyValuePairComparer(IComparer<TKey> keyComparer)
		{
			if (keyComparer == null)
			{
				this.keyComparer = Comparer<TKey>.Default;
			}
			else
			{
				this.keyComparer = keyComparer;
			}
		}

		public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
		{
			return keyComparer.Compare(x.Key, y.Key);
		}
	}

	[NonSerialized]
	private KeyCollection _keys;

	[NonSerialized]
	private ValueCollection _values;

	private TreeSet<KeyValuePair<TKey, TValue>> _set;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	/// <summary>Gets or sets the value associated with the specified key.</summary>
	/// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
	/// <param name="key">The key of the value to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
	public TValue this[TKey key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return (_set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue))) ?? throw new KeyNotFoundException()).Item.Value;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			SortedSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
			if (node == null)
			{
				_set.Add(new KeyValuePair<TKey, TValue>(key, value));
				return;
			}
			node.Item = new KeyValuePair<TKey, TValue>(node.Item.Key, value);
			_set.UpdateVersion();
		}
	}

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	public int Count => _set.Count;

	/// <summary>Gets the <see cref="T:System.Collections.Generic.IComparer`1" /> used to order the elements of the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>The <see cref="T:System.Collections.Generic.IComparer`1" /> used to order the elements of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /></returns>
	public IComparer<TKey> Comparer => ((KeyValuePairComparer)_set.Comparer).keyComparer;

	/// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.SortedDictionary`2.KeyCollection" /> containing the keys in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	public KeyCollection Keys
	{
		get
		{
			if (_keys == null)
			{
				_keys = new KeyCollection(this);
			}
			return _keys;
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

	/// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.SortedDictionary`2.ValueCollection" /> containing the values in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	public ValueCollection Values
	{
		get
		{
			if (_values == null)
			{
				_values = new ValueCollection(this);
			}
			return _values;
		}
	}

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsReadOnly => false;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Keys => Keys;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Values => Values;

	/// <summary>Gets or sets the element with the specified key.</summary>
	/// <returns>The element with the specified key, or null if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	/// <param name="key">The key of the element to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.-or-A value is being assigned, and <paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</exception>
	object IDictionary.this[object key]
	{
		get
		{
			if (IsCompatibleKey(key) && TryGetValue((TKey)key, out var value))
			{
				return value;
			}
			return null;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (value == null && default(TValue) != null)
			{
				throw new ArgumentNullException("value");
			}
			try
			{
				TKey key2 = (TKey)key;
				try
				{
					this[key2] = (TValue)value;
				}
				catch (InvalidCastException)
				{
					throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
				}
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", key, typeof(TKey)), "key");
			}
		}
	}

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.SortedDictionary`2" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. </returns>
	object ICollection.SyncRoot => ((ICollection)_set).SyncRoot;

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> class that is empty and uses the default <see cref="T:System.Collections.Generic.IComparer`1" /> implementation for the key type.</summary>
	public SortedDictionary()
		: this((IComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default <see cref="T:System.Collections.Generic.IComparer`1" /> implementation for the key type.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public SortedDictionary(IDictionary<TKey, TValue> dictionary)
		: this(dictionary, (IComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to compare keys.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.Comparer`1" /> for the type of the key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public SortedDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		_set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			_set.Add(item);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> class that is empty and uses the specified <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to compare keys.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.Comparer`1" /> for the type of the key.</param>
	public SortedDictionary(IComparer<TKey> comparer)
	{
		_set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
	{
		_set.Add(keyValuePair);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		SortedSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
		if (node == null)
		{
			return false;
		}
		if (keyValuePair.Value == null)
		{
			return node.Item.Value == null;
		}
		return EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		SortedSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
		if (node == null)
		{
			return false;
		}
		if (EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value))
		{
			_set.Remove(keyValuePair);
			return true;
		}
		return false;
	}

	/// <summary>Adds an element with the specified key and value into the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</exception>
	public void Add(TKey key, TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		_set.Add(new KeyValuePair<TKey, TValue>(key, value));
	}

	/// <summary>Removes all elements from the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	public void Clear()
	{
		_set.Clear();
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool ContainsKey(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return _set.Contains(new KeyValuePair<TKey, TValue>(key, default(TValue)));
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> contains an element with the specified value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> contains an element with the specified value; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />. The value can be null for reference types.</param>
	public bool ContainsValue(TValue value)
	{
		bool found = false;
		if (value == null)
		{
			_set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
			{
				if (node.Item.Value == null)
				{
					found = true;
					return false;
				}
				return true;
			});
		}
		else
		{
			EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
			_set.InOrderTreeWalk(delegate(SortedSet<KeyValuePair<TKey, TValue>>.Node node)
			{
				if (valueComparer.Equals(node.Item.Value, value))
				{
					found = true;
					return false;
				}
				return true;
			});
		}
		return found;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> to the specified array of <see cref="T:System.Collections.Generic.KeyValuePair`2" /> structures, starting at the specified index.</summary>
	/// <param name="array">The one-dimensional array of <see cref="T:System.Collections.Generic.KeyValuePair`2" /> structures that is the destination of the elements copied from the current <see cref="T:System.Collections.Generic.SortedDictionary`2" /> The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.SortedDictionary`2" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		_set.CopyTo(array, index);
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.SortedDictionary`2.Enumerator" /> for the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this, 1);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this, 1);
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</summary>
	/// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.SortedDictionary`2" />.</returns>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return _set.Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
	}

	/// <summary>Gets the value associated with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.SortedDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		SortedSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
		if (node == null)
		{
			value = default(TValue);
			return false;
		}
		value = node.Item.Value;
		return true;
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.ICollection`1" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.Generic.ICollection`1" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_set).CopyTo(array, index);
	}

	/// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The object to use as the key of the element to add.</param>
	/// <param name="value">The object to use as the value of the element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.IDictionary" />.-or-<paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.IDictionary" />.-or-An element with the same key already exists in the <see cref="T:System.Collections.IDictionary" />.</exception>
	void IDictionary.Add(object key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (value == null && default(TValue) != null)
		{
			throw new ArgumentNullException("value");
		}
		try
		{
			TKey key2 = (TKey)key;
			try
			{
				Add(key2, (TValue)value);
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
			}
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(global::SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", key, typeof(TKey)), "key");
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	bool IDictionary.Contains(object key)
	{
		if (IsCompatibleKey(key))
		{
			return ContainsKey((TKey)key);
		}
		return false;
	}

	private static bool IsCompatibleKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return key is TKey;
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(this, 2);
	}

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	void IDictionary.Remove(object key)
	{
		if (IsCompatibleKey(key))
		{
			Remove((TKey)key);
		}
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this, 1);
	}
}
