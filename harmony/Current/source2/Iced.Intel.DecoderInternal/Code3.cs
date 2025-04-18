namespace Iced.Intel.DecoderInternal;

internal struct Code3
{
	public unsafe fixed ushort codes[3];

	public unsafe Code3(Code code16, Code code32, Code code64)
	{
		codes[0] = (ushort)code16;
		codes[1] = (ushort)code32;
		codes[2] = (ushort)code64;
	}
}
