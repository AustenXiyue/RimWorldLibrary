namespace System.Xml;

internal class Ucs4Encoding2143 : Ucs4Encoding
{
	public override string EncodingName => "ucs-4 (order 2143)";

	public Ucs4Encoding2143()
	{
		ucs4Decoder = new Ucs4Decoder2143();
	}

	public override byte[] GetPreamble()
	{
		return new byte[4] { 0, 0, 255, 254 };
	}
}
