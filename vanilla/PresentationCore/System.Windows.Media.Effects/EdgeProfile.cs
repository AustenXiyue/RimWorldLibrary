namespace System.Windows.Media.Effects;

/// <summary>Specifies the type of curve to apply to the edge of a bitmap.</summary>
public enum EdgeProfile
{
	/// <summary>An edge that is a straight line. </summary>
	Linear,
	/// <summary>A concave edge that curves in.</summary>
	CurvedIn,
	/// <summary>A convex edge that curves out.</summary>
	CurvedOut,
	/// <summary>An edge that curves up and then down, like a ridge. </summary>
	BulgedUp
}
