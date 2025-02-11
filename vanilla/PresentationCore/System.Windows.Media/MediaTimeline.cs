using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides a <see cref="T:System.Windows.Media.Animation.Timeline" /> for media content.</summary>
public class MediaTimeline : Timeline, IUriContext
{
	internal const uint LastTimelineFlag = 1u;

	internal ITypeDescriptorContext _context;

	private Uri _baseUri;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.MediaTimeline.Source" /> dependency property.</summary>
	/// <returns>The identifier for the  <see cref="P:System.Windows.Media.MediaTimeline.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	internal static Uri s_Source;

	/// <summary>Gets or sets the base URI of the current application context. </summary>
	/// <returns>The base URI of the application context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return _baseUri;
		}
		set
		{
			_baseUri = value;
		}
	}

	/// <summary>Gets or sets the media source associated with the timeline.  </summary>
	/// <returns>The media source associated with the timeline. The default is null. </returns>
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValueInternal(SourceProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of a <see cref="T:System.Windows.Media.MediaTimeline" /> class using the supplied Uri as the media source.</summary>
	/// <param name="source">The media source for the timeline.</param>
	public MediaTimeline(Uri source)
		: this()
	{
		Source = source;
	}

	internal MediaTimeline(ITypeDescriptorContext context, Uri source)
		: this()
	{
		_context = context;
		Source = source;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MediaTimeline" /> class.</summary>
	public MediaTimeline()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MediaTimeline" /> that begins at the specified time.</summary>
	/// <param name="beginTime">The time to begin the timeline.</param>
	public MediaTimeline(TimeSpan? beginTime)
		: this()
	{
		base.BeginTime = beginTime;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MediaTimeline" /> that begins at the specified time and lasts for the specified duration.</summary>
	/// <param name="beginTime">The time to begin media playback.</param>
	/// <param name="duration">The length of time for media playback.</param>
	public MediaTimeline(TimeSpan? beginTime, Duration duration)
		: this()
	{
		base.BeginTime = beginTime;
		base.Duration = duration;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MediaTimeline" /> class that begins at the specified time over the specified time and has the specified repeat behavior.</summary>
	/// <param name="beginTime">The time to begin media playback.</param>
	/// <param name="duration">The length of time for media playback.</param>
	/// <param name="repeatBehavior">The repeat behavior to use when the playback duration has been reached.</param>
	public MediaTimeline(TimeSpan? beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: this()
	{
		base.BeginTime = beginTime;
		base.Duration = duration;
		base.RepeatBehavior = repeatBehavior;
	}

	/// <summary>Creates a <see cref="T:System.Windows.Media.MediaClock" /> for this timeline.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.MediaClock" /> for this timeline.</returns>
	protected internal override Clock AllocateClock()
	{
		if (Source == null)
		{
			throw new InvalidOperationException(SR.Media_UriNotSpecified);
		}
		return new MediaClock(this);
	}

	/// <summary>Makes this instance of MediaTimeline unmodifiable or determines whether it can be made unmodifiable.</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this <see cref="T:System.Windows.Media.MediaTimeline" /> can be made unmodifiable, or false if it cannot be made unmodifiable. If <paramref name="isChecking" /> is false, this method returns true if the specified <see cref="T:System.Windows.Media.MediaTimeline" /> is now unmodifiable, or false if it cannot be made unmodifiable, with the side effect of having made the actual change in frozen status to this object.</returns>
	/// <param name="isChecking">true to check if the timeline can be frozen; false to freeze the timeline. </param>
	protected override bool FreezeCore(bool isChecking)
	{
		bool flag = base.FreezeCore(isChecking);
		if (!flag)
		{
			return false;
		}
		if (isChecking)
		{
			flag &= !HasExpression(LookupEntry(SourceProperty.GlobalIndex), SourceProperty);
		}
		return flag;
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.MediaTimeline" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.MediaTimeline" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		MediaTimeline sourceTimeline = (MediaTimeline)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceTimeline);
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.MediaTimeline" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.MediaTimeline" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		MediaTimeline sourceTimeline = (MediaTimeline)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceTimeline);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.MediaTimeline" /> object. </summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.MediaTimeline" /> object to clone and freeze.</param>
	protected override void GetAsFrozenCore(Freezable source)
	{
		MediaTimeline sourceTimeline = (MediaTimeline)source;
		base.GetAsFrozenCore(source);
		CopyCommon(sourceTimeline);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.MediaTimeline" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.MediaTimeline" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		MediaTimeline sourceTimeline = (MediaTimeline)source;
		base.GetCurrentValueAsFrozenCore(source);
		CopyCommon(sourceTimeline);
	}

	private void CopyCommon(MediaTimeline sourceTimeline)
	{
		_context = sourceTimeline._context;
		_baseUri = sourceTimeline._baseUri;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.MediaClock" /> associated with the <see cref="T:System.Windows.Media.MediaTimeline" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Media.MediaClock" />.</returns>
	public new MediaClock CreateClock()
	{
		return (MediaClock)base.CreateClock();
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Duration" /> from a specified clock.</summary>
	/// <returns>If <paramref name="clock" /> is a <see cref="T:System.Windows.Media.MediaClock" />, the <see cref="P:System.Windows.Media.MediaPlayer.NaturalDuration" /> value of the <see cref="T:System.Windows.Media.MediaPlayer" /> associated with <paramref name="clock" />, or <see cref="P:System.Windows.Duration.Automatic" /> if the <paramref name="clock" /> is not a <see cref="T:System.Windows.Media.MediaClock" />. </returns>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.Clock" /> whose natural duration is desired.</param>
	protected override Duration GetNaturalDurationCore(Clock clock)
	{
		MediaClock mediaClock = (MediaClock)clock;
		if (mediaClock.Player == null)
		{
			return Duration.Automatic;
		}
		return mediaClock.Player.NaturalDuration;
	}

	/// <summary>Returns the string that represents the media source.</summary>
	/// <returns>The string that represents the media source.</returns>
	public override string ToString()
	{
		if (null == Source)
		{
			throw new InvalidOperationException(SR.Media_UriNotSpecified);
		}
		return Source.ToString();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.MediaTimeline" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MediaTimeline Clone()
	{
		return (MediaTimeline)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.MediaTimeline" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new MediaTimeline CloneCurrentValue()
	{
		return (MediaTimeline)base.CloneCurrentValue();
	}

	/// <summary>Creates a new instance of the MediaTimeline.</summary>
	/// <returns>The new instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new MediaTimeline();
	}

	static MediaTimeline()
	{
		Type typeFromHandle = typeof(MediaTimeline);
		SourceProperty = Animatable.RegisterProperty("Source", typeof(Uri), typeFromHandle, null, null, null, isIndependentlyAnimated: false, null);
	}
}
