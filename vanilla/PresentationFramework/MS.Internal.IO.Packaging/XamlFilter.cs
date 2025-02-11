#define TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class XamlFilter : IManagedFilter
{
	internal enum FilterState
	{
		Uninitialized = 1,
		FindNextUnit,
		FindNextFlowUnit,
		UseContentExtractor,
		EndOfStream
	}

	[Flags]
	internal enum AttributesToIgnore
	{
		None = 0,
		Title = 1,
		Content = 2
	}

	private const string _inDocumentCodeURI = "http://schemas.microsoft.com/winfx/2006/xaml";

	private const string _pageContentName = "PageContent";

	private const string _glyphRunName = "Glyphs";

	private const string _pageContentSourceAttribute = "Source";

	private const string _fixedPageName = "FixedPage";

	private const string _xmlLangAttribute = "xml:lang";

	private const string _paragraphSeparator = "\u2029";

	private const string _unicodeStringAttribute = "UnicodeString";

	private readonly ContentDescriptor _defaultContentDescriptor = new ContentDescriptor(hasIndexableContent: true, isInline: false, null, null);

	private readonly ContentDescriptor _nonIndexableElementDescriptor = new ContentDescriptor(hasIndexableContent: false);

	private static readonly ManagedFullPropSpec _propSpec = new ManagedFullPropSpec(IndexingFilterMarshaler.PSGUID_STORAGE, 19u);

	private Stack _contextStack;

	private FilterState _filterState;

	private string _currentContent;

	private uint _currentChunkID;

	private int _countOfCharactersReturned;

	private IndexingContentUnit _indexingContentUnit;

	private bool _expectingBlockStart;

	private XmlReader _topLevelReader;

	private FixedPageContentExtractor _fixedPageContentExtractor;

	private XmlDocument _fixedPageDomTree;

	private Stream _xamlStream;

	private bool _filterContents;

	private bool _returnCanonicalParagraphBreaks;

	private XmlReader _xamlReader;

	private AttributesToIgnore _attributesToIgnore;

	private Hashtable _xamlElementContentDescriptorDictionary;

	private Dictionary<string, uint> _lcidDictionary;

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddPresentationDescriptor(string Key)
	{
		_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", Key), new ContentDescriptor(hasIndexableContent: true, isInline: false, null, null));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void AddPresentationDescriptor(string Key, string Value)
	{
		_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", Key), new ContentDescriptor(hasIndexableContent: true, isInline: false, Value, null));
	}

	private void InitElementDictionary()
	{
		if (_xamlElementContentDescriptorDictionary == null)
		{
			_xamlElementContentDescriptorDictionary = new Hashtable(300);
			AddPresentationDescriptor("TextBox", "Text");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Italic"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("GridViewColumnHeader", "Content");
			AddPresentationDescriptor("Canvas");
			AddPresentationDescriptor("ListBox");
			AddPresentationDescriptor("ItemsControl");
			AddPresentationDescriptor("AdornerDecorator");
			AddPresentationDescriptor("ComponentResourceKey");
			AddPresentationDescriptor("Button", "Content");
			AddPresentationDescriptor("FrameworkRichTextComposition", "Text");
			AddPresentationDescriptor("LinkTarget");
			AddPresentationDescriptor("TextBlock", "Text");
			AddPresentationDescriptor("DataTemplateSelector");
			AddPresentationDescriptor("MediaElement");
			AddPresentationDescriptor("PrintDialogException");
			AddPresentationDescriptor("DialogResultConverter");
			AddPresentationDescriptor("ComboBoxItem", "Content");
			AddPresentationDescriptor("AttachedPropertyBrowsableForChildrenAttribute");
			AddPresentationDescriptor("RowDefinition");
			AddPresentationDescriptor("TextSearch");
			AddPresentationDescriptor("DocumentReference");
			AddPresentationDescriptor("GridViewColumn");
			AddPresentationDescriptor("ValidationError");
			AddPresentationDescriptor("PasswordBox");
			AddPresentationDescriptor("InkCanvas");
			AddPresentationDescriptor("DataTrigger");
			AddPresentationDescriptor("TemplatePartAttribute");
			AddPresentationDescriptor("BlockUIContainer");
			AddPresentationDescriptor("LengthConverter");
			AddPresentationDescriptor("TextChange");
			AddPresentationDescriptor("Decorator");
			AddPresentationDescriptor("ToolTip", "Content");
			AddPresentationDescriptor("FigureLengthConverter");
			AddPresentationDescriptor("ValidationResult");
			AddPresentationDescriptor("ContentControl", "Content");
			AddPresentationDescriptor("CornerRadiusConverter");
			AddPresentationDescriptor("JournalEntryListConverter");
			AddPresentationDescriptor("ToggleButton", "Content");
			AddPresentationDescriptor("Paragraph");
			AddPresentationDescriptor("HeaderedContentControl", "Content");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "LineBreak"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Window"), new ContentDescriptor(hasIndexableContent: true, isInline: false, "Content", "Title"));
			AddPresentationDescriptor("StyleSelector");
			AddPresentationDescriptor("FixedPage");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Path"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			AddPresentationDescriptor("GroupStyleSelector");
			AddPresentationDescriptor("GroupStyle");
			AddPresentationDescriptor("BorderGapMaskConverter");
			AddPresentationDescriptor("Slider");
			AddPresentationDescriptor("GroupItem", "Content");
			AddPresentationDescriptor("ResourceDictionary");
			AddPresentationDescriptor("StackPanel");
			AddPresentationDescriptor("DockPanel");
			AddPresentationDescriptor("Image");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Ellipse"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			AddPresentationDescriptor("HeaderedItemsControl");
			AddPresentationDescriptor("ColumnDefinition");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Polygon"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			AddPresentationDescriptor("PropertyPathConverter");
			AddPresentationDescriptor("Menu");
			AddPresentationDescriptor("Condition");
			AddPresentationDescriptor("TemplateBindingExtension");
			AddPresentationDescriptor("TextElementEditingBehaviorAttribute");
			AddPresentationDescriptor("RepeatButton", "Content");
			AddPresentationDescriptor("AdornedElementPlaceholder");
			AddPresentationDescriptor("JournalEntry");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Figure"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("BulletDecorator");
			AddPresentationDescriptor("SpellingError");
			AddPresentationDescriptor("InkPresenter");
			AddPresentationDescriptor("DataTemplateKey");
			AddPresentationDescriptor("ItemsPanelTemplate");
			AddPresentationDescriptor("FlowDocumentPageViewer");
			AddPresentationDescriptor("GridViewRowPresenter", "Content");
			AddPresentationDescriptor("ThicknessConverter");
			AddPresentationDescriptor("FixedDocumentSequence");
			AddPresentationDescriptor("MenuScrollingVisibilityConverter");
			AddPresentationDescriptor("TemplateBindingExpressionConverter");
			AddPresentationDescriptor("GridViewHeaderRowPresenter");
			AddPresentationDescriptor("TreeViewItem");
			AddPresentationDescriptor("TemplateBindingExtensionConverter");
			AddPresentationDescriptor("MultiTrigger");
			AddPresentationDescriptor("ComboBox", "Text");
			AddPresentationDescriptor("UniformGrid");
			AddPresentationDescriptor("ListBoxItem", "Content");
			AddPresentationDescriptor("Grid");
			AddPresentationDescriptor("Trigger");
			AddPresentationDescriptor("RichTextBox");
			AddPresentationDescriptor("GroupBox", "Content");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "InlineUIContainer"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("CheckBox", "Content");
			AddPresentationDescriptor("ToolBarPanel");
			AddPresentationDescriptor("DynamicResourceExtension");
			AddPresentationDescriptor("FontSizeConverter");
			AddPresentationDescriptor("Separator");
			AddPresentationDescriptor("Table");
			AddPresentationDescriptor("VirtualizingStackPanel");
			AddPresentationDescriptor("DocumentViewer");
			AddPresentationDescriptor("TableRow");
			AddPresentationDescriptor("RadioButton", "Content");
			AddPresentationDescriptor("StaticResourceExtension");
			AddPresentationDescriptor("TableColumn");
			AddPresentationDescriptor("Track");
			AddPresentationDescriptor("ProgressBar");
			AddPresentationDescriptor("ListViewItem", "Content");
			AddPresentationDescriptor("ZoomPercentageConverter");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Floater"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("TabItem", "Content");
			AddPresentationDescriptor("FlowDocument");
			AddPresentationDescriptor("Label", "Content");
			AddPresentationDescriptor("WrapPanel");
			AddPresentationDescriptor("ListItem");
			AddPresentationDescriptor("FrameworkPropertyMetadata");
			AddPresentationDescriptor("NameScope");
			AddPresentationDescriptor("TreeView");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Rectangle"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Hyperlink"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("TableRowGroup");
			AddPresentationDescriptor("Application");
			AddPresentationDescriptor("TickBar");
			AddPresentationDescriptor("ResizeGrip");
			AddPresentationDescriptor("FrameworkElement");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Run"), new ContentDescriptor(hasIndexableContent: true, isInline: true, "Text", null));
			AddPresentationDescriptor("FrameworkContentElement");
			AddPresentationDescriptor("ItemContainerGenerator");
			AddPresentationDescriptor("ThemeDictionaryExtension");
			AddPresentationDescriptor("AccessText", "Text");
			AddPresentationDescriptor("Frame", "Content");
			AddPresentationDescriptor("LostFocusEventManager");
			AddPresentationDescriptor("EventTrigger");
			AddPresentationDescriptor("DataErrorValidationRule");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Page"), new ContentDescriptor(hasIndexableContent: true, isInline: false, "Content", "WindowTitle"));
			AddPresentationDescriptor("GridLengthConverter");
			AddPresentationDescriptor("TextSelection", "Text");
			AddPresentationDescriptor("FixedDocument");
			AddPresentationDescriptor("HierarchicalDataTemplate");
			AddPresentationDescriptor("MessageBox");
			AddPresentationDescriptor("Style");
			AddPresentationDescriptor("ScrollContentPresenter", "Content");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Span"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("TextPointer");
			AddPresentationDescriptor("FrameworkElementFactory", "Text");
			AddPresentationDescriptor("ExceptionValidationRule");
			AddPresentationDescriptor("DocumentPageView");
			AddPresentationDescriptor("ToolBar");
			AddPresentationDescriptor("ListView");
			AddPresentationDescriptor("StyleTypedPropertyAttribute");
			AddPresentationDescriptor("ToolBarOverflowPanel");
			AddPresentationDescriptor("BooleanToVisibilityConverter");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Line"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			AddPresentationDescriptor("MenuItem");
			AddPresentationDescriptor("Section");
			AddPresentationDescriptor("DynamicResourceExtensionConverter");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Underline"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("TemplateBindingExpression");
			AddPresentationDescriptor("Viewport3D");
			AddPresentationDescriptor("PrintDialog");
			AddPresentationDescriptor("ItemsPresenter");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/xps/2005/06", "Polyline"), new ContentDescriptor(hasIndexableContent: false, isInline: false, null, null));
			AddPresentationDescriptor("FrameworkTextComposition", "Text");
			AddPresentationDescriptor("TextRange", "Text");
			AddPresentationDescriptor("StatusBarItem", "Content");
			AddPresentationDescriptor("FlowDocumentReader");
			AddPresentationDescriptor("TextEffectTarget");
			AddPresentationDescriptor("ColorConvertedBitmapExtension");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "NavigationWindow"), new ContentDescriptor(hasIndexableContent: true, isInline: false, "Content", "Title"));
			AddPresentationDescriptor("AdornerLayer");
			AddPresentationDescriptor("GridView");
			AddPresentationDescriptor("CustomPopupPlacementCallback");
			AddPresentationDescriptor("MultiDataTrigger");
			AddPresentationDescriptor("NavigationService", "Content");
			AddPresentationDescriptor("PropertyPath");
			_xamlElementContentDescriptorDictionary.Add(new ElementTableKey("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Bold"), new ContentDescriptor(hasIndexableContent: true, isInline: true, null, null));
			AddPresentationDescriptor("ResourceReferenceKeyNotFoundException");
			AddPresentationDescriptor("StatusBar");
			AddPresentationDescriptor("Border");
			AddPresentationDescriptor("SpellCheck");
			AddPresentationDescriptor("SoundPlayerAction");
			AddPresentationDescriptor("ContentPresenter", "Content");
			AddPresentationDescriptor("EventSetter");
			AddPresentationDescriptor("StickyNoteControl");
			AddPresentationDescriptor("UserControl", "Content");
			AddPresentationDescriptor("FlowDocumentScrollViewer");
			AddPresentationDescriptor("ThemeInfoAttribute");
			AddPresentationDescriptor("List");
			AddPresentationDescriptor("DataTemplate");
			AddPresentationDescriptor("GridSplitter");
			AddPresentationDescriptor("TableCell");
			AddPresentationDescriptor("Thumb");
			AddPresentationDescriptor("Glyphs");
			AddPresentationDescriptor("ScrollViewer", "Content");
			AddPresentationDescriptor("TabPanel");
			AddPresentationDescriptor("Setter");
			AddPresentationDescriptor("PageContent");
			AddPresentationDescriptor("TabControl");
			AddPresentationDescriptor("Typography");
			AddPresentationDescriptor("ScrollBar");
			AddPresentationDescriptor("NullableBoolConverter");
			AddPresentationDescriptor("ControlTemplate");
			AddPresentationDescriptor("ContextMenu");
			AddPresentationDescriptor("Popup");
			AddPresentationDescriptor("Control");
			AddPresentationDescriptor("ToolBarTray");
			AddPresentationDescriptor("Expander", "Content");
			AddPresentationDescriptor("JournalEntryUnifiedViewConverter");
			AddPresentationDescriptor("Viewbox");
		}
	}

	internal XamlFilter(Stream stream)
	{
		Trace.TraceInformation("New Xaml filter created.");
		_lcidDictionary = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
		_contextStack = new Stack(32);
		InitializeDeclaredFields();
		_xamlStream = stream;
		CreateXmlReader();
		_filterState = FilterState.FindNextUnit;
	}

	private void InitializeDeclaredFields()
	{
		ClearStack();
		_filterState = FilterState.Uninitialized;
		_currentChunkID = 0u;
		LoadContentDescriptorDictionary();
		_countOfCharactersReturned = 0;
		_currentContent = null;
		_indexingContentUnit = null;
		_expectingBlockStart = true;
		_topLevelReader = null;
		_fixedPageContentExtractor = null;
		_fixedPageDomTree = null;
	}

	public IFILTER_FLAGS Init(IFILTER_INIT grfFlags, ManagedFullPropSpec[] aAttributes)
	{
		_filterContents = true;
		if (aAttributes != null && aAttributes.Length != 0)
		{
			_filterContents = false;
			for (int i = 0; i < aAttributes.Length; i++)
			{
				if (aAttributes[i].Guid == IndexingFilterMarshaler.PSGUID_STORAGE && aAttributes[i].Property.PropType == PropSpecType.Id && aAttributes[i].Property.PropId == 19)
				{
					_filterContents = true;
					break;
				}
			}
		}
		_returnCanonicalParagraphBreaks = (grfFlags & IFILTER_INIT.IFILTER_INIT_CANON_PARAGRAPHS) != 0;
		return IFILTER_FLAGS.IFILTER_FLAGS_NONE;
	}

	public ManagedChunk GetChunk()
	{
		if (!_filterContents)
		{
			_currentContent = null;
			return null;
		}
		if (_xamlReader == null)
		{
			throw new COMException(SR.FilterGetChunkNoStream, -2147215613);
		}
		if (_filterState == FilterState.EndOfStream)
		{
			EnsureXmlReaderIsClosed();
			return null;
		}
		IndexingContentUnit indexingContentUnit;
		try
		{
			indexingContentUnit = NextContentUnit();
		}
		catch (XmlException ex)
		{
			EnsureXmlReaderIsClosed();
			throw new COMException(ex.Message, -2147215604);
		}
		if (indexingContentUnit == null)
		{
			_currentContent = null;
			EnsureXmlReaderIsClosed();
			return null;
		}
		_currentContent = indexingContentUnit.Text;
		_countOfCharactersReturned = 0;
		return indexingContentUnit;
	}

	public string GetText(int bufferCharacterCount)
	{
		if (_currentContent == null)
		{
			SecurityHelper.ThrowExceptionForHR(-2147215611);
		}
		int num = _currentContent.Length - _countOfCharactersReturned;
		if (num <= 0)
		{
			SecurityHelper.ThrowExceptionForHR(-2147215615);
		}
		if (num > bufferCharacterCount)
		{
			num = bufferCharacterCount;
		}
		string result = _currentContent.Substring(_countOfCharactersReturned, num);
		_countOfCharactersReturned += num;
		return result;
	}

	public object GetValue()
	{
		SecurityHelper.ThrowExceptionForHR(-2147215610);
		return null;
	}

	internal IndexingContentUnit NextContentUnit()
	{
		IndexingContentUnit indexingContentUnit = null;
		while (indexingContentUnit == null)
		{
			if (_filterState == FilterState.UseContentExtractor)
			{
				if (!_fixedPageContentExtractor.AtEndOfPage)
				{
					bool inline;
					uint lcid;
					string text = _fixedPageContentExtractor.NextGlyphContent(out inline, out lcid);
					_expectingBlockStart = !inline;
					return BuildIndexingContentUnit(text, lcid);
				}
				_fixedPageContentExtractor = null;
				_topLevelReader = _xamlReader;
				_xamlReader = new XmlNodeReader(_fixedPageDomTree.DocumentElement);
				_filterState = FilterState.FindNextFlowUnit;
			}
			if (_xamlReader.EOF)
			{
				switch (_filterState)
				{
				case FilterState.FindNextUnit:
					_filterState = FilterState.EndOfStream;
					return null;
				case FilterState.FindNextFlowUnit:
					_xamlReader.Close();
					_xamlReader = _topLevelReader;
					_filterState = FilterState.FindNextUnit;
					break;
				}
			}
			switch (_xamlReader.NodeType)
			{
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				indexingContentUnit = HandleTextData();
				break;
			case XmlNodeType.Element:
				indexingContentUnit = HandleElementStart();
				break;
			case XmlNodeType.EndElement:
				indexingContentUnit = HandleElementEnd();
				break;
			default:
				_xamlReader.Read();
				break;
			}
		}
		return indexingContentUnit;
	}

	private void LoadContentDescriptorDictionary()
	{
		InitElementDictionary();
	}

	private IndexingContentUnit BuildIndexingContentUnit(string text, uint lcid)
	{
		CHUNK_BREAKTYPE breakType = CHUNK_BREAKTYPE.CHUNK_NO_BREAK;
		if (_expectingBlockStart)
		{
			breakType = CHUNK_BREAKTYPE.CHUNK_EOP;
			if (_returnCanonicalParagraphBreaks)
			{
				text = "\u2029" + text;
			}
		}
		if (_indexingContentUnit == null)
		{
			_indexingContentUnit = new IndexingContentUnit(text, AllocateChunkID(), breakType, _propSpec, lcid);
		}
		else
		{
			_indexingContentUnit.InitIndexingContentUnit(text, AllocateChunkID(), breakType, _propSpec, lcid);
		}
		_expectingBlockStart = false;
		return _indexingContentUnit;
	}

	private ContentDescriptor GetContentInformationAboutCustomElement(ElementTableKey customElement)
	{
		return _defaultContentDescriptor;
	}

	private IndexingContentUnit HandleTextData()
	{
		if (TopOfStack() != null)
		{
			IndexingContentUnit result = BuildIndexingContentUnit(_xamlReader.Value, GetCurrentLcid());
			_xamlReader.Read();
			return result;
		}
		_xamlReader.Read();
		return null;
	}

	private IndexingContentUnit HandleElementStart()
	{
		ElementTableKey elementTableKey = new ElementTableKey(_xamlReader.NamespaceURI, _xamlReader.LocalName);
		if (IsPrefixedPropertyName(elementTableKey.BaseName, out var propertyName))
		{
			ContentDescriptor contentDescriptor = TopOfStack();
			if (contentDescriptor == null)
			{
				SkipCurrentElement();
				return null;
			}
			bool flag = elementTableKey.XmlNamespace.Equals(ElementTableKey.XamlNamespace, StringComparison.Ordinal) && (propertyName == contentDescriptor.ContentProp || propertyName == contentDescriptor.TitleProp);
			if (!flag)
			{
				SkipCurrentElement();
				return null;
			}
			Push(new ContentDescriptor(flag, TopOfStack().IsInline, string.Empty, null));
			_xamlReader.Read();
			return null;
		}
		bool handled;
		IndexingContentUnit indexingContentUnit = HandleFixedFormatTag(elementTableKey, out handled);
		if (handled)
		{
			return indexingContentUnit;
		}
		Invariant.Assert(indexingContentUnit == null);
		ContentDescriptor contentDescriptor2 = (ContentDescriptor)_xamlElementContentDescriptorDictionary[elementTableKey];
		if (contentDescriptor2 == null)
		{
			contentDescriptor2 = (elementTableKey.XmlNamespace.Equals(ElementTableKey.XamlNamespace, StringComparison.Ordinal) ? _defaultContentDescriptor : ((!elementTableKey.XmlNamespace.Equals("http://schemas.microsoft.com/winfx/2006/xaml", StringComparison.Ordinal)) ? GetContentInformationAboutCustomElement(elementTableKey) : _nonIndexableElementDescriptor));
			_xamlElementContentDescriptorDictionary.Add(elementTableKey, contentDescriptor2);
		}
		if (!contentDescriptor2.HasIndexableContent)
		{
			SkipCurrentElement();
			return null;
		}
		string text = null;
		if (contentDescriptor2.TitleProp != null && (_attributesToIgnore & AttributesToIgnore.Title) == 0)
		{
			text = GetPropertyAsAttribute(contentDescriptor2.TitleProp);
			if (text != null && text.Length > 0)
			{
				_attributesToIgnore |= AttributesToIgnore.Title;
				_expectingBlockStart = true;
				IndexingContentUnit result = BuildIndexingContentUnit(text, GetCurrentLcid());
				_expectingBlockStart = true;
				return result;
			}
		}
		string text2 = null;
		if (contentDescriptor2.ContentProp != null && (_attributesToIgnore & AttributesToIgnore.Content) == 0)
		{
			text2 = GetPropertyAsAttribute(contentDescriptor2.ContentProp);
			if (text2 != null && text2.Length > 0)
			{
				_attributesToIgnore |= AttributesToIgnore.Content;
				if (!contentDescriptor2.IsInline)
				{
					_expectingBlockStart = true;
				}
				IndexingContentUnit result2 = BuildIndexingContentUnit(text2, GetCurrentLcid());
				_expectingBlockStart = !contentDescriptor2.IsInline;
				return result2;
			}
		}
		_attributesToIgnore = AttributesToIgnore.None;
		if (_xamlReader.IsEmptyElement)
		{
			if (!contentDescriptor2.IsInline)
			{
				_expectingBlockStart = true;
			}
			_xamlReader.Read();
			return null;
		}
		Push(contentDescriptor2);
		_xamlReader.Read();
		return null;
	}

	private IndexingContentUnit HandleElementEnd()
	{
		Pop();
		_xamlReader.Read();
		return null;
	}

	private IndexingContentUnit HandleFixedFormatTag(ElementTableKey elementFullName, out bool handled)
	{
		handled = true;
		if (!elementFullName.XmlNamespace.Equals(ElementTableKey.FixedMarkupNamespace, StringComparison.Ordinal))
		{
			handled = false;
			return null;
		}
		if (string.CompareOrdinal(elementFullName.BaseName, "Glyphs") == 0)
		{
			if (_filterState == FilterState.FindNextFlowUnit)
			{
				SkipCurrentElement();
				return null;
			}
			return ProcessGlyphRun();
		}
		if (string.CompareOrdinal(elementFullName.BaseName, "FixedPage") == 0)
		{
			if (_filterState == FilterState.FindNextFlowUnit)
			{
				Push(_defaultContentDescriptor);
				_xamlReader.Read();
				return null;
			}
			return ProcessFixedPage();
		}
		if (string.CompareOrdinal(elementFullName.BaseName, "PageContent") == 0)
		{
			if (_xamlReader.GetAttribute("Source") != null)
			{
				SkipCurrentElement();
				return null;
			}
			Push(_defaultContentDescriptor);
			_xamlReader.Read();
			return null;
		}
		handled = false;
		return null;
	}

	private IndexingContentUnit ProcessGlyphRun()
	{
		string attribute = _xamlReader.GetAttribute("UnicodeString");
		if (attribute == null || attribute.Length == 0)
		{
			SkipCurrentElement();
			return null;
		}
		_expectingBlockStart = true;
		uint currentLcid = GetCurrentLcid();
		SkipCurrentElement();
		return BuildIndexingContentUnit(attribute, currentLcid);
	}

	private IndexingContentUnit ProcessFixedPage()
	{
		if (_filterState == FilterState.FindNextFlowUnit)
		{
			throw new XmlException(SR.XamlFilterNestedFixedPage);
		}
		string xml = _xamlReader.ReadOuterXml();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xml);
		if (_xamlReader.XmlLang.Length > 0)
		{
			xmlDocument.DocumentElement.SetAttribute("xml:lang", _xamlReader.XmlLang);
		}
		_fixedPageContentExtractor = new FixedPageContentExtractor(xmlDocument.DocumentElement);
		_fixedPageDomTree = xmlDocument;
		_filterState = FilterState.UseContentExtractor;
		return null;
	}

	private void CreateXmlReader()
	{
		if (_xamlReader != null)
		{
			_xamlReader.Close();
		}
		_xamlReader = new XmlTextReader(_xamlStream);
		((XmlTextReader)_xamlReader).WhitespaceHandling = WhitespaceHandling.Significant;
		_attributesToIgnore = AttributesToIgnore.None;
	}

	private void EnsureXmlReaderIsClosed()
	{
		if (_xamlReader != null)
		{
			_xamlReader.Close();
		}
	}

	private uint GetCurrentLcid()
	{
		string languageString = GetLanguageString();
		if (languageString.Length == 0)
		{
			return (uint)CultureInfo.InvariantCulture.LCID;
		}
		if (_lcidDictionary.ContainsKey(languageString))
		{
			return _lcidDictionary[languageString];
		}
		CultureInfo cultureInfo = new CultureInfo(languageString);
		_lcidDictionary.Add(languageString, (uint)cultureInfo.LCID);
		return (uint)cultureInfo.LCID;
	}

	private string GetLanguageString()
	{
		string xmlLang = _xamlReader.XmlLang;
		if (xmlLang.Length == 0 && _topLevelReader != null)
		{
			xmlLang = _topLevelReader.XmlLang;
		}
		return xmlLang;
	}

	private void SkipCurrentElement()
	{
		_xamlReader.Skip();
	}

	private bool IsPrefixedPropertyName(string name, out string propertyName)
	{
		int num = name.IndexOf('.');
		if (num == -1)
		{
			propertyName = null;
			return false;
		}
		propertyName = name.Substring(num + 1);
		return true;
	}

	private uint AllocateChunkID()
	{
		Invariant.Assert(_currentChunkID <= uint.MaxValue);
		_currentChunkID++;
		return _currentChunkID;
	}

	private string GetPropertyAsAttribute(string propertyName)
	{
		string text = _xamlReader.GetAttribute(propertyName);
		if (text != null)
		{
			return text;
		}
		bool flag = _xamlReader.MoveToFirstAttribute();
		while (flag)
		{
			if (IsPrefixedPropertyName(_xamlReader.LocalName, out var propertyName2) && propertyName2.Equals(propertyName, StringComparison.Ordinal))
			{
				text = _xamlReader.Value;
				break;
			}
			flag = _xamlReader.MoveToNextAttribute();
		}
		_xamlReader.MoveToElement();
		return text;
	}

	private ContentDescriptor TopOfStack()
	{
		return (ContentDescriptor)_contextStack.Peek();
	}

	private void Push(ContentDescriptor contentDescriptor)
	{
		if (!contentDescriptor.IsInline)
		{
			_expectingBlockStart = true;
		}
		_contextStack.Push(contentDescriptor);
	}

	private ContentDescriptor Pop()
	{
		ContentDescriptor obj = (ContentDescriptor)_contextStack.Pop();
		if (!obj.IsInline)
		{
			_expectingBlockStart = true;
		}
		return obj;
	}

	private void ClearStack()
	{
		_contextStack.Clear();
	}
}
