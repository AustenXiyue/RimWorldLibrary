using System.Text;

namespace MS.Internal.Text;

internal static class InternalEncoding
{
	static InternalEncoding()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	internal static Encoding GetEncoding(int codepage)
	{
		return Encoding.GetEncoding(codepage);
	}

	internal static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
	{
		return Encoding.Convert(srcEncoding, dstEncoding, bytes);
	}
}
