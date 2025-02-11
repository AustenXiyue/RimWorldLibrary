using System.IO.Packaging;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls;

internal class AVElementHelper
{
	private MediaPlayer _mediaPlayer;

	private MediaElement _element;

	private Uri _baseUri;

	private MediaState _unloadedBehavior = MediaState.Close;

	private MediaState _loadedBehavior = MediaState.Play;

	private MediaState _currentState = MediaState.Close;

	private bool _isLoaded;

	private SettableState<TimeSpan> _position;

	private SettableState<MediaState> _mediaState;

	private SettableState<Uri> _source;

	private SettableState<MediaClock> _clock;

	private SettableState<double> _speedRatio;

	private SettableState<double> _volume;

	private SettableState<bool> _isMuted;

	private SettableState<double> _balance;

	private SettableState<bool> _isScrubbingEnabled;

	internal MediaPlayer Player => _mediaPlayer;

	internal Uri BaseUri
	{
		get
		{
			return _baseUri;
		}
		set
		{
			if (value.Scheme != PackUriHelper.UriSchemePack)
			{
				_baseUri = value;
			}
			else
			{
				_baseUri = null;
			}
		}
	}

	internal TimeSpan Position
	{
		get
		{
			if (_currentState == MediaState.Close)
			{
				return _position._value;
			}
			return _mediaPlayer.Position;
		}
	}

	internal MediaClock Clock => _clock._value;

	internal double SpeedRatio => _speedRatio._value;

	internal AVElementHelper(MediaElement element)
	{
		_element = element;
		_position = new SettableState<TimeSpan>(new TimeSpan(0L));
		_mediaState = new SettableState<MediaState>(MediaState.Close);
		_source = new SettableState<Uri>(null);
		_clock = new SettableState<MediaClock>(null);
		_speedRatio = new SettableState<double>(1.0);
		_volume = new SettableState<double>(0.5);
		_isMuted = new SettableState<bool>(value: false);
		_balance = new SettableState<double>(0.0);
		_isScrubbingEnabled = new SettableState<bool>(value: false);
		_mediaPlayer = new MediaPlayer();
		HookEvents();
	}

	internal static AVElementHelper GetHelper(DependencyObject d)
	{
		if (d is MediaElement mediaElement)
		{
			return mediaElement.Helper;
		}
		throw new ArgumentException(SR.AudioVideo_InvalidDependencyObject);
	}

	internal void SetUnloadedBehavior(MediaState unloadedBehavior)
	{
		_unloadedBehavior = unloadedBehavior;
		HandleStateChange();
	}

	internal void SetLoadedBehavior(MediaState loadedBehavior)
	{
		_loadedBehavior = loadedBehavior;
		HandleStateChange();
	}

	internal void SetPosition(TimeSpan position)
	{
		_position._isSet = true;
		_position._value = position;
		HandleStateChange();
	}

	internal void SetClock(MediaClock clock)
	{
		_clock._value = clock;
		_clock._isSet = true;
		HandleStateChange();
	}

	internal void SetSpeedRatio(double speedRatio)
	{
		_speedRatio._wasSet = (_speedRatio._isSet = true);
		_speedRatio._value = speedRatio;
		HandleStateChange();
	}

	internal void SetState(MediaState mediaState)
	{
		if (_loadedBehavior != 0 && _unloadedBehavior != 0)
		{
			throw new NotSupportedException(SR.AudioVideo_CannotControlMedia);
		}
		_mediaState._value = mediaState;
		_mediaState._isSet = true;
		HandleStateChange();
	}

	internal void SetVolume(double volume)
	{
		_volume._wasSet = (_volume._isSet = true);
		_volume._value = volume;
		HandleStateChange();
	}

	internal void SetBalance(double balance)
	{
		_balance._wasSet = (_balance._isSet = true);
		_balance._value = balance;
		HandleStateChange();
	}

	internal void SetIsMuted(bool isMuted)
	{
		_isMuted._wasSet = (_isMuted._isSet = true);
		_isMuted._value = isMuted;
		HandleStateChange();
	}

	internal void SetScrubbingEnabled(bool isScrubbingEnabled)
	{
		_isScrubbingEnabled._wasSet = (_isScrubbingEnabled._isSet = true);
		_isScrubbingEnabled._value = isScrubbingEnabled;
		HandleStateChange();
	}

	private void HookEvents()
	{
		_mediaPlayer.MediaOpened += OnMediaOpened;
		_mediaPlayer.MediaFailed += OnMediaFailed;
		_mediaPlayer.BufferingStarted += OnBufferingStarted;
		_mediaPlayer.BufferingEnded += OnBufferingEnded;
		_mediaPlayer.MediaEnded += OnMediaEnded;
		_mediaPlayer.ScriptCommand += OnScriptCommand;
		_element.Loaded += OnLoaded;
		_element.Unloaded += OnUnloaded;
	}

