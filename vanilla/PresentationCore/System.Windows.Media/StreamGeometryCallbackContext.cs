namespace System.Windows.Media;

internal class StreamGeometryCallbackContext : ByteStreamGeometryContext
{
	private StreamGeometry _owner;

	internal StreamGeometryCallbackContext(StreamGeometry owner)
	{
		_owner = owner;
	}

	protected override void CloseCore(byte[] data)
	{
		_owner.Close(data);
	}
}
