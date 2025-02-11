namespace System.Windows.Media;

/// <summary>Specifies whether to cache tiled brush objects.</summary>
public enum CachingHint
{
	/// <summary>No caching hints are specified.</summary>
	Unspecified,
	/// <summary>Cache the tiled brush objects in an off-screen buffer, using the caching hints specified by the <see cref="T:System.Windows.Media.RenderOptions" /> settings.</summary>
	Cache
}
