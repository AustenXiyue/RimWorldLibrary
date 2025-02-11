namespace System.Windows.Controls;

/// <summary>Specifies the method the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> uses to manage virtualizing its child items.</summary>
public enum VirtualizationMode
{
	/// <summary>Create and discard the item containers.</summary>
	Standard,
	/// <summary>Reuse the item containers.</summary>
	Recycling
}
