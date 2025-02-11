using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Controls code access permissions for event logging.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class EventLogPermission : ResourcePermissionBase
{
	private EventLogPermissionEntryCollection innerCollection;

	/// <summary>Gets the collection of permission entries for this permissions request.</summary>
	/// <returns>A collection that contains the permission entries for this permissions request.</returns>
	/// <filterpriority>2</filterpriority>
	public EventLogPermissionEntryCollection PermissionEntries
	{
		get
		{
			if (innerCollection == null)
			{
				innerCollection = new EventLogPermissionEntryCollection(this);
			}
			return innerCollection;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogPermission" /> class.</summary>
	public EventLogPermission()
	{
		SetUp();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogPermission" /> class with the specified permission entries.</summary>
	/// <param name="permissionAccessEntries">An array of  objects that represent permission entries. The <see cref="P:System.Diagnostics.EventLogPermission.PermissionEntries" /> property is set to this value. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="permissionAccessEntries" /> is null.</exception>
	public EventLogPermission(EventLogPermissionEntry[] permissionAccessEntries)
	{
		if (permissionAccessEntries == null)
		{
			throw new ArgumentNullException("permissionAccessEntries");
		}
		SetUp();
		innerCollection = new EventLogPermissionEntryCollection(this);
		innerCollection.AddRange(permissionAccessEntries);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogPermission" /> class with the specified permission state.</summary>
	/// <param name="state">One of the enumeration values that specifies the permission state (full access or no access to resources). </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public EventLogPermission(PermissionState state)
		: base(state)
	{
		SetUp();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.EventLogPermission" /> class with the specified access levels and the name of the computer to use.</summary>
	/// <param name="permissionAccess">One of the enumeration values that specifies an access level. </param>
	/// <param name="machineName">The name of the computer on which to read or write events. </param>
	public EventLogPermission(EventLogPermissionAccess permissionAccess, string machineName)
	{
		SetUp();
		innerCollection = new EventLogPermissionEntryCollection(this);
		innerCollection.Add(new EventLogPermissionEntry(permissionAccess, machineName));
	}

	private void SetUp()
	{
		base.TagNames = new string[1] { "Machine" };
		base.PermissionAccessType = typeof(EventLogPermissionAccess);
	}

	internal ResourcePermissionBaseEntry[] GetEntries()
	{
		return GetPermissionEntries();
	}

	internal void ClearEntries()
	{
		Clear();
	}

	internal void Add(object obj)
	{
		EventLogPermissionEntry eventLogPermissionEntry = obj as EventLogPermissionEntry;
		AddPermissionAccess(eventLogPermissionEntry.CreateResourcePermissionBaseEntry());
	}

	internal void Remove(object obj)
	{
		EventLogPermissionEntry eventLogPermissionEntry = obj as EventLogPermissionEntry;
		RemovePermissionAccess(eventLogPermissionEntry.CreateResourcePermissionBaseEntry());
	}
}
