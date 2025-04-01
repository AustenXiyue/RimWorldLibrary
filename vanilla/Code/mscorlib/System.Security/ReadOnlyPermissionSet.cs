using System.Collections;
using Unity;

namespace System.Security;

/// <summary>Represents a read-only collection that can contain many different types of permissions.</summary>
[Serializable]
public sealed class ReadOnlyPermissionSet : PermissionSet
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Security.ReadOnlyPermissionSet" /> class. </summary>
	/// <param name="permissionSetXml">The XML element from which to take the value of the new <see cref="T:System.Security.ReadOnlyPermissionSet" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="permissionSetXml" /> is null.</exception>
	public ReadOnlyPermissionSet(SecurityElement permissionSetXml)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	protected override IPermission AddPermissionImpl(IPermission perm)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	protected override IEnumerator GetEnumeratorImpl()
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	protected override IPermission GetPermissionImpl(Type permClass)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	protected override IPermission RemovePermissionImpl(Type permClass)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}

	protected override IPermission SetPermissionImpl(IPermission perm)
	{
		ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
