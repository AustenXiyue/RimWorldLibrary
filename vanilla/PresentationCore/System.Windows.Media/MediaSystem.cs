using System.Collections;
using System.Threading;
using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Composition;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal static class MediaSystem
{
	private static int s_refCount = 0;

	private static ArrayList _mediaContexts = new ArrayList();

	private static bool s_isConnected = false;

	private static DUCE.Channel s_serviceChannel;

	private static bool s_animationSmoothing = true;

	private static nint s_pConnection;

	private static bool s_forceSoftareForGraphicsStreamMagnifier;

	private static int s_DisableDirtyRectangles = 0;

	internal static bool DisableDirtyRectangles => s_DisableDirtyRectangles != 0;

	internal static bool IsTransportConnected
	{
		get
		{
			return s_isConnected;
		}
		set
		{
			s_isConnected = value;
		}
	}

	internal static bool ForceSoftwareRendering
	{
		get
		{
			using (CompositionEngineLock.Acquire())
			{
				return s_forceSoftareForGraphicsStreamMagnifier;
			}
		}
	}

	internal static DUCE.Channel ServiceChannel => s_serviceChannel;

	internal static nint Connection => s_pConnection;

	internal static bool AnimationSmoothing => s_animationSmoothing;

	public static bool Startup(MediaContext mc)
	{
		HRESULT.Check(UnsafeNativeMethods.MilCoreApi.MilVersionCheck(MS.Internal.Composition.Version.MilSdkVersion));
		using (CompositionEngineLock.Acquire())
		{
			_mediaContexts.Add(mc);
			if (s_refCount == 0)
			{
				HRESULT.Check(SafeNativeMethods.MilCompositionEngine_InitializePartitionManager(0));
				s_forceSoftareForGraphicsStreamMagnifier = UnsafeNativeMethods.MilCoreApi.WgxConnection_ShouldForceSoftwareForGraphicsStreamClient();
				ConnectTransport();
				ReadAnimationSmoothingSetting();
			}
			s_refCount++;
		}
		UnsafeNativeMethods.MilCoreApi.RenderOptions_EnableHardwareAccelerationInRdp(CoreAppContextSwitches.EnableHardwareAccelerationInRdp);
		return true;
	}

	internal static bool ConnectChannels(MediaContext mc)
	{
		bool result = false;
		using (CompositionEngineLock.Acquire())
		{
			if (IsTransportConnected)
			{
				mc.CreateChannels();
				result = true;
			}
		}
		return result;
	}

	private static void ReadAnimationSmoothingSetting()
	{
	}

	internal static void Shutdown(MediaContext mc)
	{
		using (CompositionEngineLock.Acquire())
		{
			_mediaContexts.Remove(mc);
			s_refCount--;
			if (s_refCount == 0)
			{
				if (IsTransportConnected)
				{
					DisconnectTransport();
				}
				HRESULT.Check(SafeNativeMethods.MilCompositionEngine_DeinitializePartitionManager());
			}
		}
	}

	internal static void PropagateDirtyRectangleSettings()
	{
		int num = s_DisableDirtyRectangles;
		int num2 = (CoreAppContextSwitches.DisableDirtyRectangles ? 1 : 0);
		if (num2 != num && Interlocked.CompareExchange(ref s_DisableDirtyRectangles, num2, num) == num)
		{
			NotifyRedirectionEnvironmentChanged();
		}
	}

	internal static void NotifyRedirectionEnvironmentChanged()
	{
		using (CompositionEngineLock.Acquire())
		{
			s_forceSoftareForGraphicsStreamMagnifier = UnsafeNativeMethods.MilCoreApi.WgxConnection_ShouldForceSoftwareForGraphicsStreamClient();
			foreach (MediaContext mediaContext in _mediaContexts)
			{
				mediaContext.PostInvalidateRenderMode();
			}
		}
	}

	private static void ConnectTransport()
	{
		if (IsTransportConnected)
		{
			throw new InvalidOperationException(SR.MediaSystem_OutOfOrderConnectOrDisconnect);
		}
		HRESULT.Check(UnsafeNativeMethods.MilCoreApi.WgxConnection_Create(requestSynchronousTransport: false, out s_pConnection));
		s_serviceChannel = new DUCE.Channel(null, isOutOfBandChannel: false, s_pConnection, isSynchronous: false);
		IsTransportConnected = true;
	}

	private static void DisconnectTransport()
	{
		if (IsTransportConnected)
		{
			s_serviceChannel.Close();
			HRESULT.Check(UnsafeNativeMethods.MilCoreApi.WgxConnection_Disconnect(s_pConnection));
			s_serviceChannel = null;
			s_pConnection = IntPtr.Zero;
			IsTransportConnected = false;
		}
	}

	internal static void AssertSameContext(DispatcherObject reference, DispatcherObject other)
	{
		if (other != null && reference.Dispatcher != null && other.Dispatcher != null && reference.Dispatcher != other.Dispatcher)
		{
			throw new ArgumentException(SR.MediaSystem_ApiInvalidContext);
		}
	}
}
