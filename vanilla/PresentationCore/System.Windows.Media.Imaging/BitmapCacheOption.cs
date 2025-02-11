namespace System.Windows.Media.Imaging;

/// <summary>Specifies how a bitmap image takes advantage of memory caching.</summary>
public enum BitmapCacheOption
{
	/// <summary>Caches the entire image into memory. This is the default value.</summary>
	Default = 0,
	/// <summary>Creates a memory store for requested data only. The first request loads the image directly; subsequent requests are filled from the cache.</summary>
	OnDemand = 0,
	/// <summary>Caches the entire image into memory at load time. All requests for image data are filled from the memory store.</summary>
	OnLoad = 1,
	/// <summary>Do not create a memory store. All requests for the image are filled directly by the image file.</summary>
	None = 2
}
