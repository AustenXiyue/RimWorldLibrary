using System.Collections.Generic;

namespace System.Windows.Input;

internal static class StylusPointPropertyIds
{
	internal enum HidUsagePage
	{
		Undefined = 0,
		Generic = 1,
		Simulation = 2,
		Vr = 3,
		Sport = 4,
		Game = 5,
		Keyboard = 7,
		Led = 8,
		Button = 9,
		Ordinal = 10,
		Telephony = 11,
		Consumer = 12,
		Digitizer = 13,
		Unicode = 16,
		Alphanumeric = 20,
		BarcodeScanner = 140,
		WeighingDevice = 141,
		MagneticStripeReader = 142,
		CameraControl = 144,
		MicrosoftBluetoothHandsfree = 65523
	}

	internal enum HidUsage
	{
		TipPressure = 48,
		X = 48,
		BarrelPressure = 49,
		Y = 49,
		Z = 50,
		XTilt = 61,
		YTilt = 62,
		Azimuth = 63,
		Altitude = 64,
		Twist = 65,
		TipSwitch = 66,
		SecondaryTipSwitch = 67,
		BarrelSwitch = 68,
		TouchConfidence = 71,
		Width = 72,
		Height = 73,
		TransducerSerialNumber = 91
	}

	public static readonly Guid X = new Guid(1502243471, 21184, 19360, 147, 175, 175, 53, 116, 17, 165, 97);

	public static readonly Guid Y = new Guid(3040845685u, 1248, 17560, 167, 238, 195, 13, 187, 90, 144, 17);

	public static readonly Guid Z = new Guid(1935334192, 3771, 18312, 160, 228, 15, 49, 100, 144, 5, 93);

	public static readonly Guid Width = new Guid(3131828557u, 10002, 18677, 190, 157, 143, 139, 94, 160, 113, 26);

	public static readonly Guid Height = new Guid(3860355282u, 58439, 16920, 157, 63, 24, 134, 92, 32, 61, 244);

	public static readonly Guid SystemTouch = new Guid(3875981316u, 22512, 20224, 138, 12, 133, 61, 87, 120, 155, 233);

	public static readonly Guid PacketStatus = new Guid(1846413247u, 45031, 19703, 135, 209, 175, 100, 70, 32, 132, 24);

	public static readonly Guid SerialNumber = new Guid(2024282966, 2357, 17555, 186, 174, 0, 84, 26, 138, 22, 196);

	public static readonly Guid NormalPressure = new Guid(1929859117u, 63988, 19992, 179, 242, 44, 225, 177, 163, 97, 12);

	public static readonly Guid TangentPressure = new Guid(1839483019, 21060, 16876, 144, 91, 50, 216, 154, 184, 8, 9);

	public static readonly Guid ButtonPressure = new Guid(2340417476u, 38570, 19454, 172, 38, 138, 95, 11, 224, 123, 245);

	public static readonly Guid XTiltOrientation = new Guid(2832235322u, 35824, 16560, 149, 169, 184, 10, 107, 183, 135, 191);

	public static readonly Guid YTiltOrientation = new Guid(244523913, 7543, 17327, 172, 0, 91, 149, 13, 109, 75, 45);

	public static readonly Guid AzimuthOrientation = new Guid(43066292u, 34856, 16651, 178, 80, 160, 83, 101, 149, 229, 220);

	public static readonly Guid AltitudeOrientation = new Guid(2195637703u, 63162, 18694, 137, 79, 102, 214, 141, 252, 69, 108);

	public static readonly Guid TwistOrientation = new Guid(221399392, 5042, 16868, 172, 230, 122, 233, 212, 61, 45, 59);

	public static readonly Guid PitchRotation = new Guid(2138986423u, 48695, 19425, 163, 86, 122, 132, 22, 14, 24, 147);

	public static readonly Guid RollRotation = new Guid(1566400086, 27561, 19547, 159, 176, 133, 28, 145, 113, 78, 86);

	public static readonly Guid YawRotation = new Guid(1787074944, 31802, 17847, 170, 130, 144, 162, 98, 149, 14, 137);

	public static readonly Guid TipButton = new Guid(59851731, 30923, 17564, 168, 231, 103, 209, 136, 100, 195, 50);

	public static readonly Guid BarrelButton = new Guid(4034003752u, 26171, 16783, 133, 166, 149, 49, 174, 62, 205, 250);

	public static readonly Guid SecondaryTipButton = new Guid(1735669634, 3813, 16794, 161, 43, 39, 58, 158, 192, 143, 61);

