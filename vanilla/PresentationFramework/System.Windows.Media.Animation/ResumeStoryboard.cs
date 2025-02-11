namespace System.Windows.Media.Animation;

/// <summary>Supports a trigger action that resumes a paused <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class ResumeStoryboard : ControllableStoryboardAction
{
	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.Resume(containingFE);
		}
		else
		{
			storyboard.Resume(containingFCE);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.ResumeStoryboard" /> class. </summary>
	public ResumeStoryboard()
	{
	}
}
