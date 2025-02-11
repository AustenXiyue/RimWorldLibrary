using System.Windows.Automation.Peers;
using System.Windows.Input;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that creates a container that has a border and a header for user interface (UI) content.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class GroupBox : HeaderedContentControl
{
	static GroupBox()
	{
		UIElement.FocusableProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(false));
		Control.IsTabStopProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(false));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupBox), new FrameworkPropertyMetadata(typeof(GroupBox)));
		EventManager.RegisterClassHandler(typeof(GroupBox), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		ControlsTraceLogger.AddControl(TelemetryControls.GroupBox);
	}

	/// <summary>Creates an implementation of <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.GroupBox" /> control.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.GroupBoxAutomationPeer" /> for the <see cref="T:System.Windows.Controls.GroupBox" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new GroupBoxAutomationPeer(this);
	}

	/// <summary>Responds when the <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for the <see cref="T:System.Windows.Controls.GroupBox" /> is pressed.</summary>
	/// <param name="e">The event information.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		if (!e.Handled && e.Scope == null && e.Target == null)
		{
			e.Target = sender as GroupBox;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GroupBox" /> class.</summary>
	public GroupBox()
	{
	}
}
