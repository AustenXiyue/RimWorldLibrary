using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;
using MS.Win32.PresentationCore;

namespace System.Windows.Media.Imaging;

/// <summary>Provides a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that can be written to and updated.</summary>
public sealed class WriteableBitmap : BitmapSource
{
	private nint _backBuffer;

	private uint _backBufferSize;

	private MS.Internal.SecurityCriticalDataForSet<int> _backBufferStride;

	private SafeMILHandle _pDoubleBufferedBitmap;

	private SafeMILHandle _pBackBufferLock;

	private BitmapSourceSafeMILHandle _pBackBuffer;

	private uint _lockCount;

	private bool _hasDirtyRects = true;

	private bool _isWaitingForCommit;

	private ManualResetEvent _copyCompletedEvent = new ManualResetEvent(initialState: true);

	private EventHandler _committingBatchHandler;

	private bool _actLikeSimpleBitmap;

	/// <summary>Gets a pointer to the back buffer. </summary>
	/// <returns>An <see cref="T:System.IntPtr" /> that points to the base address of the back buffer. </returns>
	public nint BackBuffer
	{
		get
		{
			ReadPreamble();
			return _backBuffer;
		}
		private set
		{
			_backBuffer = value;
		}
	}

	/// <summary>Gets a value indicating the number of bytes in a single row of pixel data. </summary>
	/// <returns>An integer indicating the number of bytes in a single row of pixel data. </returns>
	public int BackBufferStride
	{
		get
		{
			ReadPreamble();
			return _backBufferStride.Value;
		}
	}

	private EventHandler CommittingBatchHandler
	{
		get
		{
			if (_committingBatchHandler == null)
			{
				_committingBatchHandler = OnCommittingBatch;
			}
			return _committingBatchHandler;
		}
	}

	internal WriteableBitmap()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> class using the given <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> to use for initialization.</param>
	public WriteableBitmap(BitmapSource source)
		: base(useVirtuals: true)
	{
		InitFromBitmapSource(source);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> class with the specified parameters.</summary>
	/// <param name="pixelWidth">The desired width of the bitmap.</param>
	/// <param name="pixelHeight">The desired height of the bitmap.</param>
	/// <param name="dpiX">The horizontal dots per inch (dpi) of the bitmap.</param>
	/// <param name="dpiY">The vertical dots per inch (dpi) of the bitmap.</param>
	/// <param name="pixelFormat">The <see cref="T:System.Windows.Media.PixelFormat" /> of the bitmap.</param>
	/// <param name="palette">The <see cref="T:System.Windows.Media.Imaging.BitmapPalette" /> of the bitmap.</param>
	public WriteableBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette)
		: base(useVirtuals: true)
	{
		BeginInit();
		if (pixelFormat.Palettized && palette == null)
		{
			throw new InvalidOperationException(SR.Image_IndexedPixelFormatRequiresPalette);
		}
		if (pixelFormat.Format == PixelFormatEnum.Default)
		{
			throw new ArgumentException(SR.Effect_PixelFormat, "pixelFormat");
		}
		if (pixelWidth < 0)
		{
			HRESULT.Check(-2147024362);
		}
		if (pixelWidth == 0)
		{
			HRESULT.Check(-2147024809);
		}
		if (pixelHeight < 0)
		{
			HRESULT.Check(-2147024362);
		}
		if (pixelHeight == 0)
		{
			HRESULT.Check(-2147024809);
		}
		Guid pixelFormatGuid = pixelFormat.Guid;
		SafeMILHandle pPalette = new SafeMILHandle();
		if (pixelFormat.Palettized)
		{
			pPalette = palette.InternalPalette;
		}
		HRESULT.Check(MILSwDoubleBufferedBitmap.Create((uint)pixelWidth, (uint)pixelHeight, dpiX, dpiY, ref pixelFormatGuid, pPalette, out _pDoubleBufferedBitmap));
		_pDoubleBufferedBitmap.UpdateEstimatedSize(GetEstimatedSize(pixelWidth, pixelHeight, pixelFormat));
		Lock();
		Unlock();
		EndInit();
	}

