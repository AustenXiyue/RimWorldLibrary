using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Interop;
using MS.Internal.KnownBoxes;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace System.Windows;

/// <summary>Provides a startup screen for a Windows Presentation Foundation (WPF) application. </summary>
public class SplashScreen
{
	private nint _hwnd = IntPtr.Zero;

	private string _resourceName;

	private nint _hInstance;

	private NativeMethods.BitmapHandle _hBitmap;

	private ushort _wndClass;

	private DispatcherTimer _dt;

	private TimeSpan _fadeoutDuration;

	private DateTime _fadeoutEnd;

	private NativeMethods.BLENDFUNCTION _blendFunc;

	private ResourceManager _resourceManager;

	private Dispatcher _dispatcher;

	private static NativeMethods.WndProc _defWndProc;

	private const string CLASSNAME = "SplashScreen";

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.SplashScreen" /> class with the specified resource. </summary>
	/// <param name="resourceName">The name of the embedded resource.</param>
	public SplashScreen(string resourceName)
		: this(Assembly.GetEntryAssembly(), resourceName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.SplashScreen" /> class with the specified resource assembly. </summary>
	/// <param name="resourceAssembly">The assembly that contains the embedded resource.</param>
	/// <param name="resourceName">The name of the embedded resource.</param>
	public SplashScreen(Assembly resourceAssembly, string resourceName)
	{
		if (resourceAssembly == null)
		{
			throw new ArgumentNullException("resourceAssembly");
		}
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentNullException("resourceName");
		}
		_resourceName = resourceName.ToLowerInvariant();
		_hInstance = Marshal.GetHINSTANCE(resourceAssembly.ManifestModule);
		AssemblyName assemblyName = new AssemblyName(resourceAssembly.FullName);
		_resourceManager = new ResourceManager(assemblyName.Name + ".g", resourceAssembly);
	}

	/// <summary>Displays the splash screen. </summary>
	/// <param name="autoClose">true to automatically close the splash screen; false to close the splash screen manually.</param>
	/// <exception cref="T:System.IO.IOException">The resource specified in the constructor could not be found. </exception>
	public void Show(bool autoClose)
	{
		Show(autoClose, topMost: false);
	}

