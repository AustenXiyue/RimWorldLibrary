using Unity;

namespace System.Security.Permissions;

/// <summary>Determines the permission flags that apply to a <see cref="T:System.ComponentModel.TypeDescriptor" />.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TypeDescriptorPermissionAttribute : CodeAccessSecurityAttribute
{
	/// <summary>Gets or sets the <see cref="T:System.Security.Permissions.TypeDescriptorPermissionFlags" /> for the <see cref="T:System.ComponentModel.TypeDescriptor" />. </summary>
	/// <returns>The <see cref="T:System.Security.Permissions.TypeDescriptorPermissionFlags" /> for the <see cref="T:System.ComponentModel.TypeDescriptor" />.</returns>
	public TypeDescriptorPermissionFlags Flags
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(TypeDescriptorPermissionFlags);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Gets or sets a value that indicates whether the type descriptor can be accessed from partial trust. </summary>
	/// <returns>true if the type descriptor can be accessed from partial trust; otherwise, false. </returns>
	public bool RestrictedRegistrationAccess
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Permissions.TypeDescriptorPermissionAttribute" /> class with the specified <see cref="T:System.Security.Permissions.SecurityAction" />. </summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	public TypeDescriptorPermissionAttribute(SecurityAction action)
	{
	}

	/// <returns>A serializable permission object.</returns>
	public override IPermission CreatePermission()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
