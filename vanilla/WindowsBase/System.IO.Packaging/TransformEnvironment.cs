using MS.Internal.WindowsBase;

namespace System.IO.Packaging;

internal class TransformEnvironment
{
	private DataSpaceManager transformHost;

	private string transformLabel;

	internal bool RequireOtherInstanceData
	{
		get
		{
			return false;
		}
		set
		{
			transformHost.CheckDisposedStatus();
			throw new NotSupportedException(SR.NYIDefault);
		}
	}

	internal bool RequireInstanceDataUnaltered
	{
		get
		{
			return false;
		}
		set
		{
			transformHost.CheckDisposedStatus();
			throw new NotSupportedException(SR.NYIDefault);
		}
	}

	internal bool DefaultInstanceDataTransform
	{
		get
		{
			return false;
		}
		set
		{
			transformHost.CheckDisposedStatus();
			throw new NotSupportedException(SR.NYIDefault);
		}
	}

	internal string TransformLabel => transformLabel;

	internal TransformEnvironment(DataSpaceManager host, string instanceLabel)
	{
		transformHost = host;
		transformLabel = instanceLabel;
	}

	internal Stream GetPrimaryInstanceData()
	{
		transformHost.CheckDisposedStatus();
		return transformHost.GetPrimaryInstanceStreamOf(transformLabel);
	}

	internal StorageInfo GetInstanceDataStorage()
	{
		transformHost.CheckDisposedStatus();
		StorageInfo instanceDataStorageOf = transformHost.GetInstanceDataStorageOf(transformLabel);
		if (!instanceDataStorageOf.Exists)
		{
			instanceDataStorageOf.Create();
		}
		return instanceDataStorageOf;
	}
}