	/// <summary>Displays the splash screen.</summary>
	/// <param name="autoClose">true to automatically close the splash screen; false to close the splash screen manually.</param>
	/// <param name="topMost">true if the splash screen window should use the WS_EX_TOPMOST style; otherwise false.</param>
	public unsafe void Show(bool autoClose, bool topMost)
	{
		if (_hwnd != IntPtr.Zero)
		{
			return;
		}
		UnmanagedMemoryStream resourceStream;
		using (resourceStream = GetResourceStream())
		{
			if (resourceStream != null)
			{
				resourceStream.Seek(0L, SeekOrigin.Begin);
				nint pImgBuffer = new IntPtr(resourceStream.PositionPointer);
				if (CreateLayeredWindowFromImgBuffer(pImgBuffer, resourceStream.Length, topMost) && autoClose)
				{
					Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, (DispatcherOperationCallback)delegate(object arg)
					{
						((SplashScreen)arg).Close(TimeSpan.FromSeconds(0.3));
						return (object)null;
					}, this);
				}
				_dispatcher = Dispatcher.CurrentDispatcher;
				return;
			}
			throw new IOException(SR.Format(SR.UnableToLocateResource, _resourceName));
		}
	}

	private UnmanagedMemoryStream GetResourceStream()
	{
		UnmanagedMemoryStream stream = _resourceManager.GetStream(_resourceName, CultureInfo.CurrentUICulture);
		if (stream != null)
		{
			return stream;
		}
		string resourceIDFromRelativePath = ResourceIDHelper.GetResourceIDFromRelativePath(_resourceName);
		return _resourceManager.GetStream(resourceIDFromRelativePath, CultureInfo.CurrentUICulture);
	}

	private unsafe nint CreateWindow(NativeMethods.BitmapHandle hBitmap, int width, int height, bool topMost)
	{
		if (_defWndProc == null)
		{
			_defWndProc = UnsafeNativeMethods.DefWindowProc;
		}
		NativeMethods.WNDCLASSEX_D wNDCLASSEX_D = new NativeMethods.WNDCLASSEX_D();
		wNDCLASSEX_D.cbSize = Marshal.SizeOf(typeof(NativeMethods.WNDCLASSEX_D));
		wNDCLASSEX_D.style = 3;
		wNDCLASSEX_D.lpfnWndProc = null;
		wNDCLASSEX_D.hInstance = _hInstance;
		wNDCLASSEX_D.hCursor = IntPtr.Zero;
		wNDCLASSEX_D.lpszClassName = "SplashScreen";
		wNDCLASSEX_D.lpszMenuName = string.Empty;
		wNDCLASSEX_D.lpfnWndProc = _defWndProc;
		_wndClass = UnsafeNativeMethods.IntRegisterClassEx(wNDCLASSEX_D);
		if (_wndClass == 0)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 1410)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
		int systemMetrics = UnsafeNativeMethods.GetSystemMetrics(SM.CXSCREEN);
		int systemMetrics2 = UnsafeNativeMethods.GetSystemMetrics(SM.CYSCREEN);
		int x = (systemMetrics - width) / 2;
		int y = (systemMetrics2 - height) / 2;
		HandleRef handleRef = new HandleRef(null, IntPtr.Zero);
		nint num = UnsafeNativeMethods.CreateWindowEx(0x80180 | (topMost ? 8 : 0), "SplashScreen", SR.SplashScreenIsLoading, -1879048192, x, y, width, height, handleRef, handleRef, new HandleRef(null, _hInstance), IntPtr.Zero);
		nint dC = UnsafeNativeMethods.GetDC(default(HandleRef));
		nint num2 = UnsafeNativeMethods.CreateCompatibleDC(new HandleRef(null, dC));
		nint obj = UnsafeNativeMethods.SelectObject(new HandleRef(null, num2), hBitmap.MakeHandleRef(null).Handle);
		NativeMethods.POINT pOINT = new NativeMethods.POINT(width, height);
		NativeMethods.POINT pOINT2 = new NativeMethods.POINT(x, y);
		NativeMethods.POINT pOINT3 = new NativeMethods.POINT(0, 0);
		_blendFunc = default(NativeMethods.BLENDFUNCTION);
		_blendFunc.BlendOp = 0;
		_blendFunc.BlendFlags = 0;
		_blendFunc.SourceConstantAlpha = byte.MaxValue;
		_blendFunc.AlphaFormat = 1;
		bool num3 = UnsafeNativeMethods.UpdateLayeredWindow(num, dC, &pOINT2, &pOINT, num2, &pOINT3, 0, ref _blendFunc, 2);
		UnsafeNativeMethods.SelectObject(new HandleRef(null, num2), obj);
		UnsafeNativeMethods.ReleaseDC(default(HandleRef), new HandleRef(null, num2));
		UnsafeNativeMethods.ReleaseDC(default(HandleRef), new HandleRef(null, dC));
		if (!num3)
		{
			UnsafeNativeMethods.HRESULT.Check(Marshal.GetHRForLastWin32Error());
		}
		return num;
	}

	/// <summary>Closes the splash screen. </summary>
	/// <param name="fadeoutDuration">A <see cref="T:System.TimeSpan" /> that specifies how long it will take for the splash screen to fade after the close operation has been initiated. </param>
	public void Close(TimeSpan fadeoutDuration)
	{
		object obj = null;
		if (_dispatcher != null)
		{
			obj = ((!_dispatcher.CheckAccess()) ? _dispatcher.Invoke(DispatcherPriority.Normal, new DispatcherOperationCallback(CloseInternal), fadeoutDuration) : CloseInternal(fadeoutDuration));
		}
		if (obj != BooleanBoxes.TrueBox)
		{
			DestroyResources();
		}
	}

	private object CloseInternal(object fadeOutArg)
	{
		TimeSpan timeSpan = (TimeSpan)fadeOutArg;
		if (timeSpan <= TimeSpan.Zero)
		{
			DestroyResources();
			return BooleanBoxes.TrueBox;
		}
		if (_dt != null || _hwnd == IntPtr.Zero)
		{
			return BooleanBoxes.TrueBox;
		}
		if (UnsafeNativeMethods.SetActiveWindow(new HandleRef(null, _hwnd)) == IntPtr.Zero)
		{
			DestroyResources();
			return BooleanBoxes.TrueBox;
		}
		_dt = new DispatcherTimer();
		_dt.Interval = TimeSpan.FromMilliseconds(30.0);
		_fadeoutDuration = timeSpan;
		_fadeoutEnd = DateTime.UtcNow + _fadeoutDuration;
		_dt.Tick += Fadeout_Tick;
		_dt.Start();
		return BooleanBoxes.TrueBox;
	}

	private unsafe void Fadeout_Tick(object unused, EventArgs args)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow >= _fadeoutEnd)
		{
			DestroyResources();
			return;
		}
		double num = (_fadeoutEnd - utcNow).TotalMilliseconds / _fadeoutDuration.TotalMilliseconds;
		_blendFunc.SourceConstantAlpha = (byte)(255.0 * num);
		UnsafeNativeMethods.UpdateLayeredWindow(_hwnd, IntPtr.Zero, null, null, IntPtr.Zero, null, 0, ref _blendFunc, 2);
	}

	private void DestroyResources()
	{
		if (_dt != null)
		{
			_dt.Stop();
			_dt = null;
		}
		if (_hwnd != IntPtr.Zero)
		{
			HandleRef hWnd = new HandleRef(null, _hwnd);
			if (UnsafeNativeMethods.IsWindow(hWnd))
			{
				UnsafeNativeMethods.IntDestroyWindow(hWnd);
			}
			_hwnd = IntPtr.Zero;
		}
		if (_hBitmap != null && !_hBitmap.IsClosed)
		{
			UnsafeNativeMethods.DeleteObject(_hBitmap.MakeHandleRef(null).Handle);
			_hBitmap.Close();
			_hBitmap = null;
		}
		if (_wndClass != 0)
		{
			if (UnsafeNativeMethods.IntUnregisterClass(new IntPtr(_wndClass), _hInstance) != 0)
			{
				_defWndProc = null;
			}
			_wndClass = 0;
		}
		if (_resourceManager != null)
		{
			_resourceManager.ReleaseAllResources();
		}
	}

	private bool CreateLayeredWindowFromImgBuffer(nint pImgBuffer, long cImgBufferLen, bool topMost)
	{
		bool flag = false;
		nint ppICodecFactory = IntPtr.Zero;
		nint ppIDecode = IntPtr.Zero;
		nint ppIStream = IntPtr.Zero;
		nint ppIFrameDecode = IntPtr.Zero;
		nint ppFormatConverter = IntPtr.Zero;
		nint ppBitmapFlipRotator = IntPtr.Zero;
		try
		{
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CreateImagingFactory(566u, out ppICodecFactory));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CreateStream(ppICodecFactory, out ppIStream));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.InitializeStreamFromMemory(ppIStream, pImgBuffer, (uint)cImgBufferLen));
			Guid guidVendor = Guid.Empty;
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CreateDecoderFromStream(ppICodecFactory, ppIStream, ref guidVendor, 0u, out ppIDecode));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.GetFrame(ppIDecode, 0u, out ppIFrameDecode));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CreateFormatConverter(ppICodecFactory, out ppFormatConverter));
			Guid dstFormat = UnsafeNativeMethods.WIC.WICPixelFormat32bppPBGRA;
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.InitializeFormatConverter(ppFormatConverter, ppIFrameDecode, ref dstFormat, 0, IntPtr.Zero, 0.0, UnsafeNativeMethods.WIC.WICPaletteType.WICPaletteTypeCustom));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CreateBitmapFlipRotator(ppICodecFactory, out ppBitmapFlipRotator));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.InitializeBitmapFlipRotator(ppBitmapFlipRotator, ppFormatConverter, UnsafeNativeMethods.WIC.WICBitmapTransformOptions.WICBitmapTransformFlipVertical));
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.GetBitmapSize(ppBitmapFlipRotator, out var puiWidth, out var puiHeight));
			int num = puiWidth * 4;
			NativeMethods.BITMAPINFO bitmapInfo = new NativeMethods.BITMAPINFO(puiWidth, puiHeight, 32);
			bitmapInfo.bmiHeader_biCompression = 0;
			bitmapInfo.bmiHeader_biSizeImage = num * puiHeight;
			nint ppvBits = IntPtr.Zero;
			_hBitmap = UnsafeNativeMethods.CreateDIBSection(default(HandleRef), ref bitmapInfo, 0, ref ppvBits, null, 0);
			Int32Rect prc = new Int32Rect(0, 0, puiWidth, puiHeight);
			UnsafeNativeMethods.HRESULT.Check(UnsafeNativeMethods.WIC.CopyPixels(ppBitmapFlipRotator, ref prc, num, num * puiHeight, ppvBits));
			_hwnd = CreateWindow(_hBitmap, puiWidth, puiHeight, topMost);
			flag = true;
		}
		finally
		{
			if (ppICodecFactory != IntPtr.Zero)
			{
				Marshal.Release(ppICodecFactory);
			}
			if (ppIDecode != IntPtr.Zero)
			{
				Marshal.Release(ppIDecode);
			}
			if (ppIStream != IntPtr.Zero)
			{
				Marshal.Release(ppIStream);
			}
			if (ppIFrameDecode != IntPtr.Zero)
			{
				Marshal.Release(ppIFrameDecode);
			}
			if (ppFormatConverter != IntPtr.Zero)
			{
				Marshal.Release(ppFormatConverter);
			}
			if (ppBitmapFlipRotator != IntPtr.Zero)
			{
				Marshal.Release(ppBitmapFlipRotator);
			}
			if (!flag)
			{
				DestroyResources();
			}
		}
		return flag;
	}
}
