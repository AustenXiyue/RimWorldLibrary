using System.Collections;

namespace System.Configuration;

/// <summary>Contains a collection of settings property values that map <see cref="T:System.Configuration.SettingsProperty" /> objects to <see cref="T:System.Configuration.SettingsPropertyValue" /> objects.</summary>
/// <filterpriority>2</filterpriority>
public class SettingsPropertyValueCollection : ICloneable, ICollection, IEnumerable
{
	private Hashtable items;

	private bool isReadOnly;

	/// <summary>Gets a value that specifies the number of <see cref="T:System.Configuration.SettingsPropertyValue" /> objects in the collection.</summary>
	/// <returns>The number of <see cref="T:System.Configuration.SettingsPropertyValue" /> objects in the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public int Count => items.Count;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> collection is synchronized; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSynchronized
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets an item from the collection.</summary>
	/// <returns>The <see cref="T:System.Configuration.SettingsPropertyValue" /> object with the specified <paramref name="name" />.</returns>
	/// <param name="name">A <see cref="T:System.Configuration.SettingsPropertyValue" /> object.</param>
	/// <filterpriority>2</filterpriority>
	public SettingsPropertyValue this[string name] => (SettingsPropertyValue)items[name];

	/// <summary>Gets the object to synchronize access to the collection.</summary>
	/// <returns>The object to synchronize access to the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public object SyncRoot
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> class.</summary>
	public SettingsPropertyValueCollection()
	{
		items = new Hashtable();
	}

	/// <summary>Adds a <see cref="T:System.Configuration.SettingsPropertyValue" /> object to the collection.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsPropertyValue" /> object.</param>
	/// <exception cref="T:System.NotSupportedException">An attempt was made to add an item to the collection, but the collection was marked as read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public void Add(SettingsPropertyValue property)
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		items.Add(property.Name, property);
	}

	internal void Add(SettingsPropertyValueCollection vals)
	{
		foreach (SettingsPropertyValue val in vals)
		{
			Add(val);
		}
	}

	/// <summary>Removes all <see cref="T:System.Configuration.SettingsPropertyValue" /> objects from the collection.</summary>
	/// <filterpriority>2</filterpriority>
	public void Clear()
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		items.Clear();
	}

	/// <summary>Creates a copy of the existing collection.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> class.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new SettingsPropertyValueCollection
		{
			items = (Hashtable)items.Clone()
		};
	}

	/// <summary>Copies this <see cref="T:System.Configuration.SettingsPropertyValueCollection" /> collection to an array.</summary>
	/// <param name="array">The array to copy the collection to.</param>
	/// <param name="index">The index at which to begin copying.</param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(Array array, int index)
	{
		items.Values.CopyTo(array, index);
	}

	/// <summary>Gets the <see cref="T:System.Collections.IEnumerator" /> object as it applies to the collection.</summary>
	/// <returns>The <see cref="T:System.Collections.IEnumerator" /> object as it applies to the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerator GetEnumerator()
	{
		return items.Values.GetEnumerator();
	}

	/// <summary>Removes a <see cref="T:System.Configuration.SettingsPropertyValue" /> object from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.SettingsPropertyValue" /> object.</param>
	/// <exception cref="T:System.NotSupportedException">An attempt was made to remove an item from the collection, but the collection was marked as read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public void Remove(string name)
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		items.Remove(name);
	}

	/// <summary>Sets the collection to be read-only.</summary>
	/// <filterpriority>2</filterpriority>
	public void SetReadOnly()
	{
		isReadOnly = true;
	}
}
