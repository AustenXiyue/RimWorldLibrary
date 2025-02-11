using System.Runtime.InteropServices;

namespace System.IO.Compression;

internal static class ZLibNative
{
	public enum FlushCode
	{
		NoFlush = 0,
		SyncFlush = 2,
		Finish = 4
	}

	public enum ErrorCode
	{
		Ok = 0,
		StreamEnd = 1,
		NeedDictionary = 2,
		StreamError = -2,
		DataError = -3,
		MemError = -4,
		BufError = -5,
		VersionError = -6
	}

	public enum CompressionLevel
	{
		NoCompression = 0,
		BestSpeed = 1,
		DefaultCompression = -1
	}

	public enum CompressionStrategy
	{
		DefaultStrategy
	}

	public enum CompressionMethod
	{
		Deflated = 8
	}

	public sealed class ZLibStreamHandle : SafeHandle
	{
		public enum State
		{
			NotInitialized,
			InitializedForDeflate,
			InitializedForInflate,
			Disposed
		}

		private ZStream _zStream;

		private volatile State _initializationState;

		public override bool IsInvalid => handle == new IntPtr(-1);

		public State InitializationState => _initializationState;

		public nint NextIn
		{
			get
			{
				return _zStream.nextIn;
			}
			set
			{
				_zStream.nextIn = value;
			}
		}

		public uint AvailIn
		{
			get
			{
				return _zStream.availIn;
			}
			set
			{
				_zStream.availIn = value;
			}
		}

		public nint NextOut
		{
			get
			{
				return _zStream.nextOut;
			}
			set
			{
				_zStream.nextOut = value;
			}
		}

		public uint AvailOut
		{
			get
			{
				return _zStream.availOut;
			}
			set
			{
				_zStream.availOut = value;
			}
		}

		public ZLibStreamHandle()
			: base(new IntPtr(-1), ownsHandle: true)
		{
			_zStream = default(ZStream);
			_initializationState = State.NotInitialized;
			SetHandle(IntPtr.Zero);
		}

		protected override bool ReleaseHandle()
		{
			return InitializationState switch
			{
				State.NotInitialized => true, 
				State.InitializedForDeflate => DeflateEnd() == ErrorCode.Ok, 
				State.InitializedForInflate => InflateEnd() == ErrorCode.Ok, 
				State.Disposed => true, 
				_ => false, 
			};
		}

		private void EnsureNotDisposed()
		{
			if (InitializationState == State.Disposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
		}

		private void EnsureState(State requiredState)
		{
			if (InitializationState != requiredState)
			{
				throw new InvalidOperationException("InitializationState != " + requiredState);
			}
		}

		public ErrorCode DeflateInit2_(CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
		{
			EnsureNotDisposed();
			EnsureState(State.NotInitialized);
			ErrorCode result = global::Interop.Zlib.DeflateInit2_(ref _zStream, level, CompressionMethod.Deflated, windowBits, memLevel, strategy);
			_initializationState = State.InitializedForDeflate;
			return result;
		}

		public ErrorCode Deflate(FlushCode flush)
		{
			EnsureNotDisposed();
			EnsureState(State.InitializedForDeflate);
			return global::Interop.Zlib.Deflate(ref _zStream, flush);
		}

		public ErrorCode DeflateEnd()
		{
			EnsureNotDisposed();
			EnsureState(State.InitializedForDeflate);
			ErrorCode result = global::Interop.Zlib.DeflateEnd(ref _zStream);
			_initializationState = State.Disposed;
			return result;
		}

		public ErrorCode InflateInit2_(int windowBits)
		{
			EnsureNotDisposed();
			EnsureState(State.NotInitialized);
			ErrorCode result = global::Interop.Zlib.InflateInit2_(ref _zStream, windowBits);
			_initializationState = State.InitializedForInflate;
			return result;
		}

		public ErrorCode Inflate(FlushCode flush)
		{
			EnsureNotDisposed();
			EnsureState(State.InitializedForInflate);
			return global::Interop.Zlib.Inflate(ref _zStream, flush);
		}

		public ErrorCode InflateEnd()
		{
			EnsureNotDisposed();
			EnsureState(State.InitializedForInflate);
			ErrorCode result = global::Interop.Zlib.InflateEnd(ref _zStream);
			_initializationState = State.Disposed;
			return result;
		}

		public string GetErrorMessage()
		{
			if (_zStream.msg == ZNullPtr)
			{
				return string.Empty;
			}
			return Marshal.PtrToStringAnsi(_zStream.msg);
		}
	}

	internal struct ZStream
	{
		internal nint nextIn;

		internal nint nextOut;

		internal nint msg;

		internal nint state;

		internal uint availIn;

		internal uint availOut;
	}

	internal static readonly nint ZNullPtr = IntPtr.Zero;

	public const int Deflate_DefaultWindowBits = -15;

	public const int GZip_DefaultWindowBits = 31;

	public const int Deflate_DefaultMemLevel = 8;

	public const int Deflate_NoCompressionMemLevel = 7;

	public static ErrorCode CreateZLibStreamForDeflate(out ZLibStreamHandle zLibStreamHandle, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
	{
		zLibStreamHandle = new ZLibStreamHandle();
		return zLibStreamHandle.DeflateInit2_(level, windowBits, memLevel, strategy);
	}

	public static ErrorCode CreateZLibStreamForInflate(out ZLibStreamHandle zLibStreamHandle, int windowBits)
	{
		zLibStreamHandle = new ZLibStreamHandle();
		return zLibStreamHandle.InflateInit2_(windowBits);
	}
}
