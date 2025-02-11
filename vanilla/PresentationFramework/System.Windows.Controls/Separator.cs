using System.Windows.Automation.Peers;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary> Control that is used to separate items in items controls. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class Separator : Control
{
	private static DependencyObjectType _dType;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static Separator()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Separator), new FrameworkPropertyMetadata(typeof(Separator)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Separator));
		UIElement.IsEnabledProperty.OverrideMetadata(typeof(Separator), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ControlsTraceLogger.AddControl(TelemetryControls.Separator);
	}

	internal static void PrepareContainer(Control container)
	{
		if (container != null)
		{
			container.IsEnabled = false;
			container.HorizontalContentAlignment = HorizontalAlignment.Stretch;
		}
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.SeparatorAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new SeparatorAutomationPeer(this);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Controls.Separator" /> class. </summary>
	public Separator()
	{
	}
}
