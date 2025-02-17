namespace Iced.Intel;

internal static class InstructionInfoExtensions
{
	public static Code NegateConditionCode(this Code code)
	{
		uint num;
		if ((num = (uint)(code - 1854)) <= 47 || (num = (uint)(code - 159)) <= 47 || (num = (uint)(code - 1169)) <= 47)
		{
			if (((num / 3) & 1) != 0)
			{
				return code - 3;
			}
			return code + 3;
		}
		num = (uint)(code - 1902);
		if (num <= 15)
		{
			return (Code)((num ^ 1) + 1902);
		}
		num = (uint)(code - 657);
		if (num <= 13)
		{
			return (Code)(657 + (num + 7) % 14);
		}
		return code;
	}

	public static Code ToShortBranch(this Code code)
	{
		uint num = (uint)(code - 1854);
		if (num <= 47)
		{
			return (Code)(num + 159);
		}
		num = (uint)(code - 694);
		if (num <= 2)
		{
			return (Code)(num + 699);
		}
		return code;
	}

	public static Code ToNearBranch(this Code code)
	{
		uint num = (uint)(code - 159);
		if (num <= 47)
		{
			return (Code)(num + 1854);
		}
		num = (uint)(code - 699);
		if (num <= 2)
		{
			return (Code)(num + 694);
		}
		return code;
	}
}
