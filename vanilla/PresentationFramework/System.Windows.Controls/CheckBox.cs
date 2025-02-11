using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that a user can select and clear.   </summary>
[DefaultEvent("CheckStateChanged")]
[Localizability(LocalizationCategory.CheckBox)]
public class CheckBox : ToggleButton
{
	private static DependencyObjectType _dType;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static CheckBox()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBox), new FrameworkPropertyMetadata(typeof(CheckBox)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(CheckBox));
		KeyboardNavigation.AcceptsReturnProperty.OverrideMetadata(typeof(CheckBox), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ControlsTraceLogger.AddControl(TelemetryControls.CheckBox);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CheckBox" /> class.</summary>
	public CheckBox()
	{
	}

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.CheckBox" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.CheckBox" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new CheckBoxAutomationPeer(this);
	}

	/// <summary> Responds to a <see cref="T:System.Windows.Controls.CheckBox" /><see cref="E:System.Windows.UIElement.KeyDown" /> event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data. </param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (!base.IsThreeState)
		{
			if (e.Key == Key.OemPlus || e.Key == Key.Add)
			{
				e.Handled = true;
				ClearValue(ButtonBase.IsPressedPropertyKey);
				SetCurrentValueInternal(ToggleButton.IsCheckedProperty, BooleanBoxes.TrueBox);
			}
			else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
			{
				e.Handled = true;
				ClearValue(ButtonBase.IsPressedPropertyKey);
				SetCurrentValueInternal(ToggleButton.IsCheckedProperty, BooleanBoxes.FalseBox);
			}
		}
	}

	/// <summary>Called when the access key for a <see cref="T:System.Windows.Controls.CheckBox" /> is invoked. </summary>
	/// <param name="e">The <see cref="T:System.Windows.Input.AccessKeyEventArgs" /> that contains the event data.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		if (!base.IsKeyboardFocused)
		{
			Focus();
		}
		base.OnAccessKey(e);
	}
}
