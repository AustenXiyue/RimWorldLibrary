using System.Collections;

namespace System.IO.Packaging;

internal interface IDataTransform
{
	bool IsReady { get; }

	bool FixedSettings { get; }

	object TransformIdentifier { get; }

	Stream GetTransformedStream(Stream encodedDataStream, IDictionary transformContext);
}
