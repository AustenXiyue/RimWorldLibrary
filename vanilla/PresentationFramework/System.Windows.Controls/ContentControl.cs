using System.Collections;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control with a single piece of content of any type.  </summary>
[DefaultProperty("Content")]
[ContentProperty("Content")]
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class ContentControl : Control, IAddChild
{
	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContentControl.Content" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentControl.Content" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentProperty;

	private static readonly DependencyPropertyKey HasContentPropertyKey;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContentControl.HasContent" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentControl.HasContent" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HasContentProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentTemplateProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentControl.ContentStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentControl.ContentStringFormat" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentStringFormatProperty;

	private static DependencyObjectType _dType;

	/// <summary> Gets an enumerator to the content control's logical child elements. </summary>
	/// <returns>An enumerator. The default value is null.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			object content = Content;
			if (ContentIsNotLogical || content == null)
			{
				return EmptyEnumerator.Instance;
			}
			if (base.TemplatedParent != null && content is DependencyObject current)
			{
				DependencyObject parent = LogicalTreeHelper.GetParent(current);
				if (parent != null && parent != this)
				{
					return EmptyEnumerator.Instance;
				}
			}
			return new ContentModelTreeEnumerator(this, content);
		}
	}

	/// <summary> Gets or sets the content of a <see cref="T:System.Windows.Controls.ContentControl" />. </summary>
	/// <returns>An object that contains the control's content. The default value is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public object Content
	{
		get
		{
			return GetValue(ContentProperty);
		}
		set
		{
			SetValue(ContentProperty, value);
		}
	}

	/// <summary> Gets a value that indicates whether the <see cref="T:System.Windows.Controls.ContentControl" /> contains content. </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ContentControl" /> has content; otherwise false. The default value is false.</returns>
	[Browsable(false)]
	[ReadOnly(true)]
	public bool HasContent => (bool)GetValue(HasContentProperty);

	/// <summary> Gets or sets the data template used to display the content of the <see cref="T:System.Windows.Controls.ContentControl" />. </summary>
	/// <returns>A data template. The default value is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public DataTemplate ContentTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ContentTemplateProperty);
		}
		set
		{
			SetValue(ContentTemplateProperty, value);
		}
	}

	/// <summary> Gets or sets a template selector that enables an application writer to provide custom template-selection logic. </summary>
	/// <returns>A data template selector. The default value is null.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector ContentTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty);
		}
		set
		{
			SetValue(ContentTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property if it is displayed as a string.</returns>
	[Bindable(true)]
	[CustomCategory("Content")]
	public string ContentStringFormat
	{
		get
		{
			return (string)GetValue(ContentStringFormatProperty);
		}
		set
		{
			SetValue(ContentStringFormatProperty, value);
		}
	}

	internal bool ContentIsNotLogical
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.ContentIsNotLogical);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.ContentIsNotLogical, value);
		}
	}

	internal bool ContentIsItem
	{
		get
		{
			return ReadControlFlag(ControlBoolFlags.ContentIsItem);
		}
		set
		{
			WriteControlFlag(ControlBoolFlags.ContentIsItem, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 4;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ContentControl" /> class. </summary>
	public ContentControl()
	{
	}

	static ContentControl()
	{
		ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ContentControl), new FrameworkPropertyMetadata((object)null, (PropertyChangedCallback)OnContentChanged));
		HasContentPropertyKey = DependencyProperty.RegisterReadOnly("HasContent", typeof(bool), typeof(ContentControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.None));
		HasContentProperty = HasContentPropertyKey.DependencyProperty;
		ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentControl), new FrameworkPropertyMetadata(null, OnContentTemplateChanged));
		ContentTemplateSelectorProperty = DependencyProperty.Register("ContentTemplateSelector", typeof(DataTemplateSelector), typeof(ContentControl), new FrameworkPropertyMetadata(null, OnContentTemplateSelectorChanged));
		ContentStringFormatProperty = DependencyProperty.Register("ContentStringFormat", typeof(string), typeof(ContentControl), new FrameworkPropertyMetadata(null, OnContentStringFormatChanged));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentControl), new FrameworkPropertyMetadata(typeof(ContentControl)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ContentControl));
		ControlsTraceLogger.AddControl(TelemetryControls.ContentControl);
	}

	internal override string GetPlainText()
	{
		return ContentObjectToString(Content);
	}

	internal static string ContentObjectToString(object content)
	{
		if (content != null)
		{
			if (content is FrameworkElement frameworkElement)
			{
				return frameworkElement.GetPlainText();
			}
			return content.ToString();
		}
		return string.Empty;
	}

	internal void PrepareContentControl(object item, DataTemplate itemTemplate, DataTemplateSelector itemTemplateSelector, string itemStringFormat)
	{
		if (item != this)
		{
			ContentIsNotLogical = true;
			if (ContentIsItem || !HasNonDefaultValue(ContentProperty))
			{
				Content = item;
				ContentIsItem = true;
			}
			if (itemTemplate != null)
			{
				SetValue(ContentTemplateProperty, itemTemplate);
			}
			if (itemTemplateSelector != null)
			{
				SetValue(ContentTemplateSelectorProperty, itemTemplateSelector);
			}
			if (itemStringFormat != null)
			{
				SetValue(ContentStringFormatProperty, itemStringFormat);
			}
		}
		else
		{
			ContentIsNotLogical = false;
		}
	}

	internal void ClearContentControl(object item)
	{
		if (item != this && ContentIsItem)
		{
			Content = BindingExpressionBase.DisconnectedItem;
		}
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property should be persisted.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property should be persisted; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual bool ShouldSerializeContent()
	{
		return ReadLocalValue(ContentProperty) != DependencyProperty.UnsetValue;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">  An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		AddChild(value);
	}

	/// <summary>Adds a specified object as the child of a <see cref="T:System.Windows.Controls.ContentControl" />. </summary>
	/// <param name="value">The object to add.</param>
	protected virtual void AddChild(object value)
	{
		if (Content == null || value == null)
		{
			Content = value;
			return;
		}
		throw new InvalidOperationException(SR.ContentControlCannotHaveMultipleContent);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">  A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		AddText(text);
	}

	/// <summary>Adds a specified text string to a <see cref="T:System.Windows.Controls.ContentControl" />. </summary>
	/// <param name="text">The string to add.</param>
	protected virtual void AddText(string text)
	{
		AddChild(text);
	}

	private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentControl obj = (ContentControl)d;
		obj.SetValue(HasContentPropertyKey, (e.NewValue != null) ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
		obj.OnContentChanged(e.OldValue, e.NewValue);
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property changes. </summary>
	/// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
	/// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property.</param>
	protected virtual void OnContentChanged(object oldContent, object newContent)
	{
		RemoveLogicalChild(oldContent);
		if (ContentIsNotLogical)
		{
			return;
		}
		if (newContent is DependencyObject current)
		{
			DependencyObject parent = LogicalTreeHelper.GetParent(current);
			if (parent != null)
			{
				if (base.TemplatedParent != null && FrameworkObject.IsEffectiveAncestor(parent, this))
				{
					return;
				}
				LogicalTreeHelper.RemoveLogicalChild(parent, newContent);
			}
		}
		AddLogicalChild(newContent);
	}

	private static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentControl)d).OnContentTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> property changes. </summary>
	/// <param name="oldContentTemplate">The old value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> property.</param>
	/// <param name="newContentTemplate">The new value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplate" /> property.</param>
	protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
	{
		Helper.CheckTemplateAndTemplateSelector("Content", ContentTemplateProperty, ContentTemplateSelectorProperty, this);
	}

	private static void OnContentTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentControl)d).OnContentTemplateSelectorChanged((DataTemplateSelector)e.NewValue, (DataTemplateSelector)e.NewValue);
	}

	/// <summary> Called when the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> property changes. </summary>
	/// <param name="oldContentTemplateSelector">The old value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> property.</param>
	/// <param name="newContentTemplateSelector">The new value of the <see cref="P:System.Windows.Controls.ContentControl.ContentTemplateSelector" /> property.</param>
	protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
	{
		Helper.CheckTemplateAndTemplateSelector("Content", ContentTemplateProperty, ContentTemplateSelectorProperty, this);
	}

	private static void OnContentStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentControl)d).OnContentStringFormatChanged((string)e.OldValue, (string)e.NewValue);
	}

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.ContentControl.ContentStringFormat" /> property changes.</summary>
	/// <param name="oldContentStringFormat">The old value of <see cref="P:System.Windows.Controls.ContentControl.ContentStringFormat" />.</param>
	/// <param name="newContentStringFormat">The new value of <see cref="P:System.Windows.Controls.ContentControl.ContentStringFormat" />.</param>
	protected virtual void OnContentStringFormatChanged(string oldContentStringFormat, string newContentStringFormat)
	{
	}
}
