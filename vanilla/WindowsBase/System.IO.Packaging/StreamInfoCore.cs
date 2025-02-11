using MS.Internal.IO.Packaging.CompoundFile;

namespace System.IO.Packaging;

internal class StreamInfoCore
{
	internal string streamName;

	internal IStream safeIStream;

	internal string dataSpaceLabel;

	internal object exposedStream;

	internal StreamInfoCore(string nameStream, string label)
		: this(nameStream, label, null)
	{
	}

	internal StreamInfoCore(string nameStream, string label, IStream s)
	{
		streamName = nameStream;
		dataSpaceLabel = label;
		safeIStream = s;
		exposedStream = null;
	}
}
