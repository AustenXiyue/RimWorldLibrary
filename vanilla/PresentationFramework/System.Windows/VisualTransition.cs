using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>Represents the visual behavior that occurs when a control transitions from one state to another.</summary>
[ContentProperty("Storyboard")]
public class VisualTransition : DependencyObject
{
	private Duration _generatedDuration = new Duration(default(TimeSpan));

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.VisualState" /> to transition from.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.VisualState" /> to transition from.</returns>
	public string From { get; set; }

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.VisualState" /> to transition to.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.VisualState" /> to transition to.</returns>
	public string To { get; set; }

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Animation.Storyboard" /> that occurs when the transition occurs.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.Storyboard" /> that occurs when the transition occurs.</returns>
	public Storyboard Storyboard { get; set; }

	/// <summary>Gets or sets the time that it takes to move from one state to another.</summary>
	/// <returns>The time that it takes to move from one state to another.</returns>
	[TypeConverter(typeof(DurationConverter))]
	public Duration GeneratedDuration
	{
		get
		{
			return _generatedDuration;
		}
		set
		{
			_generatedDuration = value;
		}
	}

	/// <summary>Gets or sets a custom mathematical formula that is used to transition between states.</summary>
	/// <returns>A custom mathematical formula that is used to transition between states.</returns>
	public IEasingFunction GeneratedEasingFunction { get; set; }

	internal bool IsDefault
	{
		get
		{
			if (From == null)
			{
				return To == null;
			}
			return false;
		}
	}

	internal bool DynamicStoryboardCompleted { get; set; }

	internal bool ExplicitStoryboardCompleted { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.VisualTransition" /> class. </summary>
	public VisualTransition()
	{
		DynamicStoryboardCompleted = true;
		ExplicitStoryboardCompleted = true;
	}
}
