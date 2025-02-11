using System.Windows.Data;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents the text input of a <see cref="T:System.Windows.Controls.DatePicker" />.</summary>
[TemplatePart(Name = "PART_Watermark", Type = typeof(ContentControl))]
public sealed class DatePickerTextBox : TextBox
{
	private const string ElementContentName = "PART_Watermark";

	private ContentControl elementContent;

	internal static readonly DependencyProperty WatermarkProperty;

	internal object Watermark
	{
		get
		{
			return GetValue(WatermarkProperty);
		}
		set
		{
			SetValue(WatermarkProperty, value);
		}
	}

	static DatePickerTextBox()
	{
		WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(DatePickerTextBox), new PropertyMetadata(OnWatermarkPropertyChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DatePickerTextBox), new FrameworkPropertyMetadata(typeof(DatePickerTextBox)));
		TextBox.TextProperty.OverrideMetadata(typeof(DatePickerTextBox), new FrameworkPropertyMetadata(Control.OnVisualStatePropertyChanged));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DatePickerTextBox" /> class. </summary>
	public DatePickerTextBox()
	{
		SetCurrentValue(WatermarkProperty, SR.DatePickerTextBox_DefaultWatermarkText);
		base.Loaded += OnLoaded;
		base.IsEnabledChanged += OnDatePickerTextBoxIsEnabledChanged;
	}

	/// <summary>Builds the visual tree for the <see cref="T:System.Windows.Controls.Primitives.DatePickerTextBox" /> when a new template is applied.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		elementContent = ExtractTemplatePart<ContentControl>("PART_Watermark");
		if (elementContent != null)
		{
			Binding binding = new Binding("Watermark");
			binding.Source = this;
			elementContent.SetBinding(ContentControl.ContentProperty, binding);
		}
		OnWatermarkChanged();
	}

	protected override void OnGotFocus(RoutedEventArgs e)
	{
		base.OnGotFocus(e);
		if (base.IsEnabled && !string.IsNullOrEmpty(base.Text))
		{
			Select(0, base.Text.Length);
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		ApplyTemplate();
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		base.ChangeVisualState(useTransitions);
		if (Watermark != null && string.IsNullOrEmpty(base.Text))
		{
			VisualStates.GoToState(this, useTransitions, "Watermarked", "Unwatermarked");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Unwatermarked");
		}
	}

	private T ExtractTemplatePart<T>(string partName) where T : DependencyObject
	{
		DependencyObject templateChild = GetTemplateChild(partName);
		return ExtractTemplatePart<T>(partName, templateChild);
	}

	private static T ExtractTemplatePart<T>(string partName, DependencyObject obj) where T : DependencyObject
	{
		return obj as T;
	}

	private void OnDatePickerTextBoxIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		bool flag = (bool)e.NewValue;
		SetCurrentValueInternal(TextBoxBase.IsReadOnlyProperty, BooleanBoxes.Box(!flag));
	}

	private void OnWatermarkChanged()
	{
		if (elementContent != null && Watermark is Control control)
		{
			control.IsTabStop = false;
			control.IsHitTestVisible = false;
		}
	}

	private static void OnWatermarkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		DatePickerTextBox obj = sender as DatePickerTextBox;
		obj.OnWatermarkChanged();
		obj.UpdateVisualState();
	}
}
