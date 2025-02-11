namespace System.Windows.Controls.Primitives;

/// <summary>Represents an implementation of a <see cref="T:System.Windows.Controls.Primitives.Thumb" /> control that enables a <see cref="T:System.Windows.Window" /> to change its size. </summary>
public class ResizeGrip : Control
{
	private static DependencyObjectType _dType;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	internal override int EffectiveValuesInitialSize => 28;

	static ResizeGrip()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeGrip), new FrameworkPropertyMetadata(typeof(ResizeGrip)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ResizeGrip));
		Window.IWindowServiceProperty.OverrideMetadata(typeof(ResizeGrip), new FrameworkPropertyMetadata(_OnWindowServiceChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ResizeGrip" /> class. </summary>
	public ResizeGrip()
	{
	}

	private static void _OnWindowServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as ResizeGrip).OnWindowServiceChanged(e.OldValue as Window, e.NewValue as Window);
	}

	private void OnWindowServiceChanged(Window oldWindow, Window newWindow)
	{
		if (oldWindow != null && oldWindow != newWindow)
		{
			oldWindow.ClearResizeGripControl(this);
		}
		newWindow?.SetResizeGripControl(this);
	}
}
