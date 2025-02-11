using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("82BA7092-4C88-427D-A7BC-16DD93FEB67E")]
internal interface IRestrictedErrorInfo
{
	void GetErrorDetails(out string description, out int error, out string restrictedDescription, out string capabilitySid);

	string GetReference();
}
