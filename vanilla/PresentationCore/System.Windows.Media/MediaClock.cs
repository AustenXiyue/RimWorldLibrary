using System.IO.Packaging;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>Maintains the timing state for media through a <see cref="T:System.Windows.Media.MediaTimeline" />.</summary>
public class MediaClock : Clock
{
	private MediaPlayer _mediaPlayer;

	/// <summary>Gets the <see cref="T:System.Windows.Media.MediaTimeline" /> that describes the controlling behavior of the clock. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.MediaTimeline" /> that describes the controlling behavior of the clock.</returns>
	public new MediaTimeline Timeline => (MediaTimeline)base.Timeline;

	internal override bool NeedsTicksWhenActive => true;

	internal MediaPlayer Player
	{
		get
		{
			return _mediaPlayer;
		}
		set
		{
			MediaPlayer mediaPlayer = _mediaPlayer;
			if (value != mediaPlayer)
			{
				_mediaPlayer = value;
				if (mediaPlayer != null)
				{
					mediaPlayer.Clock = null;
				}
				if (value != null)
				{
					value.Clock = this;
					Uri baseUri = ((IUriContext)Timeline).BaseUri;
					Uri uri = null;
					uri = ((!(baseUri != null) || !(baseUri.Scheme != PackUriHelper.UriSchemePack) || Timeline.Source.IsAbsoluteUri) ? Timeline.Source : new Uri(baseUri, Timeline.Source));
					value.SetSource(uri);
					SpeedChanged();
				}
			}
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.MediaClock" /> class. </summary>
	/// <param name="media">The timeline to use as a template for the media clock.</param>
	protected internal MediaClock(MediaTimeline media)
		: base(media)
	{
	}

	/// <summary>Retrieves a value that indicates whether the media clock can slip.</summary>
	/// <returns>true if the media clock can slip; otherwise false.</returns>
	protected override bool GetCanSlip()
	{
		return true;
	}

	/// <summary>Retrieves a value that identifies the actual media time. This value can be used for slip synchronization.</summary>
	/// <returns>The actual media time.</returns>
	protected override TimeSpan GetCurrentTimeCore()
	{
		if (_mediaPlayer != null)
		{
			return _mediaPlayer.Position;
		}
		return base.GetCurrentTimeCore();
	}

	/// <summary>Invoked when the clock is stopped.</summary>
	protected override void Stopped()
	{
		if (_mediaPlayer != null)
		{
			_mediaPlayer.SetSpeedRatio(0.0);
			_mediaPlayer.SetPosition(TimeSpan.FromTicks(0L));
		}
	}

	/// <summary>Invoked when the clock speed has changed.</summary>
	protected override void SpeedChanged()
	{
		Sync();
	}

	/// <summary>Invoked when movement is discontinues.</summary>
	protected override void DiscontinuousTimeMovement()
	{
		Sync();
	}

	private void Sync()
	{
		if (_mediaPlayer != null)
		{
			double? currentGlobalSpeed = base.CurrentGlobalSpeed;
			double num = (currentGlobalSpeed.HasValue ? currentGlobalSpeed.Value : 0.0);
			TimeSpan? currentTime = base.CurrentTime;
			TimeSpan position = (currentTime.HasValue ? currentTime.Value : TimeSpan.Zero);
			if (num == 0.0)
			{
				_mediaPlayer.SetSpeedRatio(num);
				_mediaPlayer.SetPosition(position);
			}
			else
			{
				_mediaPlayer.SetPosition(position);
				_mediaPlayer.SetSpeedRatio(num);
			}
		}
	}
}
