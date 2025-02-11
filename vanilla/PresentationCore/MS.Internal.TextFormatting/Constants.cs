namespace MS.Internal.TextFormatting;

internal static class Constants
{
	public const double GreatestMutiplierOfEm = 100.0;

	public const double DefaultRealToIdeal = 300.0;

	public const double DefaultIdealToReal = 1.0 / 300.0;

	public const int IdealInfiniteWidth = 1073741822;

	public const double RealInfiniteWidth = 3579139.4066666667;

	public const double MinInterWordCompressionPerEm = 0.2;

	public const double MaxInterWordExpansionPerEm = 0.5;

	public const int AcceptableLineStretchability = 2;

	public const int MinCchToCacheBeforeAndAfter = 16;
}
