using System.Buffers.Binary;

namespace System.Xml;

internal sealed class Ucs4Decoder1234 : Ucs4Decoder
{
	internal override int GetFullChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		byteCount += byteIndex;
		int i = byteIndex;
		int num = charIndex;
		for (; i + 3 < byteCount; i += 4)
		{
			uint num2 = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(i));
			if (num2 > 1114111)
			{
				throw new ArgumentException(System.SR.Format(System.SR.Enc_InvalidByteInEncoding, new object[1] { i }), (string?)null);
			}
			if (num2 > 65535)
			{
				Ucs4Decoder.Ucs4ToUTF16(num2, chars, num);
				num++;
			}
			else
			{
				if (XmlCharType.IsSurrogate((int)num2))
				{
					throw new XmlException(System.SR.Xml_InvalidCharInThisEncoding, string.Empty);
				}
				chars[num] = (char)num2;
			}
			num++;
		}
		return num - charIndex;
	}
}
