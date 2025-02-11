using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct HRESULT
{
	internal const int FACILITY_NT_BIT = 268435456;

	internal const int FACILITY_MASK = 2147418112;

	internal const int FACILITY_WINCODEC_ERROR = 144179200;

	internal const int COMPONENT_MASK = 57344;

	internal const int COMPONENT_WINCODEC_ERROR = 8192;

	internal const int S_OK = 0;

	internal const int E_FAIL = -2147467259;

	internal const int E_OUTOFMEMORY = -2147024882;

	internal const int D3DERR_OUTOFVIDEOMEMORY = -2005532292;

	internal static bool IsWindowsCodecError(int hr)
	{
		return (hr & 0x7FFFE000) == 144187392;
	}

	internal static Exception ConvertHRToException(int hr)
	{
		Exception exceptionForHR = Marshal.GetExceptionForHR(hr, -1);
		if ((hr & 0x10000000) == 268435456)
		{
			if ((hr & -268435457) == -1073741801)
			{
				return new OutOfMemoryException();
			}
			return exceptionForHR;
		}
		switch (hr)
		{
		case -1073741801:
			return new OutOfMemoryException();
		case -2003292412:
			return new InvalidOperationException(SR.Image_WrongState, exceptionForHR);
		case -2147024362:
		case -2003292411:
			return new OverflowException(SR.Image_Overflow, exceptionForHR);
		case -2003292409:
			return new FileFormatException(null, SR.Image_UnknownFormat, exceptionForHR);
		case -2003292405:
			return new FileLoadException(SR.MilErr_UnsupportedVersion, exceptionForHR);
		case -2003292404:
			return new InvalidOperationException(SR.WIC_NotInitialized, exceptionForHR);
		case -2003292352:
			return new ArgumentException(SR.Image_PropertyNotFound, exceptionForHR);
		case -2003292351:
			return new NotSupportedException(SR.Image_PropertyNotSupported, exceptionForHR);
		case -2003292350:
			return new ArgumentException(SR.Image_PropertySize, exceptionForHR);
		case -2003292349:
			return new InvalidOperationException(SR.Image_CodecPresent, exceptionForHR);
		case -2003292348:
			return new NotSupportedException(SR.Image_NoThumbnail, exceptionForHR);
		case -2003292347:
			return new InvalidOperationException(SR.Image_NoPalette, exceptionForHR);
		case -2003292346:
			return new ArgumentException(SR.Image_TooManyScanlines, exceptionForHR);
		case -2003292344:
			return new InvalidOperationException(SR.Image_InternalError, exceptionForHR);
		case -2003292343:
			return new ArgumentException(SR.Image_BadDimensions, exceptionForHR);
		case -2003292336:
		case -2003292277:
			return new NotSupportedException(SR.Image_ComponentNotFound, exceptionForHR);
		case -2003292320:
		case -2003292273:
			return new FileFormatException(null, SR.Image_DecoderError, exceptionForHR);
		case -2003292319:
			return new FileFormatException(null, SR.Image_HeaderError, exceptionForHR);
		case -2003292318:
			return new ArgumentException(SR.Image_FrameMissing, exceptionForHR);
		case -2003292317:
			return new ArgumentException(SR.Image_BadMetadataHeader, exceptionForHR);
		case -2003292304:
			return new ArgumentException(SR.Image_BadStreamData, exceptionForHR);
		case -2003292303:
			return new InvalidOperationException(SR.Image_StreamWrite, exceptionForHR);
		case -2003292288:
			return new NotSupportedException(SR.Image_UnsupportedPixelFormat, exceptionForHR);
		case -2003292287:
			return new NotSupportedException(SR.Image_UnsupportedOperation, exceptionForHR);
		case -2003292335:
			return new ArgumentException(SR.Image_SizeOutOfRange, exceptionForHR);
		case -2003292302:
			return new IOException(SR.Image_StreamRead, exceptionForHR);
		case -2003292272:
			return new IOException(SR.Image_InvalidQueryRequest, exceptionForHR);
		case -2003292271:
			return new FileFormatException(null, SR.Image_UnexpectedMetadataType, exceptionForHR);
		case -2003292270:
			return new FileFormatException(null, SR.Image_RequestOnlyValidAtMetadataRoot, exceptionForHR);
		case -2003292269:
			return new IOException(SR.Image_InvalidQueryCharacter, exceptionForHR);
		case -2003292275:
			return new FileFormatException(null, SR.Image_DuplicateMetadataPresent, exceptionForHR);
		case -2003292274:
			return new FileFormatException(null, SR.Image_PropertyUnexpectedType, exceptionForHR);
		case -2003292334:
			return new FileFormatException(null, SR.Image_TooMuchMetadata, exceptionForHR);
		case -2003292301:
			return new NotSupportedException(SR.Image_StreamNotAvailable, exceptionForHR);
		case -2003292276:
			return new ArgumentException(SR.Image_InsufficientBuffer, exceptionForHR);
		case -2147024809:
			return new ArgumentException(SR.Format(SR.Media_InvalidArgument, null), exceptionForHR);
		case -2147022885:
			return new FileFormatException(null, SR.Image_InvalidColorContext, exceptionForHR);
		case -2003304442:
			return new InvalidOperationException(SR.Image_DisplayStateInvalid, exceptionForHR);
		case -2003304441:
			return new ArithmeticException(SR.Image_SingularMatrix, exceptionForHR);
		case -2003303161:
			return new InvalidWmpVersionException(SR.Format(SR.Media_InvalidWmpVersion, null), exceptionForHR);
		case -2003303160:
			return new NotSupportedException(SR.Format(SR.Media_InsufficientVideoResources, null), exceptionForHR);
		case -2003303159:
			return new NotSupportedException(SR.Format(SR.Media_HardwareVideoAccelerationNotAvailable, null), exceptionForHR);
		case -2003303155:
			return new NotSupportedException(SR.Format(SR.Media_PlayerIsClosed, null), exceptionForHR);
		case -1072885782:
			return new FileNotFoundException(SR.Format(SR.Media_DownloadFailed, null), exceptionForHR);
		case -1072885354:
			return new SecurityException(SR.Media_LogonFailure, exceptionForHR);
		case -1072885353:
			return new FileNotFoundException(SR.Media_FileNotFound, exceptionForHR);
		case -1072885351:
		case -1072885350:
			return new FileFormatException(SR.Media_FileFormatNotSupported, exceptionForHR);
		case -1072885347:
			return new FileFormatException(SR.Media_PlaylistFormatNotSupported, exceptionForHR);
		case -2003304438:
			return new ArithmeticException(SR.Geometry_BadNumber, exceptionForHR);
		case -2003302400:
			return new ArgumentException(SR.D3DImage_InvalidUsage, exceptionForHR);
		case -2003302399:
			return new ArgumentException(SR.D3DImage_SurfaceTooBig, exceptionForHR);
		case -2003302398:
			return new ArgumentException(SR.D3DImage_InvalidPool, exceptionForHR);
		case -2003302397:
			return new ArgumentException(SR.D3DImage_InvalidDevice, exceptionForHR);
		case -2003302396:
			return new ArgumentException(SR.D3DImage_AARequires9Ex, exceptionForHR);
		default:
			return exceptionForHR;
		}
	}

	public static void Check(int hr)
	{
		if (hr >= 0)
		{
			return;
		}
		throw ConvertHRToException(hr);
	}

	public static bool Succeeded(int hr)
	{
		if (hr >= 0)
		{
			return true;
		}
		return false;
	}

	public static bool Failed(int hr)
	{
		return !Succeeded(hr);
	}
}
