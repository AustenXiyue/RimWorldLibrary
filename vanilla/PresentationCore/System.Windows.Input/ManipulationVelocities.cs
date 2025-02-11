namespace System.Windows.Input;

/// <summary>Describes the speed at which manipulations occurs.</summary>
public class ManipulationVelocities
{
	/// <summary>Gets or sets the speed of linear motion.</summary>
	/// <returns>The speed of linear motion in device-independent units (1/96th inch per unit) per millisecond.</returns>
	public Vector LinearVelocity { get; private set; }

	/// <summary>Gets or sets the speed of rotation.</summary>
	/// <returns>The speed of rotation in degrees per millisecond.</returns>
	public double AngularVelocity { get; private set; }

	/// <summary>Gets or sets the rate at which the manipulation is resized. </summary>
	/// <returns>The rate at which the manipulation is resized in device-independent units (1/96th inch per unit) per millisecond.</returns>
	public Vector ExpansionVelocity { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ManipulationVelocities" /> class. </summary>
	/// <param name="linearVelocity">The speed of linear motion in device-independent units (1/96th inch per unit) per millisecond.</param>
	/// <param name="angularVelocity">The speed of rotation in degrees per millisecond.</param>
	/// <param name="expansionVelocity">The rate at which the manipulation is resized in device-independent units (1/96th inch per unit) per millisecond.</param>
	public ManipulationVelocities(Vector linearVelocity, double angularVelocity, Vector expansionVelocity)
	{
		LinearVelocity = linearVelocity;
		AngularVelocity = angularVelocity;
		ExpansionVelocity = expansionVelocity;
	}
}
