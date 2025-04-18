namespace Iced.Intel;

internal struct ConstantOffsets
{
	public byte DisplacementOffset;

	public byte DisplacementSize;

	public byte ImmediateOffset;

	public byte ImmediateSize;

	public byte ImmediateOffset2;

	public byte ImmediateSize2;

	private byte pad1;

	private byte pad2;

	public readonly bool HasDisplacement => DisplacementSize != 0;

	public readonly bool HasImmediate => ImmediateSize != 0;

	public readonly bool HasImmediate2 => ImmediateSize2 != 0;
}
