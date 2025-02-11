using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class StreamAsIStream
{
	private const int STREAM_SEEK_SET = 0;

	private const int STREAM_SEEK_CUR = 1;

	private const int STREAM_SEEK_END = 2;

	protected Stream dataStream;

	private Exception _lastException;

	private long virtualPosition = -1L;

	private StreamAsIStream(Stream dataStream)
	{
		this.dataStream = dataStream;
	}

	private void ActualizeVirtualPosition()
	{
		if (virtualPosition != -1)
		{
			if (virtualPosition > dataStream.Length)
			{
				dataStream.SetLength(virtualPosition);
			}
			dataStream.Position = virtualPosition;
			virtualPosition = -1L;
		}
	}

	public int Clone(out nint stream)
	{
		stream = IntPtr.Zero;
		try
		{
			Verify();
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return -2147467263;
	}

	public int Commit(uint grfCommitFlags)
	{
		try
		{
			Verify();
			dataStream.Flush();
			ActualizeVirtualPosition();
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int CopyTo(nint pstm, long cb, out long cbRead, out long cbWritten)
	{
		int result = 0;
		uint num = 4096u;
		byte[] buffer = new byte[num];
		cbWritten = 0L;
		cbRead = 0L;
		try
		{
			Verify();
			while (cbWritten < cb)
			{
				uint num2 = num;
				if (cbWritten + num2 > cb)
				{
					num2 = (uint)(cb - cbWritten);
				}
				uint cbRead2 = 0u;
				result = Read(buffer, num2, out cbRead2);
				if (cbRead2 != 0)
				{
					cbRead += cbRead2;
					uint cbWritten2 = 0u;
					result = MILIStreamWrite(pstm, buffer, cbRead2, out cbWritten2);
					if (cbWritten2 != cbRead2)
					{
						return result;
					}
					cbWritten += cbRead2;
					continue;
				}
				break;
			}
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return result;
	}

	public int LockRegion(long libOffset, long cb, uint dwLockType)
	{
		try
		{
			Verify();
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return -2147467263;
	}

	public int Read(byte[] buffer, uint cb, out uint cbRead)
	{
		cbRead = 0u;
		try
		{
			Verify();
			ActualizeVirtualPosition();
			cbRead = (uint)dataStream.Read(buffer, 0, (int)cb);
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int Revert()
	{
		try
		{
			Verify();
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return -2147467263;
	}

	public unsafe int Seek(long offset, uint origin, long* plibNewPostion)
	{
		try
		{
			Verify();
			long position = virtualPosition;
			if (virtualPosition == -1)
			{
				position = dataStream.Position;
			}
			long length = dataStream.Length;
			switch (origin)
			{
			case 0u:
				if (offset <= length)
				{
					dataStream.Position = offset;
					virtualPosition = -1L;
				}
				else
				{
					virtualPosition = offset;
				}
				break;
			case 2u:
				if (offset <= 0)
				{
					dataStream.Position = length + offset;
					virtualPosition = -1L;
				}
				else
				{
					virtualPosition = length + offset;
				}
				break;
			case 1u:
				if (offset + position <= length)
				{
					dataStream.Position = position + offset;
					virtualPosition = -1L;
				}
				else
				{
					virtualPosition = offset + position;
				}
				break;
			}
			if (plibNewPostion != null)
			{
				if (virtualPosition != -1)
				{
					*plibNewPostion = virtualPosition;
				}
				else
				{
					*plibNewPostion = dataStream.Position;
				}
			}
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int SetSize(long value)
	{
		try
		{
			Verify();
			dataStream.SetLength(value);
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int Stat(out STATSTG statstg, uint grfStatFlag)
	{
		STATSTG sTATSTG = (statstg = default(STATSTG));
		try
		{
			Verify();
			sTATSTG.type = 2;
			sTATSTG.cbSize = dataStream.Length;
			sTATSTG.grfLocksSupported = 2;
			sTATSTG.pwcsName = null;
			statstg = sTATSTG;
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int UnlockRegion(long libOffset, long cb, uint dwLockType)
	{
		try
		{
			Verify();
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return -2147467263;
	}

	public int Write(byte[] buffer, uint cb, out uint cbWritten)
	{
		cbWritten = 0u;
		try
		{
			Verify();
			ActualizeVirtualPosition();
			dataStream.Write(buffer, 0, (int)cb);
			cbWritten = cb;
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int CanWrite(out bool canWrite)
	{
		canWrite = false;
		try
		{
			Verify();
			canWrite = dataStream.CanWrite;
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	public int CanSeek(out bool canSeek)
	{
		canSeek = false;
		try
		{
			Verify();
			canSeek = dataStream.CanSeek;
		}
		catch (Exception lastException)
		{
			return SecurityHelper.GetHRForException(_lastException = lastException);
		}
		return 0;
	}

	private void Verify()
	{
		if (dataStream == null)
		{
			throw new ObjectDisposedException(SR.Media_StreamClosed);
		}
	}

	internal static StreamAsIStream FromSD(ref StreamDescriptor sd)
	{
		GCHandle handle = sd.m_handle;
		return (StreamAsIStream)handle.Target;
	}

	internal static int Clone(ref StreamDescriptor pSD, out nint stream)
	{
		return FromSD(ref pSD).Clone(out stream);
	}

	internal static int Commit(ref StreamDescriptor pSD, uint grfCommitFlags)
	{
		return FromSD(ref pSD).Commit(grfCommitFlags);
	}

	internal static int CopyTo(ref StreamDescriptor pSD, nint pstm, long cb, out long cbRead, out long cbWritten)
	{
		return FromSD(ref pSD).CopyTo(pstm, cb, out cbRead, out cbWritten);
	}

	internal static int LockRegion(ref StreamDescriptor pSD, long libOffset, long cb, uint dwLockType)
	{
		return FromSD(ref pSD).LockRegion(libOffset, cb, dwLockType);
	}

	internal static int Read(ref StreamDescriptor pSD, byte[] buffer, uint cb, out uint cbRead)
	{
		return FromSD(ref pSD).Read(buffer, cb, out cbRead);
	}

	internal static int Revert(ref StreamDescriptor pSD)
	{
		return FromSD(ref pSD).Revert();
	}

	internal unsafe static int Seek(ref StreamDescriptor pSD, long offset, uint origin, long* plibNewPostion)
	{
		return FromSD(ref pSD).Seek(offset, origin, plibNewPostion);
	}

	internal static int SetSize(ref StreamDescriptor pSD, long value)
	{
		return FromSD(ref pSD).SetSize(value);
	}

	internal static int Stat(ref StreamDescriptor pSD, out STATSTG statstg, uint grfStatFlag)
	{
		return FromSD(ref pSD).Stat(out statstg, grfStatFlag);
	}

	internal static int UnlockRegion(ref StreamDescriptor pSD, long libOffset, long cb, uint dwLockType)
	{
		return FromSD(ref pSD).UnlockRegion(libOffset, cb, dwLockType);
	}

	internal static int Write(ref StreamDescriptor pSD, byte[] buffer, uint cb, out uint cbWritten)
	{
		return FromSD(ref pSD).Write(buffer, cb, out cbWritten);
	}

	internal static int CanWrite(ref StreamDescriptor pSD, out bool canWrite)
	{
		return FromSD(ref pSD).CanWrite(out canWrite);
	}

	internal static int CanSeek(ref StreamDescriptor pSD, out bool canSeek)
	{
		return FromSD(ref pSD).CanSeek(out canSeek);
	}

	internal static nint IStreamMemoryFrom(nint comStream)
	{
		nint ppIStream = IntPtr.Zero;
		using FactoryMaker factoryMaker = new FactoryMaker();
		if (HRESULT.Failed(UnsafeNativeMethods.WICImagingFactory.CreateStream(factoryMaker.ImagingFactoryPtr, out ppIStream)))
		{
			return IntPtr.Zero;
		}
		if (HRESULT.Failed(UnsafeNativeMethods.WICStream.InitializeFromIStream(ppIStream, comStream)))
		{
			UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ppIStream);
			return IntPtr.Zero;
		}
		return ppIStream;
	}

	internal static nint IStreamFrom(nint memoryBuffer, int bufferSize)
	{
		nint ppIStream = IntPtr.Zero;
		using FactoryMaker factoryMaker = new FactoryMaker();
		if (HRESULT.Failed(UnsafeNativeMethods.WICImagingFactory.CreateStream(factoryMaker.ImagingFactoryPtr, out ppIStream)))
		{
			return IntPtr.Zero;
		}
		if (HRESULT.Failed(UnsafeNativeMethods.WICStream.InitializeFromMemory(ppIStream, memoryBuffer, (uint)bufferSize)))
		{
			UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref ppIStream);
			return IntPtr.Zero;
		}
		return ppIStream;
	}

	internal static nint IStreamFrom(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		nint ppStream = IntPtr.Zero;
		StreamAsIStream value = new StreamAsIStream(stream);
		StreamDescriptor pSD = default(StreamDescriptor);
		pSD.pfnDispose = StaticPtrs.pfnDispose;
		pSD.pfnClone = StaticPtrs.pfnClone;
		pSD.pfnCommit = StaticPtrs.pfnCommit;
		pSD.pfnCopyTo = StaticPtrs.pfnCopyTo;
		pSD.pfnLockRegion = StaticPtrs.pfnLockRegion;
		pSD.pfnRead = StaticPtrs.pfnRead;
		pSD.pfnRevert = StaticPtrs.pfnRevert;
		pSD.pfnSeek = StaticPtrs.pfnSeek;
		pSD.pfnSetSize = StaticPtrs.pfnSetSize;
		pSD.pfnStat = StaticPtrs.pfnStat;
		pSD.pfnUnlockRegion = StaticPtrs.pfnUnlockRegion;
		pSD.pfnWrite = StaticPtrs.pfnWrite;
		pSD.pfnCanWrite = StaticPtrs.pfnCanWrite;
		pSD.pfnCanSeek = StaticPtrs.pfnCanSeek;
		pSD.m_handle = GCHandle.Alloc(value, GCHandleType.Normal);
		HRESULT.Check(UnsafeNativeMethods.MilCoreApi.MILCreateStreamFromStreamDescriptor(ref pSD, out ppStream));
		return ppStream;
	}

	[DllImport("wpfgfx_cor3.dll")]
	private static extern int MILIStreamWrite(nint pStream, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, uint cb, out uint cbWritten);
}
