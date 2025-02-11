using System;
using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("DF0B3D60-548F-101B-8E65-08002B2BD119")]
internal interface ISupportErrorInfo
{
	bool InterfaceSupportsErrorInfo(Guid riid);
}
