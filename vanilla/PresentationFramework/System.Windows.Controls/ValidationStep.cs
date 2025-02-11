namespace System.Windows.Controls;

/// <summary>Specifies when a <see cref="T:System.Windows.Controls.ValidationRule" /> runs.</summary>
public enum ValidationStep
{
	/// <summary>Runs the <see cref="T:System.Windows.Controls.ValidationRule" /> before any conversion occurs.</summary>
	RawProposedValue,
	/// <summary>Runs the <see cref="T:System.Windows.Controls.ValidationRule" /> after the value is converted.</summary>
	ConvertedProposedValue,
	/// <summary>Runs the <see cref="T:System.Windows.Controls.ValidationRule" /> after the source is updated.</summary>
	UpdatedValue,
	/// <summary>Runs the <see cref="T:System.Windows.Controls.ValidationRule" /> after the value has been committed to the source.</summary>
	CommittedValue
}
