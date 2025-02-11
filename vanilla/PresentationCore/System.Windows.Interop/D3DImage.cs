using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;
using MS.Win32;
using MS.Win32.PresentationCore;

namespace System.Windows.Interop;

/// <summary>An <see cref="T:System.Windows.Media.ImageSource" /> that displays a user-created Direct3D surface. </summary>
public class D3DImage : ImageSource, IAppDomainShutdownListener
{
	/// <summary>Identifies the <see cref="P:System.Windows.Interop.D3DImage.IsFrontBufferAvailable" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Interop.D3DImage.IsFrontBufferAvailable" /> dependency property. </returns>
	public static readonly DependencyProperty IsFrontBufferAvailableProperty;

	private static readonly DependencyPropertyKey IsFrontBufferAvailablePropertyKey;

	internal DUCE.MultiChannelResource _duceResource;

	private double _dpiX;

	private double _dpiY;

	private SafeMILHandle _pInteropDeviceBitmap;

	private BitmapSource _softwareCopy;

	private nint _pUserSurfaceUnsafe;

	private bool _isSoftwareFallbackEnabled;

	private ManualResetEvent _canWriteEvent;

	private UnsafeNativeMethods.InteropDeviceBitmap.FrontBufferAvailableCallback _availableCallback;

	private DependencyPropertyChangedEventHandler _isFrontBufferAvailableChangedHandlers;

	private EventHandler _sendPresentDelegate;

	private WeakReference<IAppDomainShutdownListener> _listener;

	private uint _lockCount;

	private uint _pixelWidth;

	private uint _pixelHeight;

	private uint _version;

	private bool _isDirty;

	private bool _isWaitingForPresent;

	private bool _isChangePending;

	private bool _waitingForUpdateResourceBecauseBitmapChanged;

	/// <summary>Gets a value that indicates whether a front buffer exists.</summary>
	/// <returns>true if a front buffer exists; otherwise, false. </returns>
	public bool IsFrontBufferAvailable => (bool)GetValue(IsFrontBufferAvailableProperty);

	/// <summary>Gets the width of the <see cref="T:System.Windows.Interop.D3DImage" />, in pixels.</summary>
	/// <returns>The width of the <see cref="T:System.Windows.Interop.D3DImage" />, in pixels.</returns>
	public int PixelWidth
	{
		get
		{
			ReadPreamble();
			return (int)_pixelWidth;
		}
	}

	/// <summary>Gets the height of the <see cref="T:System.Windows.Interop.D3DImage" />, in pixels.</summary>
	/// <returns>The height of the <see cref="T:System.Windows.Interop.D3DImage" />, in pixels.</returns>
	public int PixelHeight
	{
		get
		{
			ReadPreamble();
			return (int)_pixelHeight;
		}
	}

	/// <summary>Gets the width of the <see cref="T:System.Windows.Interop.D3DImage" />. </summary>
	/// <returns>The width of the <see cref="T:System.Windows.Interop.D3DImage" />, in measure units. A measure unit is 1/96th inch.</returns>
	public sealed override double Width
	{
		get
		{
			ReadPreamble();
			return ImageSource.PixelsToDIPs(_dpiX, (int)_pixelWidth);
		}
	}

	/// <summary>Gets the height of the <see cref="T:System.Windows.Interop.D3DImage" />. </summary>
	/// <returns>The height of the <see cref="T:System.Windows.Interop.D3DImage" />, in measure units. A measure unit is 1/96th inch.</returns>
	public sealed override double Height
	{
		get
		{
			ReadPreamble();
			return ImageSource.PixelsToDIPs(_dpiY, (int)_pixelHeight);
		}
	}

