namespace System.Windows.Media.Animation;

/// <summary>A trigger action that removes a <see cref="T:System.Windows.Media.Animation.Storyboard" />.</summary>
public sealed class RemoveStoryboard : ControllableStoryboardAction
{
	internal override void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
		if (containingFE != null)
		{
			storyboard.Remove(containingFE);
		}
		else
		{
			storyboard.Remove(containingFCE);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.RemoveStoryboard" /> class.</summary>
	public RemoveStoryboard()
	{
	}
}
