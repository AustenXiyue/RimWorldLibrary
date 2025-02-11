namespace System.Windows.Media;

/// <summary>Specifies how to draw the gradient outside a gradient brush's gradient vector or space.  </summary>
public enum GradientSpreadMethod
{
	/// <summary>Default value. The color values at the ends of the gradient vector fill the remaining space. </summary>
	Pad,
	/// <summary>The gradient is repeated in the reverse direction until the space is filled. </summary>
	Reflect,
	/// <summary>The gradient is repeated in the original direction until the space is filled. </summary>
	Repeat
}
