using System;
using System.Runtime.InteropServices;

namespace WinRT.Interop;

[Guid("000001da-0000-0000-C000-000000000046")]
internal interface IContextCallback
{
	unsafe void ContextCallback(PFNCONTEXTCALL pfnCallback, ComCallData* pParam, Guid riid, int iMethod);
}
