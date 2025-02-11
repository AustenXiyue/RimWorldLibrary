namespace System.Windows.Media.Animation;

/// <summary>A trigger action that pauses a <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class PauseStoryboard : ControllableStoryboardAction
{
	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.Pause(containingFE);
		}
		else
		{
			storyboard.Pause(containingFCE);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.PauseStoryboard" /> class.</summary>
	public PauseStoryboard()
	{
	}
}
