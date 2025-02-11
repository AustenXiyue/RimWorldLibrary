using System.IO;
using Microsoft.Win32.SafeHandles;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

internal sealed class UnknownBitmapDecoder : BitmapDecoder
{
	private class CoInitSafeHandle : SafeMILHandle
	{
		public CoInitSafeHandle()
		{
			UnsafeNativeMethods.WICCodec.CoInitialize(IntPtr.Zero);
		}

		protected override bool ReleaseHandle()
		{
			UnsafeNativeMethods.WICCodec.CoUninitialize();
			return true;
		}
	}

	private CoInitSafeHandle _safeHandle = new CoInitSafeHandle();

	private UnknownBitmapDecoder()
	{
	}

	internal UnknownBitmapDecoder(SafeMILHandle decoderHandle, BitmapDecoder decoder, Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, bool insertInDecoderCache, bool originalWritable, Stream uriStream, UnmanagedMemoryStream unmanagedMemoryStream, SafeFileHandle safeFilehandle)
		: base(decoderHandle, decoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, originalWritable, uriStream, unmanagedMemoryStream, safeFilehandle)
	{
	}

	internal override void SealObject()
	{
		throw new NotImplementedException();
	}
}
