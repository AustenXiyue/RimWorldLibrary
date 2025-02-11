using MS.Internal;
using MS.Internal.Composition;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class FactoryMaker : IDisposable
{
	private bool _disposed;

	private static nint s_pFactory;

	private static nint s_pImagingFactory;

	private static int s_cInstance = 0;

	private static readonly object s_factoryMakerLock = new object();

	private bool _fValidObject;

	internal nint FactoryPtr => s_pFactory;

	internal nint ImagingFactoryPtr
	{
		get
		{
			if (s_pImagingFactory == IntPtr.Zero)
			{
				lock (s_factoryMakerLock)
				{
					HRESULT.Check(UnsafeNativeMethods.WICCodec.CreateImagingFactory(566u, out s_pImagingFactory));
				}
			}
			return s_pImagingFactory;
		}
	}

	internal FactoryMaker()
	{
		lock (s_factoryMakerLock)
		{
			if (s_pFactory == IntPtr.Zero)
			{
				HRESULT.Check(UnsafeNativeMethods.MILFactory2.CreateFactory(out s_pFactory, MS.Internal.Composition.Version.MilSdkVersion));
			}
			s_cInstance++;
			_fValidObject = true;
		}
	}

	~FactoryMaker()
	{
		Dispose(fDisposing: false);
	}

	public void Dispose()
	{
		Dispose(fDisposing: true);
	}

	protected virtual void Dispose(bool fDisposing)
	{
		if (_disposed)
		{
			return;
		}
		if (_fValidObject)
		{
			lock (s_factoryMakerLock)
			{
				s_cInstance--;
				_fValidObject = false;
				if (s_cInstance == 0)
				{
					UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref s_pFactory);
					if (s_pImagingFactory != IntPtr.Zero)
					{
						UnsafeNativeMethods.MILUnknown.ReleaseInterface(ref s_pImagingFactory);
					}
					s_pFactory = IntPtr.Zero;
					s_pImagingFactory = IntPtr.Zero;
				}
			}
		}
		_disposed = true;
		if (fDisposing)
		{
			GC.SuppressFinalize(this);
		}
	}
}
