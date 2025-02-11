using System.Windows.Automation;
using System.Windows.Automation.Peers;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents an item of a <see cref="T:System.Windows.Controls.Primitives.StatusBar" /> control. </summary>
[Localizability(LocalizationCategory.Inherit)]
public class StatusBarItem : ContentControl
{
	private static DependencyObjectType _dType;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static StatusBarItem()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBarItem), new FrameworkPropertyMetadata(typeof(StatusBarItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(StatusBarItem));
		Control.IsTabStopProperty.OverrideMetadata(typeof(StatusBarItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(StatusBarItem), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Specifies an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.StatusBarItemAutomationPeer" /> for this <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new StatusBarItemAutomationPeer(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.StatusBarItem" /> class.</summary>
	public StatusBarItem()
	{
	}
}
