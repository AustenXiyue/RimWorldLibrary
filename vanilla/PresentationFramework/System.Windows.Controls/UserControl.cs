using System.Windows.Automation.Peers;
using System.Windows.Input;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Provides a simple way to create a control.</summary>
public class UserControl : ContentControl
{
	private static DependencyObjectType _dType;

	internal override FrameworkElement StateGroupsRoot => base.Content as FrameworkElement;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static UserControl()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata(typeof(UserControl)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(UserControl));
		UIElement.FocusableProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
		Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(UserControl), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.UserControl" /> class.</summary>
	public UserControl()
	{
	}

	internal override void AdjustBranchSource(RoutedEventArgs e)
	{
		e.Source = this;
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.UserControl" />.</summary>
	/// <returns>A new <see cref="T:System.Windows.Automation.Peers.UserControlAutomationPeer" /> for this <see cref="T:System.Windows.Controls.UserControl" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new UserControlAutomationPeer(this);
	}
}
