using System.IO;
using System.Text;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Media;

internal class MediaEventsHelper : IInvokable
{
	private MediaPlayer _sender;

	private Dispatcher _dispatcher;

	private DispatcherOperationCallback _mediaOpened;

	private DispatcherOperationCallback _mediaFailed;

	private DispatcherOperationCallback _mediaPrerolled;

	private DispatcherOperationCallback _mediaEnded;

	private DispatcherOperationCallback _bufferingStarted;

	private DispatcherOperationCallback _bufferingEnded;

	private DispatcherOperationCallback _scriptCommand;

	private DispatcherOperationCallback _newFrame;

	private UniqueEventHelper<ExceptionEventArgs> _mediaFailedHelper = new UniqueEventHelper<ExceptionEventArgs>();

	private UniqueEventHelper _mediaOpenedHelper = new UniqueEventHelper();

	private UniqueEventHelper _mediaPrerolledHelper = new UniqueEventHelper();

	private UniqueEventHelper _mediaEndedHelper = new UniqueEventHelper();

	private UniqueEventHelper _bufferingStartedHelper = new UniqueEventHelper();

	private UniqueEventHelper _bufferingEndedHelper = new UniqueEventHelper();

	private UniqueEventHelper<MediaScriptCommandEventArgs> _scriptCommandHelper = new UniqueEventHelper<MediaScriptCommandEventArgs>();

	private UniqueEventHelper _newFrameHelper = new UniqueEventHelper();

