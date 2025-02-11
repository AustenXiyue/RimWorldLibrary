namespace System.Windows.Media.Animation;

/// <summary>A trigger action that advances a <see cref="T:System.Windows.Media.Animation.Storyboard" /> to the end of its fill period. </summary>
public sealed class SkipStoryboardToFill : ControllableStoryboardAction
{
	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.SkipToFill(containingFE);
		}
		else
		{
			storyboard.SkipToFill(containingFCE);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.SkipStoryboardToFill" /> class.</summary>
	public SkipStoryboardToFill()
	{
	}
}