	/// <summary>Gets the metadata associated with the image source.</summary>
	/// <returns>null in all cases. </returns>
	public sealed override ImageMetadata Metadata
	{
		get
		{
			ReadPreamble();
			return null;
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Interop.D3DImage.IsFrontBufferAvailable" /> property changes. </summary>
	public event DependencyPropertyChangedEventHandler IsFrontBufferAvailableChanged
	{
		add
		{
			WritePreamble();
			if (value != null)
			{
				_isFrontBufferAvailableChangedHandlers = (DependencyPropertyChangedEventHandler)Delegate.Combine(_isFrontBufferAvailableChangedHandlers, value);
			}
		}
		remove
		{
			WritePreamble();
			if (value != null)
			{
				_isFrontBufferAvailableChangedHandlers = (DependencyPropertyChangedEventHandler)Delegate.Remove(_isFrontBufferAvailableChangedHandlers, value);
			}
		}
	}

	static D3DImage()
	{
		IsFrontBufferAvailablePropertyKey = DependencyProperty.RegisterReadOnly("IsFrontBufferAvailable", typeof(bool), typeof(D3DImage), new UIPropertyMetadata(BooleanBoxes.TrueBox, IsFrontBufferAvailablePropertyChanged));
		IsFrontBufferAvailableProperty = IsFrontBufferAvailablePropertyKey.DependencyProperty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.D3DImage" /> class. </summary>
	public D3DImage()
		: this(96.0, 96.0)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Interop.D3DImage" /> class with the specified display resolution. </summary>
	/// <param name="dpiX">The display resolution on the x-axis. </param>
	/// <param name="dpiY">The display resolution on the y-axis.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="dpiX" /> or <paramref name="dpiY" /> is less than zero.</exception>
	public D3DImage(double dpiX, double dpiY)
	{
		if (dpiX < 0.0)
		{
			throw new ArgumentOutOfRangeException("dpiX", SR.ParameterMustBeGreaterThanZero);
		}
		if (dpiY < 0.0)
		{
			throw new ArgumentOutOfRangeException("dpiY", SR.ParameterMustBeGreaterThanZero);
		}
		_canWriteEvent = new ManualResetEvent(initialState: true);
		_availableCallback = Callback;
		_sendPresentDelegate = SendPresent;
		_dpiX = dpiX;
		_dpiY = dpiY;
		_listener = new WeakReference<IAppDomainShutdownListener>(this);
		AppDomainShutdownMonitor.Add(_listener);
	}

	/// <summary>Frees resources and performs other cleanup operations before the <see cref="T:System.Windows.Interop.D3DImage" /> is reclaimed by garbage collection. </summary>
	~D3DImage()
	{
		if (_pInteropDeviceBitmap != null)
		{
			UnsafeNativeMethods.InteropDeviceBitmap.Detach(_pInteropDeviceBitmap);
		}
		AppDomainShutdownMonitor.Remove(_listener);
	}

	public void SetBackBuffer(D3DResourceType backBufferType, nint backBuffer)
	{
		SetBackBuffer(backBufferType, backBuffer, enableSoftwareFallback: false);
	}

	public void SetBackBuffer(D3DResourceType backBufferType, nint backBuffer, bool enableSoftwareFallback)
	{
		WritePreamble();
		if (_lockCount == 0)
		{
			throw new InvalidOperationException(SR.Image_MustBeLocked);
		}
		if (backBufferType != 0)
		{
			throw new ArgumentOutOfRangeException("backBufferType");
		}
		if (backBuffer == IntPtr.Zero || backBuffer != _pUserSurfaceUnsafe)
		{
			SafeMILHandle ppInteropDeviceBitmap = null;
			uint pixelWidth = 0u;
			uint pixelHeight = 0u;
			if (backBuffer != IntPtr.Zero)
			{
				HRESULT.Check(UnsafeNativeMethods.InteropDeviceBitmap.Create(backBuffer, _dpiX, _dpiY, ++_version, _availableCallback, enableSoftwareFallback, out ppInteropDeviceBitmap, out pixelWidth, out pixelHeight));
			}
			if (_pInteropDeviceBitmap != null)
			{
				UnsafeNativeMethods.InteropDeviceBitmap.Detach(_pInteropDeviceBitmap);
				UnsubscribeFromCommittingBatch();
				_isDirty = false;
			}
			_pInteropDeviceBitmap = ppInteropDeviceBitmap;
			_pUserSurfaceUnsafe = backBuffer;
			_pixelWidth = pixelWidth;
			_pixelHeight = pixelHeight;
			_isSoftwareFallbackEnabled = enableSoftwareFallback;
			if (_pInteropDeviceBitmap == null)
			{
				_isChangePending = true;
			}
			RegisterForAsyncUpdateResource();
			_waitingForUpdateResourceBecauseBitmapChanged = true;
		}
	}

	/// <summary>Locks the <see cref="T:System.Windows.Interop.D3DImage" /> and enables operations on the back buffer. </summary>
	/// <exception cref="T:System.InvalidOperationException">The lock count equals <see cref="F:System.UInt32.MaxValue" />.</exception>
	public void Lock()
	{
		WritePreamble();
		LockImpl(Duration.Forever);
	}

	/// <summary>Attempts to lock the <see cref="T:System.Windows.Interop.D3DImage" /> and waits for the specified duration.</summary>
	/// <returns>true if the lock was acquired; otherwise, false. </returns>
	/// <param name="timeout">The duration to wait for the lock to be acquired.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="timeout" /> is set to <see cref="P:System.Windows.Duration.Automatic" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The lock count equals <see cref="F:System.UInt32.MaxValue" />.</exception>
	public bool TryLock(Duration timeout)
	{
		WritePreamble();
		if (timeout == Duration.Automatic)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		return LockImpl(timeout);
	}

	/// <summary>Decrements the lock count for the <see cref="T:System.Windows.Interop.D3DImage" />. </summary>
	public void Unlock()
	{
		WritePreamble();
		if (_lockCount == 0)
		{
			throw new InvalidOperationException(SR.Image_MustBeLocked);
		}
		_lockCount--;
		if (_isDirty && _lockCount == 0)
		{
			SubscribeToCommittingBatch();
		}
		if (_isChangePending)
		{
			_isChangePending = false;
			WritePostscript();
		}
	}

	/// <summary>Specifies the area of the back buffer that changed. </summary>
	/// <param name="dirtyRect">An <see cref="T:System.Windows.Int32Rect" /> that represents the area that changed.</param>
	/// <exception cref="T:System.InvalidOperationException">The bitmap has not been locked by a call to the <see cref="M:System.Windows.Interop.D3DImage.Lock" /> or <see cref="M:System.Windows.Interop.D3DImage.TryLock(System.Windows.Duration)" /> methods. -or- The back buffer has not been assigned by a call to the <see cref="M:System.Windows.Interop.D3DImage.SetBackBuffer(System.Windows.Interop.D3DResourceType,System.IntPtr)" /> method. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">One or more of the following conditions is true.<paramref name="dirtyRect.X" /> &lt; 0 <paramref name="dirtyRect.Y" /> &lt; 0 <paramref name="dirtyRect.Width" /> &lt; 0 or <paramref name="dirtyRect.Width" /> &gt; <see cref="P:System.Windows.Interop.D3DImage.PixelWidth" /><paramref name="dirtyRect.Height" /> &lt; 0 or <paramref name="dirtyRect.Height" /> &gt; <see cref="P:System.Windows.Interop.D3DImage.PixelHeight" /></exception>
	public void AddDirtyRect(Int32Rect dirtyRect)
	{
		WritePreamble();
		if (_lockCount == 0)
		{
			throw new InvalidOperationException(SR.Image_MustBeLocked);
		}
		if (_pInteropDeviceBitmap == null)
		{
			throw new InvalidOperationException(SR.D3DImage_MustHaveBackBuffer);
		}
		dirtyRect.ValidateForDirtyRect("dirtyRect", PixelWidth, PixelHeight);
		if (dirtyRect.HasArea)
		{
			HRESULT.Check(UnsafeNativeMethods.InteropDeviceBitmap.AddDirtyRect(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height, _pInteropDeviceBitmap));
			_isDirty = true;
			_isChangePending = true;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Interop.D3DImage" /> object, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (which may no longer resolve), but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new D3DImage Clone()
	{
		return (D3DImage)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Interop.D3DImage" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are copied. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new D3DImage CloneCurrentValue()
	{
		return (D3DImage)base.CloneCurrentValue();
	}

	/// <summary>When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Interop.D3DImage" /> derived class. </summary>
	/// <returns>The new instance. </returns>
	protected override Freezable CreateInstanceCore()
	{
		return new D3DImage();
	}

	/// <summary>Makes the <see cref="T:System.Windows.Interop.D3DImage" /> unmodifiable or determines whether it can be made unmodifiable. </summary>
	/// <returns>false in all cases.</returns>
	/// <param name="isChecking">Has no effect.</param>
	protected sealed override bool FreezeCore(bool isChecking)
	{
		return false;
	}

	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		base.CloneCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to be cloned.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		base.CloneCurrentValueCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <param name="sourceFreezable">The instance to copy.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetAsFrozenCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Freezable" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <summary>Creates a software copy of the <see cref="T:System.Windows.Interop.D3DImage" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is a software copy of the current state of the back buffer; otherwise, null if the back buffer cannot be read.</returns>
	protected internal virtual BitmapSource CopyBackBuffer()
	{
		BitmapSource result = null;
		if (_pInteropDeviceBitmap != null && HRESULT.Succeeded(UnsafeNativeMethods.InteropDeviceBitmap.GetAsSoftwareBitmap(_pInteropDeviceBitmap, out var pIWICBitmapSource)))
		{
			result = new CachedBitmap(pIWICBitmapSource);
		}
		return result;
	}

	private void CloneCommon(Freezable sourceFreezable)
	{
		D3DImage d3DImage = (D3DImage)sourceFreezable;
		_dpiX = d3DImage._dpiX;
		_dpiY = d3DImage._dpiY;
		Lock();
		SetBackBuffer(D3DResourceType.IDirect3DSurface9, d3DImage._pUserSurfaceUnsafe);
		Unlock();
	}

	private void SubscribeToCommittingBatch()
	{
		if (!_isWaitingForPresent)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			if (_duceResource.IsOnChannel(mediaContext.Channel))
			{
				mediaContext.CommittingBatch += _sendPresentDelegate;
				_isWaitingForPresent = true;
			}
		}
	}

	private void UnsubscribeFromCommittingBatch()
	{
		if (_isWaitingForPresent)
		{
			MediaContext.From(base.Dispatcher).CommittingBatch -= _sendPresentDelegate;
			_isWaitingForPresent = false;
		}
	}

	private bool LockImpl(Duration timeout)
	{
		bool result = false;
		if (_lockCount == uint.MaxValue)
		{
			throw new InvalidOperationException(SR.Image_LockCountLimit);
		}
		if (_lockCount == 0)
		{
			result = ((!(timeout == Duration.Forever)) ? _canWriteEvent.WaitOne(timeout.TimeSpan, exitContext: false) : _canWriteEvent.WaitOne());
			UnsubscribeFromCommittingBatch();
		}
		_lockCount++;
		return result;
	}

	private static void IsFrontBufferAvailablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		bool num = (bool)e.NewValue;
		D3DImage d3DImage = (D3DImage)d;
		if (!num && !d3DImage._isSoftwareFallbackEnabled)
		{
			d3DImage._pUserSurfaceUnsafe = IntPtr.Zero;
		}
		if (d3DImage._isFrontBufferAvailableChangedHandlers != null)
		{
			d3DImage._isFrontBufferAvailableChangedHandlers(d3DImage, e);
		}
	}

	private unsafe void SendPresent(object sender, EventArgs args)
	{
		if (!_waitingForUpdateResourceBecauseBitmapChanged)
		{
			UnsubscribeFromCommittingBatch();
			DUCE.Channel channel = sender as DUCE.Channel;
			DUCE.MILCMD_D3DIMAGE_PRESENT mILCMD_D3DIMAGE_PRESENT = default(DUCE.MILCMD_D3DIMAGE_PRESENT);
			mILCMD_D3DIMAGE_PRESENT.Type = MILCMD.MilCmdD3DImagePresent;
			mILCMD_D3DIMAGE_PRESENT.Handle = _duceResource.GetHandle(channel);
			nint currentProcess = MS.Win32.UnsafeNativeMethods.GetCurrentProcess();
			if (!MS.Win32.UnsafeNativeMethods.DuplicateHandle(currentProcess, _canWriteEvent.SafeWaitHandle, currentProcess, out var hTargetHandle, 0u, fInheritHandle: false, 2u))
			{
				throw new Win32Exception();
			}
			mILCMD_D3DIMAGE_PRESENT.hEvent = (ulong)((IntPtr)hTargetHandle).ToPointer();
			channel.SendCommand((byte*)(&mILCMD_D3DIMAGE_PRESENT), sizeof(DUCE.MILCMD_D3DIMAGE_PRESENT), sendInSeparateBatch: true);
			_isDirty = false;
			_canWriteEvent.Reset();
		}
	}

	private object SetIsFrontBufferAvailable(object isAvailableVersionPair)
	{
		Pair pair = (Pair)isAvailableVersionPair;
		if ((uint)pair.Second == _version)
		{
			bool value = (bool)pair.First;
			SetValue(IsFrontBufferAvailablePropertyKey, value);
		}
		return null;
	}

	private void Callback(bool isFrontBufferAvailable, uint version)
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(SetIsFrontBufferAvailable), new Pair(BooleanBoxes.Box(isFrontBufferAvailable), version));
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (!skipOnChannelCheck && !_duceResource.IsOnChannel(channel))
		{
			return;
		}
		base.UpdateResource(channel, skipOnChannelCheck);
		bool isSynchronous = channel.IsSynchronous;
		DUCE.MILCMD_D3DIMAGE mILCMD_D3DIMAGE = default(DUCE.MILCMD_D3DIMAGE);
		mILCMD_D3DIMAGE.Type = MILCMD.MilCmdD3DImage;
		mILCMD_D3DIMAGE.Handle = _duceResource.GetHandle(channel);
		if (_pInteropDeviceBitmap != null)
		{
			UnsafeNativeMethods.MILUnknown.AddRef(_pInteropDeviceBitmap);
			mILCMD_D3DIMAGE.pInteropDeviceBitmap = (ulong)((IntPtr)_pInteropDeviceBitmap.DangerousGetHandle()).ToPointer();
		}
		else
		{
			mILCMD_D3DIMAGE.pInteropDeviceBitmap = 0uL;
		}
		mILCMD_D3DIMAGE.pSoftwareBitmap = 0uL;
		if (isSynchronous)
		{
			_softwareCopy = CopyBackBuffer();
			if (_softwareCopy != null)
			{
				UnsafeNativeMethods.MILUnknown.AddRef(_softwareCopy.WicSourceHandle);
				mILCMD_D3DIMAGE.pSoftwareBitmap = (ulong)((IntPtr)_softwareCopy.WicSourceHandle.DangerousGetHandle()).ToPointer();
			}
		}
		channel.SendCommand((byte*)(&mILCMD_D3DIMAGE), sizeof(DUCE.MILCMD_D3DIMAGE), sendInSeparateBatch: false);
		if (!isSynchronous)
		{
			_waitingForUpdateResourceBecauseBitmapChanged = false;
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_D3DIMAGE))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
			if (!channel.IsSynchronous && _isDirty)
			{
				SubscribeToCommittingBatch();
			}
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			ReleaseOnChannelAnimations(channel);
			if (!channel.IsSynchronous)
			{
				UnsubscribeFromCommittingBatch();
			}
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	void IAppDomainShutdownListener.NotifyShutdown()
	{
		if (_pInteropDeviceBitmap != null)
		{
			UnsafeNativeMethods.InteropDeviceBitmap.Detach(_pInteropDeviceBitmap);
		}
		GC.SuppressFinalize(this);
	}
}
