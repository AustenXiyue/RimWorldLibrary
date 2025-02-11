namespace System.Windows.Input;

internal static class StylusPointPropertyInfoDefaults
{
	internal static readonly StylusPointPropertyInfo X = new StylusPointPropertyInfo(StylusPointProperties.X, int.MinValue, int.MaxValue, StylusPointPropertyUnit.Centimeters, 1000f);

	internal static readonly StylusPointPropertyInfo Y = new StylusPointPropertyInfo(StylusPointProperties.Y, int.MinValue, int.MaxValue, StylusPointPropertyUnit.Centimeters, 1000f);

	internal static readonly StylusPointPropertyInfo Z = new StylusPointPropertyInfo(StylusPointProperties.Z, int.MinValue, int.MaxValue, StylusPointPropertyUnit.Centimeters, 1000f);

	internal static readonly StylusPointPropertyInfo Width = new StylusPointPropertyInfo(StylusPointProperties.Width, int.MinValue, int.MaxValue, StylusPointPropertyUnit.Centimeters, 1000f);

	internal static readonly StylusPointPropertyInfo Height = new StylusPointPropertyInfo(StylusPointProperties.Height, int.MinValue, int.MaxValue, StylusPointPropertyUnit.Centimeters, 1000f);

	internal static readonly StylusPointPropertyInfo SystemTouch = new StylusPointPropertyInfo(StylusPointProperties.SystemTouch, 0, 1, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo PacketStatus = new StylusPointPropertyInfo(StylusPointProperties.PacketStatus, int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo SerialNumber = new StylusPointPropertyInfo(StylusPointProperties.SerialNumber, int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo NormalPressure = new StylusPointPropertyInfo(StylusPointProperties.NormalPressure, 0, 1023, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo TangentPressure = new StylusPointPropertyInfo(StylusPointProperties.TangentPressure, 0, 1023, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo ButtonPressure = new StylusPointPropertyInfo(StylusPointProperties.ButtonPressure, 0, 1023, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo XTiltOrientation = new StylusPointPropertyInfo(StylusPointProperties.XTiltOrientation, 0, 3600, StylusPointPropertyUnit.Degrees, 10f);

	internal static readonly StylusPointPropertyInfo YTiltOrientation = new StylusPointPropertyInfo(StylusPointProperties.YTiltOrientation, 0, 3600, StylusPointPropertyUnit.Degrees, 10f);

	internal static readonly StylusPointPropertyInfo AzimuthOrientation = new StylusPointPropertyInfo(StylusPointProperties.AzimuthOrientation, 0, 3600, StylusPointPropertyUnit.Degrees, 10f);

	internal static readonly StylusPointPropertyInfo AltitudeOrientation = new StylusPointPropertyInfo(StylusPointProperties.AltitudeOrientation, -900, 900, StylusPointPropertyUnit.Degrees, 10f);

	internal static readonly StylusPointPropertyInfo TwistOrientation = new StylusPointPropertyInfo(StylusPointProperties.TwistOrientation, 0, 3600, StylusPointPropertyUnit.Degrees, 10f);

	internal static readonly StylusPointPropertyInfo PitchRotation = new StylusPointPropertyInfo(StylusPointProperties.PitchRotation, int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo RollRotation = new StylusPointPropertyInfo(StylusPointProperties.RollRotation, int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo YawRotation = new StylusPointPropertyInfo(StylusPointProperties.YawRotation, int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo TipButton = new StylusPointPropertyInfo(StylusPointProperties.TipButton, 0, 1, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo BarrelButton = new StylusPointPropertyInfo(StylusPointProperties.BarrelButton, 0, 1, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo SecondaryTipButton = new StylusPointPropertyInfo(StylusPointProperties.SecondaryTipButton, 0, 1, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo DefaultValue = new StylusPointPropertyInfo(new StylusPointProperty(Guid.NewGuid(), isButton: false), int.MinValue, int.MaxValue, StylusPointPropertyUnit.None, 1f);

	internal static readonly StylusPointPropertyInfo DefaultButton = new StylusPointPropertyInfo(new StylusPointProperty(Guid.NewGuid(), isButton: true), 0, 1, StylusPointPropertyUnit.None, 1f);

	internal static StylusPointPropertyInfo GetStylusPointPropertyInfoDefault(StylusPointProperty stylusPointProperty)
	{
		if (stylusPointProperty.Id == StylusPointPropertyIds.X)
		{
			return X;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Y)
		{
			return Y;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Z)
		{
			return Z;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Width)
		{
			return Width;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.Height)
		{
			return Height;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.SystemTouch)
		{
			return SystemTouch;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.PacketStatus)
		{
			return PacketStatus;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.SerialNumber)
		{
			return SerialNumber;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.NormalPressure)
		{
			return NormalPressure;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.TangentPressure)
		{
			return TangentPressure;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.ButtonPressure)
		{
			return ButtonPressure;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.XTiltOrientation)
		{
			return XTiltOrientation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.YTiltOrientation)
		{
			return YTiltOrientation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.AzimuthOrientation)
		{
			return AzimuthOrientation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.AltitudeOrientation)
		{
			return AltitudeOrientation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.TwistOrientation)
		{
			return TwistOrientation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.PitchRotation)
		{
			return PitchRotation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.RollRotation)
		{
			return RollRotation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.YawRotation)
		{
			return YawRotation;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.TipButton)
		{
			return TipButton;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.BarrelButton)
		{
			return BarrelButton;
		}
		if (stylusPointProperty.Id == StylusPointPropertyIds.SecondaryTipButton)
		{
			return SecondaryTipButton;
		}
		if (stylusPointProperty.IsButton)
		{
			return DefaultButton;
		}
		return DefaultValue;
	}
}
