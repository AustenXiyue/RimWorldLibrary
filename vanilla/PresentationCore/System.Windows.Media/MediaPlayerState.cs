using System.IO.Packaging;
using System.Windows.Media.Composition;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

internal class MediaPlayerState
{
	private class Helper
	{
		private WeakReference _nativeMedia;

		internal Helper(SafeMediaHandle nativeMedia)
		{
			_nativeMedia = new WeakReference(nativeMedia);
		}

		internal void ProcessExitHandler(object sender, EventArgs args)
		{
			SafeMediaHandle safeMediaHandle = (SafeMediaHandle)_nativeMedia.Target;
			if (safeMediaHandle != null)
			{
				MILMedia.ProcessExitHandler(safeMediaHandle);
			}
		}
	}

	private double _volume;

	private double _balance;

	private bool _muted;

	private bool _scrubbingEnabled;

	private SafeMediaHandle _nativeMedia;

	private MediaEventsHelper _mediaEventsHelper;

	private const double DEFAULT_VOLUME = 0.5;

	private const double DEFAULT_BALANCE = 0.0;

	private double _speedRatio;

	private bool _paused;

	private Uri _sourceUri;

	private MediaClock _mediaClock;

	private Dispatcher _dispatcher;

	private UniqueEventHelper _newFrameHelper = new UniqueEventHelper();

	private UniqueEventHelper _mediaOpenedHelper = new UniqueEventHelper();

	private const float _defaultDevicePixelsPerInch = 96f;

	private Helper _helper;

	internal bool IsBuffering
	{
		get
		{
			VerifyAPI();
			bool pIsBuffering = false;
			HRESULT.Check(MILMedia.IsBuffering(_nativeMedia, ref pIsBuffering));
			return pIsBuffering;
		}
	}

	internal bool CanPause
	{
		get
		{
			VerifyAPI();
			bool pCanPause = false;
			HRESULT.Check(MILMedia.CanPause(_nativeMedia, ref pCanPause));
			return pCanPause;
		}
	}

	internal double DownloadProgress
	{
		get
		{
			VerifyAPI();
			double pProgress = 0.0;
			HRESULT.Check(MILMedia.GetDownloadProgress(_nativeMedia, ref pProgress));
			return pProgress;
		}
	}

	internal double BufferingProgress
	{
		get
		{
			VerifyAPI();
			double pProgress = 0.0;
			HRESULT.Check(MILMedia.GetBufferingProgress(_nativeMedia, ref pProgress));
			return pProgress;
		}
	}

	internal int NaturalVideoHeight
	{
		get
		{
			VerifyAPI();
			uint puiHeight = 0u;
			HRESULT.Check(MILMedia.GetNaturalHeight(_nativeMedia, ref puiHeight));
			return (int)puiHeight;
		}
	}

	internal int NaturalVideoWidth
	{
		get
		{
			VerifyAPI();
			uint puiWidth = 0u;
			HRESULT.Check(MILMedia.GetNaturalWidth(_nativeMedia, ref puiWidth));
			return (int)puiWidth;
		}
	}

	internal bool HasAudio
	{
		get
		{
			VerifyAPI();
			bool pfHasAudio = true;
			HRESULT.Check(MILMedia.HasAudio(_nativeMedia, ref pfHasAudio));
			return pfHasAudio;
		}
	}

	internal bool HasVideo
	{
		get
		{
			VerifyAPI();
			bool pfHasVideo = false;
			HRESULT.Check(MILMedia.HasVideo(_nativeMedia, ref pfHasVideo));
			return pfHasVideo;
		}
	}

	internal Uri Source
	{
		get
		{
			VerifyAPI();
			return _sourceUri;
		}
	}

