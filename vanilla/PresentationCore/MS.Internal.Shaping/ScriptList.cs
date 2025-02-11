namespace MS.Internal.Shaping;

internal struct ScriptList
{
	private const int offsetScriptCount = 0;

	private const int offsetScriptRecordArray = 2;

	private const int sizeScriptRecord = 6;

	private const int offsetScriptRecordTag = 0;

	private const int offsetScriptRecordOffset = 4;

	private int offset;

	public ScriptTable FindScript(FontTable Table, uint Tag)
	{
		for (ushort num = 0; num < GetScriptCount(Table); num++)
		{
			if (GetScriptTag(Table, num) == Tag)
			{
				return GetScriptTable(Table, num);
			}
		}
		return new ScriptTable(int.MaxValue);
	}

	public ushort GetScriptCount(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public uint GetScriptTag(FontTable Table, ushort Index)
	{
		return Table.GetUInt(offset + 2 + Index * 6);
	}

	public ScriptTable GetScriptTable(FontTable Table, ushort Index)
	{
		return new ScriptTable(offset + Table.GetOffset(offset + 2 + Index * 6 + 4));
	}

	public ScriptList(int Offset)
	{
		offset = Offset;
	}
}
