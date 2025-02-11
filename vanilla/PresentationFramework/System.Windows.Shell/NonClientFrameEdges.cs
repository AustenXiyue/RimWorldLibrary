namespace System.Windows.Shell;

/// <summary>Specifies constants that indicate which edges of the window frame are not owned by the client.</summary>
[Flags]
public enum NonClientFrameEdges
{
	/// <summary>All edges are owned by the client (value = 0).</summary>
	None = 0,
	/// <summary>The left edge is not owned by the client (value = 1).</summary>
	Left = 1,
	/// <summary>The top edge is not owned by the client (value = 2).</summary>
	Top = 2,
	/// <summary>The right edge is not owned by the client (value = 4).</summary>
	Right = 4,
	/// <summary>The bottom edge is not owned by the client (value = 8).</summary>
	Bottom = 8
}
