using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Indicates the progress of an operation. </summary>
[TemplatePart(Name = "PART_Track", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_Indicator", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_GlowRect", Type = typeof(FrameworkElement))]
public class ProgressBar : RangeBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ProgressBar.IsIndeterminate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ProgressBar.IsIndeterminate" /> dependency property.</returns>
	public static readonly DependencyProperty IsIndeterminateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ProgressBar.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ProgressBar.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	private const string TrackTemplateName = "PART_Track";

	private const string IndicatorTemplateName = "PART_Indicator";

	private const string GlowingRectTemplateName = "PART_GlowRect";

	private FrameworkElement _track;

	private FrameworkElement _indicator;

	private FrameworkElement _glow;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Controls.ProgressBar" /> shows actual values or generic, continuous progress feedback.   </summary>
	/// <returns>false if the <see cref="T:System.Windows.Controls.ProgressBar" /> shows actual values; true if the <see cref="T:System.Windows.Controls.ProgressBar" /> shows generic progress. The default is false.</returns>
	public bool IsIndeterminate
	{
		get
		{
			return (bool)GetValue(IsIndeterminateProperty);
		}
		set
		{
			SetValue(IsIndeterminateProperty, value);
		}
	}

	/// <summary>Gets or sets the orientation of a <see cref="T:System.Windows.Controls.ProgressBar" />: horizontal or vertical.   </summary>
	/// <returns>One of the <see cref="T:System.Windows.Controls.Orientation" /> values. The default is <see cref="F:System.Windows.Controls.Orientation.Horizontal" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static ProgressBar()
	{
		IsIndeterminateProperty = DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(ProgressBar), new FrameworkPropertyMetadata(false, OnIsIndeterminateChanged));
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ProgressBar), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged), IsValidOrientation);
		UIElement.FocusableProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(typeof(ProgressBar)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ProgressBar));
		RangeBase.MaximumProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(100.0));
		Control.ForegroundProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(OnForegroundChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.ProgressBar);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ProgressBar" /> class. </summary>
	public ProgressBar()
	{
		base.IsVisibleChanged += delegate
		{
			UpdateAnimation();
			if (_glow == null)
			{
				UpdateVisualState();
			}
		};
	}

	private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ProgressBar obj = (ProgressBar)d;
		if (UIElementAutomationPeer.FromElement(obj) is ProgressBarAutomationPeer progressBarAutomationPeer)
		{
			progressBarAutomationPeer.InvalidatePeer();
		}
		obj.SetProgressBarGlowElementBrush();
		obj.SetProgressBarIndicatorLength();
		obj.UpdateVisualState();
	}

	private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProgressBar)d).SetProgressBarGlowElementBrush();
	}

	internal static bool IsValidOrientation(object o)
	{
		Orientation orientation = (Orientation)o;
		if (orientation != 0)
		{
			return orientation == Orientation.Vertical;
		}
		return true;
	}

	private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ProgressBar)d).SetProgressBarIndicatorLength();
	}

	private void SetProgressBarIndicatorLength()
	{
		if (_track != null && _indicator != null)
		{
			double minimum = base.Minimum;
			double maximum = base.Maximum;
			double value = base.Value;
			double num = ((IsIndeterminate || maximum <= minimum) ? 1.0 : ((value - minimum) / (maximum - minimum)));
			_indicator.Width = num * _track.ActualWidth;
			UpdateAnimation();
		}
	}

	private void SetProgressBarGlowElementBrush()
	{
		if (_glow == null)
		{
			return;
		}
		_glow.InvalidateProperty(UIElement.OpacityMaskProperty);
		_glow.InvalidateProperty(Shape.FillProperty);
		if (IsIndeterminate)
		{
			if (base.Foreground is SolidColorBrush)
			{
				Color color = ((SolidColorBrush)base.Foreground).Color;
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
				linearGradientBrush.StartPoint = new Point(0.0, 0.0);
				linearGradientBrush.EndPoint = new Point(1.0, 0.0);
				linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
				linearGradientBrush.GradientStops.Add(new GradientStop(color, 0.4));
				linearGradientBrush.GradientStops.Add(new GradientStop(color, 0.6));
				linearGradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
				_glow.SetCurrentValue(Shape.FillProperty, linearGradientBrush);
			}
			else
			{
				LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush();
				linearGradientBrush2.StartPoint = new Point(0.0, 0.0);
				linearGradientBrush2.EndPoint = new Point(1.0, 0.0);
				linearGradientBrush2.GradientStops.Add(new GradientStop(Colors.Transparent, 0.0));
				linearGradientBrush2.GradientStops.Add(new GradientStop(Colors.Black, 0.4));
				linearGradientBrush2.GradientStops.Add(new GradientStop(Colors.Black, 0.6));
				linearGradientBrush2.GradientStops.Add(new GradientStop(Colors.Transparent, 1.0));
				_glow.SetCurrentValue(UIElement.OpacityMaskProperty, linearGradientBrush2);
				_glow.SetCurrentValue(Shape.FillProperty, base.Foreground);
			}
		}
	}

	private void UpdateAnimation()
	{
		if (_glow != null)
		{
			if (base.IsVisible && _glow.Width > 0.0 && _indicator.Width > 0.0)
			{
				double num = _indicator.Width + _glow.Width;
				double num2 = -1.0 * _glow.Width;
				TimeSpan timeSpan = TimeSpan.FromSeconds((double)(int)(num - num2) / 200.0);
				TimeSpan timeSpan2 = TimeSpan.FromSeconds(1.0);
				TimeSpan value = ((!DoubleUtil.GreaterThan(_glow.Margin.Left, num2) || !DoubleUtil.LessThan(_glow.Margin.Left, num - 1.0)) ? TimeSpan.Zero : TimeSpan.FromSeconds(-1.0 * (_glow.Margin.Left - num2) / 200.0));
				ThicknessAnimationUsingKeyFrames thicknessAnimationUsingKeyFrames = new ThicknessAnimationUsingKeyFrames();
				thicknessAnimationUsingKeyFrames.BeginTime = value;
				thicknessAnimationUsingKeyFrames.Duration = new Duration(timeSpan + timeSpan2);
				thicknessAnimationUsingKeyFrames.RepeatBehavior = RepeatBehavior.Forever;
				thicknessAnimationUsingKeyFrames.KeyFrames.Add(new LinearThicknessKeyFrame(new Thickness(num2, 0.0, 0.0, 0.0), TimeSpan.FromSeconds(0.0)));
				thicknessAnimationUsingKeyFrames.KeyFrames.Add(new LinearThicknessKeyFrame(new Thickness(num, 0.0, 0.0, 0.0), timeSpan));
				_glow.BeginAnimation(FrameworkElement.MarginProperty, thicknessAnimationUsingKeyFrames);
			}
			else
			{
				_glow.BeginAnimation(FrameworkElement.MarginProperty, null);
			}
		}
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!IsIndeterminate || !base.IsVisible)
		{
			VisualStateManager.GoToState(this, "Determinate", useTransitions);
		}
		else
		{
			VisualStateManager.GoToState(this, "Indeterminate", useTransitions);
		}
		ChangeValidationVisualState(useTransitions);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ProgressBarAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.</summary>
	/// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ProgressBarAutomationPeer(this);
	}

	/// <summary>Updates the current position of the <see cref="T:System.Windows.Controls.ProgressBar" /> when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property changes. </summary>
	/// <param name="oldMinimum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	/// <param name="newMinimum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> property.</param>
	protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
	{
		base.OnMinimumChanged(oldMinimum, newMinimum);
		SetProgressBarIndicatorLength();
	}

	/// <summary>Updates the current position of the <see cref="T:System.Windows.Controls.ProgressBar" /> when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property changes. </summary>
	/// <param name="oldMaximum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	/// <param name="newMaximum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> property.</param>
	protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
	{
		base.OnMaximumChanged(oldMaximum, newMaximum);
		SetProgressBarIndicatorLength();
	}

	/// <summary>Updates the current position of the <see cref="T:System.Windows.Controls.ProgressBar" /> when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property changes. </summary>
	/// <param name="oldValue">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property.</param>
	/// <param name="newValue">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Value" /> property.</param>
	protected override void OnValueChanged(double oldValue, double newValue)
	{
		base.OnValueChanged(oldValue, newValue);
		SetProgressBarIndicatorLength();
	}

	/// <summary>Called when a template is applied to a <see cref="T:System.Windows.Controls.ProgressBar" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (_track != null)
		{
			_track.SizeChanged -= OnTrackSizeChanged;
		}
		_track = GetTemplateChild("PART_Track") as FrameworkElement;
		_indicator = GetTemplateChild("PART_Indicator") as FrameworkElement;
		_glow = GetTemplateChild("PART_GlowRect") as FrameworkElement;
		if (_track != null)
		{
			_track.SizeChanged += OnTrackSizeChanged;
		}
		if (IsIndeterminate)
		{
			SetProgressBarGlowElementBrush();
		}
	}

	private void OnTrackSizeChanged(object sender, SizeChangedEventArgs e)
	{
		SetProgressBarIndicatorLength();
	}
}
