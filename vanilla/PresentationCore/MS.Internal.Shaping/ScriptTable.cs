namespace MS.Internal.Shaping;

internal struct ScriptTable
{
	private const int offsetDefaultLangSys = 0;

	private const int offsetLangSysCount = 2;

	private const int offsetLangSysRecordArray = 4;

	private const int sizeLangSysRecord = 6;

	private const int offsetLangSysRecordTag = 0;

	private const int offsetLangSysRecordOffset = 4;

	private int offset;

	public bool IsNull => offset == int.MaxValue;

	public LangSysTable FindLangSys(FontTable Table, uint Tag)
	{
		if (IsNull)
		{
			return new LangSysTable(int.MaxValue);
		}
		if (Tag == 1684434036)
		{
			if (IsDefaultLangSysExists(Table))
			{
				return new LangSysTable(offset + Table.GetOffset(offset));
			}
			return new LangSysTable(int.MaxValue);
		}
		for (ushort num = 0; num < GetLangSysCount(Table); num++)
		{
			if (GetLangSysTag(Table, num) == Tag)
			{
				return GetLangSysTable(Table, num);
			}
		}
		return new LangSysTable(int.MaxValue);
	}

	public bool IsDefaultLangSysExists(FontTable Table)
	{
		return Table.GetOffset(offset) != 0;
	}

	public LangSysTable GetDefaultLangSysTable(FontTable Table)
	{
		if (IsDefaultLangSysExists(Table))
		{
			return new LangSysTable(offset + Table.GetOffset(offset));
		}
		return new LangSysTable(int.MaxValue);
	}

	public ushort GetLangSysCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	public uint GetLangSysTag(FontTable Table, ushort Index)
	{
		return Table.GetUInt(offset + 4 + Index * 6);
	}

	public LangSysTable GetLangSysTable(FontTable Table, ushort Index)
	{
		return new LangSysTable(offset + Table.GetOffset(offset + 4 + Index * 6 + 4));
	}

	public ScriptTable(int Offset)
	{
		offset = Offset;
	}
}
