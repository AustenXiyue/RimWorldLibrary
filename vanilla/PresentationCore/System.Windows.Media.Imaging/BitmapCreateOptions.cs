namespace System.Windows.Media.Imaging;

/// <summary>Specifies initialization options for bitmap images.</summary>
[Flags]
public enum BitmapCreateOptions
{
	/// <summary>No <see cref="T:System.Windows.Media.Imaging.BitmapCreateOptions" /> are specified. This is the default value.</summary>
	None = 0,
	/// <summary>Ensures that the <see cref="T:System.Windows.Media.PixelFormat" /> a file is stored in is the same as it is loaded to.</summary>
	PreservePixelFormat = 1,
	/// <summary>Causes a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object to delay initialization until it is necessary. This is useful when dealing with collections of images.</summary>
	DelayCreation = 2,
	/// <summary>Causes a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> to ignore an embedded color profile.</summary>
	IgnoreColorProfile = 4,
	/// <summary>Loads images without using an existing image cache. This option should only be selected when images in a cache need to be refreshed.</summary>
	IgnoreImageCache = 8
}
