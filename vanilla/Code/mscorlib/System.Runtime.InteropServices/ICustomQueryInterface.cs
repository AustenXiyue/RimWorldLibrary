using System.Security;

namespace System.Runtime.InteropServices;

/// <summary>Enables developers to provide a custom, managed implementation of the IUnknown::QueryInterface(REFIID riid, void **ppvObject) method.</summary>
[ComVisible(false)]
public interface ICustomQueryInterface
{
	/// <summary>Returns an interface according to a specified interface ID.</summary>
	/// <returns>One of the enumeration values that indicates whether a custom implementation of IUnknown::QueryInterface was used.</returns>
	/// <param name="iid">The GUID of the requested interface.</param>
	/// <param name="ppv">A reference to the requested interface, when this method returns.</param>
	[SecurityCritical]
	CustomQueryInterfaceResult GetInterface([In] ref Guid iid, out IntPtr ppv);
}