	/// <summary>Specifies the area of the bitmap that changed. </summary>
	/// <param name="dirtyRect">An <see cref="T:System.Windows.Int32Rect" /> representing the area that changed. Dimensions are in pixels. </param>
	/// <exception cref="T:System.InvalidOperationException">The bitmap has not been locked by a call to the <see cref="M:System.Windows.Media.Imaging.WriteableBitmap.Lock" /> or <see cref="M:System.Windows.Media.Imaging.WriteableBitmap.TryLock(System.Windows.Duration)" /> methods. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="dirtyRect" /> falls outside the bounds of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" />. </exception>
	public void AddDirtyRect(Int32Rect dirtyRect)
	{
		WritePreamble();
		if (_lockCount == 0)
		{
			throw new InvalidOperationException(SR.Image_MustBeLocked);
		}
		dirtyRect.ValidateForDirtyRect("dirtyRect", _pixelWidth, _pixelHeight);
		if (dirtyRect.HasArea)
		{
			MILSwDoubleBufferedBitmap.AddDirtyRect(_pDoubleBufferedBitmap, ref dirtyRect);
			_hasDirtyRects = true;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new WriteableBitmap Clone()
	{
		return (WriteableBitmap)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.ByteAnimationUsingKeyFrames" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new WriteableBitmap CloneCurrentValue()
	{
		return (WriteableBitmap)base.CloneCurrentValue();
	}

	/// <summary>Reserves the back buffer for updates.</summary>
	public void Lock()
	{
		TryLock(Duration.Forever);
	}

	/// <summary>Attempts to lock the bitmap, waiting for no longer than the specified length of time. </summary>
	/// <returns>true if the lock was acquired; otherwise, false. </returns>
	/// <param name="timeout">A <see cref="T:System.Windows.Duration" /> that represents the length of time to wait. A value of 0 returns immediately. A value of <see cref="P:System.Windows.Duration.Forever" /> blocks indefinitely. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is set to <see cref="P:System.Windows.Duration.Automatic" />. </exception>
	public bool TryLock(Duration timeout)
	{
		WritePreamble();
		if (timeout == Duration.Automatic)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		TimeSpan timeout2 = ((!(timeout == Duration.Forever)) ? timeout.TimeSpan : TimeSpan.FromMilliseconds(-1.0));
		if (_lockCount == uint.MaxValue)
		{
			throw new InvalidOperationException(SR.Image_LockCountLimit);
		}
		if (_lockCount == 0)
		{
			if (!AcquireBackBuffer(timeout2, waitForCopy: true))
			{
				return false;
			}
			Int32Rect prcLock = new Int32Rect(0, 0, _pixelWidth, _pixelHeight);
			HRESULT.Check(UnsafeNativeMethods.WICBitmap.Lock(base.WicSourceHandle, ref prcLock, LockFlags.MIL_LOCK_WRITE, out _pBackBufferLock));
			if (_backBuffer == IntPtr.Zero)
			{
				nint ppbData = IntPtr.Zero;
				uint pcbBufferSize = 0u;
				HRESULT.Check(UnsafeNativeMethods.WICBitmapLock.GetDataPointer(_pBackBufferLock, ref pcbBufferSize, ref ppbData));
				BackBuffer = ppbData;
				uint pcbStride = 0u;
				HRESULT.Check(UnsafeNativeMethods.WICBitmapLock.GetStride(_pBackBufferLock, ref pcbStride));
				Invariant.Assert(pcbStride <= int.MaxValue);
				_backBufferStride.Value = (int)pcbStride;
			}
			UnsubscribeFromCommittingBatch();
		}
		_lockCount++;
		return true;
	}

	/// <summary>Releases the back buffer to make it available for display. </summary>
	/// <exception cref="T:System.InvalidOperationException">The bitmap has not been locked by a call to the <see cref="M:System.Windows.Media.Imaging.WriteableBitmap.Lock" /> or <see cref="M:System.Windows.Media.Imaging.WriteableBitmap.TryLock(System.Windows.Duration)" /> methods. </exception>
	public void Unlock()
	{
		WritePreamble();
		if (_lockCount == 0)
		{
			throw new InvalidOperationException(SR.Image_MustBeLocked);
		}
		Invariant.Assert(_lockCount != 0, "Lock count should never be negative!");
		_lockCount--;
		if (_lockCount == 0)
		{
			_pBackBufferLock.Dispose();
			_pBackBufferLock = null;
			if (_hasDirtyRects)
			{
				SubscribeToCommittingBatch();
				WritePostscript();
			}
		}
	}

	public void WritePixels(Int32Rect sourceRect, nint sourceBuffer, int sourceBufferSize, int sourceBufferStride, int destinationX, int destinationY)
	{
		WritePreamble();
		WritePixelsImpl(sourceRect, sourceBuffer, sourceBufferSize, sourceBufferStride, destinationX, destinationY, backwardsCompat: false);
	}

	/// <summary>Updates the pixels in the specified region of the bitmap.</summary>
	/// <param name="sourceRect">The rectangle in <paramref name="sourceBuffer" /> to copy.</param>
	/// <param name="sourceBuffer">The input buffer used to update the bitmap.</param>
	/// <param name="sourceBufferStride">The stride of the input buffer, in bytes.</param>
	/// <param name="destinationX">The destination x-coordinate of the left-most pixel in the back buffer.</param>
	/// <param name="destinationY">The destination y-coordinate of the top-most pixel in the back buffer.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One or more of the following conditions is true.<paramref name="sourceRect" /> falls outside the bounds of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" />.<paramref name="destinationX" /> or <paramref name="destinationY" /> is outside the bounds of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" />.  <paramref name="sourceBufferStride" /> &lt; 1 </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceBuffer" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceBuffer" /> has a rank other than 1 or 2, or its length is less than or equal to 0.</exception>
	public void WritePixels(Int32Rect sourceRect, Array sourceBuffer, int sourceBufferStride, int destinationX, int destinationY)
	{
		WritePreamble();
		ValidateArrayAndGetInfo(sourceBuffer, backwardsCompat: false, out var _, out var sourceBufferSize, out var elementType);
		if (elementType == null || !elementType.IsValueType)
		{
			throw new ArgumentException(SR.Image_InvalidArrayForPixel);
		}
		GCHandle gCHandle = GCHandle.Alloc(sourceBuffer, GCHandleType.Pinned);
		try
		{
			nint sourceBuffer2 = gCHandle.AddrOfPinnedObject();
			WritePixelsImpl(sourceRect, sourceBuffer2, sourceBufferSize, sourceBufferStride, destinationX, destinationY, backwardsCompat: false);
		}
		finally
		{
			gCHandle.Free();
		}
	}

	public void WritePixels(Int32Rect sourceRect, nint buffer, int bufferSize, int stride)
	{
		WritePreamble();
		if (bufferSize < 1)
		{
			throw new ArgumentOutOfRangeException("bufferSize", SR.Format(SR.ParameterCannotBeLessThan, 1));
		}
		if (stride < 1)
		{
			throw new ArgumentOutOfRangeException("stride", SR.Format(SR.ParameterCannotBeLessThan, 1));
		}
		if (!sourceRect.IsEmpty && sourceRect.Width > 0 && sourceRect.Height > 0)
		{
			int x = sourceRect.X;
			int y = sourceRect.Y;
			sourceRect.X = 0;
			sourceRect.Y = 0;
			WritePixelsImpl(sourceRect, buffer, bufferSize, stride, x, y, backwardsCompat: true);
		}
	}

	/// <summary>Updates the pixels in the specified region of the bitmap.</summary>
	/// <param name="sourceRect">The rectangle of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" /> to update.</param>
	/// <param name="pixels">The pixel array used to update the bitmap.</param>
	/// <param name="stride">The stride of the update region in <paramref name="pixels" />.</param>
	/// <param name="offset">The input buffer offset.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One or more of the following conditions is true.<paramref name="sourceRect" /> falls outside the bounds of the <see cref="T:System.Windows.Media.Imaging.WriteableBitmap" />.  <paramref name="stride" /> &lt; 1 <paramref name="offset" /> &lt; 0 </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pixels" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="pixels" /> has a rank other than 1 or 2, or its length is less than or equal to 0.</exception>
	public void WritePixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
	{
		WritePreamble();
		if (sourceRect.IsEmpty || sourceRect.Width <= 0 || sourceRect.Height <= 0)
		{
			return;
		}
		ValidateArrayAndGetInfo(pixels, backwardsCompat: true, out var elementSize, out var sourceBufferSize, out var elementType);
		if (stride < 1)
		{
			throw new ArgumentOutOfRangeException("stride", SR.Format(SR.ParameterCannotBeLessThan, 1));
		}
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", SR.Format(SR.ParameterCannotBeLessThan, 0));
		}
		if (elementType == null || !elementType.IsValueType)
		{
			throw new ArgumentException(SR.Image_InvalidArrayForPixel);
		}
		checked
		{
			int num = offset * elementSize;
			if (num >= sourceBufferSize)
			{
				throw new IndexOutOfRangeException();
			}
			int x = sourceRect.X;
			int y = sourceRect.Y;
			sourceRect.X = 0;
			sourceRect.Y = 0;
			GCHandle gCHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			try
			{
				nint num2 = gCHandle.AddrOfPinnedObject();
				num2 = new IntPtr(unchecked((long)num2) + unchecked((long)num));
				sourceBufferSize -= num;
				WritePixelsImpl(sourceRect, num2, sourceBufferSize, stride, x, y, backwardsCompat: true);
			}
			finally
			{
				gCHandle.Free();
			}
		}
	}

	protected override Freezable CreateInstanceCore()
	{
		return new WriteableBitmap();
	}

	protected override void CloneCore(Freezable sourceFreezable)
	{
		WriteableBitmap sourceBitmap = (WriteableBitmap)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override bool FreezeCore(bool isChecking)
	{
		bool flag = _lockCount == 0 && base.FreezeCore(isChecking);
		if (flag && !isChecking)
		{
			HRESULT.Check(MILSwDoubleBufferedBitmap.ProtectBackBuffer(_pDoubleBufferedBitmap));
			AcquireBackBuffer(TimeSpan.Zero, waitForCopy: false);
			_needsUpdate = true;
			_hasDirtyRects = false;
			base.WicSourceHandle.CopyMemoryPressure(_pDoubleBufferedBitmap);
			_actLikeSimpleBitmap = true;
			int channelCount = _duceResource.GetChannelCount();
			for (int i = 0; i < channelCount; i++)
			{
				DUCE.Channel channel = _duceResource.GetChannel(i);
				uint refCountOnChannel = _duceResource.GetRefCountOnChannel(channel);
				for (uint num = 0u; num < refCountOnChannel; num++)
				{
					((DUCE.IResource)this).ReleaseOnChannel(channel);
				}
				for (uint num2 = 0u; num2 < refCountOnChannel; num2++)
				{
					((DUCE.IResource)this).AddRefOnChannel(channel);
				}
			}
			_pDoubleBufferedBitmap.Dispose();
			_pDoubleBufferedBitmap = null;
			_copyCompletedEvent.Close();
			_copyCompletedEvent = null;
			_committingBatchHandler = null;
			_pBackBuffer = null;
		}
		return flag;
	}

	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		WriteableBitmap sourceBitmap = (WriteableBitmap)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		WriteableBitmap sourceBitmap = (WriteableBitmap)sourceFreezable;
		base.GetAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		WriteableBitmap sourceBitmap = (WriteableBitmap)sourceFreezable;
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CopyCommon(sourceBitmap);
	}

	private long GetEstimatedSize(int pixelWidth, int pixelHeight, PixelFormat pixelFormat)
	{
		return pixelWidth * pixelHeight * pixelFormat.InternalBitsPerPixel / 8 * 2;
	}

	private void InitFromBitmapSource(BitmapSource source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (source.PixelWidth < 0)
		{
			HRESULT.Check(-2147024362);
		}
		if (source.PixelHeight < 0)
		{
			HRESULT.Check(-2147024362);
		}
		BeginInit();
		_syncObject = source.SyncObject;
		lock (_syncObject)
		{
			Guid pixelFormatGuid = source.Format.Guid;
			SafeMILHandle pPalette = new SafeMILHandle();
			if (source.Format.Palettized)
			{
				pPalette = source.Palette.InternalPalette;
			}
			HRESULT.Check(MILSwDoubleBufferedBitmap.Create((uint)source.PixelWidth, (uint)source.PixelHeight, source.DpiX, source.DpiY, ref pixelFormatGuid, pPalette, out _pDoubleBufferedBitmap));
			_pDoubleBufferedBitmap.UpdateEstimatedSize(GetEstimatedSize(source.PixelWidth, source.PixelHeight, source.Format));
			Lock();
			Int32Rect int32Rect = new Int32Rect(0, 0, _pixelWidth, _pixelHeight);
			int bufferSize = checked(_backBufferStride.Value * source.PixelHeight);
			source.CriticalCopyPixels(int32Rect, _backBuffer, bufferSize, _backBufferStride.Value);
			AddDirtyRect(int32Rect);
			Unlock();
		}
		EndInit();
	}

	private unsafe void WritePixelsImpl(Int32Rect sourceRect, nint sourceBuffer, int sourceBufferSize, int sourceBufferStride, int destinationX, int destinationY, bool backwardsCompat)
	{
		if (sourceRect.X < 0)
		{
			throw new ArgumentOutOfRangeException("sourceRect", SR.ParameterCannotBeNegative);
		}
		if (sourceRect.Y < 0)
		{
			throw new ArgumentOutOfRangeException("sourceRect", SR.ParameterCannotBeNegative);
		}
		if (sourceRect.Width < 0)
		{
			throw new ArgumentOutOfRangeException("sourceRect", SR.Format(SR.ParameterMustBeBetween, 0, _pixelWidth));
		}
		if (sourceRect.Width > _pixelWidth)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("sourceRect", SR.Format(SR.ParameterMustBeBetween, 0, _pixelWidth));
			}
			HRESULT.Check(-2147024809);
		}
		if (sourceRect.Height < 0)
		{
			throw new ArgumentOutOfRangeException("sourceRect", SR.Format(SR.ParameterMustBeBetween, 0, _pixelHeight));
		}
		if (sourceRect.Height > _pixelHeight)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("sourceRect", SR.Format(SR.ParameterMustBeBetween, 0, _pixelHeight));
			}
			HRESULT.Check(-2147024809);
		}
		if (destinationX < 0)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("sourceRect", SR.ParameterCannotBeNegative);
			}
			HRESULT.Check(-2147024362);
		}
		if (destinationX > _pixelWidth - sourceRect.Width)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("destinationX", SR.Format(SR.ParameterMustBeBetween, 0, _pixelWidth - sourceRect.Width));
			}
			HRESULT.Check(-2147024809);
		}
		if (destinationY < 0)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("destinationY", SR.Format(SR.ParameterMustBeBetween, 0, _pixelHeight - sourceRect.Height));
			}
			HRESULT.Check(-2147024362);
		}
		if (destinationY > _pixelHeight - sourceRect.Height)
		{
			if (!backwardsCompat)
			{
				throw new ArgumentOutOfRangeException("destinationY", SR.Format(SR.ParameterMustBeBetween, 0, _pixelHeight - sourceRect.Height));
			}
			HRESULT.Check(-2147024809);
		}
		if (sourceBuffer == IntPtr.Zero)
		{
			throw new ArgumentNullException(backwardsCompat ? "buffer" : "sourceBuffer");
		}
		if (sourceBufferStride < 1)
		{
			throw new ArgumentOutOfRangeException("sourceBufferStride", SR.Format(SR.ParameterCannotBeLessThan, 1));
		}
		if (sourceRect.Width == 0 || sourceRect.Height == 0)
		{
			return;
		}
		uint num = checked((uint)((sourceRect.X + sourceRect.Width) * _format.InternalBitsPerPixel) + 7) / 8;
		uint copyWidthInBits;
		uint inputBufferOffsetInBits;
		uint num4;
		uint outputBufferOffsetInBits;
		Int32Rect dirtyRect;
		uint num6;
		checked
		{
			uint num2 = (uint)((sourceRect.Y + sourceRect.Height - 1) * sourceBufferStride) + num;
			if (sourceBufferSize < num2)
			{
				if (!backwardsCompat)
				{
					throw new ArgumentException(SR.Image_InsufficientBufferSize, "sourceBufferSize");
				}
				HRESULT.Check(-2003292276);
			}
			copyWidthInBits = (uint)(sourceRect.Width * _format.InternalBitsPerPixel);
			uint num3 = (uint)unchecked(checked(sourceRect.X * _format.InternalBitsPerPixel) / 8);
			inputBufferOffsetInBits = (uint)unchecked(checked(sourceRect.X * _format.InternalBitsPerPixel) % 8);
			num4 = (uint)(sourceRect.Y * sourceBufferStride + num3);
			uint num5 = (uint)unchecked(checked(destinationX * _format.InternalBitsPerPixel) / 8);
			outputBufferOffsetInBits = (uint)unchecked(checked(destinationX * _format.InternalBitsPerPixel) % 8);
			dirtyRect = sourceRect;
			dirtyRect.X = destinationX;
			dirtyRect.Y = destinationY;
			num6 = (uint)(destinationY * _backBufferStride.Value) + num5;
		}
		byte* ptr = (byte*)((IntPtr)_backBuffer).ToPointer();
		ptr = (byte*)checked(unchecked((nuint)ptr) + unchecked((nuint)num6));
		uint outputBufferSize = checked(_backBufferSize - num6);
		byte* ptr2 = (byte*)((IntPtr)sourceBuffer).ToPointer();
		ptr2 = (byte*)checked(unchecked((nuint)ptr2) + unchecked((nuint)num4));
		checked
		{
			uint inputBufferSize = (uint)sourceBufferSize - num4;
			Lock();
			MILUtilities.MILCopyPixelBuffer(ptr, outputBufferSize, (uint)_backBufferStride.Value, outputBufferOffsetInBits, ptr2, inputBufferSize, (uint)sourceBufferStride, inputBufferOffsetInBits, (uint)sourceRect.Height, copyWidthInBits);
			AddDirtyRect(dirtyRect);
			Unlock();
		}
	}

	private bool AcquireBackBuffer(TimeSpan timeout, bool waitForCopy)
	{
		bool result = false;
		if (_pBackBuffer == null)
		{
			bool flag = true;
			if (waitForCopy)
			{
				flag = _copyCompletedEvent.WaitOne(timeout, exitContext: false);
			}
			if (flag)
			{
				MILSwDoubleBufferedBitmap.GetBackBuffer(_pDoubleBufferedBitmap, out _pBackBuffer, out _backBufferSize);
				BitmapSourceSafeMILHandle syncObject = (base.WicSourceHandle = _pBackBuffer);
				_syncObject = syncObject;
				result = true;
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	private void CopyCommon(WriteableBitmap sourceBitmap)
	{
		base.Animatable_IsResourceInvalidationNecessary = false;
		_actLikeSimpleBitmap = false;
		InitFromBitmapSource(sourceBitmap);
		base.Animatable_IsResourceInvalidationNecessary = true;
	}

	private void BeginInit()
	{
		_bitmapInit.BeginInit();
	}

	private void EndInit()
	{
		_bitmapInit.EndInit();
		FinalizeCreation();
	}

	internal override void FinalizeCreation()
	{
		base.IsSourceCached = true;
		base.CreationCompleted = true;
		UpdateCachedSettings();
	}

	private void ValidateArrayAndGetInfo(Array sourceBuffer, bool backwardsCompat, out int elementSize, out int sourceBufferSize, out Type elementType)
	{
		if (sourceBuffer == null)
		{
			throw new ArgumentNullException(backwardsCompat ? "pixels" : "sourceBuffer");
		}
		checked
		{
			if (sourceBuffer.Rank == 1)
			{
				if (sourceBuffer.GetLength(0) <= 0)
				{
					if (!backwardsCompat)
					{
						throw new ArgumentException(SR.Image_InsufficientBuffer, "sourceBuffer");
					}
					elementSize = 1;
					sourceBufferSize = 0;
					elementType = null;
				}
				else
				{
					object value = sourceBuffer.GetValue(0);
					elementSize = Marshal.SizeOf(value);
					sourceBufferSize = sourceBuffer.GetLength(0) * elementSize;
					elementType = value.GetType();
				}
				return;
			}
			if (sourceBuffer.Rank == 2)
			{
				if (sourceBuffer.GetLength(0) <= 0 || sourceBuffer.GetLength(1) <= 0)
				{
					if (!backwardsCompat)
					{
						throw new ArgumentException(SR.Image_InsufficientBuffer, "sourceBuffer");
					}
					elementSize = 1;
					sourceBufferSize = 0;
					elementType = null;
				}
				else
				{
					object value2 = sourceBuffer.GetValue(0, 0);
					elementSize = Marshal.SizeOf(value2);
					sourceBufferSize = sourceBuffer.GetLength(0) * sourceBuffer.GetLength(1) * elementSize;
					elementType = value2.GetType();
				}
				return;
			}
			throw new ArgumentException(SR.Collection_BadRank, backwardsCompat ? "pixels" : "sourceBuffer");
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_actLikeSimpleBitmap)
		{
			return base.AddRefOnChannelCore(channel);
		}
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DOUBLEBUFFEREDBITMAP))
		{
			if (!channel.IsSynchronous && _hasDirtyRects)
			{
				SubscribeToCommittingBatch();
			}
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			if (!channel.IsSynchronous)
			{
				UnsubscribeFromCommittingBatch();
			}
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal unsafe override void UpdateBitmapSourceResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (_actLikeSimpleBitmap)
		{
			base.UpdateBitmapSourceResource(channel, skipOnChannelCheck);
		}
		else if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			DUCE.MILCMD_DOUBLEBUFFEREDBITMAP mILCMD_DOUBLEBUFFEREDBITMAP = default(DUCE.MILCMD_DOUBLEBUFFEREDBITMAP);
			mILCMD_DOUBLEBUFFEREDBITMAP.Type = MILCMD.MilCmdDoubleBufferedBitmap;
			mILCMD_DOUBLEBUFFEREDBITMAP.Handle = _duceResource.GetHandle(channel);
			mILCMD_DOUBLEBUFFEREDBITMAP.SwDoubleBufferedBitmap = (ulong)((IntPtr)_pDoubleBufferedBitmap.DangerousGetHandle()).ToPointer();
			mILCMD_DOUBLEBUFFEREDBITMAP.UseBackBuffer = (channel.IsSynchronous ? 1u : 0u);
			UnsafeNativeMethods.MILUnknown.AddRef(_pDoubleBufferedBitmap);
			channel.SendCommand((byte*)(&mILCMD_DOUBLEBUFFEREDBITMAP), sizeof(DUCE.MILCMD_DOUBLEBUFFEREDBITMAP), sendInSeparateBatch: false);
		}
	}

	private void SubscribeToCommittingBatch()
	{
		if (!_isWaitingForCommit)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			if (_duceResource.IsOnChannel(mediaContext.Channel))
			{
				mediaContext.CommittingBatch += CommittingBatchHandler;
				_isWaitingForCommit = true;
			}
		}
	}

	private void UnsubscribeFromCommittingBatch()
	{
		if (_isWaitingForCommit)
		{
			MediaContext.From(base.Dispatcher).CommittingBatch -= CommittingBatchHandler;
			_isWaitingForCommit = false;
		}
	}

	private unsafe void OnCommittingBatch(object sender, EventArgs args)
	{
		UnsubscribeFromCommittingBatch();
		_copyCompletedEvent.Reset();
		_pBackBuffer = null;
		DUCE.Channel channel = sender as DUCE.Channel;
		nint currentProcess = MS.Win32.UnsafeNativeMethods.GetCurrentProcess();
		if (!MS.Win32.UnsafeNativeMethods.DuplicateHandle(currentProcess, _copyCompletedEvent.SafeWaitHandle, currentProcess, out var hTargetHandle, 0u, fInheritHandle: false, 2u))
		{
			throw new Win32Exception();
		}
		DUCE.MILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD mILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD = default(DUCE.MILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD);
		mILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD.Type = MILCMD.MilCmdDoubleBufferedBitmapCopyForward;
		mILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD.Handle = _duceResource.GetHandle(channel);
		mILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD.CopyCompletedEvent = (ulong)((IntPtr)hTargetHandle).ToInt64();
		channel.SendCommand((byte*)(&mILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD), sizeof(DUCE.MILCMD_DOUBLEBUFFEREDBITMAP_COPYFORWARD));
		channel.CloseBatch();
		_hasDirtyRects = false;
	}
}
