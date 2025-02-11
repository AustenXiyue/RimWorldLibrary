using System.Collections;
using System.IO;
using System.IO.Packaging;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class CompressionTransform : IDataTransform
{
	private TransformEnvironment _transformEnvironment;

	private VersionedStreamOwner _versionedStreamOwner;

	private static readonly string _featureName = "Microsoft.Metadata.CompressionTransform";

	private static readonly VersionPair _currentFeatureVersion = new VersionPair(1, 0);

	private static readonly VersionPair _minimumReaderVersion = new VersionPair(1, 0);

	private static readonly VersionPair _minimumUpdaterVersion = new VersionPair(1, 0);

	private const long _lowWaterMark = 102400L;

	private const long _highWaterMark = 10485760L;

	public bool IsReady => true;

	public bool FixedSettings => true;

	public object TransformIdentifier => ClassTransformIdentifier;

	internal static string ClassTransformIdentifier => "{86DE7F2B-DDCE-486d-B016-405BBE82B8BC}";

	Stream IDataTransform.GetTransformedStream(Stream encodedStream, IDictionary transformContext)
	{
		Stream tempStream = new SparseMemoryStream(102400L, 10485760L);
		tempStream = new CompressEmulationStream(encodedStream, tempStream, 0L, new CompoundFileDeflateTransform());
		return new VersionedStream(tempStream, _versionedStreamOwner);
	}

	public CompressionTransform(TransformEnvironment myEnvironment)
	{
		_transformEnvironment = myEnvironment;
		_versionedStreamOwner = new VersionedStreamOwner(_transformEnvironment.GetPrimaryInstanceData(), new FormatVersion(_featureName, _minimumReaderVersion, _minimumUpdaterVersion, _currentFeatureVersion));
	}
}
