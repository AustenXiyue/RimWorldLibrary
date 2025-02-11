namespace System.Windows.Input;

/// <summary>Contains a <see cref="T:System.Windows.Input.StylusPointProperty" /> for each property that the WPF supports.</summary>
public static class StylusPointProperties
{
	/// <summary>Represents the x-coordinate in the tablet coordinate space.</summary>
	public static readonly StylusPointProperty X = new StylusPointProperty(StylusPointPropertyIds.X, isButton: false);

	/// <summary>Represents the y-coordinate in the tablet coordinate space.</summary>
	public static readonly StylusPointProperty Y = new StylusPointProperty(StylusPointPropertyIds.Y, isButton: false);

	/// <summary>Represents the z-coordinate or distance of the pen tip from the tablet surface.</summary>
	public static readonly StylusPointProperty Z = new StylusPointProperty(StylusPointPropertyIds.Z, isButton: false);

	/// <summary>Represents the width of the contact point on the digitizer.</summary>
	public static readonly StylusPointProperty Width = new StylusPointProperty(StylusPointPropertyIds.Width, isButton: false);

	/// <summary>Represents the height of the contact point on the digitizer.</summary>
	public static readonly StylusPointProperty Height = new StylusPointProperty(StylusPointPropertyIds.Height, isButton: false);

	/// <summary>Represents the point of contact that generates the <see cref="T:System.Windows.Input.StylusPoint" />, whether initiated by a finger, palm, or any other touch.</summary>
	public static readonly StylusPointProperty SystemTouch = new StylusPointProperty(StylusPointPropertyIds.SystemTouch, isButton: false);

	/// <summary>Represents the current status of the cursor.</summary>
	public static readonly StylusPointProperty PacketStatus = new StylusPointProperty(StylusPointPropertyIds.PacketStatus, isButton: false);

	/// <summary>Identifies the <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
	public static readonly StylusPointProperty SerialNumber = new StylusPointProperty(StylusPointPropertyIds.SerialNumber, isButton: false);

	/// <summary>Represents the pressure of the pen tip perpendicular to the Tablet PC surface.</summary>
	public static readonly StylusPointProperty NormalPressure = new StylusPointProperty(StylusPointPropertyIds.NormalPressure, isButton: false);

	/// <summary>Represents the pen tip pressure along the plane of the Tablet PC surface.</summary>
	public static readonly StylusPointProperty TangentPressure = new StylusPointProperty(StylusPointPropertyIds.TangentPressure, isButton: false);

	/// <summary>Represents the pressure on a pressure-sensitive button.</summary>
	public static readonly StylusPointProperty ButtonPressure = new StylusPointProperty(StylusPointPropertyIds.ButtonPressure, isButton: false);

	/// <summary>Represents the angle between the (<paramref name="y,z" />) plane and the pen and y-axis plane.</summary>
	public static readonly StylusPointProperty XTiltOrientation = new StylusPointProperty(StylusPointPropertyIds.XTiltOrientation, isButton: false);

	/// <summary>Represents the angle between the (x, z) plane and the pen and x-axis plane.</summary>
	public static readonly StylusPointProperty YTiltOrientation = new StylusPointProperty(StylusPointPropertyIds.YTiltOrientation, isButton: false);

	/// <summary>Represents the clockwise rotation of the cursor, through a full circular range around the z-axis.</summary>
	public static readonly StylusPointProperty AzimuthOrientation = new StylusPointProperty(StylusPointPropertyIds.AzimuthOrientation, isButton: false);

	/// <summary>Represents the angle between the axis of the pen and the surface of the Tablet PC.</summary>
	public static readonly StylusPointProperty AltitudeOrientation = new StylusPointProperty(StylusPointPropertyIds.AltitudeOrientation, isButton: false);

	/// <summary>Represents the clockwise rotation of the cursor around its own axis.</summary>
	public static readonly StylusPointProperty TwistOrientation = new StylusPointProperty(StylusPointPropertyIds.TwistOrientation, isButton: false);

	/// <summary>Represents whether the tip is above or below a horizontal line that is perpendicular to the writing surface.</summary>
	public static readonly StylusPointProperty PitchRotation = new StylusPointProperty(StylusPointPropertyIds.PitchRotation, isButton: false);

	/// <summary>Represents the clockwise rotation of the pen around its own axis.</summary>
	public static readonly StylusPointProperty RollRotation = new StylusPointProperty(StylusPointPropertyIds.RollRotation, isButton: false);

	/// <summary>Represents the angle of the pen to the left or right around the center of its horizontal axis when the pen is horizontal.</summary>
	public static readonly StylusPointProperty YawRotation = new StylusPointProperty(StylusPointPropertyIds.YawRotation, isButton: false);

	/// <summary>Represents the tip button of a stylus.</summary>
	public static readonly StylusPointProperty TipButton = new StylusPointProperty(StylusPointPropertyIds.TipButton, isButton: true);

	/// <summary>Represents the barrel button of a stylus.</summary>
	public static readonly StylusPointProperty BarrelButton = new StylusPointProperty(StylusPointPropertyIds.BarrelButton, isButton: true);

	/// <summary>Represents the secondary tip button of a stylus.</summary>
	public static readonly StylusPointProperty SecondaryTipButton = new StylusPointProperty(StylusPointPropertyIds.SecondaryTipButton, isButton: true);
}
