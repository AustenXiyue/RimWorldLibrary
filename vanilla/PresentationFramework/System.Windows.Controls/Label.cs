using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Markup;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents the text label for a control and provides support for access keys.</summary>
[Localizability(LocalizationCategory.Label)]
public class Label : ContentControl
{
	/// <summary> Identifies the <see cref="P:System.Windows.Controls.Label.Target" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Label.Target" /> dependency property.</returns>
	public static readonly DependencyProperty TargetProperty;

	private static readonly DependencyProperty LabeledByProperty;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the element that receives focus when the user presses the label's access key. </summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that receives focus when the user presses the access key. The default is null.</returns>
	[TypeConverter(typeof(NameReferenceConverter))]
	public UIElement Target
	{
		get
		{
			return (UIElement)GetValue(TargetProperty);
		}
		set
		{
			SetValue(TargetProperty, value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static Label()
	{
		TargetProperty = DependencyProperty.Register("Target", typeof(UIElement), typeof(Label), new FrameworkPropertyMetadata(null, OnTargetChanged));
		LabeledByProperty = DependencyProperty.RegisterAttached("LabeledBy", typeof(Label), typeof(Label), new FrameworkPropertyMetadata((object)null));
		EventManager.RegisterClassHandler(typeof(Label), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Label), new FrameworkPropertyMetadata(typeof(Label)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Label));
		Control.IsTabStopProperty.OverrideMetadata(typeof(Label), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		UIElement.FocusableProperty.OverrideMetadata(typeof(Label), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ControlsTraceLogger.AddControl(TelemetryControls.Label);
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Controls.Label" /> class. </summary>
	public Label()
	{
	}

	private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Label label = (Label)d;
		UIElement uIElement = (UIElement)e.OldValue;
		UIElement uIElement2 = (UIElement)e.NewValue;
		if (uIElement != null && uIElement.GetValue(LabeledByProperty) == label)
		{
			uIElement.ClearValue(LabeledByProperty);
		}
		uIElement2?.SetValue(LabeledByProperty, label);
	}

	internal static Label GetLabeledBy(DependencyObject o)
	{
		if (o == null)
		{
			throw new ArgumentNullException("o");
		}
		return (Label)o.GetValue(LabeledByProperty);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.LabelAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new LabelAutomationPeer(this);
	}

	private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
	{
		Label label = sender as Label;
		if (!e.Handled && e.Scope == null && (e.Target == null || e.Target == label))
		{
			e.Target = label.Target;
		}
	}
}
