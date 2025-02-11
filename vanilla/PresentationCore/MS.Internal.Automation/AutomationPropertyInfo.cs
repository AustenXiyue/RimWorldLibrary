using System.Windows;
using System.Windows.Automation;

namespace MS.Internal.Automation;

internal class AutomationPropertyInfo
{
	private AutomationProperty _id;

	private DependencyProperty _dependencyProperty;

	private DependencyProperty _overrideDP;

	internal AutomationProperty ID => _id;

	internal DependencyProperty DependencyProperty => _dependencyProperty;

	internal DependencyProperty OverrideDP => _overrideDP;

	internal AutomationPropertyInfo(AutomationProperty id, DependencyProperty dependencyProperty, DependencyProperty overrideDP)
	{
		_id = id;
		_dependencyProperty = dependencyProperty;
		_overrideDP = overrideDP;
	}
}
