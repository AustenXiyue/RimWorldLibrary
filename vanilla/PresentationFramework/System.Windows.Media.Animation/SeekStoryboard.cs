using System.ComponentModel;

namespace System.Windows.Media.Animation;

/// <summary>A trigger action that provides functionality for seeking (skipping) to a specified time within the active period of a <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class SeekStoryboard : ControllableStoryboardAction
{
	private TimeSpan _offset = TimeSpan.Zero;

	private TimeSeekOrigin _origin;

	/// <summary>Gets or sets the amount by which the storyboard should move forward or backward from the seek origin <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Origin" />. .</summary>
	/// <returns>A positive or negative value that specifies the amount by which the storyboard should move forward or backward from the seek origin <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Origin" />. The default value is 0. </returns>
	public TimeSpan Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SeekStoryboard"));
			}
			_offset = value;
		}
	}

	/// <summary>Gets or sets the position from which this seek operation's <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Offset" /> is applied. </summary>
	/// <returns>The position from which this seek operation's <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Offset" /> is applied. The default value is <see cref="F:System.Windows.Media.Animation.TimeSeekOrigin.BeginTime" />.</returns>
	[DefaultValue(TimeSeekOrigin.BeginTime)]
	public TimeSeekOrigin Origin
	{
		get
		{
			return _origin;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SeekStoryboard"));
			}
			if (value == TimeSeekOrigin.BeginTime || value == TimeSeekOrigin.Duration)
			{
				_origin = value;
				return;
			}
			throw new ArgumentException(SR.Storyboard_UnrecognizedTimeSeekOrigin);
		}
	}

	/// <summary>Returns a value that indicates whether the <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Offset" /> property of this <see cref="T:System.Windows.Media.Animation.SeekStoryboard" /> should be serialized.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Media.Animation.SeekStoryboard.Offset" /> property of this <see cref="T:System.Windows.Media.Animation.SeekStoryboard" /> should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeOffset()
	{
		return !TimeSpan.Zero.Equals(_offset);
	}

	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.Seek(containingFE, Offset, Origin);
		}
		else
		{
			storyboard.Seek(containingFCE, Offset, Origin);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SeekStoryboard" /> class.</summary>
	public SeekStoryboard()
	{
	}
}
