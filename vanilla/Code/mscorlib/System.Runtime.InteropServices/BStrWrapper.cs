using System.Security;
using System.Security.Permissions;

namespace System.Runtime.InteropServices;

/// <summary>Marshals data of type VT_BSTR from managed to unmanaged code. This class cannot be inherited.</summary>
[Serializable]
[ComVisible(true)]
public sealed class BStrWrapper
{
	private string m_WrappedObject;

	/// <summary>Gets the wrapped <see cref="T:System.String" /> object to marshal as type VT_BSTR.</summary>
	/// <returns>The object that is wrapped by <see cref="T:System.Runtime.InteropServices.BStrWrapper" />.</returns>
	public string WrappedObject => m_WrappedObject;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.BStrWrapper" /> class with the specified <see cref="T:System.String" /> object.</summary>
	/// <param name="value">The object to wrap and marshal as VT_BSTR.</param>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public BStrWrapper(string value)
	{
		m_WrappedObject = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.BStrWrapper" /> class with the specified <see cref="T:System.Object" /> object.</summary>
	/// <param name="value">The object to wrap and marshal as VT_BSTR.</param>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public BStrWrapper(object value)
	{
		m_WrappedObject = (string)value;
	}
}
