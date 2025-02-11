using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace System.Windows.Documents;

internal sealed class FixedElement : DependencyObject
{
	internal enum ElementType
	{
		Paragraph,
		Inline,
		Run,
		Span,
		Bold,
		Italic,
		Underline,
		Object,
		Container,
		Section,
		Figure,
		Table,
		TableRowGroup,
		TableRow,
		TableCell,
		List,
		ListItem,
		Header,
		Footer,
		Hyperlink,
		InlineUIContainer
	}

	public static readonly DependencyProperty LanguageProperty = FrameworkElement.LanguageProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty FlowDirectionProperty = FrameworkElement.FlowDirectionProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty CellSpacingProperty = Table.CellSpacingProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty BorderThicknessProperty = Block.BorderThicknessProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty BorderBrushProperty = Block.BorderBrushProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty ColumnSpanProperty = TableCell.ColumnSpanProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty NavigateUriProperty = Hyperlink.NavigateUriProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty NameProperty = AutomationProperties.NameProperty.AddOwner(typeof(FixedElement));

	public static readonly DependencyProperty HelpTextProperty = AutomationProperties.HelpTextProperty.AddOwner(typeof(FixedElement));

	private ElementType _type;

	private FixedTextPointer _start;

	private FixedTextPointer _end;

	private object _object;

	private int _pageIndex;

	internal bool IsTextElement
	{
		get
		{
			if (_type != ElementType.Object)
			{
				return _type != ElementType.Container;
			}
			return false;
		}
	}

	internal Type Type => _type switch
	{
		ElementType.Paragraph => typeof(Paragraph), 
		ElementType.Inline => typeof(Inline), 
		ElementType.Run => typeof(Run), 
		ElementType.Span => typeof(Span), 
		ElementType.Bold => typeof(Bold), 
		ElementType.Italic => typeof(Italic), 
		ElementType.Underline => typeof(Underline), 
		ElementType.Object => typeof(object), 
		ElementType.Table => typeof(Table), 
		ElementType.TableRowGroup => typeof(TableRowGroup), 
		ElementType.TableRow => typeof(TableRow), 
		ElementType.TableCell => typeof(TableCell), 
		ElementType.List => typeof(List), 
		ElementType.ListItem => typeof(ListItem), 
		ElementType.Section => typeof(Section), 
		ElementType.Figure => typeof(Figure), 
		ElementType.Hyperlink => typeof(Hyperlink), 
		ElementType.InlineUIContainer => typeof(InlineUIContainer), 
		_ => typeof(object), 
	};

	internal FixedTextPointer Start => _start;

	internal FixedTextPointer End => _end;

	internal int PageIndex => _pageIndex;

	internal object Object
	{
		set
		{
			_object = value;
		}
	}

	internal FixedElement(ElementType type, FixedTextPointer start, FixedTextPointer end, int pageIndex)
	{
		_type = type;
		_start = start;
		_end = end;
		_pageIndex = pageIndex;
	}

	internal void Append(FixedElement e)
	{
		if (_type == ElementType.InlineUIContainer)
		{
			_object = e._object;
		}
	}

	internal object GetObject()
	{
		if (_type == ElementType.Hyperlink || _type == ElementType.Paragraph || (_type >= ElementType.Table && _type <= ElementType.TableCell))
		{
			if (_object == null)
			{
				_object = BuildObjectTree();
			}
			return _object;
		}
		if (_type != ElementType.Object && _type != ElementType.InlineUIContainer)
		{
			return null;
		}
		Image image = GetImage();
		object result = image;
		if (_type == ElementType.InlineUIContainer)
		{
			result = new InlineUIContainer
			{
				Child = image
			};
		}
		return result;
	}

	internal object BuildObjectTree()
	{
		IAddChild addChild;
		switch (_type)
		{
		case ElementType.Table:
			addChild = new Table();
			break;
		case ElementType.TableRowGroup:
			addChild = new TableRowGroup();
			break;
		case ElementType.TableRow:
			addChild = new TableRow();
			break;
		case ElementType.TableCell:
			addChild = new TableCell();
			break;
		case ElementType.Paragraph:
			addChild = new Paragraph();
			break;
		case ElementType.Hyperlink:
		{
			Hyperlink hyperlink = new Hyperlink();
			hyperlink.NavigateUri = GetValue(NavigateUriProperty) as Uri;
			hyperlink.RequestNavigate += ClickHyperlink;
			AutomationProperties.SetHelpText(hyperlink, (string)GetValue(HelpTextProperty));
			AutomationProperties.SetName(hyperlink, (string)GetValue(NameProperty));
			addChild = hyperlink;
			break;
		}
		default:
			addChild = null;
			break;
		}
		ITextPointer textPointer = ((ITextPointer)_start).CreatePointer();
		while (textPointer.CompareTo(_end) < 0)
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
				addChild.AddText(textPointer.GetTextInRun(LogicalDirection.Forward));
				break;
			case TextPointerContext.EmbeddedElement:
				addChild.AddChild(textPointer.GetAdjacentElement(LogicalDirection.Forward));
				break;
			case TextPointerContext.ElementStart:
			{
				object adjacentElement = textPointer.GetAdjacentElement(LogicalDirection.Forward);
				if (adjacentElement != null)
				{
					addChild.AddChild(adjacentElement);
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					textPointer.MoveToElementEdge(ElementEdge.BeforeEnd);
				}
				break;
			}
			}
			textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		return addChild;
	}

	private Image GetImage()
	{
		Image image = null;
		Uri uri = _object as Uri;
		if (uri != null)
		{
			image = new Image();
			image.Source = new BitmapImage(uri);
			image.Width = image.Source.Width;
			image.Height = image.Source.Height;
			AutomationProperties.SetName(image, (string)GetValue(NameProperty));
			AutomationProperties.SetHelpText(image, (string)GetValue(HelpTextProperty));
		}
		return image;
	}

	private void ClickHyperlink(object sender, RequestNavigateEventArgs args)
	{
		FixedDocument fixedDocument = _start.FixedTextContainer.FixedDocument;
		int pageNumber = fixedDocument.GetPageNumber(_start);
		Hyperlink.RaiseNavigate(fixedDocument.SyncGetPage(pageNumber, forceReload: false), args.Uri, null);
	}
}
