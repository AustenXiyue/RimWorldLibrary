namespace System.Windows.Media.Animation;

/// <summary>A trigger action that stops a <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class StopStoryboard : ControllableStoryboardAction
{
	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.Stop(containingFE);
		}
		else
		{
			storyboard.Stop(containingFCE);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.StopStoryboard" /> class.</summary>
	public StopStoryboard()
	{
	}
}
