using System;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class EncryptedPackageFilter : IFilter
{
	private IFilter _filter;

	internal EncryptedPackageFilter(EncryptedPackageEnvelope encryptedPackage)
	{
		if (encryptedPackage == null)
		{
			throw new ArgumentNullException("encryptedPackage");
		}
		_filter = new IndexingFilterMarshaler(new CorePropertiesFilter(encryptedPackage.PackageProperties));
	}

	public IFILTER_FLAGS Init([In] IFILTER_INIT grfFlags, [In] uint cAttributes, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] FULLPROPSPEC[] aAttributes)
	{
		return _filter.Init(grfFlags, cAttributes, aAttributes);
	}

	public STAT_CHUNK GetChunk()
	{
		return _filter.GetChunk();
	}

	public void GetText(ref uint bufCharacterCount, nint pBuffer)
	{
		throw new COMException(SR.FilterGetTextNotSupported, -2147215611);
	}

	public nint GetValue()
	{
		return _filter.GetValue();
	}

	public nint BindRegion([In] FILTERREGION origPos, [In] ref Guid riid)
	{
		throw new NotImplementedException(SR.FilterBindRegionNotImplemented);
	}
}
