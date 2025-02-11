namespace MS.Internal;

internal class AvTraceDetails
{
	private int _id;

	private string[] _labels;

	public int Id => _id;

	public virtual string Message => _labels[0];

	public string[] Labels => _labels;

	public AvTraceDetails(int id, string[] labels)
	{
		_id = id;
		_labels = labels;
	}
}
