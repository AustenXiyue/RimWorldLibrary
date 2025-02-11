namespace System.Windows.Media;

/// <summary>Specifies which algorithm is used to scale bitmap images.</summary>
public enum BitmapScalingMode
{
	/// <summary>Use the default bitmap scaling mode, which is <see cref="F:System.Windows.Media.BitmapScalingMode.Linear" />.</summary>
	Unspecified = 0,
	/// <summary>Use bilinear bitmap scaling, which is faster than <see cref="F:System.Windows.Media.BitmapScalingMode.HighQuality" /> mode, but produces lower quality output. The <see cref="F:System.Windows.Media.BitmapScalingMode.LowQuality" /> mode is the same as the <see cref="F:System.Windows.Media.BitmapScalingMode.Linear" /> mode.</summary>
	LowQuality = 1,
	/// <summary>Use high quality bitmap scaling, which is slower than <see cref="F:System.Windows.Media.BitmapScalingMode.LowQuality" /> mode, but produces higher quality output. The <see cref="F:System.Windows.Media.BitmapScalingMode.HighQuality" /> mode is the same as the <see cref="F:System.Windows.Media.BitmapScalingMode.Fant" /> mode.</summary>
	HighQuality = 2,
	/// <summary>Use linear bitmap scaling, which is faster than <see cref="F:System.Windows.Media.BitmapScalingMode.HighQuality" /> mode, but produces lower quality output.</summary>
	Linear = 1,
	/// <summary>Use very high quality Fant bitmap scaling, which is slower than all other bitmap scaling modes, but produces higher quality output.</summary>
	Fant = 2,
	/// <summary>Use nearest-neighbor bitmap scaling, which provides performance benefits over <see cref="F:System.Windows.Media.BitmapScalingMode.LowQuality" /> mode when the software rasterizer is used. This mode is often used to magnify a bitmap. </summary>
	NearestNeighbor = 3
}
