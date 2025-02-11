namespace System.Windows.Media.Effects;

/// <summary>Indicates the policy for rendering a <see cref="T:System.Windows.Media.Effects.ShaderEffect" /> in software. </summary>
public enum ShaderRenderMode
{
	/// <summary>Allow hardware and software rendering.</summary>
	Auto,
	/// <summary>Force software rendering.</summary>
	SoftwareOnly,
	/// <summary>Require hardware rendering, ignore if unavailable.</summary>
	HardwareOnly
}
