namespace System.IO.Packaging;

internal class TransformInitializationEventArgs : EventArgs
{
	private IDataTransform dataInstance;

	private string dataSpaceLabel;

	private string streamPath;

	private string transformLabel;

	internal IDataTransform DataTransform => dataInstance;

	internal string DataSpaceLabel => dataSpaceLabel;

	internal string Path => streamPath;

	internal string TransformInstanceLabel => transformLabel;

	internal TransformInitializationEventArgs(IDataTransform instance, string dataSpaceInstanceLabel, string transformedStreamPath, string transformInstanceLabel)
	{
		dataInstance = instance;
		dataSpaceLabel = dataSpaceInstanceLabel;
		streamPath = transformedStreamPath;
		transformLabel = transformInstanceLabel;
	}
}
