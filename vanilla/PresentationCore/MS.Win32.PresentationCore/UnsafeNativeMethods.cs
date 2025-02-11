using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;
using MS.Internal;

namespace MS.Win32.PresentationCore;

internal static class UnsafeNativeMethods
{
	internal static class MilCoreApi
	{
		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilCompositionEngine_EnterCompositionEngineLock")]
		internal static extern void EnterCompositionEngineLock();

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilCompositionEngine_ExitCompositionEngineLock")]
		internal static extern void ExitCompositionEngineLock();

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilCompositionEngine_EnterMediaSystemLock")]
		internal static extern void EnterMediaSystemLock();

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilCompositionEngine_ExitMediaSystemLock")]
		internal static extern void ExitMediaSystemLock();

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilVersionCheck(uint uiCallerMilSdkVersion);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern bool WgxConnection_ShouldForceSoftwareForGraphicsStreamClient();

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int WgxConnection_Create(bool requestSynchronousTransport, out nint ppConnection);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int WgxConnection_Disconnect(nint pTranspManager);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MILCreateStreamFromStreamDescriptor(ref StreamDescriptor pSD, out nint ppStream);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern void MilUtility_GetTileBrushMapping(D3DMATRIX* transform, D3DMATRIX* relativeTransform, Stretch stretch, AlignmentX alignmentX, AlignmentY alignmentY, BrushMappingMode viewPortUnits, BrushMappingMode viewBoxUnits, Rect* shapeFillBounds, Rect* contentBounds, ref Rect viewport, ref Rect viewbox, out D3DMATRIX contentToShape, out int brushIsEmpty);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilUtility_PathGeometryBounds(MIL_PEN_DATA* pPenData, double* pDashArray, MilMatrix3x2D* pWorldMatrix, FillRule fillRule, byte* pPathData, uint nSize, MilMatrix3x2D* pGeometryMatrix, double rTolerance, bool fRelative, bool fSkipHollows, MilRectD* pBounds);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilUtility_PathGeometryCombine(MilMatrix3x2D* pMatrix, MilMatrix3x2D* pMatrix1, FillRule fillRule1, byte* pPathData1, uint nSize1, MilMatrix3x2D* pMatrix2, FillRule fillRule2, byte* pPathData2, uint nSize2, double rTolerance, bool fRelative, PathGeometry.AddFigureToListDelegate addFigureCallback, GeometryCombineMode combineMode, out FillRule resultFillRule);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilUtility_PathGeometryWiden(MIL_PEN_DATA* pPenData, double* pDashArray, MilMatrix3x2D* pMatrix, FillRule fillRule, byte* pPathData, uint nSize, double rTolerance, bool fRelative, PathGeometry.AddFigureToListDelegate addFigureCallback, out FillRule widenedFillRule);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilUtility_PathGeometryOutline(MilMatrix3x2D* pMatrix, FillRule fillRule, byte* pPathData, uint nSize, double rTolerance, bool fRelative, PathGeometry.AddFigureToListDelegate addFigureCallback, out FillRule outlinedFillRule);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilUtility_PathGeometryFlatten(MilMatrix3x2D* pMatrix, FillRule fillRule, byte* pPathData, uint nSize, double rTolerance, bool fRelative, PathGeometry.AddFigureToListDelegate addFigureCallback, out FillRule resultFillRule);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilGlyphCache_BeginCommandAtRenderTime(nint pMilSlaveGlyphCacheTarget, byte* pbData, uint cbSize, uint cbExtra);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilGlyphCache_AppendCommandDataAtRenderTime(nint pMilSlaveGlyphCacheTarget, byte* pbData, uint cbSize);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilGlyphCache_EndCommandAtRenderTime(nint pMilSlaveGlyphCacheTarget);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilGlyphRun_SetGeometryAtRenderTime(nint pMilGlyphRunTarget, byte* pCmd, uint cbCmd);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilGlyphRun_GetGlyphOutline(nint pFontFace, ushort glyphIndex, bool sideways, double renderingEmSize, out byte* pPathGeometryData, out uint pSize, out FillRule pFillRule);

