using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>Represents the visual appearance of the control when it is in a specific state.</summary>
[ContentProperty("Storyboard")]
[RuntimeNameProperty("Name")]
public class VisualState : DependencyObject
{
	private static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(VisualState));

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.VisualState" />.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.VisualState" />.</returns>
	public string Name { get; set; }

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Animation.Storyboard" /> that defines the appearance of the control when it is in the state that is represented by the <see cref="T:System.Windows.VisualState" />. </summary>
	/// <returns>A storyboard that defines the appearance of the control when it is in the state that is represented by the <see cref="T:System.Windows.VisualState" />. The default is null.</returns>
	public Storyboard Storyboard
	{
		get
		{
			return (Storyboard)GetValue(StoryboardProperty);
		}
		set
		{
			SetValue(StoryboardProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.VisualState" /> class. </summary>
	public VisualState()
	{
	}
}
