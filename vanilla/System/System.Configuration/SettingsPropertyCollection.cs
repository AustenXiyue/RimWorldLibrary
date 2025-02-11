using System.Collections;

namespace System.Configuration;

/// <summary>Contains a collection of <see cref="T:System.Configuration.SettingsProperty" /> objects.</summary>
/// <filterpriority>2</filterpriority>
public class SettingsPropertyCollection : ICloneable, ICollection, IEnumerable
{
	private Hashtable items;

	private bool isReadOnly;

	/// <summary>Gets a value that specifies the number of <see cref="T:System.Configuration.SettingsProperty" /> objects in the collection.</summary>
	/// <returns>The number of <see cref="T:System.Configuration.SettingsProperty" /> objects in the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public int Count => items.Count;

	/// <summary>Gets a value that indicates whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Configuration.SettingsPropertyCollection" /> is synchronized; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSynchronized => false;

	/// <summary>Gets the collection item with the specified name.</summary>
	/// <returns>The <see cref="T:System.Configuration.SettingsProperty" /> object with the specified <paramref name="name" />.</returns>
	/// <param name="name">The name of the <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <filterpriority>2</filterpriority>
	public SettingsProperty this[string name] => (SettingsProperty)items[name];

	/// <summary>Gets the object to synchronize access to the collection.</summary>
	/// <returns>The object to synchronize access to the collection.</returns>
	/// <filterpriority>2</filterpriority>
	public object SyncRoot => this;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> class.</summary>
	public SettingsPropertyCollection()
	{
		items = new Hashtable();
	}

	/// <summary>Adds a <see cref="T:System.Configuration.SettingsProperty" /> object to the collection.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public void Add(SettingsProperty property)
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		OnAdd(property);
		items.Add(property.Name, property);
		OnAddComplete(property);
	}

	/// <summary>Removes all <see cref="T:System.Configuration.SettingsProperty" /> objects from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public void Clear()
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		OnClear();
		items.Clear();
		OnClearComplete();
	}

	/// <summary>Creates a copy of the existing collection.</summary>
	/// <returns>A <see cref="T:System.Configuration.SettingsPropertyCollection" /> class.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new SettingsPropertyCollection
		{
			items = (Hashtable)items.Clone()
		};
	}

	/// <summary>Copies this <see cref="T:System.Configuration.SettingsPropertyCollection" /> object to an array.</summary>
	/// <param name="array">The array to copy the object to.</param>
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

	/// <summary>Removes a <see cref="T:System.Configuration.SettingsProperty" /> object from the collection.</summary>
	/// <param name="name">The name of the <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <filterpriority>2</filterpriority>
	public void Remove(string name)
	{
		if (isReadOnly)
		{
			throw new NotSupportedException();
		}
		SettingsProperty property = (SettingsProperty)items[name];
		OnRemove(property);
		items.Remove(name);
		OnRemoveComplete(property);
	}

	/// <summary>Sets the collection to be read-only.</summary>
	/// <filterpriority>2</filterpriority>
	public void SetReadOnly()
	{
		isReadOnly = true;
	}

	/// <summary>Performs additional, custom processing when adding to the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	protected virtual void OnAdd(SettingsProperty property)
	{
	}

	/// <summary>Performs additional, custom processing after adding to the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	protected virtual void OnAddComplete(SettingsProperty property)
	{
	}

	/// <summary>Performs additional, custom processing when clearing the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	protected virtual void OnClear()
	{
	}

	/// <summary>Performs additional, custom processing after clearing the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	protected virtual void OnClearComplete()
	{
	}

	/// <summary>Performs additional, custom processing when removing the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	protected virtual void OnRemove(SettingsProperty property)
	{
	}

	/// <summary>Performs additional, custom processing after removing the contents of the <see cref="T:System.Configuration.SettingsPropertyCollection" /> instance.</summary>
	/// <param name="property">A <see cref="T:System.Configuration.SettingsProperty" /> object.</param>
	protected virtual void OnRemoveComplete(SettingsProperty property)
	{
	}
}
