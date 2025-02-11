namespace MS.Internal.Documents;

internal static class DocumentViewerConstants
{
	private const double _minimumZoom = 5.0;

	private const double _minimumThumbnailsZoom = 12.5;

	private const double _maximumZoom = 5000.0;

	private const int _maximumMaxPagesAcross = 32;

	public static double MinimumZoom => 5.0;

	public static double MaximumZoom => 5000.0;

	public static double MinimumScale => 0.05;

	public static double MinimumThumbnailsScale => 0.125;

	public static double MaximumScale => 50.0;

	public static int MaximumMaxPagesAcross => 32;
}