		[DllImport("wpfgfx_cor3.dll")]
		internal unsafe static extern int MilGlyphRun_ReleasePathGeometryData(byte* pPathGeometryData);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern int MilCreateReversePInvokeWrapper(nint pFcn, out nint reversePInvokeWrapper);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern void MilReleasePInvokePtrBlocking(nint reversePInvokeWrapper);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern void RenderOptions_ForceSoftwareRenderingModeForProcess(bool fForce);

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern bool RenderOptions_IsSoftwareRenderingForcedForProcess();

		[DllImport("wpfgfx_cor3.dll")]
		internal static extern void RenderOptions_EnableHardwareAccelerationInRdp(bool value);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MilResource_CreateCWICWrapperBitmap")]
		internal static extern int CreateCWICWrapperBitmap(BitmapSourceSafeMILHandle pIWICBitmapSource, out BitmapSourceSafeMILHandle pCWICWrapperBitmap);
	}

	internal static class WICComponentInfo
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentInfo_GetCLSID_Proxy")]
		internal static extern int GetCLSID(SafeMILHandle THIS_PTR, out Guid pclsid);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentInfo_GetAuthor_Proxy")]
		internal static extern int GetAuthor(SafeMILHandle THIS_PTR, uint cchAuthor, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzAuthor, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentInfo_GetVersion_Proxy")]
		internal static extern int GetVersion(SafeMILHandle THIS_PTR, uint cchVersion, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzVersion, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentInfo_GetSpecVersion_Proxy")]
		internal static extern int GetSpecVersion(SafeMILHandle THIS_PTR, uint cchSpecVersion, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzSpecVersion, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentInfo_GetFriendlyName_Proxy")]
		internal static extern int GetFriendlyName(SafeMILHandle THIS_PTR, uint cchFriendlyName, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFriendlyName, out uint pcchActual);
	}

	internal static class WICBitmapCodecInfo
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_GetContainerFormat_Proxy")]
		internal static extern int GetContainerFormat(SafeMILHandle THIS_PTR, out Guid pguidContainerFormat);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_GetDeviceManufacturer_Proxy")]
		internal static extern int GetDeviceManufacturer(SafeMILHandle THIS_PTR, uint cchDeviceManufacturer, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceManufacturer, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_GetDeviceModels_Proxy")]
		internal static extern int GetDeviceModels(SafeMILHandle THIS_PTR, uint cchDeviceModels, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzDeviceModels, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_GetMimeTypes_Proxy")]
		internal static extern int GetMimeTypes(SafeMILHandle THIS_PTR, uint cchMimeTypes, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzMimeTypes, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_GetFileExtensions_Proxy")]
		internal static extern int GetFileExtensions(SafeMILHandle THIS_PTR, uint cchFileExtensions, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzFileExtensions, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_DoesSupportAnimation_Proxy")]
		internal static extern int DoesSupportAnimation(SafeMILHandle THIS_PTR, out bool pfSupportAnimation);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_DoesSupportLossless_Proxy")]
		internal static extern int DoesSupportLossless(SafeMILHandle THIS_PTR, out bool pfSupportLossless);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapCodecInfo_DoesSupportMultiframe_Proxy")]
		internal static extern int DoesSupportMultiframe(SafeMILHandle THIS_PTR, out bool pfSupportMultiframe);
	}

	internal static class WICMetadataQueryReader
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryReader_GetContainerFormat_Proxy")]
		internal static extern int GetContainerFormat(SafeMILHandle THIS_PTR, out Guid pguidContainerFormat);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryReader_GetLocation_Proxy")]
		internal static extern int GetLocation(SafeMILHandle THIS_PTR, uint cchLocation, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzNamespace, out uint pcchActual);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryReader_GetMetadataByName_Proxy")]
		internal static extern int GetMetadataByName(SafeMILHandle THIS_PTR, [MarshalAs(UnmanagedType.LPWStr)] string wzName, ref PROPVARIANT propValue);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryReader_GetMetadataByName_Proxy")]
		internal static extern int ContainsMetadataByName(SafeMILHandle THIS_PTR, [MarshalAs(UnmanagedType.LPWStr)] string wzName, nint propVar);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryReader_GetEnumerator_Proxy")]
		internal static extern int GetEnumerator(SafeMILHandle THIS_PTR, out SafeMILHandle enumString);
	}

	internal static class WICMetadataQueryWriter
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryWriter_SetMetadataByName_Proxy")]
		internal static extern int SetMetadataByName(SafeMILHandle THIS_PTR, [MarshalAs(UnmanagedType.LPWStr)] string wzName, ref PROPVARIANT propValue);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataQueryWriter_RemoveMetadataByName_Proxy")]
		internal static extern int RemoveMetadataByName(SafeMILHandle THIS_PTR, [MarshalAs(UnmanagedType.LPWStr)] string wzName);
	}

	internal static class WICFastMetadataEncoder
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICFastMetadataEncoder_Commit_Proxy")]
		internal static extern int Commit(SafeMILHandle THIS_PTR);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICFastMetadataEncoder_GetMetadataQueryWriter_Proxy")]
		internal static extern int GetMetadataQueryWriter(SafeMILHandle THIS_PTR, out SafeMILHandle ppIQueryWriter);
	}

	internal static class EnumString
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IEnumString_Next_WIC_Proxy")]
		internal static extern int Next(SafeMILHandle THIS_PTR, int celt, ref nint rgElt, ref int pceltFetched);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IEnumString_Reset_WIC_Proxy")]
		internal static extern int Reset(SafeMILHandle THIS_PTR);
	}

	internal static class IPropertyBag2
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IPropertyBag2_Write_Proxy")]
		internal static extern int Write(SafeMILHandle THIS_PTR, uint cProperties, ref PROPBAG2 propBag, ref PROPVARIANT propValue);
	}

