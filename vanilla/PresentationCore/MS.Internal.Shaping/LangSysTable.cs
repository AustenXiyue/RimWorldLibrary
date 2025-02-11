namespace MS.Internal.Shaping;

internal struct LangSysTable
{
	private const int offsetRequiredFeature = 2;

	private const int offsetFeatureCount = 4;

	private const int offsetFeatureIndexArray = 6;

	private const int sizeFeatureIndex = 2;

	private int offset;

	public bool IsNull => offset == int.MaxValue;

	public FeatureTable FindFeature(FontTable Table, FeatureList Features, uint FeatureTag)
	{
		ushort num = FeatureCount(Table);
		for (ushort num2 = 0; num2 < num; num2++)
		{
			ushort featureIndex = GetFeatureIndex(Table, num2);
			if (Features.FeatureTag(Table, featureIndex) == FeatureTag)
			{
				return Features.FeatureTable(Table, featureIndex);
			}
		}
		return new FeatureTable(int.MaxValue);
	}

	public FeatureTable RequiredFeature(FontTable Table, FeatureList Features)
	{
		ushort uShort = Table.GetUShort(offset + 2);
		if (uShort != ushort.MaxValue)
		{
			return Features.FeatureTable(Table, uShort);
		}
		return new FeatureTable(int.MaxValue);
	}

	public ushort FeatureCount(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	public ushort GetFeatureIndex(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 6 + Index * 2);
	}

	public LangSysTable(int Offset)
	{
		offset = Offset;
	}
}