	private static Dictionary<HidUsagePage, Dictionary<HidUsage, Guid>> _hidToGuidMap = new Dictionary<HidUsagePage, Dictionary<HidUsage, Guid>>
	{
		{
			HidUsagePage.Generic,
			new Dictionary<HidUsage, Guid>
			{
				{
					HidUsage.TipPressure,
					X
				},
				{
					HidUsage.BarrelPressure,
					Y
				},
				{
					HidUsage.Z,
					Z
				}
			}
		},
		{
			HidUsagePage.Digitizer,
			new Dictionary<HidUsage, Guid>
			{
				{
					HidUsage.Width,
					Width
				},
				{
					HidUsage.Height,
					Height
				},
				{
					HidUsage.TouchConfidence,
					SystemTouch
				},
				{
					HidUsage.TipPressure,
					NormalPressure
				},
				{
					HidUsage.BarrelPressure,
					ButtonPressure
				},
				{
					HidUsage.XTilt,
					XTiltOrientation
				},
				{
					HidUsage.YTilt,
					YTiltOrientation
				},
				{
					HidUsage.Azimuth,
					AzimuthOrientation
				},
				{
					HidUsage.Altitude,
					AltitudeOrientation
				},
				{
					HidUsage.Twist,
					TwistOrientation
				},
				{
					HidUsage.TipSwitch,
					TipButton
				},
				{
					HidUsage.SecondaryTipSwitch,
					SecondaryTipButton
				},
				{
					HidUsage.BarrelSwitch,
					BarrelButton
				},
				{
					HidUsage.TransducerSerialNumber,
					SerialNumber
				}
			}
		}
	};

	internal static Guid GetKnownGuid(HidUsagePage page, HidUsage usage)
	{
		Guid value = Guid.Empty;
		Dictionary<HidUsage, Guid> value2 = null;
		if (_hidToGuidMap.TryGetValue(page, out value2))
		{
			value2.TryGetValue(usage, out value);
		}
		return value;
	}

	internal static bool IsKnownId(Guid guid)
	{
		if (guid == X || guid == Y || guid == Z || guid == Width || guid == Height || guid == SystemTouch || guid == PacketStatus || guid == SerialNumber || guid == NormalPressure || guid == TangentPressure || guid == ButtonPressure || guid == XTiltOrientation || guid == YTiltOrientation || guid == AzimuthOrientation || guid == AltitudeOrientation || guid == TwistOrientation || guid == PitchRotation || guid == RollRotation || guid == YawRotation || guid == TipButton || guid == BarrelButton || guid == SecondaryTipButton)
		{
			return true;
		}
		return false;
	}

	internal static string GetStringRepresentation(Guid guid)
	{
		if (guid == X)
		{
			return "X";
		}
		if (guid == Y)
		{
			return "Y";
		}
		if (guid == Z)
		{
			return "Z";
		}
		if (guid == Width)
		{
			return "Width";
		}
		if (guid == Height)
		{
			return "Height";
		}
		if (guid == SystemTouch)
		{
			return "SystemTouch";
		}
		if (guid == PacketStatus)
		{
			return "PacketStatus";
		}
		if (guid == SerialNumber)
		{
			return "SerialNumber";
		}
		if (guid == NormalPressure)
		{
			return "NormalPressure";
		}
		if (guid == TangentPressure)
		{
			return "TangentPressure";
		}
		if (guid == ButtonPressure)
		{
			return "ButtonPressure";
		}
		if (guid == XTiltOrientation)
		{
			return "XTiltOrientation";
		}
		if (guid == YTiltOrientation)
		{
			return "YTiltOrientation";
		}
		if (guid == AzimuthOrientation)
		{
			return "AzimuthOrientation";
		}
		if (guid == AltitudeOrientation)
		{
			return "AltitudeOrientation";
		}
		if (guid == TwistOrientation)
		{
			return "TwistOrientation";
		}
		if (guid == PitchRotation)
		{
			return "PitchRotation";
		}
		if (guid == RollRotation)
		{
			return "RollRotation";
		}
		if (guid == AltitudeOrientation)
		{
			return "AltitudeOrientation";
		}
		if (guid == YawRotation)
		{
			return "YawRotation";
		}
		if (guid == TipButton)
		{
			return "TipButton";
		}
		if (guid == BarrelButton)
		{
			return "BarrelButton";
		}
		if (guid == SecondaryTipButton)
		{
			return "SecondaryTipButton";
		}
		return "Unknown";
	}

	internal static bool IsKnownButton(Guid guid)
	{
		if (guid == TipButton || guid == BarrelButton || guid == SecondaryTipButton)
		{
			return true;
		}
		return false;
	}
}