	internal double Volume
	{
		get
		{
			VerifyAPI();
			return _volume;
		}
		set
		{
			VerifyAPI();
			if (double.IsNaN(value))
			{
				throw new ArgumentException(SR.ParameterValueCannotBeNaN, "value");
			}
			if (DoubleUtil.GreaterThanOrClose(value, 1.0))
			{
				value = 1.0;
			}
			else if (DoubleUtil.LessThanOrClose(value, 0.0))
			{
				value = 0.0;
			}
			if (!DoubleUtil.AreClose(_volume, value))
			{
				if (!_muted)
				{
					HRESULT.Check(MILMedia.SetVolume(_nativeMedia, value));
					_volume = value;
				}
				else
				{
					_volume = value;
				}
			}
		}
	}

	internal double Balance
	{
		get
		{
			VerifyAPI();
			return _balance;
		}
		set
		{
			VerifyAPI();
			if (double.IsNaN(value))
			{
				throw new ArgumentException(SR.ParameterValueCannotBeNaN, "value");
			}
			if (DoubleUtil.GreaterThanOrClose(value, 1.0))
			{
				value = 1.0;
			}
			else if (DoubleUtil.LessThanOrClose(value, -1.0))
			{
				value = -1.0;
			}
			if (!DoubleUtil.AreClose(_balance, value))
			{
				HRESULT.Check(MILMedia.SetBalance(_nativeMedia, value));
				_balance = value;
			}
		}
	}

	internal bool ScrubbingEnabled
	{
		get
		{
			VerifyAPI();
			return _scrubbingEnabled;
		}
		set
		{
			VerifyAPI();
			if (value != _scrubbingEnabled)
			{
				HRESULT.Check(MILMedia.SetIsScrubbingEnabled(_nativeMedia, value));
				_scrubbingEnabled = value;
			}
		}
	}

	internal bool IsMuted
	{
		get
		{
			VerifyAPI();
			return _muted;
		}
		set
		{
			VerifyAPI();
			double volume = _volume;
			if (value && !_muted)
			{
				Volume = 0.0;
				_muted = true;
				_volume = volume;
			}
			else if (!value && _muted)
			{
				_muted = false;
				_volume = 0.0;
				Volume = volume;
			}
		}
	}

	internal Duration NaturalDuration
	{
		get
		{
			VerifyAPI();
			long pllLength = 0L;
			HRESULT.Check(MILMedia.GetMediaLength(_nativeMedia, ref pllLength));
			if (pllLength == 0L)
			{
				return Duration.Automatic;
			}
			return new Duration(TimeSpan.FromTicks(pllLength));
		}
	}

	internal TimeSpan Position
	{
		get
		{
			VerifyAPI();
			return GetPosition();
		}
		set
		{
			VerifyAPI();
			VerifyNotControlledByClock();
			SetPosition(value);
		}
	}

	internal double SpeedRatio
	{
		get
		{
			VerifyAPI();
			return _speedRatio;
		}
		set
		{
			VerifyAPI();
			VerifyNotControlledByClock();
			if (value < 0.0)
			{
				value = 0.0;
			}
			SetSpeedRatio(value);
		}
	}

	internal Dispatcher Dispatcher => _dispatcher;

	internal MediaClock Clock
	{
		get
		{
			VerifyAPI();
			return _mediaClock;
		}
	}

	private double PrivateSpeedRatio
	{
		set
		{
			VerifyAPI();
			if (double.IsNaN(value))
			{
				throw new ArgumentException(SR.ParameterValueCannotBeNaN, "value");
			}
			HRESULT.Check(MILMedia.SetRate(_nativeMedia, value));
		}
	}

	internal event EventHandler<ExceptionEventArgs> MediaFailed
	{
		add
		{
			VerifyAPI();
			_mediaEventsHelper.MediaFailed += value;
		}
		remove
		{
			VerifyAPI();
			_mediaEventsHelper.MediaFailed -= value;
		}
	}

