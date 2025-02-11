namespace MS.Internal.Ink.InkSerializedFormat;

internal struct GorillaAlgoByte
{
	public uint BitCount;

	public uint PadCount;

	public GorillaAlgoByte(uint bitCount, uint padCount)
	{
		BitCount = bitCount;
		PadCount = padCount;
	}
}
