using System.ComponentModel;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Interactively controls a <see cref="T:System.Windows.Media.Animation.Clock" />.</summary>
public sealed class ClockController : DispatcherObject
{
	private Clock _owner;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Animation.Clock" /> controlled by this <see cref="T:System.Windows.Media.Animation.ClockController" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.Clock" /> controlled by this <see cref="T:System.Windows.Media.Animation.ClockController" />.</returns>
	public Clock Clock => _owner;

	/// <summary>Gets or sets the interactive speed of the target <see cref="T:System.Windows.Media.Animation.Clock" />.</summary>
	/// <returns>A finite value greater than zero that describes the interactive speed of the target clock. This value is multiplied against the value of the <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> of the clock's <see cref="T:System.Windows.Media.Animation.Timeline" />. For example, if the timeline's <see cref="P:System.Windows.Media.Animation.Timeline.SpeedRatio" /> is 0.5 and the <see cref="T:System.Windows.Media.Animation.ClockController" /> object's <see cref="P:System.Windows.Media.Animation.ClockController.SpeedRatio" /> is 3.0, the timeline moves at 1.5 times normal speed (0.5 * 3.0). The default value is 1.0.</returns>
	public double SpeedRatio
	{
		get
		{
			return _owner.InternalGetSpeedRatio();
		}
		set
		{
			if (value < 0.0 || value > double.MaxValue || double.IsNaN(value))
			{
				throw new ArgumentException(SR.Timing_InvalidArgFinitePositive, "value");
			}
			_owner.InternalSetSpeedRatio(value);
		}
	}

	internal ClockController(Clock owner)
	{
		_owner = owner;
	}

	/// <summary>Sets the target <see cref="P:System.Windows.Media.Animation.ClockController.Clock" /> to begin at the next tick.</summary>
	public void Begin()
	{
		_owner.InternalBegin();
	}

	/// <summary>Advances the current time of the target <see cref="T:System.Windows.Media.Animation.Clock" /> to the end of its active period. </summary>
	public void SkipToFill()
	{
		_owner.InternalSkipToFill();
	}

	/// <summary>Stops the target <see cref="T:System.Windows.Media.Animation.Clock" /> from progressing. </summary>
	public void Pause()
	{
		_owner.InternalPause();
	}

	/// <summary>Enables a <see cref="T:System.Windows.Media.Animation.Clock" /> that was previously paused to resume progressing.</summary>
	public void Resume()
	{
		_owner.InternalResume();
	}

	/// <summary>Seeks the target <see cref="P:System.Windows.Media.Animation.ClockController.Clock" /> by the specified amount when the next tick occurs. If the target clock is stopped, seeking makes it active again.</summary>
	/// <param name="offset">The seek offset, measured in the target clock's time. This offset is relative to the clock's <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.BeginTime" /> or <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.Duration" />, depending on the value of <paramref name="origin" />. </param>
	/// <param name="origin">A value that indicates whether the specified offset is relative to the target clock's <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.BeginTime" /> or <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.Duration" />.</param>
	public void Seek(TimeSpan offset, TimeSeekOrigin origin)
	{
		if (!TimeEnumHelper.IsValidTimeSeekOrigin(origin))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "TimeSeekOrigin"));
		}
		if (origin == TimeSeekOrigin.Duration)
		{
			Duration resolvedDuration = _owner.ResolvedDuration;
			if (!resolvedDuration.HasTimeSpan)
			{
				throw new InvalidOperationException(SR.Timing_SeekDestinationIndefinite);
			}
			offset += resolvedDuration.TimeSpan;
		}
		if (offset < TimeSpan.Zero)
		{
			throw new InvalidOperationException(SR.Timing_SeekDestinationNegative);
		}
		_owner.InternalSeek(offset);
	}

	/// <summary>Seeks the target <see cref="T:System.Windows.Media.Animation.Clock" /> by the specified amount immediately. If the target clock is stopped, seeking makes it active again.</summary>
	/// <param name="offset">The seek offset, measured in the target clock's time. This offset is relative to the clock's <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.BeginTime" /> or <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.Duration" />, depending on the value of <paramref name="origin" />.</param>
	/// <param name="origin">A value that indicates whether the specified offset is relative to the target clock's <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.BeginTime" /> or <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.Duration" />.</param>
	public void SeekAlignedToLastTick(TimeSpan offset, TimeSeekOrigin origin)
	{
		if (!TimeEnumHelper.IsValidTimeSeekOrigin(origin))
		{
			throw new InvalidEnumArgumentException(SR.Format(SR.Enum_Invalid, "TimeSeekOrigin"));
		}
		if (origin == TimeSeekOrigin.Duration)
		{
			Duration resolvedDuration = _owner.ResolvedDuration;
			if (!resolvedDuration.HasTimeSpan)
			{
				throw new InvalidOperationException(SR.Timing_SeekDestinationIndefinite);
			}
			offset += resolvedDuration.TimeSpan;
		}
		if (offset < TimeSpan.Zero)
		{
			throw new InvalidOperationException(SR.Timing_SeekDestinationNegative);
		}
		_owner.InternalSeekAlignedToLastTick(offset);
	}

	/// <summary>Stops the target <see cref="T:System.Windows.Media.Animation.Clock" />. </summary>
	public void Stop()
	{
		_owner.InternalStop();
	}

	/// <summary>Removes the <see cref="T:System.Windows.Media.Animation.Clock" /> associated with this <see cref="T:System.Windows.Media.Animation.ClockController" /> from the properties it animates. The clock and its child clocks will no longer affect these properties. </summary>
	public void Remove()
	{
		_owner.InternalRemove();
	}
}
