namespace System.Windows.Input;

/// <summary>Specifies how manipulation events are interpreted.</summary>
[Flags]
public enum ManipulationModes
{
	/// <summary>Manipulation events do not occur.</summary>
	None = 0,
	/// <summary>A manipulation can translate an object horizontally.</summary>
	TranslateX = 1,
	/// <summary>A manipulation can translate an object vertically.</summary>
	TranslateY = 2,
	/// <summary>A manipulation can translate an object.</summary>
	Translate = 3,
	/// <summary>A manipulation can rotate an object.</summary>
	Rotate = 4,
	/// <summary>A manipulation can scale an object.</summary>
	Scale = 8,
	/// <summary>A manipulation can scale, translate, or rotate an object and can occur with one point of input.</summary>
	All = 0xF
}
