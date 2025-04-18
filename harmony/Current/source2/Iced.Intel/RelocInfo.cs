namespace Iced.Intel;

internal readonly struct RelocInfo
{
	public readonly ulong Address;

	public readonly RelocKind Kind;

	public RelocInfo(RelocKind kind, ulong address)
	{
		Kind = kind;
		Address = address;
	}
}
