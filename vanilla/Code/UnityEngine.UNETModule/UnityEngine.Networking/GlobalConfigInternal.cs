using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[NativeHeader("Modules/UNET/UNETConfiguration.h")]
[NativeConditional("ENABLE_NETWORK && ENABLE_UNET", true)]
internal class GlobalConfigInternal : IDisposable
{
	public IntPtr m_Ptr;

	[NativeProperty("m_ThreadAwakeTimeout", TargetType.Field)]
	private extern uint ThreadAwakeTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_ReactorModel", TargetType.Field)]
	private extern byte ReactorModel
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_ReactorMaximumReceivedMessages", TargetType.Field)]
	private extern ushort ReactorMaximumReceivedMessages
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_ReactorMaximumSentMessages", TargetType.Field)]
	private extern ushort ReactorMaximumSentMessages
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MaxPacketSize", TargetType.Field)]
	private extern ushort MaxPacketSize
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MaxHosts", TargetType.Field)]
	private extern ushort MaxHosts
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_ThreadPoolSize", TargetType.Field)]
	private extern byte ThreadPoolSize
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MinTimerTimeout", TargetType.Field)]
	private extern uint MinTimerTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MaxTimerTimeout", TargetType.Field)]
	private extern uint MaxTimerTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MinNetSimulatorTimeout", TargetType.Field)]
	private extern uint MinNetSimulatorTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeProperty("m_MaxNetSimulatorTimeout", TargetType.Field)]
	private extern uint MaxNetSimulatorTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public GlobalConfigInternal(GlobalConfig config)
	{
		if (config == null)
		{
			throw new NullReferenceException("config is not defined");
		}
		m_Ptr = InternalCreate();
		ThreadAwakeTimeout = config.ThreadAwakeTimeout;
		ReactorModel = (byte)config.ReactorModel;
		ReactorMaximumReceivedMessages = config.ReactorMaximumReceivedMessages;
		ReactorMaximumSentMessages = config.ReactorMaximumSentMessages;
		MaxPacketSize = config.MaxPacketSize;
		MaxHosts = config.MaxHosts;
		if (config.ThreadPoolSize == 0 || config.ThreadPoolSize > 254)
		{
			throw new ArgumentOutOfRangeException("Worker thread pool size should be >= 1 && < 254 (for server only)");
		}
		byte threadPoolSize = config.ThreadPoolSize;
		if (config.ThreadPoolSize > 1)
		{
			Debug.LogWarning("Worker thread pool size can be > 1 only for server platforms: Win, OSX or Linux");
			threadPoolSize = 1;
		}
		ThreadPoolSize = threadPoolSize;
		MinTimerTimeout = config.MinTimerTimeout;
		MaxTimerTimeout = config.MaxTimerTimeout;
		MinNetSimulatorTimeout = config.MinNetSimulatorTimeout;
		MaxNetSimulatorTimeout = config.MaxNetSimulatorTimeout;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_Ptr != IntPtr.Zero)
		{
			InternalDestroy(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
	}

	~GlobalConfigInternal()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		if (m_Ptr != IntPtr.Zero)
		{
			InternalDestroy(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr InternalCreate();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	private static extern void InternalDestroy(IntPtr ptr);
}
