using System.Windows.Automation;

namespace MS.Internal.Automation;

internal class AutomationPatternInfo
{
	private AutomationPattern _id;

	private WrapObject _wcpWrapper;

	internal AutomationPattern ID => _id;

	internal WrapObject WcpWrapper => _wcpWrapper;

	internal AutomationPatternInfo(AutomationPattern id, WrapObject wcpWrapper)
	{
		_id = id;
		_wcpWrapper = wcpWrapper;
	}
}
