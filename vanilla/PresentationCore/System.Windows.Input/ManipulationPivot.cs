namespace System.Windows.Input;

/// <summary>Specifies how a rotation occurs with one point of user input.</summary>
public class ManipulationPivot
{
	/// <summary>Gets or sets the center of a single-point manipulation.</summary>
	/// <returns>The center of a single-point manipulation.</returns>
	public Point Center { get; set; }

	/// <summary>Gets or sets the area around the pivot that is used to determine how much rotation and translation occurs when a single point of contact initiates the manipulation.</summary>
	/// <returns>The area around the pivot that is used to determine how much rotation and translation occurs when a single point of contact initiates the manipulation.</returns>
	public double Radius { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ManipulationPivot" /> class. </summary>
	public ManipulationPivot()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.ManipulationPivot" /> class with the specified point of a single-point manipulation. </summary>
	/// <param name="center">The center of a single-point manipulation.</param>
	/// <param name="radius">The area around the pivot that is used to determine how much rotation and translation occurs when a single point of contact initiates the manipulation.</param>
	public ManipulationPivot(Point center, double radius)
	{
		Center = center;
		Radius = radius;
	}
}
