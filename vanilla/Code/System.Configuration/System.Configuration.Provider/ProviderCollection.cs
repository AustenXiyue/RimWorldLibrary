using System.Collections;

namespace System.Configuration.Provider;

/// <summary>Represents a collection of provider objects that inherit from <see cref="T:System.Configuration.Provider.ProviderBase" />.</summary>
public class ProviderCollection : ICollection, IEnumerable
{
	private Hashtable lookup;

	private bool readOnly;

	private ArrayList values;

	/// <summary>Gets the number of providers in the collection.</summary>
	/// <returns>The number of providers in the collection.</returns>
	public int Count => values.Count;

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets the current object.</summary>
	/// <returns>The current object.</returns>
	public object SyncRoot => this;

	/// <summary>Gets the provider with the specified name.</summary>
	/// <returns>The provider with the specified name.</returns>
	/// <param name="name">The key by which the provider is identified.</param>
	public ProviderBase this[string name]
	{
		get
		{
			object obj = lookup[name];
			if (obj == null)
			{
				return null;
			}
			return values[(int)obj] as ProviderBase;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.Provider.ProviderCollection" /> class. </summary>
	public ProviderCollection()
	{
		lookup = new Hashtable(10, StringComparer.InvariantCultureIgnoreCase);
		values = new ArrayList();
	}

	/// <summary>Adds a provider to the collection.</summary>
	/// <param name="provider">The provider to be added.</param>
	/// <exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="provider" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Configuration.Provider.ProviderBase.Name" /> of <paramref name="provider" /> is null.- or -The length of the <see cref="P:System.Configuration.Provider.ProviderBase.Name" /> of <paramref name="provider" /> is less than 1.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public virtual void Add(ProviderBase provider)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		if (provider == null || provider.Name == null)
		{
			throw new ArgumentNullException();
		}
		int num = values.Add(provider);
		try
		{
			lookup.Add(provider.Name, num);
		}
		catch
		{
			values.RemoveAt(num);
			throw;
		}
	}

	/// <summary>Removes all items from the collection.</summary>
	/// <exception cref="T:System.NotSupportedException">The collection is set to read-only.</exception>
	public void Clear()
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		values.Clear();
		lookup.Clear();
	}

	/// <summary>Copies the contents of the collection to the given array starting at the specified index.</summary>
	/// <param name="array">The array to copy the elements of the collection to.</param>
	/// <param name="index">The index of the collection item at which to start the copying process.</param>
	public void CopyTo(ProviderBase[] array, int index)
	{
		values.CopyTo(array, index);
	}

	/// <summary>Copies the elements of the <see cref="T:System.Configuration.Provider.ProviderCollection" /> to an array, starting at a particular array index.</summary>
	/// <param name="array">The array to copy the elements of the collection to.</param>
	/// <param name="index">The index of the array at which to start copying provider instances from the collection.</param>
	void ICollection.CopyTo(Array array, int index)
	{
		values.CopyTo(array, index);
	}

	/// <summary>Returns an object that implements the <see cref="T:System.Collections.IEnumerator" /> interface to iterate through the collection.</summary>
	/// <returns>An object that implements <see cref="T:System.Collections.IEnumerator" /> to iterate through the collection.</returns>
	public IEnumerator GetEnumerator()
	{
		return values.GetEnumerator();
	}

	/// <summary>Removes a provider from the collection.</summary>
	/// <param name="name">The name of the provider to be removed.</param>
	/// <exception cref="T:System.NotSupportedException">The collection has been set to read-only.</exception>
	public void Remove(string name)
	{
		if (readOnly)
		{
			throw new NotSupportedException();
		}
		object obj = lookup[name];
		if (obj == null || !(obj is int num))
		{
			throw new ArgumentException();
		}
		if (num >= values.Count)
		{
			throw new ArgumentException();
		}
		values.RemoveAt(num);
		lookup.Remove(name);
		ArrayList arrayList = new ArrayList();
		foreach (DictionaryEntry item in lookup)
		{
			if ((int)item.Value > num)
			{
				arrayList.Add(item.Key);
			}
		}
		foreach (string item2 in arrayList)
		{
			lookup[item2] = (int)lookup[item2] - 1;
		}
	}

	/// <summary>Sets the collection to be read-only.</summary>
	public void SetReadOnly()
	{
		readOnly = true;
	}
}
