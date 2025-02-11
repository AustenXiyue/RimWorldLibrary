namespace System.Windows.Media.Effects;

/// <summary>Describes the kernel used to create the effect.</summary>
public enum KernelType
{
	/// <summary>A distributed curve that creates a smooth distribution for a blur. </summary>
	Gaussian,
	/// <summary>A simple blur created with a square distribution curve. </summary>
	Box
}
