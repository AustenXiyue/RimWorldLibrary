namespace System.Windows.Media.Effects;

/// <summary>Indicates the way <see cref="T:System.Windows.Media.Brush" />-valued dependency properties are sampled in a custom shader effect. </summary>
public enum SamplingMode
{
	/// <summary>Use nearest neighbor sampling.</summary>
	NearestNeighbor,
	/// <summary>Use bilinear sampling.</summary>
	Bilinear,
	/// <summary>The system selects the most appropriate sampling mode. </summary>
	Auto
}
