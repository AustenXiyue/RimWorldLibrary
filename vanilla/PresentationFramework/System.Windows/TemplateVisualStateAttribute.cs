namespace System.Windows;

/// <summary>Specifies that a control can be in a certain state and that a <see cref="T:System.Windows.VisualState" /> is expected in the control's <see cref="T:System.Windows.Controls.ControlTemplate" />.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TemplateVisualStateAttribute : Attribute
{
	/// <summary>Gets or sets the name of the state that the control can be in.</summary>
	/// <returns>The name of the state that the control can be in.</returns>
	public string Name { get; set; }

	/// <summary>Gets or sets the name of the group that the state belongs to.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.VisualStateGroup" /> that the state belongs to.</returns>
	public string GroupName { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateVisualStateAttribute" /> class. </summary>
	public TemplateVisualStateAttribute()
	{
	}
}
