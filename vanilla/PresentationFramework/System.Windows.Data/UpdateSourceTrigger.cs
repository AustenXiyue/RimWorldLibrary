namespace System.Windows.Data;

/// <summary>Describes the timing of binding source updates.</summary>
public enum UpdateSourceTrigger
{
	/// <summary>The default <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> value of the binding target property. The default value for most dependency properties is <see cref="F:System.Windows.Data.UpdateSourceTrigger.PropertyChanged" />, while the <see cref="P:System.Windows.Controls.TextBox.Text" /> property has a default value of <see cref="F:System.Windows.Data.UpdateSourceTrigger.LostFocus" />.</summary>
	Default,
	/// <summary>Updates the binding source immediately whenever the binding target property changes.</summary>
	PropertyChanged,
	/// <summary>Updates the binding source whenever the binding target element loses focus.</summary>
	LostFocus,
	/// <summary>Updates the binding source only when you call the <see cref="M:System.Windows.Data.BindingExpression.UpdateSource" /> method.</summary>
	Explicit
}
