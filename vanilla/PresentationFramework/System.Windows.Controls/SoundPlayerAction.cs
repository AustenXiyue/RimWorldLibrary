using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a lightweight audio playback <see cref="T:System.Windows.TriggerAction" /> used to play .wav files.</summary>
public class SoundPlayerAction : TriggerAction, IDisposable
{
	private delegate Stream LoadStreamCaller(Uri uri);

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.SoundPlayerAction.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.SoundPlayerAction.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(SoundPlayerAction), new FrameworkPropertyMetadata(OnSourceChanged));

	private SoundPlayer m_player;

	private Uri m_lastRequestedAbsoluteUri;

	private bool m_streamLoadInProgress;

	private bool m_playRequested;

	private bool m_uriChangedWhileLoadingStream;

	/// <summary>Gets or sets the audio source location. </summary>
	/// <returns>The audio source location.</returns>
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.SoundPlayerAction" /> class.</summary>
	public SoundPlayerAction()
	{
	}

	/// <summary>Releases the resources used by the <see cref="T:System.Windows.Controls.SoundPlayerAction" /> class.</summary>
	public void Dispose()
	{
		if (m_player != null)
		{
			m_player.Dispose();
		}
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((SoundPlayerAction)d).OnSourceChangedHelper((Uri)e.NewValue);
	}

	private void OnSourceChangedHelper(Uri newValue)
	{
		if (newValue == null || newValue.IsAbsoluteUri)
		{
			m_lastRequestedAbsoluteUri = newValue;
		}
		else
		{
			m_lastRequestedAbsoluteUri = BaseUriHelper.GetResolvedUri(BaseUriHelper.BaseUri, newValue);
		}
		m_player = null;
		m_playRequested = false;
		if (m_streamLoadInProgress)
		{
			m_uriChangedWhileLoadingStream = true;
		}
		else
		{
			BeginLoadStream();
		}
	}

	internal sealed override void Invoke(FrameworkElement el, FrameworkContentElement ctntEl, Style targetStyle, FrameworkTemplate targetTemplate, long layer)
	{
		PlayWhenLoaded();
	}

	internal sealed override void Invoke(FrameworkElement el)
	{
		PlayWhenLoaded();
	}

	private void PlayWhenLoaded()
	{
		if (m_streamLoadInProgress)
		{
			m_playRequested = true;
		}
		else if (m_player != null)
		{
			m_player.Play();
		}
	}

	private void BeginLoadStream()
	{
		if (m_lastRequestedAbsoluteUri != null)
		{
			m_streamLoadInProgress = true;
			Task.Run(delegate
			{
				Stream asyncResult = WpfWebRequestHelper.CreateRequestAndGetResponseStream(m_lastRequestedAbsoluteUri);
				LoadStreamCallback(asyncResult);
			});
		}
	}

	private Stream LoadStreamAsync(Uri uri)
	{
		return WpfWebRequestHelper.CreateRequestAndGetResponseStream(uri);
	}

	private void LoadStreamCallback(Stream asyncResult)
	{
		DispatcherOperationCallback method = OnLoadStreamCompleted;
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, method, asyncResult);
	}

	private object OnLoadStreamCompleted(object asyncResultArg)
	{
		Stream stream = (Stream)asyncResultArg;
		if (m_uriChangedWhileLoadingStream)
		{
			m_uriChangedWhileLoadingStream = false;
			stream?.Dispose();
			BeginLoadStream();
		}
		else if (stream != null)
		{
			if (m_player == null)
			{
				m_player = new SoundPlayer(stream);
			}
			else
			{
				m_player.Stream = stream;
			}
			m_player.LoadCompleted += OnSoundPlayerLoadCompleted;
			m_player.LoadAsync();
		}
		return null;
	}

	private void OnSoundPlayerLoadCompleted(object sender, AsyncCompletedEventArgs e)
	{
		if (m_player != sender)
		{
			return;
		}
		if (m_uriChangedWhileLoadingStream)
		{
			m_player = null;
			m_uriChangedWhileLoadingStream = false;
			BeginLoadStream();
			return;
		}
		m_streamLoadInProgress = false;
		if (m_playRequested)
		{
			m_playRequested = false;
			m_player.Play();
		}
	}
}
