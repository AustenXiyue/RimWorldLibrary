namespace System.Xml;

internal abstract class IncrementalReadDecoder
{
	internal abstract int DecodedCount { get; }

	internal abstract bool IsFull { get; }

	internal abstract void SetNextOutputBuffer(Array array, int offset, int len);

	internal abstract int Decode(char[] chars, int startPos, int len);

	internal abstract int Decode(string str, int startPos, int len);

	internal abstract void Reset();
}
