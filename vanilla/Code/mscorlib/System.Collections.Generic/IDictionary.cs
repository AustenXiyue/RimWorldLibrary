namespace System.Collections.Generic;

/// <summary>Represents a generic collection of key/value pairs.</summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <filterpriority>1</filterpriority>
public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	/// <summary>Gets or sets the element with the specified key.</summary>
	/// <returns>The element with the specified key.</returns>
	/// <param name="key">The key of the element to get or set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
	/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
	TValue this[TKey key] { get; set; }

	/// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	ICollection<TKey> Keys { get; }

	/// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	ICollection<TValue> Values { get; }

	/// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
	/// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
	/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	bool ContainsKey(TKey key);

	/// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <param name="key">The object to use as the key of the element to add.</param>
	/// <param name="value">The object to use as the value of the element to add.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
	void Add(TKey key, TValue value);

	/// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
	/// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
	/// <param name="key">The key of the element to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
	bool Remove(TKey key);

	/// <summary>Gets the value associated with the specified key.</summary>
	/// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
	/// <param name="key">The key whose value to get.</param>
	/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	bool TryGetValue(TKey key, out TValue value);
}