	internal event EventHandler MediaOpened
	{
		add
		{
			VerifyAPI();
			_mediaOpenedHelper.AddEvent(value);
		}
		remove
		{
			VerifyAPI();
			_mediaOpenedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler MediaEnded
	{
		add
		{
			VerifyAPI();
			_mediaEventsHelper.MediaEnded += value;
		}
		remove
		{
			VerifyAPI();
			_mediaEventsHelper.MediaEnded -= value;
		}
	}

	internal event EventHandler BufferingStarted
	{
		add
		{
			VerifyAPI();
			_mediaEventsHelper.BufferingStarted += value;
		}
		remove
		{
			VerifyAPI();
			_mediaEventsHelper.BufferingStarted -= value;
		}
	}

	internal event EventHandler BufferingEnded
	{
		add
		{
			VerifyAPI();
			_mediaEventsHelper.BufferingEnded += value;
		}
		remove
		{
			VerifyAPI();
			_mediaEventsHelper.BufferingEnded -= value;
		}
	}

	internal event EventHandler<MediaScriptCommandEventArgs> ScriptCommand
	{
		add
		{
			VerifyAPI();
			_mediaEventsHelper.ScriptCommand += value;
		}
		remove
		{
			VerifyAPI();
			_mediaEventsHelper.ScriptCommand -= value;
		}
	}

	internal event EventHandler NewFrame
	{
		add
		{
			VerifyAPI();
			_newFrameHelper.AddEvent(value);
		}
		remove
		{
			VerifyAPI();
			_newFrameHelper.RemoveEvent(value);
		}
	}

	internal MediaPlayerState(MediaPlayer mediaPlayer)
	{
		_dispatcher = mediaPlayer.Dispatcher;
		Init();
		CreateMedia(mediaPlayer);
		_mediaEventsHelper.NewFrame += OnNewFrame;
		_mediaEventsHelper.MediaPrerolled += OnMediaOpened;
	}

	private void Init()
	{
		_volume = 0.5;
		_balance = 0.0;
		_speedRatio = 1.0;
		_paused = false;
		_muted = false;
		_sourceUri = null;
		_scrubbingEnabled = false;
	}

	~MediaPlayerState()
	{
		if (_helper != null)
		{
			AppDomain.CurrentDomain.ProcessExit -= _helper.ProcessExitHandler;
		}
	}

	internal void SetClock(MediaClock clock, MediaPlayer player)
	{
		VerifyAPI();
		MediaClock mediaClock = _mediaClock;
		if (mediaClock != clock)
		{
			_mediaClock = clock;
			if (mediaClock != null)
			{
				mediaClock.Player = null;
			}
			if (clock != null)
			{
				clock.Player = player;
			}
			if (clock == null)
			{
				Open(null);
			}
		}
	}

	internal void Open(Uri source)
	{
		VerifyAPI();
		VerifyNotControlledByClock();
		SetSource(source);
		SetPosition(TimeSpan.Zero);
	}

	internal void Play()
	{
		VerifyAPI();
		VerifyNotControlledByClock();
		_paused = false;
		PrivateSpeedRatio = SpeedRatio;
	}

	internal void Pause()
	{
		VerifyAPI();
		VerifyNotControlledByClock();
		_paused = true;
		PrivateSpeedRatio = 0.0;
	}

	internal void Stop()
	{
		VerifyAPI();
		VerifyNotControlledByClock();
		Pause();
		Position = TimeSpan.FromTicks(0L);
	}

	internal void Close()
	{
		VerifyAPI();
		VerifyNotControlledByClock();
		HRESULT.Check(MILMedia.Close(_nativeMedia));
		SetClock(null, null);
		Init();
	}

	internal void SendCommandMedia(DUCE.Channel channel, DUCE.ResourceHandle handle, bool notifyUceDirectly)
	{
		SendMediaPlayerCommand(channel, handle, notifyUceDirectly);
		if (!notifyUceDirectly)
		{
			NeedUIFrameUpdate();
		}
	}

	private void NeedUIFrameUpdate()
	{
		VerifyAPI();
		HRESULT.Check(MILMedia.NeedUIFrameUpdate(_nativeMedia));
	}

	private void CreateMedia(MediaPlayer mediaPlayer)
	{
		SafeMILHandle unmanagedProxy = null;
		MediaEventsHelper.CreateMediaEventsHelper(mediaPlayer, out _mediaEventsHelper, out unmanagedProxy);
		try
		{
			using FactoryMaker factoryMaker = new FactoryMaker();
			HRESULT.Check(UnsafeNativeMethods.MILFactory2.CreateMediaPlayer(factoryMaker.FactoryPtr, unmanagedProxy, canOpenAllMedia: true, out _nativeMedia));
		}
		catch
		{
			if (_nativeMedia != null && !_nativeMedia.IsInvalid)
			{
				_nativeMedia.Close();
			}
			throw;
		}
		_helper = new Helper(_nativeMedia);
		AppDomain.CurrentDomain.ProcessExit += _helper.ProcessExitHandler;
	}

	private void OpenMedia(Uri source)
	{
		string text = null;
		if (source != null && source.IsAbsoluteUri && source.Scheme == PackUriHelper.UriSchemePack)
		{
			try
			{
				source = BaseUriHelper.ConvertPackUriToAbsoluteExternallyVisibleUri(source);
			}
			catch (InvalidOperationException)
			{
				source = null;
				_mediaEventsHelper.RaiseMediaFailed(new NotSupportedException(SR.Format(SR.Media_PackURIsAreNotSupported, null)));
			}
		}
		if (source != null)
		{
			Uri baseDirectory = SecurityHelper.GetBaseDirectory(AppDomain.CurrentDomain);
			Uri absoluteUri = ResolveUri(source, baseDirectory);
			text = DemandPermissions(absoluteUri);
		}
		else
		{
			text = null;
		}
		HRESULT.Check(MILMedia.Open(_nativeMedia, text));
	}

	private Uri ResolveUri(Uri uri, Uri appBase)
	{
		if (uri.IsAbsoluteUri)
		{
			return uri;
		}
		return new Uri(appBase, uri);
	}

	private string DemandPermissions(Uri absoluteUri)
	{
		string result = BindUriHelper.UriToString(absoluteUri);
		if (SecurityHelper.MapUrlToZoneWrapper(absoluteUri) == 0 && absoluteUri.IsFile)
		{
			result = absoluteUri.LocalPath;
		}
		return result;
	}

	internal void SetPosition(TimeSpan value)
	{
		VerifyAPI();
		HRESULT.Check(MILMedia.SetPosition(_nativeMedia, value.Ticks));
	}

	private TimeSpan GetPosition()
	{
		VerifyAPI();
		long pllTime = 0L;
		HRESULT.Check(MILMedia.GetPosition(_nativeMedia, ref pllTime));
		return TimeSpan.FromTicks(pllTime);
	}

	internal void SetSpeedRatio(double value)
	{
		_speedRatio = value;
		if (!_paused || _mediaClock != null)
		{
			PrivateSpeedRatio = _speedRatio;
		}
	}

	internal void SetSource(Uri source)
	{
		if (source != _sourceUri)
		{
			OpenMedia(source);
			_sourceUri = source;
		}
	}

	private void VerifyAPI()
	{
		_dispatcher.VerifyAccess();
		if (_nativeMedia == null || _nativeMedia.IsInvalid)
		{
			throw new NotSupportedException(SR.Image_BadVersion);
		}
	}

	private void VerifyNotControlledByClock()
	{
		if (Clock != null)
		{
			throw new InvalidOperationException(SR.Media_NotAllowedWhileTimingEngineInControl);
		}
	}

	private void SendMediaPlayerCommand(DUCE.Channel channel, DUCE.ResourceHandle handle, bool notifyUceDirectly)
	{
		UnsafeNativeMethods.MILUnknown.AddRef(_nativeMedia);
		channel.SendCommandMedia(handle, _nativeMedia, notifyUceDirectly);
	}

	private void OnNewFrame(object sender, EventArgs args)
	{
		_newFrameHelper.InvokeEvents(sender, args);
	}

	private void OnMediaOpened(object sender, EventArgs args)
	{
		_mediaOpenedHelper.InvokeEvents(sender, args);
	}
}
