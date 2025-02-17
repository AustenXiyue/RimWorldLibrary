namespace Iced.Intel;

internal static class CodeExtensions
{
	internal static bool IgnoresSegment(this Code code)
	{
		if ((uint)(code - 290) <= 2u || (uint)(code - 1040) <= 3u || (uint)(code - 1047) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IgnoresIndex(this Code code)
	{
		if (code == Code.Bndldx_bnd_mib || code == Code.Bndstx_mib_bnd)
		{
			return true;
		}
		return false;
	}

	internal static bool IsTileStrideIndex(this Code code)
	{
		if ((uint)(code - 4228) <= 2u)
		{
			return true;
		}
		return false;
	}
}
