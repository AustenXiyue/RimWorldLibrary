using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace System.Collections.Generic;

/// <summary>Represents a collection of keys and values.</summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <filterpriority>1</filterpriority>
[Serializable]
[DebuggerTypeProxy(typeof(IDictionaryDebugView<, >))]
[DebuggerDisplay("Count = {Count}")]
public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable, IDeserializationCallback
{
	private struct Entry
	{
		public int hashCode;

		public int next;

		public TKey key;

		public TValue value;
	}

	/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	[Serializable]
	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
	{
		private Dictionary<TKey, TValue> dictionary;

		private int version;

		private int index;

		private KeyValuePair<TKey, TValue> current;

		private int getEnumeratorRetType;

		internal const int DictEntry = 1;

		internal const int KeyValuePair = 2;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2" /> at the current position of the enumerator.</returns>
		public KeyValuePair<TKey, TValue> Current => current;

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the collection at the current position of the enumerator, as an <see cref="T:System.Object" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IEnumerator.Current
		{
			get
			{
				if (index == 0 || index == dictionary.count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				if (getEnumeratorRetType == 1)
				{
					return new DictionaryEntry(current.Key, current.Value);
				}
				return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
			}
		}

		/// <summary>Gets the element at the current position of the enumerator.</summary>
		/// <returns>The element in the dictionary at the current position of the enumerator, as a <see cref="T:System.Collections.DictionaryEntry" />.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		DictionaryEntry IDictionaryEnumerator.Entry
		{
			get
			{
				if (index == 0 || index == dictionary.count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return new DictionaryEntry(current.Key, current.Value);
			}
		}

		/// <summary>Gets the key of the element at the current position of the enumerator.</summary>
		/// <returns>The key of the element in the dictionary at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IDictionaryEnumerator.Key
		{
			get
			{
				if (index == 0 || index == dictionary.count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return current.Key;
			}
		}

		/// <summary>Gets the value of the element at the current position of the enumerator.</summary>
		/// <returns>The value of the element in the dictionary at the current position of the enumerator.</returns>
		/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
		object IDictionaryEnumerator.Value
		{
			get
			{
				if (index == 0 || index == dictionary.count + 1)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return current.Value;
			}
		}

		internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
		{
			this.dictionary = dictionary;
			version = dictionary.version;
			index = 0;
			this.getEnumeratorRetType = getEnumeratorRetType;
			current = default(KeyValuePair<TKey, TValue>);
		}

		/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			if (version != dictionary.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			while ((uint)index < (uint)dictionary.count)
			{
				if (dictionary.entries[index].hashCode >= 0)
				{
					current = new KeyValuePair<TKey, TValue>(dictionary.entries[index].key, dictionary.entries[index].value);
					index++;
					return true;
				}
				index++;
			}
			index = dictionary.count + 1;
			current = default(KeyValuePair<TKey, TValue>);
			return false;
		}

		/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" />.</summary>
		public void Dispose()
		{
		}

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
		void IEnumerator.Reset()
		{
			if (version != dictionary.version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			index = 0;
			current = default(KeyValuePair<TKey, TValue>);
		}
	}

	/// <summary>Represents the collection of keys in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited.</summary>
	[Serializable]
	[DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<, >))]
	[DebuggerDisplay("Count = {Count}")]
	public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
	{
		/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
		[Serializable]
		public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
		{
			private Dictionary<TKey, TValue> dictionary;

			private int index;

			private int version;

			private TKey currentKey;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> at the current position of the enumerator.</returns>
			public TKey Current => currentKey;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the collection at the current position of the enumerator.</returns>
			/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return currentKey;
				}
			}

			internal Enumerator(Dictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
				version = dictionary.version;
				index = 0;
				currentKey = default(TKey);
			}

			/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" />.</summary>
			public void Dispose()
			{
			}

			/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			public bool MoveNext()
			{
				if (version != dictionary.version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				while ((uint)index < (uint)dictionary.count)
				{
					if (dictionary.entries[index].hashCode >= 0)
					{
						currentKey = dictionary.entries[index].key;
						index++;
						return true;
					}
					index++;
				}
				index = dictionary.count + 1;
				currentKey = default(TKey);
				return false;
			}

			/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			void IEnumerator.Reset()
			{
				if (version != dictionary.version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				index = 0;
				currentKey = default(TKey);
			}
		}

		private Dictionary<TKey, TValue> dictionary;

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.Retrieving the value of this property is an O(1) operation.</returns>
		public int Count => dictionary.Count;

		bool ICollection<TKey>.IsReadOnly => true;

		/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />, this property always returns false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />, this property always returns the current instance.</returns>
		object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> class that reflects the keys in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose keys are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="dictionary" /> is null.</exception>
		public KeyCollection(Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}
			this.dictionary = dictionary;
		}

		/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(TKey[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			if (array.Length - index < dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			int count = dictionary.count;
			Entry[] entries = dictionary.entries;
			for (int i = 0; i < count; i++)
			{
				if (entries[i].hashCode >= 0)
				{
					array[index++] = entries[i].key;
				}
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
			return dictionary.ContainsKey(item);
		}

		bool ICollection<TKey>.Remove(TKey item)
		{
			throw new NotSupportedException("Mutating a key collection derived from a dictionary is not allowed.");
		}

		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.</exception>
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
			if (index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			if (array.Length - index < dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			if (array is TKey[] array2)
			{
				CopyTo(array2, index);
				return;
			}
			if (!(array is object[] array3))
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
			int count = dictionary.count;
			Entry[] entries = dictionary.entries;
			try
			{
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0)
					{
						array3[index++] = entries[i].key;
					}
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}
	}

	/// <summary>Represents the collection of values in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited. </summary>
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<, >))]
	public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
	{
		/// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
		[Serializable]
		public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
		{
			private Dictionary<TKey, TValue> dictionary;

			private int index;

			private int version;

			private TValue currentValue;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> at the current position of the enumerator.</returns>
			public TValue Current => currentValue;

			/// <summary>Gets the element at the current position of the enumerator.</summary>
			/// <returns>The element in the collection at the current position of the enumerator.</returns>
			/// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || index == dictionary.count + 1)
					{
						throw new InvalidOperationException("Enumeration has either not started or has already finished.");
					}
					return currentValue;
				}
			}

			internal Enumerator(Dictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
				version = dictionary.version;
				index = 0;
				currentValue = default(TValue);
			}

			/// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" />.</summary>
			public void Dispose()
			{
			}

			/// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			public bool MoveNext()
			{
				if (version != dictionary.version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				while ((uint)index < (uint)dictionary.count)
				{
					if (dictionary.entries[index].hashCode >= 0)
					{
						currentValue = dictionary.entries[index].value;
						index++;
						return true;
					}
					index++;
				}
				index = dictionary.count + 1;
				currentValue = default(TValue);
				return false;
			}

			/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			void IEnumerator.Reset()
			{
				if (version != dictionary.version)
				{
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}
				index = 0;
				currentValue = default(TValue);
			}
		}

		private Dictionary<TKey, TValue> dictionary;

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>
		public int Count => dictionary.Count;

		bool ICollection<TValue>.IsReadOnly => true;

		/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />, this property always returns false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />, this property always returns the current instance.</returns>
		object ICollection.SyncRoot => ((ICollection)dictionary).SyncRoot;

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> class that reflects the values in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose values are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="dictionary" /> is null.</exception>
		public ValueCollection(Dictionary<TKey, TValue> dictionary)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}
			this.dictionary = dictionary;
		}

		/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
		/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(TValue[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			if (array.Length - index < dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			int count = dictionary.count;
			Entry[] entries = dictionary.entries;
			for (int i = 0; i < count; i++)
			{
				if (entries[i].hashCode >= 0)
				{
					array[index++] = entries[i].value;
				}
			}
		}

		void ICollection<TValue>.Add(TValue item)
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}

		bool ICollection<TValue>.Remove(TValue item)
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}

		void ICollection<TValue>.Clear()
		{
			throw new NotSupportedException("Mutating a value collection derived from a dictionary is not allowed.");
		}

		bool ICollection<TValue>.Contains(TValue item)
		{
			return dictionary.ContainsValue(item);
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(dictionary);
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.</exception>
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
			if (index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
			}
			if (array.Length - index < dictionary.Count)
			{
				throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
			}
			if (array is TValue[] array2)
			{
				CopyTo(array2, index);
				return;
			}
			if (!(array is object[] array3))
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
			int count = dictionary.count;
			Entry[] entries = dictionary.entries;
			try
			{
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0)
					{
						array3[index++] = entries[i].value;
					}
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
			}
		}
	}

	private int[] buckets;

	private Entry[] entries;

	private int count;

	private int version;

	private int freeList;

	private int freeCount;

	private IEqualityComparer<TKey> comparer;

	private KeyCollection keys;

	private ValueCollection values;

	private object _syncRoot;

	private const string VersionName = "Version";

	private const string HashSizeName = "HashSize";

	private const string KeyValuePairsName = "KeyValuePairs";

	private const string ComparerName = "Comparer";

	/// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> that is used to determine equality of keys for the dictionary. </summary>
	/// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface implementation that is used to determine equality of keys for the current <see cref="T:System.Collections.Generic.Dictionary`2" /> and to provide hash values for the keys.</returns>
	public IEqualityComparer<TKey> Comparer => comparer;

	/// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public int Count => count - freeCount;

	/// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public KeyCollection Keys
	{
		get
		{
			if (keys == null)
			{
				keys = new KeyCollection(this);
			}
			return keys;
		}
	}

	ICollection<TKey> IDictionary<TKey, TValue>.Keys
	{
		get
		{
			if (keys == null)
			{
				keys = new KeyCollection(this);
			}
			return keys;
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
	{
		get
		{
			if (keys == null)
			{
				keys = new KeyCollection(this);
			}
			return keys;
		}
	}

	/// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public ValueCollection Values
	{
		get
		{
			if (values == null)
			{
				values = new ValueCollection(this);
			}
			return values;
		}
	}

	ICollection<TValue> IDictionary<TKey, TValue>.Values
	{
		get
		{
			if (values == null)
			{
				values = new ValueCollection(this);
			}
			return values;
		}
	}

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
	{
		get
		{
			if (values == null)
			{
				values = new ValueCollection(this);
			}
			return values;
		}
	}

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
			int num = FindEntry(key);
			if (num >= 0)
			{
				return entries[num].value;
			}
			throw new KeyNotFoundException();
		}
		set
		{
			TryInsert(key, value, InsertionBehavior.OverwriteExisting);
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />. </returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange<object>(ref _syncRoot, new object(), (object)null);
			}
			return _syncRoot;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> has a fixed size.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> has a fixed size; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IDictionary" /> is read-only.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> is read-only; otherwise, false.  In the default implementation of <see cref="T:System.Collections.Generic.Dictionary`2" />, this property always returns false.</returns>
	bool IDictionary.IsReadOnly => false;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the keys of the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Keys => Keys;

	/// <summary>Gets an <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> containing the values in the <see cref="T:System.Collections.IDictionary" />.</returns>
	ICollection IDictionary.Values => Values;

	/// <summary>Gets or sets the value with the specified key.</summary>
	/// <returns>The value associated with the specified key, or null if <paramref name="key" /> is not in the dictionary or <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	/// <param name="key">The key of the value to get.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A value is being assigned, and <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.-or-A value is being assigned, and <paramref name="value" /> is of a type that is not assignable to the value type <paramref name="TValue" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
	object IDictionary.this[object key]
	{
		get
		{
			if (IsCompatibleKey(key))
			{
				int num = FindEntry((TKey)key);
				if (num >= 0)
				{
					return entries[num].value;
				}
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
					throw new ArgumentException(SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
				}
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException(SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", key, typeof(TKey)), "key");
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
	public Dictionary()
		: this(0, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than 0.</exception>
	public Dictionary(int capacity)
		: this(capacity, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
	public Dictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="capacity" /> is less than 0.</exception>
	public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", capacity, "Non-negative number required.");
		}
		if (capacity > 0)
		{
			Initialize(capacity);
		}
		this.comparer = comparer ?? EqualityComparer<TKey>.Default;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public Dictionary(IDictionary<TKey, TValue> dictionary)
		: this(dictionary, (IEqualityComparer<TKey>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
	/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
	/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
	public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		: this(dictionary?.Count ?? 0, comparer)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (dictionary.GetType() == typeof(Dictionary<TKey, TValue>))
		{
			Dictionary<TKey, TValue> obj = (Dictionary<TKey, TValue>)dictionary;
			int num = obj.count;
			Entry[] array = obj.entries;
			for (int i = 0; i < num; i++)
			{
				if (array[i].hashCode >= 0)
				{
					Add(array[i].key, array[i].value);
				}
			}
			return;
		}
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			Add(item.Key, item.Value);
		}
	}

	public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
		: this(collection, (IEqualityComparer<TKey>)null)
	{
	}

	public Dictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
		: this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		foreach (KeyValuePair<TKey, TValue> item in collection)
		{
			Add(item.Key, item.Value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class with serialized data.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
	protected Dictionary(SerializationInfo info, StreamingContext context)
	{
		DictionaryHashHelpers.SerializationInfoTable.Add(this, info);
	}

	/// <summary>Adds the specified key and value to the dictionary.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
	public void Add(TKey key, TValue value)
	{
		TryInsert(key, value, InsertionBehavior.ThrowOnExisting);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
	{
		Add(keyValuePair.Key, keyValuePair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = FindEntry(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
		{
			return true;
		}
		return false;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
	{
		int num = FindEntry(keyValuePair.Key);
		if (num >= 0 && EqualityComparer<TValue>.Default.Equals(entries[num].value, keyValuePair.Value))
		{
			Remove(keyValuePair.Key);
			return true;
		}
		return false;
	}

	/// <summary>Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	public void Clear()
	{
		if (count > 0)
		{
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i] = -1;
			}
			Array.Clear(entries, 0, count);
			freeList = -1;
			count = 0;
			freeCount = 0;
			version++;
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool ContainsKey(TKey key)
	{
		return FindEntry(key) >= 0;
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified value; otherwise, false.</returns>
	/// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be null for reference types.</param>
	public bool ContainsValue(TValue value)
	{
		if (value == null)
		{
			for (int i = 0; i < count; i++)
			{
				if (entries[i].hashCode >= 0 && entries[i].value == null)
				{
					return true;
				}
			}
		}
		else
		{
			EqualityComparer<TValue> @default = EqualityComparer<TValue>.Default;
			for (int j = 0; j < count; j++)
			{
				if (entries[j].hashCode >= 0 && @default.Equals(entries[j].value, value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		int num = count;
		Entry[] array2 = entries;
		for (int i = 0; i < num; i++)
		{
			if (array2[i].hashCode >= 0)
			{
				array[index++] = new KeyValuePair<TKey, TValue>(array2[i].key, array2[i].value);
			}
		}
	}

	/// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this, 2);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this, 2);
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	[SecurityCritical]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Version", version);
		info.AddValue("Comparer", comparer, typeof(IEqualityComparer<TKey>));
		info.AddValue("HashSize", (buckets != null) ? buckets.Length : 0);
		if (buckets != null)
		{
			KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
			CopyTo(array, 0);
			info.AddValue("KeyValuePairs", array, typeof(KeyValuePair<TKey, TValue>[]));
		}
	}

	private int FindEntry(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (buckets != null)
		{
			int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
			for (int num2 = buckets[num % buckets.Length]; num2 >= 0; num2 = entries[num2].next)
			{
				if (entries[num2].hashCode == num && comparer.Equals(entries[num2].key, key))
				{
					return num2;
				}
			}
		}
		return -1;
	}

	private void Initialize(int capacity)
	{
		int prime = HashHelpers.GetPrime(capacity);
		buckets = new int[prime];
		for (int i = 0; i < buckets.Length; i++)
		{
			buckets[i] = -1;
		}
		entries = new Entry[prime];
		freeList = -1;
	}

	private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (buckets == null)
		{
			Initialize(0);
		}
		int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
		int num2 = num % buckets.Length;
		int num3 = 0;
		for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
		{
			if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
			{
				switch (behavior)
				{
				case InsertionBehavior.OverwriteExisting:
					entries[num4].value = value;
					version++;
					return true;
				case InsertionBehavior.ThrowOnExisting:
					throw new ArgumentException(SR.Format("An item with the same key has already been added. Key: {0}", key));
				default:
					return false;
				}
			}
			num3++;
		}
		int num5;
		if (freeCount > 0)
		{
			num5 = freeList;
			freeList = entries[num5].next;
			freeCount--;
		}
		else
		{
			if (count == entries.Length)
			{
				Resize();
				num2 = num % buckets.Length;
			}
			num5 = count;
			count++;
		}
		entries[num5].hashCode = num;
		entries[num5].next = buckets[num2];
		entries[num5].key = key;
		entries[num5].value = value;
		buckets[num2] = num5;
		version++;
		if (num3 > 100 && comparer is NonRandomizedStringEqualityComparer)
		{
			comparer = (IEqualityComparer<TKey>)EqualityComparer<string>.Default;
			Resize(entries.Length, forceNewHashCodes: true);
		}
		return true;
	}

	/// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event.</param>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.Dictionary`2" /> instance is invalid.</exception>
	public virtual void OnDeserialization(object sender)
	{
		DictionaryHashHelpers.SerializationInfoTable.TryGetValue(this, out var value);
		if (value == null)
		{
			return;
		}
		int @int = value.GetInt32("Version");
		int int2 = value.GetInt32("HashSize");
		comparer = (IEqualityComparer<TKey>)value.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
		if (int2 != 0)
		{
			buckets = new int[int2];
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i] = -1;
			}
			entries = new Entry[int2];
			freeList = -1;
			KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])value.GetValue("KeyValuePairs", typeof(KeyValuePair<TKey, TValue>[]));
			if (array == null)
			{
				throw new SerializationException("The keys for this dictionary are missing.");
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j].Key == null)
				{
					throw new SerializationException("One of the serialized keys is null.");
				}
				Add(array[j].Key, array[j].Value);
			}
		}
		else
		{
			buckets = null;
		}
		version = @int;
		DictionaryHashHelpers.SerializationInfoTable.Remove(this);
	}

	private void Resize()
	{
		Resize(HashHelpers.ExpandPrime(count), forceNewHashCodes: false);
	}

	private void Resize(int newSize, bool forceNewHashCodes)
	{
		int[] array = new int[newSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		Entry[] array2 = new Entry[newSize];
		Array.Copy(entries, 0, array2, 0, count);
		if (forceNewHashCodes)
		{
			for (int j = 0; j < count; j++)
			{
				if (array2[j].hashCode != -1)
				{
					array2[j].hashCode = comparer.GetHashCode(array2[j].key) & 0x7FFFFFFF;
				}
			}
		}
		for (int k = 0; k < count; k++)
		{
			if (array2[k].hashCode >= 0)
			{
				int num = array2[k].hashCode % newSize;
				array2[k].next = array[num];
				array[num] = k;
			}
		}
		buckets = array;
		entries = array2;
	}

	/// <summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
	/// <returns>true if the element is successfully found and removed; otherwise, false.  This method returns false if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (buckets != null)
		{
			int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
			int num2 = num % buckets.Length;
			int num3 = -1;
			for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
			{
				if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
				{
					if (num3 < 0)
					{
						buckets[num2] = entries[num4].next;
					}
					else
					{
						entries[num3].next = entries[num4].next;
					}
					entries[num4].hashCode = -1;
					entries[num4].next = freeList;
					entries[num4].key = default(TKey);
					entries[num4].value = default(TValue);
					freeList = num4;
					freeCount++;
					version++;
					return true;
				}
				num3 = num4;
			}
		}
		return false;
	}

	public bool Remove(TKey key, out TValue value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (buckets != null)
		{
			int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
			int num2 = num % buckets.Length;
			int num3 = -1;
			for (int num4 = buckets[num2]; num4 >= 0; num4 = entries[num4].next)
			{
				if (entries[num4].hashCode == num && comparer.Equals(entries[num4].key, key))
				{
					if (num3 < 0)
					{
						buckets[num2] = entries[num4].next;
					}
					else
					{
						entries[num3].next = entries[num4].next;
					}
					value = entries[num4].value;
					entries[num4].hashCode = -1;
					entries[num4].next = freeList;
					entries[num4].key = default(TKey);
					entries[num4].value = default(TValue);
					freeList = num4;
					freeCount++;
					version++;
					return true;
				}
				num3 = num4;
			}
		}
		value = default(TValue);
		return false;
	}

	/// <summary>Gets the value associated with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key of the value to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = FindEntry(key);
		if (num >= 0)
		{
			value = entries[num].value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	public bool TryAdd(TKey key, TValue value)
	{
		return TryInsert(key, value, InsertionBehavior.None);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		CopyTo(array, index);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an array, starting at the specified array index.</summary>
	/// <param name="array">The one-dimensional array that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The array must have zero-based indexing.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is multidimensional.-or-<paramref name="array" /> does not have zero-based indexing.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.Generic.ICollection`1" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
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
		if (index < 0 || index > array.Length)
		{
			throw new ArgumentOutOfRangeException("index", index, "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
		}
		if (array is KeyValuePair<TKey, TValue>[] array2)
		{
			CopyTo(array2, index);
			return;
		}
		if (array is DictionaryEntry[])
		{
			DictionaryEntry[] array3 = array as DictionaryEntry[];
			Entry[] array4 = entries;
			for (int i = 0; i < count; i++)
			{
				if (array4[i].hashCode >= 0)
				{
					array3[index++] = new DictionaryEntry(array4[i].key, array4[i].value);
				}
			}
			return;
		}
		if (!(array is object[] array5))
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
		}
		try
		{
			int num = count;
			Entry[] array6 = entries;
			for (int j = 0; j < num; j++)
			{
				if (array6[j].hashCode >= 0)
				{
					array5[index++] = new KeyValuePair<TKey, TValue>(array6[j].key, array6[j].value);
				}
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Target array type is not compatible with the type of items in the collection.", "array");
		}
	}

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this, 2);
	}

	private static bool IsCompatibleKey(object key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return key is TKey;
	}

	/// <summary>Adds the specified key and value to the dictionary.</summary>
	/// <param name="key">The object to use as the key.</param>
	/// <param name="value">The object to use as the value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="key" /> is of a type that is not assignable to the key type <paramref name="TKey" /> of the <see cref="T:System.Collections.Generic.Dictionary`2" />.-or-<paramref name="value" /> is of a type that is not assignable to <paramref name="TValue" />, the type of values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.-or-A value with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
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
				throw new ArgumentException(SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(TValue)), "value");
			}
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException(SR.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", key, typeof(TKey)), "key");
		}
	}

	/// <summary>Determines whether the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.IDictionary" /> contains an element with the specified key; otherwise, false.</returns>
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

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for the <see cref="T:System.Collections.IDictionary" />.</returns>
	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new Enumerator(this, 1);
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
}
