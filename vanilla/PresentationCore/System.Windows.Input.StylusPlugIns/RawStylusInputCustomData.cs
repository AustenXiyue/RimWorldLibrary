namespace System.Windows.Input.StylusPlugIns;

internal class RawStylusInputCustomData
{
	private StylusPlugIn _owner;

	private object _data;

	public object Data => _data;

	public StylusPlugIn Owner => _owner;

	public RawStylusInputCustomData(StylusPlugIn owner, object data)
	{
		_data = data;
		_owner = owner;
	}
}
