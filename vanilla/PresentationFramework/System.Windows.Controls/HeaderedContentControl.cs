using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Provides the base implementation for all controls that contain single content and have a header.</summary>
[Localizability(LocalizationCategory.Text)]
public class HeaderedContentControl : ContentControl
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderProperty;

	internal static readonly DependencyPropertyKey HasHeaderPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedContentControl.HasHeader" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedContentControl.HasHeader" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HasHeaderProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplateSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderStringFormat" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeaderStringFormatProperty;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets the data used for the header of each control.  </summary>
	/// <returns>A header object. The default is null.</returns>
	[Bindable(true)]
	[Category("Content")]
	[Localizability(LocalizationCategory.Label)]
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

	/// <summary>Gets a value that indicates whether the header is null.  </summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property is not null; otherwise, false. The default is false.</returns>
	[Bindable(false)]
	[Browsable(false)]
	public bool HasHeader => (bool)GetValue(HasHeaderProperty);

	/// <summary>Gets or sets the template used to display the content of the control's header.  </summary>
	/// <returns>A data template. The default is null.</returns>
	[Bindable(true)]
	[Category("Content")]
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

	/// <summary>Gets or sets a data template selector that provides custom logic for choosing the template used to display the header.  </summary>
	/// <returns>A data template selector object. The default is null.</returns>
	[Bindable(true)]
	[Category("Content")]
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

	/// <summary>Gets or sets a composite string that specifies how to format the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property if it is displayed as a string. The default is null.</returns>
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

	/// <summary>Gets an enumerator to the logical child elements of the <see cref="T:System.Windows.Controls.ControlTemplate" />. </summary>
	/// <returns>An enumerator. </returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			object header = Header;
			if (HeaderIsNotLogical || header == null)
			{
				return base.LogicalChildren;
			}
			return new HeaderedContentModelTreeEnumerator(this, base.ContentIsNotLogical ? null : base.Content, header);
		}
	}

	internal bool HeaderIsNotLogical
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.HeaderIsNotLogical);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.HeaderIsNotLogical, value);
		}
	}

	internal bool HeaderIsItem
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

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static HeaderedContentControl()
	{
		HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(HeaderedContentControl), new FrameworkPropertyMetadata((object)null, (PropertyChangedCallback)OnHeaderChanged));
		HasHeaderPropertyKey = DependencyProperty.RegisterReadOnly("HasHeader", typeof(bool), typeof(HeaderedContentControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		HasHeaderProperty = HasHeaderPropertyKey.DependencyProperty;
		HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(HeaderedContentControl), new FrameworkPropertyMetadata(null, OnHeaderTemplateChanged));
		HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(HeaderedContentControl), new FrameworkPropertyMetadata(null, OnHeaderTemplateSelectorChanged));
		HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat", typeof(string), typeof(HeaderedContentControl), new FrameworkPropertyMetadata(null, OnHeaderStringFormatChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderedContentControl), new FrameworkPropertyMetadata(typeof(HeaderedContentControl)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(HeaderedContentControl));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.HeaderedContentControl" /> class. </summary>
	public HeaderedContentControl()
	{
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		HeaderedContentControl obj = (HeaderedContentControl)d;
		obj.SetValue(HasHeaderPropertyKey, (e.NewValue != null) ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
		obj.OnHeaderChanged(e.OldValue, e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property of a <see cref="T:System.Windows.Controls.HeaderedContentControl" /> changes. </summary>
	/// <param name="oldHeader">Old value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property.</param>
	/// <param name="newHeader">New value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.Header" /> property.</param>
	protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
	{
		RemoveLogicalChild(oldHeader);
		AddLogicalChild(newHeader);
	}

	private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedContentControl)d).OnHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" /> property changes. </summary>
	/// <param name="oldHeaderTemplate">Old value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" /> property.</param>
	/// <param name="newHeaderTemplate">New value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplate" /> property.</param>
	protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
	{
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
	}

	private static void OnHeaderTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedContentControl)d).OnHeaderTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplateSelector" /> property changes. </summary>
	/// <param name="oldHeaderTemplateSelector">Old value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplateSelector" /> property.</param>
	/// <param name="newHeaderTemplateSelector">New value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderTemplateSelector" /> property.</param>
	protected virtual void OnHeaderTemplateSelectorChanged(DataTemplateSelector oldHeaderTemplateSelector, DataTemplateSelector newHeaderTemplateSelector)
	{
		Helper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
	}

	private static void OnHeaderStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedContentControl)d).OnHeaderStringFormatChanged((string)e.OldValue, (string)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderStringFormat" /> property changes.</summary>
	/// <param name="oldHeaderStringFormat">The old value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderStringFormat" /> property.</param>
	/// <param name="newHeaderStringFormat">The new value of the <see cref="P:System.Windows.Controls.HeaderedContentControl.HeaderStringFormat" /> property.</param>
	protected virtual void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
	{
	}

	internal override string GetPlainText()
	{
		return ContentControl.ContentObjectToString(Header);
	}

	internal void PrepareHeaderedContentControl(object item, DataTemplate itemTemplate, DataTemplateSelector itemTemplateSelector, string stringFormat)
	{
		if (item != this)
		{
			base.ContentIsNotLogical = true;
			HeaderIsNotLogical = true;
			if (base.ContentIsItem || !HasNonDefaultValue(ContentControl.ContentProperty))
			{
				base.Content = item;
				base.ContentIsItem = true;
			}
			if (!(item is Visual) && (HeaderIsItem || !HasNonDefaultValue(HeaderProperty)))
			{
				Header = item;
				HeaderIsItem = true;
			}
			if (itemTemplate != null)
			{
				SetValue(HeaderTemplateProperty, itemTemplate);
			}
			if (itemTemplateSelector != null)
			{
				SetValue(HeaderTemplateSelectorProperty, itemTemplateSelector);
			}
			if (stringFormat != null)
			{
				SetValue(HeaderStringFormatProperty, stringFormat);
			}
		}
		else
		{
			base.ContentIsNotLogical = false;
		}
	}

	internal void ClearHeaderedContentControl(object item)
	{
		if (item != this)
		{
			if (base.ContentIsItem)
			{
				base.Content = BindingExpressionBase.DisconnectedItem;
			}
			if (HeaderIsItem)
			{
				Header = BindingExpressionBase.DisconnectedItem;
			}
		}
	}

	/// <summary>Provides a string representation of a <see cref="T:System.Windows.Controls.HeaderedContentControl" />. </summary>
	/// <returns>A string representation of the object.</returns>
	public override string ToString()
	{
		string text = GetType().ToString();
		string headerText = string.Empty;
		string contentText = string.Empty;
		bool valuesDefined = false;
		if (CheckAccess())
		{
			headerText = ContentControl.ContentObjectToString(Header);
			contentText = ContentControl.ContentObjectToString(base.Content);
			valuesDefined = true;
		}
		else
		{
			base.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), (DispatcherOperationCallback)delegate
			{
				headerText = ContentControl.ContentObjectToString(Header);
				contentText = ContentControl.ContentObjectToString(base.Content);
				valuesDefined = true;
				return (object)null;
			}, null);
		}
		if (valuesDefined)
		{
			return SR.Format(SR.ToStringFormatString_HeaderedContentControl, text, headerText, contentText);
		}
		return text;
	}
}