	internal event EventHandler<ExceptionEventArgs> MediaFailed
	{
		add
		{
			_mediaFailedHelper.AddEvent(value);
		}
		remove
		{
			_mediaFailedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler MediaOpened
	{
		add
		{
			_mediaOpenedHelper.AddEvent(value);
		}
		remove
		{
			_mediaOpenedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler MediaPrerolled
	{
		add
		{
			_mediaPrerolledHelper.AddEvent(value);
		}
		remove
		{
			_mediaPrerolledHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler MediaEnded
	{
		add
		{
			_mediaEndedHelper.AddEvent(value);
		}
		remove
		{
			_mediaEndedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler BufferingStarted
	{
		add
		{
			_bufferingStartedHelper.AddEvent(value);
		}
		remove
		{
			_bufferingStartedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler BufferingEnded
	{
		add
		{
			_bufferingEndedHelper.AddEvent(value);
		}
		remove
		{
			_bufferingEndedHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler<MediaScriptCommandEventArgs> ScriptCommand
	{
		add
		{
			_scriptCommandHelper.AddEvent(value);
		}
		remove
		{
			_scriptCommandHelper.RemoveEvent(value);
		}
	}

	internal event EventHandler NewFrame
	{
		add
		{
			_newFrameHelper.AddEvent(value);
		}
		remove
		{
			_newFrameHelper.RemoveEvent(value);
		}
	}

	private event DispatcherOperationCallback DispatcherMediaFailed;

	private event DispatcherOperationCallback DispatcherMediaOpened;

	private event DispatcherOperationCallback DispatcherMediaPrerolled;

	private event DispatcherOperationCallback DispatcherMediaEnded;

	private event DispatcherOperationCallback DispatcherBufferingStarted;

	private event DispatcherOperationCallback DispatcherBufferingEnded;

	private event DispatcherOperationCallback DispatcherScriptCommand;

	private event DispatcherOperationCallback DispatcherMediaNewFrame;

	internal MediaEventsHelper(MediaPlayer mediaPlayer)
	{
		_mediaOpened = OnMediaOpened;
		DispatcherMediaOpened += _mediaOpened;
		_mediaFailed = OnMediaFailed;
		DispatcherMediaFailed += _mediaFailed;
		_mediaPrerolled = OnMediaPrerolled;
		DispatcherMediaPrerolled += _mediaPrerolled;
		_mediaEnded = OnMediaEnded;
		DispatcherMediaEnded += _mediaEnded;
		_bufferingStarted = OnBufferingStarted;
		DispatcherBufferingStarted += _bufferingStarted;
		_bufferingEnded = OnBufferingEnded;
		DispatcherBufferingEnded += _bufferingEnded;
		_scriptCommand = OnScriptCommand;
		DispatcherScriptCommand += _scriptCommand;
		_newFrame = OnNewFrame;
		DispatcherMediaNewFrame += _newFrame;
		SetSender(mediaPlayer);
	}

	internal static void CreateMediaEventsHelper(MediaPlayer mediaPlayer, out MediaEventsHelper eventsHelper, out SafeMILHandle unmanagedProxy)
	{
		eventsHelper = new MediaEventsHelper(mediaPlayer);
		unmanagedProxy = EventProxyWrapper.CreateEventProxyWrapper(eventsHelper);
	}

	internal void SetSender(MediaPlayer sender)
	{
		_sender = sender;
		_dispatcher = sender.Dispatcher;
	}

	internal void RaiseMediaFailed(Exception e)
	{
		if (this.DispatcherMediaFailed != null)
		{
			_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherMediaFailed, new ExceptionEventArgs(e));
		}
	}

	void IInvokable.RaiseEvent(byte[] buffer, int cb)
	{
		AVEvent aVEvent = AVEvent.AVMediaNone;
		int num = 0;
		int num2 = 0;
		num2 = 16;
		if (cb < num2)
		{
			return;
		}
		using BinaryReader binaryReader = new BinaryReader(new MemoryStream(buffer));
		aVEvent = (AVEvent)binaryReader.ReadUInt32();
		num = (int)binaryReader.ReadUInt32();
		switch (aVEvent)
		{
		case AVEvent.AVMediaOpened:
			if (this.DispatcherMediaOpened != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherMediaOpened, null);
			}
			break;
		case AVEvent.AVMediaFailed:
			RaiseMediaFailed(HRESULT.ConvertHRToException(num));
			break;
		case AVEvent.AVMediaBufferingStarted:
			if (this.DispatcherBufferingStarted != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherBufferingStarted, null);
			}
			break;
		case AVEvent.AVMediaBufferingEnded:
			if (this.DispatcherBufferingEnded != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherBufferingEnded, null);
			}
			break;
		case AVEvent.AVMediaEnded:
			if (this.DispatcherMediaEnded != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherMediaEnded, null);
			}
			break;
		case AVEvent.AVMediaPrerolled:
			if (this.DispatcherMediaPrerolled != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherMediaPrerolled, null);
			}
			break;
		case AVEvent.AVMediaScriptCommand:
			HandleScriptCommand(binaryReader);
			break;
		case AVEvent.AVMediaNewFrame:
			if (this.DispatcherMediaNewFrame != null)
			{
				_dispatcher.BeginInvoke(DispatcherPriority.Background, this.DispatcherMediaNewFrame, null);
			}
			break;
		case AVEvent.AVMediaClosed:
		case AVEvent.AVMediaStarted:
		case AVEvent.AVMediaStopped:
		case AVEvent.AVMediaPaused:
		case AVEvent.AVMediaRateChanged:
			break;
		}
	}

	private void HandleScriptCommand(BinaryReader reader)
	{
		int stringLength = (int)reader.ReadUInt32();
		int stringLength2 = (int)reader.ReadUInt32();
		if (this.DispatcherScriptCommand != null)
		{
			string stringFromReader = GetStringFromReader(reader, stringLength);
			string stringFromReader2 = GetStringFromReader(reader, stringLength2);
			_dispatcher.BeginInvoke(DispatcherPriority.Normal, this.DispatcherScriptCommand, new MediaScriptCommandEventArgs(stringFromReader, stringFromReader2));
		}
	}

	private string GetStringFromReader(BinaryReader reader, int stringLength)
	{
		StringBuilder stringBuilder = new StringBuilder(stringLength);
		stringBuilder.Length = stringLength;
		for (int i = 0; i < stringLength; i++)
		{
			stringBuilder[i] = (char)reader.ReadUInt16();
		}
		return stringBuilder.ToString();
	}

	private object OnMediaOpened(object o)
	{
		_mediaOpenedHelper.InvokeEvents(_sender, null);
		return null;
	}

	private object OnMediaPrerolled(object o)
	{
		_mediaPrerolledHelper.InvokeEvents(_sender, null);
		return null;
	}

	private object OnMediaEnded(object o)
	{
		_mediaEndedHelper.InvokeEvents(_sender, null);
		return null;
	}

	private object OnBufferingStarted(object o)
	{
		_bufferingStartedHelper.InvokeEvents(_sender, null);
		return null;
	}

	private object OnBufferingEnded(object o)
	{
		_bufferingEndedHelper.InvokeEvents(_sender, null);
		return null;
	}

	private object OnMediaFailed(object o)
	{
		ExceptionEventArgs args = (ExceptionEventArgs)o;
		_mediaFailedHelper.InvokeEvents(_sender, args);
		return null;
	}

	private object OnScriptCommand(object o)
	{
		MediaScriptCommandEventArgs args = (MediaScriptCommandEventArgs)o;
		_scriptCommandHelper.InvokeEvents(_sender, args);
		return null;
	}

	private object OnNewFrame(object e)
	{
		_newFrameHelper.InvokeEvents(_sender, null);
		return null;
	}
}
