namespace System.Xml;

internal class Ucs4Encoding4321 : Ucs4Encoding
{
	public override string EncodingName => "ucs-4";

	public Ucs4Encoding4321()
	{
		ucs4Decoder = new Ucs4Decoder4321();
	}

	public override byte[] GetPreamble()
	{
		return new byte[4] { 255, 254, 0, 0 };
	}
}
