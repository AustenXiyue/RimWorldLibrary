using System.Globalization;

namespace MS.Internal;

internal class AvTraceFormat : AvTraceDetails
{
	private string _message;

	public override string Message => _message;

	public AvTraceFormat(AvTraceDetails details, object[] args)
		: base(details.Id, details.Labels)
	{
		_message = string.Format(CultureInfo.InvariantCulture, details.Labels[0], args);
	}
}
