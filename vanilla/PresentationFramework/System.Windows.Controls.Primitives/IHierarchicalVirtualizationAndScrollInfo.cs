namespace System.Windows.Controls.Primitives;

/// <summary>Provides properties through which a control that displays hierarchical data communicates with a <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</summary>
public interface IHierarchicalVirtualizationAndScrollInfo
{
	/// <summary>Gets or sets an object that represents the sizes of the control's viewport and cache.</summary>
	/// <returns>An object that represents the sizes of the control's viewport and cache.</returns>
	HierarchicalVirtualizationConstraints Constraints { get; set; }

	/// <summary>Gets an object that represents the desired size of the control's header, in device-independent units (1/96th inch per unit) and in logical units.</summary>
	/// <returns>An object that represents the desired size of the control's header, in device-independent units (1/96th inch per unit) and in logical units.</returns>
	HierarchicalVirtualizationHeaderDesiredSizes HeaderDesiredSizes { get; }

	/// <summary>Gets or sets an object that represents the desired size of the control's items, in device-independent units (1/96th inch per unit) and in logical units.</summary>
	/// <returns>An object that represents the desired size of the control's items, in device-independent units (1/96th inch per unit) and in logical units.</returns>
	HierarchicalVirtualizationItemDesiredSizes ItemDesiredSizes { get; set; }

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Panel" /> that displays the items of the control.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Panel" /> that displays the items of the control.</returns>
	Panel ItemsHost { get; }

	/// <summary>Gets or sets a value that indicates whether the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items.</summary>
	/// <returns>true if the owning <see cref="T:System.Windows.Controls.ItemsControl" /> should virtualize its items; otherwise, false.</returns>
	bool MustDisableVirtualization { get; set; }

	/// <summary>Gets a value that indicates whether the control's layout pass occurs at a lower priority.</summary>
	/// <returns>true if the control's layout pass occurs at a lower priority; otherwise, false.</returns>
	bool InBackgroundLayout { get; set; }
}
