using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Implements a selectable item inside a <see cref="T:System.Windows.Controls.ComboBox" />.  </summary>
[Localizability(LocalizationCategory.ComboBox)]
public class ComboBoxItem : ListBoxItem
{
	private static readonly DependencyPropertyKey IsHighlightedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ComboBoxItem.IsHighlighted" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ComboBoxItem.IsHighlighted" /> dependency property.</returns>
	public static readonly DependencyProperty IsHighlightedProperty;

	private static DependencyObjectType _dType;

	/// <summary>Gets a value that indicates whether the item is highlighted.   </summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.ComboBoxItem" /> is highlighted; otherwise, false. The default is false.</returns>
	public bool IsHighlighted
	{
		get
		{
			return (bool)GetValue(IsHighlightedProperty);
		}
		protected set
		{
			SetValue(IsHighlightedPropertyKey, BooleanBoxes.Box(value));
		}
	}

	private ComboBox ParentComboBox => base.ParentSelector as ComboBox;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ComboBoxItem" /> class</summary>
	public ComboBoxItem()
	{
	}

	static ComboBoxItem()
	{
		IsHighlightedPropertyKey = DependencyProperty.RegisterReadOnly("IsHighlighted", typeof(bool), typeof(ComboBoxItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsHighlightedProperty = IsHighlightedPropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxItem), new FrameworkPropertyMetadata(typeof(ComboBoxItem)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ComboBoxItem));
	}

	/// <summary> Responds to the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseButtonEventArgs" />.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		e.Handled = true;
		ParentComboBox?.NotifyComboBoxItemMouseDown(this);
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary> Responds to the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseButtonEventArgs" />.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		e.Handled = true;
		ParentComboBox?.NotifyComboBoxItemMouseUp(this);
		base.OnMouseLeftButtonUp(e);
	}

	/// <summary> Responds to a <see cref="E:System.Windows.UIElement.MouseEnter" /> event. </summary>
	/// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs" />.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		e.Handled = true;
		ParentComboBox?.NotifyComboBoxItemEnter(this);
		base.OnMouseEnter(e);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes. </summary>
	/// <param name="oldContent">Old content.</param>
	/// <param name="newContent">New content.</param>
	protected override void OnContentChanged(object oldContent, object newContent)
	{
		base.OnContentChanged(oldContent, newContent);
		ComboBox parentComboBox;
		if (base.IsSelected && (parentComboBox = ParentComboBox) != null)
		{
			parentComboBox.SelectedItemUpdated();
		}
		SetFlags(newContent is UIElement, VisualFlags.IsLayoutIslandRoot);
	}

	/// <summary>Announces that the keyboard is focused on this element. </summary>
	/// <param name="e">Keyboard input event arguments.</param>
	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		e.Handled = true;
		ParentComboBox?.NotifyComboBoxItemEnter(this);
		base.OnGotKeyboardFocus(e);
	}

	internal void SetIsHighlighted(bool isHighlighted)
	{
		IsHighlighted = isHighlighted;
	}
}