	internal static class WICBitmapSource
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapSource_GetSize_Proxy")]
		internal static extern int GetSize(SafeMILHandle THIS_PTR, out uint puiWidth, out uint puiHeight);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapSource_GetPixelFormat_Proxy")]
		internal static extern int GetPixelFormat(SafeMILHandle THIS_PTR, out Guid pPixelFormatEnum);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapSource_GetResolution_Proxy")]
		internal static extern int GetResolution(SafeMILHandle THIS_PTR, out double pDpiX, out double pDpiY);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapSource_CopyPalette_Proxy")]
		internal static extern int CopyPalette(SafeMILHandle THIS_PTR, SafeMILHandle pIPalette);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapSource_CopyPixels_Proxy")]
		internal static extern int CopyPixels(SafeMILHandle THIS_PTR, ref Int32Rect prc, uint cbStride, uint cbBufferSize, nint pvPixels);
	}

	internal static class WICBitmapDecoder
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetDecoderInfo_Proxy")]
		internal static extern int GetDecoderInfo(SafeMILHandle THIS_PTR, out SafeMILHandle ppIDecoderInfo);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_CopyPalette_Proxy")]
		internal static extern int CopyPalette(SafeMILHandle THIS_PTR, SafeMILHandle pIPalette);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetPreview_Proxy")]
		internal static extern int GetPreview(SafeMILHandle THIS_PTR, out nint ppIBitmapSource);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetColorContexts_Proxy")]
		internal static extern int GetColorContexts(SafeMILHandle THIS_PTR, uint count, nint[] ppIColorContext, out uint pActualCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetThumbnail_Proxy")]
		internal static extern int GetThumbnail(SafeMILHandle THIS_PTR, out nint ppIThumbnail);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetMetadataQueryReader_Proxy")]
		internal static extern int GetMetadataQueryReader(SafeMILHandle THIS_PTR, out nint ppIQueryReader);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetFrameCount_Proxy")]
		internal static extern int GetFrameCount(SafeMILHandle THIS_PTR, out uint pFrameCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapDecoder_GetFrame_Proxy")]
		internal static extern int GetFrame(SafeMILHandle THIS_PTR, uint index, out nint ppIFrameDecode);
	}

	internal static class WICBitmapFrameDecode
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameDecode_GetThumbnail_Proxy")]
		internal static extern int GetThumbnail(SafeMILHandle THIS_PTR, out nint ppIThumbnail);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameDecode_GetMetadataQueryReader_Proxy")]
		internal static extern int GetMetadataQueryReader(SafeMILHandle THIS_PTR, out nint ppIQueryReader);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameDecode_GetColorContexts_Proxy")]
		internal static extern int GetColorContexts(SafeMILHandle THIS_PTR, uint count, nint[] ppIColorContext, out uint pActualCount);
	}

	internal static class MILUnknown
	{
		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILAddRef")]
		internal static extern uint AddRef(SafeMILHandle pIUnkown);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILAddRef")]
		internal static extern uint AddRef(SafeReversePInvokeWrapper pIUnknown);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILRelease")]
		internal static extern int Release(nint pIUnkown);

		internal static void ReleaseInterface(ref nint ptr)
		{
			if (ptr != IntPtr.Zero)
			{
				Release(ptr);
				ptr = IntPtr.Zero;
			}
		}

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILQueryInterface")]
		internal static extern int QueryInterface(nint pIUnknown, ref Guid guid, out nint ppvObject);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILQueryInterface")]
		internal static extern int QueryInterface(SafeMILHandle pIUnknown, ref Guid guid, out nint ppvObject);
	}

	internal static class WICStream
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICStream_InitializeFromIStream_Proxy")]
		internal static extern int InitializeFromIStream(nint pIWICStream, nint pIStream);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICStream_InitializeFromMemory_Proxy")]
		internal static extern int InitializeFromMemory(nint pIWICStream, nint pbBuffer, uint cbSize);
	}

	internal static class WindowsCodecApi
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "WICCreateBitmapFromSection")]
		internal static extern int CreateBitmapFromSection(uint width, uint height, ref Guid pixelFormatGuid, nint hSection, uint stride, uint offset, out BitmapSourceSafeMILHandle ppIBitmap);
	}

	internal static class WICBitmapFrameEncode
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle pIEncoderOptions);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_Commit_Proxy")]
		internal static extern int Commit(SafeMILHandle THIS_PTR);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_SetSize_Proxy")]
		internal static extern int SetSize(SafeMILHandle THIS_PTR, int width, int height);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_SetResolution_Proxy")]
		internal static extern int SetResolution(SafeMILHandle THIS_PTR, double dpiX, double dpiY);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_WriteSource_Proxy")]
		internal static extern int WriteSource(SafeMILHandle THIS_PTR, SafeMILHandle pIBitmapSource, ref Int32Rect r);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_SetThumbnail_Proxy")]
		internal static extern int SetThumbnail(SafeMILHandle THIS_PTR, SafeMILHandle pIThumbnail);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_GetMetadataQueryWriter_Proxy")]
		internal static extern int GetMetadataQueryWriter(SafeMILHandle THIS_PTR, out SafeMILHandle ppIQueryWriter);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFrameEncode_SetColorContexts_Proxy")]
		internal static extern int SetColorContexts(SafeMILHandle THIS_PTR, uint nIndex, nint[] ppIColorContext);
	}

	internal static class WICBitmapEncoder
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, nint pStream, WICBitmapEncodeCacheOption option);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_GetEncoderInfo_Proxy")]
		internal static extern int GetEncoderInfo(SafeMILHandle THIS_PTR, out SafeMILHandle ppIEncoderInfo);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_CreateNewFrame_Proxy")]
		internal static extern int CreateNewFrame(SafeMILHandle THIS_PTR, out SafeMILHandle ppIFramEncode, out SafeMILHandle ppIEncoderOptions);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_SetThumbnail_Proxy")]
		internal static extern int SetThumbnail(SafeMILHandle THIS_PTR, SafeMILHandle pIThumbnail);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_SetPalette_Proxy")]
		internal static extern int SetPalette(SafeMILHandle THIS_PTR, SafeMILHandle pIPalette);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_GetMetadataQueryWriter_Proxy")]
		internal static extern int GetMetadataQueryWriter(SafeMILHandle THIS_PTR, out SafeMILHandle ppIQueryWriter);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapEncoder_Commit_Proxy")]
		internal static extern int Commit(SafeMILHandle THIS_PTR);
	}

	internal static class WICPalette
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_InitializePredefined_Proxy")]
		internal static extern int InitializePredefined(SafeMILHandle THIS_PTR, WICPaletteType ePaletteType, bool fAddTransparentColor);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_InitializeCustom_Proxy")]
		internal static extern int InitializeCustom(SafeMILHandle THIS_PTR, nint pColors, int colorCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_InitializeFromBitmap_Proxy")]
		internal static extern int InitializeFromBitmap(SafeMILHandle THIS_PTR, SafeMILHandle pISurface, int colorCount, bool fAddTransparentColor);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_InitializeFromPalette_Proxy")]
		internal static extern int InitializeFromPalette(nint THIS_PTR, SafeMILHandle pIWICPalette);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_GetType_Proxy")]
		internal static extern int GetType(SafeMILHandle THIS_PTR, out WICPaletteType pePaletteType);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_GetColorCount_Proxy")]
		internal static extern int GetColorCount(SafeMILHandle THIS_PTR, out int pColorCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_GetColors_Proxy")]
		internal static extern int GetColors(SafeMILHandle THIS_PTR, int colorCount, nint pColors, out int pcActualCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPalette_HasAlpha_Proxy")]
		internal static extern int HasAlpha(SafeMILHandle THIS_PTR, out bool pfHasAlpha);
	}

	internal static class WICImagingFactory
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateDecoderFromStream_Proxy")]
		internal static extern int CreateDecoderFromStream(nint pICodecFactory, nint pIStream, ref Guid guidVendor, uint metadataFlags, out nint ppIDecode);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateDecoderFromFileHandle_Proxy")]
		internal static extern int CreateDecoderFromFileHandle(nint pICodecFactory, SafeFileHandle hFileHandle, ref Guid guidVendor, uint metadataFlags, out nint ppIDecode);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateComponentInfo_Proxy")]
		internal static extern int CreateComponentInfo(nint pICodecFactory, ref Guid clsidComponent, out nint ppIComponentInfo);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreatePalette_Proxy")]
		internal static extern int CreatePalette(nint pICodecFactory, out SafeMILHandle ppIPalette);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateFormatConverter_Proxy")]
		internal static extern int CreateFormatConverter(nint pICodecFactory, out BitmapSourceSafeMILHandle ppFormatConverter);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapScaler_Proxy")]
		internal static extern int CreateBitmapScaler(nint pICodecFactory, out BitmapSourceSafeMILHandle ppBitmapScaler);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapClipper_Proxy")]
		internal static extern int CreateBitmapClipper(nint pICodecFactory, out BitmapSourceSafeMILHandle ppBitmapClipper);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapFlipRotator_Proxy")]
		internal static extern int CreateBitmapFlipRotator(nint pICodecFactory, out BitmapSourceSafeMILHandle ppBitmapFlipRotator);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateStream_Proxy")]
		internal static extern int CreateStream(nint pICodecFactory, out nint ppIStream);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateEncoder_Proxy")]
		internal static extern int CreateEncoder(nint pICodecFactory, ref Guid guidContainerFormat, ref Guid guidVendor, out SafeMILHandle ppICodec);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapFromSource_Proxy")]
		internal static extern int CreateBitmapFromSource(nint THIS_PTR, SafeMILHandle pIBitmapSource, WICBitmapCreateCacheOptions options, out BitmapSourceSafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapFromMemory_Proxy")]
		internal static extern int CreateBitmapFromMemory(nint THIS_PTR, uint width, uint height, ref Guid pixelFormatGuid, uint stride, uint cbBufferSize, nint pvPixels, out BitmapSourceSafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmap_Proxy")]
		internal static extern int CreateBitmap(nint THIS_PTR, uint width, uint height, ref Guid pixelFormatGuid, WICBitmapCreateCacheOptions options, out BitmapSourceSafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapFromHBITMAP_Proxy")]
		internal static extern int CreateBitmapFromHBITMAP(nint THIS_PTR, nint hBitmap, nint hPalette, WICBitmapAlphaChannelOption options, out BitmapSourceSafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateBitmapFromHICON_Proxy")]
		internal static extern int CreateBitmapFromHICON(nint THIS_PTR, nint hIcon, out BitmapSourceSafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateFastMetadataEncoderFromDecoder_Proxy")]
		internal static extern int CreateFastMetadataEncoderFromDecoder(nint THIS_PTR, SafeMILHandle pIDecoder, out SafeMILHandle ppIFME);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateFastMetadataEncoderFromFrameDecode_Proxy")]
		internal static extern int CreateFastMetadataEncoderFromFrameDecode(nint THIS_PTR, BitmapSourceSafeMILHandle pIFrameDecode, out SafeMILHandle ppIBitmap);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateQueryWriter_Proxy")]
		internal static extern int CreateQueryWriter(nint THIS_PTR, ref Guid metadataFormat, ref Guid guidVendor, out nint queryWriter);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICImagingFactory_CreateQueryWriterFromReader_Proxy")]
		internal static extern int CreateQueryWriterFromReader(nint THIS_PTR, SafeMILHandle queryReader, ref Guid guidVendor, out nint queryWriter);
	}

	internal static class WICComponentFactory
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentFactory_CreateMetadataWriterFromReader_Proxy")]
		internal static extern int CreateMetadataWriterFromReader(nint pICodecFactory, SafeMILHandle pIMetadataReader, ref Guid guidVendor, out nint metadataWriter);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICComponentFactory_CreateQueryWriterFromBlockWriter_Proxy")]
		internal static extern int CreateQueryWriterFromBlockWriter(nint pICodecFactory, nint pIBlockWriter, ref nint ppIQueryWriter);
	}

	internal static class WICMetadataBlockReader
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataBlockReader_GetCount_Proxy")]
		internal static extern int GetCount(nint pIBlockReader, out uint count);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICMetadataBlockReader_GetReaderByIndex_Proxy")]
		internal static extern int GetReaderByIndex(nint pIBlockReader, uint index, out SafeMILHandle pIMetadataReader);
	}

	internal static class WICPixelFormatInfo
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPixelFormatInfo_GetBitsPerPixel_Proxy")]
		internal static extern int GetBitsPerPixel(nint pIPixelFormatInfo, out uint uiBitsPerPixel);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPixelFormatInfo_GetChannelCount_Proxy")]
		internal static extern int GetChannelCount(nint pIPixelFormatInfo, out uint uiChannelCount);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICPixelFormatInfo_GetChannelMask_Proxy")]
		internal unsafe static extern int GetChannelMask(nint pIPixelFormatInfo, uint uiChannelIndex, uint cbMaskBuffer, byte* pbMaskBuffer, out uint cbActual);
	}

	internal static class WICBitmapClipper
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapClipper_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle source, ref Int32Rect prc);
	}

	internal static class WICBitmapFlipRotator
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapFlipRotator_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle source, WICBitmapTransformOptions options);
	}

	internal static class WICBitmapScaler
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapScaler_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle source, uint width, uint height, WICInterpolationMode mode);
	}

	internal static class WICFormatConverter
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICFormatConverter_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle source, ref Guid dstFormat, DitherType dither, SafeMILHandle bitmapPalette, double alphaThreshold, WICPaletteType paletteTranslate);
	}

	internal static class IWICColorContext
	{
		internal enum WICColorContextType : uint
		{
			WICColorContextUninitialized,
			WICColorContextProfile,
			WICColorContextExifColorSpace
		}

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICColorContext_InitializeFromMemory_Proxy")]
		internal static extern int InitializeFromMemory(SafeMILHandle THIS_PTR, byte[] pbBuffer, uint cbBufferSize);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "IWICColorContext_GetProfileBytes_Proxy")]
		internal static extern int GetProfileBytes(SafeMILHandle THIS_PTR, uint cbBuffer, byte[] pbBuffer, out uint pcbActual);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "IWICColorContext_GetType_Proxy")]
		internal static extern int GetType(SafeMILHandle THIS_PTR, out WICColorContextType pType);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "IWICColorContext_GetExifColorSpace_Proxy")]
		internal static extern int GetExifColorSpace(SafeMILHandle THIS_PTR, out uint pValue);
	}

	internal static class WICColorTransform
	{
		[DllImport("WindowsCodecsExt.dll", EntryPoint = "IWICColorTransform_Initialize_Proxy")]
		internal static extern int Initialize(SafeMILHandle THIS_PTR, SafeMILHandle source, SafeMILHandle pIContextSource, SafeMILHandle pIContextDest, ref Guid pixelFmtDest);
	}

	internal static class WICBitmap
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmap_Lock_Proxy")]
		internal static extern int Lock(SafeMILHandle THIS_PTR, ref Int32Rect prcLock, LockFlags flags, out SafeMILHandle ppILock);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmap_SetResolution_Proxy")]
		internal static extern int SetResolution(SafeMILHandle THIS_PTR, double dpiX, double dpiY);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmap_SetPalette_Proxy")]
		internal static extern int SetPalette(SafeMILHandle THIS_PTR, SafeMILHandle pIPalette);
	}

	internal static class WICBitmapLock
	{
		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapLock_GetStride_Proxy")]
		internal static extern int GetStride(SafeMILHandle pILock, ref uint pcbStride);

		[DllImport("WindowsCodecs.dll", EntryPoint = "IWICBitmapLock_GetDataPointer_STA_Proxy")]
		internal static extern int GetDataPointer(SafeMILHandle pILock, ref uint pcbBufferSize, ref nint ppbData);
	}

	internal static class WICCodec
	{
		internal const int WINCODEC_SDK_VERSION = 566;

		[DllImport("WindowsCodecs.dll", EntryPoint = "WICCreateImagingFactory_Proxy")]
		internal static extern int CreateImagingFactory(uint SDKVersion, out nint ppICodecFactory);

		[DllImport("WindowsCodecs.dll")]
		internal static extern int WICConvertBitmapSource(ref Guid dstPixelFormatGuid, SafeMILHandle pISrc, out BitmapSourceSafeMILHandle ppIDst);

		[DllImport("WindowsCodecs.dll", EntryPoint = "WICSetEncoderFormat_Proxy")]
		internal static extern int WICSetEncoderFormat(SafeMILHandle pSourceIn, SafeMILHandle pIPalette, SafeMILHandle pIFrameEncode, out SafeMILHandle ppSourceOut);

		[DllImport("WindowsCodecs.dll")]
		internal static extern int WICMapGuidToShortName(ref Guid guid, uint cchName, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder wzName, ref uint pcchActual);

		[DllImport("WindowsCodecs.dll")]
		internal static extern int WICMapShortNameToGuid([MarshalAs(UnmanagedType.LPWStr)] string wzName, ref Guid guid);

		[DllImport("WindowsCodecsExt.dll", EntryPoint = "WICCreateColorTransform_Proxy")]
		internal static extern int CreateColorTransform(out BitmapSourceSafeMILHandle ppWICColorTransform);

		[DllImport("WindowsCodecs.dll", EntryPoint = "WICCreateColorContext_Proxy")]
		internal static extern int CreateColorContext(nint pICodecFactory, out SafeMILHandle ppColorContext);

		[DllImport("ole32.dll")]
		internal static extern int CoInitialize(nint reserved);

		[DllImport("ole32.dll")]
		internal static extern void CoUninitialize();
	}

	internal static class Mscms
	{
		[DllImport("mscms.dll")]
		internal static extern ColorTransformHandle CreateMultiProfileTransform(nint[] pahProfiles, uint nProfiles, uint[] padwIntent, uint nIntents, uint dwFlags, uint indexPreferredCMM);

		[DllImport("mscms.dll", SetLastError = true)]
		internal static extern bool DeleteColorTransform(nint hColorTransform);

		[DllImport("mscms.dll")]
		internal static extern int TranslateColors(ColorTransformHandle hColorTransform, nint paInputColors, uint nColors, uint ctInput, nint paOutputColors, uint ctOutput);

		[DllImport("mscms.dll")]
		internal static extern SafeProfileHandle OpenColorProfile(ref MS.Win32.UnsafeNativeMethods.PROFILE pProfile, uint dwDesiredAccess, uint dwShareMode, uint dwCreationMode);

		[DllImport("mscms.dll", SetLastError = true)]
		internal static extern bool CloseColorProfile(nint phProfile);

		[DllImport("mscms.dll", SetLastError = true)]
		internal static extern bool GetColorProfileHeader(SafeProfileHandle phProfile, out MS.Win32.UnsafeNativeMethods.PROFILEHEADER pHeader);

		[DllImport("mscms.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int GetColorDirectory(nint pMachineName, StringBuilder pBuffer, out uint pdwSize);

		[DllImport("mscms.dll", BestFitMapping = false, CharSet = CharSet.Auto)]
		internal static extern int GetStandardColorSpaceProfile(nint pMachineName, uint dwProfileID, StringBuilder pProfileName, out uint pdwSize);

		[DllImport("mscms.dll", SetLastError = true)]
		internal static extern bool GetColorProfileFromHandle(SafeProfileHandle hProfile, byte[] pBuffer, ref uint pdwSize);
	}

	internal static class MILFactory2
	{
		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILCreateFactory")]
		internal static extern int CreateFactory(out nint ppIFactory, uint SDKVersion);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILFactoryCreateMediaPlayer")]
		internal static extern int CreateMediaPlayer(nint THIS_PTR, SafeMILHandle pEventProxy, bool canOpenAllMedia, out SafeMediaHandle ppMedia);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILFactoryCreateBitmapRenderTarget")]
		internal static extern int CreateBitmapRenderTarget(nint THIS_PTR, uint width, uint height, PixelFormatEnum pixelFormatEnum, float dpiX, float dpiY, MILRTInitializationFlags dwFlags, out SafeMILHandle ppIRenderTargetBitmap);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "MILFactoryCreateSWRenderTargetForBitmap")]
		internal static extern int CreateBitmapRenderTargetForBitmap(nint THIS_PTR, BitmapSourceSafeMILHandle pIBitmap, out SafeMILHandle ppIRenderTargetBitmap);
	}

	internal static class InteropDeviceBitmap
	{
		internal delegate void FrontBufferAvailableCallback(bool lost, uint version);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "InteropDeviceBitmap_Create")]
		internal static extern int Create(nint d3dResource, double dpiX, double dpiY, uint version, FrontBufferAvailableCallback pfnCallback, bool isSoftwareFallbackEnabled, out SafeMILHandle ppInteropDeviceBitmap, out uint pixelWidth, out uint pixelHeight);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "InteropDeviceBitmap_Detach")]
		internal static extern void Detach(SafeMILHandle pInteropDeviceBitmap);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "InteropDeviceBitmap_AddDirtyRect")]
		internal static extern int AddDirtyRect(int x, int y, int w, int h, SafeMILHandle pInteropDeviceBitmap);

		[DllImport("wpfgfx_cor3.dll", EntryPoint = "InteropDeviceBitmap_GetAsSoftwareBitmap")]
		internal static extern int GetAsSoftwareBitmap(SafeMILHandle pInteropDeviceBitmap, out BitmapSourceSafeMILHandle pIWICBitmapSource);
	}
}
