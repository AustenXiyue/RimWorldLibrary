using System.Collections;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Encapsulates a page of content that can be navigated to and hosted by Windows Internet Explorer, <see cref="T:System.Windows.Navigation.NavigationWindow" />, and <see cref="T:System.Windows.Controls.Frame" />.</summary>
[ContentProperty("Content")]
public class Page : FrameworkElement, IWindowService, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.Content" /> dependency property.</summary>
	/// <returns>The identifier the <see cref="P:System.Windows.Controls.Page.Content" /> dependency property.</returns>
	public static readonly DependencyProperty ContentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.Background" /> dependency property.</summary>
	/// <returns>The identifier the <see cref="P:System.Windows.Controls.Page.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.Title" /> dependency property.</summary>
	public static readonly DependencyProperty TitleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.KeepAlive" /> dependency property.</summary>
	public static readonly DependencyProperty KeepAliveProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.Foreground" /> dependency property.</summary>
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.FontFamily" /> dependency property.</summary>
	public static readonly DependencyProperty FontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.FontSize" /> dependency property.</summary>
	public static readonly DependencyProperty FontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Page.Template" /> dependency property.</summary>
	/// <returns>The identifier the <see cref="P:System.Windows.Controls.Page.Template" /> dependency property.</returns>
	public static readonly DependencyProperty TemplateProperty;

	private IWindowService _currentIws;

	private PageHelperObject _pho;

	private SetPropertyFlags _setPropertyFlags;

	private bool _isTopLevel;

	private ControlTemplate _templateCache;

	private static DependencyObjectType _dType;

	/// <summary>Returns an enumerator for the logical child elements of a <see cref="T:System.Windows.Controls.Page" />.</summary>
	/// <returns>The <see cref="T:System.Collections.IEnumerator" /> for the logical child elements of a <see cref="T:System.Windows.Controls.Page" />.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			VerifyAccess();
			return new SingleChildEnumerator(Content);
		}
	}

	/// <summary>Gets or sets the content of a <see cref="T:System.Windows.Controls.Page" />.  </summary>
	/// <returns>An object that contains the content of a <see cref="T:System.Windows.Controls.Page" />. The default is <see cref="P:System.Windows.SystemFonts.MessageFontFamily" />.</returns>
	public object Content
	{
		get
		{
			VerifyAccess();
			return GetValue(ContentProperty);
		}
		set
		{
			VerifyAccess();
			SetValue(ContentProperty, value);
		}
	}

	string IWindowService.Title
	{
		get
		{
			VerifyAccess();
			if (WindowService == null)
			{
				throw new InvalidOperationException(SR.CannotQueryPropertiesWhenPageNotInTreeWithWindow);
			}
			return WindowService.Title;
		}
		set
		{
			VerifyAccess();
			if (WindowService == null)
			{
				PageHelperObject._windowTitle = value;
				PropertyIsSet(SetPropertyFlags.WindowTitle);
			}
			else if (_isTopLevel)
			{
				WindowService.Title = value;
				PropertyIsSet(SetPropertyFlags.WindowTitle);
			}
		}
	}

	/// <summary>Gets or sets the title of the host <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" /> of a <see cref="T:System.Windows.Controls.Page" />.</summary>
	/// <returns>The title of a window that directly hosts the <see cref="T:System.Windows.Controls.Page" />.</returns>
	[Localizability(LocalizationCategory.Title)]
	public string WindowTitle
	{
		get
		{
			VerifyAccess();
			return ((IWindowService)this).Title;
		}
		set
		{
			VerifyAccess();
			((IWindowService)this).Title = value;
		}
	}

	double IWindowService.Height
	{
		get
		{
			VerifyAccess();
			if (WindowService == null)
			{
				throw new InvalidOperationException(SR.CannotQueryPropertiesWhenPageNotInTreeWithWindow);
			}
			return WindowService.Height;
		}
		set
		{
			VerifyAccess();
			if (WindowService == null)
			{
				PageHelperObject._windowHeight = value;
				PropertyIsSet(SetPropertyFlags.WindowHeight);
			}
			else if (_isTopLevel)
			{
				if (!WindowService.UserResized)
				{
					WindowService.Height = value;
				}
				PropertyIsSet(SetPropertyFlags.WindowHeight);
			}
		}
	}

	/// <summary>Gets or sets the height of the host <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" /> of a <see cref="T:System.Windows.Controls.Page" />.</summary>
	/// <returns>The height of a window that directly hosts a <see cref="T:System.Windows.Controls.Page" />.</returns>
	public double WindowHeight
	{
		get
		{
			VerifyAccess();
			return ((IWindowService)this).Height;
		}
		set
		{
			VerifyAccess();
			((IWindowService)this).Height = value;
		}
	}

	double IWindowService.Width
	{
		get
		{
			VerifyAccess();
			if (WindowService == null)
			{
				throw new InvalidOperationException(SR.CannotQueryPropertiesWhenPageNotInTreeWithWindow);
			}
			return WindowService.Width;
		}
		set
		{
			VerifyAccess();
			if (WindowService == null)
			{
				PageHelperObject._windowWidth = value;
				PropertyIsSet(SetPropertyFlags.WindowWidth);
			}
			else if (_isTopLevel)
			{
				if (!WindowService.UserResized)
				{
					WindowService.Width = value;
				}
				PropertyIsSet(SetPropertyFlags.WindowWidth);
			}
		}
	}

	/// <summary>Gets or sets the width of the host <see cref="T:System.Windows.Window" /> or <see cref="T:System.Windows.Navigation.NavigationWindow" /> of a <see cref="T:System.Windows.Controls.Page" />.</summary>
	/// <returns>The width of a window that directly hosts a <see cref="T:System.Windows.Controls.Page" />.</returns>
	public double WindowWidth
	{
		get
		{
			VerifyAccess();
			return ((IWindowService)this).Width;
		}
		set
		{
			VerifyAccess();
			((IWindowService)this).Width = value;
		}
	}

	/// <summary>Gets or sets the background for a <see cref="T:System.Windows.Controls.Page" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that <see cref="T:System.Windows.Controls.Page" /> uses to paint its background.</returns>
	[Category("Appearance")]
	public Brush Background
	{
		get
		{
			return (Brush)GetValue(BackgroundProperty);
		}
		set
		{
			SetValue(BackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets the title of the <see cref="T:System.Windows.Controls.Page" />.  </summary>
	/// <returns>The title of the <see cref="T:System.Windows.Controls.Page" />.</returns>
	public string Title
	{
		get
		{
			return (string)GetValue(TitleProperty);
		}
		set
		{
			SetValue(TitleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the navigation UI of a <see cref="T:System.Windows.Navigation.NavigationWindow" /> on Microsoft Internet Explorer 6 is visible.</summary>
	/// <returns>true if the navigation UI of a host <see cref="T:System.Windows.Navigation.NavigationWindow" /> is visible; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.Page.ShowsNavigationUI" /> property is inspected on a <see cref="T:System.Windows.Controls.Page" /> instance that is not hosted by a <see cref="T:System.Windows.Window" />, <see cref="T:System.Windows.Navigation.NavigationWindow" />, or a browser.</exception>
	public bool ShowsNavigationUI
	{
		get
		{
			VerifyAccess();
			if (WindowService == null)
			{
				throw new InvalidOperationException(SR.CannotQueryPropertiesWhenPageNotInTreeWithWindow);
			}
			if (WindowService is NavigationWindow navigationWindow)
			{
				return navigationWindow.ShowsNavigationUI;
			}
			return false;
		}
		set
		{
			VerifyAccess();
			if (WindowService == null)
			{
				PageHelperObject._showsNavigationUI = value;
				PropertyIsSet(SetPropertyFlags.ShowsNavigationUI);
			}
			else if (_isTopLevel)
			{
				SetShowsNavigationUI(value);
				PropertyIsSet(SetPropertyFlags.ShowsNavigationUI);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.Page" /> instance is retained in navigation history.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Page" /> instance is retained in navigation history; otherwise, false. The default is false.</returns>
	public bool KeepAlive
	{
		get
		{
			return JournalEntry.GetKeepAlive(this);
		}
		set
		{
			JournalEntry.SetKeepAlive(this, value);
		}
	}

	/// <summary>Gets the navigation service that the host of the page is using to manage navigation.</summary>
	/// <returns>The <see cref="T:System.Windows.Navigation.NavigationService" /> object that the host of the page is using to manage navigation, or null if the host does not support navigation.</returns>
	public NavigationService NavigationService => NavigationService.GetNavigationService(this);

	/// <summary>Gets or sets the foreground for a <see cref="T:System.Windows.Controls.Page" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that <see cref="T:System.Windows.Controls.Page" /> uses to paint its foreground. The default is <see cref="P:System.Windows.Media.Brushes.Black" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public Brush Foreground
	{
		get
		{
			return (Brush)GetValue(ForegroundProperty);
		}
		set
		{
			SetValue(ForegroundProperty, value);
		}
	}

	/// <summary>Gets or sets the name of the specified font family.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.FontFamily" /> that is the font family for the content of a <see cref="T:System.Windows.Controls.Page" />. The default is <see cref="P:System.Windows.SystemFonts.MessageFontFamily" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.Font, Modifiability = Modifiability.Unmodifiable)]
	public FontFamily FontFamily
	{
		get
		{
			return (FontFamily)GetValue(FontFamilyProperty);
		}
		set
		{
			SetValue(FontFamilyProperty, value);
		}
	}

	/// <summary>Gets or sets the font size.  </summary>
	/// <returns>The font size for the content of a <see cref="T:System.Windows.Controls.Page" />. The default is <see cref="P:System.Windows.SystemFonts.MessageFontSize" />.</returns>
	[TypeConverter(typeof(FontSizeConverter))]
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.None)]
	public double FontSize
	{
		get
		{
			return (double)GetValue(FontSizeProperty);
		}
		set
		{
			SetValue(FontSizeProperty, value);
		}
	}

	/// <summary>Gets or sets the control template for a <see cref="T:System.Windows.Controls.Page" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ControlTemplate" /> for a <see cref="T:System.Windows.Controls.Page" />.</returns>
	public ControlTemplate Template
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

	internal override FrameworkTemplate TemplateInternal => Template;

	internal override FrameworkTemplate TemplateCache
	{
		get
		{
			return _templateCache;
		}
		set
		{
			_templateCache = (ControlTemplate)value;
		}
	}

	bool IWindowService.UserResized
	{
		get
		{
			Invariant.Assert(_currentIws != null, "_currentIws cannot be null here.");
			return _currentIws.UserResized;
		}
	}

	private IWindowService WindowService => _currentIws;

	private PageHelperObject PageHelperObject
	{
		get
		{
			if (_pho == null)
			{
				_pho = new PageHelperObject();
			}
			return _pho;
		}
	}

	internal override int EffectiveValuesInitialSize => 19;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static Page()
	{
		ContentProperty = ContentControl.ContentProperty.AddOwner(typeof(Page), new FrameworkPropertyMetadata(OnContentChanged));
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(Page), new FrameworkPropertyMetadata(Panel.BackgroundProperty.GetDefaultValue(typeof(Panel)), FrameworkPropertyMetadataOptions.None));
		TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Page), new FrameworkPropertyMetadata(null, OnTitleChanged));
		KeepAliveProperty = JournalEntry.KeepAliveProperty.AddOwner(typeof(Page));
		ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(Page));
		FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(Page));
		FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(Page));
		TemplateProperty = Control.TemplateProperty.AddOwner(typeof(Page), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTemplateChanged));
		Window.IWindowServiceProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(_OnWindowServiceChanged));
		UIElement.FocusableProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Page), new FrameworkPropertyMetadata(typeof(Page)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(Page));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Page" /> class.</summary>
	public Page()
	{
		PropertyMetadata metadata = TemplateProperty.GetMetadata(base.DependencyObjectType);
		ControlTemplate controlTemplate = (ControlTemplate)metadata.DefaultValue;
		if (controlTemplate != null)
		{
			OnTemplateChanged(this, new DependencyPropertyChangedEventArgs(TemplateProperty, metadata, null, controlTemplate));
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddChild(System.Object)" />.</summary>
	/// <param name="obj">The child object to add.</param>
	void IAddChild.AddChild(object obj)
	{
		VerifyAccess();
		if (Content == null || obj == null)
		{
			Content = obj;
			return;
		}
		throw new InvalidOperationException(SR.PageCannotHaveMultipleContent);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Windows.Markup.IAddChild.AddText(System.String)" />.</summary>
	/// <param name="str">The text to add to the object.</param>
	void IAddChild.AddText(string str)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(str, this);
	}

	internal bool ShouldJournalWindowTitle()
	{
		return IsPropertySet(SetPropertyFlags.WindowTitle);
	}

	private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Page)d).PropertyIsSet(SetPropertyFlags.Title);
	}

	internal override void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		OnTemplateChanged((ControlTemplate)oldTemplate, (ControlTemplate)newTemplate);
	}

	private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StyleHelper.UpdateTemplateCache((Page)d, (FrameworkTemplate)e.OldValue, (FrameworkTemplate)e.NewValue, TemplateProperty);
	}

	/// <summary>Called when the template for a <see cref="T:System.Windows.Controls.Page" /> changes.</summary>
	/// <param name="oldTemplate">The old template.</param>
	/// <param name="newTemplate">The new template. </param>
	protected virtual void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
	}

	/// <summary>Measures the child elements of the <see cref="T:System.Windows.Controls.Page" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that is the actual size of the window. The method may return a larger value, in which case the parent may need to add scroll bars.</returns>
	/// <param name="constraint">The available area that the window can give to its children.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		VerifyAccess();
		if (VisualChildrenCount > 0 && GetVisualChild(0) is UIElement uIElement)
		{
			uIElement.Measure(constraint);
			return uIElement.DesiredSize;
		}
		return new Size(0.0, 0.0);
	}

	/// <summary>Arranges the content (child elements) of the <see cref="T:System.Windows.Controls.Page" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Size" /> that represents the arranged size of the page.</returns>
	/// <param name="arrangeBounds">The size to use to arrange the child elements.</param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		VerifyAccess();
		if (VisualChildrenCount > 0 && GetVisualChild(0) is UIElement uIElement)
		{
			uIElement.Arrange(new Rect(default(Point), arrangeBounds));
		}
		return arrangeBounds;
	}

	/// <summary>Called when the parent of the <see cref="T:System.Windows.Controls.Page" /> is changed.</summary>
	/// <param name="oldParent">The previous parent. Set to null if the <see cref="T:System.Windows.DependencyObject" /> did not have a previous parent.</param>
	/// <exception cref="T:System.InvalidOperationException">The new parent is neither a <see cref="T:System.Windows.Window" /> nor a <see cref="T:System.Windows.Controls.Frame" />.</exception>
	protected internal sealed override void OnVisualParentChanged(DependencyObject oldParent)
	{
		VerifyAccess();
		base.OnVisualParentChanged(oldParent);
		if (!(VisualTreeHelper.GetParent(this) is Visual visual) || base.Parent is Window || (NavigationService != null && NavigationService.Content == this))
		{
			return;
		}
		bool flag = false;
		FrameworkElement frameworkElement = visual as FrameworkElement;
		if (frameworkElement != null)
		{
			DependencyObject dependencyObject = frameworkElement;
			while (frameworkElement != null && frameworkElement.TemplatedParent != null)
			{
				dependencyObject = frameworkElement.TemplatedParent;
				frameworkElement = dependencyObject as FrameworkElement;
				if (frameworkElement is Frame)
				{
					break;
				}
			}
			if (dependencyObject is Window || dependencyObject is Frame)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		throw new InvalidOperationException(SR.ParentOfPageMustBeWindowOrFrame);
	}

	private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Page)d).OnContentChanged(e.OldValue, e.NewValue);
	}

	private void OnContentChanged(object oldContent, object newContent)
	{
		RemoveLogicalChild(oldContent);
		AddLogicalChild(newContent);
	}

	private static void _OnWindowServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as Page).OnWindowServiceChanged(e.NewValue as IWindowService);
	}

	private void OnWindowServiceChanged(IWindowService iws)
	{
		_currentIws = iws;
		DetermineTopLevel();
		if (_currentIws != null && _isTopLevel)
		{
			PropagateProperties();
		}
	}

	private void DetermineTopLevel()
	{
		if (base.Parent is FrameworkElement { InheritanceBehavior: InheritanceBehavior.Default })
		{
			_isTopLevel = true;
		}
		else
		{
			_isTopLevel = false;
		}
	}

	private void PropagateProperties()
	{
		if (_pho != null)
		{
			if (IsPropertySet(SetPropertyFlags.WindowTitle))
			{
				_currentIws.Title = PageHelperObject._windowTitle;
			}
			if (IsPropertySet(SetPropertyFlags.WindowHeight) && !_currentIws.UserResized)
			{
				_currentIws.Height = PageHelperObject._windowHeight;
			}
			if (IsPropertySet(SetPropertyFlags.WindowWidth) && !_currentIws.UserResized)
			{
				_currentIws.Width = PageHelperObject._windowWidth;
			}
			if (IsPropertySet(SetPropertyFlags.ShowsNavigationUI))
			{
				SetShowsNavigationUI(PageHelperObject._showsNavigationUI);
			}
		}
	}

	private void SetShowsNavigationUI(bool showsNavigationUI)
	{
		if (_currentIws is NavigationWindow navigationWindow)
		{
			navigationWindow.ShowsNavigationUI = showsNavigationUI;
		}
	}

	private bool IsPropertySet(SetPropertyFlags property)
	{
		return (_setPropertyFlags & property) != 0;
	}

	private void PropertyIsSet(SetPropertyFlags property)
	{
		_setPropertyFlags |= property;
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.Page.WindowTitle" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeWindowTitle()
	{
		return IsPropertySet(SetPropertyFlags.WindowTitle);
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.Page.WindowHeight" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeWindowHeight()
	{
		return IsPropertySet(SetPropertyFlags.WindowHeight);
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.Page.WindowWidth" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeWindowWidth()
	{
		return IsPropertySet(SetPropertyFlags.WindowWidth);
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.Page.Title" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeTitle()
	{
		return IsPropertySet(SetPropertyFlags.Title);
	}

	/// <summary>Allows derived classes to determine the serialization behavior of the <see cref="P:System.Windows.Controls.Page.ShowsNavigationUI" /> property.</summary>
	/// <returns>true if the content should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeShowsNavigationUI()
	{
		return IsPropertySet(SetPropertyFlags.ShowsNavigationUI);
	}
}
