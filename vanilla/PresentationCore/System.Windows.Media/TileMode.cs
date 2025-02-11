namespace System.Windows.Media;

/// <summary>Describes how a <see cref="T:System.Windows.Media.TileBrush" /> paints tiles onto an output area. </summary>
public enum TileMode
{
	/// <summary>The base tile is drawn but not repeated. The remaining area is transparent </summary>
	None = 0,
	/// <summary>The base tile is drawn and the remaining area is filled by repeating the base tile. The right edge of one tile meets the left edge of the next, and similarly for the bottom and top edges.</summary>
	Tile = 4,
	/// <summary>The same as <see cref="F:System.Windows.Media.TileMode.Tile" /> except that alternate columns of tiles are flipped horizontally. The base tile itself is not flipped. </summary>
	FlipX = 1,
	/// <summary>The same as <see cref="F:System.Windows.Media.TileMode.Tile" /> except that alternate rows of tiles are flipped vertically. The base tile itself is not flipped. </summary>
	FlipY = 2,
	/// <summary>The combination of <see cref="F:System.Windows.Media.TileMode.FlipX" /> and <see cref="F:System.Windows.Media.TileMode.FlipY" />. The base tile itself is not flipped. </summary>
	FlipXY = 3
}
