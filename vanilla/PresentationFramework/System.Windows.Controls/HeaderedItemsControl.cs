using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that contains multiple items and has a header.</summary>
[DefaultProperty("Header")]
[Localizability(LocalizationCategory.Menu)]
public class HeaderedItemsControl : ItemsControl
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(HeaderedItemsControl), new FrameworkPropertyMetadata((object)null, (PropertyChangedCallback)OnHeaderChanged));

	private static readonly DependencyPropertyKey HasHeaderPropertyKey = HeaderedContentControl.HasHeaderPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HasHeader" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HasHeader" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HasHeaderProperty = HeaderedContentControl.HasHeaderProperty.AddOwner(typeof(HeaderedItemsControl));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplate" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(HeaderedItemsControl), new FrameworkPropertyMetadata(null, OnHeaderTemplateChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplateSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = HeaderedContentControl.HeaderTemplateSelectorProperty.AddOwner(typeof(HeaderedItemsControl), new FrameworkPropertyMetadata(null, OnHeaderTemplateSelectorChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderStringFormat" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat", typeof(string), typeof(HeaderedItemsControl), new FrameworkPropertyMetadata(null, OnHeaderStringFormatChanged));

	/// <summary>Gets or sets the item that labels the control.  </summary>
	/// <returns>An object that labels the <see cref="T:System.Windows.Controls.HeaderedItemsControl" />. The default is null. A header can be a string or a <see cref="T:System.Windows.UIElement" />.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public object Header
	{
		get
		{
			return GetValue(HeaderProperty);
		}
		set
		{
			SetValue(HeaderProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> has a header.  </summary>
	/// <returns>true if the control has a header; otherwise, false. The default is false.</returns>
	[Bindable(false)]
	[Browsable(false)]
	public bool HasHeader => (bool)GetValue(HasHeaderProperty);

	/// <summary>Gets or sets the template used to display the contents of the control's header.  </summary>
	/// <returns>A data template used to display a control's header. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public DataTemplate HeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(HeaderTemplateProperty);
		}
		set
		{
			SetValue(HeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the object that provides custom selection logic for a template used to display the header of each item.  </summary>
	/// <returns>A data template selector. The default is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public DataTemplateSelector HeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(HeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> property if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> property if it is displayed as a string.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public string HeaderStringFormat
	{
		get
		{
			return (string)GetValue(HeaderStringFormatProperty);
		}
		set
		{
			SetValue(HeaderStringFormatProperty, value);
		}
	}

	/// <summary>Gets an enumerator to the logical child elements of the <see cref="T:System.Windows.Controls.HeaderedItemsControl" />. </summary>
	/// <returns>An enumerator. The default is null.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			object header = Header;
			if (ReadControlFlag(ControlBoolFlags.HeaderIsNotLogical) || header == null)
			{
				return base.LogicalChildren;
			}
			return new HeaderedItemsModelTreeEnumerator(this, base.LogicalChildren, header);
		}
	}

	private bool HeaderIsItem
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.HeaderIsItem);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.HeaderIsItem, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> class. </summary>
	public HeaderedItemsControl()
	{
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		HeaderedItemsControl obj = (HeaderedItemsControl)d;
		obj.SetValue(HasHeaderPropertyKey, (e.NewValue != null) ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
		obj.OnHeaderChanged(e.OldValue, e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> property of a <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> changes. </summary>
	/// <param name="oldHeader">The old value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> property.</param>
	/// <param name="newHeader">The new value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.Header" /> property.</param>
	protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
	{
		if (IsHeaderLogical())
		{
			RemoveLogicalChild(oldHeader);
			AddLogicalChild(newHeader);
		}
	}

	private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedItemsControl)d).OnHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplate" /> property changes. </summary>
	/// <param name="oldHeaderTemplate">The old value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplate" /> property.</param>
	/// <param name="newHeaderTemplate">The new value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplate" /> property.</param>
	protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
	{
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
	}

	private static void OnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedItemsControl)d).OnHeaderTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplateSelector" /> property changes. </summary>
	/// <param name="oldHeaderTemplateSelector">The old value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplateSelector" /> property.</param>
	/// <param name="newHeaderTemplateSelector">The new value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderTemplateSelector" /> property.</param>
	protected virtual void OnHeaderTemplateSelectorChanged(DataTemplateSelector oldHeaderTemplateSelector, DataTemplateSelector newHeaderTemplateSelector)
	{
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
	}

	private static void OnHeaderStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedItemsControl)d).OnHeaderStringFormatChanged((string)e.OldValue, (string)e.NewValue);
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderStringFormat" /> property changes.</summary>
	/// <param name="oldHeaderStringFormat">The old value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderStringFormat" /> property.</param>
	/// <param name="newHeaderStringFormat">The new value of the <see cref="P:System.Windows.Controls.HeaderedItemsControl.HeaderStringFormat" /> property.</param>
	protected virtual void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
	{
	}

	internal void PrepareHeaderedItemsControl(object item, ItemsControl parentItemsControl)
	{
		bool flag = item != this;
		WriteControlFlag(ControlBoolFlags.HeaderIsNotLogical, flag);
		PrepareItemsControl(item, parentItemsControl);
		if (flag)
		{
			if (HeaderIsItem || !HasNonDefaultValue(HeaderProperty))
			{
				Header = item;
				HeaderIsItem = true;
			}
			DataTemplate itemTemplate = parentItemsControl.ItemTemplate;
			DataTemplateSelector itemTemplateSelector = parentItemsControl.ItemTemplateSelector;
			string itemStringFormat = parentItemsControl.ItemStringFormat;
			if (itemTemplate != null)
			{
				SetValue(HeaderTemplateProperty, itemTemplate);
			}
			if (itemTemplateSelector != null)
			{
				SetValue(HeaderTemplateSelectorProperty, itemTemplateSelector);
			}
			if (itemStringFormat != null && Helper.HasDefaultValue(this, HeaderStringFormatProperty))
			{
				SetValue(HeaderStringFormatProperty, itemStringFormat);
			}
			PrepareHierarchy(item, parentItemsControl);
		}
	}

	internal void ClearHeaderedItemsControl(object item)
	{
		ClearItemsControl(item);
		if (item != this && HeaderIsItem)
		{
			Header = BindingExpressionBase.DisconnectedItem;
		}
	}

	internal override string GetPlainText()
	{
		return ContentControl.ContentObjectToString(Header);
	}

	/// <summary>Returns the string representation of a <see cref="T:System.Windows.Controls.HeaderedItemsControl" /> object. </summary>
	/// <returns>A string that represents this object.</returns>
	public override string ToString()
	{
		string text = GetType().ToString();
		string headerText = string.Empty;
		int itemCount = 0;
		bool valuesDefined = false;
		if (CheckAccess())
		{
			headerText = ContentControl.ContentObjectToString(Header);
			itemCount = (base.HasItems ? base.Items.Count : 0);
			valuesDefined = true;
		}
		else
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), (DispatcherOperationCallback)delegate
			{
				headerText = ContentControl.ContentObjectToString(Header);
				itemCount = (base.HasItems ? base.Items.Count : 0);
				valuesDefined = true;
				return (object)null;
			}, null);
		}
		if (valuesDefined)
		{
			return SR.Format(SR.ToStringFormatString_HeaderedItemsControl, text, headerText, itemCount);
		}
		return text;
	}

	private void PrepareHierarchy(object item, ItemsControl parentItemsControl)
	{
		DataTemplate dataTemplate = HeaderTemplate;
		if (dataTemplate == null)
		{
			DataTemplateSelector headerTemplateSelector = HeaderTemplateSelector;
			if (headerTemplateSelector != null)
			{
				dataTemplate = headerTemplateSelector.SelectTemplate(item, this);
			}
			if (dataTemplate == null)
			{
				dataTemplate = (DataTemplate)FrameworkElement.FindTemplateResourceInternal(this, item, typeof(DataTemplate));
			}
		}
		if (!(dataTemplate is HierarchicalDataTemplate hierarchicalDataTemplate))
		{
			return;
		}
		bool flag = base.ItemTemplate == parentItemsControl.ItemTemplate;
		bool flag2 = base.ItemContainerStyle == parentItemsControl.ItemContainerStyle;
		if (hierarchicalDataTemplate.ItemsSource != null && !HasNonDefaultValue(ItemsControl.ItemsSourceProperty))
		{
			SetBinding(ItemsControl.ItemsSourceProperty, hierarchicalDataTemplate.ItemsSource);
		}
		if (hierarchicalDataTemplate.IsItemStringFormatSet && base.ItemStringFormat == parentItemsControl.ItemStringFormat)
		{
			ClearValue(ItemsControl.ItemTemplateProperty);
			ClearValue(ItemsControl.ItemTemplateSelectorProperty);
			ClearValue(ItemsControl.ItemStringFormatProperty);
			if (hierarchicalDataTemplate.ItemStringFormat != null)
			{
				base.ItemStringFormat = hierarchicalDataTemplate.ItemStringFormat;
			}
		}
		if (hierarchicalDataTemplate.IsItemTemplateSelectorSet && base.ItemTemplateSelector == parentItemsControl.ItemTemplateSelector)
		{
			ClearValue(ItemsControl.ItemTemplateProperty);
			ClearValue(ItemsControl.ItemTemplateSelectorProperty);
			if (hierarchicalDataTemplate.ItemTemplateSelector != null)
			{
				base.ItemTemplateSelector = hierarchicalDataTemplate.ItemTemplateSelector;
			}
		}
		if (hierarchicalDataTemplate.IsItemTemplateSet && flag)
		{
			ClearValue(ItemsControl.ItemTemplateProperty);
			if (hierarchicalDataTemplate.ItemTemplate != null)
			{
				base.ItemTemplate = hierarchicalDataTemplate.ItemTemplate;
			}
		}
		if (hierarchicalDataTemplate.IsItemContainerStyleSelectorSet && base.ItemContainerStyleSelector == parentItemsControl.ItemContainerStyleSelector)
		{
			ClearValue(ItemsControl.ItemContainerStyleProperty);
			ClearValue(ItemsControl.ItemContainerStyleSelectorProperty);
			if (hierarchicalDataTemplate.ItemContainerStyleSelector != null)
			{
				base.ItemContainerStyleSelector = hierarchicalDataTemplate.ItemContainerStyleSelector;
			}
		}
		if (hierarchicalDataTemplate.IsItemContainerStyleSet && flag2)
		{
			ClearValue(ItemsControl.ItemContainerStyleProperty);
			if (hierarchicalDataTemplate.ItemContainerStyle != null)
			{
				base.ItemContainerStyle = hierarchicalDataTemplate.ItemContainerStyle;
			}
		}
		if (hierarchicalDataTemplate.IsAlternationCountSet && base.AlternationCount == parentItemsControl.AlternationCount)
		{
			ClearValue(ItemsControl.AlternationCountProperty);
			if (true)
			{
				base.AlternationCount = hierarchicalDataTemplate.AlternationCount;
			}
		}
		if (hierarchicalDataTemplate.IsItemBindingGroupSet && base.ItemBindingGroup == parentItemsControl.ItemBindingGroup)
		{
			ClearValue(ItemsControl.ItemBindingGroupProperty);
			if (hierarchicalDataTemplate.ItemBindingGroup != null)
			{
				base.ItemBindingGroup = hierarchicalDataTemplate.ItemBindingGroup;
			}
		}
	}

	private bool IsBound(DependencyProperty dp, Binding binding)
	{
		BindingExpressionBase bindingExpression = BindingOperations.GetBindingExpression(this, dp);
		if (bindingExpression != null)
		{
			return bindingExpression.ParentBindingBase == binding;
		}
		return false;
	}

	private bool IsHeaderLogical()
	{
		if (ReadControlFlag(ControlBoolFlags.HeaderIsNotLogical))
		{
			return false;
		}
		if (BindingOperations.IsDataBound(this, HeaderProperty))
		{
			WriteControlFlag(ControlBoolFlags.HeaderIsNotLogical, set: true);
			return false;
		}
		return true;
	}
}
