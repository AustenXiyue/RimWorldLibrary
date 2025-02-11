using System;

namespace MS.Internal.AppModel;

[Serializable]
internal struct SubStream
{
	internal string _propertyName;

	internal byte[] _data;

	internal SubStream(string propertyName, byte[] dataBytes)
	{
		_propertyName = propertyName;
		_data = dataBytes;
	}
}
