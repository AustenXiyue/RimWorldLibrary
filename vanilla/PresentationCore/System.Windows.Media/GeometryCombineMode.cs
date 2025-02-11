namespace System.Windows.Media;

/// <summary>Specifies the different methods by which two geometries can be combined.  </summary>
public enum GeometryCombineMode
{
	/// <summary>The two regions are combined by taking the union of both.  The resulting geometry is geometry <paramref name="A" /> + geometry <paramref name="B" />.</summary>
	Union,
	/// <summary>The two regions are combined by taking their intersection.  The new area consists of the overlapping region between the two geometries.  </summary>
	Intersect,
	/// <summary>The two regions are combined by taking the area that exists in the first region but not the second and the area that exists in the second region but not the first.  The new region consists of <paramref name="(A-B)" /> + <paramref name="(B-A)" />, where <paramref name="A" /> and <paramref name="B" /> are geometries.  </summary>
	Xor,
	/// <summary>The second region is excluded from the first.  Given two geometries, <paramref name="A" /> and <paramref name="B" />, the area of geometry <paramref name="B" /> is removed from the area of geometry <paramref name="A" />, producing a region that is <paramref name="A-B" />.</summary>
	Exclude
}
