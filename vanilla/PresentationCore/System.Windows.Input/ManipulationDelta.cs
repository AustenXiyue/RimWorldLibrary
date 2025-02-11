namespace System.Windows.Input;

/// <summary>Contains transformation data that is accumulated when manipulation events occur.</summary>
public class ManipulationDelta
{
	/// <summary>Gets or sets the linear motion of the manipulation.</summary>
	/// <returns>The linear motion of the manipulation in device-independent units (1/96th inch per unit).</returns>
	public Vector Translation { get; private set; }

	/// <summary>Gets or sets the rotation of the manipulation in degrees.</summary>
	/// <returns>The rotation of the manipulation in degrees.</returns>
	public double Rotation { get; private set; }

	/// <summary>Gets or sets the amount the manipulation has resized as a multiplier.</summary>
	/// <returns>The amount the manipulation has resized.</returns>
	public Vector Scale { get; private set; }

	/// <summary>Gets or sets the amount the manipulation has resized in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The amount the manipulation has resized in device-independent units (1/96th inch per unit).</returns>
	public Vector Expansion { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ManipulationDelta" /> class. </summary>
	/// <param name="translation">The linear motion of the manipulation in device-independent units (1/96th inch per unit).</param>
	/// <param name="rotation">The rotation of the manipulation in degrees.</param>
	/// <param name="scale">The amount the manipulation has resized as a multiplier.</param>
	/// <param name="expansion">The amount the manipulation has resized in device-independent units (1/96th inch per unit).</param>
	public ManipulationDelta(Vector translation, double rotation, Vector scale, Vector expansion)
	{
		Translation = translation;
		Rotation = rotation;
		Scale = scale;
		Expansion = expansion;
	}
}
