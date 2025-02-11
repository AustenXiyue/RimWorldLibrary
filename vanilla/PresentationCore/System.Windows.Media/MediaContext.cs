using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;
using MS.Win32;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class MediaContext : DispatcherObject, IDisposable, IClock
{
	private struct ChannelManager
	{
		private DUCE.Channel _asyncChannel;

		private DUCE.Channel _asyncOutOfBandChannel;

		private Queue<DUCE.Channel> _freeSyncChannels;

		private DUCE.Channel _syncServiceChannel;

		private nint _pSyncConnection;

		internal DUCE.Channel Channel => _asyncChannel;

		internal DUCE.Channel OutOfBandChannel => _asyncOutOfBandChannel;

		internal void CreateChannels()
		{
			Invariant.Assert(_asyncChannel == null);
			Invariant.Assert(_asyncOutOfBandChannel == null);
			_asyncChannel = new DUCE.Channel(MediaSystem.ServiceChannel, isOutOfBandChannel: false, MediaSystem.Connection, isSynchronous: false);
			_asyncOutOfBandChannel = new DUCE.Channel(MediaSystem.ServiceChannel, isOutOfBandChannel: true, MediaSystem.Connection, isSynchronous: false);
		}

		internal void RemoveSyncChannels()
		{
			if (_freeSyncChannels != null)
			{
				while (_freeSyncChannels.Count > 0)
				{
					_freeSyncChannels.Dequeue().Close();
				}
				_freeSyncChannels = null;
			}
			if (_syncServiceChannel != null)
			{
				_syncServiceChannel.Close();
				_syncServiceChannel = null;
			}
		}

		internal void RemoveChannels()
		{
			if (_asyncChannel != null)
			{
				_asyncChannel.Close();
				_asyncChannel = null;
			}
			if (_asyncOutOfBandChannel != null)
			{
				_asyncOutOfBandChannel.Close();
				_asyncOutOfBandChannel = null;
			}
			RemoveSyncChannels();
			if (_pSyncConnection != IntPtr.Zero)
			{
				HRESULT.Check(UnsafeNativeMethods.MilCoreApi.WgxConnection_Disconnect(_pSyncConnection));
				_pSyncConnection = IntPtr.Zero;
			}
		}

		internal DUCE.Channel AllocateSyncChannel()
		{
			if (_pSyncConnection == IntPtr.Zero)
			{
				HRESULT.Check(UnsafeNativeMethods.MilCoreApi.WgxConnection_Create(requestSynchronousTransport: true, out _pSyncConnection));
			}
			if (_freeSyncChannels == null)
			{
				_freeSyncChannels = new Queue<DUCE.Channel>(3);
			}
			if (_freeSyncChannels.Count > 0)
			{
				return _freeSyncChannels.Dequeue();
			}
			if (_syncServiceChannel == null)
			{
				_syncServiceChannel = new DUCE.Channel(null, isOutOfBandChannel: false, _pSyncConnection, isSynchronous: true);
			}
			return new DUCE.Channel(_syncServiceChannel, isOutOfBandChannel: false, _pSyncConnection, isSynchronous: true);
		}

		internal void ReleaseSyncChannel(DUCE.Channel channel)
		{
			Invariant.Assert(_freeSyncChannels != null);
			if (_freeSyncChannels.Count <= 3)
			{
				_freeSyncChannels.Enqueue(channel);
			}
			else
			{
				channel.Close();
			}
		}
	}

	private class InvokeOnRenderCallback
	{
		private DispatcherOperationCallback _callback;

		private object _arg;

		public InvokeOnRenderCallback(DispatcherOperationCallback callback, object arg)
		{
			_callback = callback;
			_arg = arg;
		}

		public void DoWork()
		{
			_callback(_arg);
		}
	}

	internal delegate void ResourcesUpdatedHandler(DUCE.Channel channel, bool skipOnChannelCheck);

	private enum InterlockState
	{
		Disabled,
		RequestedStart,
		Idle,
		WaitingForResponse,
		WaitingForNextFrame
	}

	private TimeManager _timeManager;

	private bool _isDisposed;

	private EventHandler _destroyHandler;

	private Guid _contextGuid;

	private DispatcherOperation _currentRenderOp;

	private DispatcherOperation _inputMarkerOp;

	private DispatcherOperationCallback _renderMessage;

	private DispatcherOperationCallback _animRenderMessage;

	private DispatcherOperationCallback _inputMarkerMessage;

	private DispatcherOperationCallback _renderModeMessage;

	private DispatcherTimer _promoteRenderOpToInput;

	private DispatcherTimer _promoteRenderOpToRender;

	private DispatcherTimer _estimatedNextVSyncTimer;

	private ChannelManager _channelManager;

	private DUCE.Resource _uceEtwEvent;

	private bool _isRendering;

	private bool _isDisconnecting;

	private bool _isConnected;

	private FrugalObjectList<InvokeOnRenderCallback> _invokeOnRenderCallbacks;

	private HashSet<ICompositionTarget> _registeredICompositionTargets;

	private int _readOnlyAccessCounter;

	private BoundsDrawingContextWalker _cachedBoundsDrawingContextWalker = new BoundsDrawingContextWalker();

	private static int _contextRenderID;

	private int _tier;

	private FrugalObjectList<LoadedOrUnloadedOperation> _loadedOrUnloadedPendingOperations;

	private TimeSpan _timeDelay = TimeSpan.FromMilliseconds(10.0);

	private bool _commitPendingAfterRender;

	private MediaContextNotificationWindow _notificationWindow;

	private DUCE.ChannelSet? _currentRenderingChannel;

	private InterlockState _interlockState;

	private bool _needToCommitChannel;

	private long _lastPresentationTime;

	private long _lastCommitTime;

	private long _lastInputMarkerTime;

	private long _averagePresentationInterval;

	private TimeSpan _estimatedNextPresentationTime;

	private int _displayRefreshRate;

	private int _adjustedRefreshRate;

	private int _animationRenderRate;

	private MIL_PRESENTATION_RESULTS _lastPresentationResults = MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_VSYNC_UNSUPPORTED;

	private static long _perfCounterFreq;

	private const long MaxTicksWithoutInput = 5000000L;

	internal static bool IsClockSupported => _perfCounterFreq != 0;

	internal static bool ShouldRenderEvenWhenNoDisplayDevicesAreAvailable { get; }

	internal int Tier => _tier;

	internal uint PixelShaderVersion { get; private set; }

	internal uint MaxPixelShader30InstructionSlots { get; private set; }

	internal bool HasSSE2Support { get; private set; }

	internal Size MaxTextureSize { get; private set; }

	private bool HasCommittedThisVBlankInterval
	{
		get
		{
			if (_animationRenderRate == 0)
			{
				return false;
			}
			if (CurrentTicks - _lastCommitTime < RefreshPeriod && _lastCommitTime > CountsToTicks(_lastPresentationTime))
			{
				return true;
			}
			return false;
		}
	}

	internal static long CurrentTicks
	{
		get
		{
			MS.Win32.SafeNativeMethods.QueryPerformanceCounter(out var lpPerformanceCount);
			return CountsToTicks(lpPerformanceCount);
		}
	}

	private long RefreshPeriod => 10000000L / (long)_animationRenderRate;

	TimeSpan IClock.CurrentTime
	{
		get
		{
			MS.Win32.SafeNativeMethods.QueryPerformanceCounter(out var lpPerformanceCount);
			long num = CountsToTicks(lpPerformanceCount);
			if (_interlockState != 0)
			{
				if (_animationRenderRate != 0 && _lastPresentationResults != MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_VSYNC_UNSUPPORTED)
				{
					_averagePresentationInterval = RefreshPeriod;
					long num2 = 0L;
					if (InterlockIsWaiting)
					{
						num2 = 1L;
					}
					long num3 = num + TicksUntilNextVsync(num) + num2 * RefreshPeriod;
					if ((num3 - _estimatedNextPresentationTime.Ticks) * _animationRenderRate > TimeSpan.FromMilliseconds(500.0).Ticks)
					{
						_estimatedNextPresentationTime = TimeSpan.FromTicks(num3);
					}
				}
				else
				{
					_estimatedNextPresentationTime = TimeSpan.FromTicks(num);
				}
			}
			else
			{
				_estimatedNextPresentationTime = TimeSpan.FromTicks(num);
			}
			return _estimatedNextPresentationTime;
		}
	}

	internal static MediaContext CurrentMediaContext => From(Dispatcher.CurrentDispatcher);

	private int InvokeOnRenderCallbacksCount
	{
		get
		{
			if (_invokeOnRenderCallbacks == null)
			{
				return 0;
			}
			return _invokeOnRenderCallbacks.Count;
		}
	}

	internal bool WriteAccessEnabled => _readOnlyAccessCounter <= 0;

	internal TimeManager TimeManager => _timeManager;

	internal DUCE.Channel Channel => _channelManager.Channel;

	internal DUCE.Channel OutOfBandChannel => _channelManager.OutOfBandChannel;

	internal bool IsConnected => _isConnected;

	private bool InterlockIsWaiting
	{
		get
		{
			if (_interlockState != InterlockState.WaitingForNextFrame)
			{
				return _interlockState == InterlockState.WaitingForResponse;
			}
			return true;
		}
	}

	private bool InterlockIsEnabled
	{
		get
		{
			if (_interlockState != 0)
			{
				return _interlockState != InterlockState.RequestedStart;
			}
			return false;
		}
	}

	internal event EventHandler InvalidPixelShaderEncountered;

	internal event EventHandler TierChanged;

	internal event EventHandler RenderComplete
	{
		add
		{
			if (_commitPendingAfterRender)
			{
				_commitPendingAfterRender = false;
			}
			_renderCompleteHandlers += value;
		}
		remove
		{
			_renderCompleteHandlers -= value;
		}
	}

	internal event ResourcesUpdatedHandler ResourcesUpdated
	{
		add
		{
			_resourcesUpdatedHandlers += value;
		}
		remove
		{
			_resourcesUpdatedHandlers -= value;
		}
	}

	private event EventHandler _renderCompleteHandlers;

	private event ResourcesUpdatedHandler _resourcesUpdatedHandlers;

	internal event EventHandler Rendering;

	internal event EventHandler CommittingBatch;

	static MediaContext()
	{
		ShouldRenderEvenWhenNoDisplayDevicesAreAvailable = ((!Environment.UserInteractive) ? (!CoreAppContextSwitches.ShouldNotRenderInNonInteractiveWindowStation) : CoreAppContextSwitches.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable);
		_contextRenderID = 0;
		MS.Win32.SafeNativeMethods.QueryPerformanceFrequency(out _perfCounterFreq);
		long lpPerformanceCount;
		if (IsClockSupported)
		{
			MS.Win32.SafeNativeMethods.QueryPerformanceCounter(out lpPerformanceCount);
		}
		else
		{
			lpPerformanceCount = 0L;
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientQPCFrequency, _perfCounterFreq, lpPerformanceCount);
	}

	private static long CountsToTicks(long counts)
	{
		return 10000000 * (counts / _perfCounterFreq) + 10000000 * (counts % _perfCounterFreq) / _perfCounterFreq;
	}

	private static long TicksToCounts(long ticks)
	{
		return _perfCounterFreq * (ticks / 10000000) + _perfCounterFreq * (ticks % 10000000) / 10000000;
	}

	private static bool IsPrime(int number)
	{
		if ((number & 1) == 0)
		{
			return false;
		}
		int num = (int)Math.Sqrt(number);
		for (int i = 3; i <= num; i += 2)
		{
			if (number % i == 0)
			{
				return false;
			}
		}
		return true;
	}

	private static int FindNextPrime(int number)
	{
		while (!IsPrime(++number))
		{
		}
		return number;
	}

	internal MediaContext(Dispatcher dispatcher)
	{
		if (IsClockSupported)
		{
			MS.Win32.SafeNativeMethods.QueryPerformanceCounter(out _lastPresentationTime);
			_estimatedNextPresentationTime = TimeSpan.FromTicks(CountsToTicks(_lastPresentationTime));
		}
		_contextGuid = Guid.NewGuid();
		_registeredICompositionTargets = new HashSet<ICompositionTarget>();
		_renderModeMessage = InvalidateRenderMode;
		_notificationWindow = new MediaContextNotificationWindow(this);
		if (MediaSystem.Startup(this))
		{
			_isConnected = MediaSystem.ConnectChannels(this);
		}
		_destroyHandler = OnDestroyContext;
		base.Dispatcher.ShutdownFinished += _destroyHandler;
		_renderMessage = RenderMessageHandler;
		_animRenderMessage = AnimatedRenderMessageHandler;
		_inputMarkerMessage = InputMarkerMessageHandler;
		dispatcher.Reserved0 = this;
		_timeManager = new TimeManager();
		_timeManager.Start();
		_timeManager.NeedTickSooner += OnNeedTickSooner;
		_promoteRenderOpToInput = new DispatcherTimer(DispatcherPriority.Render);
		_promoteRenderOpToInput.Tick += PromoteRenderOpToInput;
		_promoteRenderOpToRender = new DispatcherTimer(DispatcherPriority.Render);
		_promoteRenderOpToRender.Tick += PromoteRenderOpToRender;
		_estimatedNextVSyncTimer = new DispatcherTimer(DispatcherPriority.Render);
		_estimatedNextVSyncTimer.Tick += EstimatedNextVSyncTimeExpired;
		_commitPendingAfterRender = false;
	}

	internal void NotifySyncChannelMessage(DUCE.Channel channel)
	{
		DUCE.MilMessage.Message message;
		while (channel.PeekNextMessage(out message))
		{
			switch (message.Type)
			{
			case DUCE.MilMessage.Type.PartitionIsZombie:
				_channelManager.RemoveSyncChannels();
				NotifyPartitionIsZombie(message.HRESULTFailure.HRESULTFailureCode);
				break;
			default:
				HandleInvalidPacketNotification();
				break;
			case DUCE.MilMessage.Type.Caps:
			case DUCE.MilMessage.Type.SyncModeStatus:
			case DUCE.MilMessage.Type.Presented:
				break;
			}
		}
	}

	internal void NotifyChannelMessage()
	{
		if (Channel == null)
		{
			return;
		}
		DUCE.MilMessage.Message message;
		while (Channel.PeekNextMessage(out message))
		{
			switch (message.Type)
			{
			case DUCE.MilMessage.Type.Caps:
				NotifySetCaps(message.Caps.Caps);
				break;
			case DUCE.MilMessage.Type.SyncModeStatus:
				NotifySyncModeStatus(message.SyncModeStatus.Enabled);
				break;
			case DUCE.MilMessage.Type.Presented:
				NotifyPresented(message.Presented.PresentationResults, message.Presented.PresentationTime, message.Presented.RefreshRate);
				break;
			case DUCE.MilMessage.Type.PartitionIsZombie:
				NotifyPartitionIsZombie(message.HRESULTFailure.HRESULTFailureCode);
				break;
			case DUCE.MilMessage.Type.BadPixelShader:
				NotifyBadPixelShader();
				break;
			default:
				HandleInvalidPacketNotification();
				break;
			}
		}
	}

	internal void PostInvalidateRenderMode()
	{
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, _renderModeMessage, null);
	}

	private object InvalidateRenderMode(object dontCare)
	{
		foreach (ICompositionTarget registeredICompositionTarget in _registeredICompositionTargets)
		{
			if (registeredICompositionTarget is HwndTarget hwndTarget)
			{
				hwndTarget.InvalidateRenderMode();
			}
		}
		return null;
	}

	private void NotifySetCaps(MilGraphicsAccelerationCaps caps)
	{
		PixelShaderVersion = caps.PixelShaderVersion;
		MaxPixelShader30InstructionSlots = caps.MaxPixelShader30InstructionSlots;
		HasSSE2Support = Convert.ToBoolean(caps.HasSSE2Support);
		MaxTextureSize = new Size(caps.MaxTextureWidth, caps.MaxTextureHeight);
		int tierValue = caps.TierValue;
		if (_tier != tierValue)
		{
			_tier = tierValue;
			if (this.TierChanged != null)
			{
				this.TierChanged(null, null);
			}
		}
	}

	private void NotifyBadPixelShader()
	{
		if (this.InvalidPixelShaderEncountered != null)
		{
			this.InvalidPixelShaderEncountered(null, null);
			return;
		}
		throw new InvalidOperationException(SR.MediaContext_NoBadShaderHandler);
	}

	private void NotifyPartitionIsZombie(int failureCode)
	{
		switch (failureCode)
		{
		case -2147024882:
			throw new OutOfMemoryException();
		case -2005532292:
			throw new OutOfMemoryException(SR.MediaContext_OutOfVideoMemory);
		default:
			throw new InvalidOperationException(SR.MediaContext_RenderThreadError);
		}
	}

	private void HandleInvalidPacketNotification()
	{
	}

	private unsafe void RequestTier(DUCE.Channel channel)
	{
		DUCE.MILCMD_CHANNEL_REQUESTTIER mILCMD_CHANNEL_REQUESTTIER = default(DUCE.MILCMD_CHANNEL_REQUESTTIER);
		mILCMD_CHANNEL_REQUESTTIER.Type = MILCMD.MilCmdChannelRequestTier;
		mILCMD_CHANNEL_REQUESTTIER.ReturnCommonMinimum = 0u;
		channel.SendCommand((byte*)(&mILCMD_CHANNEL_REQUESTTIER), sizeof(DUCE.MILCMD_CHANNEL_REQUESTTIER));
	}

	private void ScheduleNextRenderOp(TimeSpan minimumDelay)
	{
		if (_isDisconnecting || _needToCommitChannel)
		{
			return;
		}
		TimeSpan timeSpan = TimeSpan.Zero;
		if (this.Rendering == null)
		{
			timeSpan = _timeManager.GetNextTickNeeded();
		}
		if (timeSpan >= TimeSpan.Zero)
		{
			timeSpan = TimeSpan.FromTicks(Math.Max(timeSpan.Ticks, minimumDelay.Ticks));
			EnterInterlockedPresentation();
		}
		else
		{
			LeaveInterlockedPresentation();
		}
		if (timeSpan > TimeSpan.FromSeconds(1.0))
		{
			if (_currentRenderOp == null)
			{
				_currentRenderOp = base.Dispatcher.BeginInvoke(DispatcherPriority.Inactive, _animRenderMessage, null);
				_promoteRenderOpToRender.Interval = timeSpan;
				_promoteRenderOpToRender.Start();
			}
		}
		else if (timeSpan > TimeSpan.Zero)
		{
			if (_currentRenderOp == null)
			{
				_currentRenderOp = base.Dispatcher.BeginInvoke(DispatcherPriority.Inactive, _animRenderMessage, null);
				_promoteRenderOpToInput.Interval = timeSpan;
				_promoteRenderOpToInput.Start();
				_promoteRenderOpToRender.Interval = TimeSpan.FromSeconds(1.0);
				_promoteRenderOpToRender.Start();
			}
		}
		else if (timeSpan == TimeSpan.Zero)
		{
			DispatcherPriority priority = DispatcherPriority.Render;
			if (_inputMarkerOp == null)
			{
				_inputMarkerOp = base.Dispatcher.BeginInvoke(DispatcherPriority.Input, _inputMarkerMessage, null);
				_lastInputMarkerTime = CurrentTicks;
			}
			else if (CurrentTicks - _lastInputMarkerTime > 5000000)
			{
				priority = DispatcherPriority.Input;
			}
			if (_currentRenderOp == null)
			{
				_currentRenderOp = base.Dispatcher.BeginInvoke(priority, _animRenderMessage, null);
			}
			else
			{
				_currentRenderOp.Priority = priority;
			}
			_promoteRenderOpToInput.Stop();
			_promoteRenderOpToRender.Stop();
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientScheduleRender, timeSpan.TotalMilliseconds);
	}

	private void CommitChannelAfterNextVSync()
	{
		if (_animationRenderRate != 0)
		{
			long currentTicks = CurrentTicks;
			long num = currentTicks + TicksUntilNextVsync(currentTicks) + 10000;
			_estimatedNextVSyncTimer.Interval = TimeSpan.FromTicks(num - currentTicks);
			_estimatedNextVSyncTimer.Tag = num;
		}
		else
		{
			_estimatedNextVSyncTimer.Interval = TimeSpan.FromMilliseconds(17.0);
		}
		_estimatedNextVSyncTimer.Start();
		_interlockState = InterlockState.WaitingForNextFrame;
		_lastPresentationResults = MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_NOPRESENT;
	}

	private void NotifyPresented(MIL_PRESENTATION_RESULTS presentationResults, long presentationTime, int displayRefreshRate)
	{
		if (!InterlockIsEnabled)
		{
			return;
		}
		TimeSpan minimumDelay = TimeSpan.Zero;
		_lastPresentationResults = presentationResults;
		_interlockState = InterlockState.Idle;
		switch (presentationResults)
		{
		case MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_VSYNC:
			if (displayRefreshRate != _displayRefreshRate)
			{
				_displayRefreshRate = displayRefreshRate;
				_adjustedRefreshRate = FindNextPrime(displayRefreshRate + 5);
			}
			_animationRenderRate = Math.Max(_adjustedRefreshRate, _timeManager.GetMaxDesiredFrameRate());
			_lastPresentationTime = presentationTime;
			break;
		case MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_VSYNC_UNSUPPORTED:
			minimumDelay = _timeDelay;
			break;
		case MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_NOPRESENT:
			CommitChannelAfterNextVSync();
			break;
		case MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_DWM:
			_animationRenderRate = displayRefreshRate;
			_lastPresentationTime = presentationTime;
			break;
		}
		_animationRenderRate = Math.Min(_animationRenderRate, 1000);
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientUceNotifyPresent, _lastPresentationTime, (long)presentationResults);
		if (presentationResults == MIL_PRESENTATION_RESULTS.MIL_PRESENTATION_NOPRESENT)
		{
			return;
		}
		_estimatedNextVSyncTimer.Stop();
		if (!InterlockIsWaiting && _needToCommitChannel)
		{
			if (HasCommittedThisVBlankInterval)
			{
				CommitChannelAfterNextVSync();
				return;
			}
			CommitChannel();
		}
		ScheduleNextRenderOp(minimumDelay);
	}

	private long TicksSinceLastPresent(long currentTime)
	{
		return currentTime - CountsToTicks(_lastPresentationTime);
	}

	private long TicksSinceLastVsync(long currentTime)
	{
		return TicksSinceLastPresent(currentTime) % RefreshPeriod;
	}

	private long TicksUntilNextVsync(long currentTime)
	{
		return RefreshPeriod - TicksSinceLastVsync(currentTime);
	}

	private void NotifySyncModeStatus(int enabledResult)
	{
		if (_interlockState != InterlockState.RequestedStart)
		{
			return;
		}
		if (enabledResult >= 0)
		{
			_interlockState = InterlockState.Idle;
			if (Channel != null)
			{
				this.CommittingBatch?.Invoke(Channel, new EventArgs());
				Channel.SyncFlush();
			}
		}
		else
		{
			_interlockState = InterlockState.Disabled;
		}
	}

	internal void CreateChannels()
	{
		_channelManager.CreateChannels();
		DUCE.NotifyPolicyChangeForNonInteractiveMode(ShouldRenderEvenWhenNoDisplayDevicesAreAvailable, Channel);
		HookNotifications();
		_uceEtwEvent.CreateOrAddRefOnChannel(this, Channel, DUCE.ResourceType.TYPE_ETWEVENTRESOURCE);
		RequestTier(Channel);
		Channel.CloseBatch();
		Channel.Commit();
		CompleteRender();
		NotifyChannelMessage();
	}

	private void RemoveChannels()
	{
		if (Channel != null)
		{
			_uceEtwEvent.ReleaseOnChannel(Channel);
			LeaveInterlockedPresentation();
		}
		_channelManager.RemoveChannels();
	}

	private unsafe void EnterInterlockedPresentation()
	{
		if (!InterlockIsEnabled && MediaSystem.AnimationSmoothing && Channel.MarshalType == ChannelMarshalType.ChannelMarshalTypeCrossThread && IsClockSupported)
		{
			DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE mILCMD_PARTITION_SETVBLANKSYNCMODE = default(DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE);
			mILCMD_PARTITION_SETVBLANKSYNCMODE.Type = MILCMD.MilCmdPartitionSetVBlankSyncMode;
			mILCMD_PARTITION_SETVBLANKSYNCMODE.Enable = 1u;
			Channel.SendCommand((byte*)(&mILCMD_PARTITION_SETVBLANKSYNCMODE), sizeof(DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE), sendInSeparateBatch: true);
			_interlockState = InterlockState.RequestedStart;
		}
	}

	private unsafe void LeaveInterlockedPresentation()
	{
		bool num = _interlockState == InterlockState.Disabled;
		if (_interlockState == InterlockState.WaitingForResponse)
		{
			CompleteRender();
		}
		_estimatedNextVSyncTimer.Stop();
		if (!num)
		{
			_interlockState = InterlockState.Disabled;
			DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE mILCMD_PARTITION_SETVBLANKSYNCMODE = default(DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE);
			mILCMD_PARTITION_SETVBLANKSYNCMODE.Type = MILCMD.MilCmdPartitionSetVBlankSyncMode;
			mILCMD_PARTITION_SETVBLANKSYNCMODE.Enable = 0u;
			Channel.SendCommand((byte*)(&mILCMD_PARTITION_SETVBLANKSYNCMODE), sizeof(DUCE.MILCMD_PARTITION_SETVBLANKSYNCMODE), sendInSeparateBatch: true);
			_needToCommitChannel = true;
			CommitChannel();
		}
	}

	private void HookNotifications()
	{
		_notificationWindow.SetAsChannelNotificationWindow();
		RegisterForNotifications(Channel);
	}

	internal static MediaContext From(Dispatcher dispatcher)
	{
		MediaContext mediaContext = (MediaContext)dispatcher.Reserved0;
		if (mediaContext == null)
		{
			mediaContext = new MediaContext(dispatcher);
		}
		return mediaContext;
	}

	private void OnDestroyContext(object sender, EventArgs e)
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		if (!_isDisposed)
		{
			ICompositionTarget[] array = new ICompositionTarget[_registeredICompositionTargets.Count];
			_registeredICompositionTargets.CopyTo(array, 0);
			ICompositionTarget[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Dispose();
			}
			_registeredICompositionTargets = null;
			_notificationWindow.Dispose();
			base.Dispatcher.ShutdownFinished -= _destroyHandler;
			_destroyHandler = null;
			_timeManager.NeedTickSooner -= OnNeedTickSooner;
			_timeManager.Stop();
			_isDisposed = true;
			RemoveChannels();
			MediaSystem.Shutdown(this);
			_timeManager = null;
			GC.SuppressFinalize(this);
		}
	}

	internal static void RegisterICompositionTarget(Dispatcher dispatcher, ICompositionTarget iv)
	{
		From(dispatcher).RegisterICompositionTargetInternal(iv);
	}

	private void RegisterICompositionTargetInternal(ICompositionTarget iv)
	{
		if (Channel != null)
		{
			DUCE.ChannelSet channelSet = ((!_currentRenderingChannel.HasValue) ? GetChannels() : _currentRenderingChannel.Value);
			iv.AddRefOnChannel(channelSet.Channel, channelSet.OutOfBandChannel);
		}
		_registeredICompositionTargets.Add(iv);
	}

	internal static void UnregisterICompositionTarget(Dispatcher dispatcher, ICompositionTarget iv)
	{
		From(dispatcher).UnregisterICompositionTargetInternal(iv);
	}

	private void UnregisterICompositionTargetInternal(ICompositionTarget iv)
	{
		if (!_isDisposed)
		{
			if (Channel != null)
			{
				DUCE.ChannelSet channelSet = ((!_currentRenderingChannel.HasValue) ? GetChannels() : _currentRenderingChannel.Value);
				iv.ReleaseOnChannel(channelSet.Channel, channelSet.OutOfBandChannel);
			}
			_registeredICompositionTargets.Remove(iv);
		}
	}

	internal void BeginInvokeOnRender(DispatcherOperationCallback callback, object arg)
	{
		if (_invokeOnRenderCallbacks == null)
		{
			_invokeOnRenderCallbacks = new FrugalObjectList<InvokeOnRenderCallback>();
		}
		_invokeOnRenderCallbacks.Add(new InvokeOnRenderCallback(callback, arg));
		if (!_isRendering)
		{
			PostRender();
		}
	}

	[FriendAccessAllowed]
	internal LoadedOrUnloadedOperation AddLoadedOrUnloadedCallback(DispatcherOperationCallback callback, DependencyObject target)
	{
		LoadedOrUnloadedOperation loadedOrUnloadedOperation = new LoadedOrUnloadedOperation(callback, target);
		if (_loadedOrUnloadedPendingOperations == null)
		{
			_loadedOrUnloadedPendingOperations = new FrugalObjectList<LoadedOrUnloadedOperation>(1);
		}
		_loadedOrUnloadedPendingOperations.Add(loadedOrUnloadedOperation);
		return loadedOrUnloadedOperation;
	}

	[FriendAccessAllowed]
	internal void RemoveLoadedOrUnloadedCallback(LoadedOrUnloadedOperation op)
	{
		op.Cancel();
		if (_loadedOrUnloadedPendingOperations == null)
		{
			return;
		}
		for (int i = 0; i < _loadedOrUnloadedPendingOperations.Count; i++)
		{
			if (_loadedOrUnloadedPendingOperations[i] == op)
			{
				_loadedOrUnloadedPendingOperations.RemoveAt(i);
				break;
			}
		}
	}

	internal void PostRender()
	{
		if (!_isDisposed && !_isRendering)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientPostRender);
			if (_currentRenderOp != null)
			{
				_currentRenderOp.Priority = DispatcherPriority.Render;
			}
			else
			{
				_currentRenderOp = base.Dispatcher.BeginInvoke(DispatcherPriority.Render, _renderMessage, null);
			}
			_promoteRenderOpToInput.Stop();
			_promoteRenderOpToRender.Stop();
		}
	}

	internal void Resize(ICompositionTarget resizedCompositionTarget)
	{
		if (_currentRenderOp != null)
		{
			_currentRenderOp.Abort();
			_currentRenderOp = null;
		}
		_promoteRenderOpToInput.Stop();
		_promoteRenderOpToRender.Stop();
		RenderMessageHandler(resizedCompositionTarget);
	}

	private object RenderMessageHandler(object resizedCompositionTarget)
	{
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientRenderHandlerBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, PerfService.GetPerfElementID(this));
		}
		RenderMessageHandlerCore(resizedCompositionTarget);
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientRenderHandlerEnd);
		return null;
	}

	private object AnimatedRenderMessageHandler(object resizedCompositionTarget)
	{
		if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info))
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientAnimRenderHandlerBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, PerfService.GetPerfElementID(this));
		}
		RenderMessageHandlerCore(resizedCompositionTarget);
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientAnimRenderHandlerEnd);
		return null;
	}

	private object InputMarkerMessageHandler(object arg)
	{
		_inputMarkerOp = null;
		return null;
	}

	private void RenderMessageHandlerCore(object resizedCompositionTarget)
	{
		if (Channel == null)
		{
			return;
		}
		_isRendering = true;
		_promoteRenderOpToInput.Stop();
		_promoteRenderOpToRender.Stop();
		bool flag = true;
		try
		{
			int num = 0;
			do
			{
				num++;
				if (num > 153)
				{
					throw new InvalidOperationException(SR.MediaContext_InfiniteTickLoop);
				}
				_timeManager.Tick();
				_timeManager.LockTickTime();
				FireInvokeOnRenderCallbacks();
				if (this.Rendering != null && num == 1)
				{
					this.Rendering?.Invoke(base.Dispatcher, new RenderingEventArgs(_timeManager.LastTickTime));
					FireInvokeOnRenderCallbacks();
				}
			}
			while (_timeManager.IsDirty);
			_timeManager.UnlockTickTime();
			MediaSystem.PropagateDirtyRectangleSettings();
			InputManager.UnsecureCurrent.InvalidateInputDevices();
			bool flag2 = !InterlockIsWaiting;
			Render((ICompositionTarget)resizedCompositionTarget);
			if (_currentRenderOp != null)
			{
				_currentRenderOp.Abort();
				_currentRenderOp = null;
			}
			if (!InterlockIsEnabled)
			{
				ScheduleNextRenderOp(_timeDelay);
			}
			else if (flag2)
			{
				ScheduleNextRenderOp(TimeSpan.Zero);
			}
			flag = false;
		}
		finally
		{
			if (flag && _currentRenderOp != null)
			{
				_currentRenderOp.Abort();
				_currentRenderOp = null;
			}
			_isRendering = false;
		}
	}

	private void FireInvokeOnRenderCallbacks()
	{
		int num = 0;
		int invokeOnRenderCallbacksCount = InvokeOnRenderCallbacksCount;
		while (true)
		{
			if (invokeOnRenderCallbacksCount > 0)
			{
				num++;
				if (num > 153)
				{
					throw new InvalidOperationException(SR.MediaContext_InfiniteLayoutLoop);
				}
				FrugalObjectList<InvokeOnRenderCallback> invokeOnRenderCallbacks = _invokeOnRenderCallbacks;
				_invokeOnRenderCallbacks = null;
				for (int i = 0; i < invokeOnRenderCallbacksCount; i++)
				{
					invokeOnRenderCallbacks[i].DoWork();
				}
				invokeOnRenderCallbacksCount = InvokeOnRenderCallbacksCount;
			}
			else
			{
				FireLoadedPendingCallbacks();
				invokeOnRenderCallbacksCount = InvokeOnRenderCallbacksCount;
				if (invokeOnRenderCallbacksCount <= 0)
				{
					break;
				}
			}
		}
	}

	private void FireLoadedPendingCallbacks()
	{
		if (_loadedOrUnloadedPendingOperations == null)
		{
			return;
		}
		int count = _loadedOrUnloadedPendingOperations.Count;
		if (count != 0)
		{
			FrugalObjectList<LoadedOrUnloadedOperation> loadedOrUnloadedPendingOperations = _loadedOrUnloadedPendingOperations;
			_loadedOrUnloadedPendingOperations = null;
			for (int i = 0; i < count; i++)
			{
				loadedOrUnloadedPendingOperations[i].DoWork();
			}
		}
	}

	private void Render(ICompositionTarget resizedCompositionTarget)
	{
		using (base.Dispatcher.DisableProcessing())
		{
			bool flag = false;
			uint num = (uint)Interlocked.Increment(ref _contextRenderID);
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info))
			{
				flag = true;
				DUCE.ETWEvent.RaiseEvent(_uceEtwEvent.Handle, num, Channel);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMediaRenderBegin, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, num, TicksToCounts(_estimatedNextPresentationTime.Ticks));
			}
			DUCE.ChannelSet value = default(DUCE.ChannelSet);
			foreach (ICompositionTarget registeredICompositionTarget in _registeredICompositionTargets)
			{
				value.Channel = _channelManager.Channel;
				value.OutOfBandChannel = _channelManager.OutOfBandChannel;
				_currentRenderingChannel = value;
				registeredICompositionTarget.Render(registeredICompositionTarget == resizedCompositionTarget, value.Channel);
				_currentRenderingChannel = null;
			}
			RaiseResourcesUpdated();
			if (Channel != null)
			{
				Channel.CloseBatch();
			}
			_needToCommitChannel = true;
			_commitPendingAfterRender = true;
			if (!InterlockIsWaiting)
			{
				if (HasCommittedThisVBlankInterval)
				{
					CommitChannelAfterNextVSync();
				}
				else
				{
					CommitChannel();
				}
			}
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientMediaRenderEnd, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info);
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientUIResponse, EventTrace.Keyword.KeywordGraphics, EventTrace.Level.Info, GetHashCode(), num);
			}
		}
	}

	private void CommitChannel()
	{
		if (Channel != null)
		{
			if (InterlockIsEnabled)
			{
				long currentTicks = CurrentTicks;
				long num = _estimatedNextPresentationTime.Ticks;
				if (_animationRenderRate > 0)
				{
					long num2 = currentTicks + TicksUntilNextVsync(currentTicks);
					if (num2 > num)
					{
						num = num2;
					}
				}
				RequestPresentedNotification(Channel, TicksToCounts(num));
				_interlockState = InterlockState.WaitingForResponse;
				_lastCommitTime = currentTicks;
			}
			this.CommittingBatch?.Invoke(Channel, new EventArgs());
			Channel.Commit();
			if (_commitPendingAfterRender)
			{
				if (this._renderCompleteHandlers != null)
				{
					this._renderCompleteHandlers(this, null);
				}
				_commitPendingAfterRender = false;
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordGraphics, EventTrace.Event.WClientUICommitChannel, _contextRenderID);
		}
		_needToCommitChannel = false;
	}

	private unsafe void RequestPresentedNotification(DUCE.Channel channel, long estimatedFrameTime)
	{
		DUCE.MILCMD_PARTITION_NOTIFYPRESENT mILCMD_PARTITION_NOTIFYPRESENT = default(DUCE.MILCMD_PARTITION_NOTIFYPRESENT);
		mILCMD_PARTITION_NOTIFYPRESENT.Type = MILCMD.MilCmdPartitionNotifyPresent;
		mILCMD_PARTITION_NOTIFYPRESENT.FrameTime = (ulong)estimatedFrameTime;
		channel.SendCommand((byte*)(&mILCMD_PARTITION_NOTIFYPRESENT), sizeof(DUCE.MILCMD_PARTITION_NOTIFYPRESENT), sendInSeparateBatch: true);
	}

	internal void CompleteRender()
	{
		if (Channel == null)
		{
			return;
		}
		if (InterlockIsEnabled)
		{
			if (_interlockState == InterlockState.WaitingForResponse)
			{
				do
				{
					this.CommittingBatch?.Invoke(Channel, new EventArgs());
					Channel.WaitForNextMessage();
					NotifyChannelMessage();
				}
				while (_interlockState == InterlockState.WaitingForResponse);
			}
			_estimatedNextVSyncTimer.Stop();
			_interlockState = InterlockState.Idle;
			if (!_needToCommitChannel)
			{
				return;
			}
			CommitChannel();
			if (_interlockState == InterlockState.WaitingForResponse)
			{
				do
				{
					Channel.WaitForNextMessage();
					NotifyChannelMessage();
				}
				while (_interlockState == InterlockState.WaitingForResponse);
				_estimatedNextVSyncTimer.Stop();
				_interlockState = InterlockState.Idle;
			}
		}
		else
		{
			this.CommittingBatch?.Invoke(Channel, new EventArgs());
			Channel.SyncFlush();
		}
	}

	private void OnNeedTickSooner(object sender, EventArgs e)
	{
		PostRender();
	}

	internal void VerifyWriteAccess()
	{
		if (!WriteAccessEnabled)
		{
			throw new InvalidOperationException(SR.MediaContext_APINotAllowed);
		}
	}

	internal void PushReadOnlyAccess()
	{
		_readOnlyAccessCounter++;
	}

	internal void PopReadOnlyAccess()
	{
		_readOnlyAccessCounter--;
	}

	private void RaiseResourcesUpdated()
	{
		if (this._resourcesUpdatedHandlers != null)
		{
			DUCE.ChannelSet channels = GetChannels();
			this._resourcesUpdatedHandlers(channels.Channel, skipOnChannelCheck: false);
			this._resourcesUpdatedHandlers = null;
		}
	}

	internal DUCE.Channel AllocateSyncChannel()
	{
		return _channelManager.AllocateSyncChannel();
	}

	internal void ReleaseSyncChannel(DUCE.Channel channel)
	{
		_channelManager.ReleaseSyncChannel(channel);
	}

	internal BoundsDrawingContextWalker AcquireBoundsDrawingContextWalker()
	{
		if (_cachedBoundsDrawingContextWalker == null)
		{
			return new BoundsDrawingContextWalker();
		}
		BoundsDrawingContextWalker cachedBoundsDrawingContextWalker = _cachedBoundsDrawingContextWalker;
		_cachedBoundsDrawingContextWalker = null;
		cachedBoundsDrawingContextWalker.ClearState();
		return cachedBoundsDrawingContextWalker;
	}

	internal void ReleaseBoundsDrawingContextWalker(BoundsDrawingContextWalker ctx)
	{
		_cachedBoundsDrawingContextWalker = ctx;
	}

	private void PromoteRenderOpToInput(object sender, EventArgs e)
	{
		if (_currentRenderOp != null)
		{
			_currentRenderOp.Priority = DispatcherPriority.Input;
		}
		((DispatcherTimer)sender).Stop();
	}

	private void PromoteRenderOpToRender(object sender, EventArgs e)
	{
		if (_currentRenderOp != null)
		{
			_currentRenderOp.Priority = DispatcherPriority.Render;
		}
		((DispatcherTimer)sender).Stop();
	}

	private void EstimatedNextVSyncTimeExpired(object sender, EventArgs e)
	{
		long currentTicks = CurrentTicks;
		DispatcherTimer dispatcherTimer = (DispatcherTimer)sender;
		long num = 0L;
		if (dispatcherTimer.Tag != null)
		{
			num = (long)dispatcherTimer.Tag;
		}
		if (num > currentTicks)
		{
			dispatcherTimer.Stop();
			dispatcherTimer.Interval = TimeSpan.FromTicks(num - currentTicks);
			dispatcherTimer.Start();
			return;
		}
		_interlockState = InterlockState.Idle;
		if (_needToCommitChannel)
		{
			CommitChannel();
			ScheduleNextRenderOp(TimeSpan.Zero);
		}
		dispatcherTimer.Stop();
	}

	private unsafe void RegisterForNotifications(DUCE.Channel channel)
	{
		DUCE.MILCMD_PARTITION_REGISTERFORNOTIFICATIONS mILCMD_PARTITION_REGISTERFORNOTIFICATIONS = default(DUCE.MILCMD_PARTITION_REGISTERFORNOTIFICATIONS);
		mILCMD_PARTITION_REGISTERFORNOTIFICATIONS.Type = MILCMD.MilCmdPartitionRegisterForNotifications;
		mILCMD_PARTITION_REGISTERFORNOTIFICATIONS.Enable = 1u;
		channel.SendCommand((byte*)(&mILCMD_PARTITION_REGISTERFORNOTIFICATIONS), sizeof(DUCE.MILCMD_PARTITION_REGISTERFORNOTIFICATIONS));
	}

	internal DUCE.ChannelSet GetChannels()
	{
		DUCE.ChannelSet result = default(DUCE.ChannelSet);
		result.Channel = _channelManager.Channel;
		result.OutOfBandChannel = _channelManager.OutOfBandChannel;
		return result;
	}
}
