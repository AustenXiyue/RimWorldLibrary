using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Allows control of code access permissions for <see cref="T:System.Diagnostics.PerformanceCounter" />.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class PerformanceCounterPermission : ResourcePermissionBase
{
	private PerformanceCounterPermissionEntryCollection innerCollection;

	/// <summary>Gets the collection of permission entries for this permissions request.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntryCollection" /> that contains the permission entries for this permissions request.</returns>
	/// <filterpriority>2</filterpriority>
	public PerformanceCounterPermissionEntryCollection PermissionEntries
	{
		get
		{
			if (innerCollection == null)
			{
				innerCollection = new PerformanceCounterPermissionEntryCollection(this);
			}
			return innerCollection;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterPermission" /> class.</summary>
	public PerformanceCounterPermission()
	{
		SetUp();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterPermission" /> class with the specified permission access level entries.</summary>
	/// <param name="permissionAccessEntries">An array of <see cref="T:System.Diagnostics.PerformanceCounterPermissionEntry" /> objects. The <see cref="P:System.Diagnostics.PerformanceCounterPermission.PermissionEntries" /> property is set to this value. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="permissionAccessEntries" /> is null.</exception>
	public PerformanceCounterPermission(PerformanceCounterPermissionEntry[] permissionAccessEntries)
	{
		if (permissionAccessEntries == null)
		{
			throw new ArgumentNullException("permissionAccessEntries");
		}
		SetUp();
		innerCollection = new PerformanceCounterPermissionEntryCollection(this);
		innerCollection.AddRange(permissionAccessEntries);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterPermission" /> class with the specified permission state.</summary>
	/// <param name="state">One of the <see cref="T:System.Security.Permissions.PermissionState" /> values. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="state" /> parameter is not a valid value of <see cref="T:System.Security.Permissions.PermissionState" />. </exception>
	public PerformanceCounterPermission(PermissionState state)
		: base(state)
	{
		SetUp();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterPermission" /> class with the specified access levels, the name of the computer to use, and the category associated with the performance counter.</summary>
	/// <param name="permissionAccess">One of the <see cref="T:System.Diagnostics.PerformanceCounterPermissionAccess" /> values. </param>
	/// <param name="machineName">The server on which the performance counter and its associate category reside. </param>
	/// <param name="categoryName">The name of the performance counter category (performance object) with which the performance counter is associated. </param>
	public PerformanceCounterPermission(PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName)
	{
		SetUp();
		innerCollection = new PerformanceCounterPermissionEntryCollection(this);
		innerCollection.Add(new PerformanceCounterPermissionEntry(permissionAccess, machineName, categoryName));
	}

	private void SetUp()
	{
		base.TagNames = new string[2] { "Machine", "Category" };
		base.PermissionAccessType = typeof(PerformanceCounterPermissionAccess);
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
		PerformanceCounterPermissionEntry performanceCounterPermissionEntry = obj as PerformanceCounterPermissionEntry;
		AddPermissionAccess(performanceCounterPermissionEntry.CreateResourcePermissionBaseEntry());
	}

	internal void Remove(object obj)
	{
		PerformanceCounterPermissionEntry performanceCounterPermissionEntry = obj as PerformanceCounterPermissionEntry;
		RemovePermissionAccess(performanceCounterPermissionEntry.CreateResourcePermissionBaseEntry());
	}
}
