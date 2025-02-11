using System.Globalization;
using System.Reflection;
using MS.Internal.Ink.InkSerializedFormat;

namespace System.Windows.Ink;

internal static class KnownIds
{
	internal static readonly Guid X = KnownIdCache.OriginalISFIdTable[0];

	internal static readonly Guid Y = KnownIdCache.OriginalISFIdTable[1];

	internal static readonly Guid Z = KnownIdCache.OriginalISFIdTable[2];

	internal static readonly Guid PacketStatus = KnownIdCache.OriginalISFIdTable[3];

	internal static readonly Guid TimerTick = KnownIdCache.OriginalISFIdTable[4];

	internal static readonly Guid SerialNumber = KnownIdCache.OriginalISFIdTable[5];

	internal static readonly Guid NormalPressure = KnownIdCache.OriginalISFIdTable[6];

	internal static readonly Guid TangentPressure = KnownIdCache.OriginalISFIdTable[7];

	internal static readonly Guid ButtonPressure = KnownIdCache.OriginalISFIdTable[8];

	internal static readonly Guid XTiltOrientation = KnownIdCache.OriginalISFIdTable[9];

	internal static readonly Guid YTiltOrientation = KnownIdCache.OriginalISFIdTable[10];

	internal static readonly Guid AzimuthOrientation = KnownIdCache.OriginalISFIdTable[11];

	internal static readonly Guid AltitudeOrientation = KnownIdCache.OriginalISFIdTable[12];

	internal static readonly Guid TwistOrientation = KnownIdCache.OriginalISFIdTable[13];

	internal static readonly Guid PitchRotation = KnownIdCache.OriginalISFIdTable[14];

	internal static readonly Guid RollRotation = KnownIdCache.OriginalISFIdTable[15];

	internal static readonly Guid YawRotation = KnownIdCache.OriginalISFIdTable[16];

	internal static readonly Guid Color = KnownIdCache.OriginalISFIdTable[18];

	internal static readonly Guid DrawingFlags = KnownIdCache.OriginalISFIdTable[22];

	internal static readonly Guid CursorId = KnownIdCache.OriginalISFIdTable[23];

	internal static readonly Guid WordAlternates = KnownIdCache.OriginalISFIdTable[24];

	internal static readonly Guid CharacterAlternates = KnownIdCache.OriginalISFIdTable[25];

	internal static readonly Guid InkMetrics = KnownIdCache.OriginalISFIdTable[26];

	internal static readonly Guid GuideStructure = KnownIdCache.OriginalISFIdTable[27];

	internal static readonly Guid Timestamp = KnownIdCache.OriginalISFIdTable[28];

	internal static readonly Guid Language = KnownIdCache.OriginalISFIdTable[29];

	internal static readonly Guid Transparency = KnownIdCache.OriginalISFIdTable[30];

	internal static readonly Guid CurveFittingError = KnownIdCache.OriginalISFIdTable[31];

	internal static readonly Guid RecognizedLattice = KnownIdCache.OriginalISFIdTable[32];

	internal static readonly Guid CursorDown = KnownIdCache.OriginalISFIdTable[33];

	internal static readonly Guid SecondaryTipSwitch = KnownIdCache.OriginalISFIdTable[34];

	internal static readonly Guid TabletPick = KnownIdCache.OriginalISFIdTable[36];

	internal static readonly Guid BarrelDown = KnownIdCache.OriginalISFIdTable[35];

	internal static readonly Guid RasterOperation = KnownIdCache.OriginalISFIdTable[37];

	internal static readonly Guid StylusHeight = KnownIdCache.OriginalISFIdTable[20];

	internal static readonly Guid StylusWidth = KnownIdCache.OriginalISFIdTable[19];

	internal static readonly Guid Highlighter = KnownIdCache.TabletInternalIdTable[0];

	internal static readonly Guid InkProperties = KnownIdCache.TabletInternalIdTable[1];

	internal static readonly Guid InkStyleBold = KnownIdCache.TabletInternalIdTable[2];

	internal static readonly Guid InkStyleItalics = KnownIdCache.TabletInternalIdTable[3];

	internal static readonly Guid StrokeTimestamp = KnownIdCache.TabletInternalIdTable[4];

	internal static readonly Guid StrokeTimeId = KnownIdCache.TabletInternalIdTable[5];

	internal static readonly Guid StylusTip = new Guid(891733809u, 61049, 18824, 185, 62, 112, 217, 47, 137, 7, 237);

	internal static readonly Guid StylusTipTransform = new Guid(1264827414, 31684, 20434, 149, 218, 172, byte.MaxValue, 71, 117, 115, 45);

	internal static readonly Guid IsHighlighter = new Guid(3459276314u, 3592, 17891, 140, 220, 228, 11, 180, 80, 111, 33);

	internal static readonly Guid PenStyle = KnownIdCache.OriginalISFIdTable[17];

	internal static readonly Guid PenTip = KnownIdCache.OriginalISFIdTable[21];

	internal static readonly Guid InkCustomStrokes = KnownIdCache.TabletInternalIdTable[7];

	internal static readonly Guid InkStrokeLattice = KnownIdCache.TabletInternalIdTable[6];

	private static MemberInfo[] PublicMemberInfo = null;

	internal static string ConvertToString(Guid id)
	{
		if (PublicMemberInfo == null)
		{
			PublicMemberInfo = typeof(KnownIds).FindMembers(MemberTypes.Field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null);
		}
		MemberInfo[] publicMemberInfo = PublicMemberInfo;
		foreach (MemberInfo memberInfo in publicMemberInfo)
		{
			if (id == (Guid)typeof(KnownIds).InvokeMember(memberInfo.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField, null, null, Array.Empty<object>(), CultureInfo.InvariantCulture))
			{
				return memberInfo.Name;
			}
		}
		return id.ToString();
	}
}
