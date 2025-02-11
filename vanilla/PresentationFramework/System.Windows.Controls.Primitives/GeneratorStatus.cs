namespace System.Windows.Controls.Primitives;

/// <summary>Used by <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> to indicate the status of its item generation.</summary>
public enum GeneratorStatus
{
	/// <summary>The generator has not tried to generate content.</summary>
	NotStarted,
	/// <summary>The generator is generating containers.</summary>
	GeneratingContainers,
	/// <summary>The generator has finished generating containers.</summary>
	ContainersGenerated,
	/// <summary>The generator has finished generating containers, but encountered one or more errors.</summary>
	Error
}
