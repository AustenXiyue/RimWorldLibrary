using System.Collections;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using MS.Internal.Utility;

namespace System.Windows.Documents;

/// <summary>Provides the content for a high fidelity, fixed-format page. </summary>
[ContentProperty("Children")]
public sealed class FixedPage : FrameworkElement, IAddChildInternal, IAddChild, IFixedNavigate, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.PrintTicket" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.PrintTicket" /> dependency property.</returns>
	public static readonly DependencyProperty PrintTicketProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.Background" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.Background" /> dependency property.</returns>
	public static readonly DependencyProperty BackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.Left" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.Left" /> attached property.</returns>
	public static readonly DependencyProperty LeftProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.Top" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.Top" /> attached property.</returns>
	public static readonly DependencyProperty TopProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.Right" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.Right" /> dependency property.</returns>
	public static readonly DependencyProperty RightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.Bottom" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.Bottom" /> attached property.</returns>
	public static readonly DependencyProperty BottomProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.ContentBox" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.ContentBox" /> dependency property.</returns>
	public static readonly DependencyProperty ContentBoxProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.BleedBox" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.DocumentPage.BleedBox" /> dependency property.</returns>
	public static readonly DependencyProperty BleedBoxProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.FixedPage.NavigateUri" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.FixedPage.NavigateUri" /> attached property.</returns>
	public static readonly DependencyProperty NavigateUriProperty;

	private string _startPartUriString;

	private UIElementCollection _uiElementCollection;

	/// <summary>Gets or sets the base URI of the current application context. </summary>
	/// <returns>The base URI of the application context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return (Uri)GetValue(BaseUriHelper.BaseUriProperty);
		}
		set
		{
			SetValue(BaseUriHelper.BaseUriProperty, value);
		}
	}

	protected internal override IEnumerator LogicalChildren => Children.GetEnumerator();

	/// <summary>Gets a collection of the <see cref="T:System.Windows.Documents.FixedPage" /> child elements. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.UIElementCollection" /> of the child elements.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public UIElementCollection Children
	{
		get
		{
			if (_uiElementCollection == null)
			{
				_uiElementCollection = CreateUIElementCollection(this);
			}
			return _uiElementCollection;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Printing.PrintTicket" /> that is associated with the page.  </summary>
	/// <returns>The <see cref="T:System.Printing.PrintTicket" /> for the page.</returns>
	public object PrintTicket
	{
		get
		{
			return GetValue(PrintTicketProperty);
		}
		set
		{
			SetValue(PrintTicketProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used for rendering the page background.   </summary>
	/// <returns>The brush for rendering the page background.</returns>
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

	/// <summary>Gets or sets the bounding rectangle of the content area; that is, the area of the page within the margins, if any.  </summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> that defines the content area.</returns>
	public Rect ContentBox
	{
		get
		{
			return (Rect)GetValue(ContentBoxProperty);
		}
		set
		{
			SetValue(ContentBoxProperty, value);
		}
	}

	/// <summary>Gets or sets a rectangle defining the overflow area for bleeds, registration marks, and crop marks.  </summary>
	/// <returns>The <see cref="T:System.Windows.Rect" /> defining the overflow area.</returns>
	public Rect BleedBox
	{
		get
		{
			return (Rect)GetValue(BleedBoxProperty);
		}
		set
		{
			SetValue(BleedBoxProperty, value);
		}
	}

	protected override int VisualChildrenCount
	{
		get
		{
			if (_uiElementCollection == null)
			{
				return 0;
			}
			return _uiElementCollection.Count;
		}
	}

	internal string StartPartUriString
	{
		get
		{
			return _startPartUriString;
		}
		set
		{
			_startPartUriString = value;
		}
	}

	static FixedPage()
	{
		PrintTicketProperty = DependencyProperty.RegisterAttached("PrintTicket", typeof(object), typeof(FixedPage), new FrameworkPropertyMetadata((object)null));
		BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(FixedPage), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));
		LeftProperty = DependencyProperty.RegisterAttached("Left", typeof(double), typeof(FixedPage), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));
		TopProperty = DependencyProperty.RegisterAttached("Top", typeof(double), typeof(FixedPage), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));
		RightProperty = DependencyProperty.RegisterAttached("Right", typeof(double), typeof(FixedPage), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));
		BottomProperty = DependencyProperty.RegisterAttached("Bottom", typeof(double), typeof(FixedPage), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange));
		ContentBoxProperty = DependencyProperty.Register("ContentBox", typeof(Rect), typeof(FixedPage), new FrameworkPropertyMetadata(Rect.Empty));
		BleedBoxProperty = DependencyProperty.Register("BleedBox", typeof(Rect), typeof(FixedPage), new FrameworkPropertyMetadata(Rect.Empty));
		NavigateUriProperty = DependencyProperty.RegisterAttached("NavigateUri", typeof(Uri), typeof(FixedPage), new FrameworkPropertyMetadata(null, Hyperlink.OnNavigateUriChanged, Hyperlink.CoerceNavigateUri));
		FrameworkPropertyMetadata typeMetadata = new FrameworkPropertyMetadata(FlowDirection.LeftToRight, FrameworkPropertyMetadataOptions.AffectsParentArrange)
		{
			CoerceValueCallback = CoerceFlowDirection
		};
		FrameworkElement.FlowDirectionProperty.OverrideMetadata(typeof(FixedPage), typeMetadata);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.FixedPage" /> class. </summary>
	public FixedPage()
	{
		Init();
	}

	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new FixedPageAutomationPeer(this);
	}

	protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
	{
	}

	protected override void OnRender(DrawingContext dc)
	{
		Brush background = Background;
		if (background != null)
		{
			dc.DrawRectangle(background, null, new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
		}
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is UIElement element))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(UIElement)), "value");
		}
		Children.Add(element);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Returns the distance between the left side of an element and the left side of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <returns>The distance between the right side of an element and the right side of its parent canvas.</returns>
	/// <param name="element">The element from which to get the left offset.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetLeft(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(LeftProperty);
	}

	/// <summary>Sets the distance between the left side of an element and the left side of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <param name="element">The element on which to set the left offset.</param>
	/// <param name="length">The new distance between the left side of the element and the left side of its parent canvas.</param>
	public static void SetLeft(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(LeftProperty, length);
	}

	/// <summary>Returns the distance between the top of an element and the top of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <returns>The distance between the top of an element and the top of its parent canvas.</returns>
	/// <param name="element">The element from which to get the top offset.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetTop(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(TopProperty);
	}

	/// <summary>Sets the distance between the top of an element and the top of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <param name="element">The element on which to set the top offset.</param>
	/// <param name="length">The new distance between the top side of the element and the top side of its parent canvas.</param>
	public static void SetTop(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TopProperty, length);
	}

	/// <summary>Returns the distance between the right side of an element and the right side of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <returns>The distance between the right side of an element and the right side of its parent canvas.</returns>
	/// <param name="element">The element from which to get the right offset.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetRight(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(RightProperty);
	}

	/// <summary>Sets the distance between the right side of an element and the right side of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <param name="element">The element on which to set the right offset.</param>
	/// <param name="length">The new distance between the right side of the element and the right side of its parent canvas.</param>
	public static void SetRight(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(RightProperty, length);
	}

	/// <summary>Returns the distance between the bottom of an element and the bottom of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <returns>The distance between the bottom of an element and the bottom of its parent canvas.</returns>
	/// <param name="element">The element from which to get the bottom offset.</param>
	[TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
	[AttachedPropertyBrowsableForChildren]
	public static double GetBottom(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(BottomProperty);
	}

	/// <summary>Sets the distance between the bottom of an element and the bottom of its parent <see cref="T:System.Windows.Controls.Canvas" />.</summary>
	/// <param name="element">The element on which to set the bottom offset.</param>
	/// <param name="length">The new distance between the bottom side of the element and the bottom side of its parent canvas.</param>
	public static void SetBottom(UIElement element, double length)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(BottomProperty, length);
	}

	/// <summary>Returns the <see cref="P:System.Windows.Documents.FixedPage.NavigateUri" /> property for a given element.</summary>
	/// <returns>The <see cref="T:System.Uri" /> of <paramref name="element" />.</returns>
	/// <param name="element">The element from which to get the property.</param>
	[AttachedPropertyBrowsableForChildren]
	public static Uri GetNavigateUri(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Uri)element.GetValue(NavigateUriProperty);
	}

	/// <summary>Sets the uniform resource identifier (URI) to navigate to when a hyperlink is clicked.</summary>
	/// <param name="element">The element on which to set the URI offset.</param>
	/// <param name="uri">The URI to navigate to when a hyperlink is clicked.</param>
	public static void SetNavigateUri(UIElement element, Uri uri)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(NavigateUriProperty, uri);
	}

	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		base.OnVisualParentChanged(oldParent);
		if (oldParent == null)
		{
			HighlightVisual highlightVisual = HighlightVisual.GetHighlightVisual(this);
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
			if (highlightVisual == null && adornerLayer != null && LogicalTreeHelper.GetParent(this) is PageContent current && LogicalTreeHelper.GetParent(current) is FixedDocument panel && adornerLayer != null)
			{
				int zOrder = 1073741823;
				adornerLayer.Add(new HighlightVisual(panel, this), zOrder);
			}
		}
	}

	private static object CoerceFlowDirection(DependencyObject page, object flowDirection)
	{
		return FlowDirection.LeftToRight;
	}

	internal static Uri GetLinkUri(IInputElement element, Uri inputUri)
	{
		DependencyObject dependencyObject = element as DependencyObject;
		if (inputUri != null)
		{
			Uri uri = inputUri;
			if (!inputUri.IsAbsoluteUri)
			{
				uri = new Uri(new Uri("http://microsoft.com/"), inputUri);
			}
			string fragment = uri.Fragment;
			int num;
			if (fragment != null)
			{
				num = fragment.Length;
				if (num != 0)
				{
					string text = inputUri.ToString();
					inputUri = new Uri(text.Substring(0, text.IndexOf('#')), UriKind.RelativeOrAbsolute);
					if (!inputUri.IsAbsoluteUri)
					{
						string startPartUriString = GetStartPartUriString(dependencyObject);
						if (startPartUriString != null)
						{
							inputUri = new Uri(startPartUriString, UriKind.RelativeOrAbsolute);
						}
					}
				}
			}
			else
			{
				num = 0;
			}
			Uri baseUri = BaseUriHelper.GetBaseUri(dependencyObject);
			Uri uri2 = MS.Internal.Utility.BindUriHelper.GetUriToNavigate(dependencyObject, baseUri, inputUri);
			if (num != 0)
			{
				uri2 = new Uri(uri2.ToString() + fragment, UriKind.RelativeOrAbsolute);
			}
			return uri2;
		}
		return null;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (_uiElementCollection == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _uiElementCollection[index];
	}

	private UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
	{
		return new UIElementCollection(this, logicalParent);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
		foreach (UIElement child in Children)
		{
			child.Measure(availableSize);
		}
		return default(Size);
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		foreach (UIElement child in Children)
		{
			double x = 0.0;
			double y = 0.0;
			double left = GetLeft(child);
			if (!double.IsNaN(left))
			{
				x = left;
			}
			else
			{
				double right = GetRight(child);
				if (!double.IsNaN(right))
				{
					x = arrangeSize.Width - child.DesiredSize.Width - right;
				}
			}
			double top = GetTop(child);
			if (!double.IsNaN(top))
			{
				y = top;
			}
			else
			{
				double bottom = GetBottom(child);
				if (!double.IsNaN(bottom))
				{
					y = arrangeSize.Height - child.DesiredSize.Height - bottom;
				}
			}
			child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
		}
		return arrangeSize;
	}

	void IFixedNavigate.NavigateAsync(string elementID)
	{
		FixedHyperLink.NavigateToElement(this, elementID);
	}

	UIElement IFixedNavigate.FindElementByID(string elementID, out FixedPage rootFixedPage)
	{
		UIElement result = null;
		rootFixedPage = this;
		UIElementCollection children = Children;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode(children[i], elementID);
			if (dependencyObject != null)
			{
				result = dependencyObject as UIElement;
				break;
			}
		}
		return result;
	}

	internal FixedNode CreateFixedNode(int pageIndex, UIElement e)
	{
		return _CreateFixedNode(pageIndex, e);
	}

	internal Glyphs GetGlyphsElement(FixedNode node)
	{
		return GetElement(node) as Glyphs;
	}

	internal DependencyObject GetElement(FixedNode node)
	{
		int num = node[1];
		if (num < 0 || num > Children.Count)
		{
			return null;
		}
		DependencyObject dependencyObject = Children[num];
		for (int i = 2; i <= node.ChildLevels; i++)
		{
			num = node[i];
			if (dependencyObject is Canvas)
			{
				dependencyObject = ((Canvas)dependencyObject).Children[num];
				continue;
			}
			IEnumerable children = LogicalTreeHelper.GetChildren(dependencyObject);
			if (children == null)
			{
				return null;
			}
			int num2 = -1;
			IEnumerator enumerator = children.GetEnumerator();
			while (enumerator.MoveNext())
			{
				num2++;
				if (num2 == num)
				{
					dependencyObject = (DependencyObject)enumerator.Current;
					break;
				}
			}
		}
		return dependencyObject;
	}

	private void Init()
	{
		if (XpsValidatingLoader.DocumentMode)
		{
			base.InheritanceBehavior = InheritanceBehavior.SkipAllNext;
		}
	}

	internal StoryFragments GetPageStructure()
	{
		return FixedDocument.GetStoryFragments(this);
	}

	internal int[] _CreateChildIndex(DependencyObject e)
	{
		ArrayList arrayList = new ArrayList();
		while (e != this)
		{
			DependencyObject parent = LogicalTreeHelper.GetParent(e);
			int num = -1;
			if (parent is FixedPage)
			{
				num = ((FixedPage)parent).Children.IndexOf((UIElement)e);
			}
			else if (parent is Canvas)
			{
				num = ((Canvas)parent).Children.IndexOf((UIElement)e);
			}
			else
			{
				IEnumerator enumerator = LogicalTreeHelper.GetChildren(parent).GetEnumerator();
				while (enumerator.MoveNext())
				{
					num++;
					if (enumerator.Current == e)
					{
						break;
					}
				}
			}
			arrayList.Insert(0, num);
			e = parent;
		}
		while (e != this)
		{
		}
		return (int[])arrayList.ToArray(typeof(int));
	}

	private FixedNode _CreateFixedNode(int pageIndex, UIElement e)
	{
		return FixedNode.Create(pageIndex, _CreateChildIndex(e));
	}

	private static string GetStartPartUriString(DependencyObject current)
	{
		DependencyObject dependencyObject = current;
		FixedPage fixedPage = current as FixedPage;
		while (fixedPage == null && dependencyObject != null)
		{
			dependencyObject = dependencyObject.InheritanceParent;
			fixedPage = dependencyObject as FixedPage;
		}
		if (fixedPage == null)
		{
			return null;
		}
		if (fixedPage.StartPartUriString == null)
		{
			for (DependencyObject parent = LogicalTreeHelper.GetParent(current); parent != null; parent = LogicalTreeHelper.GetParent(parent))
			{
				if (parent is FixedDocumentSequence { BaseUri: var baseUri })
				{
					if (baseUri != null)
					{
						string text = baseUri.ToString();
						string fragment = baseUri.Fragment;
						if (fragment != null && fragment.Length != 0)
						{
							fixedPage.StartPartUriString = text.Substring(0, text.IndexOf('#'));
						}
						else
						{
							fixedPage.StartPartUriString = baseUri.ToString();
						}
					}
					break;
				}
			}
			if (fixedPage.StartPartUriString == null)
			{
				fixedPage.StartPartUriString = string.Empty;
			}
		}
		if (fixedPage.StartPartUriString == string.Empty)
		{
			return null;
		}
		return fixedPage.StartPartUriString;
	}
}