	private void HandleStateChange()
	{
		MediaState mediaState = _mediaState._value;
		bool flag = false;
		bool flag2 = false;
		if (_isLoaded)
		{
			if (_clock._value != null)
			{
				mediaState = MediaState.Manual;
				flag = true;
			}
			else if (_loadedBehavior != 0)
			{
				mediaState = _loadedBehavior;
			}
			else if (_source._wasSet)
			{
				if (_loadedBehavior != 0)
				{
					mediaState = MediaState.Play;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		else if (_unloadedBehavior != 0)
		{
			mediaState = _unloadedBehavior;
		}
		else
		{
			Invariant.Assert(_unloadedBehavior == MediaState.Manual);
			if (_clock._value != null)
			{
				mediaState = MediaState.Manual;
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		bool flag3 = false;
		if (mediaState != MediaState.Close && mediaState != 0)
		{
			Invariant.Assert(!flag);
			if (_mediaPlayer.Clock != null)
			{
				_mediaPlayer.Clock = null;
			}
			if (_currentState == MediaState.Close || _source._isSet)
			{
				if (_isScrubbingEnabled._wasSet)
				{
					_mediaPlayer.ScrubbingEnabled = _isScrubbingEnabled._value;
					_isScrubbingEnabled._isSet = false;
				}
				if (_clock._value == null)
				{
					_mediaPlayer.Open(UriFromSourceUri(_source._value));
				}
				flag3 = true;
			}
		}
		else if (flag)
		{
			if (_currentState == MediaState.Close || _clock._isSet)
			{
				if (_isScrubbingEnabled._wasSet)
				{
					_mediaPlayer.ScrubbingEnabled = _isScrubbingEnabled._value;
					_isScrubbingEnabled._isSet = false;
				}
				_mediaPlayer.Clock = _clock._value;
				_clock._isSet = false;
				flag3 = true;
			}
		}
		else if (mediaState == MediaState.Close && _currentState != MediaState.Close)
		{
			_mediaPlayer.Clock = null;
			_mediaPlayer.Close();
			_currentState = MediaState.Close;
		}
		if (!(_currentState != MediaState.Close || flag3))
		{
			return;
		}
		if (_position._isSet)
		{
			_mediaPlayer.Position = _position._value;
			_position._isSet = false;
		}
		if (_volume._isSet || (flag3 && _volume._wasSet))
		{
			_mediaPlayer.Volume = _volume._value;
			_volume._isSet = false;
		}
		if (_balance._isSet || (flag3 && _balance._wasSet))
		{
			_mediaPlayer.Balance = _balance._value;
			_balance._isSet = false;
		}
		if (_isMuted._isSet || (flag3 && _isMuted._wasSet))
		{
			_mediaPlayer.IsMuted = _isMuted._value;
			_isMuted._isSet = false;
		}
		if (_isScrubbingEnabled._isSet)
		{
			_mediaPlayer.ScrubbingEnabled = _isScrubbingEnabled._value;
			_isScrubbingEnabled._isSet = false;
		}
		if (mediaState == MediaState.Play && _source._isSet)
		{
			_mediaPlayer.Play();
			if (!_speedRatio._wasSet)
			{
				_mediaPlayer.SpeedRatio = 1.0;
			}
			_source._isSet = false;
			_mediaState._isSet = false;
		}
		else if (_currentState != mediaState || (flag2 && _mediaState._isSet))
		{
			switch (mediaState)
			{
			case MediaState.Play:
				_mediaPlayer.Play();
				break;
			case MediaState.Pause:
				_mediaPlayer.Pause();
				break;
			case MediaState.Stop:
				_mediaPlayer.Stop();
				break;
			default:
				Invariant.Assert(condition: false, "Unexpected state request.");
				break;
			case MediaState.Manual:
				break;
			}
			if (flag2)
			{
				_mediaState._isSet = false;
			}
		}
		_currentState = mediaState;
		if (_speedRatio._isSet || (flag3 && _speedRatio._wasSet))
		{
			_mediaPlayer.SpeedRatio = _speedRatio._value;
			_speedRatio._isSet = false;
		}
	}

	private Uri UriFromSourceUri(Uri sourceUri)
	{
		if (sourceUri != null)
		{
			if (sourceUri.IsAbsoluteUri)
			{
				return sourceUri;
			}
			if (BaseUri != null)
			{
				return new Uri(BaseUri, sourceUri);
			}
		}
		return sourceUri;
	}

	internal static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			GetHelper(d).MemberOnInvalidateSource(e);
		}
	}

	private void MemberOnInvalidateSource(DependencyPropertyChangedEventArgs e)
	{
		if (_clock._value != null)
		{
			throw new InvalidOperationException(SR.MediaElement_CannotSetSourceOnMediaElementDrivenByClock);
		}
		_source._value = (Uri)e.NewValue;
		_source._wasSet = (_source._isSet = true);
		HandleStateChange();
	}

	private void OnMediaFailed(object sender, ExceptionEventArgs args)
	{
		_element.OnMediaFailed(sender, args);
	}

	private void OnMediaOpened(object sender, EventArgs args)
	{
		_element.InvalidateMeasure();
		_element.OnMediaOpened(sender, args);
	}

	private void OnBufferingStarted(object sender, EventArgs args)
	{
		_element.OnBufferingStarted(sender, args);
	}

	private void OnBufferingEnded(object sender, EventArgs args)
	{
		_element.OnBufferingEnded(sender, args);
	}

	private void OnMediaEnded(object sender, EventArgs args)
	{
		_element.OnMediaEnded(sender, args);
	}

	private void OnScriptCommand(object sender, MediaScriptCommandEventArgs args)
	{
		_element.OnScriptCommand(sender, args);
	}

	private void OnLoaded(object sender, RoutedEventArgs args)
	{
		_isLoaded = true;
		HandleStateChange();
	}

	private void OnUnloaded(object sender, RoutedEventArgs args)
	{
		_isLoaded = false;
		HandleStateChange();
	}
}
