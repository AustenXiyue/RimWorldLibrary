using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Interop;

[ComImport]
[ComVisible(true)]
[Guid("89BCB740-6119-101A-BCB7-00DD010655AF")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IFilter
{
	IFILTER_FLAGS Init([In] IFILTER_INIT grfFlags, [In] uint cAttributes, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] FULLPROPSPEC[] aAttributes);

	STAT_CHUNK GetChunk();

	void GetText([In][Out] ref uint pcwcBuffer, [In] nint pBuffer);

	nint GetValue();

	nint BindRegion([In] FILTERREGION origPos, [In] ref Guid riid);
}
