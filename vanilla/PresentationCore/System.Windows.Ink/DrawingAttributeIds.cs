using MS.Internal.Ink.InkSerializedFormat;

namespace System.Windows.Ink;

/// <summary>Contains a set of GUIDs that identify the properties in the <see cref="T:System.Windows.Ink.DrawingAttributes" /> class.</summary>
public static class DrawingAttributeIds
{
	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.Color" /> property.</summary>
	public static readonly Guid Color = KnownIdCache.OriginalISFIdTable[18];

	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.StylusTip" /> property.</summary>
	public static readonly Guid StylusTip = new Guid(891733809u, 61049, 18824, 185, 62, 112, 217, 47, 137, 7, 237);

	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.StylusTipTransform" /> property.</summary>
	public static readonly Guid StylusTipTransform = new Guid(1264827414, 31684, 20434, 149, 218, 172, byte.MaxValue, 71, 117, 115, 45);

	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.Height" /> property.</summary>
	public static readonly Guid StylusHeight = KnownIdCache.OriginalISFIdTable[20];

	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.Width" /> property.</summary>
	public static readonly Guid StylusWidth = KnownIdCache.OriginalISFIdTable[19];

	/// <summary>Identifies the internal DrawingFlags property.</summary>
	public static readonly Guid DrawingFlags = KnownIdCache.OriginalISFIdTable[22];

	/// <summary>Identifies the <see cref="P:System.Windows.Ink.DrawingAttributes.IsHighlighter" /> property.</summary>
	public static readonly Guid IsHighlighter = new Guid(3459276314u, 3592, 17891, 140, 220, 228, 11, 180, 80, 111, 33);
}
