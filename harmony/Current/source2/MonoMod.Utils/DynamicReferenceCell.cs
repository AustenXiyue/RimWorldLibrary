namespace MonoMod.Utils;

internal record struct DynamicReferenceCell
{
	public int Index { get; internal set; }

	public int Hash { get; internal set; }

	public DynamicReferenceCell(int idx, int hash)
	{
		Index = idx;
		Hash = hash;
	}
}
