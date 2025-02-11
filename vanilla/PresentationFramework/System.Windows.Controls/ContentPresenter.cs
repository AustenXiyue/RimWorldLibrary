using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Displays the content of a <see cref="T:System.Windows.Controls.ContentControl" />.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public class ContentPresenter : FrameworkElement
{
	private class UseContentTemplate : DataTemplate
	{
		public UseContentTemplate()
		{
			base.CanBuildVisualTree = true;
		}

		internal override bool BuildVisualTree(FrameworkElement container)
		{
			object content = ((ContentPresenter)container).Content;
			UIElement uIElement = content as UIElement;
			if (uIElement == null)
			{
				uIElement = (UIElement)TypeDescriptor.GetConverter(ReflectionHelper.GetReflectionType(content)).ConvertTo(content, typeof(UIElement));
			}
			StyleHelper.AddCustomTemplateRoot(container, uIElement);
			return true;
		}
	}

	private class DefaultTemplate : DataTemplate
	{
		private class TypeContext : ITypeDescriptorContext, IServiceProvider
		{
			private object _instance;

			IContainer ITypeDescriptorContext.Container => null;

			object ITypeDescriptorContext.Instance => _instance;

			PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

			public TypeContext(object instance)
			{
				_instance = instance;
			}

			void ITypeDescriptorContext.OnComponentChanged()
			{
			}

			bool ITypeDescriptorContext.OnComponentChanging()
			{
				return false;
			}

			object IServiceProvider.GetService(Type serviceType)
			{
				return null;
			}
		}

		public DefaultTemplate()
		{
			base.CanBuildVisualTree = true;
		}

		internal override bool BuildVisualTree(FrameworkElement container)
		{
			bool flag = EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "ContentPresenter.BuildVisualTree");
			}
			try
			{
				ContentPresenter contentPresenter = (ContentPresenter)container;
				return DefaultExpansion(contentPresenter.Content, contentPresenter) != null;
			}
			finally
			{
				if (flag)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, string.Format(CultureInfo.InvariantCulture, "ContentPresenter.BuildVisualTree for CP {0}", container.GetHashCode()));
				}
			}
		}

		private UIElement DefaultExpansion(object content, ContentPresenter container)
		{
			if (content == null)
			{
				return null;
			}
			TextBlock textBlock = CreateTextBlock(container);
			textBlock.IsContentPresenterContainer = true;
			if (container != null)
			{
				StyleHelper.AddCustomTemplateRoot(container, textBlock, checkVisualParent: false, mustCacheTreeStateOnChild: true);
			}
			DoDefaultExpansion(textBlock, content, container);
			return textBlock;
		}

		private void DoDefaultExpansion(TextBlock textBlock, object content, ContentPresenter container)
		{
			if (content is Inline item)
			{
				textBlock.Inlines.Add(item);
				return;
			}
			bool flag = false;
			XmlLanguage language = container.Language;
			CultureInfo specificCulture = language.GetSpecificCulture();
			container.CacheLanguage(language);
			string contentStringFormat;
			if ((contentStringFormat = container.ContentStringFormat) != null)
			{
				try
				{
					contentStringFormat = Helper.GetEffectiveStringFormat(contentStringFormat);
					textBlock.Text = string.Format(specificCulture, contentStringFormat, content);
					flag = true;
				}
				catch (FormatException)
				{
				}
			}
			if (!flag)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(ReflectionHelper.GetReflectionType(content));
				TypeContext context = new TypeContext(content);
				if (converter != null && converter.CanConvertTo(context, typeof(string)))
				{
					textBlock.Text = (string)converter.ConvertTo(context, specificCulture, content, typeof(string));
				}
				else
				{
					textBlock.Text = string.Format(specificCulture, "{0}", content);
				}
			}
		}
	}

	private class DefaultSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			DataTemplate dataTemplate = null;
			if (item != null)
			{
				dataTemplate = (DataTemplate)FrameworkElement.FindTemplateResourceInternal(container, item, typeof(DataTemplate));
			}
			if (dataTemplate == null)
			{
				TypeConverter typeConverter = null;
				dataTemplate = ((!(item is string s)) ? ((item is UIElement) ? UIElementContentTemplate : (SystemXmlHelper.IsXmlNode(item) ? ((ContentPresenter)container).SelectTemplateForXML() : ((item is Inline) ? DefaultContentTemplate : ((item == null || (typeConverter = TypeDescriptor.GetConverter(ReflectionHelper.GetReflectionType(item))) == null || !typeConverter.CanConvertTo(typeof(UIElement))) ? DefaultContentTemplate : UIElementContentTemplate)))) : ((ContentPresenter)container).SelectTemplateForString(s));
			}
			return dataTemplate;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.RecognizesAccessKey" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.RecognizesAccessKey" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty RecognizesAccessKeyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.Content" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.Content" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.ContentStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.ContentStringFormat" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentStringFormatProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContentPresenter.ContentSource" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContentPresenter.ContentSource" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ContentSourceProperty;

	internal static readonly DependencyProperty TemplateProperty;

	private DataTemplate _templateCache;

	private bool _templateIsCurrent;

	private bool _contentIsItem;

	private XmlLanguage _language;

	private static DataTemplate s_AccessTextTemplate;

	private static DataTemplate s_StringTemplate;

	private static DataTemplate s_XmlNodeTemplate;

	private static DataTemplate s_UIElementTemplate;

	private static DataTemplate s_DefaultTemplate;

	private static DefaultSelector s_DefaultTemplateSelector;

	private static readonly UncommonField<DataTemplate> XMLFormattingTemplateField;

	private static readonly UncommonField<DataTemplate> StringFormattingTemplateField;

	private static readonly UncommonField<DataTemplate> AccessTextFormattingTemplateField;

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.ContentPresenter" /> should use <see cref="T:System.Windows.Controls.AccessText" /> in its style.   </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ContentPresenter" /> should use <see cref="T:System.Windows.Controls.AccessText" /> in its style; otherwise, false. The default is false.</returns>
	public bool RecognizesAccessKey
	{
		get
		{
			return (bool)GetValue(RecognizesAccessKeyProperty);
		}
		set
		{
			SetValue(RecognizesAccessKeyProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the data used to generate the child elements of a <see cref="T:System.Windows.Controls.ContentPresenter" />.  </summary>
	/// <returns>The data used to generate the child elements. The default is null.</returns>
	public object Content
	{
		get
		{
			return GetValue(ContentControl.ContentProperty);
		}
		set
		{
			SetValue(ContentControl.ContentProperty, value);
		}
	}

	/// <summary>Gets or sets the template used to display the content of the control.   </summary>
	/// <returns>A <see cref="T:System.Windows.DataTemplate" /> that defines the visualization of the content. The default is null.</returns>
	public DataTemplate ContentTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ContentControl.ContentTemplateProperty);
		}
		set
		{
			SetValue(ContentControl.ContentTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Controls.DataTemplateSelector" />, which allows the application writer to provide custom logic for choosing the template that is used to display the content of the control.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.DataTemplateSelector" /> object that supplies logic to return a <see cref="T:System.Windows.DataTemplate" /> to apply. The default is null.</returns>
	public DataTemplateSelector ContentTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ContentControl.ContentTemplateSelectorProperty);
		}
		set
		{
			SetValue(ContentControl.ContentTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the <see cref="P:System.Windows.Controls.ContentPresenter.Content" /> property if it is displayed as a string.</summary>
	/// <returns>A composite string that specifies how to format the <see cref="P:System.Windows.Controls.ContentPresenter.Content" /> property if it is displayed as a string. The default is null.</returns>
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

	/// <summary>Gets or sets the base name to use during automatic aliasing.  </summary>
	/// <returns>The base name to use during automatic aliasing. The default is "Content".</returns>
	public string ContentSource
	{
		get
		{
			return GetValue(ContentSourceProperty) as string;
		}
		set
		{
			SetValue(ContentSourceProperty, value);
		}
	}

	internal static DataTemplate AccessTextContentTemplate => s_AccessTextTemplate;

	internal static DataTemplate StringContentTemplate => s_StringTemplate;

	internal override FrameworkTemplate TemplateInternal => Template;

	internal override FrameworkTemplate TemplateCache
	{
		get
		{
			return _templateCache;
		}
		set
		{
			_templateCache = (DataTemplate)value;
		}
	}

	internal bool TemplateIsCurrent => _templateIsCurrent;

	private static DataTemplate XmlNodeContentTemplate => s_XmlNodeTemplate;

	private static DataTemplate UIElementContentTemplate => s_UIElementTemplate;

	private static DataTemplate DefaultContentTemplate => s_DefaultTemplate;

	private static DefaultSelector DefaultTemplateSelector => s_DefaultTemplateSelector;

	private DataTemplate FormattingAccessTextContentTemplate
	{
		get
		{
			DataTemplate dataTemplate = AccessTextFormattingTemplateField.GetValue(this);
			if (dataTemplate == null)
			{
				Binding binding = new Binding();
				binding.StringFormat = ContentStringFormat;
				FrameworkElementFactory frameworkElementFactory = CreateAccessTextFactory();
				frameworkElementFactory.SetBinding(AccessText.TextProperty, binding);
				dataTemplate = new DataTemplate();
				dataTemplate.VisualTree = frameworkElementFactory;
				dataTemplate.Seal();
				AccessTextFormattingTemplateField.SetValue(this, dataTemplate);
			}
			return dataTemplate;
		}
	}

	private DataTemplate FormattingStringContentTemplate
	{
		get
		{
			DataTemplate dataTemplate = StringFormattingTemplateField.GetValue(this);
			if (dataTemplate == null)
			{
				Binding binding = new Binding();
				binding.StringFormat = ContentStringFormat;
				FrameworkElementFactory frameworkElementFactory = CreateTextBlockFactory();
				frameworkElementFactory.SetBinding(TextBlock.TextProperty, binding);
				dataTemplate = new DataTemplate();
				dataTemplate.VisualTree = frameworkElementFactory;
				dataTemplate.Seal();
				StringFormattingTemplateField.SetValue(this, dataTemplate);
			}
			return dataTemplate;
		}
	}

	private DataTemplate FormattingXmlNodeContentTemplate
	{
		get
		{
			DataTemplate dataTemplate = XMLFormattingTemplateField.GetValue(this);
			if (dataTemplate == null)
			{
				Binding binding = new Binding();
				binding.XPath = ".";
				binding.StringFormat = ContentStringFormat;
				FrameworkElementFactory frameworkElementFactory = CreateTextBlockFactory();
				frameworkElementFactory.SetBinding(TextBlock.TextProperty, binding);
				dataTemplate = new DataTemplate();
				dataTemplate.VisualTree = frameworkElementFactory;
				dataTemplate.Seal();
				XMLFormattingTemplateField.SetValue(this, dataTemplate);
			}
			return dataTemplate;
		}
	}

	private DataTemplate Template
	{
		get
		{
			return _templateCache;
		}
		set
		{
			SetValue(TemplateProperty, value);
		}
	}

	private bool IsUsingDefaultStringTemplate
	{
		get
		{
			if (Template == StringContentTemplate || Template == AccessTextContentTemplate)
			{
				return true;
			}
			DataTemplate value = StringFormattingTemplateField.GetValue(this);
			if (value != null && value == Template)
			{
				return true;
			}
			value = AccessTextFormattingTemplateField.GetValue(this);
			if (value != null && value == Template)
			{
				return true;
			}
			return false;
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	static ContentPresenter()
	{
		RecognizesAccessKeyProperty = DependencyProperty.Register("RecognizesAccessKey", typeof(bool), typeof(ContentPresenter), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ContentProperty = ContentControl.ContentProperty.AddOwner(typeof(ContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnContentChanged));
		ContentTemplateProperty = ContentControl.ContentTemplateProperty.AddOwner(typeof(ContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnContentTemplateChanged));
		ContentTemplateSelectorProperty = ContentControl.ContentTemplateSelectorProperty.AddOwner(typeof(ContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnContentTemplateSelectorChanged));
		ContentStringFormatProperty = DependencyProperty.Register("ContentStringFormat", typeof(string), typeof(ContentPresenter), new FrameworkPropertyMetadata(null, OnContentStringFormatChanged));
		ContentSourceProperty = DependencyProperty.Register("ContentSource", typeof(string), typeof(ContentPresenter), new FrameworkPropertyMetadata("Content"));
		TemplateProperty = DependencyProperty.Register("Template", typeof(DataTemplate), typeof(ContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTemplateChanged));
		XMLFormattingTemplateField = new UncommonField<DataTemplate>();
		StringFormattingTemplateField = new UncommonField<DataTemplate>();
		AccessTextFormattingTemplateField = new UncommonField<DataTemplate>();
		DataTemplate dataTemplate = new DataTemplate();
		FrameworkElementFactory frameworkElementFactory = CreateAccessTextFactory();
		frameworkElementFactory.SetValue(AccessText.TextProperty, new TemplateBindingExtension(ContentProperty));
		dataTemplate.VisualTree = frameworkElementFactory;
		dataTemplate.Seal();
		s_AccessTextTemplate = dataTemplate;
		DataTemplate dataTemplate2 = new DataTemplate();
		frameworkElementFactory = CreateTextBlockFactory();
		frameworkElementFactory.SetValue(TextBlock.TextProperty, new TemplateBindingExtension(ContentProperty));
		dataTemplate2.VisualTree = frameworkElementFactory;
		dataTemplate2.Seal();
		s_StringTemplate = dataTemplate2;
		DataTemplate dataTemplate3 = new DataTemplate();
		frameworkElementFactory = CreateTextBlockFactory();
		frameworkElementFactory.SetBinding(binding: new Binding
		{
			XPath = "."
		}, dp: TextBlock.TextProperty);
		dataTemplate3.VisualTree = frameworkElementFactory;
		dataTemplate3.Seal();
		s_XmlNodeTemplate = dataTemplate3;
		UseContentTemplate useContentTemplate = new UseContentTemplate();
		useContentTemplate.Seal();
		s_UIElementTemplate = useContentTemplate;
		DefaultTemplate defaultTemplate = new DefaultTemplate();
		defaultTemplate.Seal();
		s_DefaultTemplate = defaultTemplate;
		s_DefaultTemplateSelector = new DefaultSelector();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ContentPresenter" /> class.</summary>
	public ContentPresenter()
	{
		Initialize();
	}

	private void Initialize()
	{
		PropertyMetadata metadata = TemplateProperty.GetMetadata(base.DependencyObjectType);
		DataTemplate dataTemplate = (DataTemplate)metadata.DefaultValue;
		if (dataTemplate != null)
		{
			OnTemplateChanged(this, new DependencyPropertyChangedEventArgs(TemplateProperty, metadata, null, dataTemplate));
		}
		base.DataContext = null;
	}

	private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentPresenter contentPresenter = (ContentPresenter)d;
		if (!contentPresenter._templateIsCurrent)
		{
			return;
		}
		bool flag;
		if (e.NewValue == BindingExpressionBase.DisconnectedItem)
		{
			flag = false;
		}
		else if (contentPresenter.ContentTemplate != null)
		{
			flag = false;
		}
		else if (contentPresenter.ContentTemplateSelector != null)
		{
			flag = true;
		}
		else if (contentPresenter.Template == UIElementContentTemplate)
		{
			flag = true;
			contentPresenter.Template = null;
		}
		else if (contentPresenter.Template == DefaultContentTemplate)
		{
			flag = true;
		}
		else
		{
			Type type;
			object obj = DataTypeForItem(e.OldValue, contentPresenter, out type);
			object obj2 = DataTypeForItem(e.NewValue, contentPresenter, out type);
			flag = obj != obj2;
			if (!flag && contentPresenter.RecognizesAccessKey && typeof(string) == obj2 && contentPresenter.IsUsingDefaultStringTemplate)
			{
				string obj3 = (string)e.OldValue;
				string text = (string)e.NewValue;
				bool num = obj3.IndexOf(AccessText.AccessKeyMarker) > -1;
				bool flag2 = text.IndexOf(AccessText.AccessKeyMarker) > -1;
				if (num != flag2)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			contentPresenter._templateIsCurrent = false;
		}
		if (contentPresenter._templateIsCurrent && contentPresenter.Template != UIElementContentTemplate)
		{
			contentPresenter.DataContext = e.NewValue;
		}
	}

	private static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentPresenter obj = (ContentPresenter)d;
		obj._templateIsCurrent = false;
		obj.OnContentTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> changes. </summary>
	/// <param name="oldContentTemplate">The old value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> property.</param>
	/// <param name="newContentTemplate">The new value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> property.</param>
	protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
	{
		Helper.CheckTemplateAndTemplateSelector("Content", ContentTemplateProperty, ContentTemplateSelectorProperty, this);
		Template = null;
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the effective value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeContentTemplateSelector()
	{
		return false;
	}

	private static void OnContentTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ContentPresenter obj = (ContentPresenter)d;
		obj._templateIsCurrent = false;
		obj.OnContentTemplateSelectorChanged((DataTemplateSelector)e.OldValue, (DataTemplateSelector)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> property changes. </summary>
	/// <param name="oldContentTemplateSelector">The old value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> property.</param>
	/// <param name="newContentTemplateSelector">The new value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplateSelector" /> property.</param>
	protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
	{
		Helper.CheckTemplateAndTemplateSelector("Content", ContentTemplateProperty, ContentTemplateSelectorProperty, this);
		Template = null;
	}

	private static void OnContentStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentPresenter)d).OnContentStringFormatChanged((string)e.OldValue, (string)e.NewValue);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ContentPresenter.ContentStringFormat" /> property changes.</summary>
	/// <param name="oldContentStringFormat">The old value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentStringFormat" /> property.</param>
	/// <param name="newContentStringFormat">The new value of the <see cref="P:System.Windows.Controls.ContentPresenter.ContentStringFormat" /> property.</param>
	protected virtual void OnContentStringFormatChanged(string oldContentStringFormat, string newContentStringFormat)
	{
		XMLFormattingTemplateField.ClearValue(this);
		StringFormattingTemplateField.ClearValue(this);
		AccessTextFormattingTemplateField.ClearValue(this);
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (base.TemplatedParent == null)
		{
			InvalidateProperty(ContentProperty);
		}
		if (_language != null && _language != base.Language)
		{
			_templateIsCurrent = false;
		}
		if (!_templateIsCurrent)
		{
			EnsureTemplate();
			_templateIsCurrent = true;
		}
	}

	/// <summary>Determines the size of the <see cref="T:System.Windows.Controls.ContentPresenter" /> object based on the sizing properties, margin, and requested size of the child content.</summary>
	/// <returns>The size that is required to arrange child content.</returns>
	/// <param name="constraint">An upper limit value that the return value should not exceed.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		return Helper.MeasureElementWithSingleChild(this, constraint);
	}

	/// <summary>Positions the single child element and determines the content of a <see cref="T:System.Windows.Controls.ContentPresenter" /> object. </summary>
	/// <returns>The actual size needed by the element.</returns>
	/// <param name="arrangeSize">The size that this <see cref="T:System.Windows.Controls.ContentPresenter" /> object should use to arrange its child element.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		return Helper.ArrangeElementWithSingleChild(this, arrangeSize);
	}

	/// <summary>Returns the template to use. This may depend on the content or other properties.</summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> to use.</returns>
	protected virtual DataTemplate ChooseTemplate()
	{
		DataTemplate dataTemplate = null;
		object content = Content;
		dataTemplate = ContentTemplate;
		if (dataTemplate == null && ContentTemplateSelector != null)
		{
			dataTemplate = ContentTemplateSelector.SelectTemplate(content, this);
		}
		if (dataTemplate == null)
		{
			dataTemplate = DefaultTemplateSelector.SelectTemplate(content, this);
		}
		return dataTemplate;
	}

	internal void PrepareContentPresenter(object item, DataTemplate itemTemplate, DataTemplateSelector itemTemplateSelector, string stringFormat)
	{
		if (item != this)
		{
			if (_contentIsItem || !HasNonDefaultValue(ContentProperty))
			{
				Content = item;
				_contentIsItem = true;
			}
			if (itemTemplate != null)
			{
				SetValue(ContentTemplateProperty, itemTemplate);
			}
			if (itemTemplateSelector != null)
			{
				SetValue(ContentTemplateSelectorProperty, itemTemplateSelector);
			}
			if (stringFormat != null)
			{
				SetValue(ContentStringFormatProperty, stringFormat);
			}
		}
	}

	internal void ClearContentPresenter(object item)
	{
		if (item != this && _contentIsItem)
		{
			Content = BindingExpressionBase.DisconnectedItem;
		}
	}

	internal static object DataTypeForItem(object item, DependencyObject target, out Type type)
	{
		if (item == null)
		{
			type = null;
			return null;
		}
		type = ReflectionHelper.GetReflectionType(item);
		object result;
		if (SystemXmlLinqHelper.IsXElement(item))
		{
			result = SystemXmlLinqHelper.GetXElementTagName(item);
			type = null;
		}
		else if (!SystemXmlHelper.IsXmlNode(item))
		{
			result = ((!(type == typeof(object))) ? type : null);
		}
		else
		{
			result = SystemXmlHelper.GetXmlTagName(item, target);
			type = null;
		}
		return result;
	}

	internal void ReevaluateTemplate()
	{
		if (Template != ChooseTemplate())
		{
			_templateIsCurrent = false;
			InvalidateMeasure();
		}
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		OnTemplateChanged((DataTemplate)oldTemplate, (DataTemplate)newTemplate);
	}

	private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StyleHelper.UpdateTemplateCache((ContentPresenter)d, (FrameworkTemplate)e.OldValue, (FrameworkTemplate)e.NewValue, TemplateProperty);
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ContentPresenter.ContentTemplate" /> changes </summary>
	/// <param name="oldTemplate">The old <see cref="T:System.Windows.DataTemplate" /> object value.</param>
	/// <param name="newTemplate">The new <see cref="T:System.Windows.DataTemplate" /> object value.</param>
	protected virtual void OnTemplateChanged(DataTemplate oldTemplate, DataTemplate newTemplate)
	{
	}

	private void EnsureTemplate()
	{
		DataTemplate template = Template;
		DataTemplate dataTemplate = null;
		_templateIsCurrent = false;
		while (!_templateIsCurrent)
		{
			_templateIsCurrent = true;
			dataTemplate = ChooseTemplate();
			if (template != dataTemplate)
			{
				Template = null;
			}
			if (dataTemplate != UIElementContentTemplate)
			{
				base.DataContext = Content;
			}
			else
			{
				ClearValue(FrameworkElement.DataContextProperty);
			}
		}
		Template = dataTemplate;
		if (template == dataTemplate)
		{
			StyleHelper.DoTemplateInvalidations(this, template);
		}
	}

	private DataTemplate SelectTemplateForString(string s)
	{
		string contentStringFormat = ContentStringFormat;
		if (RecognizesAccessKey && s.IndexOf(AccessText.AccessKeyMarker) > -1)
		{
			return string.IsNullOrEmpty(contentStringFormat) ? AccessTextContentTemplate : FormattingAccessTextContentTemplate;
		}
		return string.IsNullOrEmpty(contentStringFormat) ? StringContentTemplate : FormattingStringContentTemplate;
	}

	private DataTemplate SelectTemplateForXML()
	{
		if (!string.IsNullOrEmpty(ContentStringFormat))
		{
			return FormattingXmlNodeContentTemplate;
		}
		return XmlNodeContentTemplate;
	}

	internal static FrameworkElementFactory CreateAccessTextFactory()
	{
		return new FrameworkElementFactory(typeof(AccessText));
	}

	internal static FrameworkElementFactory CreateTextBlockFactory()
	{
		return new FrameworkElementFactory(typeof(TextBlock));
	}

	private static TextBlock CreateTextBlock(ContentPresenter container)
	{
		return new TextBlock();
	}

	private void CacheLanguage(XmlLanguage language)
	{
		_language = language;
	}
}
