using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using Mono.Audio;

namespace System.Media;

/// <summary>Controls playback of a sound from a .wav file.</summary>
[Serializable]
[ToolboxItem(false)]
public class SoundPlayer : Component, ISerializable
{
	private string sound_location;

	private Stream audiostream;

	private object tag = string.Empty;

	private MemoryStream mstream;

	private bool load_completed;

	private int load_timeout = 10000;

	private AudioDevice adev;

	private AudioData adata;

	private bool stopped;

	private Win32SoundPlayer win32_player;

	private static readonly bool use_win32_player;

	/// <summary>Gets a value indicating whether loading of a .wav file has successfully completed.</summary>
	/// <returns>true if a .wav file is loaded; false if a .wav file has not yet been loaded.</returns>
	public bool IsLoadCompleted => load_completed;

	/// <summary>Gets or sets the time, in milliseconds, in which the .wav file must load.</summary>
	/// <returns>The number of milliseconds to wait. The default is 10000 (10 seconds).</returns>
	public int LoadTimeout
	{
		get
		{
			return load_timeout;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("timeout must be >= 0");
			}
			load_timeout = value;
		}
	}

	/// <summary>Gets or sets the file path or URL of the .wav file to load.</summary>
	/// <returns>The file path or URL from which to load a .wav file, or <see cref="F:System.String.Empty" /> if no file path is present. The default is <see cref="F:System.String.Empty" />.</returns>
	public string SoundLocation
	{
		get
		{
			return sound_location;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			sound_location = value;
			load_completed = false;
			OnSoundLocationChanged(EventArgs.Empty);
			if (this.SoundLocationChanged != null)
			{
				this.SoundLocationChanged(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.IO.Stream" /> from which to load the .wav file.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> from which to load the .wav file, or null if no stream is available. The default is null.</returns>
	public Stream Stream
	{
		get
		{
			return audiostream;
		}
		set
		{
			if (audiostream != value)
			{
				audiostream = value;
				load_completed = false;
				OnStreamChanged(EventArgs.Empty);
				if (this.StreamChanged != null)
				{
					this.StreamChanged(this, EventArgs.Empty);
				}
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Object" /> that contains data about the <see cref="T:System.Media.SoundPlayer" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that contains data about the <see cref="T:System.Media.SoundPlayer" />.</returns>
	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	/// <summary>Occurs when a .wav file has been successfully or unsuccessfully loaded.</summary>
	public event AsyncCompletedEventHandler LoadCompleted;

	/// <summary>Occurs when a new audio source path for this <see cref="T:System.Media.SoundPlayer" /> has been set.</summary>
	public event EventHandler SoundLocationChanged;

	/// <summary>Occurs when a new <see cref="T:System.IO.Stream" /> audio source for this <see cref="T:System.Media.SoundPlayer" /> has been set.</summary>
	public event EventHandler StreamChanged;

	static SoundPlayer()
	{
		use_win32_player = Environment.OSVersion.Platform != PlatformID.Unix;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Media.SoundPlayer" /> class.</summary>
	public SoundPlayer()
	{
		sound_location = string.Empty;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Media.SoundPlayer" /> class, and attaches the .wav file within the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">A <see cref="T:System.IO.Stream" /> to a .wav file.</param>
	public SoundPlayer(Stream stream)
		: this()
	{
		audiostream = stream;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Media.SoundPlayer" /> class, and attaches the specified .wav file.</summary>
	/// <param name="soundLocation">The location of a .wav file to load.</param>
	/// <exception cref="T:System.UriFormatException">The URL value specified by <paramref name="soundLocation" /> cannot be resolved.</exception>
	public SoundPlayer(string soundLocation)
		: this()
	{
		if (soundLocation == null)
		{
			throw new ArgumentNullException("soundLocation");
		}
		sound_location = soundLocation;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Media.SoundPlayer" /> class.</summary>
	/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to be used for deserialization.</param>
	/// <param name="context">The destination to be used for deserialization.</param>
	/// <exception cref="T:System.UriFormatException">The <see cref="P:System.Media.SoundPlayer.SoundLocation" /> specified in <paramref name="serializationInfo" /> cannot be resolved.</exception>
	protected SoundPlayer(SerializationInfo serializationInfo, StreamingContext context)
		: this()
	{
		throw new NotImplementedException();
	}

	private void LoadFromStream(Stream s)
	{
		mstream = new MemoryStream();
		byte[] buffer = new byte[4096];
		int count;
		while ((count = s.Read(buffer, 0, 4096)) > 0)
		{
			mstream.Write(buffer, 0, count);
		}
		mstream.Position = 0L;
	}

	private void LoadFromUri(string location)
	{
		mstream = null;
		Stream stream = null;
		if (string.IsNullOrEmpty(location))
		{
			return;
		}
		stream = ((!File.Exists(location)) ? WebRequest.Create(location).GetResponse().GetResponseStream() : new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read));
		using (stream)
		{
			LoadFromStream(stream);
		}
	}

	/// <summary>Loads a sound synchronously.</summary>
	/// <exception cref="T:System.ServiceProcess.TimeoutException">The elapsed time during loading exceeds the time, in milliseconds, specified by <see cref="P:System.Media.SoundPlayer.LoadTimeout" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> cannot be found.</exception>
	public void Load()
	{
		if (load_completed)
		{
			return;
		}
		if (audiostream != null)
		{
			LoadFromStream(audiostream);
		}
		else
		{
			LoadFromUri(sound_location);
		}
		adata = null;
		adev = null;
		load_completed = true;
		AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(null, cancelled: false, this);
		OnLoadCompleted(e);
		if (this.LoadCompleted != null)
		{
			this.LoadCompleted(this, e);
		}
		if (use_win32_player)
		{
			if (win32_player == null)
			{
				win32_player = new Win32SoundPlayer(mstream);
			}
			else
			{
				win32_player.Stream = mstream;
			}
		}
	}

	private void AsyncFinished(IAsyncResult ar)
	{
		(ar.AsyncState as ThreadStart).EndInvoke(ar);
	}

	/// <summary>Loads a .wav file from a stream or a Web resource using a new thread.</summary>
	/// <exception cref="T:System.ServiceProcess.TimeoutException">The elapsed time during loading exceeds the time, in milliseconds, specified by <see cref="P:System.Media.SoundPlayer.LoadTimeout" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> cannot be found.</exception>
	public void LoadAsync()
	{
		if (!load_completed)
		{
			ThreadStart threadStart = Load;
			threadStart.BeginInvoke(AsyncFinished, threadStart);
		}
	}

	/// <summary>Raises the <see cref="E:System.Media.SoundPlayer.LoadCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.ComponentModel.AsyncCompletedEventArgs" />  that contains the event data. </param>
	protected virtual void OnLoadCompleted(AsyncCompletedEventArgs e)
	{
	}

	/// <summary>Raises the <see cref="E:System.Media.SoundPlayer.SoundLocationChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnSoundLocationChanged(EventArgs e)
	{
	}

	/// <summary>Raises the <see cref="E:System.Media.SoundPlayer.StreamChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnStreamChanged(EventArgs e)
	{
	}

	private void Start()
	{
		if (!use_win32_player)
		{
			stopped = false;
			if (adata != null)
			{
				adata.IsStopped = false;
			}
		}
		if (!load_completed)
		{
			Load();
		}
	}

	/// <summary>Plays the .wav file using a new thread, and loads the .wav file first if it has not been loaded.</summary>
	/// <exception cref="T:System.ServiceProcess.TimeoutException">The elapsed time during loading exceeds the time, in milliseconds, specified by <see cref="P:System.Media.SoundPlayer.LoadTimeout" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> cannot be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">The .wav header is corrupted; the file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> is not a PCM .wav file.</exception>
	public void Play()
	{
		if (!use_win32_player)
		{
			ThreadStart threadStart = PlaySync;
			threadStart.BeginInvoke(AsyncFinished, threadStart);
			return;
		}
		Start();
		if (mstream == null)
		{
			SystemSounds.Beep.Play();
		}
		else
		{
			win32_player.Play();
		}
	}

	private void PlayLoop()
	{
		Start();
		if (mstream == null)
		{
			SystemSounds.Beep.Play();
			return;
		}
		while (!stopped)
		{
			PlaySync();
		}
	}

	/// <summary>Plays and loops the .wav file using a new thread, and loads the .wav file first if it has not been loaded.</summary>
	/// <exception cref="T:System.ServiceProcess.TimeoutException">The elapsed time during loading exceeds the time, in milliseconds, specified by <see cref="P:System.Media.SoundPlayer.LoadTimeout" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> cannot be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">The .wav header is corrupted; the file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> is not a PCM .wav file.</exception>
	public void PlayLooping()
	{
		if (!use_win32_player)
		{
			ThreadStart threadStart = PlayLoop;
			threadStart.BeginInvoke(AsyncFinished, threadStart);
			return;
		}
		Start();
		if (mstream == null)
		{
			SystemSounds.Beep.Play();
		}
		else
		{
			win32_player.PlayLooping();
		}
	}

	/// <summary>Plays the .wav file and loads the .wav file first if it has not been loaded.</summary>
	/// <exception cref="T:System.ServiceProcess.TimeoutException">The elapsed time during loading exceeds the time, in milliseconds, specified by <see cref="P:System.Media.SoundPlayer.LoadTimeout" />. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> cannot be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">The .wav header is corrupted; the file specified by <see cref="P:System.Media.SoundPlayer.SoundLocation" /> is not a PCM .wav file.</exception>
	public void PlaySync()
	{
		Start();
		if (mstream == null)
		{
			SystemSounds.Beep.Play();
			return;
		}
		if (!use_win32_player)
		{
			try
			{
				if (adata == null)
				{
					adata = new WavData(mstream);
				}
				if (adev == null)
				{
					adev = AudioDevice.CreateDevice(null);
				}
				if (adata != null)
				{
					adata.Setup(adev);
					adata.Play(adev);
				}
				return;
			}
			catch
			{
				return;
			}
		}
		win32_player.PlaySync();
	}

	/// <summary>Stops playback of the sound if playback is occurring.</summary>
	public void Stop()
	{
		if (!use_win32_player)
		{
			stopped = true;
			if (adata != null)
			{
				adata.IsStopped = true;
			}
		}
		else
		{
			win32_player.Stop();
		}
	}

	/// <summary>For a description of this member, see the <see cref="M:System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> method.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" />  to populate with data.</param>
	/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
}
