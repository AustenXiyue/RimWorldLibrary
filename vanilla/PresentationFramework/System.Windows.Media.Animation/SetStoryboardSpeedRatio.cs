using System.ComponentModel;

namespace System.Windows.Media.Animation;

/// <summary>A trigger action that changes the speed of a <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class SetStoryboardSpeedRatio : ControllableStoryboardAction
{
	private double _speedRatio = 1.0;

	/// <summary>Gets or sets a new <see cref="T:System.Windows.Media.Animation.Storyboard" /> animation speed as a ratio of the old animation speed.</summary>
	/// <returns>The speed ratio for the <see cref="T:System.Windows.Media.Animation.Storyboard" />. The default value is 1.0. </returns>
	[DefaultValue(1.0)]
	public double SpeedRatio
	{
		get
		{
			return _speedRatio;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "SetStoryboardSpeedRatio"));
			}
			_speedRatio = value;
		}
	}

	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.SetSpeedRatio(containingFE, SpeedRatio);
		}
		else
		{
			storyboard.SetSpeedRatio(containingFCE, SpeedRatio);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SetStoryboardSpeedRatio" /> class.</summary>
	public SetStoryboardSpeedRatio()
	{
	}
}
