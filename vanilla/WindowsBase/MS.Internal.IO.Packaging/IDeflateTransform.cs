using System.IO;

namespace MS.Internal.IO.Packaging;

internal interface IDeflateTransform
{
	void Decompress(Stream source, Stream sink);

	void Compress(Stream source, Stream sink);
}
