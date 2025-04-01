using System.Collections;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics;

/// <summary>Contains a strongly typed collection of <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> objects.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public class PerformanceCounterPermissionEntryCollection : CollectionBase
{
	private PerformanceCounterPermission owner;

	/// <summary>Gets or sets the object at a specified index.</summary>
	/// <returns>The <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object that exists at the specified index.</returns>
	/// <param name="index">The zero-based index into the collection. </param>
	/// <filterpriority>2</filterpriority>
	public PerformanceCounterPermissionEntry this[int index]
	{
		get
		{
			return (PerformanceCounterPermissionEntry)base.InnerList[index];
		}
		set
		{
			base.InnerList[index] = value;
		}
	}

	internal PerformanceCounterPermissionEntryCollection(PerformanceCounterPermission owner)
	{
		this.owner = owner;
		ResourcePermissionBaseEntry[] entries = owner.GetEntries();
		if (entries.Length != 0)
		{
			ResourcePermissionBaseEntry[] array = entries;
			foreach (ResourcePermissionBaseEntry obj in array)
			{
				PerformanceCounterPermissionAccess permissionAccess = (PerformanceCounterPermissionAccess)obj.PermissionAccess;
				string machineName = obj.PermissionAccessPath[0];
				string categoryName = obj.PermissionAccessPath[1];
				PerformanceCounterPermissionEntry value = new PerformanceCounterPermissionEntry(permissionAccess, machineName, categoryName);
				base.InnerList.Add(value);
			}
		}
	}

	internal PerformanceCounterPermissionEntryCollection(ResourcePermissionBaseEntry[] entries)
	{
		foreach (ResourcePermissionBaseEntry resourcePermissionBaseEntry in entries)
		{
			base.List.Add(new PerformanceCounterPermissionEntry((PerformanceCounterPermissionAccess)resourcePermissionBaseEntry.PermissionAccess, resourcePermissionBaseEntry.PermissionAccessPath[0], resourcePermissionBaseEntry.PermissionAccessPath[1]));
		}
	}

	/// <summary>Adds a specified <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> to this collection.</summary>
	/// <returns>The zero-based index of the added <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object to add. </param>
	/// <filterpriority>2</filterpriority>
	public int Add(PerformanceCounterPermissionEntry value)
	{
		return base.List.Add(value);
	}

	/// <summary>Appends a set of specified permission entries to this collection.</summary>
	/// <param name="value">An array of type <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> objects that contains the permission entries to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public void AddRange(PerformanceCounterPermissionEntry[] value)
	{
		foreach (PerformanceCounterPermissionEntry value2 in value)
		{
			base.List.Add(value2);
		}
	}

	/// <summary>Appends a set of specified permission entries to this collection.</summary>
	/// <param name="value">A <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntryCollection" /> that contains the permission entries to add. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public void AddRange(PerformanceCounterPermissionEntryCollection value)
	{
		foreach (PerformanceCounterPermissionEntry item in value)
		{
			base.List.Add(item);
		}
	}

	/// <summary>Determines whether this collection contains a specified <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object.</summary>
	/// <returns>true if the specified <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object belongs to this collection; otherwise, false.</returns>
	/// <param name="value">The <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> object to find. </param>
	/// <filterpriority>2</filterpriority>
	public bool Contains(PerformanceCounterPermissionEntry value)
	{
		return base.List.Contains(value);
	}

	/// <summary>Copies the permission entries from this collection to an array, starting at a particular index of the array.</summary>
	/// <param name="array">An array of type <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> that receives this collection's permission entries. </param>
	/// <param name="index">The zero-based index at which to begin copying the permission entries. </param>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(PerformanceCounterPermissionEntry[] array, int index)
	{
		base.List.CopyTo(array, index);
	}

	/// <summary>Determines the index of a specified permission entry in this collection.</summary>
	/// <returns>The zero-based index of the specified permission entry, or -1 if the permission entry was not found in the collection.</returns>
	/// <param name="value">The permission entry for which to search. </param>
	/// <filterpriority>2</filterpriority>
	public int IndexOf(PerformanceCounterPermissionEntry value)
	{
		return base.List.IndexOf(value);
	}

	/// <summary>Inserts a permission entry into this collection at a specified index.</summary>
	/// <param name="index">The zero-based index of the collection at which to insert the permission entry. </param>
	/// <param name="value">The permission entry to insert into this collection. </param>
	/// <filterpriority>2</filterpriority>
	public void Insert(int index, PerformanceCounterPermissionEntry value)
	{
		base.List.Insert(index, value);
	}

	/// <summary>Performs additional custom processes after clearing the contents of the collection.</summary>
	protected override void OnClear()
	{
		owner.ClearEntries();
	}

	/// <summary>Performs additional custom processes before a new permission entry is inserted into the collection.</summary>
	/// <param name="index">The zero-based index at which to insert <paramref name="value" />. </param>
	/// <param name="value">The new value of the permission entry at <paramref name="index" />. </param>
	protected override void OnInsert(int index, object value)
	{
		owner.Add(value);
	}

	/// <summary>Performs additional custom processes when removing a new permission entry from the collection.</summary>
	/// <param name="index">The zero-based index at which <paramref name="value" /> can be found. </param>
	/// <param name="value">The permission entry to remove from <paramref name="index" />. </param>
	protected override void OnRemove(int index, object value)
	{
		owner.Remove(value);
	}

	/// <summary>Performs additional custom processes before setting a value in the collection.</summary>
	/// <param name="index">The zero-based index at which <paramref name="oldValue" /> can be found. </param>
	/// <param name="oldValue">The value to replace with <paramref name="newValue" />. </param>
	/// <param name="newValue">The new value of the permission entry at <paramref name="index" />. </param>
	protected override void OnSet(int index, object oldValue, object newValue)
	{
		owner.Remove(oldValue);
		owner.Add(newValue);
	}

	/// <summary>Removes a specified permission entry from this collection.</summary>
	/// <param name="value">The permission entry to remove. </param>
	/// <filterpriority>2</filterpriority>
	public void Remove(PerformanceCounterPermissionEntry value)
	{
		base.List.Remove(value);
	}

	internal PerformanceCounterPermissionEntryCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
