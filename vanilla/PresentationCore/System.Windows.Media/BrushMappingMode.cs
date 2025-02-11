namespace System.Windows.Media;

/// <summary>Specifies the coordinate system used by a <see cref="T:System.Windows.Media.Brush" />. </summary>
public enum BrushMappingMode
{
	/// <summary>The coordinate system is not relative to a bounding box. Values are interpreted directly in local space.  </summary>
	Absolute,
	/// <summary>The coordinate system is relative to a bounding box: 0 indicates 0 percent of the bounding box, and 1 indicates 100 percent of the bounding box. For example, (0.5, 0.5) describes a point in the middle of the bounding box, and (1, 1) describes a point at the bottom right of the bounding box. </summary>
	RelativeToBoundingBox
}
