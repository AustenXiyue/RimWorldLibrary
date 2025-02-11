using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml.Serialization;
using MS.Internal.Markup;

namespace System.Windows.Baml2006;

internal class WpfSharedBamlSchemaContext : XamlSchemaContext
{
	private const int KnownPropertyCount = 270;

	private const int KnownTypeCount = 759;

	private object _syncObject;

	private Baml6Assembly[] _knownBamlAssemblies;

	private WpfKnownType[] _knownBamlTypes;

	private WpfKnownMember[] _knownBamlMembers;

	private Dictionary<Type, XamlType> _masterTypeTable;

	private XmlnsDictionary _wpfDefaultNamespace;

	private List<ThemeKnownTypeHelper> _themeHelpers;

	private static readonly Lazy<XamlMember> _xStaticMemberProperty = new Lazy<XamlMember>(() => XamlLanguage.Static.GetMember("MemberType"));

	private static readonly Lazy<XamlMember> _xTypeTypeProperty = new Lazy<XamlMember>(() => XamlLanguage.Static.GetMember("Type"));

	private static readonly Lazy<XamlMember> _resourceDictionaryDefContentProperty = new Lazy<XamlMember>(() => _resourceDictionaryType.Value.GetMember("DeferrableContent"));

	private static readonly Lazy<XamlType> _resourceDictionaryType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(ResourceDictionary)));

	private static readonly Lazy<XamlType> _eventSetterType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(EventSetter)));

	private static readonly Lazy<XamlMember> _eventSetterEventProperty = new Lazy<XamlMember>(() => _eventSetterType.Value.GetMember("Event"));

	private static readonly Lazy<XamlMember> _eventSetterHandlerProperty = new Lazy<XamlMember>(() => _eventSetterType.Value.GetMember("Handler"));

	private static readonly Lazy<XamlMember> _frameworkTemplateTemplateProperty = new Lazy<XamlMember>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(FrameworkTemplate)).GetMember("Template"));

	private static readonly Lazy<XamlType> _staticResourceExtensionType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(StaticResourceExtension)));

	internal XamlMember StaticExtensionMemberTypeProperty => _xStaticMemberProperty.Value;

	internal XamlMember TypeExtensionTypeProperty => _xTypeTypeProperty.Value;

	internal XamlMember ResourceDictionaryDeferredContentProperty => _resourceDictionaryDefContentProperty.Value;

	internal XamlType ResourceDictionaryType => _resourceDictionaryType.Value;

	internal XamlType EventSetterType => _eventSetterType.Value;

	internal XamlMember EventSetterEventProperty => _eventSetterEventProperty.Value;

	internal XamlMember EventSetterHandlerProperty => _eventSetterHandlerProperty.Value;

	internal XamlMember FrameworkTemplateTemplateProperty => _frameworkTemplateTemplateProperty.Value;

	internal XamlType StaticResourceExtensionType => _staticResourceExtensionType.Value;

	internal Baml2006ReaderSettings Settings { get; set; }

	internal List<ThemeKnownTypeHelper> ThemeKnownTypeHelpers
	{
		get
		{
			if (_themeHelpers == null)
			{
				_themeHelpers = new List<ThemeKnownTypeHelper>();
			}
			return _themeHelpers;
		}
	}

	private WpfKnownMember CreateKnownMember(short bamlNumber)
	{
		return bamlNumber switch
		{
			1 => Create_BamlProperty_AccessText_Text(), 
			2 => Create_BamlProperty_BeginStoryboard_Storyboard(), 
			3 => Create_BamlProperty_BitmapEffectGroup_Children(), 
			4 => Create_BamlProperty_Border_Background(), 
			5 => Create_BamlProperty_Border_BorderBrush(), 
			6 => Create_BamlProperty_Border_BorderThickness(), 
			7 => Create_BamlProperty_ButtonBase_Command(), 
			8 => Create_BamlProperty_ButtonBase_CommandParameter(), 
			9 => Create_BamlProperty_ButtonBase_CommandTarget(), 
			10 => Create_BamlProperty_ButtonBase_IsPressed(), 
			11 => Create_BamlProperty_ColumnDefinition_MaxWidth(), 
			12 => Create_BamlProperty_ColumnDefinition_MinWidth(), 
			13 => Create_BamlProperty_ColumnDefinition_Width(), 
			14 => Create_BamlProperty_ContentControl_Content(), 
			15 => Create_BamlProperty_ContentControl_ContentTemplate(), 
			16 => Create_BamlProperty_ContentControl_ContentTemplateSelector(), 
			17 => Create_BamlProperty_ContentControl_HasContent(), 
			18 => Create_BamlProperty_ContentElement_Focusable(), 
			19 => Create_BamlProperty_ContentPresenter_Content(), 
			20 => Create_BamlProperty_ContentPresenter_ContentSource(), 
			21 => Create_BamlProperty_ContentPresenter_ContentTemplate(), 
			22 => Create_BamlProperty_ContentPresenter_ContentTemplateSelector(), 
			23 => Create_BamlProperty_ContentPresenter_RecognizesAccessKey(), 
			24 => Create_BamlProperty_Control_Background(), 
			25 => Create_BamlProperty_Control_BorderBrush(), 
			26 => Create_BamlProperty_Control_BorderThickness(), 
			27 => Create_BamlProperty_Control_FontFamily(), 
			28 => Create_BamlProperty_Control_FontSize(), 
			29 => Create_BamlProperty_Control_FontStretch(), 
			30 => Create_BamlProperty_Control_FontStyle(), 
			31 => Create_BamlProperty_Control_FontWeight(), 
			32 => Create_BamlProperty_Control_Foreground(), 
			33 => Create_BamlProperty_Control_HorizontalContentAlignment(), 
			34 => Create_BamlProperty_Control_IsTabStop(), 
			35 => Create_BamlProperty_Control_Padding(), 
			36 => Create_BamlProperty_Control_TabIndex(), 
			37 => Create_BamlProperty_Control_Template(), 
			38 => Create_BamlProperty_Control_VerticalContentAlignment(), 
			39 => Create_BamlProperty_DockPanel_Dock(), 
			40 => Create_BamlProperty_DockPanel_LastChildFill(), 
			41 => Create_BamlProperty_DocumentViewerBase_Document(), 
			42 => Create_BamlProperty_DrawingGroup_Children(), 
			43 => Create_BamlProperty_FlowDocumentReader_Document(), 
			44 => Create_BamlProperty_FlowDocumentScrollViewer_Document(), 
			45 => Create_BamlProperty_FrameworkContentElement_Style(), 
			46 => Create_BamlProperty_FrameworkElement_FlowDirection(), 
			47 => Create_BamlProperty_FrameworkElement_Height(), 
			48 => Create_BamlProperty_FrameworkElement_HorizontalAlignment(), 
			49 => Create_BamlProperty_FrameworkElement_Margin(), 
			50 => Create_BamlProperty_FrameworkElement_MaxHeight(), 
			51 => Create_BamlProperty_FrameworkElement_MaxWidth(), 
			52 => Create_BamlProperty_FrameworkElement_MinHeight(), 
			53 => Create_BamlProperty_FrameworkElement_MinWidth(), 
			54 => Create_BamlProperty_FrameworkElement_Name(), 
			55 => Create_BamlProperty_FrameworkElement_Style(), 
			56 => Create_BamlProperty_FrameworkElement_VerticalAlignment(), 
			57 => Create_BamlProperty_FrameworkElement_Width(), 
			58 => Create_BamlProperty_GeneralTransformGroup_Children(), 
			59 => Create_BamlProperty_GeometryGroup_Children(), 
			60 => Create_BamlProperty_GradientBrush_GradientStops(), 
			61 => Create_BamlProperty_Grid_Column(), 
			62 => Create_BamlProperty_Grid_ColumnSpan(), 
			63 => Create_BamlProperty_Grid_Row(), 
			64 => Create_BamlProperty_Grid_RowSpan(), 
			65 => Create_BamlProperty_GridViewColumn_Header(), 
			66 => Create_BamlProperty_HeaderedContentControl_HasHeader(), 
			67 => Create_BamlProperty_HeaderedContentControl_Header(), 
			68 => Create_BamlProperty_HeaderedContentControl_HeaderTemplate(), 
			69 => Create_BamlProperty_HeaderedContentControl_HeaderTemplateSelector(), 
			70 => Create_BamlProperty_HeaderedItemsControl_HasHeader(), 
			71 => Create_BamlProperty_HeaderedItemsControl_Header(), 
			72 => Create_BamlProperty_HeaderedItemsControl_HeaderTemplate(), 
			73 => Create_BamlProperty_HeaderedItemsControl_HeaderTemplateSelector(), 
			74 => Create_BamlProperty_Hyperlink_NavigateUri(), 
			75 => Create_BamlProperty_Image_Source(), 
			76 => Create_BamlProperty_Image_Stretch(), 
			77 => Create_BamlProperty_ItemsControl_ItemContainerStyle(), 
			78 => Create_BamlProperty_ItemsControl_ItemContainerStyleSelector(), 
			79 => Create_BamlProperty_ItemsControl_ItemTemplate(), 
			80 => Create_BamlProperty_ItemsControl_ItemTemplateSelector(), 
			81 => Create_BamlProperty_ItemsControl_ItemsPanel(), 
			82 => Create_BamlProperty_ItemsControl_ItemsSource(), 
			83 => Create_BamlProperty_MaterialGroup_Children(), 
			84 => Create_BamlProperty_Model3DGroup_Children(), 
			85 => Create_BamlProperty_Page_Content(), 
			86 => Create_BamlProperty_Panel_Background(), 
			87 => Create_BamlProperty_Path_Data(), 
			88 => Create_BamlProperty_PathFigure_Segments(), 
			89 => Create_BamlProperty_PathGeometry_Figures(), 
			90 => Create_BamlProperty_Popup_Child(), 
			91 => Create_BamlProperty_Popup_IsOpen(), 
			92 => Create_BamlProperty_Popup_Placement(), 
			93 => Create_BamlProperty_Popup_PopupAnimation(), 
			94 => Create_BamlProperty_RowDefinition_Height(), 
			95 => Create_BamlProperty_RowDefinition_MaxHeight(), 
			96 => Create_BamlProperty_RowDefinition_MinHeight(), 
			97 => Create_BamlProperty_ScrollViewer_CanContentScroll(), 
			98 => Create_BamlProperty_ScrollViewer_HorizontalScrollBarVisibility(), 
			99 => Create_BamlProperty_ScrollViewer_VerticalScrollBarVisibility(), 
			100 => Create_BamlProperty_Shape_Fill(), 
			101 => Create_BamlProperty_Shape_Stroke(), 
			102 => Create_BamlProperty_Shape_StrokeThickness(), 
			103 => Create_BamlProperty_TextBlock_Background(), 
			104 => Create_BamlProperty_TextBlock_FontFamily(), 
			105 => Create_BamlProperty_TextBlock_FontSize(), 
			106 => Create_BamlProperty_TextBlock_FontStretch(), 
			107 => Create_BamlProperty_TextBlock_FontStyle(), 
			108 => Create_BamlProperty_TextBlock_FontWeight(), 
			109 => Create_BamlProperty_TextBlock_Foreground(), 
			110 => Create_BamlProperty_TextBlock_Text(), 
			111 => Create_BamlProperty_TextBlock_TextDecorations(), 
			112 => Create_BamlProperty_TextBlock_TextTrimming(), 
			113 => Create_BamlProperty_TextBlock_TextWrapping(), 
			114 => Create_BamlProperty_TextBox_Text(), 
			115 => Create_BamlProperty_TextElement_Background(), 
			116 => Create_BamlProperty_TextElement_FontFamily(), 
			117 => Create_BamlProperty_TextElement_FontSize(), 
			118 => Create_BamlProperty_TextElement_FontStretch(), 
			119 => Create_BamlProperty_TextElement_FontStyle(), 
			120 => Create_BamlProperty_TextElement_FontWeight(), 
			121 => Create_BamlProperty_TextElement_Foreground(), 
			122 => Create_BamlProperty_TimelineGroup_Children(), 
			123 => Create_BamlProperty_Track_IsDirectionReversed(), 
			124 => Create_BamlProperty_Track_Maximum(), 
			125 => Create_BamlProperty_Track_Minimum(), 
			126 => Create_BamlProperty_Track_Orientation(), 
			127 => Create_BamlProperty_Track_Value(), 
			128 => Create_BamlProperty_Track_ViewportSize(), 
			129 => Create_BamlProperty_Transform3DGroup_Children(), 
			130 => Create_BamlProperty_TransformGroup_Children(), 
			131 => Create_BamlProperty_UIElement_ClipToBounds(), 
			132 => Create_BamlProperty_UIElement_Focusable(), 
			133 => Create_BamlProperty_UIElement_IsEnabled(), 
			134 => Create_BamlProperty_UIElement_RenderTransform(), 
			135 => Create_BamlProperty_UIElement_Visibility(), 
			136 => Create_BamlProperty_Viewport3D_Children(), 
			138 => Create_BamlProperty_AdornedElementPlaceholder_Child(), 
			139 => Create_BamlProperty_AdornerDecorator_Child(), 
			140 => Create_BamlProperty_AnchoredBlock_Blocks(), 
			141 => Create_BamlProperty_ArrayExtension_Items(), 
			142 => Create_BamlProperty_BlockUIContainer_Child(), 
			143 => Create_BamlProperty_Bold_Inlines(), 
			144 => Create_BamlProperty_BooleanAnimationUsingKeyFrames_KeyFrames(), 
			145 => Create_BamlProperty_Border_Child(), 
			146 => Create_BamlProperty_BulletDecorator_Child(), 
			147 => Create_BamlProperty_Button_Content(), 
			148 => Create_BamlProperty_ButtonBase_Content(), 
			149 => Create_BamlProperty_ByteAnimationUsingKeyFrames_KeyFrames(), 
			150 => Create_BamlProperty_Canvas_Children(), 
			151 => Create_BamlProperty_CharAnimationUsingKeyFrames_KeyFrames(), 
			152 => Create_BamlProperty_CheckBox_Content(), 
			153 => Create_BamlProperty_ColorAnimationUsingKeyFrames_KeyFrames(), 
			154 => Create_BamlProperty_ComboBox_Items(), 
			155 => Create_BamlProperty_ComboBoxItem_Content(), 
			156 => Create_BamlProperty_ContextMenu_Items(), 
			157 => Create_BamlProperty_ControlTemplate_VisualTree(), 
			158 => Create_BamlProperty_DataTemplate_VisualTree(), 
			159 => Create_BamlProperty_DataTrigger_Setters(), 
			160 => Create_BamlProperty_DecimalAnimationUsingKeyFrames_KeyFrames(), 
			161 => Create_BamlProperty_Decorator_Child(), 
			162 => Create_BamlProperty_DockPanel_Children(), 
			163 => Create_BamlProperty_DocumentViewer_Document(), 
			164 => Create_BamlProperty_DoubleAnimationUsingKeyFrames_KeyFrames(), 
			165 => Create_BamlProperty_EventTrigger_Actions(), 
			166 => Create_BamlProperty_Expander_Content(), 
			167 => Create_BamlProperty_Figure_Blocks(), 
			168 => Create_BamlProperty_FixedDocument_Pages(), 
			169 => Create_BamlProperty_FixedDocumentSequence_References(), 
			170 => Create_BamlProperty_FixedPage_Children(), 
			171 => Create_BamlProperty_Floater_Blocks(), 
			172 => Create_BamlProperty_FlowDocument_Blocks(), 
			173 => Create_BamlProperty_FlowDocumentPageViewer_Document(), 
			174 => Create_BamlProperty_FrameworkTemplate_VisualTree(), 
			175 => Create_BamlProperty_Grid_Children(), 
			176 => Create_BamlProperty_GridView_Columns(), 
			177 => Create_BamlProperty_GridViewColumnHeader_Content(), 
			178 => Create_BamlProperty_GroupBox_Content(), 
			179 => Create_BamlProperty_GroupItem_Content(), 
			180 => Create_BamlProperty_HeaderedContentControl_Content(), 
			181 => Create_BamlProperty_HeaderedItemsControl_Items(), 
			182 => Create_BamlProperty_HierarchicalDataTemplate_VisualTree(), 
			183 => Create_BamlProperty_Hyperlink_Inlines(), 
			184 => Create_BamlProperty_InkCanvas_Children(), 
			185 => Create_BamlProperty_InkPresenter_Child(), 
			186 => Create_BamlProperty_InlineUIContainer_Child(), 
			187 => Create_BamlProperty_InputScopeName_NameValue(), 
			188 => Create_BamlProperty_Int16AnimationUsingKeyFrames_KeyFrames(), 
			189 => Create_BamlProperty_Int32AnimationUsingKeyFrames_KeyFrames(), 
			190 => Create_BamlProperty_Int64AnimationUsingKeyFrames_KeyFrames(), 
			191 => Create_BamlProperty_Italic_Inlines(), 
			192 => Create_BamlProperty_ItemsControl_Items(), 
			193 => Create_BamlProperty_ItemsPanelTemplate_VisualTree(), 
			194 => Create_BamlProperty_Label_Content(), 
			195 => Create_BamlProperty_LinearGradientBrush_GradientStops(), 
			196 => Create_BamlProperty_List_ListItems(), 
			197 => Create_BamlProperty_ListBox_Items(), 
			198 => Create_BamlProperty_ListBoxItem_Content(), 
			199 => Create_BamlProperty_ListItem_Blocks(), 
			200 => Create_BamlProperty_ListView_Items(), 
			201 => Create_BamlProperty_ListViewItem_Content(), 
			202 => Create_BamlProperty_MatrixAnimationUsingKeyFrames_KeyFrames(), 
			203 => Create_BamlProperty_Menu_Items(), 
			204 => Create_BamlProperty_MenuBase_Items(), 
			205 => Create_BamlProperty_MenuItem_Items(), 
			206 => Create_BamlProperty_ModelVisual3D_Children(), 
			207 => Create_BamlProperty_MultiBinding_Bindings(), 
			208 => Create_BamlProperty_MultiDataTrigger_Setters(), 
			209 => Create_BamlProperty_MultiTrigger_Setters(), 
			210 => Create_BamlProperty_ObjectAnimationUsingKeyFrames_KeyFrames(), 
			211 => Create_BamlProperty_PageContent_Child(), 
			212 => Create_BamlProperty_PageFunctionBase_Content(), 
			213 => Create_BamlProperty_Panel_Children(), 
			214 => Create_BamlProperty_Paragraph_Inlines(), 
			215 => Create_BamlProperty_ParallelTimeline_Children(), 
			216 => Create_BamlProperty_Point3DAnimationUsingKeyFrames_KeyFrames(), 
			217 => Create_BamlProperty_PointAnimationUsingKeyFrames_KeyFrames(), 
			218 => Create_BamlProperty_PriorityBinding_Bindings(), 
			219 => Create_BamlProperty_QuaternionAnimationUsingKeyFrames_KeyFrames(), 
			220 => Create_BamlProperty_RadialGradientBrush_GradientStops(), 
			221 => Create_BamlProperty_RadioButton_Content(), 
			222 => Create_BamlProperty_RectAnimationUsingKeyFrames_KeyFrames(), 
			223 => Create_BamlProperty_RepeatButton_Content(), 
			224 => Create_BamlProperty_RichTextBox_Document(), 
			225 => Create_BamlProperty_Rotation3DAnimationUsingKeyFrames_KeyFrames(), 
			226 => Create_BamlProperty_Run_Text(), 
			227 => Create_BamlProperty_ScrollViewer_Content(), 
			228 => Create_BamlProperty_Section_Blocks(), 
			229 => Create_BamlProperty_Selector_Items(), 
			230 => Create_BamlProperty_SingleAnimationUsingKeyFrames_KeyFrames(), 
			231 => Create_BamlProperty_SizeAnimationUsingKeyFrames_KeyFrames(), 
			232 => Create_BamlProperty_Span_Inlines(), 
			233 => Create_BamlProperty_StackPanel_Children(), 
			234 => Create_BamlProperty_StatusBar_Items(), 
			235 => Create_BamlProperty_StatusBarItem_Content(), 
			236 => Create_BamlProperty_Storyboard_Children(), 
			237 => Create_BamlProperty_StringAnimationUsingKeyFrames_KeyFrames(), 
			238 => Create_BamlProperty_Style_Setters(), 
			239 => Create_BamlProperty_TabControl_Items(), 
			240 => Create_BamlProperty_TabItem_Content(), 
			241 => Create_BamlProperty_TabPanel_Children(), 
			242 => Create_BamlProperty_Table_RowGroups(), 
			243 => Create_BamlProperty_TableCell_Blocks(), 
			244 => Create_BamlProperty_TableRow_Cells(), 
			245 => Create_BamlProperty_TableRowGroup_Rows(), 
			246 => Create_BamlProperty_TextBlock_Inlines(), 
			247 => Create_BamlProperty_ThicknessAnimationUsingKeyFrames_KeyFrames(), 
			248 => Create_BamlProperty_ToggleButton_Content(), 
			249 => Create_BamlProperty_ToolBar_Items(), 
			250 => Create_BamlProperty_ToolBarOverflowPanel_Children(), 
			251 => Create_BamlProperty_ToolBarPanel_Children(), 
			252 => Create_BamlProperty_ToolBarTray_ToolBars(), 
			253 => Create_BamlProperty_ToolTip_Content(), 
			254 => Create_BamlProperty_TreeView_Items(), 
			255 => Create_BamlProperty_TreeViewItem_Items(), 
			256 => Create_BamlProperty_Trigger_Setters(), 
			257 => Create_BamlProperty_Underline_Inlines(), 
			258 => Create_BamlProperty_UniformGrid_Children(), 
			259 => Create_BamlProperty_UserControl_Content(), 
			260 => Create_BamlProperty_Vector3DAnimationUsingKeyFrames_KeyFrames(), 
			261 => Create_BamlProperty_VectorAnimationUsingKeyFrames_KeyFrames(), 
			262 => Create_BamlProperty_Viewbox_Child(), 
			263 => Create_BamlProperty_Viewport3DVisual_Children(), 
			264 => Create_BamlProperty_VirtualizingPanel_Children(), 
			265 => Create_BamlProperty_VirtualizingStackPanel_Children(), 
			266 => Create_BamlProperty_Window_Content(), 
			267 => Create_BamlProperty_WrapPanel_Children(), 
			268 => Create_BamlProperty_XmlDataProvider_XmlSerializer(), 
			269 => Create_BamlProperty_TextBox_IsReadOnly(), 
			270 => Create_BamlProperty_RichTextBox_IsReadOnly(), 
			_ => throw new InvalidOperationException("Invalid BAML number"), 
		};
	}

	private uint GetTypeNameHashForPropeties(string typeName)
	{
		uint num = 0u;
		for (int i = 1; i < 15 && i < typeName.Length; i++)
		{
			num = 101 * num + typeName[i];
		}
		return num;
	}

	internal WpfKnownMember CreateKnownMember(string type, string property)
	{
		switch (GetTypeNameHashForPropeties(type))
		{
		case 1632072630u:
			if (property == "Text")
			{
				return GetKnownBamlMember(-1);
			}
			return null;
		case 491630740u:
			if (!(property == "Storyboard"))
			{
				if (property == "Name")
				{
					return Create_BamlProperty_BeginStoryboard_Name();
				}
				return null;
			}
			return GetKnownBamlMember(-2);
		case 381891668u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-3);
			}
			return null;
		case 3079254648u:
			return property switch
			{
				"Background" => GetKnownBamlMember(-4), 
				"BorderBrush" => GetKnownBamlMember(-5), 
				"BorderThickness" => GetKnownBamlMember(-6), 
				"Child" => GetKnownBamlMember(-145), 
				_ => null, 
			};
		case 848944877u:
			return property switch
			{
				"Command" => GetKnownBamlMember(-7), 
				"CommandParameter" => GetKnownBamlMember(-8), 
				"CommandTarget" => GetKnownBamlMember(-9), 
				"IsPressed" => GetKnownBamlMember(-10), 
				"Content" => GetKnownBamlMember(-148), 
				"ClickMode" => Create_BamlProperty_ButtonBase_ClickMode(), 
				_ => null, 
			};
		case 175175278u:
			return property switch
			{
				"MaxWidth" => GetKnownBamlMember(-11), 
				"MinWidth" => GetKnownBamlMember(-12), 
				"Width" => GetKnownBamlMember(-13), 
				_ => null, 
			};
		case 4237892661u:
			return property switch
			{
				"Content" => GetKnownBamlMember(-14), 
				"ContentTemplate" => GetKnownBamlMember(-15), 
				"ContentTemplateSelector" => GetKnownBamlMember(-16), 
				"HasContent" => GetKnownBamlMember(-17), 
				_ => null, 
			};
		case 3154930786u:
			if (property == "Focusable")
			{
				return GetKnownBamlMember(-18);
			}
			return null;
		case 3536639290u:
			return property switch
			{
				"Content" => GetKnownBamlMember(-19), 
				"ContentSource" => GetKnownBamlMember(-20), 
				"ContentTemplate" => GetKnownBamlMember(-21), 
				"ContentTemplateSelector" => GetKnownBamlMember(-22), 
				"RecognizesAccessKey" => GetKnownBamlMember(-23), 
				_ => null, 
			};
		case 1367449766u:
			return property switch
			{
				"Background" => GetKnownBamlMember(-24), 
				"BorderBrush" => GetKnownBamlMember(-25), 
				"BorderThickness" => GetKnownBamlMember(-26), 
				"FontFamily" => GetKnownBamlMember(-27), 
				"FontSize" => GetKnownBamlMember(-28), 
				"FontStretch" => GetKnownBamlMember(-29), 
				"FontStyle" => GetKnownBamlMember(-30), 
				"FontWeight" => GetKnownBamlMember(-31), 
				"Foreground" => GetKnownBamlMember(-32), 
				"HorizontalContentAlignment" => GetKnownBamlMember(-33), 
				"IsTabStop" => GetKnownBamlMember(-34), 
				"Padding" => GetKnownBamlMember(-35), 
				"TabIndex" => GetKnownBamlMember(-36), 
				"Template" => GetKnownBamlMember(-37), 
				"VerticalContentAlignment" => GetKnownBamlMember(-38), 
				_ => null, 
			};
		case 1236602933u:
			if (!(property == "LastChildFill"))
			{
				if (property == "Children")
				{
					return GetKnownBamlMember(-162);
				}
				return null;
			}
			return GetKnownBamlMember(-40);
		case 1681553739u:
			if (property == "Document")
			{
				return GetKnownBamlMember(-41);
			}
			return null;
		case 1534050549u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-42);
			}
			return null;
		case 2545195941u:
			if (property == "Document")
			{
				return GetKnownBamlMember(-43);
			}
			return null;
		case 2545205957u:
			if (property == "Document")
			{
				return GetKnownBamlMember(-44);
			}
			return null;
		case 1543401471u:
			return property switch
			{
				"Style" => GetKnownBamlMember(-45), 
				"Name" => Create_BamlProperty_FrameworkContentElement_Name(), 
				"Resources" => Create_BamlProperty_FrameworkContentElement_Resources(), 
				_ => null, 
			};
		case 767240674u:
			return property switch
			{
				"FlowDirection" => GetKnownBamlMember(-46), 
				"Height" => GetKnownBamlMember(-47), 
				"HorizontalAlignment" => GetKnownBamlMember(-48), 
				"Margin" => GetKnownBamlMember(-49), 
				"MaxHeight" => GetKnownBamlMember(-50), 
				"MaxWidth" => GetKnownBamlMember(-51), 
				"MinHeight" => GetKnownBamlMember(-52), 
				"MinWidth" => GetKnownBamlMember(-53), 
				"Name" => GetKnownBamlMember(-54), 
				"Style" => GetKnownBamlMember(-55), 
				"VerticalAlignment" => GetKnownBamlMember(-56), 
				"Width" => GetKnownBamlMember(-57), 
				"Resources" => Create_BamlProperty_FrameworkElement_Resources(), 
				"Triggers" => Create_BamlProperty_FrameworkElement_Triggers(), 
				_ => null, 
			};
		case 2497569086u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-58);
			}
			return null;
		case 2762527090u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-59);
			}
			return null;
		case 3696127683u:
			if (!(property == "GradientStops"))
			{
				if (property == "MappingMode")
				{
					return Create_BamlProperty_GradientBrush_MappingMode();
				}
				return null;
			}
			return GetKnownBamlMember(-60);
		case 1173619u:
			return property switch
			{
				"Children" => GetKnownBamlMember(-175), 
				"ColumnDefinitions" => Create_BamlProperty_Grid_ColumnDefinitions(), 
				"RowDefinitions" => Create_BamlProperty_Grid_RowDefinitions(), 
				_ => null, 
			};
		case 812041272u:
			if (property == "Header")
			{
				return GetKnownBamlMember(-65);
			}
			return null;
		case 4042892829u:
			return property switch
			{
				"HasHeader" => GetKnownBamlMember(-66), 
				"Header" => GetKnownBamlMember(-67), 
				"HeaderTemplate" => GetKnownBamlMember(-68), 
				"HeaderTemplateSelector" => GetKnownBamlMember(-69), 
				"Content" => GetKnownBamlMember(-180), 
				_ => null, 
			};
		case 3794574170u:
			return property switch
			{
				"HasHeader" => GetKnownBamlMember(-70), 
				"Header" => GetKnownBamlMember(-71), 
				"HeaderTemplate" => GetKnownBamlMember(-72), 
				"HeaderTemplateSelector" => GetKnownBamlMember(-73), 
				"Items" => GetKnownBamlMember(-181), 
				_ => null, 
			};
		case 1732790398u:
			if (!(property == "NavigateUri"))
			{
				if (property == "Inlines")
				{
					return GetKnownBamlMember(-183);
				}
				return null;
			}
			return GetKnownBamlMember(-74);
		case 113302810u:
			if (!(property == "Source"))
			{
				if (property == "Stretch")
				{
					return GetKnownBamlMember(-76);
				}
				return null;
			}
			return GetKnownBamlMember(-75);
		case 2414917938u:
			return property switch
			{
				"ItemContainerStyle" => GetKnownBamlMember(-77), 
				"ItemContainerStyleSelector" => GetKnownBamlMember(-78), 
				"ItemTemplate" => GetKnownBamlMember(-79), 
				"ItemTemplateSelector" => GetKnownBamlMember(-80), 
				"ItemsPanel" => GetKnownBamlMember(-81), 
				"ItemsSource" => GetKnownBamlMember(-82), 
				"Items" => GetKnownBamlMember(-192), 
				_ => null, 
			};
		case 1343785127u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-83);
			}
			return null;
		case 4100099324u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-84);
			}
			return null;
		case 1000001u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-85);
			}
			return null;
		case 101071616u:
			return property switch
			{
				"Background" => GetKnownBamlMember(-86), 
				"Children" => GetKnownBamlMember(-213), 
				"IsItemsHost" => Create_BamlProperty_Panel_IsItemsHost(), 
				_ => null, 
			};
		case 1001317u:
			if (property == "Data")
			{
				return GetKnownBamlMember(-87);
			}
			return null;
		case 649104411u:
			return property switch
			{
				"Segments" => GetKnownBamlMember(-88), 
				"IsClosed" => Create_BamlProperty_PathFigure_IsClosed(), 
				"IsFilled" => Create_BamlProperty_PathFigure_IsFilled(), 
				_ => null, 
			};
		case 213893085u:
			if (property == "Figures")
			{
				return GetKnownBamlMember(-89);
			}
			return null;
		case 115517852u:
			return property switch
			{
				"Child" => GetKnownBamlMember(-90), 
				"IsOpen" => GetKnownBamlMember(-91), 
				"Placement" => GetKnownBamlMember(-92), 
				"PopupAnimation" => GetKnownBamlMember(-93), 
				_ => null, 
			};
		case 2231796391u:
			return property switch
			{
				"Height" => GetKnownBamlMember(-94), 
				"MaxHeight" => GetKnownBamlMember(-95), 
				"MinHeight" => GetKnownBamlMember(-96), 
				_ => null, 
			};
		case 1540591646u:
			return property switch
			{
				"CanContentScroll" => GetKnownBamlMember(-97), 
				"HorizontalScrollBarVisibility" => GetKnownBamlMember(-98), 
				"VerticalScrollBarVisibility" => GetKnownBamlMember(-99), 
				"Content" => GetKnownBamlMember(-227), 
				_ => null, 
			};
		case 108152214u:
			return property switch
			{
				"Fill" => GetKnownBamlMember(-100), 
				"Stroke" => GetKnownBamlMember(-101), 
				"StrokeThickness" => GetKnownBamlMember(-102), 
				"StrokeLineJoin" => Create_BamlProperty_Shape_StrokeLineJoin(), 
				"StrokeStartLineCap" => Create_BamlProperty_Shape_StrokeStartLineCap(), 
				"StrokeEndLineCap" => Create_BamlProperty_Shape_StrokeEndLineCap(), 
				"Stretch" => Create_BamlProperty_Shape_Stretch(), 
				"StrokeMiterLimit" => Create_BamlProperty_Shape_StrokeMiterLimit(), 
				_ => null, 
			};
		case 1089745292u:
			return property switch
			{
				"Background" => GetKnownBamlMember(-103), 
				"FontFamily" => GetKnownBamlMember(-104), 
				"FontSize" => GetKnownBamlMember(-105), 
				"FontStretch" => GetKnownBamlMember(-106), 
				"FontStyle" => GetKnownBamlMember(-107), 
				"FontWeight" => GetKnownBamlMember(-108), 
				"Foreground" => GetKnownBamlMember(-109), 
				"Text" => GetKnownBamlMember(-110), 
				"TextDecorations" => GetKnownBamlMember(-111), 
				"TextTrimming" => GetKnownBamlMember(-112), 
				"TextWrapping" => GetKnownBamlMember(-113), 
				"Inlines" => GetKnownBamlMember(-246), 
				"TextAlignment" => Create_BamlProperty_TextBlock_TextAlignment(), 
				_ => null, 
			};
		case 385774234u:
			return property switch
			{
				"Text" => GetKnownBamlMember(-114), 
				"IsReadOnly" => GetKnownBamlMember(-269), 
				"TextWrapping" => Create_BamlProperty_TextBox_TextWrapping(), 
				"TextAlignment" => Create_BamlProperty_TextBox_TextAlignment(), 
				_ => null, 
			};
		case 2075696131u:
			return property switch
			{
				"Background" => GetKnownBamlMember(-115), 
				"FontFamily" => GetKnownBamlMember(-116), 
				"FontSize" => GetKnownBamlMember(-117), 
				"FontStretch" => GetKnownBamlMember(-118), 
				"FontStyle" => GetKnownBamlMember(-119), 
				"FontWeight" => GetKnownBamlMember(-120), 
				"Foreground" => GetKnownBamlMember(-121), 
				_ => null, 
			};
		case 3627706972u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-122);
			}
			return null;
		case 118453917u:
			return property switch
			{
				"IsDirectionReversed" => GetKnownBamlMember(-123), 
				"Maximum" => GetKnownBamlMember(-124), 
				"Minimum" => GetKnownBamlMember(-125), 
				"Orientation" => GetKnownBamlMember(-126), 
				"Value" => GetKnownBamlMember(-127), 
				"ViewportSize" => GetKnownBamlMember(-128), 
				"Thumb" => Create_BamlProperty_Track_Thumb(), 
				"IncreaseRepeatButton" => Create_BamlProperty_Track_IncreaseRepeatButton(), 
				"DecreaseRepeatButton" => Create_BamlProperty_Track_DecreaseRepeatButton(), 
				_ => null, 
			};
		case 966650152u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-129);
			}
			return null;
		case 1543239001u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-130);
			}
			return null;
		case 4081990243u:
			return property switch
			{
				"ClipToBounds" => GetKnownBamlMember(-131), 
				"Focusable" => GetKnownBamlMember(-132), 
				"IsEnabled" => GetKnownBamlMember(-133), 
				"RenderTransform" => GetKnownBamlMember(-134), 
				"Visibility" => GetKnownBamlMember(-135), 
				"Uid" => Create_BamlProperty_UIElement_Uid(), 
				"RenderTransformOrigin" => Create_BamlProperty_UIElement_RenderTransformOrigin(), 
				"SnapsToDevicePixels" => Create_BamlProperty_UIElement_SnapsToDevicePixels(), 
				"CommandBindings" => Create_BamlProperty_UIElement_CommandBindings(), 
				"InputBindings" => Create_BamlProperty_UIElement_InputBindings(), 
				"AllowDrop" => Create_BamlProperty_UIElement_AllowDrop(), 
				_ => null, 
			};
		case 1489718377u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-136);
			}
			return null;
		case 2369223502u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-138);
			}
			return null;
		case 1535690395u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-139);
			}
			return null;
		case 3699188754u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-140);
			}
			return null;
		case 1752642139u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-141);
			}
			return null;
		case 826277256u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-142);
			}
			return null;
		case 1143319u:
			if (property == "Inlines")
			{
				return GetKnownBamlMember(-143);
			}
			return null;
		case 1583456952u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-144);
			}
			return null;
		case 109056765u:
			if (!(property == "Child"))
			{
				if (property == "Bullet")
				{
					return Create_BamlProperty_BulletDecorator_Bullet();
				}
				return null;
			}
			return GetKnownBamlMember(-146);
		case 3705841878u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-147);
			}
			return null;
		case 2361592662u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-149);
			}
			return null;
		case 1618471045u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-150);
			}
			return null;
		case 2420033511u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-151);
			}
			return null;
		case 2742486520u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-152);
			}
			return null;
		case 755422265u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-153);
			}
			return null;
		case 1171637538u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-154);
			}
			return null;
		case 2979510881u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-155);
			}
			return null;
		case 3042134663u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-156);
			}
			return null;
		case 3159584246u:
			return property switch
			{
				"VisualTree" => GetKnownBamlMember(-157), 
				"Triggers" => Create_BamlProperty_ControlTemplate_Triggers(), 
				"TargetType" => Create_BamlProperty_ControlTemplate_TargetType(), 
				_ => null, 
			};
		case 1376032174u:
			return property switch
			{
				"VisualTree" => GetKnownBamlMember(-158), 
				"Triggers" => Create_BamlProperty_DataTemplate_Triggers(), 
				"DataTemplateKey" => Create_BamlProperty_DataTemplate_DataTemplateKey(), 
				"DataType" => Create_BamlProperty_DataTemplate_DataType(), 
				_ => null, 
			};
		case 1374402354u:
			return property switch
			{
				"Setters" => GetKnownBamlMember(-159), 
				"Value" => Create_BamlProperty_DataTrigger_Value(), 
				"Binding" => Create_BamlProperty_DataTrigger_Binding(), 
				_ => null, 
			};
		case 2615247465u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-160);
			}
			return null;
		case 4019572119u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-161);
			}
			return null;
		case 441893333u:
			if (property == "Document")
			{
				return GetKnownBamlMember(-163);
			}
			return null;
		case 3239315111u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-164);
			}
			return null;
		case 4284496765u:
			return property switch
			{
				"Actions" => GetKnownBamlMember(-165), 
				"RoutedEvent" => Create_BamlProperty_EventTrigger_RoutedEvent(), 
				"SourceName" => Create_BamlProperty_EventTrigger_SourceName(), 
				_ => null, 
			};
		case 4206512190u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-166);
			}
			return null;
		case 2443733648u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-167);
			}
			return null;
		case 1831113161u:
			if (property == "Pages")
			{
				return GetKnownBamlMember(-168);
			}
			return null;
		case 372593541u:
			if (property == "References")
			{
				return GetKnownBamlMember(-169);
			}
			return null;
		case 3222460427u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-170);
			}
			return null;
		case 4281390711u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-171);
			}
			return null;
		case 1742124221u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-172);
			}
			return null;
		case 2545175141u:
			if (property == "Document")
			{
				return GetKnownBamlMember(-173);
			}
			return null;
		case 3079776431u:
			return property switch
			{
				"VisualTree" => GetKnownBamlMember(-174), 
				"Template" => Create_BamlProperty_FrameworkTemplate_Template(), 
				"Resources" => Create_BamlProperty_FrameworkTemplate_Resources(), 
				_ => null, 
			};
		case 4253354066u:
			if (property == "Columns")
			{
				return GetKnownBamlMember(-176);
			}
			return null;
		case 411789920u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-177);
			}
			return null;
		case 389898151u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-178);
			}
			return null;
		case 732268889u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-179);
			}
			return null;
		case 1663174964u:
			if (property == "VisualTree")
			{
				return GetKnownBamlMember(-182);
			}
			return null;
		case 1971172509u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-184);
			}
			return null;
		case 2495415765u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-185);
			}
			return null;
		case 2232234900u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-186);
			}
			return null;
		case 3737794794u:
			if (property == "NameValue")
			{
				return GetKnownBamlMember(-187);
			}
			return null;
		case 486209962u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-188);
			}
			return null;
		case 1197649600u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-189);
			}
			return null;
		case 276220969u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-190);
			}
			return null;
		case 3582123533u:
			if (property == "Inlines")
			{
				return GetKnownBamlMember(-191);
			}
			return null;
		case 374054883u:
			if (property == "VisualTree")
			{
				return GetKnownBamlMember(-193);
			}
			return null;
		case 100949204u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-194);
			}
			return null;
		case 2575003659u:
			return property switch
			{
				"GradientStops" => GetKnownBamlMember(-195), 
				"StartPoint" => Create_BamlProperty_LinearGradientBrush_StartPoint(), 
				"EndPoint" => Create_BamlProperty_LinearGradientBrush_EndPoint(), 
				_ => null, 
			};
		case 1082836u:
			if (property == "ListItems")
			{
				return GetKnownBamlMember(-196);
			}
			return null;
		case 3251168569u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-197);
			}
			return null;
		case 2579567368u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-198);
			}
			return null;
		case 1957772275u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-199);
			}
			return null;
		case 1971053987u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-200);
			}
			return null;
		case 1169860818u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-201);
			}
			return null;
		case 3589500084u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-202);
			}
			return null;
		case 1041528u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-203);
			}
			return null;
		case 2685586543u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-204);
			}
			return null;
		case 2692991063u:
			return property switch
			{
				"Items" => GetKnownBamlMember(-205), 
				"Role" => Create_BamlProperty_MenuItem_Role(), 
				"IsChecked" => Create_BamlProperty_MenuItem_IsChecked(), 
				_ => null, 
			};
		case 3203967083u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-206);
			}
			return null;
		case 3251291345u:
			return property switch
			{
				"Bindings" => GetKnownBamlMember(-207), 
				"Converter" => Create_BamlProperty_MultiBinding_Converter(), 
				"ConverterParameter" => Create_BamlProperty_MultiBinding_ConverterParameter(), 
				_ => null, 
			};
		case 141114174u:
			if (!(property == "Setters"))
			{
				if (property == "Conditions")
				{
					return Create_BamlProperty_MultiDataTrigger_Conditions();
				}
				return null;
			}
			return GetKnownBamlMember(-208);
		case 1888893854u:
			if (!(property == "Setters"))
			{
				if (property == "Conditions")
				{
					return Create_BamlProperty_MultiTrigger_Conditions();
				}
				return null;
			}
			return GetKnownBamlMember(-209);
		case 489623484u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-210);
			}
			return null;
		case 3329578860u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-211);
			}
			return null;
		case 1539399457u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-212);
			}
			return null;
		case 1481865686u:
			if (property == "Inlines")
			{
				return GetKnownBamlMember(-214);
			}
			return null;
		case 3221949491u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-215);
			}
			return null;
		case 3250492243u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-216);
			}
			return null;
		case 4060568379u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-217);
			}
			return null;
		case 1940998317u:
			if (property == "Bindings")
			{
				return GetKnownBamlMember(-218);
			}
			return null;
		case 1366062463u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-219);
			}
			return null;
		case 3215452047u:
			if (property == "GradientStops")
			{
				return GetKnownBamlMember(-220);
			}
			return null;
		case 1262679173u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-221);
			}
			return null;
		case 4013926948u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-222);
			}
			return null;
		case 3359941107u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-223);
			}
			return null;
		case 4130373030u:
			if (!(property == "Document"))
			{
				if (property == "IsReadOnly")
				{
					return GetKnownBamlMember(-270);
				}
				return null;
			}
			return GetKnownBamlMember(-224);
		case 1536792507u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-225);
			}
			return null;
		case 11927u:
			if (property == "Text")
			{
				return GetKnownBamlMember(-226);
			}
			return null;
		case 2495870938u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-228);
			}
			return null;
		case 1509448966u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-229);
			}
			return null;
		case 3423765539u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-230);
			}
			return null;
		case 1362944236u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-231);
			}
			return null;
		case 1152419u:
			if (property == "Inlines")
			{
				return GetKnownBamlMember(-232);
			}
			return null;
		case 2127983347u:
			if (!(property == "Children"))
			{
				if (property == "Orientation")
				{
					return Create_BamlProperty_StackPanel_Orientation();
				}
				return null;
			}
			return GetKnownBamlMember(-233);
		case 3803038822u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-234);
			}
			return null;
		case 2195627365u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-235);
			}
			return null;
		case 3693620786u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-236);
			}
			return null;
		case 2579011428u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-237);
			}
			return null;
		case 120760246u:
			return property switch
			{
				"Setters" => GetKnownBamlMember(-238), 
				"TargetType" => Create_BamlProperty_Style_TargetType(), 
				"Triggers" => Create_BamlProperty_Style_Triggers(), 
				"BasedOn" => Create_BamlProperty_Style_BasedOn(), 
				"Resources" => Create_BamlProperty_Style_Resources(), 
				_ => null, 
			};
		case 671959932u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-239);
			}
			return null;
		case 3256889750u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-240);
			}
			return null;
		case 3237288451u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-241);
			}
			return null;
		case 100949904u:
			if (property == "RowGroups")
			{
				return GetKnownBamlMember(-242);
			}
			return null;
		case 3145595724u:
			if (property == "Blocks")
			{
				return GetKnownBamlMember(-243);
			}
			return null;
		case 1859848980u:
			if (property == "Cells")
			{
				return GetKnownBamlMember(-244);
			}
			return null;
		case 3051751957u:
			if (property == "Rows")
			{
				return GetKnownBamlMember(-245);
			}
			return null;
		case 2134797854u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-247);
			}
			return null;
		case 1516882570u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-248);
			}
			return null;
		case 1462776703u:
			if (!(property == "Items"))
			{
				if (property == "Orientation")
				{
					return Create_BamlProperty_ToolBar_Orientation();
				}
				return null;
			}
			return GetKnownBamlMember(-249);
		case 4085468031u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-250);
			}
			return null;
		case 1646651323u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-251);
			}
			return null;
		case 1891671667u:
			if (property == "ToolBars")
			{
				return GetKnownBamlMember(-252);
			}
			return null;
		case 1462961127u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-253);
			}
			return null;
		case 971718127u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-254);
			}
			return null;
		case 908592222u:
			if (property == "Items")
			{
				return GetKnownBamlMember(-255);
			}
			return null;
		case 2299171064u:
			return property switch
			{
				"Setters" => GetKnownBamlMember(-256), 
				"Value" => Create_BamlProperty_Trigger_Value(), 
				"SourceName" => Create_BamlProperty_Trigger_SourceName(), 
				"Property" => Create_BamlProperty_Trigger_Property(), 
				_ => null, 
			};
		case 4251506749u:
			if (property == "Inlines")
			{
				return GetKnownBamlMember(-257);
			}
			return null;
		case 3726396217u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-258);
			}
			return null;
		case 4049813583u:
			if (property == "Content")
			{
				return GetKnownBamlMember(-259);
			}
			return null;
		case 2006016895u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-260);
			}
			return null;
		case 1631317593u:
			if (property == "KeyFrames")
			{
				return GetKnownBamlMember(-261);
			}
			return null;
		case 1797740290u:
			if (property == "Child")
			{
				return GetKnownBamlMember(-262);
			}
			return null;
		case 282171645u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-263);
			}
			return null;
		case 1133493129u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-264);
			}
			return null;
		case 1133525638u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-265);
			}
			return null;
		case 2450772053u:
			return property switch
			{
				"Content" => GetKnownBamlMember(-266), 
				"ResizeMode" => Create_BamlProperty_Window_ResizeMode(), 
				"WindowState" => Create_BamlProperty_Window_WindowState(), 
				"Title" => Create_BamlProperty_Window_Title(), 
				"AllowsTransparency" => Create_BamlProperty_Window_AllowsTransparency(), 
				_ => null, 
			};
		case 15810163u:
			if (property == "Children")
			{
				return GetKnownBamlMember(-267);
			}
			return null;
		case 3607421190u:
			if (!(property == "XmlSerializer"))
			{
				if (property == "XPath")
				{
					return Create_BamlProperty_XmlDataProvider_XPath();
				}
				return null;
			}
			return GetKnownBamlMember(-268);
		case 2040874456u:
			return property switch
			{
				"Value" => Create_BamlProperty_Setter_Value(), 
				"TargetName" => Create_BamlProperty_Setter_TargetName(), 
				"Property" => Create_BamlProperty_Setter_Property(), 
				_ => null, 
			};
		case 2714779469u:
			return property switch
			{
				"Path" => Create_BamlProperty_Binding_Path(), 
				"Converter" => Create_BamlProperty_Binding_Converter(), 
				"Source" => Create_BamlProperty_Binding_Source(), 
				"RelativeSource" => Create_BamlProperty_Binding_RelativeSource(), 
				"Mode" => Create_BamlProperty_Binding_Mode(), 
				"ElementName" => Create_BamlProperty_Binding_ElementName(), 
				"UpdateSourceTrigger" => Create_BamlProperty_Binding_UpdateSourceTrigger(), 
				"XPath" => Create_BamlProperty_Binding_XPath(), 
				"ConverterParameter" => Create_BamlProperty_Binding_ConverterParameter(), 
				_ => null, 
			};
		case 2189110588u:
			if (!(property == "ResourceId"))
			{
				if (property == "TypeInTargetAssembly")
				{
					return Create_BamlProperty_ComponentResourceKey_TypeInTargetAssembly();
				}
				return null;
			}
			return Create_BamlProperty_ComponentResourceKey_ResourceId();
		case 1796721919u:
			if (property == "BeginTime")
			{
				return Create_BamlProperty_Timeline_BeginTime();
			}
			return null;
		case 4254925692u:
			return property switch
			{
				"DeferrableContent" => Create_BamlProperty_ResourceDictionary_DeferrableContent(), 
				"Source" => Create_BamlProperty_ResourceDictionary_Source(), 
				"MergedDictionaries" => Create_BamlProperty_ResourceDictionary_MergedDictionaries(), 
				_ => null, 
			};
		case 1147347271u:
			if (property == "AncestorType")
			{
				return Create_BamlProperty_RelativeSource_AncestorType();
			}
			return null;
		case 790939867u:
			if (property == "Resources")
			{
				return Create_BamlProperty_Application_Resources();
			}
			return null;
		case 2699530403u:
			if (property == "Command")
			{
				return Create_BamlProperty_CommandBinding_Command();
			}
			return null;
		case 2006957996u:
			return property switch
			{
				"Property" => Create_BamlProperty_Condition_Property(), 
				"Value" => Create_BamlProperty_Condition_Value(), 
				"Binding" => Create_BamlProperty_Condition_Binding(), 
				_ => null, 
			};
		case 137005044u:
			if (property == "FallbackValue")
			{
				return Create_BamlProperty_BindingBase_FallbackValue();
			}
			return null;
		case 350834986u:
			return property switch
			{
				"TileMode" => Create_BamlProperty_TileBrush_TileMode(), 
				"ViewboxUnits" => Create_BamlProperty_TileBrush_ViewboxUnits(), 
				"ViewportUnits" => Create_BamlProperty_TileBrush_ViewportUnits(), 
				_ => null, 
			};
		case 2108852657u:
			if (property == "Pen")
			{
				return Create_BamlProperty_GeometryDrawing_Pen();
			}
			return null;
		case 3750147462u:
			if (property == "Command")
			{
				return Create_BamlProperty_InputBinding_Command();
			}
			return null;
		case 4223882185u:
			if (!(property == "Gesture"))
			{
				if (property == "Key")
				{
					return Create_BamlProperty_KeyBinding_Key();
				}
				return null;
			}
			return Create_BamlProperty_KeyBinding_Gesture();
		case 3936355701u:
			if (property == "Orientation")
			{
				return Create_BamlProperty_ScrollBar_Orientation();
			}
			return null;
		case 837389320u:
			if (property == "SharedSizeGroup")
			{
				return Create_BamlProperty_DefinitionBase_SharedSizeGroup();
			}
			return null;
		case 112414925u:
			if (property == "TextAlignment")
			{
				return Create_BamlProperty_Block_TextAlignment();
			}
			return null;
		case 10311u:
			if (property == "LineJoin")
			{
				return Create_BamlProperty_Pen_LineJoin();
			}
			return null;
		case 2246554763u:
			if (property == "Color")
			{
				return Create_BamlProperty_SolidColorBrush_Color();
			}
			return null;
		case 118659550u:
			if (property == "Opacity")
			{
				return Create_BamlProperty_Brush_Opacity();
			}
			return null;
		case 3484345457u:
			return property switch
			{
				"AcceptsTab" => Create_BamlProperty_TextBoxBase_AcceptsTab(), 
				"VerticalScrollBarVisibility" => Create_BamlProperty_TextBoxBase_VerticalScrollBarVisibility(), 
				"HorizontalScrollBarVisibility" => Create_BamlProperty_TextBoxBase_HorizontalScrollBarVisibility(), 
				_ => null, 
			};
		case 2067968796u:
			if (property == "IsStroked")
			{
				return Create_BamlProperty_PathSegment_IsStroked();
			}
			return null;
		case 118454921u:
			if (!(property == "JournalOwnership"))
			{
				if (property == "NavigationUIVisibility")
				{
					return Create_BamlProperty_Frame_NavigationUIVisibility();
				}
				return null;
			}
			return Create_BamlProperty_Frame_JournalOwnership();
		case 4272319926u:
			if (property == "ObjectType")
			{
				return Create_BamlProperty_ObjectDataProvider_ObjectType();
			}
			return null;
		default:
			return null;
		}
	}

	internal WpfKnownMember CreateKnownAttachableMember(string type, string property)
	{
		switch (GetTypeNameHashForPropeties(type))
		{
		case 1236602933u:
			if (property == "Dock")
			{
				return GetKnownBamlMember(-39);
			}
			return null;
		case 1173619u:
			return property switch
			{
				"Column" => GetKnownBamlMember(-61), 
				"ColumnSpan" => GetKnownBamlMember(-62), 
				"Row" => GetKnownBamlMember(-63), 
				"RowSpan" => GetKnownBamlMember(-64), 
				_ => null, 
			};
		case 1618471045u:
			return property switch
			{
				"Top" => Create_BamlProperty_Canvas_Top(), 
				"Left" => Create_BamlProperty_Canvas_Left(), 
				"Bottom" => Create_BamlProperty_Canvas_Bottom(), 
				"Right" => Create_BamlProperty_Canvas_Right(), 
				_ => null, 
			};
		case 1509448966u:
			if (property == "IsSelected")
			{
				return Create_BamlProperty_Selector_IsSelected();
			}
			return null;
		case 3693620786u:
			if (!(property == "TargetName"))
			{
				if (property == "TargetProperty")
				{
					return Create_BamlProperty_Storyboard_TargetProperty();
				}
				return null;
			}
			return Create_BamlProperty_Storyboard_TargetName();
		case 1133493129u:
			if (property == "IsVirtualizing")
			{
				return Create_BamlProperty_VirtualizingPanel_IsVirtualizing();
			}
			return null;
		case 3749867153u:
			if (property == "NameScope")
			{
				return Create_BamlProperty_NameScope_NameScope();
			}
			return null;
		case 378630271u:
			if (property == "JournalEntryPosition")
			{
				return Create_BamlProperty_JournalEntryUnifiedViewConverter_JournalEntryPosition();
			}
			return null;
		case 249275044u:
			if (!(property == "DirectionalNavigation"))
			{
				if (property == "TabNavigation")
				{
					return Create_BamlProperty_KeyboardNavigation_TabNavigation();
				}
				return null;
			}
			return Create_BamlProperty_KeyboardNavigation_DirectionalNavigation();
		case 3951806740u:
			if (property == "ToolTip")
			{
				return Create_BamlProperty_ToolTipService_ToolTip();
			}
			return null;
		default:
			return null;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_AccessText_Text()
	{
		DependencyProperty textProperty = AccessText.TextProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(AccessText)), "Text", textProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BeginStoryboard_Storyboard()
	{
		DependencyProperty storyboardProperty = BeginStoryboard.StoryboardProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BeginStoryboard)), "Storyboard", storyboardProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BitmapEffectGroup_Children()
	{
		DependencyProperty childrenProperty = BitmapEffectGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BitmapEffectGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Border_Background()
	{
		DependencyProperty backgroundProperty = Border.BackgroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Border)), "Background", backgroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Border_BorderBrush()
	{
		DependencyProperty borderBrushProperty = Border.BorderBrushProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Border)), "BorderBrush", borderBrushProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Border_BorderThickness()
	{
		DependencyProperty borderThicknessProperty = Border.BorderThicknessProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Border)), "BorderThickness", borderThicknessProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ThicknessConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_Command()
	{
		DependencyProperty commandProperty = ButtonBase.CommandProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "Command", commandProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(CommandConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_CommandParameter()
	{
		DependencyProperty commandParameterProperty = ButtonBase.CommandParameterProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "CommandParameter", commandParameterProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_CommandTarget()
	{
		DependencyProperty commandTargetProperty = ButtonBase.CommandTargetProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "CommandTarget", commandTargetProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_IsPressed()
	{
		DependencyProperty isPressedProperty = ButtonBase.IsPressedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "IsPressed", isPressedProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ColumnDefinition_MaxWidth()
	{
		DependencyProperty maxWidthProperty = ColumnDefinition.MaxWidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ColumnDefinition)), "MaxWidth", maxWidthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ColumnDefinition_MinWidth()
	{
		DependencyProperty minWidthProperty = ColumnDefinition.MinWidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ColumnDefinition)), "MinWidth", minWidthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ColumnDefinition_Width()
	{
		DependencyProperty widthProperty = ColumnDefinition.WidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ColumnDefinition)), "Width", widthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(GridLengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentControl_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentControl)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentControl_ContentTemplate()
	{
		DependencyProperty contentTemplateProperty = ContentControl.ContentTemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentControl)), "ContentTemplate", contentTemplateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentControl_ContentTemplateSelector()
	{
		DependencyProperty contentTemplateSelectorProperty = ContentControl.ContentTemplateSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentControl)), "ContentTemplateSelector", contentTemplateSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentControl_HasContent()
	{
		DependencyProperty hasContentProperty = ContentControl.HasContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentControl)), "HasContent", hasContentProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentElement_Focusable()
	{
		DependencyProperty focusableProperty = ContentElement.FocusableProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentElement)), "Focusable", focusableProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentPresenter_Content()
	{
		DependencyProperty contentProperty = ContentPresenter.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentPresenter)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentPresenter_ContentSource()
	{
		DependencyProperty contentSourceProperty = ContentPresenter.ContentSourceProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentPresenter)), "ContentSource", contentSourceProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentPresenter_ContentTemplate()
	{
		DependencyProperty contentTemplateProperty = ContentPresenter.ContentTemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentPresenter)), "ContentTemplate", contentTemplateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentPresenter_ContentTemplateSelector()
	{
		DependencyProperty contentTemplateSelectorProperty = ContentPresenter.ContentTemplateSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentPresenter)), "ContentTemplateSelector", contentTemplateSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContentPresenter_RecognizesAccessKey()
	{
		DependencyProperty recognizesAccessKeyProperty = ContentPresenter.RecognizesAccessKeyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContentPresenter)), "RecognizesAccessKey", recognizesAccessKeyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_Background()
	{
		DependencyProperty backgroundProperty = Control.BackgroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "Background", backgroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_BorderBrush()
	{
		DependencyProperty borderBrushProperty = Control.BorderBrushProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "BorderBrush", borderBrushProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_BorderThickness()
	{
		DependencyProperty borderThicknessProperty = Control.BorderThicknessProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "BorderThickness", borderThicknessProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ThicknessConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_FontFamily()
	{
		DependencyProperty fontFamilyProperty = Control.FontFamilyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "FontFamily", fontFamilyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontFamilyConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_FontSize()
	{
		DependencyProperty fontSizeProperty = Control.FontSizeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "FontSize", fontSizeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontSizeConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_FontStretch()
	{
		DependencyProperty fontStretchProperty = Control.FontStretchProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "FontStretch", fontStretchProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStretchConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_FontStyle()
	{
		DependencyProperty fontStyleProperty = Control.FontStyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "FontStyle", fontStyleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStyleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_FontWeight()
	{
		DependencyProperty fontWeightProperty = Control.FontWeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "FontWeight", fontWeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontWeightConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_Foreground()
	{
		DependencyProperty foregroundProperty = Control.ForegroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "Foreground", foregroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_HorizontalContentAlignment()
	{
		DependencyProperty horizontalContentAlignmentProperty = Control.HorizontalContentAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "HorizontalContentAlignment", horizontalContentAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(HorizontalAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_IsTabStop()
	{
		DependencyProperty isTabStopProperty = Control.IsTabStopProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "IsTabStop", isTabStopProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_Padding()
	{
		DependencyProperty paddingProperty = Control.PaddingProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "Padding", paddingProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ThicknessConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_TabIndex()
	{
		DependencyProperty tabIndexProperty = Control.TabIndexProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "TabIndex", tabIndexProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Int32Converter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_Template()
	{
		DependencyProperty templateProperty = Control.TemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "Template", templateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Control_VerticalContentAlignment()
	{
		DependencyProperty verticalContentAlignmentProperty = Control.VerticalContentAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Control)), "VerticalContentAlignment", verticalContentAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(VerticalAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DockPanel_Dock()
	{
		DependencyProperty dockProperty = DockPanel.DockProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DockPanel)), "Dock", dockProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(Dock);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DockPanel_LastChildFill()
	{
		DependencyProperty lastChildFillProperty = DockPanel.LastChildFillProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DockPanel)), "LastChildFill", lastChildFillProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DocumentViewerBase_Document()
	{
		DependencyProperty documentProperty = DocumentViewerBase.DocumentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DocumentViewerBase)), "Document", documentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DrawingGroup_Children()
	{
		DependencyProperty childrenProperty = DrawingGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DrawingGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FlowDocumentReader_Document()
	{
		DependencyProperty documentProperty = FlowDocumentReader.DocumentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FlowDocumentReader)), "Document", documentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FlowDocumentScrollViewer_Document()
	{
		DependencyProperty documentProperty = FlowDocumentScrollViewer.DocumentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FlowDocumentScrollViewer)), "Document", documentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkContentElement_Style()
	{
		DependencyProperty styleProperty = FrameworkContentElement.StyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkContentElement)), "Style", styleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_FlowDirection()
	{
		DependencyProperty flowDirectionProperty = FrameworkElement.FlowDirectionProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "FlowDirection", flowDirectionProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FlowDirection);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Height()
	{
		DependencyProperty heightProperty = FrameworkElement.HeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Height", heightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_HorizontalAlignment()
	{
		DependencyProperty horizontalAlignmentProperty = FrameworkElement.HorizontalAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "HorizontalAlignment", horizontalAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(HorizontalAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Margin()
	{
		DependencyProperty marginProperty = FrameworkElement.MarginProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Margin", marginProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ThicknessConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_MaxHeight()
	{
		DependencyProperty maxHeightProperty = FrameworkElement.MaxHeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "MaxHeight", maxHeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_MaxWidth()
	{
		DependencyProperty maxWidthProperty = FrameworkElement.MaxWidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "MaxWidth", maxWidthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_MinHeight()
	{
		DependencyProperty minHeightProperty = FrameworkElement.MinHeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "MinHeight", minHeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_MinWidth()
	{
		DependencyProperty minWidthProperty = FrameworkElement.MinWidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "MinWidth", minWidthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Name()
	{
		DependencyProperty nameProperty = FrameworkElement.NameProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Name", nameProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Style()
	{
		DependencyProperty styleProperty = FrameworkElement.StyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Style", styleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_VerticalAlignment()
	{
		DependencyProperty verticalAlignmentProperty = FrameworkElement.VerticalAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "VerticalAlignment", verticalAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(VerticalAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Width()
	{
		DependencyProperty widthProperty = FrameworkElement.WidthProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Width", widthProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GeneralTransformGroup_Children()
	{
		DependencyProperty childrenProperty = GeneralTransformGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GeneralTransformGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GeometryGroup_Children()
	{
		DependencyProperty childrenProperty = GeometryGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GeometryGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GradientBrush_GradientStops()
	{
		DependencyProperty gradientStopsProperty = GradientBrush.GradientStopsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GradientBrush)), "GradientStops", gradientStopsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_Column()
	{
		DependencyProperty columnProperty = Grid.ColumnProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "Column", columnProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(Int32Converter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_ColumnSpan()
	{
		DependencyProperty columnSpanProperty = Grid.ColumnSpanProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "ColumnSpan", columnSpanProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(Int32Converter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_Row()
	{
		DependencyProperty rowProperty = Grid.RowProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "Row", rowProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(Int32Converter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_RowSpan()
	{
		DependencyProperty rowSpanProperty = Grid.RowSpanProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "RowSpan", rowSpanProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(Int32Converter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GridViewColumn_Header()
	{
		DependencyProperty headerProperty = GridViewColumn.HeaderProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GridViewColumn)), "Header", headerProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedContentControl_HasHeader()
	{
		DependencyProperty hasHeaderProperty = HeaderedContentControl.HasHeaderProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedContentControl)), "HasHeader", hasHeaderProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedContentControl_Header()
	{
		DependencyProperty headerProperty = HeaderedContentControl.HeaderProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedContentControl)), "Header", headerProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedContentControl_HeaderTemplate()
	{
		DependencyProperty headerTemplateProperty = HeaderedContentControl.HeaderTemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedContentControl)), "HeaderTemplate", headerTemplateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedContentControl_HeaderTemplateSelector()
	{
		DependencyProperty headerTemplateSelectorProperty = HeaderedContentControl.HeaderTemplateSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedContentControl)), "HeaderTemplateSelector", headerTemplateSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedItemsControl_HasHeader()
	{
		DependencyProperty hasHeaderProperty = HeaderedItemsControl.HasHeaderProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedItemsControl)), "HasHeader", hasHeaderProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedItemsControl_Header()
	{
		DependencyProperty headerProperty = HeaderedItemsControl.HeaderProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedItemsControl)), "Header", headerProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedItemsControl_HeaderTemplate()
	{
		DependencyProperty headerTemplateProperty = HeaderedItemsControl.HeaderTemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedItemsControl)), "HeaderTemplate", headerTemplateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedItemsControl_HeaderTemplateSelector()
	{
		DependencyProperty headerTemplateSelectorProperty = HeaderedItemsControl.HeaderTemplateSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedItemsControl)), "HeaderTemplateSelector", headerTemplateSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Hyperlink_NavigateUri()
	{
		DependencyProperty navigateUriProperty = Hyperlink.NavigateUriProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Hyperlink)), "NavigateUri", navigateUriProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(UriTypeConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Image_Source()
	{
		DependencyProperty sourceProperty = Image.SourceProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Image)), "Source", sourceProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Image_Stretch()
	{
		DependencyProperty stretchProperty = Image.StretchProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Image)), "Stretch", stretchProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Stretch);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemContainerStyle()
	{
		DependencyProperty itemContainerStyleProperty = ItemsControl.ItemContainerStyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemContainerStyle", itemContainerStyleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemContainerStyleSelector()
	{
		DependencyProperty itemContainerStyleSelectorProperty = ItemsControl.ItemContainerStyleSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemContainerStyleSelector", itemContainerStyleSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemTemplate()
	{
		DependencyProperty itemTemplateProperty = ItemsControl.ItemTemplateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemTemplate", itemTemplateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemTemplateSelector()
	{
		DependencyProperty itemTemplateSelectorProperty = ItemsControl.ItemTemplateSelectorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemTemplateSelector", itemTemplateSelectorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemsPanel()
	{
		DependencyProperty itemsPanelProperty = ItemsControl.ItemsPanelProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemsPanel", itemsPanelProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_ItemsSource()
	{
		DependencyProperty itemsSourceProperty = ItemsControl.ItemsSourceProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "ItemsSource", itemsSourceProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MaterialGroup_Children()
	{
		DependencyProperty childrenProperty = MaterialGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MaterialGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Model3DGroup_Children()
	{
		DependencyProperty childrenProperty = Model3DGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Model3DGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Page_Content()
	{
		DependencyProperty contentProperty = Page.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Page)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Panel_Background()
	{
		DependencyProperty backgroundProperty = Panel.BackgroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Panel)), "Background", backgroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Path_Data()
	{
		DependencyProperty dataProperty = Path.DataProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Path)), "Data", dataProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(GeometryConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PathFigure_Segments()
	{
		DependencyProperty segmentsProperty = PathFigure.SegmentsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PathFigure)), "Segments", segmentsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PathGeometry_Figures()
	{
		DependencyProperty figuresProperty = PathGeometry.FiguresProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PathGeometry)), "Figures", figuresProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PathFigureCollectionConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Popup_Child()
	{
		DependencyProperty childProperty = Popup.ChildProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Popup)), "Child", childProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Popup_IsOpen()
	{
		DependencyProperty isOpenProperty = Popup.IsOpenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Popup)), "IsOpen", isOpenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Popup_Placement()
	{
		DependencyProperty placementProperty = Popup.PlacementProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Popup)), "Placement", placementProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PlacementMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Popup_PopupAnimation()
	{
		DependencyProperty popupAnimationProperty = Popup.PopupAnimationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Popup)), "PopupAnimation", popupAnimationProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PopupAnimation);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RowDefinition_Height()
	{
		DependencyProperty heightProperty = RowDefinition.HeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RowDefinition)), "Height", heightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(GridLengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RowDefinition_MaxHeight()
	{
		DependencyProperty maxHeightProperty = RowDefinition.MaxHeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RowDefinition)), "MaxHeight", maxHeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RowDefinition_MinHeight()
	{
		DependencyProperty minHeightProperty = RowDefinition.MinHeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RowDefinition)), "MinHeight", minHeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ScrollViewer_CanContentScroll()
	{
		DependencyProperty canContentScrollProperty = ScrollViewer.CanContentScrollProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ScrollViewer)), "CanContentScroll", canContentScrollProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ScrollViewer_HorizontalScrollBarVisibility()
	{
		DependencyProperty horizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ScrollViewer)), "HorizontalScrollBarVisibility", horizontalScrollBarVisibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ScrollBarVisibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ScrollViewer_VerticalScrollBarVisibility()
	{
		DependencyProperty verticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ScrollViewer)), "VerticalScrollBarVisibility", verticalScrollBarVisibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ScrollBarVisibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_Fill()
	{
		DependencyProperty fillProperty = Shape.FillProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "Fill", fillProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_Stroke()
	{
		DependencyProperty strokeProperty = Shape.StrokeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "Stroke", strokeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_StrokeThickness()
	{
		DependencyProperty strokeThicknessProperty = Shape.StrokeThicknessProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "StrokeThickness", strokeThicknessProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_Background()
	{
		DependencyProperty backgroundProperty = TextBlock.BackgroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "Background", backgroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_FontFamily()
	{
		DependencyProperty fontFamilyProperty = TextBlock.FontFamilyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "FontFamily", fontFamilyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontFamilyConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_FontSize()
	{
		DependencyProperty fontSizeProperty = TextBlock.FontSizeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "FontSize", fontSizeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontSizeConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_FontStretch()
	{
		DependencyProperty fontStretchProperty = TextBlock.FontStretchProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "FontStretch", fontStretchProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStretchConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_FontStyle()
	{
		DependencyProperty fontStyleProperty = TextBlock.FontStyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "FontStyle", fontStyleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStyleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_FontWeight()
	{
		DependencyProperty fontWeightProperty = TextBlock.FontWeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "FontWeight", fontWeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontWeightConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_Foreground()
	{
		DependencyProperty foregroundProperty = TextBlock.ForegroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "Foreground", foregroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_Text()
	{
		DependencyProperty textProperty = TextBlock.TextProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "Text", textProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_TextDecorations()
	{
		DependencyProperty textDecorationsProperty = TextBlock.TextDecorationsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "TextDecorations", textDecorationsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextDecorationCollectionConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_TextTrimming()
	{
		DependencyProperty textTrimmingProperty = TextBlock.TextTrimmingProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "TextTrimming", textTrimmingProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextTrimming);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_TextWrapping()
	{
		DependencyProperty textWrappingProperty = TextBlock.TextWrappingProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "TextWrapping", textWrappingProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextWrapping);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBox_Text()
	{
		DependencyProperty textProperty = TextBox.TextProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBox)), "Text", textProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBox_IsReadOnly()
	{
		DependencyProperty isReadOnlyProperty = TextBoxBase.IsReadOnlyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBox)), "IsReadOnly", isReadOnlyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_Background()
	{
		DependencyProperty backgroundProperty = TextElement.BackgroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "Background", backgroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_FontFamily()
	{
		DependencyProperty fontFamilyProperty = TextElement.FontFamilyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "FontFamily", fontFamilyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontFamilyConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_FontSize()
	{
		DependencyProperty fontSizeProperty = TextElement.FontSizeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "FontSize", fontSizeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontSizeConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_FontStretch()
	{
		DependencyProperty fontStretchProperty = TextElement.FontStretchProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "FontStretch", fontStretchProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStretchConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_FontStyle()
	{
		DependencyProperty fontStyleProperty = TextElement.FontStyleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "FontStyle", fontStyleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontStyleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_FontWeight()
	{
		DependencyProperty fontWeightProperty = TextElement.FontWeightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "FontWeight", fontWeightProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(FontWeightConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextElement_Foreground()
	{
		DependencyProperty foregroundProperty = TextElement.ForegroundProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextElement)), "Foreground", foregroundProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TimelineGroup_Children()
	{
		DependencyProperty childrenProperty = TimelineGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TimelineGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_IsDirectionReversed()
	{
		DependencyProperty isDirectionReversedProperty = Track.IsDirectionReversedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "IsDirectionReversed", isDirectionReversedProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_Maximum()
	{
		DependencyProperty maximumProperty = Track.MaximumProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "Maximum", maximumProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_Minimum()
	{
		DependencyProperty minimumProperty = Track.MinimumProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "Minimum", minimumProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_Orientation()
	{
		DependencyProperty orientationProperty = Track.OrientationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "Orientation", orientationProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Orientation);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_Value()
	{
		DependencyProperty valueProperty = Track.ValueProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "Value", valueProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_ViewportSize()
	{
		DependencyProperty viewportSizeProperty = Track.ViewportSizeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "ViewportSize", viewportSizeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Transform3DGroup_Children()
	{
		DependencyProperty childrenProperty = Transform3DGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Transform3DGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TransformGroup_Children()
	{
		DependencyProperty childrenProperty = TransformGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TransformGroup)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_ClipToBounds()
	{
		DependencyProperty clipToBoundsProperty = UIElement.ClipToBoundsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "ClipToBounds", clipToBoundsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_Focusable()
	{
		DependencyProperty focusableProperty = UIElement.FocusableProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "Focusable", focusableProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_IsEnabled()
	{
		DependencyProperty isEnabledProperty = UIElement.IsEnabledProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "IsEnabled", isEnabledProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_RenderTransform()
	{
		DependencyProperty renderTransformProperty = UIElement.RenderTransformProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "RenderTransform", renderTransformProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TransformConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_Visibility()
	{
		DependencyProperty visibilityProperty = UIElement.VisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "Visibility", visibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Visibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Viewport3D_Children()
	{
		DependencyProperty childrenProperty = Viewport3D.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Viewport3D)), "Children", childrenProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_AdornedElementPlaceholder_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(AdornedElementPlaceholder)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((AdornedElementPlaceholder)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((AdornedElementPlaceholder)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_AdornerDecorator_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(AdornerDecorator)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((AdornerDecorator)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((AdornerDecorator)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_AnchoredBlock_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(AnchoredBlock)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((AnchoredBlock)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ArrayExtension_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ArrayExtension)), "Items", typeof(IList), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ArrayExtension)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BlockUIContainer_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BlockUIContainer)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BlockUIContainer)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BlockUIContainer)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Bold_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Bold)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Bold)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BooleanAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BooleanAnimationUsingKeyFrames)), "KeyFrames", typeof(BooleanKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BooleanAnimationUsingKeyFrames)target).KeyFrames = (BooleanKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BooleanAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Border_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Border)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Border)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Border)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BulletDecorator_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BulletDecorator)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BulletDecorator)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BulletDecorator)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Button_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Button)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ByteAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ByteAnimationUsingKeyFrames)), "KeyFrames", typeof(ByteKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ByteAnimationUsingKeyFrames)target).KeyFrames = (ByteKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ByteAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Canvas_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Canvas)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Canvas)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_CharAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(CharAnimationUsingKeyFrames)), "KeyFrames", typeof(CharKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((CharAnimationUsingKeyFrames)target).KeyFrames = (CharKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((CharAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_CheckBox_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(CheckBox)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ColorAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ColorAnimationUsingKeyFrames)), "KeyFrames", typeof(ColorKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ColorAnimationUsingKeyFrames)target).KeyFrames = (ColorKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ColorAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ComboBox_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ComboBox)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ComboBox)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ComboBoxItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ComboBoxItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ContextMenu_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ContextMenu)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ContextMenu)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ControlTemplate_VisualTree()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ControlTemplate)), "VisualTree", typeof(FrameworkElementFactory), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ControlTemplate)target).VisualTree = (FrameworkElementFactory)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ControlTemplate)target).VisualTree;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTemplate_VisualTree()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTemplate)), "VisualTree", typeof(FrameworkElementFactory), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DataTemplate)target).VisualTree = (FrameworkElementFactory)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DataTemplate)target).VisualTree;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTrigger_Setters()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTrigger)), "Setters", typeof(SetterBaseCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((DataTrigger)target).Setters;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DecimalAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DecimalAnimationUsingKeyFrames)), "KeyFrames", typeof(DecimalKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DecimalAnimationUsingKeyFrames)target).KeyFrames = (DecimalKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DecimalAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Decorator_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Decorator)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Decorator)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Decorator)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DockPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DockPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((DockPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DocumentViewer_Document()
	{
		DependencyProperty documentProperty = DocumentViewerBase.DocumentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DocumentViewer)), "Document", documentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DoubleAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DoubleAnimationUsingKeyFrames)), "KeyFrames", typeof(DoubleKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DoubleAnimationUsingKeyFrames)target).KeyFrames = (DoubleKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DoubleAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_EventTrigger_Actions()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(EventTrigger)), "Actions", typeof(TriggerActionCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((EventTrigger)target).Actions;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Expander_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Expander)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Figure_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Figure)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Figure)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FixedDocument_Pages()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FixedDocument)), "Pages", typeof(PageContentCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((FixedDocument)target).Pages;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FixedDocumentSequence_References()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FixedDocumentSequence)), "References", typeof(DocumentReferenceCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((FixedDocumentSequence)target).References;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FixedPage_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FixedPage)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((FixedPage)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Floater_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Floater)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Floater)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FlowDocument_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FlowDocument)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((FlowDocument)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FlowDocumentPageViewer_Document()
	{
		DependencyProperty documentProperty = DocumentViewerBase.DocumentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FlowDocumentPageViewer)), "Document", documentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkTemplate_VisualTree()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkTemplate)), "VisualTree", typeof(FrameworkElementFactory), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((FrameworkTemplate)target).VisualTree = (FrameworkElementFactory)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkTemplate)target).VisualTree;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Grid)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GridView_Columns()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GridView)), "Columns", typeof(GridViewColumnCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((GridView)target).Columns;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GridViewColumnHeader_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GridViewColumnHeader)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GroupBox_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GroupBox)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GroupItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GroupItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedContentControl_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedContentControl)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HeaderedItemsControl_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HeaderedItemsControl)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((HeaderedItemsControl)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_HierarchicalDataTemplate_VisualTree()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(HierarchicalDataTemplate)), "VisualTree", typeof(FrameworkElementFactory), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((HierarchicalDataTemplate)target).VisualTree = (FrameworkElementFactory)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((HierarchicalDataTemplate)target).VisualTree;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Hyperlink_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Hyperlink)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Hyperlink)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_InkCanvas_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(InkCanvas)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((InkCanvas)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_InkPresenter_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(InkPresenter)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((InkPresenter)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((InkPresenter)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_InlineUIContainer_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(InlineUIContainer)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((InlineUIContainer)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((InlineUIContainer)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_InputScopeName_NameValue()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(InputScopeName)), "NameValue", typeof(InputScopeNameValue), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(InputScopeNameValue);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((InputScopeName)target).NameValue = (InputScopeNameValue)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((InputScopeName)target).NameValue;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Int16AnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Int16AnimationUsingKeyFrames)), "KeyFrames", typeof(Int16KeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Int16AnimationUsingKeyFrames)target).KeyFrames = (Int16KeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Int16AnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Int32AnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Int32AnimationUsingKeyFrames)), "KeyFrames", typeof(Int32KeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Int32AnimationUsingKeyFrames)target).KeyFrames = (Int32KeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Int32AnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Int64AnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Int64AnimationUsingKeyFrames)), "KeyFrames", typeof(Int64KeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Int64AnimationUsingKeyFrames)target).KeyFrames = (Int64KeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Int64AnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Italic_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Italic)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Italic)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsControl_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsControl)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ItemsControl)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ItemsPanelTemplate_VisualTree()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ItemsPanelTemplate)), "VisualTree", typeof(FrameworkElementFactory), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ItemsPanelTemplate)target).VisualTree = (FrameworkElementFactory)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ItemsPanelTemplate)target).VisualTree;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Label_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Label)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_LinearGradientBrush_GradientStops()
	{
		DependencyProperty gradientStopsProperty = GradientBrush.GradientStopsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(LinearGradientBrush)), "GradientStops", gradientStopsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_List_ListItems()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(List)), "ListItems", typeof(ListItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((List)target).ListItems;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ListBox_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ListBox)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ListBox)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ListBoxItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ListBoxItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ListItem_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ListItem)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ListItem)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ListView_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ListView)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ListView)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ListViewItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ListViewItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MatrixAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MatrixAnimationUsingKeyFrames)), "KeyFrames", typeof(MatrixKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((MatrixAnimationUsingKeyFrames)target).KeyFrames = (MatrixKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((MatrixAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Menu_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Menu)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Menu)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MenuBase_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MenuBase)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MenuBase)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MenuItem_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MenuItem)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MenuItem)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ModelVisual3D_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ModelVisual3D)), "Children", typeof(Visual3DCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ModelVisual3D)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiBinding_Bindings()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiBinding)), "Bindings", typeof(Collection<BindingBase>), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MultiBinding)target).Bindings;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiDataTrigger_Setters()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiDataTrigger)), "Setters", typeof(SetterBaseCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MultiDataTrigger)target).Setters;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiTrigger_Setters()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiTrigger)), "Setters", typeof(SetterBaseCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MultiTrigger)target).Setters;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ObjectAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ObjectAnimationUsingKeyFrames)), "KeyFrames", typeof(ObjectKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ObjectAnimationUsingKeyFrames)target).KeyFrames = (ObjectKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ObjectAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PageContent_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PageContent)), "Child", typeof(FixedPage), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((PageContent)target).Child = (FixedPage)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((PageContent)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PageFunctionBase_Content()
	{
		DependencyProperty contentProperty = Page.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PageFunctionBase)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Panel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Panel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Panel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Paragraph_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Paragraph)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Paragraph)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ParallelTimeline_Children()
	{
		DependencyProperty childrenProperty = TimelineGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ParallelTimeline)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Point3DAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Point3DAnimationUsingKeyFrames)), "KeyFrames", typeof(Point3DKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Point3DAnimationUsingKeyFrames)target).KeyFrames = (Point3DKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Point3DAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PointAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PointAnimationUsingKeyFrames)), "KeyFrames", typeof(PointKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((PointAnimationUsingKeyFrames)target).KeyFrames = (PointKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((PointAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PriorityBinding_Bindings()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PriorityBinding)), "Bindings", typeof(Collection<BindingBase>), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((PriorityBinding)target).Bindings;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_QuaternionAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(QuaternionAnimationUsingKeyFrames)), "KeyFrames", typeof(QuaternionKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((QuaternionAnimationUsingKeyFrames)target).KeyFrames = (QuaternionKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((QuaternionAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RadialGradientBrush_GradientStops()
	{
		DependencyProperty gradientStopsProperty = GradientBrush.GradientStopsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RadialGradientBrush)), "GradientStops", gradientStopsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RadioButton_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RadioButton)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RectAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RectAnimationUsingKeyFrames)), "KeyFrames", typeof(RectKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((RectAnimationUsingKeyFrames)target).KeyFrames = (RectKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((RectAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RepeatButton_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RepeatButton)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RichTextBox_Document()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RichTextBox)), "Document", typeof(FlowDocument), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((RichTextBox)target).Document = (FlowDocument)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((RichTextBox)target).Document;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RichTextBox_IsReadOnly()
	{
		DependencyProperty isReadOnlyProperty = TextBoxBase.IsReadOnlyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RichTextBox)), "IsReadOnly", isReadOnlyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Rotation3DAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Rotation3DAnimationUsingKeyFrames)), "KeyFrames", typeof(Rotation3DKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Rotation3DAnimationUsingKeyFrames)target).KeyFrames = (Rotation3DKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Rotation3DAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Run_Text()
	{
		DependencyProperty textProperty = Run.TextProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Run)), "Text", textProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ScrollViewer_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ScrollViewer)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Section_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Section)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Section)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Selector_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Selector)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Selector)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_SingleAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(SingleAnimationUsingKeyFrames)), "KeyFrames", typeof(SingleKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((SingleAnimationUsingKeyFrames)target).KeyFrames = (SingleKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((SingleAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_SizeAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(SizeAnimationUsingKeyFrames)), "KeyFrames", typeof(SizeKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((SizeAnimationUsingKeyFrames)target).KeyFrames = (SizeKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((SizeAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Span_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Span)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Span)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_StackPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(StackPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((StackPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_StatusBar_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(StatusBar)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((StatusBar)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_StatusBarItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(StatusBarItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Storyboard_Children()
	{
		DependencyProperty childrenProperty = TimelineGroup.ChildrenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Storyboard)), "Children", childrenProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_StringAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(StringAnimationUsingKeyFrames)), "KeyFrames", typeof(StringKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((StringAnimationUsingKeyFrames)target).KeyFrames = (StringKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((StringAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Style_Setters()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Style)), "Setters", typeof(SetterBaseCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Style)target).Setters;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TabControl_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TabControl)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TabControl)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TabItem_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TabItem)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TabPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TabPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TabPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Table_RowGroups()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Table)), "RowGroups", typeof(TableRowGroupCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Table)target).RowGroups;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TableCell_Blocks()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TableCell)), "Blocks", typeof(BlockCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TableCell)target).Blocks;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TableRow_Cells()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TableRow)), "Cells", typeof(TableCellCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TableRow)target).Cells;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TableRowGroup_Rows()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TableRowGroup)), "Rows", typeof(TableRowCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TableRowGroup)target).Rows;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TextBlock)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ThicknessAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ThicknessAnimationUsingKeyFrames)), "KeyFrames", typeof(ThicknessKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ThicknessAnimationUsingKeyFrames)target).KeyFrames = (ThicknessKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ThicknessAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToggleButton_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToggleButton)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolBar_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolBar)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ToolBar)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolBarOverflowPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolBarOverflowPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ToolBarOverflowPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolBarPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolBarPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ToolBarPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolBarTray_ToolBars()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolBarTray)), "ToolBars", typeof(Collection<ToolBar>), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ToolBarTray)target).ToolBars;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolTip_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolTip)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TreeView_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TreeView)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TreeView)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TreeViewItem_Items()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TreeViewItem)), "Items", typeof(ItemCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((TreeViewItem)target).Items;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Trigger_Setters()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Trigger)), "Setters", typeof(SetterBaseCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Trigger)target).Setters;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Underline_Inlines()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Underline)), "Inlines", typeof(InlineCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Underline)target).Inlines;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UniformGrid_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UniformGrid)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((UniformGrid)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UserControl_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UserControl)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Vector3DAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Vector3DAnimationUsingKeyFrames)), "KeyFrames", typeof(Vector3DKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Vector3DAnimationUsingKeyFrames)target).KeyFrames = (Vector3DKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Vector3DAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_VectorAnimationUsingKeyFrames_KeyFrames()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(VectorAnimationUsingKeyFrames)), "KeyFrames", typeof(VectorKeyFrameCollection), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((VectorAnimationUsingKeyFrames)target).KeyFrames = (VectorKeyFrameCollection)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((VectorAnimationUsingKeyFrames)target).KeyFrames;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Viewbox_Child()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Viewbox)), "Child", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Viewbox)target).Child = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Viewbox)target).Child;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Viewport3DVisual_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Viewport3DVisual)), "Children", typeof(Visual3DCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Viewport3DVisual)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_VirtualizingPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(VirtualizingPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((VirtualizingPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_VirtualizingStackPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(VirtualizingStackPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((VirtualizingStackPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Window_Content()
	{
		DependencyProperty contentProperty = ContentControl.ContentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Window)), "Content", contentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_WrapPanel_Children()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(WrapPanel)), "Children", typeof(UIElementCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((WrapPanel)target).Children;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_XmlDataProvider_XmlSerializer()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(XmlDataProvider)), "XmlSerializer", typeof(IXmlSerializable), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((XmlDataProvider)target).XmlSerializer;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ControlTemplate_Triggers()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ControlTemplate)), "Triggers", typeof(TriggerCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ControlTemplate)target).Triggers;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTemplate_Triggers()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTemplate)), "Triggers", typeof(TriggerCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((DataTemplate)target).Triggers;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTemplate_DataTemplateKey()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTemplate)), "DataTemplateKey", typeof(object), isReadOnly: true, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.GetDelegate = (object target) => ((DataTemplate)target).DataTemplateKey;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ControlTemplate_TargetType()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ControlTemplate)), "TargetType", typeof(Type), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(Type);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ControlTemplate)target).TargetType = (Type)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ControlTemplate)target).TargetType;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Resources()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Resources", typeof(ResourceDictionary), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((FrameworkElement)target).Resources = (ResourceDictionary)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkElement)target).Resources;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkTemplate_Template()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkTemplate)), "Template", typeof(TemplateContent), isReadOnly: false, isAttachable: false);
		wpfKnownMember.DeferringLoaderType = typeof(TemplateContentLoader);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((FrameworkTemplate)target).Template = (TemplateContent)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkTemplate)target).Template;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_ColumnDefinitions()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "ColumnDefinitions", typeof(ColumnDefinitionCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Grid)target).ColumnDefinitions;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Grid_RowDefinitions()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Grid)), "RowDefinitions", typeof(RowDefinitionCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Grid)target).RowDefinitions;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiTrigger_Conditions()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiTrigger)), "Conditions", typeof(ConditionCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MultiTrigger)target).Conditions;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_NameScope_NameScope()
	{
		DependencyProperty nameScopeProperty = NameScope.NameScopeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(NameScope)), "NameScope", nameScopeProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Style_TargetType()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Style)), "TargetType", typeof(Type), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(Type);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Style)target).TargetType = (Type)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Style)target).TargetType;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Style_Triggers()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Style)), "Triggers", typeof(TriggerCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((Style)target).Triggers;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Setter_Value()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Setter)), "Value", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(SetterTriggerConditionValueConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Setter)target).Value = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Setter)target).Value;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Setter_TargetName()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Setter)), "TargetName", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Setter)target).TargetName = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Setter)target).TargetName;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_Path()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "Path", typeof(PropertyPath), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PropertyPathConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).Path = (PropertyPath)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).Path;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ComponentResourceKey_ResourceId()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ComponentResourceKey)), "ResourceId", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ComponentResourceKey)target).ResourceId = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ComponentResourceKey)target).ResourceId;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ComponentResourceKey_TypeInTargetAssembly()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ComponentResourceKey)), "TypeInTargetAssembly", typeof(Type), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(Type);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ComponentResourceKey)target).TypeInTargetAssembly = (Type)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ComponentResourceKey)target).TypeInTargetAssembly;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_Converter()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "Converter", typeof(IValueConverter), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).Converter = (IValueConverter)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).Converter;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_Source()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "Source", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).Source = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).Source;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_RelativeSource()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "RelativeSource", typeof(RelativeSource), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).RelativeSource = (RelativeSource)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).RelativeSource;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_Mode()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "Mode", typeof(BindingMode), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BindingMode);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).Mode = (BindingMode)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).Mode;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Timeline_BeginTime()
	{
		DependencyProperty beginTimeProperty = Timeline.BeginTimeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Timeline)), "BeginTime", beginTimeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TimeSpanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Style_BasedOn()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Style)), "BasedOn", typeof(Style), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Style)target).BasedOn = (Style)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Style)target).BasedOn;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_ElementName()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "ElementName", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).ElementName = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).ElementName;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_UpdateSourceTrigger()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "UpdateSourceTrigger", typeof(UpdateSourceTrigger), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(UpdateSourceTrigger);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).UpdateSourceTrigger = (UpdateSourceTrigger)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).UpdateSourceTrigger;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ResourceDictionary_DeferrableContent()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ResourceDictionary)), "DeferrableContent", typeof(DeferrableContent), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DeferrableContentConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ResourceDictionary)target).DeferrableContent = (DeferrableContent)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ResourceDictionary)target).DeferrableContent;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Trigger_Value()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Trigger)), "Value", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(SetterTriggerConditionValueConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Trigger)target).Value = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Trigger)target).Value;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Trigger_SourceName()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Trigger)), "SourceName", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Trigger)target).SourceName = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Trigger)target).SourceName;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_RelativeSource_AncestorType()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(RelativeSource)), "AncestorType", typeof(Type), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(Type);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((RelativeSource)target).AncestorType = (Type)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((RelativeSource)target).AncestorType;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_Uid()
	{
		DependencyProperty uidProperty = UIElement.UidProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "Uid", uidProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkContentElement_Name()
	{
		DependencyProperty nameProperty = FrameworkContentElement.NameProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkContentElement)), "Name", nameProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkContentElement_Resources()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkContentElement)), "Resources", typeof(ResourceDictionary), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((FrameworkContentElement)target).Resources = (ResourceDictionary)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkContentElement)target).Resources;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Style_Resources()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Style)), "Resources", typeof(ResourceDictionary), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Style)target).Resources = (ResourceDictionary)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Style)target).Resources;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkTemplate_Resources()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkTemplate)), "Resources", typeof(ResourceDictionary), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((FrameworkTemplate)target).Resources = (ResourceDictionary)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkTemplate)target).Resources;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Application_Resources()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Application)), "Resources", typeof(ResourceDictionary), isReadOnly: false, isAttachable: false);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Application)target).Resources = (ResourceDictionary)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Application)target).Resources;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiBinding_Converter()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiBinding)), "Converter", typeof(IMultiValueConverter), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((MultiBinding)target).Converter = (IMultiValueConverter)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((MultiBinding)target).Converter;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiBinding_ConverterParameter()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiBinding)), "ConverterParameter", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((MultiBinding)target).ConverterParameter = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((MultiBinding)target).ConverterParameter;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_LinearGradientBrush_StartPoint()
	{
		DependencyProperty startPointProperty = LinearGradientBrush.StartPointProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(LinearGradientBrush)), "StartPoint", startPointProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PointConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_LinearGradientBrush_EndPoint()
	{
		DependencyProperty endPointProperty = LinearGradientBrush.EndPointProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(LinearGradientBrush)), "EndPoint", endPointProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PointConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_CommandBinding_Command()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(CommandBinding)), "Command", typeof(ICommand), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(CommandConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((CommandBinding)target).Command = (ICommand)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((CommandBinding)target).Command;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Condition_Property()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Condition)), "Property", typeof(DependencyProperty), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DependencyPropertyConverter);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Condition)target).Property = (DependencyProperty)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Condition)target).Property;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Condition_Value()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Condition)), "Value", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(SetterTriggerConditionValueConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Condition)target).Value = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Condition)target).Value;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Condition_Binding()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Condition)), "Binding", typeof(BindingBase), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Condition)target).Binding = (BindingBase)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Condition)target).Binding;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BindingBase_FallbackValue()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BindingBase)), "FallbackValue", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BindingBase)target).FallbackValue = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BindingBase)target).FallbackValue;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Window_ResizeMode()
	{
		DependencyProperty resizeModeProperty = Window.ResizeModeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Window)), "ResizeMode", resizeModeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ResizeMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Window_WindowState()
	{
		DependencyProperty windowStateProperty = Window.WindowStateProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Window)), "WindowState", windowStateProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(WindowState);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Window_Title()
	{
		DependencyProperty titleProperty = Window.TitleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Window)), "Title", titleProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_StrokeLineJoin()
	{
		DependencyProperty strokeLineJoinProperty = Shape.StrokeLineJoinProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "StrokeLineJoin", strokeLineJoinProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PenLineJoin);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_StrokeStartLineCap()
	{
		DependencyProperty strokeStartLineCapProperty = Shape.StrokeStartLineCapProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "StrokeStartLineCap", strokeStartLineCapProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PenLineCap);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_StrokeEndLineCap()
	{
		DependencyProperty strokeEndLineCapProperty = Shape.StrokeEndLineCapProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "StrokeEndLineCap", strokeEndLineCapProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PenLineCap);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TileBrush_TileMode()
	{
		DependencyProperty tileModeProperty = TileBrush.TileModeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TileBrush)), "TileMode", tileModeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TileMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TileBrush_ViewboxUnits()
	{
		DependencyProperty viewboxUnitsProperty = TileBrush.ViewboxUnitsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TileBrush)), "ViewboxUnits", viewboxUnitsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushMappingMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TileBrush_ViewportUnits()
	{
		DependencyProperty viewportUnitsProperty = TileBrush.ViewportUnitsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TileBrush)), "ViewportUnits", viewportUnitsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushMappingMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GeometryDrawing_Pen()
	{
		DependencyProperty penProperty = GeometryDrawing.PenProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GeometryDrawing)), "Pen", penProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBox_TextWrapping()
	{
		DependencyProperty textWrappingProperty = TextBox.TextWrappingProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBox)), "TextWrapping", textWrappingProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextWrapping);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_StackPanel_Orientation()
	{
		DependencyProperty orientationProperty = StackPanel.OrientationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(StackPanel)), "Orientation", orientationProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Orientation);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_Thumb()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "Thumb", typeof(Thumb), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Track)target).Thumb = (Thumb)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Track)target).Thumb;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_IncreaseRepeatButton()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "IncreaseRepeatButton", typeof(RepeatButton), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Track)target).IncreaseRepeatButton = (RepeatButton)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Track)target).IncreaseRepeatButton;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Track_DecreaseRepeatButton()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Track)), "DecreaseRepeatButton", typeof(RepeatButton), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Track)target).DecreaseRepeatButton = (RepeatButton)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Track)target).DecreaseRepeatButton;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_EventTrigger_RoutedEvent()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(EventTrigger)), "RoutedEvent", typeof(RoutedEvent), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(RoutedEventConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((EventTrigger)target).RoutedEvent = (RoutedEvent)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((EventTrigger)target).RoutedEvent;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_InputBinding_Command()
	{
		DependencyProperty commandProperty = InputBinding.CommandProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(InputBinding)), "Command", commandProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(CommandConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_KeyBinding_Gesture()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(KeyBinding)), "Gesture", typeof(InputGesture), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(KeyGestureConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((KeyBinding)target).Gesture = (InputGesture)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((KeyBinding)target).Gesture;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBox_TextAlignment()
	{
		DependencyProperty textAlignmentProperty = TextBox.TextAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBox)), "TextAlignment", textAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBlock_TextAlignment()
	{
		DependencyProperty textAlignmentProperty = TextBlock.TextAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBlock)), "TextAlignment", textAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_JournalEntryUnifiedViewConverter_JournalEntryPosition()
	{
		DependencyProperty journalEntryPositionProperty = JournalEntryUnifiedViewConverter.JournalEntryPositionProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(JournalEntryUnifiedViewConverter)), "JournalEntryPosition", journalEntryPositionProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(JournalEntryPosition);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_GradientBrush_MappingMode()
	{
		DependencyProperty mappingModeProperty = GradientBrush.MappingModeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(GradientBrush)), "MappingMode", mappingModeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BrushMappingMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MenuItem_Role()
	{
		DependencyProperty roleProperty = MenuItem.RoleProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MenuItem)), "Role", roleProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(MenuItemRole);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTrigger_Value()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTrigger)), "Value", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DataTrigger)target).Value = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DataTrigger)target).Value;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTrigger_Binding()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTrigger)), "Binding", typeof(BindingBase), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DataTrigger)target).Binding = (BindingBase)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DataTrigger)target).Binding;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Setter_Property()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Setter)), "Property", typeof(DependencyProperty), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DependencyPropertyConverter);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Setter)target).Property = (DependencyProperty)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Setter)target).Property;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ResourceDictionary_Source()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ResourceDictionary)), "Source", typeof(Uri), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(UriTypeConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ResourceDictionary)target).Source = (Uri)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ResourceDictionary)target).Source;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BeginStoryboard_Name()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BeginStoryboard)), "Name", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BeginStoryboard)target).Name = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BeginStoryboard)target).Name;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ResourceDictionary_MergedDictionaries()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ResourceDictionary)), "MergedDictionaries", typeof(Collection<ResourceDictionary>), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((ResourceDictionary)target).MergedDictionaries;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_KeyboardNavigation_DirectionalNavigation()
	{
		DependencyProperty directionalNavigationProperty = KeyboardNavigation.DirectionalNavigationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(KeyboardNavigation)), "DirectionalNavigation", directionalNavigationProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(KeyboardNavigationMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_KeyboardNavigation_TabNavigation()
	{
		DependencyProperty tabNavigationProperty = KeyboardNavigation.TabNavigationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(KeyboardNavigation)), "TabNavigation", tabNavigationProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(KeyboardNavigationMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ScrollBar_Orientation()
	{
		DependencyProperty orientationProperty = ScrollBar.OrientationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ScrollBar)), "Orientation", orientationProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Orientation);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Trigger_Property()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Trigger)), "Property", typeof(DependencyProperty), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DependencyPropertyConverter);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Trigger)target).Property = (DependencyProperty)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Trigger)target).Property;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_EventTrigger_SourceName()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(EventTrigger)), "SourceName", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((EventTrigger)target).SourceName = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((EventTrigger)target).SourceName;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DefinitionBase_SharedSizeGroup()
	{
		DependencyProperty sharedSizeGroupProperty = DefinitionBase.SharedSizeGroupProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DefinitionBase)), "SharedSizeGroup", sharedSizeGroupProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolTipService_ToolTip()
	{
		DependencyProperty toolTipProperty = ToolTipService.ToolTipProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolTipService)), "ToolTip", toolTipProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PathFigure_IsClosed()
	{
		DependencyProperty isClosedProperty = PathFigure.IsClosedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PathFigure)), "IsClosed", isClosedProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PathFigure_IsFilled()
	{
		DependencyProperty isFilledProperty = PathFigure.IsFilledProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PathFigure)), "IsFilled", isFilledProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ButtonBase_ClickMode()
	{
		DependencyProperty clickModeProperty = ButtonBase.ClickModeProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ButtonBase)), "ClickMode", clickModeProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ClickMode);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Block_TextAlignment()
	{
		DependencyProperty textAlignmentProperty = Block.TextAlignmentProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Block)), "TextAlignment", textAlignmentProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(TextAlignment);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_RenderTransformOrigin()
	{
		DependencyProperty renderTransformOriginProperty = UIElement.RenderTransformOriginProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "RenderTransformOrigin", renderTransformOriginProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PointConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Pen_LineJoin()
	{
		DependencyProperty lineJoinProperty = Pen.LineJoinProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Pen)), "LineJoin", lineJoinProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(PenLineJoin);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_BulletDecorator_Bullet()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(BulletDecorator)), "Bullet", typeof(UIElement), isReadOnly: false, isAttachable: false);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((BulletDecorator)target).Bullet = (UIElement)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((BulletDecorator)target).Bullet;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_SnapsToDevicePixels()
	{
		DependencyProperty snapsToDevicePixelsProperty = UIElement.SnapsToDevicePixelsProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "SnapsToDevicePixels", snapsToDevicePixelsProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_CommandBindings()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "CommandBindings", typeof(CommandBindingCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((UIElement)target).CommandBindings;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_InputBindings()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "InputBindings", typeof(InputBindingCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((UIElement)target).InputBindings;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_SolidColorBrush_Color()
	{
		DependencyProperty colorProperty = SolidColorBrush.ColorProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(SolidColorBrush)), "Color", colorProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ColorConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Brush_Opacity()
	{
		DependencyProperty opacityProperty = Brush.OpacityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Brush)), "Opacity", opacityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBoxBase_AcceptsTab()
	{
		DependencyProperty acceptsTabProperty = TextBoxBase.AcceptsTabProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBoxBase)), "AcceptsTab", acceptsTabProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_PathSegment_IsStroked()
	{
		DependencyProperty isStrokedProperty = PathSegment.IsStrokedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(PathSegment)), "IsStroked", isStrokedProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_VirtualizingPanel_IsVirtualizing()
	{
		DependencyProperty isVirtualizingProperty = VirtualizingPanel.IsVirtualizingProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(VirtualizingPanel)), "IsVirtualizing", isVirtualizingProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_Stretch()
	{
		DependencyProperty stretchProperty = Shape.StretchProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "Stretch", stretchProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Stretch);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Frame_JournalOwnership()
	{
		DependencyProperty journalOwnershipProperty = Frame.JournalOwnershipProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Frame)), "JournalOwnership", journalOwnershipProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(JournalOwnership);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Frame_NavigationUIVisibility()
	{
		DependencyProperty navigationUIVisibilityProperty = Frame.NavigationUIVisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Frame)), "NavigationUIVisibility", navigationUIVisibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(NavigationUIVisibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Storyboard_TargetName()
	{
		DependencyProperty targetNameProperty = Storyboard.TargetNameProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Storyboard)), "TargetName", targetNameProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_XmlDataProvider_XPath()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(XmlDataProvider)), "XPath", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((XmlDataProvider)target).XPath = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((XmlDataProvider)target).XPath;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Selector_IsSelected()
	{
		DependencyProperty isSelectedProperty = Selector.IsSelectedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Selector)), "IsSelected", isSelectedProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_DataTemplate_DataType()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(DataTemplate)), "DataType", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.Ambient = true;
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((DataTemplate)target).DataType = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((DataTemplate)target).DataType;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Shape_StrokeMiterLimit()
	{
		DependencyProperty strokeMiterLimitProperty = Shape.StrokeMiterLimitProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Shape)), "StrokeMiterLimit", strokeMiterLimitProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(DoubleConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_UIElement_AllowDrop()
	{
		DependencyProperty allowDropProperty = UIElement.AllowDropProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(UIElement)), "AllowDrop", allowDropProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MenuItem_IsChecked()
	{
		DependencyProperty isCheckedProperty = MenuItem.IsCheckedProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MenuItem)), "IsChecked", isCheckedProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Panel_IsItemsHost()
	{
		DependencyProperty isItemsHostProperty = Panel.IsItemsHostProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Panel)), "IsItemsHost", isItemsHostProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_XPath()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "XPath", typeof(string), isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(StringConverter);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).XPath = (string)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).XPath;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Window_AllowsTransparency()
	{
		DependencyProperty allowsTransparencyProperty = Window.AllowsTransparencyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Window)), "AllowsTransparency", allowsTransparencyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(BooleanConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ObjectDataProvider_ObjectType()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ObjectDataProvider)), "ObjectType", typeof(Type), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(Type);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((ObjectDataProvider)target).ObjectType = (Type)value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((ObjectDataProvider)target).ObjectType;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_ToolBar_Orientation()
	{
		DependencyProperty orientationProperty = ToolBar.OrientationProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(ToolBar)), "Orientation", orientationProperty, isReadOnly: true, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(Orientation);
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBoxBase_VerticalScrollBarVisibility()
	{
		DependencyProperty verticalScrollBarVisibilityProperty = TextBoxBase.VerticalScrollBarVisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBoxBase)), "VerticalScrollBarVisibility", verticalScrollBarVisibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ScrollBarVisibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_TextBoxBase_HorizontalScrollBarVisibility()
	{
		DependencyProperty horizontalScrollBarVisibilityProperty = TextBoxBase.HorizontalScrollBarVisibilityProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(TextBoxBase)), "HorizontalScrollBarVisibility", horizontalScrollBarVisibilityProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(ScrollBarVisibility);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_FrameworkElement_Triggers()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(FrameworkElement)), "Triggers", typeof(TriggerCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((FrameworkElement)target).Triggers;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_MultiDataTrigger_Conditions()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(MultiDataTrigger)), "Conditions", typeof(ConditionCollection), isReadOnly: true, isAttachable: false);
		wpfKnownMember.GetDelegate = (object target) => ((MultiDataTrigger)target).Conditions;
		wpfKnownMember.IsWritePrivate = true;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_KeyBinding_Key()
	{
		DependencyProperty keyProperty = KeyBinding.KeyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(KeyBinding)), "Key", keyProperty, isReadOnly: false, isAttachable: false);
		wpfKnownMember.TypeConverterType = typeof(KeyConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Binding_ConverterParameter()
	{
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Binding)), "ConverterParameter", typeof(object), isReadOnly: false, isAttachable: false);
		wpfKnownMember.HasSpecialTypeConverter = true;
		wpfKnownMember.TypeConverterType = typeof(object);
		wpfKnownMember.SetDelegate = delegate(object target, object value)
		{
			((Binding)target).ConverterParameter = value;
		};
		wpfKnownMember.GetDelegate = (object target) => ((Binding)target).ConverterParameter;
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Canvas_Top()
	{
		DependencyProperty topProperty = Canvas.TopProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Canvas)), "Top", topProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Canvas_Left()
	{
		DependencyProperty leftProperty = Canvas.LeftProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Canvas)), "Left", leftProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Canvas_Bottom()
	{
		DependencyProperty bottomProperty = Canvas.BottomProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Canvas)), "Bottom", bottomProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Canvas_Right()
	{
		DependencyProperty rightProperty = Canvas.RightProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Canvas)), "Right", rightProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(LengthConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownMember Create_BamlProperty_Storyboard_TargetProperty()
	{
		DependencyProperty targetPropertyProperty = Storyboard.TargetPropertyProperty;
		WpfKnownMember wpfKnownMember = new WpfKnownMember(this, GetXamlType(typeof(Storyboard)), "TargetProperty", targetPropertyProperty, isReadOnly: false, isAttachable: true);
		wpfKnownMember.TypeConverterType = typeof(PropertyPathConverter);
		wpfKnownMember.Freeze();
		return wpfKnownMember;
	}

	private WpfKnownType CreateKnownBamlType(short bamlNumber, bool isBamlType, bool useV3Rules)
	{
		return bamlNumber switch
		{
			1 => Create_BamlType_AccessText(isBamlType, useV3Rules), 
			2 => Create_BamlType_AdornedElementPlaceholder(isBamlType, useV3Rules), 
			3 => Create_BamlType_Adorner(isBamlType, useV3Rules), 
			4 => Create_BamlType_AdornerDecorator(isBamlType, useV3Rules), 
			5 => Create_BamlType_AdornerLayer(isBamlType, useV3Rules), 
			6 => Create_BamlType_AffineTransform3D(isBamlType, useV3Rules), 
			7 => Create_BamlType_AmbientLight(isBamlType, useV3Rules), 
			8 => Create_BamlType_AnchoredBlock(isBamlType, useV3Rules), 
			9 => Create_BamlType_Animatable(isBamlType, useV3Rules), 
			10 => Create_BamlType_AnimationClock(isBamlType, useV3Rules), 
			11 => Create_BamlType_AnimationTimeline(isBamlType, useV3Rules), 
			12 => Create_BamlType_Application(isBamlType, useV3Rules), 
			13 => Create_BamlType_ArcSegment(isBamlType, useV3Rules), 
			14 => Create_BamlType_ArrayExtension(isBamlType, useV3Rules), 
			15 => Create_BamlType_AxisAngleRotation3D(isBamlType, useV3Rules), 
			16 => Create_BamlType_BaseIListConverter(isBamlType, useV3Rules), 
			17 => Create_BamlType_BeginStoryboard(isBamlType, useV3Rules), 
			18 => Create_BamlType_BevelBitmapEffect(isBamlType, useV3Rules), 
			19 => Create_BamlType_BezierSegment(isBamlType, useV3Rules), 
			20 => Create_BamlType_Binding(isBamlType, useV3Rules), 
			21 => Create_BamlType_BindingBase(isBamlType, useV3Rules), 
			22 => Create_BamlType_BindingExpression(isBamlType, useV3Rules), 
			23 => Create_BamlType_BindingExpressionBase(isBamlType, useV3Rules), 
			24 => Create_BamlType_BindingListCollectionView(isBamlType, useV3Rules), 
			25 => Create_BamlType_BitmapDecoder(isBamlType, useV3Rules), 
			26 => Create_BamlType_BitmapEffect(isBamlType, useV3Rules), 
			27 => Create_BamlType_BitmapEffectCollection(isBamlType, useV3Rules), 
			28 => Create_BamlType_BitmapEffectGroup(isBamlType, useV3Rules), 
			29 => Create_BamlType_BitmapEffectInput(isBamlType, useV3Rules), 
			30 => Create_BamlType_BitmapEncoder(isBamlType, useV3Rules), 
			31 => Create_BamlType_BitmapFrame(isBamlType, useV3Rules), 
			32 => Create_BamlType_BitmapImage(isBamlType, useV3Rules), 
			33 => Create_BamlType_BitmapMetadata(isBamlType, useV3Rules), 
			34 => Create_BamlType_BitmapPalette(isBamlType, useV3Rules), 
			35 => Create_BamlType_BitmapSource(isBamlType, useV3Rules), 
			36 => Create_BamlType_Block(isBamlType, useV3Rules), 
			37 => Create_BamlType_BlockUIContainer(isBamlType, useV3Rules), 
			38 => Create_BamlType_BlurBitmapEffect(isBamlType, useV3Rules), 
			39 => Create_BamlType_BmpBitmapDecoder(isBamlType, useV3Rules), 
			40 => Create_BamlType_BmpBitmapEncoder(isBamlType, useV3Rules), 
			41 => Create_BamlType_Bold(isBamlType, useV3Rules), 
			42 => Create_BamlType_BoolIListConverter(isBamlType, useV3Rules), 
			43 => Create_BamlType_Boolean(isBamlType, useV3Rules), 
			44 => Create_BamlType_BooleanAnimationBase(isBamlType, useV3Rules), 
			45 => Create_BamlType_BooleanAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			46 => Create_BamlType_BooleanConverter(isBamlType, useV3Rules), 
			47 => Create_BamlType_BooleanKeyFrame(isBamlType, useV3Rules), 
			48 => Create_BamlType_BooleanKeyFrameCollection(isBamlType, useV3Rules), 
			49 => Create_BamlType_BooleanToVisibilityConverter(isBamlType, useV3Rules), 
			50 => Create_BamlType_Border(isBamlType, useV3Rules), 
			51 => Create_BamlType_BorderGapMaskConverter(isBamlType, useV3Rules), 
			52 => Create_BamlType_Brush(isBamlType, useV3Rules), 
			53 => Create_BamlType_BrushConverter(isBamlType, useV3Rules), 
			54 => Create_BamlType_BulletDecorator(isBamlType, useV3Rules), 
			55 => Create_BamlType_Button(isBamlType, useV3Rules), 
			56 => Create_BamlType_ButtonBase(isBamlType, useV3Rules), 
			57 => Create_BamlType_Byte(isBamlType, useV3Rules), 
			58 => Create_BamlType_ByteAnimation(isBamlType, useV3Rules), 
			59 => Create_BamlType_ByteAnimationBase(isBamlType, useV3Rules), 
			60 => Create_BamlType_ByteAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			61 => Create_BamlType_ByteConverter(isBamlType, useV3Rules), 
			62 => Create_BamlType_ByteKeyFrame(isBamlType, useV3Rules), 
			63 => Create_BamlType_ByteKeyFrameCollection(isBamlType, useV3Rules), 
			64 => Create_BamlType_CachedBitmap(isBamlType, useV3Rules), 
			65 => Create_BamlType_Camera(isBamlType, useV3Rules), 
			66 => Create_BamlType_Canvas(isBamlType, useV3Rules), 
			67 => Create_BamlType_Char(isBamlType, useV3Rules), 
			68 => Create_BamlType_CharAnimationBase(isBamlType, useV3Rules), 
			69 => Create_BamlType_CharAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			70 => Create_BamlType_CharConverter(isBamlType, useV3Rules), 
			71 => Create_BamlType_CharIListConverter(isBamlType, useV3Rules), 
			72 => Create_BamlType_CharKeyFrame(isBamlType, useV3Rules), 
			73 => Create_BamlType_CharKeyFrameCollection(isBamlType, useV3Rules), 
			74 => Create_BamlType_CheckBox(isBamlType, useV3Rules), 
			75 => Create_BamlType_Clock(isBamlType, useV3Rules), 
			76 => Create_BamlType_ClockController(isBamlType, useV3Rules), 
			77 => Create_BamlType_ClockGroup(isBamlType, useV3Rules), 
			78 => Create_BamlType_CollectionContainer(isBamlType, useV3Rules), 
			79 => Create_BamlType_CollectionView(isBamlType, useV3Rules), 
			80 => Create_BamlType_CollectionViewSource(isBamlType, useV3Rules), 
			81 => Create_BamlType_Color(isBamlType, useV3Rules), 
			82 => Create_BamlType_ColorAnimation(isBamlType, useV3Rules), 
			83 => Create_BamlType_ColorAnimationBase(isBamlType, useV3Rules), 
			84 => Create_BamlType_ColorAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			85 => Create_BamlType_ColorConvertedBitmap(isBamlType, useV3Rules), 
			86 => Create_BamlType_ColorConvertedBitmapExtension(isBamlType, useV3Rules), 
			87 => Create_BamlType_ColorConverter(isBamlType, useV3Rules), 
			88 => Create_BamlType_ColorKeyFrame(isBamlType, useV3Rules), 
			89 => Create_BamlType_ColorKeyFrameCollection(isBamlType, useV3Rules), 
			90 => Create_BamlType_ColumnDefinition(isBamlType, useV3Rules), 
			91 => Create_BamlType_CombinedGeometry(isBamlType, useV3Rules), 
			92 => Create_BamlType_ComboBox(isBamlType, useV3Rules), 
			93 => Create_BamlType_ComboBoxItem(isBamlType, useV3Rules), 
			94 => Create_BamlType_CommandConverter(isBamlType, useV3Rules), 
			95 => Create_BamlType_ComponentResourceKey(isBamlType, useV3Rules), 
			96 => Create_BamlType_ComponentResourceKeyConverter(isBamlType, useV3Rules), 
			97 => Create_BamlType_CompositionTarget(isBamlType, useV3Rules), 
			98 => Create_BamlType_Condition(isBamlType, useV3Rules), 
			99 => Create_BamlType_ContainerVisual(isBamlType, useV3Rules), 
			100 => Create_BamlType_ContentControl(isBamlType, useV3Rules), 
			101 => Create_BamlType_ContentElement(isBamlType, useV3Rules), 
			102 => Create_BamlType_ContentPresenter(isBamlType, useV3Rules), 
			103 => Create_BamlType_ContentPropertyAttribute(isBamlType, useV3Rules), 
			104 => Create_BamlType_ContentWrapperAttribute(isBamlType, useV3Rules), 
			105 => Create_BamlType_ContextMenu(isBamlType, useV3Rules), 
			106 => Create_BamlType_ContextMenuService(isBamlType, useV3Rules), 
			107 => Create_BamlType_Control(isBamlType, useV3Rules), 
			108 => Create_BamlType_ControlTemplate(isBamlType, useV3Rules), 
			109 => Create_BamlType_ControllableStoryboardAction(isBamlType, useV3Rules), 
			110 => Create_BamlType_CornerRadius(isBamlType, useV3Rules), 
			111 => Create_BamlType_CornerRadiusConverter(isBamlType, useV3Rules), 
			112 => Create_BamlType_CroppedBitmap(isBamlType, useV3Rules), 
			113 => Create_BamlType_CultureInfo(isBamlType, useV3Rules), 
			114 => Create_BamlType_CultureInfoConverter(isBamlType, useV3Rules), 
			115 => Create_BamlType_CultureInfoIetfLanguageTagConverter(isBamlType, useV3Rules), 
			116 => Create_BamlType_Cursor(isBamlType, useV3Rules), 
			117 => Create_BamlType_CursorConverter(isBamlType, useV3Rules), 
			118 => Create_BamlType_DashStyle(isBamlType, useV3Rules), 
			119 => Create_BamlType_DataChangedEventManager(isBamlType, useV3Rules), 
			120 => Create_BamlType_DataTemplate(isBamlType, useV3Rules), 
			121 => Create_BamlType_DataTemplateKey(isBamlType, useV3Rules), 
			122 => Create_BamlType_DataTrigger(isBamlType, useV3Rules), 
			123 => Create_BamlType_DateTime(isBamlType, useV3Rules), 
			124 => Create_BamlType_DateTimeConverter(isBamlType, useV3Rules), 
			125 => Create_BamlType_DateTimeConverter2(isBamlType, useV3Rules), 
			126 => Create_BamlType_Decimal(isBamlType, useV3Rules), 
			127 => Create_BamlType_DecimalAnimation(isBamlType, useV3Rules), 
			128 => Create_BamlType_DecimalAnimationBase(isBamlType, useV3Rules), 
			129 => Create_BamlType_DecimalAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			130 => Create_BamlType_DecimalConverter(isBamlType, useV3Rules), 
			131 => Create_BamlType_DecimalKeyFrame(isBamlType, useV3Rules), 
			132 => Create_BamlType_DecimalKeyFrameCollection(isBamlType, useV3Rules), 
			133 => Create_BamlType_Decorator(isBamlType, useV3Rules), 
			134 => Create_BamlType_DefinitionBase(isBamlType, useV3Rules), 
			135 => Create_BamlType_DependencyObject(isBamlType, useV3Rules), 
			136 => Create_BamlType_DependencyProperty(isBamlType, useV3Rules), 
			137 => Create_BamlType_DependencyPropertyConverter(isBamlType, useV3Rules), 
			138 => Create_BamlType_DialogResultConverter(isBamlType, useV3Rules), 
			139 => Create_BamlType_DiffuseMaterial(isBamlType, useV3Rules), 
			140 => Create_BamlType_DirectionalLight(isBamlType, useV3Rules), 
			141 => Create_BamlType_DiscreteBooleanKeyFrame(isBamlType, useV3Rules), 
			142 => Create_BamlType_DiscreteByteKeyFrame(isBamlType, useV3Rules), 
			143 => Create_BamlType_DiscreteCharKeyFrame(isBamlType, useV3Rules), 
			144 => Create_BamlType_DiscreteColorKeyFrame(isBamlType, useV3Rules), 
			145 => Create_BamlType_DiscreteDecimalKeyFrame(isBamlType, useV3Rules), 
			146 => Create_BamlType_DiscreteDoubleKeyFrame(isBamlType, useV3Rules), 
			147 => Create_BamlType_DiscreteInt16KeyFrame(isBamlType, useV3Rules), 
			148 => Create_BamlType_DiscreteInt32KeyFrame(isBamlType, useV3Rules), 
			149 => Create_BamlType_DiscreteInt64KeyFrame(isBamlType, useV3Rules), 
			150 => Create_BamlType_DiscreteMatrixKeyFrame(isBamlType, useV3Rules), 
			151 => Create_BamlType_DiscreteObjectKeyFrame(isBamlType, useV3Rules), 
			152 => Create_BamlType_DiscretePoint3DKeyFrame(isBamlType, useV3Rules), 
			153 => Create_BamlType_DiscretePointKeyFrame(isBamlType, useV3Rules), 
			154 => Create_BamlType_DiscreteQuaternionKeyFrame(isBamlType, useV3Rules), 
			155 => Create_BamlType_DiscreteRectKeyFrame(isBamlType, useV3Rules), 
			156 => Create_BamlType_DiscreteRotation3DKeyFrame(isBamlType, useV3Rules), 
			157 => Create_BamlType_DiscreteSingleKeyFrame(isBamlType, useV3Rules), 
			158 => Create_BamlType_DiscreteSizeKeyFrame(isBamlType, useV3Rules), 
			159 => Create_BamlType_DiscreteStringKeyFrame(isBamlType, useV3Rules), 
			160 => Create_BamlType_DiscreteThicknessKeyFrame(isBamlType, useV3Rules), 
			161 => Create_BamlType_DiscreteVector3DKeyFrame(isBamlType, useV3Rules), 
			162 => Create_BamlType_DiscreteVectorKeyFrame(isBamlType, useV3Rules), 
			163 => Create_BamlType_DockPanel(isBamlType, useV3Rules), 
			164 => Create_BamlType_DocumentPageView(isBamlType, useV3Rules), 
			165 => Create_BamlType_DocumentReference(isBamlType, useV3Rules), 
			166 => Create_BamlType_DocumentViewer(isBamlType, useV3Rules), 
			167 => Create_BamlType_DocumentViewerBase(isBamlType, useV3Rules), 
			168 => Create_BamlType_Double(isBamlType, useV3Rules), 
			169 => Create_BamlType_DoubleAnimation(isBamlType, useV3Rules), 
			170 => Create_BamlType_DoubleAnimationBase(isBamlType, useV3Rules), 
			171 => Create_BamlType_DoubleAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			172 => Create_BamlType_DoubleAnimationUsingPath(isBamlType, useV3Rules), 
			173 => Create_BamlType_DoubleCollection(isBamlType, useV3Rules), 
			174 => Create_BamlType_DoubleCollectionConverter(isBamlType, useV3Rules), 
			175 => Create_BamlType_DoubleConverter(isBamlType, useV3Rules), 
			176 => Create_BamlType_DoubleIListConverter(isBamlType, useV3Rules), 
			177 => Create_BamlType_DoubleKeyFrame(isBamlType, useV3Rules), 
			178 => Create_BamlType_DoubleKeyFrameCollection(isBamlType, useV3Rules), 
			179 => Create_BamlType_Drawing(isBamlType, useV3Rules), 
			180 => Create_BamlType_DrawingBrush(isBamlType, useV3Rules), 
			181 => Create_BamlType_DrawingCollection(isBamlType, useV3Rules), 
			182 => Create_BamlType_DrawingContext(isBamlType, useV3Rules), 
			183 => Create_BamlType_DrawingGroup(isBamlType, useV3Rules), 
			184 => Create_BamlType_DrawingImage(isBamlType, useV3Rules), 
			185 => Create_BamlType_DrawingVisual(isBamlType, useV3Rules), 
			186 => Create_BamlType_DropShadowBitmapEffect(isBamlType, useV3Rules), 
			187 => Create_BamlType_Duration(isBamlType, useV3Rules), 
			188 => Create_BamlType_DurationConverter(isBamlType, useV3Rules), 
			189 => Create_BamlType_DynamicResourceExtension(isBamlType, useV3Rules), 
			190 => Create_BamlType_DynamicResourceExtensionConverter(isBamlType, useV3Rules), 
			191 => Create_BamlType_Ellipse(isBamlType, useV3Rules), 
			192 => Create_BamlType_EllipseGeometry(isBamlType, useV3Rules), 
			193 => Create_BamlType_EmbossBitmapEffect(isBamlType, useV3Rules), 
			194 => Create_BamlType_EmissiveMaterial(isBamlType, useV3Rules), 
			195 => Create_BamlType_EnumConverter(isBamlType, useV3Rules), 
			196 => Create_BamlType_EventManager(isBamlType, useV3Rules), 
			197 => Create_BamlType_EventSetter(isBamlType, useV3Rules), 
			198 => Create_BamlType_EventTrigger(isBamlType, useV3Rules), 
			199 => Create_BamlType_Expander(isBamlType, useV3Rules), 
			200 => Create_BamlType_Expression(isBamlType, useV3Rules), 
			201 => Create_BamlType_ExpressionConverter(isBamlType, useV3Rules), 
			202 => Create_BamlType_Figure(isBamlType, useV3Rules), 
			203 => Create_BamlType_FigureLength(isBamlType, useV3Rules), 
			204 => Create_BamlType_FigureLengthConverter(isBamlType, useV3Rules), 
			205 => Create_BamlType_FixedDocument(isBamlType, useV3Rules), 
			206 => Create_BamlType_FixedDocumentSequence(isBamlType, useV3Rules), 
			207 => Create_BamlType_FixedPage(isBamlType, useV3Rules), 
			208 => Create_BamlType_Floater(isBamlType, useV3Rules), 
			209 => Create_BamlType_FlowDocument(isBamlType, useV3Rules), 
			210 => Create_BamlType_FlowDocumentPageViewer(isBamlType, useV3Rules), 
			211 => Create_BamlType_FlowDocumentReader(isBamlType, useV3Rules), 
			212 => Create_BamlType_FlowDocumentScrollViewer(isBamlType, useV3Rules), 
			213 => Create_BamlType_FocusManager(isBamlType, useV3Rules), 
			214 => Create_BamlType_FontFamily(isBamlType, useV3Rules), 
			215 => Create_BamlType_FontFamilyConverter(isBamlType, useV3Rules), 
			216 => Create_BamlType_FontSizeConverter(isBamlType, useV3Rules), 
			217 => Create_BamlType_FontStretch(isBamlType, useV3Rules), 
			218 => Create_BamlType_FontStretchConverter(isBamlType, useV3Rules), 
			219 => Create_BamlType_FontStyle(isBamlType, useV3Rules), 
			220 => Create_BamlType_FontStyleConverter(isBamlType, useV3Rules), 
			221 => Create_BamlType_FontWeight(isBamlType, useV3Rules), 
			222 => Create_BamlType_FontWeightConverter(isBamlType, useV3Rules), 
			223 => Create_BamlType_FormatConvertedBitmap(isBamlType, useV3Rules), 
			224 => Create_BamlType_Frame(isBamlType, useV3Rules), 
			225 => Create_BamlType_FrameworkContentElement(isBamlType, useV3Rules), 
			226 => Create_BamlType_FrameworkElement(isBamlType, useV3Rules), 
			227 => Create_BamlType_FrameworkElementFactory(isBamlType, useV3Rules), 
			228 => Create_BamlType_FrameworkPropertyMetadata(isBamlType, useV3Rules), 
			229 => Create_BamlType_FrameworkPropertyMetadataOptions(isBamlType, useV3Rules), 
			230 => Create_BamlType_FrameworkRichTextComposition(isBamlType, useV3Rules), 
			231 => Create_BamlType_FrameworkTemplate(isBamlType, useV3Rules), 
			232 => Create_BamlType_FrameworkTextComposition(isBamlType, useV3Rules), 
			233 => Create_BamlType_Freezable(isBamlType, useV3Rules), 
			234 => Create_BamlType_GeneralTransform(isBamlType, useV3Rules), 
			235 => Create_BamlType_GeneralTransformCollection(isBamlType, useV3Rules), 
			236 => Create_BamlType_GeneralTransformGroup(isBamlType, useV3Rules), 
			237 => Create_BamlType_Geometry(isBamlType, useV3Rules), 
			238 => Create_BamlType_Geometry3D(isBamlType, useV3Rules), 
			239 => Create_BamlType_GeometryCollection(isBamlType, useV3Rules), 
			240 => Create_BamlType_GeometryConverter(isBamlType, useV3Rules), 
			241 => Create_BamlType_GeometryDrawing(isBamlType, useV3Rules), 
			242 => Create_BamlType_GeometryGroup(isBamlType, useV3Rules), 
			243 => Create_BamlType_GeometryModel3D(isBamlType, useV3Rules), 
			244 => Create_BamlType_GestureRecognizer(isBamlType, useV3Rules), 
			245 => Create_BamlType_GifBitmapDecoder(isBamlType, useV3Rules), 
			246 => Create_BamlType_GifBitmapEncoder(isBamlType, useV3Rules), 
			247 => Create_BamlType_GlyphRun(isBamlType, useV3Rules), 
			248 => Create_BamlType_GlyphRunDrawing(isBamlType, useV3Rules), 
			249 => Create_BamlType_GlyphTypeface(isBamlType, useV3Rules), 
			250 => Create_BamlType_Glyphs(isBamlType, useV3Rules), 
			251 => Create_BamlType_GradientBrush(isBamlType, useV3Rules), 
			252 => Create_BamlType_GradientStop(isBamlType, useV3Rules), 
			253 => Create_BamlType_GradientStopCollection(isBamlType, useV3Rules), 
			254 => Create_BamlType_Grid(isBamlType, useV3Rules), 
			255 => Create_BamlType_GridLength(isBamlType, useV3Rules), 
			256 => Create_BamlType_GridLengthConverter(isBamlType, useV3Rules), 
			257 => Create_BamlType_GridSplitter(isBamlType, useV3Rules), 
			258 => Create_BamlType_GridView(isBamlType, useV3Rules), 
			259 => Create_BamlType_GridViewColumn(isBamlType, useV3Rules), 
			260 => Create_BamlType_GridViewColumnHeader(isBamlType, useV3Rules), 
			261 => Create_BamlType_GridViewHeaderRowPresenter(isBamlType, useV3Rules), 
			262 => Create_BamlType_GridViewRowPresenter(isBamlType, useV3Rules), 
			263 => Create_BamlType_GridViewRowPresenterBase(isBamlType, useV3Rules), 
			264 => Create_BamlType_GroupBox(isBamlType, useV3Rules), 
			265 => Create_BamlType_GroupItem(isBamlType, useV3Rules), 
			266 => Create_BamlType_Guid(isBamlType, useV3Rules), 
			267 => Create_BamlType_GuidConverter(isBamlType, useV3Rules), 
			268 => Create_BamlType_GuidelineSet(isBamlType, useV3Rules), 
			269 => Create_BamlType_HeaderedContentControl(isBamlType, useV3Rules), 
			270 => Create_BamlType_HeaderedItemsControl(isBamlType, useV3Rules), 
			271 => Create_BamlType_HierarchicalDataTemplate(isBamlType, useV3Rules), 
			272 => Create_BamlType_HostVisual(isBamlType, useV3Rules), 
			273 => Create_BamlType_Hyperlink(isBamlType, useV3Rules), 
			274 => Create_BamlType_IAddChild(isBamlType, useV3Rules), 
			275 => Create_BamlType_IAddChildInternal(isBamlType, useV3Rules), 
			276 => Create_BamlType_ICommand(isBamlType, useV3Rules), 
			277 => Create_BamlType_IComponentConnector(isBamlType, useV3Rules), 
			278 => Create_BamlType_INameScope(isBamlType, useV3Rules), 
			279 => Create_BamlType_IStyleConnector(isBamlType, useV3Rules), 
			280 => Create_BamlType_IconBitmapDecoder(isBamlType, useV3Rules), 
			281 => Create_BamlType_Image(isBamlType, useV3Rules), 
			282 => Create_BamlType_ImageBrush(isBamlType, useV3Rules), 
			283 => Create_BamlType_ImageDrawing(isBamlType, useV3Rules), 
			284 => Create_BamlType_ImageMetadata(isBamlType, useV3Rules), 
			285 => Create_BamlType_ImageSource(isBamlType, useV3Rules), 
			286 => Create_BamlType_ImageSourceConverter(isBamlType, useV3Rules), 
			287 => Create_BamlType_InPlaceBitmapMetadataWriter(isBamlType, useV3Rules), 
			288 => Create_BamlType_InkCanvas(isBamlType, useV3Rules), 
			289 => Create_BamlType_InkPresenter(isBamlType, useV3Rules), 
			290 => Create_BamlType_Inline(isBamlType, useV3Rules), 
			291 => Create_BamlType_InlineCollection(isBamlType, useV3Rules), 
			292 => Create_BamlType_InlineUIContainer(isBamlType, useV3Rules), 
			293 => Create_BamlType_InputBinding(isBamlType, useV3Rules), 
			294 => Create_BamlType_InputDevice(isBamlType, useV3Rules), 
			295 => Create_BamlType_InputLanguageManager(isBamlType, useV3Rules), 
			296 => Create_BamlType_InputManager(isBamlType, useV3Rules), 
			297 => Create_BamlType_InputMethod(isBamlType, useV3Rules), 
			298 => Create_BamlType_InputScope(isBamlType, useV3Rules), 
			299 => Create_BamlType_InputScopeConverter(isBamlType, useV3Rules), 
			300 => Create_BamlType_InputScopeName(isBamlType, useV3Rules), 
			301 => Create_BamlType_InputScopeNameConverter(isBamlType, useV3Rules), 
			302 => Create_BamlType_Int16(isBamlType, useV3Rules), 
			303 => Create_BamlType_Int16Animation(isBamlType, useV3Rules), 
			304 => Create_BamlType_Int16AnimationBase(isBamlType, useV3Rules), 
			305 => Create_BamlType_Int16AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			306 => Create_BamlType_Int16Converter(isBamlType, useV3Rules), 
			307 => Create_BamlType_Int16KeyFrame(isBamlType, useV3Rules), 
			308 => Create_BamlType_Int16KeyFrameCollection(isBamlType, useV3Rules), 
			309 => Create_BamlType_Int32(isBamlType, useV3Rules), 
			310 => Create_BamlType_Int32Animation(isBamlType, useV3Rules), 
			311 => Create_BamlType_Int32AnimationBase(isBamlType, useV3Rules), 
			312 => Create_BamlType_Int32AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			313 => Create_BamlType_Int32Collection(isBamlType, useV3Rules), 
			314 => Create_BamlType_Int32CollectionConverter(isBamlType, useV3Rules), 
			315 => Create_BamlType_Int32Converter(isBamlType, useV3Rules), 
			316 => Create_BamlType_Int32KeyFrame(isBamlType, useV3Rules), 
			317 => Create_BamlType_Int32KeyFrameCollection(isBamlType, useV3Rules), 
			318 => Create_BamlType_Int32Rect(isBamlType, useV3Rules), 
			319 => Create_BamlType_Int32RectConverter(isBamlType, useV3Rules), 
			320 => Create_BamlType_Int64(isBamlType, useV3Rules), 
			321 => Create_BamlType_Int64Animation(isBamlType, useV3Rules), 
			322 => Create_BamlType_Int64AnimationBase(isBamlType, useV3Rules), 
			323 => Create_BamlType_Int64AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			324 => Create_BamlType_Int64Converter(isBamlType, useV3Rules), 
			325 => Create_BamlType_Int64KeyFrame(isBamlType, useV3Rules), 
			326 => Create_BamlType_Int64KeyFrameCollection(isBamlType, useV3Rules), 
			327 => Create_BamlType_Italic(isBamlType, useV3Rules), 
			328 => Create_BamlType_ItemCollection(isBamlType, useV3Rules), 
			329 => Create_BamlType_ItemsControl(isBamlType, useV3Rules), 
			330 => Create_BamlType_ItemsPanelTemplate(isBamlType, useV3Rules), 
			331 => Create_BamlType_ItemsPresenter(isBamlType, useV3Rules), 
			332 => Create_BamlType_JournalEntry(isBamlType, useV3Rules), 
			333 => Create_BamlType_JournalEntryListConverter(isBamlType, useV3Rules), 
			334 => Create_BamlType_JournalEntryUnifiedViewConverter(isBamlType, useV3Rules), 
			335 => Create_BamlType_JpegBitmapDecoder(isBamlType, useV3Rules), 
			336 => Create_BamlType_JpegBitmapEncoder(isBamlType, useV3Rules), 
			337 => Create_BamlType_KeyBinding(isBamlType, useV3Rules), 
			338 => Create_BamlType_KeyConverter(isBamlType, useV3Rules), 
			339 => Create_BamlType_KeyGesture(isBamlType, useV3Rules), 
			340 => Create_BamlType_KeyGestureConverter(isBamlType, useV3Rules), 
			341 => Create_BamlType_KeySpline(isBamlType, useV3Rules), 
			342 => Create_BamlType_KeySplineConverter(isBamlType, useV3Rules), 
			343 => Create_BamlType_KeyTime(isBamlType, useV3Rules), 
			344 => Create_BamlType_KeyTimeConverter(isBamlType, useV3Rules), 
			345 => Create_BamlType_KeyboardDevice(isBamlType, useV3Rules), 
			346 => Create_BamlType_Label(isBamlType, useV3Rules), 
			347 => Create_BamlType_LateBoundBitmapDecoder(isBamlType, useV3Rules), 
			348 => Create_BamlType_LengthConverter(isBamlType, useV3Rules), 
			349 => Create_BamlType_Light(isBamlType, useV3Rules), 
			350 => Create_BamlType_Line(isBamlType, useV3Rules), 
			351 => Create_BamlType_LineBreak(isBamlType, useV3Rules), 
			352 => Create_BamlType_LineGeometry(isBamlType, useV3Rules), 
			353 => Create_BamlType_LineSegment(isBamlType, useV3Rules), 
			354 => Create_BamlType_LinearByteKeyFrame(isBamlType, useV3Rules), 
			355 => Create_BamlType_LinearColorKeyFrame(isBamlType, useV3Rules), 
			356 => Create_BamlType_LinearDecimalKeyFrame(isBamlType, useV3Rules), 
			357 => Create_BamlType_LinearDoubleKeyFrame(isBamlType, useV3Rules), 
			358 => Create_BamlType_LinearGradientBrush(isBamlType, useV3Rules), 
			359 => Create_BamlType_LinearInt16KeyFrame(isBamlType, useV3Rules), 
			360 => Create_BamlType_LinearInt32KeyFrame(isBamlType, useV3Rules), 
			361 => Create_BamlType_LinearInt64KeyFrame(isBamlType, useV3Rules), 
			362 => Create_BamlType_LinearPoint3DKeyFrame(isBamlType, useV3Rules), 
			363 => Create_BamlType_LinearPointKeyFrame(isBamlType, useV3Rules), 
			364 => Create_BamlType_LinearQuaternionKeyFrame(isBamlType, useV3Rules), 
			365 => Create_BamlType_LinearRectKeyFrame(isBamlType, useV3Rules), 
			366 => Create_BamlType_LinearRotation3DKeyFrame(isBamlType, useV3Rules), 
			367 => Create_BamlType_LinearSingleKeyFrame(isBamlType, useV3Rules), 
			368 => Create_BamlType_LinearSizeKeyFrame(isBamlType, useV3Rules), 
			369 => Create_BamlType_LinearThicknessKeyFrame(isBamlType, useV3Rules), 
			370 => Create_BamlType_LinearVector3DKeyFrame(isBamlType, useV3Rules), 
			371 => Create_BamlType_LinearVectorKeyFrame(isBamlType, useV3Rules), 
			372 => Create_BamlType_List(isBamlType, useV3Rules), 
			373 => Create_BamlType_ListBox(isBamlType, useV3Rules), 
			374 => Create_BamlType_ListBoxItem(isBamlType, useV3Rules), 
			375 => Create_BamlType_ListCollectionView(isBamlType, useV3Rules), 
			376 => Create_BamlType_ListItem(isBamlType, useV3Rules), 
			377 => Create_BamlType_ListView(isBamlType, useV3Rules), 
			378 => Create_BamlType_ListViewItem(isBamlType, useV3Rules), 
			379 => Create_BamlType_Localization(isBamlType, useV3Rules), 
			380 => Create_BamlType_LostFocusEventManager(isBamlType, useV3Rules), 
			381 => Create_BamlType_MarkupExtension(isBamlType, useV3Rules), 
			382 => Create_BamlType_Material(isBamlType, useV3Rules), 
			383 => Create_BamlType_MaterialCollection(isBamlType, useV3Rules), 
			384 => Create_BamlType_MaterialGroup(isBamlType, useV3Rules), 
			385 => Create_BamlType_Matrix(isBamlType, useV3Rules), 
			386 => Create_BamlType_Matrix3D(isBamlType, useV3Rules), 
			387 => Create_BamlType_Matrix3DConverter(isBamlType, useV3Rules), 
			388 => Create_BamlType_MatrixAnimationBase(isBamlType, useV3Rules), 
			389 => Create_BamlType_MatrixAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			390 => Create_BamlType_MatrixAnimationUsingPath(isBamlType, useV3Rules), 
			391 => Create_BamlType_MatrixCamera(isBamlType, useV3Rules), 
			392 => Create_BamlType_MatrixConverter(isBamlType, useV3Rules), 
			393 => Create_BamlType_MatrixKeyFrame(isBamlType, useV3Rules), 
			394 => Create_BamlType_MatrixKeyFrameCollection(isBamlType, useV3Rules), 
			395 => Create_BamlType_MatrixTransform(isBamlType, useV3Rules), 
			396 => Create_BamlType_MatrixTransform3D(isBamlType, useV3Rules), 
			397 => Create_BamlType_MediaClock(isBamlType, useV3Rules), 
			398 => Create_BamlType_MediaElement(isBamlType, useV3Rules), 
			399 => Create_BamlType_MediaPlayer(isBamlType, useV3Rules), 
			400 => Create_BamlType_MediaTimeline(isBamlType, useV3Rules), 
			401 => Create_BamlType_Menu(isBamlType, useV3Rules), 
			402 => Create_BamlType_MenuBase(isBamlType, useV3Rules), 
			403 => Create_BamlType_MenuItem(isBamlType, useV3Rules), 
			404 => Create_BamlType_MenuScrollingVisibilityConverter(isBamlType, useV3Rules), 
			405 => Create_BamlType_MeshGeometry3D(isBamlType, useV3Rules), 
			406 => Create_BamlType_Model3D(isBamlType, useV3Rules), 
			407 => Create_BamlType_Model3DCollection(isBamlType, useV3Rules), 
			408 => Create_BamlType_Model3DGroup(isBamlType, useV3Rules), 
			409 => Create_BamlType_ModelVisual3D(isBamlType, useV3Rules), 
			410 => Create_BamlType_ModifierKeysConverter(isBamlType, useV3Rules), 
			411 => Create_BamlType_MouseActionConverter(isBamlType, useV3Rules), 
			412 => Create_BamlType_MouseBinding(isBamlType, useV3Rules), 
			413 => Create_BamlType_MouseDevice(isBamlType, useV3Rules), 
			414 => Create_BamlType_MouseGesture(isBamlType, useV3Rules), 
			415 => Create_BamlType_MouseGestureConverter(isBamlType, useV3Rules), 
			416 => Create_BamlType_MultiBinding(isBamlType, useV3Rules), 
			417 => Create_BamlType_MultiBindingExpression(isBamlType, useV3Rules), 
			418 => Create_BamlType_MultiDataTrigger(isBamlType, useV3Rules), 
			419 => Create_BamlType_MultiTrigger(isBamlType, useV3Rules), 
			420 => Create_BamlType_NameScope(isBamlType, useV3Rules), 
			421 => Create_BamlType_NavigationWindow(isBamlType, useV3Rules), 
			422 => Create_BamlType_NullExtension(isBamlType, useV3Rules), 
			423 => Create_BamlType_NullableBoolConverter(isBamlType, useV3Rules), 
			424 => Create_BamlType_NullableConverter(isBamlType, useV3Rules), 
			425 => Create_BamlType_NumberSubstitution(isBamlType, useV3Rules), 
			426 => Create_BamlType_Object(isBamlType, useV3Rules), 
			427 => Create_BamlType_ObjectAnimationBase(isBamlType, useV3Rules), 
			428 => Create_BamlType_ObjectAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			429 => Create_BamlType_ObjectDataProvider(isBamlType, useV3Rules), 
			430 => Create_BamlType_ObjectKeyFrame(isBamlType, useV3Rules), 
			431 => Create_BamlType_ObjectKeyFrameCollection(isBamlType, useV3Rules), 
			432 => Create_BamlType_OrthographicCamera(isBamlType, useV3Rules), 
			433 => Create_BamlType_OuterGlowBitmapEffect(isBamlType, useV3Rules), 
			434 => Create_BamlType_Page(isBamlType, useV3Rules), 
			435 => Create_BamlType_PageContent(isBamlType, useV3Rules), 
			436 => Create_BamlType_PageFunctionBase(isBamlType, useV3Rules), 
			437 => Create_BamlType_Panel(isBamlType, useV3Rules), 
			438 => Create_BamlType_Paragraph(isBamlType, useV3Rules), 
			439 => Create_BamlType_ParallelTimeline(isBamlType, useV3Rules), 
			440 => Create_BamlType_ParserContext(isBamlType, useV3Rules), 
			441 => Create_BamlType_PasswordBox(isBamlType, useV3Rules), 
			442 => Create_BamlType_Path(isBamlType, useV3Rules), 
			443 => Create_BamlType_PathFigure(isBamlType, useV3Rules), 
			444 => Create_BamlType_PathFigureCollection(isBamlType, useV3Rules), 
			445 => Create_BamlType_PathFigureCollectionConverter(isBamlType, useV3Rules), 
			446 => Create_BamlType_PathGeometry(isBamlType, useV3Rules), 
			447 => Create_BamlType_PathSegment(isBamlType, useV3Rules), 
			448 => Create_BamlType_PathSegmentCollection(isBamlType, useV3Rules), 
			449 => Create_BamlType_PauseStoryboard(isBamlType, useV3Rules), 
			450 => Create_BamlType_Pen(isBamlType, useV3Rules), 
			451 => Create_BamlType_PerspectiveCamera(isBamlType, useV3Rules), 
			452 => Create_BamlType_PixelFormat(isBamlType, useV3Rules), 
			453 => Create_BamlType_PixelFormatConverter(isBamlType, useV3Rules), 
			454 => Create_BamlType_PngBitmapDecoder(isBamlType, useV3Rules), 
			455 => Create_BamlType_PngBitmapEncoder(isBamlType, useV3Rules), 
			456 => Create_BamlType_Point(isBamlType, useV3Rules), 
			457 => Create_BamlType_Point3D(isBamlType, useV3Rules), 
			458 => Create_BamlType_Point3DAnimation(isBamlType, useV3Rules), 
			459 => Create_BamlType_Point3DAnimationBase(isBamlType, useV3Rules), 
			460 => Create_BamlType_Point3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			461 => Create_BamlType_Point3DCollection(isBamlType, useV3Rules), 
			462 => Create_BamlType_Point3DCollectionConverter(isBamlType, useV3Rules), 
			463 => Create_BamlType_Point3DConverter(isBamlType, useV3Rules), 
			464 => Create_BamlType_Point3DKeyFrame(isBamlType, useV3Rules), 
			465 => Create_BamlType_Point3DKeyFrameCollection(isBamlType, useV3Rules), 
			466 => Create_BamlType_Point4D(isBamlType, useV3Rules), 
			467 => Create_BamlType_Point4DConverter(isBamlType, useV3Rules), 
			468 => Create_BamlType_PointAnimation(isBamlType, useV3Rules), 
			469 => Create_BamlType_PointAnimationBase(isBamlType, useV3Rules), 
			470 => Create_BamlType_PointAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			471 => Create_BamlType_PointAnimationUsingPath(isBamlType, useV3Rules), 
			472 => Create_BamlType_PointCollection(isBamlType, useV3Rules), 
			473 => Create_BamlType_PointCollectionConverter(isBamlType, useV3Rules), 
			474 => Create_BamlType_PointConverter(isBamlType, useV3Rules), 
			475 => Create_BamlType_PointIListConverter(isBamlType, useV3Rules), 
			476 => Create_BamlType_PointKeyFrame(isBamlType, useV3Rules), 
			477 => Create_BamlType_PointKeyFrameCollection(isBamlType, useV3Rules), 
			478 => Create_BamlType_PointLight(isBamlType, useV3Rules), 
			479 => Create_BamlType_PointLightBase(isBamlType, useV3Rules), 
			480 => Create_BamlType_PolyBezierSegment(isBamlType, useV3Rules), 
			481 => Create_BamlType_PolyLineSegment(isBamlType, useV3Rules), 
			482 => Create_BamlType_PolyQuadraticBezierSegment(isBamlType, useV3Rules), 
			483 => Create_BamlType_Polygon(isBamlType, useV3Rules), 
			484 => Create_BamlType_Polyline(isBamlType, useV3Rules), 
			485 => Create_BamlType_Popup(isBamlType, useV3Rules), 
			486 => Create_BamlType_PresentationSource(isBamlType, useV3Rules), 
			487 => Create_BamlType_PriorityBinding(isBamlType, useV3Rules), 
			488 => Create_BamlType_PriorityBindingExpression(isBamlType, useV3Rules), 
			489 => Create_BamlType_ProgressBar(isBamlType, useV3Rules), 
			490 => Create_BamlType_ProjectionCamera(isBamlType, useV3Rules), 
			491 => Create_BamlType_PropertyPath(isBamlType, useV3Rules), 
			492 => Create_BamlType_PropertyPathConverter(isBamlType, useV3Rules), 
			493 => Create_BamlType_QuadraticBezierSegment(isBamlType, useV3Rules), 
			494 => Create_BamlType_Quaternion(isBamlType, useV3Rules), 
			495 => Create_BamlType_QuaternionAnimation(isBamlType, useV3Rules), 
			496 => Create_BamlType_QuaternionAnimationBase(isBamlType, useV3Rules), 
			497 => Create_BamlType_QuaternionAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			498 => Create_BamlType_QuaternionConverter(isBamlType, useV3Rules), 
			499 => Create_BamlType_QuaternionKeyFrame(isBamlType, useV3Rules), 
			500 => Create_BamlType_QuaternionKeyFrameCollection(isBamlType, useV3Rules), 
			501 => Create_BamlType_QuaternionRotation3D(isBamlType, useV3Rules), 
			502 => Create_BamlType_RadialGradientBrush(isBamlType, useV3Rules), 
			503 => Create_BamlType_RadioButton(isBamlType, useV3Rules), 
			504 => Create_BamlType_RangeBase(isBamlType, useV3Rules), 
			505 => Create_BamlType_Rect(isBamlType, useV3Rules), 
			506 => Create_BamlType_Rect3D(isBamlType, useV3Rules), 
			507 => Create_BamlType_Rect3DConverter(isBamlType, useV3Rules), 
			508 => Create_BamlType_RectAnimation(isBamlType, useV3Rules), 
			509 => Create_BamlType_RectAnimationBase(isBamlType, useV3Rules), 
			510 => Create_BamlType_RectAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			511 => Create_BamlType_RectConverter(isBamlType, useV3Rules), 
			512 => Create_BamlType_RectKeyFrame(isBamlType, useV3Rules), 
			513 => Create_BamlType_RectKeyFrameCollection(isBamlType, useV3Rules), 
			514 => Create_BamlType_Rectangle(isBamlType, useV3Rules), 
			515 => Create_BamlType_RectangleGeometry(isBamlType, useV3Rules), 
			516 => Create_BamlType_RelativeSource(isBamlType, useV3Rules), 
			517 => Create_BamlType_RemoveStoryboard(isBamlType, useV3Rules), 
			518 => Create_BamlType_RenderOptions(isBamlType, useV3Rules), 
			519 => Create_BamlType_RenderTargetBitmap(isBamlType, useV3Rules), 
			520 => Create_BamlType_RepeatBehavior(isBamlType, useV3Rules), 
			521 => Create_BamlType_RepeatBehaviorConverter(isBamlType, useV3Rules), 
			522 => Create_BamlType_RepeatButton(isBamlType, useV3Rules), 
			523 => Create_BamlType_ResizeGrip(isBamlType, useV3Rules), 
			524 => Create_BamlType_ResourceDictionary(isBamlType, useV3Rules), 
			525 => Create_BamlType_ResourceKey(isBamlType, useV3Rules), 
			526 => Create_BamlType_ResumeStoryboard(isBamlType, useV3Rules), 
			527 => Create_BamlType_RichTextBox(isBamlType, useV3Rules), 
			528 => Create_BamlType_RotateTransform(isBamlType, useV3Rules), 
			529 => Create_BamlType_RotateTransform3D(isBamlType, useV3Rules), 
			530 => Create_BamlType_Rotation3D(isBamlType, useV3Rules), 
			531 => Create_BamlType_Rotation3DAnimation(isBamlType, useV3Rules), 
			532 => Create_BamlType_Rotation3DAnimationBase(isBamlType, useV3Rules), 
			533 => Create_BamlType_Rotation3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			534 => Create_BamlType_Rotation3DKeyFrame(isBamlType, useV3Rules), 
			535 => Create_BamlType_Rotation3DKeyFrameCollection(isBamlType, useV3Rules), 
			536 => Create_BamlType_RoutedCommand(isBamlType, useV3Rules), 
			537 => Create_BamlType_RoutedEvent(isBamlType, useV3Rules), 
			538 => Create_BamlType_RoutedEventConverter(isBamlType, useV3Rules), 
			539 => Create_BamlType_RoutedUICommand(isBamlType, useV3Rules), 
			540 => Create_BamlType_RoutingStrategy(isBamlType, useV3Rules), 
			541 => Create_BamlType_RowDefinition(isBamlType, useV3Rules), 
			542 => Create_BamlType_Run(isBamlType, useV3Rules), 
			543 => Create_BamlType_RuntimeNamePropertyAttribute(isBamlType, useV3Rules), 
			544 => Create_BamlType_SByte(isBamlType, useV3Rules), 
			545 => Create_BamlType_SByteConverter(isBamlType, useV3Rules), 
			546 => Create_BamlType_ScaleTransform(isBamlType, useV3Rules), 
			547 => Create_BamlType_ScaleTransform3D(isBamlType, useV3Rules), 
			548 => Create_BamlType_ScrollBar(isBamlType, useV3Rules), 
			549 => Create_BamlType_ScrollContentPresenter(isBamlType, useV3Rules), 
			550 => Create_BamlType_ScrollViewer(isBamlType, useV3Rules), 
			551 => Create_BamlType_Section(isBamlType, useV3Rules), 
			552 => Create_BamlType_SeekStoryboard(isBamlType, useV3Rules), 
			553 => Create_BamlType_Selector(isBamlType, useV3Rules), 
			554 => Create_BamlType_Separator(isBamlType, useV3Rules), 
			555 => Create_BamlType_SetStoryboardSpeedRatio(isBamlType, useV3Rules), 
			556 => Create_BamlType_Setter(isBamlType, useV3Rules), 
			557 => Create_BamlType_SetterBase(isBamlType, useV3Rules), 
			558 => Create_BamlType_Shape(isBamlType, useV3Rules), 
			559 => Create_BamlType_Single(isBamlType, useV3Rules), 
			560 => Create_BamlType_SingleAnimation(isBamlType, useV3Rules), 
			561 => Create_BamlType_SingleAnimationBase(isBamlType, useV3Rules), 
			562 => Create_BamlType_SingleAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			563 => Create_BamlType_SingleConverter(isBamlType, useV3Rules), 
			564 => Create_BamlType_SingleKeyFrame(isBamlType, useV3Rules), 
			565 => Create_BamlType_SingleKeyFrameCollection(isBamlType, useV3Rules), 
			566 => Create_BamlType_Size(isBamlType, useV3Rules), 
			567 => Create_BamlType_Size3D(isBamlType, useV3Rules), 
			568 => Create_BamlType_Size3DConverter(isBamlType, useV3Rules), 
			569 => Create_BamlType_SizeAnimation(isBamlType, useV3Rules), 
			570 => Create_BamlType_SizeAnimationBase(isBamlType, useV3Rules), 
			571 => Create_BamlType_SizeAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			572 => Create_BamlType_SizeConverter(isBamlType, useV3Rules), 
			573 => Create_BamlType_SizeKeyFrame(isBamlType, useV3Rules), 
			574 => Create_BamlType_SizeKeyFrameCollection(isBamlType, useV3Rules), 
			575 => Create_BamlType_SkewTransform(isBamlType, useV3Rules), 
			576 => Create_BamlType_SkipStoryboardToFill(isBamlType, useV3Rules), 
			577 => Create_BamlType_Slider(isBamlType, useV3Rules), 
			578 => Create_BamlType_SolidColorBrush(isBamlType, useV3Rules), 
			579 => Create_BamlType_SoundPlayerAction(isBamlType, useV3Rules), 
			580 => Create_BamlType_Span(isBamlType, useV3Rules), 
			581 => Create_BamlType_SpecularMaterial(isBamlType, useV3Rules), 
			582 => Create_BamlType_SpellCheck(isBamlType, useV3Rules), 
			583 => Create_BamlType_SplineByteKeyFrame(isBamlType, useV3Rules), 
			584 => Create_BamlType_SplineColorKeyFrame(isBamlType, useV3Rules), 
			585 => Create_BamlType_SplineDecimalKeyFrame(isBamlType, useV3Rules), 
			586 => Create_BamlType_SplineDoubleKeyFrame(isBamlType, useV3Rules), 
			587 => Create_BamlType_SplineInt16KeyFrame(isBamlType, useV3Rules), 
			588 => Create_BamlType_SplineInt32KeyFrame(isBamlType, useV3Rules), 
			589 => Create_BamlType_SplineInt64KeyFrame(isBamlType, useV3Rules), 
			590 => Create_BamlType_SplinePoint3DKeyFrame(isBamlType, useV3Rules), 
			591 => Create_BamlType_SplinePointKeyFrame(isBamlType, useV3Rules), 
			592 => Create_BamlType_SplineQuaternionKeyFrame(isBamlType, useV3Rules), 
			593 => Create_BamlType_SplineRectKeyFrame(isBamlType, useV3Rules), 
			594 => Create_BamlType_SplineRotation3DKeyFrame(isBamlType, useV3Rules), 
			595 => Create_BamlType_SplineSingleKeyFrame(isBamlType, useV3Rules), 
			596 => Create_BamlType_SplineSizeKeyFrame(isBamlType, useV3Rules), 
			597 => Create_BamlType_SplineThicknessKeyFrame(isBamlType, useV3Rules), 
			598 => Create_BamlType_SplineVector3DKeyFrame(isBamlType, useV3Rules), 
			599 => Create_BamlType_SplineVectorKeyFrame(isBamlType, useV3Rules), 
			600 => Create_BamlType_SpotLight(isBamlType, useV3Rules), 
			601 => Create_BamlType_StackPanel(isBamlType, useV3Rules), 
			602 => Create_BamlType_StaticExtension(isBamlType, useV3Rules), 
			603 => Create_BamlType_StaticResourceExtension(isBamlType, useV3Rules), 
			604 => Create_BamlType_StatusBar(isBamlType, useV3Rules), 
			605 => Create_BamlType_StatusBarItem(isBamlType, useV3Rules), 
			606 => Create_BamlType_StickyNoteControl(isBamlType, useV3Rules), 
			607 => Create_BamlType_StopStoryboard(isBamlType, useV3Rules), 
			608 => Create_BamlType_Storyboard(isBamlType, useV3Rules), 
			609 => Create_BamlType_StreamGeometry(isBamlType, useV3Rules), 
			610 => Create_BamlType_StreamGeometryContext(isBamlType, useV3Rules), 
			611 => Create_BamlType_StreamResourceInfo(isBamlType, useV3Rules), 
			612 => Create_BamlType_String(isBamlType, useV3Rules), 
			613 => Create_BamlType_StringAnimationBase(isBamlType, useV3Rules), 
			614 => Create_BamlType_StringAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			615 => Create_BamlType_StringConverter(isBamlType, useV3Rules), 
			616 => Create_BamlType_StringKeyFrame(isBamlType, useV3Rules), 
			617 => Create_BamlType_StringKeyFrameCollection(isBamlType, useV3Rules), 
			618 => Create_BamlType_StrokeCollection(isBamlType, useV3Rules), 
			619 => Create_BamlType_StrokeCollectionConverter(isBamlType, useV3Rules), 
			620 => Create_BamlType_Style(isBamlType, useV3Rules), 
			621 => Create_BamlType_Stylus(isBamlType, useV3Rules), 
			622 => Create_BamlType_StylusDevice(isBamlType, useV3Rules), 
			623 => Create_BamlType_TabControl(isBamlType, useV3Rules), 
			624 => Create_BamlType_TabItem(isBamlType, useV3Rules), 
			625 => Create_BamlType_TabPanel(isBamlType, useV3Rules), 
			626 => Create_BamlType_Table(isBamlType, useV3Rules), 
			627 => Create_BamlType_TableCell(isBamlType, useV3Rules), 
			628 => Create_BamlType_TableColumn(isBamlType, useV3Rules), 
			629 => Create_BamlType_TableRow(isBamlType, useV3Rules), 
			630 => Create_BamlType_TableRowGroup(isBamlType, useV3Rules), 
			631 => Create_BamlType_TabletDevice(isBamlType, useV3Rules), 
			632 => Create_BamlType_TemplateBindingExpression(isBamlType, useV3Rules), 
			633 => Create_BamlType_TemplateBindingExpressionConverter(isBamlType, useV3Rules), 
			634 => Create_BamlType_TemplateBindingExtension(isBamlType, useV3Rules), 
			635 => Create_BamlType_TemplateBindingExtensionConverter(isBamlType, useV3Rules), 
			636 => Create_BamlType_TemplateKey(isBamlType, useV3Rules), 
			637 => Create_BamlType_TemplateKeyConverter(isBamlType, useV3Rules), 
			638 => Create_BamlType_TextBlock(isBamlType, useV3Rules), 
			639 => Create_BamlType_TextBox(isBamlType, useV3Rules), 
			640 => Create_BamlType_TextBoxBase(isBamlType, useV3Rules), 
			641 => Create_BamlType_TextComposition(isBamlType, useV3Rules), 
			642 => Create_BamlType_TextCompositionManager(isBamlType, useV3Rules), 
			643 => Create_BamlType_TextDecoration(isBamlType, useV3Rules), 
			644 => Create_BamlType_TextDecorationCollection(isBamlType, useV3Rules), 
			645 => Create_BamlType_TextDecorationCollectionConverter(isBamlType, useV3Rules), 
			646 => Create_BamlType_TextEffect(isBamlType, useV3Rules), 
			647 => Create_BamlType_TextEffectCollection(isBamlType, useV3Rules), 
			648 => Create_BamlType_TextElement(isBamlType, useV3Rules), 
			649 => Create_BamlType_TextSearch(isBamlType, useV3Rules), 
			650 => Create_BamlType_ThemeDictionaryExtension(isBamlType, useV3Rules), 
			651 => Create_BamlType_Thickness(isBamlType, useV3Rules), 
			652 => Create_BamlType_ThicknessAnimation(isBamlType, useV3Rules), 
			653 => Create_BamlType_ThicknessAnimationBase(isBamlType, useV3Rules), 
			654 => Create_BamlType_ThicknessAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			655 => Create_BamlType_ThicknessConverter(isBamlType, useV3Rules), 
			656 => Create_BamlType_ThicknessKeyFrame(isBamlType, useV3Rules), 
			657 => Create_BamlType_ThicknessKeyFrameCollection(isBamlType, useV3Rules), 
			658 => Create_BamlType_Thumb(isBamlType, useV3Rules), 
			659 => Create_BamlType_TickBar(isBamlType, useV3Rules), 
			660 => Create_BamlType_TiffBitmapDecoder(isBamlType, useV3Rules), 
			661 => Create_BamlType_TiffBitmapEncoder(isBamlType, useV3Rules), 
			662 => Create_BamlType_TileBrush(isBamlType, useV3Rules), 
			663 => Create_BamlType_TimeSpan(isBamlType, useV3Rules), 
			664 => Create_BamlType_TimeSpanConverter(isBamlType, useV3Rules), 
			665 => Create_BamlType_Timeline(isBamlType, useV3Rules), 
			666 => Create_BamlType_TimelineCollection(isBamlType, useV3Rules), 
			667 => Create_BamlType_TimelineGroup(isBamlType, useV3Rules), 
			668 => Create_BamlType_ToggleButton(isBamlType, useV3Rules), 
			669 => Create_BamlType_ToolBar(isBamlType, useV3Rules), 
			670 => Create_BamlType_ToolBarOverflowPanel(isBamlType, useV3Rules), 
			671 => Create_BamlType_ToolBarPanel(isBamlType, useV3Rules), 
			672 => Create_BamlType_ToolBarTray(isBamlType, useV3Rules), 
			673 => Create_BamlType_ToolTip(isBamlType, useV3Rules), 
			674 => Create_BamlType_ToolTipService(isBamlType, useV3Rules), 
			675 => Create_BamlType_Track(isBamlType, useV3Rules), 
			676 => Create_BamlType_Transform(isBamlType, useV3Rules), 
			677 => Create_BamlType_Transform3D(isBamlType, useV3Rules), 
			678 => Create_BamlType_Transform3DCollection(isBamlType, useV3Rules), 
			679 => Create_BamlType_Transform3DGroup(isBamlType, useV3Rules), 
			680 => Create_BamlType_TransformCollection(isBamlType, useV3Rules), 
			681 => Create_BamlType_TransformConverter(isBamlType, useV3Rules), 
			682 => Create_BamlType_TransformGroup(isBamlType, useV3Rules), 
			683 => Create_BamlType_TransformedBitmap(isBamlType, useV3Rules), 
			684 => Create_BamlType_TranslateTransform(isBamlType, useV3Rules), 
			685 => Create_BamlType_TranslateTransform3D(isBamlType, useV3Rules), 
			686 => Create_BamlType_TreeView(isBamlType, useV3Rules), 
			687 => Create_BamlType_TreeViewItem(isBamlType, useV3Rules), 
			688 => Create_BamlType_Trigger(isBamlType, useV3Rules), 
			689 => Create_BamlType_TriggerAction(isBamlType, useV3Rules), 
			690 => Create_BamlType_TriggerBase(isBamlType, useV3Rules), 
			691 => Create_BamlType_TypeExtension(isBamlType, useV3Rules), 
			692 => Create_BamlType_TypeTypeConverter(isBamlType, useV3Rules), 
			693 => Create_BamlType_Typography(isBamlType, useV3Rules), 
			694 => Create_BamlType_UIElement(isBamlType, useV3Rules), 
			695 => Create_BamlType_UInt16(isBamlType, useV3Rules), 
			696 => Create_BamlType_UInt16Converter(isBamlType, useV3Rules), 
			697 => Create_BamlType_UInt32(isBamlType, useV3Rules), 
			698 => Create_BamlType_UInt32Converter(isBamlType, useV3Rules), 
			699 => Create_BamlType_UInt64(isBamlType, useV3Rules), 
			700 => Create_BamlType_UInt64Converter(isBamlType, useV3Rules), 
			701 => Create_BamlType_UShortIListConverter(isBamlType, useV3Rules), 
			702 => Create_BamlType_Underline(isBamlType, useV3Rules), 
			703 => Create_BamlType_UniformGrid(isBamlType, useV3Rules), 
			704 => Create_BamlType_Uri(isBamlType, useV3Rules), 
			705 => Create_BamlType_UriTypeConverter(isBamlType, useV3Rules), 
			706 => Create_BamlType_UserControl(isBamlType, useV3Rules), 
			707 => Create_BamlType_Validation(isBamlType, useV3Rules), 
			708 => Create_BamlType_Vector(isBamlType, useV3Rules), 
			709 => Create_BamlType_Vector3D(isBamlType, useV3Rules), 
			710 => Create_BamlType_Vector3DAnimation(isBamlType, useV3Rules), 
			711 => Create_BamlType_Vector3DAnimationBase(isBamlType, useV3Rules), 
			712 => Create_BamlType_Vector3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			713 => Create_BamlType_Vector3DCollection(isBamlType, useV3Rules), 
			714 => Create_BamlType_Vector3DCollectionConverter(isBamlType, useV3Rules), 
			715 => Create_BamlType_Vector3DConverter(isBamlType, useV3Rules), 
			716 => Create_BamlType_Vector3DKeyFrame(isBamlType, useV3Rules), 
			717 => Create_BamlType_Vector3DKeyFrameCollection(isBamlType, useV3Rules), 
			718 => Create_BamlType_VectorAnimation(isBamlType, useV3Rules), 
			719 => Create_BamlType_VectorAnimationBase(isBamlType, useV3Rules), 
			720 => Create_BamlType_VectorAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			721 => Create_BamlType_VectorCollection(isBamlType, useV3Rules), 
			722 => Create_BamlType_VectorCollectionConverter(isBamlType, useV3Rules), 
			723 => Create_BamlType_VectorConverter(isBamlType, useV3Rules), 
			724 => Create_BamlType_VectorKeyFrame(isBamlType, useV3Rules), 
			725 => Create_BamlType_VectorKeyFrameCollection(isBamlType, useV3Rules), 
			726 => Create_BamlType_VideoDrawing(isBamlType, useV3Rules), 
			727 => Create_BamlType_ViewBase(isBamlType, useV3Rules), 
			728 => Create_BamlType_Viewbox(isBamlType, useV3Rules), 
			729 => Create_BamlType_Viewport3D(isBamlType, useV3Rules), 
			730 => Create_BamlType_Viewport3DVisual(isBamlType, useV3Rules), 
			731 => Create_BamlType_VirtualizingPanel(isBamlType, useV3Rules), 
			732 => Create_BamlType_VirtualizingStackPanel(isBamlType, useV3Rules), 
			733 => Create_BamlType_Visual(isBamlType, useV3Rules), 
			734 => Create_BamlType_Visual3D(isBamlType, useV3Rules), 
			735 => Create_BamlType_VisualBrush(isBamlType, useV3Rules), 
			736 => Create_BamlType_VisualTarget(isBamlType, useV3Rules), 
			737 => Create_BamlType_WeakEventManager(isBamlType, useV3Rules), 
			738 => Create_BamlType_WhitespaceSignificantCollectionAttribute(isBamlType, useV3Rules), 
			739 => Create_BamlType_Window(isBamlType, useV3Rules), 
			740 => Create_BamlType_WmpBitmapDecoder(isBamlType, useV3Rules), 
			741 => Create_BamlType_WmpBitmapEncoder(isBamlType, useV3Rules), 
			742 => Create_BamlType_WrapPanel(isBamlType, useV3Rules), 
			743 => Create_BamlType_WriteableBitmap(isBamlType, useV3Rules), 
			744 => Create_BamlType_XamlBrushSerializer(isBamlType, useV3Rules), 
			745 => Create_BamlType_XamlInt32CollectionSerializer(isBamlType, useV3Rules), 
			746 => Create_BamlType_XamlPathDataSerializer(isBamlType, useV3Rules), 
			747 => Create_BamlType_XamlPoint3DCollectionSerializer(isBamlType, useV3Rules), 
			748 => Create_BamlType_XamlPointCollectionSerializer(isBamlType, useV3Rules), 
			749 => Create_BamlType_XamlReader(isBamlType, useV3Rules), 
			750 => Create_BamlType_XamlStyleSerializer(isBamlType, useV3Rules), 
			751 => Create_BamlType_XamlTemplateSerializer(isBamlType, useV3Rules), 
			752 => Create_BamlType_XamlVector3DCollectionSerializer(isBamlType, useV3Rules), 
			753 => Create_BamlType_XamlWriter(isBamlType, useV3Rules), 
			754 => Create_BamlType_XmlDataProvider(isBamlType, useV3Rules), 
			755 => Create_BamlType_XmlLangPropertyAttribute(isBamlType, useV3Rules), 
			756 => Create_BamlType_XmlLanguage(isBamlType, useV3Rules), 
			757 => Create_BamlType_XmlLanguageConverter(isBamlType, useV3Rules), 
			758 => Create_BamlType_XmlNamespaceMapping(isBamlType, useV3Rules), 
			759 => Create_BamlType_ZoomPercentageConverter(isBamlType, useV3Rules), 
			_ => throw new InvalidOperationException("Invalid BAML number"), 
		};
	}

	private uint GetTypeNameHash(string typeName)
	{
		uint num = 0u;
		for (int i = 0; i < 26 && i < typeName.Length; i++)
		{
			num = 101 * num + typeName[i];
		}
		return num;
	}

	protected WpfKnownType CreateKnownBamlType(string typeName, bool isBamlType, bool useV3Rules)
	{
		return GetTypeNameHash(typeName) switch
		{
			826391u => Create_BamlType_Pen(isBamlType, useV3Rules), 
			848409u => Create_BamlType_Run(isBamlType, useV3Rules), 
			878704u => Create_BamlType_Uri(isBamlType, useV3Rules), 
			7210206u => Create_BamlType_Vector3DKeyFrameCollection(isBamlType, useV3Rules), 
			8626695u => Create_BamlType_Typography(isBamlType, useV3Rules), 
			10713943u => Create_BamlType_AxisAngleRotation3D(isBamlType, useV3Rules), 
			17341202u => Create_BamlType_RectKeyFrameCollection(isBamlType, useV3Rules), 
			19590438u => Create_BamlType_ItemsPanelTemplate(isBamlType, useV3Rules), 
			21757238u => Create_BamlType_Quaternion(isBamlType, useV3Rules), 
			27438720u => Create_BamlType_FigureLength(isBamlType, useV3Rules), 
			35895921u => Create_BamlType_ComponentResourceKeyConverter(isBamlType, useV3Rules), 
			44267921u => Create_BamlType_GridViewRowPresenter(isBamlType, useV3Rules), 
			50494706u => Create_BamlType_CommandBindingCollection(isBamlType, useV3Rules), 
			56425604u => Create_BamlType_SplinePoint3DKeyFrame(isBamlType, useV3Rules), 
			69143185u => Create_BamlType_Bold(isBamlType, useV3Rules), 
			69246004u => Create_BamlType_Byte(isBamlType, useV3Rules), 
			70100982u => Create_BamlType_Char(isBamlType, useV3Rules), 
			72192662u => Create_BamlType_MatrixCamera(isBamlType, useV3Rules), 
			72224805u => Create_BamlType_Enum(isBamlType, useV3Rules), 
			74282775u => Create_BamlType_RotateTransform(isBamlType, useV3Rules), 
			74324990u => Create_BamlType_Grid(isBamlType, useV3Rules), 
			74355593u => Create_BamlType_Guid(isBamlType, useV3Rules), 
			79385192u => Create_BamlType_Line(isBamlType, useV3Rules), 
			79385712u => Create_BamlType_List(isBamlType, useV3Rules), 
			80374705u => Create_BamlType_Menu(isBamlType, useV3Rules), 
			83424081u => Create_BamlType_Page(isBamlType, useV3Rules), 
			83425397u => Create_BamlType_Path(isBamlType, useV3Rules), 
			85525098u => Create_BamlType_Rect(isBamlType, useV3Rules), 
			86598511u => Create_BamlType_Size(isBamlType, useV3Rules), 
			86639180u => Create_BamlType_Visual(isBamlType, useV3Rules), 
			86667402u => Create_BamlType_Span(isBamlType, useV3Rules), 
			92454412u => Create_BamlType_ColorAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			95311897u => Create_BamlType_KeyboardDevice(isBamlType, useV3Rules), 
			98196275u => Create_BamlType_DoubleConverter(isBamlType, useV3Rules), 
			114848175u => Create_BamlType_XamlPoint3DCollectionSerializer(isBamlType, useV3Rules), 
			116324695u => Create_BamlType_SByte(isBamlType, useV3Rules), 
			117546261u => Create_BamlType_SplineVector3DKeyFrame(isBamlType, useV3Rules), 
			129393695u => Create_BamlType_VectorAnimation(isBamlType, useV3Rules), 
			133371900u => Create_BamlType_DoubleIListConverter(isBamlType, useV3Rules), 
			133966438u => Create_BamlType_ScrollContentPresenter(isBamlType, useV3Rules), 
			138822808u => Create_BamlType_UIElementCollection(isBamlType, useV3Rules), 
			141025390u => Create_BamlType_CharKeyFrame(isBamlType, useV3Rules), 
			149784707u => Create_BamlType_TextDecorationCollectionConverter(isBamlType, useV3Rules), 
			150436622u => Create_BamlType_SplineRotation3DKeyFrame(isBamlType, useV3Rules), 
			151882568u => Create_BamlType_ModelVisual3D(isBamlType, useV3Rules), 
			153543503u => Create_BamlType_CollectionView(isBamlType, useV3Rules), 
			155230905u => Create_BamlType_Shape(isBamlType, useV3Rules), 
			157696880u => Create_BamlType_BrushConverter(isBamlType, useV3Rules), 
			158646293u => Create_BamlType_TranslateTransform3D(isBamlType, useV3Rules), 
			158796542u => Create_BamlType_TileBrush(isBamlType, useV3Rules), 
			159112278u => Create_BamlType_DecimalAnimationBase(isBamlType, useV3Rules), 
			160906176u => Create_BamlType_GroupItem(isBamlType, useV3Rules), 
			162191870u => Create_BamlType_ThicknessKeyFrameCollection(isBamlType, useV3Rules), 
			163112773u => Create_BamlType_WmpBitmapEncoder(isBamlType, useV3Rules), 
			167522129u => Create_BamlType_EventManager(isBamlType, useV3Rules), 
			167785563u => Create_BamlType_XamlInt32CollectionSerializer(isBamlType, useV3Rules), 
			167838937u => Create_BamlType_Style(isBamlType, useV3Rules), 
			172295577u => Create_BamlType_SeekStoryboard(isBamlType, useV3Rules), 
			176201414u => Create_BamlType_BindingListCollectionView(isBamlType, useV3Rules), 
			180014290u => Create_BamlType_ProgressBar(isBamlType, useV3Rules), 
			185134902u => Create_BamlType_Int16Converter(isBamlType, useV3Rules), 
			185603331u => Create_BamlType_WhitespaceSignificantCollectionAttribute(isBamlType, useV3Rules), 
			188925504u => Create_BamlType_DiscreteInt64KeyFrame(isBamlType, useV3Rules), 
			193712015u => Create_BamlType_ModifierKeysConverter(isBamlType, useV3Rules), 
			208056328u => Create_BamlType_Int64AnimationBase(isBamlType, useV3Rules), 
			220163992u => Create_BamlType_GeometryCollection(isBamlType, useV3Rules), 
			230922235u => Create_BamlType_ThicknessAnimationBase(isBamlType, useV3Rules), 
			236543168u => Create_BamlType_CultureInfo(isBamlType, useV3Rules), 
			240474481u => Create_BamlType_MultiDataTrigger(isBamlType, useV3Rules), 
			246620386u => Create_BamlType_HeaderedContentControl(isBamlType, useV3Rules), 
			252088996u => Create_BamlType_Table(isBamlType, useV3Rules), 
			253854091u => Create_BamlType_DoubleAnimation(isBamlType, useV3Rules), 
			254218629u => Create_BamlType_DiscreteVector3DKeyFrame(isBamlType, useV3Rules), 
			259495020u => Create_BamlType_Thumb(isBamlType, useV3Rules), 
			260974524u => Create_BamlType_KeyGestureConverter(isBamlType, useV3Rules), 
			262392462u => Create_BamlType_TextBox(isBamlType, useV3Rules), 
			265347790u => Create_BamlType_OuterGlowBitmapEffect(isBamlType, useV3Rules), 
			269593009u => Create_BamlType_Track(isBamlType, useV3Rules), 
			278513255u => Create_BamlType_Vector3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			283659891u => Create_BamlType_PenLineJoin(isBamlType, useV3Rules), 
			285954745u => Create_BamlType_TemplateKeyConverter(isBamlType, useV3Rules), 
			291478073u => Create_BamlType_GifBitmapDecoder(isBamlType, useV3Rules), 
			297191555u => Create_BamlType_LineSegment(isBamlType, useV3Rules), 
			300220768u => Create_BamlType_CharAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			314824934u => Create_BamlType_Int32RectConverter(isBamlType, useV3Rules), 
			324370636u => Create_BamlType_Thickness(isBamlType, useV3Rules), 
			326446886u => Create_BamlType_DecimalAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			333511440u => Create_BamlType_PngBitmapDecoder(isBamlType, useV3Rules), 
			337659401u => Create_BamlType_Point3DKeyFrame(isBamlType, useV3Rules), 
			339474011u => Create_BamlType_Decimal(isBamlType, useV3Rules), 
			339935827u => Create_BamlType_DiscreteByteKeyFrame(isBamlType, useV3Rules), 
			340792718u => Create_BamlType_Int16Animation(isBamlType, useV3Rules), 
			357673449u => Create_BamlType_RuntimeNamePropertyAttribute(isBamlType, useV3Rules), 
			363476966u => Create_BamlType_UInt64Converter(isBamlType, useV3Rules), 
			373217479u => Create_BamlType_TemplateBindingExpression(isBamlType, useV3Rules), 
			374151590u => Create_BamlType_BindingBase(isBamlType, useV3Rules), 
			374415758u => Create_BamlType_ToggleButton(isBamlType, useV3Rules), 
			384741759u => Create_BamlType_RadialGradientBrush(isBamlType, useV3Rules), 
			386930200u => Create_BamlType_EmissiveMaterial(isBamlType, useV3Rules), 
			387234139u => Create_BamlType_Decorator(isBamlType, useV3Rules), 
			390343400u => Create_BamlType_RichTextBox(isBamlType, useV3Rules), 
			409173380u => Create_BamlType_Polyline(isBamlType, useV3Rules), 
			409221055u => Create_BamlType_LinearThicknessKeyFrame(isBamlType, useV3Rules), 
			411745576u => Create_BamlType_StatusBarItem(isBamlType, useV3Rules), 
			412334313u => Create_BamlType_DocumentViewer(isBamlType, useV3Rules), 
			414460394u => Create_BamlType_MultiBinding(isBamlType, useV3Rules), 
			425410901u => Create_BamlType_PresentationSource(isBamlType, useV3Rules), 
			431709905u => Create_BamlType_RowDefinitionCollection(isBamlType, useV3Rules), 
			433371184u => Create_BamlType_MeshGeometry3D(isBamlType, useV3Rules), 
			435869667u => Create_BamlType_ContextMenuService(isBamlType, useV3Rules), 
			461968488u => Create_BamlType_RenderTargetBitmap(isBamlType, useV3Rules), 
			465416194u => Create_BamlType_AdornedElementPlaceholder(isBamlType, useV3Rules), 
			473143590u => Create_BamlType_BitmapEffect(isBamlType, useV3Rules), 
			481300314u => Create_BamlType_Int64AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			490900943u => Create_BamlType_IAddChildInternal(isBamlType, useV3Rules), 
			492584280u => Create_BamlType_MouseGestureConverter(isBamlType, useV3Rules), 
			501987435u => Create_BamlType_Rotation3DAnimation(isBamlType, useV3Rules), 
			504184511u => Create_BamlType_ToolBarPanel(isBamlType, useV3Rules), 
			507138120u => Create_BamlType_BooleanConverter(isBamlType, useV3Rules), 
			509621479u => Create_BamlType_Double(isBamlType, useV3Rules), 
			511076833u => Create_BamlType_Localization(isBamlType, useV3Rules), 
			511132298u => Create_BamlType_DynamicResourceExtension(isBamlType, useV3Rules), 
			522405838u => Create_BamlType_UShortIListConverter(isBamlType, useV3Rules), 
			525600274u => Create_BamlType_TemplateBindingExtensionConverter(isBamlType, useV3Rules), 
			532150459u => Create_BamlType_DateTimeConverter2(isBamlType, useV3Rules), 
			554920085u => Create_BamlType_FontFamily(isBamlType, useV3Rules), 
			563168829u => Create_BamlType_Rect3D(isBamlType, useV3Rules), 
			566074239u => Create_BamlType_Expander(isBamlType, useV3Rules), 
			568845828u => Create_BamlType_ScrollBarVisibility(isBamlType, useV3Rules), 
			571143672u => Create_BamlType_GridViewRowPresenterBase(isBamlType, useV3Rules), 
			577530966u => Create_BamlType_DataTrigger(isBamlType, useV3Rules), 
			582823334u => Create_BamlType_UniformGrid(isBamlType, useV3Rules), 
			585590105u => Create_BamlType_CombinedGeometry(isBamlType, useV3Rules), 
			602421868u => Create_BamlType_MouseBinding(isBamlType, useV3Rules), 
			603960058u => Create_BamlType_ColorAnimationBase(isBamlType, useV3Rules), 
			614788594u => Create_BamlType_ContextMenu(isBamlType, useV3Rules), 
			615309592u => Create_BamlType_UIElement(isBamlType, useV3Rules), 
			615357807u => Create_BamlType_VectorAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			615898683u => Create_BamlType_TypeExtension(isBamlType, useV3Rules), 
			620560167u => Create_BamlType_GeneralTransformGroup(isBamlType, useV3Rules), 
			620850810u => Create_BamlType_SizeAnimationBase(isBamlType, useV3Rules), 
			623567164u => Create_BamlType_PageContent(isBamlType, useV3Rules), 
			627070138u => Create_BamlType_SplineColorKeyFrame(isBamlType, useV3Rules), 
			640587303u => Create_BamlType_RoutingStrategy(isBamlType, useV3Rules), 
			646994170u => Create_BamlType_LinearVectorKeyFrame(isBamlType, useV3Rules), 
			649244994u => Create_BamlType_CommandBinding(isBamlType, useV3Rules), 
			655979150u => Create_BamlType_SpecularMaterial(isBamlType, useV3Rules), 
			664895538u => Create_BamlType_TriggerAction(isBamlType, useV3Rules), 
			665996286u => Create_BamlType_QuaternionConverter(isBamlType, useV3Rules), 
			672969529u => Create_BamlType_CornerRadiusConverter(isBamlType, useV3Rules), 
			685428999u => Create_BamlType_PixelFormat(isBamlType, useV3Rules), 
			686620977u => Create_BamlType_XamlStyleSerializer(isBamlType, useV3Rules), 
			686841832u => Create_BamlType_GeometryConverter(isBamlType, useV3Rules), 
			687971593u => Create_BamlType_JpegBitmapDecoder(isBamlType, useV3Rules), 
			698201008u => Create_BamlType_GridLength(isBamlType, useV3Rules), 
			712702706u => Create_BamlType_DocumentReference(isBamlType, useV3Rules), 
			713325256u => Create_BamlType_FrameworkElementFactory(isBamlType, useV3Rules), 
			725957013u => Create_BamlType_Int32AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			727501438u => Create_BamlType_JournalOwnership(isBamlType, useV3Rules), 
			734249444u => Create_BamlType_BevelBitmapEffect(isBamlType, useV3Rules), 
			741421013u => Create_BamlType_DiscreteCharKeyFrame(isBamlType, useV3Rules), 
			748275923u => Create_BamlType_UInt16Converter(isBamlType, useV3Rules), 
			749660283u => Create_BamlType_InlineCollection(isBamlType, useV3Rules), 
			758538788u => Create_BamlType_ICommand(isBamlType, useV3Rules), 
			779609571u => Create_BamlType_ScaleTransform3D(isBamlType, useV3Rules), 
			782411712u => Create_BamlType_FrameworkPropertyMetadata(isBamlType, useV3Rules), 
			784038997u => Create_BamlType_TextDecoration(isBamlType, useV3Rules), 
			784826098u => Create_BamlType_Underline(isBamlType, useV3Rules), 
			787776053u => Create_BamlType_IStyleConnector(isBamlType, useV3Rules), 
			807830300u => Create_BamlType_DefinitionBase(isBamlType, useV3Rules), 
			821654102u => Create_BamlType_QuaternionAnimation(isBamlType, useV3Rules), 
			832085183u => Create_BamlType_NullableBoolConverter(isBamlType, useV3Rules), 
			840953286u => Create_BamlType_PointKeyFrameCollection(isBamlType, useV3Rules), 
			861523813u => Create_BamlType_PriorityBindingExpression(isBamlType, useV3Rules), 
			863295067u => Create_BamlType_ColorConverter(isBamlType, useV3Rules), 
			864192108u => Create_BamlType_ThicknessConverter(isBamlType, useV3Rules), 
			874593556u => Create_BamlType_ClockController(isBamlType, useV3Rules), 
			874609234u => Create_BamlType_DoubleAnimationBase(isBamlType, useV3Rules), 
			880110784u => Create_BamlType_ExpressionConverter(isBamlType, useV3Rules), 
			896504879u => Create_BamlType_DoubleCollection(isBamlType, useV3Rules), 
			897586265u => Create_BamlType_SplineRectKeyFrame(isBamlType, useV3Rules), 
			897706848u => Create_BamlType_TextBlock(isBamlType, useV3Rules), 
			905080928u => Create_BamlType_FixedDocumentSequence(isBamlType, useV3Rules), 
			906240700u => Create_BamlType_UserControl(isBamlType, useV3Rules), 
			912040738u => Create_BamlType_TextEffectCollection(isBamlType, useV3Rules), 
			916823320u => Create_BamlType_InputDevice(isBamlType, useV3Rules), 
			921174220u => Create_BamlType_TriggerCollection(isBamlType, useV3Rules), 
			922642898u => Create_BamlType_PointLight(isBamlType, useV3Rules), 
			926965831u => Create_BamlType_InputScopeName(isBamlType, useV3Rules), 
			936485592u => Create_BamlType_FrameworkRichTextComposition(isBamlType, useV3Rules), 
			937814480u => Create_BamlType_StrokeCollectionConverter(isBamlType, useV3Rules), 
			937862401u => Create_BamlType_GlyphTypeface(isBamlType, useV3Rules), 
			948576441u => Create_BamlType_ArcSegment(isBamlType, useV3Rules), 
			949941650u => Create_BamlType_PropertyPath(isBamlType, useV3Rules), 
			959679175u => Create_BamlType_XamlPathDataSerializer(isBamlType, useV3Rules), 
			961185762u => Create_BamlType_Border(isBamlType, useV3Rules), 
			967604372u => Create_BamlType_FormatConvertedBitmap(isBamlType, useV3Rules), 
			977040319u => Create_BamlType_Validation(isBamlType, useV3Rules), 
			991727131u => Create_BamlType_MouseActionConverter(isBamlType, useV3Rules), 
			996200203u => Create_BamlType_AnimationTimeline(isBamlType, useV3Rules), 
			997254168u => Create_BamlType_Geometry(isBamlType, useV3Rules), 
			997998281u => Create_BamlType_ComboBox(isBamlType, useV3Rules), 
			1016377725u => Create_BamlType_InputMethod(isBamlType, useV3Rules), 
			1018952883u => Create_BamlType_ColorAnimation(isBamlType, useV3Rules), 
			1019262156u => Create_BamlType_PathSegmentCollection(isBamlType, useV3Rules), 
			1019849924u => Create_BamlType_ThicknessAnimation(isBamlType, useV3Rules), 
			1020537735u => Create_BamlType_Material(isBamlType, useV3Rules), 
			1021162590u => Create_BamlType_Vector3DConverter(isBamlType, useV3Rules), 
			1029614653u => Create_BamlType_Point3DCollectionConverter(isBamlType, useV3Rules), 
			1042012617u => Create_BamlType_Rectangle(isBamlType, useV3Rules), 
			1043347506u => Create_BamlType_BorderGapMaskConverter(isBamlType, useV3Rules), 
			1049460504u => Create_BamlType_XmlNamespaceMappingCollection(isBamlType, useV3Rules), 
			1054011130u => Create_BamlType_ThemeDictionaryExtension(isBamlType, useV3Rules), 
			1056330559u => Create_BamlType_GifBitmapEncoder(isBamlType, useV3Rules), 
			1060097603u => Create_BamlType_ColumnDefinitionCollection(isBamlType, useV3Rules), 
			1067429912u => Create_BamlType_ObjectDataProvider(isBamlType, useV3Rules), 
			1069777608u => Create_BamlType_MouseGesture(isBamlType, useV3Rules), 
			1082938778u => Create_BamlType_TableColumn(isBamlType, useV3Rules), 
			1083837605u => Create_BamlType_KeyboardNavigation(isBamlType, useV3Rules), 
			1083922042u => Create_BamlType_PageFunctionBase(isBamlType, useV3Rules), 
			1085414201u => Create_BamlType_LateBoundBitmapDecoder(isBamlType, useV3Rules), 
			1094052145u => Create_BamlType_RectAnimationBase(isBamlType, useV3Rules), 
			1098363926u => Create_BamlType_PngBitmapEncoder(isBamlType, useV3Rules), 
			1104645377u => Create_BamlType_ContentElement(isBamlType, useV3Rules), 
			1107478903u => Create_BamlType_DecimalConverter(isBamlType, useV3Rules), 
			1117366565u => Create_BamlType_PointAnimationUsingPath(isBamlType, useV3Rules), 
			1130648825u => Create_BamlType_SplineInt16KeyFrame(isBamlType, useV3Rules), 
			1150413556u => Create_BamlType_WriteableBitmap(isBamlType, useV3Rules), 
			1158811630u => Create_BamlType_ListViewItem(isBamlType, useV3Rules), 
			1159768689u => Create_BamlType_LinearRectKeyFrame(isBamlType, useV3Rules), 
			1176820406u => Create_BamlType_Vector3DAnimation(isBamlType, useV3Rules), 
			1178626248u => Create_BamlType_InlineUIContainer(isBamlType, useV3Rules), 
			1183725611u => Create_BamlType_ContainerVisual(isBamlType, useV3Rules), 
			1184273902u => Create_BamlType_MediaElement(isBamlType, useV3Rules), 
			1186185889u => Create_BamlType_MarkupExtension(isBamlType, useV3Rules), 
			1209646082u => Create_BamlType_TranslateTransform(isBamlType, useV3Rules), 
			1210722572u => Create_BamlType_BaseIListConverter(isBamlType, useV3Rules), 
			1210906771u => Create_BamlType_VectorCollection(isBamlType, useV3Rules), 
			1221854500u => Create_BamlType_FontStyleConverter(isBamlType, useV3Rules), 
			1227117227u => Create_BamlType_FontWeightConverter(isBamlType, useV3Rules), 
			1239296217u => Create_BamlType_TextComposition(isBamlType, useV3Rules), 
			1253725583u => Create_BamlType_BulletDecorator(isBamlType, useV3Rules), 
			1263136719u => Create_BamlType_DecimalAnimation(isBamlType, useV3Rules), 
			1263268373u => Create_BamlType_Model3DGroup(isBamlType, useV3Rules), 
			1283950600u => Create_BamlType_ResizeGrip(isBamlType, useV3Rules), 
			1285079965u => Create_BamlType_DashStyle(isBamlType, useV3Rules), 
			1285743637u => Create_BamlType_StreamGeometryContext(isBamlType, useV3Rules), 
			1291553535u => Create_BamlType_SplineInt32KeyFrame(isBamlType, useV3Rules), 
			1305993458u => Create_BamlType_TextEffect(isBamlType, useV3Rules), 
			1318104087u => Create_BamlType_BooleanAnimationBase(isBamlType, useV3Rules), 
			1318159567u => Create_BamlType_ImageDrawing(isBamlType, useV3Rules), 
			1337691186u => Create_BamlType_LinearColorKeyFrame(isBamlType, useV3Rules), 
			1338939476u => Create_BamlType_TemplateBindingExtension(isBamlType, useV3Rules), 
			1339394931u => Create_BamlType_ToolBar(isBamlType, useV3Rules), 
			1339579355u => Create_BamlType_ToolTip(isBamlType, useV3Rules), 
			1347486791u => Create_BamlType_ColorKeyFrame(isBamlType, useV3Rules), 
			1359074139u => Create_BamlType_Viewport3DVisual(isBamlType, useV3Rules), 
			1366171760u => Create_BamlType_ImageMetadata(isBamlType, useV3Rules), 
			1369509399u => Create_BamlType_DialogResultConverter(isBamlType, useV3Rules), 
			1370978769u => Create_BamlType_ClockGroup(isBamlType, useV3Rules), 
			1373930089u => Create_BamlType_XamlReader(isBamlType, useV3Rules), 
			1392278866u => Create_BamlType_Size3DConverter(isBamlType, useV3Rules), 
			1395061043u => Create_BamlType_TreeView(isBamlType, useV3Rules), 
			1399972982u => Create_BamlType_SingleKeyFrameCollection(isBamlType, useV3Rules), 
			1407254931u => Create_BamlType_Inline(isBamlType, useV3Rules), 
			1411264711u => Create_BamlType_PointAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1412280093u => Create_BamlType_GridSplitter(isBamlType, useV3Rules), 
			1412505639u => Create_BamlType_CollectionContainer(isBamlType, useV3Rules), 
			1412591399u => Create_BamlType_ToolBarTray(isBamlType, useV3Rules), 
			1419366049u => Create_BamlType_Camera(isBamlType, useV3Rules), 
			1420568068u => Create_BamlType_Canvas(isBamlType, useV3Rules), 
			1423253394u => Create_BamlType_ResourceDictionary(isBamlType, useV3Rules), 
			1423763428u => Create_BamlType_Point3DAnimationBase(isBamlType, useV3Rules), 
			1433323584u => Create_BamlType_TextAlignment(isBamlType, useV3Rules), 
			1441084717u => Create_BamlType_GridView(isBamlType, useV3Rules), 
			1451810926u => Create_BamlType_ParserContext(isBamlType, useV3Rules), 
			1451899428u => Create_BamlType_QuaternionAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1452824079u => Create_BamlType_JpegBitmapEncoder(isBamlType, useV3Rules), 
			1453546252u => Create_BamlType_TickBar(isBamlType, useV3Rules), 
			1463715626u => Create_BamlType_DependencyPropertyConverter(isBamlType, useV3Rules), 
			1483217979u => Create_BamlType_XamlVector3DCollectionSerializer(isBamlType, useV3Rules), 
			1497057972u => Create_BamlType_BlockUIContainer(isBamlType, useV3Rules), 
			1503494182u => Create_BamlType_Paragraph(isBamlType, useV3Rules), 
			1503988241u => Create_BamlType_Storyboard(isBamlType, useV3Rules), 
			1505495632u => Create_BamlType_Freezable(isBamlType, useV3Rules), 
			1505896427u => Create_BamlType_FlowDocument(isBamlType, useV3Rules), 
			1514216138u => Create_BamlType_PropertyPathConverter(isBamlType, useV3Rules), 
			1518131472u => Create_BamlType_GeometryDrawing(isBamlType, useV3Rules), 
			1525454651u => Create_BamlType_ZoomPercentageConverter(isBamlType, useV3Rules), 
			1528777786u => Create_BamlType_LengthConverter(isBamlType, useV3Rules), 
			1534031197u => Create_BamlType_MatrixTransform(isBamlType, useV3Rules), 
			1551028176u => Create_BamlType_DocumentViewerBase(isBamlType, useV3Rules), 
			1553353434u => Create_BamlType_GuidelineSet(isBamlType, useV3Rules), 
			1563195901u => Create_BamlType_HierarchicalDataTemplate(isBamlType, useV3Rules), 
			1566189877u => Create_BamlType_CornerRadius(isBamlType, useV3Rules), 
			1566963134u => Create_BamlType_SplineSizeKeyFrame(isBamlType, useV3Rules), 
			1587772992u => Create_BamlType_Button(isBamlType, useV3Rules), 
			1587863541u => Create_BamlType_JournalEntryListConverter(isBamlType, useV3Rules), 
			1588658228u => Create_BamlType_DiscretePoint3DKeyFrame(isBamlType, useV3Rules), 
			1596615863u => Create_BamlType_TextElement(isBamlType, useV3Rules), 
			1599263472u => Create_BamlType_KeyTimeConverter(isBamlType, useV3Rules), 
			1610838933u => Create_BamlType_MediaPlayer(isBamlType, useV3Rules), 
			1630772625u => Create_BamlType_FixedPage(isBamlType, useV3Rules), 
			1636299558u => Create_BamlType_BeginStoryboard(isBamlType, useV3Rules), 
			1636350275u => Create_BamlType_VectorKeyFrame(isBamlType, useV3Rules), 
			1638466145u => Create_BamlType_JournalEntry(isBamlType, useV3Rules), 
			1641446656u => Create_BamlType_AffineTransform3D(isBamlType, useV3Rules), 
			1648168330u => Create_BamlType_SpotLight(isBamlType, useV3Rules), 
			1648736402u => Create_BamlType_DiscreteVectorKeyFrame(isBamlType, useV3Rules), 
			1649262223u => Create_BamlType_Condition(isBamlType, useV3Rules), 
			1661775612u => Create_BamlType_TransformConverter(isBamlType, useV3Rules), 
			1665515158u => Create_BamlType_Animatable(isBamlType, useV3Rules), 
			1667234335u => Create_BamlType_Glyphs(isBamlType, useV3Rules), 
			1669447028u => Create_BamlType_ByteConverter(isBamlType, useV3Rules), 
			1673388557u => Create_BamlType_DiscreteQuaternionKeyFrame(isBamlType, useV3Rules), 
			1676692392u => Create_BamlType_GradientStopCollection(isBamlType, useV3Rules), 
			1682538720u => Create_BamlType_MediaClock(isBamlType, useV3Rules), 
			1683116109u => Create_BamlType_QuaternionRotation3D(isBamlType, useV3Rules), 
			1684223221u => Create_BamlType_Rotation3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1689931813u => Create_BamlType_Int16AnimationBase(isBamlType, useV3Rules), 
			1698047614u => Create_BamlType_KeyboardNavigationMode(isBamlType, useV3Rules), 
			1700491611u => Create_BamlType_CompositionTarget(isBamlType, useV3Rules), 
			1709260677u => Create_BamlType_Section(isBamlType, useV3Rules), 
			1714171663u => Create_BamlType_FrameworkPropertyMetadataOptions(isBamlType, useV3Rules), 
			1720156579u => Create_BamlType_TriggerBase(isBamlType, useV3Rules), 
			1726725401u => Create_BamlType_Separator(isBamlType, useV3Rules), 
			1727243753u => Create_BamlType_XmlLanguage(isBamlType, useV3Rules), 
			1730845471u => Create_BamlType_NameScope(isBamlType, useV3Rules), 
			1737370437u => Create_BamlType_MouseDevice(isBamlType, useV3Rules), 
			1741197127u => Create_BamlType_NullableConverter(isBamlType, useV3Rules), 
			1749703332u => Create_BamlType_Point3DAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1754018176u => Create_BamlType_LineGeometry(isBamlType, useV3Rules), 
			1774798759u => Create_BamlType_Transform3DCollection(isBamlType, useV3Rules), 
			1784618733u => Create_BamlType_PathGeometry(isBamlType, useV3Rules), 
			1792077897u => Create_BamlType_StaticResourceExtension(isBamlType, useV3Rules), 
			1798811252u => Create_BamlType_Int32Collection(isBamlType, useV3Rules), 
			1799179879u => Create_BamlType_FrameworkContentElement(isBamlType, useV3Rules), 
			1810393776u => Create_BamlType_XmlLangPropertyAttribute(isBamlType, useV3Rules), 
			1811071644u => Create_BamlType_PageContentCollection(isBamlType, useV3Rules), 
			1811729200u => Create_BamlType_BooleanKeyFrame(isBamlType, useV3Rules), 
			1813359201u => Create_BamlType_Rect3DConverter(isBamlType, useV3Rules), 
			1815264388u => Create_BamlType_ThicknessKeyFrame(isBamlType, useV3Rules), 
			1817616839u => Create_BamlType_RadioButton(isBamlType, useV3Rules), 
			1825104844u => Create_BamlType_ByteAnimation(isBamlType, useV3Rules), 
			1829145558u => Create_BamlType_LinearSizeKeyFrame(isBamlType, useV3Rules), 
			1838328148u => Create_BamlType_TextCompositionManager(isBamlType, useV3Rules), 
			1838910454u => Create_BamlType_LinearDoubleKeyFrame(isBamlType, useV3Rules), 
			1841269873u => Create_BamlType_LinearInt16KeyFrame(isBamlType, useV3Rules), 
			1844348898u => Create_BamlType_RotateTransform3D(isBamlType, useV3Rules), 
			1847171633u => Create_BamlType_RoutedEvent(isBamlType, useV3Rules), 
			1847800773u => Create_BamlType_RepeatBehaviorConverter(isBamlType, useV3Rules), 
			1851065478u => Create_BamlType_Int16KeyFrame(isBamlType, useV3Rules), 
			1857902570u => Create_BamlType_DiscreteColorKeyFrame(isBamlType, useV3Rules), 
			1867638374u => Create_BamlType_LinearDecimalKeyFrame(isBamlType, useV3Rules), 
			1872596098u => Create_BamlType_GroupBox(isBamlType, useV3Rules), 
			1886012771u => Create_BamlType_SByteConverter(isBamlType, useV3Rules), 
			1888712354u => Create_BamlType_SplineVectorKeyFrame(isBamlType, useV3Rules), 
			1894131576u => Create_BamlType_ToolTipService(isBamlType, useV3Rules), 
			1899232249u => Create_BamlType_DockPanel(isBamlType, useV3Rules), 
			1899479598u => Create_BamlType_GeneralTransformCollection(isBamlType, useV3Rules), 
			1908047602u => Create_BamlType_InputScope(isBamlType, useV3Rules), 
			1908918452u => Create_BamlType_Int32CollectionConverter(isBamlType, useV3Rules), 
			1912422369u => Create_BamlType_LineBreak(isBamlType, useV3Rules), 
			1921765486u => Create_BamlType_HostVisual(isBamlType, useV3Rules), 
			1930264739u => Create_BamlType_ObjectAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1941591540u => Create_BamlType_ListBoxItem(isBamlType, useV3Rules), 
			1949943781u => Create_BamlType_Point3DConverter(isBamlType, useV3Rules), 
			1950874384u => Create_BamlType_Expression(isBamlType, useV3Rules), 
			1952565839u => Create_BamlType_BitmapDecoder(isBamlType, useV3Rules), 
			1961606018u => Create_BamlType_SingleAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			1972001235u => Create_BamlType_PathFigureCollection(isBamlType, useV3Rules), 
			1974320711u => Create_BamlType_Rotation3D(isBamlType, useV3Rules), 
			1977826323u => Create_BamlType_InputScopeNameConverter(isBamlType, useV3Rules), 
			1978946399u => Create_BamlType_PauseStoryboard(isBamlType, useV3Rules), 
			1981708784u => Create_BamlType_MatrixAnimationBase(isBamlType, useV3Rules), 
			1982598063u => Create_BamlType_Adorner(isBamlType, useV3Rules), 
			1983189101u => Create_BamlType_QuaternionAnimationBase(isBamlType, useV3Rules), 
			1987454919u => Create_BamlType_SplineThicknessKeyFrame(isBamlType, useV3Rules), 
			1992540733u => Create_BamlType_Stretch(isBamlType, useV3Rules), 
			2001481592u => Create_BamlType_Window(isBamlType, useV3Rules), 
			2002174583u => Create_BamlType_LinearInt32KeyFrame(isBamlType, useV3Rules), 
			2009851621u => Create_BamlType_MatrixKeyFrame(isBamlType, useV3Rules), 
			2011970188u => Create_BamlType_Int32KeyFrame(isBamlType, useV3Rules), 
			2012322180u => Create_BamlType_PasswordBox(isBamlType, useV3Rules), 
			2020314122u => Create_BamlType_Italic(isBamlType, useV3Rules), 
			2020350540u => Create_BamlType_GeometryModel3D(isBamlType, useV3Rules), 
			2021668905u => Create_BamlType_PointLightBase(isBamlType, useV3Rules), 
			2022237748u => Create_BamlType_DiscreteMatrixKeyFrame(isBamlType, useV3Rules), 
			2026683522u => Create_BamlType_TransformedBitmap(isBamlType, useV3Rules), 
			2042108315u => Create_BamlType_ColumnDefinition(isBamlType, useV3Rules), 
			2043908275u => Create_BamlType_PenLineCap(isBamlType, useV3Rules), 
			2045195350u => Create_BamlType_StickyNoteControl(isBamlType, useV3Rules), 
			2057591265u => Create_BamlType_ColorConvertedBitmapExtension(isBamlType, useV3Rules), 
			2082963390u => Create_BamlType_CharKeyFrameCollection(isBamlType, useV3Rules), 
			2086386488u => Create_BamlType_MatrixTransform3D(isBamlType, useV3Rules), 
			2090698417u => Create_BamlType_ResourceKey(isBamlType, useV3Rules), 
			2090772835u => Create_BamlType_CharIListConverter(isBamlType, useV3Rules), 
			2095403106u => Create_BamlType_RectKeyFrame(isBamlType, useV3Rules), 
			2105601597u => Create_BamlType_Point3DAnimation(isBamlType, useV3Rules), 
			2116926181u => Create_BamlType_ListBox(isBamlType, useV3Rules), 
			2118568062u => Create_BamlType_NumberSubstitution(isBamlType, useV3Rules), 
			2134177976u => Create_BamlType_DrawingBrush(isBamlType, useV3Rules), 
			2145171279u => Create_BamlType_InputLanguageManager(isBamlType, useV3Rules), 
			2147133521u => Create_BamlType_RepeatBehavior(isBamlType, useV3Rules), 
			2175789292u => Create_BamlType_Trigger(isBamlType, useV3Rules), 
			2181752774u => Create_BamlType_Hyperlink(isBamlType, useV3Rules), 
			2187607252u => Create_BamlType_ContentControl(isBamlType, useV3Rules), 
			2194377413u => Create_BamlType_TimeSpan(isBamlType, useV3Rules), 
			2203399086u => Create_BamlType_SetterBase(isBamlType, useV3Rules), 
			2213541446u => Create_BamlType_FrameworkTemplate(isBamlType, useV3Rules), 
			2220064835u => Create_BamlType_Timeline(isBamlType, useV3Rules), 
			2224116138u => Create_BamlType_InputScopeConverter(isBamlType, useV3Rules), 
			2233723591u => Create_BamlType_DoubleKeyFrameCollection(isBamlType, useV3Rules), 
			2239188145u => Create_BamlType_ControlTemplate(isBamlType, useV3Rules), 
			2242193787u => Create_BamlType_StringKeyFrameCollection(isBamlType, useV3Rules), 
			2242877643u => Create_BamlType_GestureRecognizer(isBamlType, useV3Rules), 
			2245298560u => Create_BamlType_KeyBinding(isBamlType, useV3Rules), 
			2247557147u => Create_BamlType_ContentPresenter(isBamlType, useV3Rules), 
			2270334750u => Create_BamlType_XmlDataProvider(isBamlType, useV3Rules), 
			2275985883u => Create_BamlType_SizeConverter(isBamlType, useV3Rules), 
			2283191896u => Create_BamlType_TableRow(isBamlType, useV3Rules), 
			2286541048u => Create_BamlType_Boolean(isBamlType, useV3Rules), 
			2291279447u => Create_BamlType_ItemsControl(isBamlType, useV3Rules), 
			2293688015u => Create_BamlType_PolyLineSegment(isBamlType, useV3Rules), 
			2303212540u => Create_BamlType_Transform3DGroup(isBamlType, useV3Rules), 
			2305986747u => Create_BamlType_IComponentConnector(isBamlType, useV3Rules), 
			2309433103u => Create_BamlType_TextSearch(isBamlType, useV3Rules), 
			2316123992u => Create_BamlType_DrawingCollection(isBamlType, useV3Rules), 
			2325564233u => Create_BamlType_DrawingContext(isBamlType, useV3Rules), 
			2338158216u => Create_BamlType_JournalEntryUnifiedViewConverter(isBamlType, useV3Rules), 
			2341092446u => Create_BamlType_BitmapMetadata(isBamlType, useV3Rules), 
			2357823000u => Create_BamlType_MenuBase(isBamlType, useV3Rules), 
			2357963071u => Create_BamlType_ListCollectionView(isBamlType, useV3Rules), 
			2359651825u => Create_BamlType_Point3D(isBamlType, useV3Rules), 
			2359651926u => Create_BamlType_Point4D(isBamlType, useV3Rules), 
			2361481257u => Create_BamlType_DiscreteInt16KeyFrame(isBamlType, useV3Rules), 
			2361798307u => Create_BamlType_Int32AnimationBase(isBamlType, useV3Rules), 
			2364198568u => Create_BamlType_Matrix3D(isBamlType, useV3Rules), 
			2365227520u => Create_BamlType_MenuItem(isBamlType, useV3Rules), 
			2366255821u => Create_BamlType_Vector3DAnimationBase(isBamlType, useV3Rules), 
			2368443675u => Create_BamlType_DecimalKeyFrameCollection(isBamlType, useV3Rules), 
			2371777274u => Create_BamlType_InkPresenter(isBamlType, useV3Rules), 
			2372223669u => Create_BamlType_Int64KeyFrameCollection(isBamlType, useV3Rules), 
			2375952462u => Create_BamlType_CursorConverter(isBamlType, useV3Rules), 
			2382404033u => Create_BamlType_RectangleGeometry(isBamlType, useV3Rules), 
			2384031129u => Create_BamlType_RowDefinition(isBamlType, useV3Rules), 
			2387462803u => Create_BamlType_StringKeyFrame(isBamlType, useV3Rules), 
			2395439922u => Create_BamlType_Rotation3DAnimationBase(isBamlType, useV3Rules), 
			2397363533u => Create_BamlType_DiffuseMaterial(isBamlType, useV3Rules), 
			2399848930u => Create_BamlType_DiscreteStringKeyFrame(isBamlType, useV3Rules), 
			2401541526u => Create_BamlType_ViewBase(isBamlType, useV3Rules), 
			2405666083u => Create_BamlType_AnchoredBlock(isBamlType, useV3Rules), 
			2414694242u => Create_BamlType_ProjectionCamera(isBamlType, useV3Rules), 
			2419799105u => Create_BamlType_EventSetter(isBamlType, useV3Rules), 
			2431400980u => Create_BamlType_ContentWrapperAttribute(isBamlType, useV3Rules), 
			2431643699u => Create_BamlType_SizeAnimation(isBamlType, useV3Rules), 
			2439005345u => Create_BamlType_SizeAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			2464017094u => Create_BamlType_FontSizeConverter(isBamlType, useV3Rules), 
			2480012208u => Create_BamlType_BooleanToVisibilityConverter(isBamlType, useV3Rules), 
			2495537998u => Create_BamlType_Ellipse(isBamlType, useV3Rules), 
			2496400610u => Create_BamlType_DataTemplate(isBamlType, useV3Rules), 
			2500030087u => Create_BamlType_OrthographicCamera(isBamlType, useV3Rules), 
			2500854951u => Create_BamlType_Setter(isBamlType, useV3Rules), 
			2507216059u => Create_BamlType_Geometry3D(isBamlType, useV3Rules), 
			2510108609u => Create_BamlType_Point3DKeyFrameCollection(isBamlType, useV3Rules), 
			2510136870u => Create_BamlType_DoubleAnimationUsingPath(isBamlType, useV3Rules), 
			2518729620u => Create_BamlType_KeySpline(isBamlType, useV3Rules), 
			2522385967u => Create_BamlType_DiscreteInt32KeyFrame(isBamlType, useV3Rules), 
			2523498610u => Create_BamlType_UriTypeConverter(isBamlType, useV3Rules), 
			2526122409u => Create_BamlType_KeyConverter(isBamlType, useV3Rules), 
			2537793262u => Create_BamlType_BitmapSource(isBamlType, useV3Rules), 
			2540000939u => Create_BamlType_VectorKeyFrameCollection(isBamlType, useV3Rules), 
			2546467590u => Create_BamlType_AdornerDecorator(isBamlType, useV3Rules), 
			2549694123u => Create_BamlType_GridViewColumn(isBamlType, useV3Rules), 
			2563020253u => Create_BamlType_GuidConverter(isBamlType, useV3Rules), 
			2563899293u => Create_BamlType_StaticExtension(isBamlType, useV3Rules), 
			2564710999u => Create_BamlType_StopStoryboard(isBamlType, useV3Rules), 
			2568847263u => Create_BamlType_CheckBox(isBamlType, useV3Rules), 
			2573940685u => Create_BamlType_CachedBitmap(isBamlType, useV3Rules), 
			2579083438u => Create_BamlType_EventTrigger(isBamlType, useV3Rules), 
			2586667908u => Create_BamlType_MaterialGroup(isBamlType, useV3Rules), 
			2588718206u => Create_BamlType_BindingExpressionBase(isBamlType, useV3Rules), 
			2590675289u => Create_BamlType_StatusBar(isBamlType, useV3Rules), 
			2594318825u => Create_BamlType_EnumConverter(isBamlType, useV3Rules), 
			2599258965u => Create_BamlType_DateTimeConverter(isBamlType, useV3Rules), 
			2603137612u => Create_BamlType_ComponentResourceKey(isBamlType, useV3Rules), 
			2604679664u => Create_BamlType_FigureLengthConverter(isBamlType, useV3Rules), 
			2616916250u => Create_BamlType_CroppedBitmap(isBamlType, useV3Rules), 
			2622762262u => Create_BamlType_Int16KeyFrameCollection(isBamlType, useV3Rules), 
			2625820903u => Create_BamlType_ItemCollection(isBamlType, useV3Rules), 
			2630693784u => Create_BamlType_ComboBoxItem(isBamlType, useV3Rules), 
			2632968446u => Create_BamlType_SetterBaseCollection(isBamlType, useV3Rules), 
			2644858326u => Create_BamlType_SolidColorBrush(isBamlType, useV3Rules), 
			2654418985u => Create_BamlType_DrawingGroup(isBamlType, useV3Rules), 
			2667804183u => Create_BamlType_FrameworkTextComposition(isBamlType, useV3Rules), 
			2677748290u => Create_BamlType_XmlNamespaceMapping(isBamlType, useV3Rules), 
			2683039828u => Create_BamlType_Polygon(isBamlType, useV3Rules), 
			2685434095u => Create_BamlType_Block(isBamlType, useV3Rules), 
			2687716458u => Create_BamlType_PolyQuadraticBezierSegment(isBamlType, useV3Rules), 
			2691678720u => Create_BamlType_Brush(isBamlType, useV3Rules), 
			2695798729u => Create_BamlType_DiscreteRectKeyFrame(isBamlType, useV3Rules), 
			2697498068u => Create_BamlType_StreamGeometry(isBamlType, useV3Rules), 
			2697933609u => Create_BamlType_SplinePointKeyFrame(isBamlType, useV3Rules), 
			2704854826u => Create_BamlType_MultiBindingExpression(isBamlType, useV3Rules), 
			2707718720u => Create_BamlType_AdornerLayer(isBamlType, useV3Rules), 
			2712654300u => Create_BamlType_KeyGesture(isBamlType, useV3Rules), 
			2714912374u => Create_BamlType_ColorConvertedBitmap(isBamlType, useV3Rules), 
			2717418325u => Create_BamlType_BitmapEncoder(isBamlType, useV3Rules), 
			2723992168u => Create_BamlType_ScrollBar(isBamlType, useV3Rules), 
			2727123374u => Create_BamlType_SplineDecimalKeyFrame(isBamlType, useV3Rules), 
			2737207145u => Create_BamlType_GeometryGroup(isBamlType, useV3Rules), 
			2741821828u => Create_BamlType_DependencyProperty(isBamlType, useV3Rules), 
			2745869284u => Create_BamlType_TabletDevice(isBamlType, useV3Rules), 
			2750913568u => Create_BamlType_TabControl(isBamlType, useV3Rules), 
			2752355982u => Create_BamlType_Vector3DKeyFrame(isBamlType, useV3Rules), 
			2764779975u => Create_BamlType_SizeKeyFrame(isBamlType, useV3Rules), 
			2770480768u => Create_BamlType_FontStretchConverter(isBamlType, useV3Rules), 
			2775858686u => Create_BamlType_DiscreteRotation3DKeyFrame(isBamlType, useV3Rules), 
			2779938702u => Create_BamlType_ByteAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			2789494496u => Create_BamlType_Clock(isBamlType, useV3Rules), 
			2792556015u => Create_BamlType_Color(isBamlType, useV3Rules), 
			2821657175u => Create_BamlType_StringConverter(isBamlType, useV3Rules), 
			2823948821u => Create_BamlType_PointAnimationBase(isBamlType, useV3Rules), 
			2825488812u => Create_BamlType_CollectionViewSource(isBamlType, useV3Rules), 
			2828266559u => Create_BamlType_DoubleKeyFrame(isBamlType, useV3Rules), 
			2829278862u => Create_BamlType_BmpBitmapDecoder(isBamlType, useV3Rules), 
			2830133971u => Create_BamlType_InputBindingCollection(isBamlType, useV3Rules), 
			2833582507u => Create_BamlType_PathFigure(isBamlType, useV3Rules), 
			2836690659u => Create_BamlType_SplineByteKeyFrame(isBamlType, useV3Rules), 
			2840652686u => Create_BamlType_DiscreteDoubleKeyFrame(isBamlType, useV3Rules), 
			2853214736u => Create_BamlType_NavigationWindow(isBamlType, useV3Rules), 
			2854085569u => Create_BamlType_Control(isBamlType, useV3Rules), 
			2855103477u => Create_BamlType_LinearQuaternionKeyFrame(isBamlType, useV3Rules), 
			2856553785u => Create_BamlType_GlyphRunDrawing(isBamlType, useV3Rules), 
			2857244043u => Create_BamlType_DrawingImage(isBamlType, useV3Rules), 
			2865322288u => Create_BamlType_CultureInfoConverter(isBamlType, useV3Rules), 
			2867809997u => Create_BamlType_QuaternionKeyFrameCollection(isBamlType, useV3Rules), 
			2872636339u => Create_BamlType_RemoveStoryboard(isBamlType, useV3Rules), 
			2880449407u => Create_BamlType_DataTemplateKey(isBamlType, useV3Rules), 
			2884063696u => Create_BamlType_FontStretch(isBamlType, useV3Rules), 
			2884746986u => Create_BamlType_WrapPanel(isBamlType, useV3Rules), 
			2892711692u => Create_BamlType_TiffBitmapDecoder(isBamlType, useV3Rules), 
			2906462199u => Create_BamlType_DiscreteThicknessKeyFrame(isBamlType, useV3Rules), 
			2910782830u => Create_BamlType_Single(isBamlType, useV3Rules), 
			2923120250u => Create_BamlType_Size3D(isBamlType, useV3Rules), 
			2953557280u => Create_BamlType_TableCell(isBamlType, useV3Rules), 
			2954759040u => Create_BamlType_KeyTime(isBamlType, useV3Rules), 
			2958853687u => Create_BamlType_ObjectKeyFrame(isBamlType, useV3Rules), 
			2971239814u => Create_BamlType_DiscreteObjectKeyFrame(isBamlType, useV3Rules), 
			2979874461u => Create_BamlType_LinearSingleKeyFrame(isBamlType, useV3Rules), 
			2990868428u => Create_BamlType_IconBitmapDecoder(isBamlType, useV3Rules), 
			2992435596u => Create_BamlType_Orientation(isBamlType, useV3Rules), 
			2997528560u => Create_BamlType_PathFigureCollectionConverter(isBamlType, useV3Rules), 
			3000815496u => Create_BamlType_Viewbox(isBamlType, useV3Rules), 
			3005129221u => Create_BamlType_SingleAnimationBase(isBamlType, useV3Rules), 
			3005265189u => Create_BamlType_TextBoxBase(isBamlType, useV3Rules), 
			3008357171u => Create_BamlType_DecimalKeyFrame(isBamlType, useV3Rules), 
			3016322347u => Create_BamlType_DoubleAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3017582335u => Create_BamlType_ObjectKeyFrameCollection(isBamlType, useV3Rules), 
			3024874406u => Create_BamlType_VectorAnimationBase(isBamlType, useV3Rules), 
			3031820371u => Create_BamlType_VideoDrawing(isBamlType, useV3Rules), 
			3035410420u => Create_BamlType_TypeTypeConverter(isBamlType, useV3Rules), 
			3040108565u => Create_BamlType_HeaderedItemsControl(isBamlType, useV3Rules), 
			3040714245u => Create_BamlType_BoolIListConverter(isBamlType, useV3Rules), 
			3042527602u => Create_BamlType_ResumeStoryboard(isBamlType, useV3Rules), 
			3055664686u => Create_BamlType_SkewTransform(isBamlType, useV3Rules), 
			3056264338u => Create_BamlType_GridViewHeaderRowPresenter(isBamlType, useV3Rules), 
			3061725932u => Create_BamlType_FrameworkElement(isBamlType, useV3Rules), 
			3062728027u => Create_BamlType_DiscreteBooleanKeyFrame(isBamlType, useV3Rules), 
			3077777987u => Create_BamlType_MediaTimeline(isBamlType, useV3Rules), 
			3078955044u => Create_BamlType_FontStyle(isBamlType, useV3Rules), 
			3080628638u => Create_BamlType_SplineDoubleKeyFrame(isBamlType, useV3Rules), 
			3087488479u => Create_BamlType_Object(isBamlType, useV3Rules), 
			3091177486u => Create_BamlType_PointCollection(isBamlType, useV3Rules), 
			3098873083u => Create_BamlType_LinearByteKeyFrame(isBamlType, useV3Rules), 
			3100133790u => Create_BamlType_Rotation3DKeyFrameCollection(isBamlType, useV3Rules), 
			3107715695u => Create_BamlType_Frame(isBamlType, useV3Rules), 
			3109717207u => Create_BamlType_Int16AnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3114475758u => Create_BamlType_FlowDocumentPageViewer(isBamlType, useV3Rules), 
			3114500727u => Create_BamlType_BindingExpression(isBamlType, useV3Rules), 
			3119883437u => Create_BamlType_XamlPointCollectionSerializer(isBamlType, useV3Rules), 
			3120891186u => Create_BamlType_DurationConverter(isBamlType, useV3Rules), 
			3124512808u => Create_BamlType_StreamResourceInfo(isBamlType, useV3Rules), 
			3131559342u => Create_BamlType_QuaternionKeyFrame(isBamlType, useV3Rules), 
			3131853152u => Create_BamlType_XamlBrushSerializer(isBamlType, useV3Rules), 
			3133507978u => Create_BamlType_TabItem(isBamlType, useV3Rules), 
			3134642154u => Create_BamlType_SkipStoryboardToFill(isBamlType, useV3Rules), 
			3150804609u => Create_BamlType_InPlaceBitmapMetadataWriter(isBamlType, useV3Rules), 
			3156616619u => Create_BamlType_TimelineCollection(isBamlType, useV3Rules), 
			3157049388u => Create_BamlType_BitmapPalette(isBamlType, useV3Rules), 
			3160936067u => Create_BamlType_ByteAnimationBase(isBamlType, useV3Rules), 
			3168456847u => Create_BamlType_ColorKeyFrameCollection(isBamlType, useV3Rules), 
			3179498907u => Create_BamlType_DoubleCollectionConverter(isBamlType, useV3Rules), 
			3184112475u => Create_BamlType_ThicknessAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3187081615u => Create_BamlType_BitmapEffectGroup(isBamlType, useV3Rules), 
			3191294337u => Create_BamlType_SetStoryboardSpeedRatio(isBamlType, useV3Rules), 
			3213500826u => Create_BamlType_MenuScrollingVisibilityConverter(isBamlType, useV3Rules), 
			3217781231u => Create_BamlType_Slider(isBamlType, useV3Rules), 
			3223906597u => Create_BamlType_ScrollViewer(isBamlType, useV3Rules), 
			3225341700u => Create_BamlType_BitmapFrame(isBamlType, useV3Rules), 
			3232444943u => Create_BamlType_SizeKeyFrameCollection(isBamlType, useV3Rules), 
			3239409257u => Create_BamlType_Point3DCollection(isBamlType, useV3Rules), 
			3245663526u => Create_BamlType_ItemsPresenter(isBamlType, useV3Rules), 
			3249372079u => Create_BamlType_ItemContainerTemplateKey(isBamlType, useV3Rules), 
			3251319004u => Create_BamlType_StylusDevice(isBamlType, useV3Rules), 
			3253060368u => Create_BamlType_SplineInt64KeyFrame(isBamlType, useV3Rules), 
			3254666903u => Create_BamlType_BlurBitmapEffect(isBamlType, useV3Rules), 
			3269592841u => Create_BamlType_TimeSpanConverter(isBamlType, useV3Rules), 
			3273980620u => Create_BamlType_ByteKeyFrameCollection(isBamlType, useV3Rules), 
			3277780192u => Create_BamlType_StrokeCollection(isBamlType, useV3Rules), 
			3310116788u => Create_BamlType_Model3DCollection(isBamlType, useV3Rules), 
			3319196847u => Create_BamlType_ControllableStoryboardAction(isBamlType, useV3Rules), 
			3326778732u => Create_BamlType_Int32KeyFrameCollection(isBamlType, useV3Rules), 
			3332504754u => Create_BamlType_DirectionalLight(isBamlType, useV3Rules), 
			3335227078u => Create_BamlType_TemplateBindingExpressionConverter(isBamlType, useV3Rules), 
			3337633457u => Create_BamlType_MatrixConverter(isBamlType, useV3Rules), 
			3337984719u => Create_BamlType_Visual3D(isBamlType, useV3Rules), 
			3342933789u => Create_BamlType_SplineQuaternionKeyFrame(isBamlType, useV3Rules), 
			3347030199u => Create_BamlType_MultiTrigger(isBamlType, useV3Rules), 
			3361683901u => Create_BamlType_XmlLanguageConverter(isBamlType, useV3Rules), 
			3363408079u => Create_BamlType_ListItem(isBamlType, useV3Rules), 
			3365175598u => Create_BamlType_DiscreteSizeKeyFrame(isBamlType, useV3Rules), 
			3376689791u => Create_BamlType_ListView(isBamlType, useV3Rules), 
			3388398850u => Create_BamlType_RectConverter(isBamlType, useV3Rules), 
			3391091418u => Create_BamlType_BitmapEffectInput(isBamlType, useV3Rules), 
			3402641106u => Create_BamlType_ItemContainerTemplate(isBamlType, useV3Rules), 
			3402758583u => Create_BamlType_ButtonBase(isBamlType, useV3Rules), 
			3402930733u => Create_BamlType_CharAnimationBase(isBamlType, useV3Rules), 
			3404714779u => Create_BamlType_CommandConverter(isBamlType, useV3Rules), 
			3408554657u => Create_BamlType_LinearPointKeyFrame(isBamlType, useV3Rules), 
			3414744787u => Create_BamlType_Image(isBamlType, useV3Rules), 
			3415963406u => Create_BamlType_Int16(isBamlType, useV3Rules), 
			3415963604u => Create_BamlType_Int32(isBamlType, useV3Rules), 
			3415963909u => Create_BamlType_Int64(isBamlType, useV3Rules), 
			3418350262u => Create_BamlType_PointKeyFrame(isBamlType, useV3Rules), 
			3421308423u => Create_BamlType_UInt16(isBamlType, useV3Rules), 
			3421308621u => Create_BamlType_UInt32(isBamlType, useV3Rules), 
			3421308926u => Create_BamlType_UInt64(isBamlType, useV3Rules), 
			3431074367u => Create_BamlType_ToolBarOverflowPanel(isBamlType, useV3Rules), 
			3440459974u => Create_BamlType_InkCanvas(isBamlType, useV3Rules), 
			3446789391u => Create_BamlType_FocusManager(isBamlType, useV3Rules), 
			3448499789u => Create_BamlType_Matrix(isBamlType, useV3Rules), 
			3462744685u => Create_BamlType_Floater(isBamlType, useV3Rules), 
			3491907900u => Create_BamlType_LinearPoint3DKeyFrame(isBamlType, useV3Rules), 
			3493262121u => Create_BamlType_DropShadowBitmapEffect(isBamlType, useV3Rules), 
			3503874477u => Create_BamlType_LinearVector3DKeyFrame(isBamlType, useV3Rules), 
			3505868102u => Create_BamlType_Cursor(isBamlType, useV3Rules), 
			3515909783u => Create_BamlType_Viewport3D(isBamlType, useV3Rules), 
			3517750932u => Create_BamlType_ParallelTimeline(isBamlType, useV3Rules), 
			3521445823u => Create_BamlType_StringAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3523046863u => Create_BamlType_MaterialCollection(isBamlType, useV3Rules), 
			3527208580u => Create_BamlType_RenderOptions(isBamlType, useV3Rules), 
			3532370792u => Create_BamlType_BitmapImage(isBamlType, useV3Rules), 
			3538186783u => Create_BamlType_Binding(isBamlType, useV3Rules), 
			3544056666u => Create_BamlType_RectAnimation(isBamlType, useV3Rules), 
			3545069055u => Create_BamlType_FontWeight(isBamlType, useV3Rules), 
			3545729620u => Create_BamlType_KeySplineConverter(isBamlType, useV3Rules), 
			3551608724u => Create_BamlType_Int32Converter(isBamlType, useV3Rules), 
			3557950823u => Create_BamlType_VisualTarget(isBamlType, useV3Rules), 
			3564716316u => Create_BamlType_RangeBase(isBamlType, useV3Rules), 
			3564745150u => Create_BamlType_CharConverter(isBamlType, useV3Rules), 
			3567044273u => Create_BamlType_MatrixAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3574070525u => Create_BamlType_RepeatButton(isBamlType, useV3Rules), 
			3578060752u => Create_BamlType_RoutedUICommand(isBamlType, useV3Rules), 
			3580418462u => Create_BamlType_Point4DConverter(isBamlType, useV3Rules), 
			3581886304u => Create_BamlType_PolyBezierSegment(isBamlType, useV3Rules), 
			3594131348u => Create_BamlType_BmpBitmapEncoder(isBamlType, useV3Rules), 
			3597588278u => Create_BamlType_StringAnimationBase(isBamlType, useV3Rules), 
			3603120821u => Create_BamlType_TemplateKey(isBamlType, useV3Rules), 
			3610917888u => Create_BamlType_FlowDocumentScrollViewer(isBamlType, useV3Rules), 
			3613077086u => Create_BamlType_GeneralTransform(isBamlType, useV3Rules), 
			3626508971u => Create_BamlType_InputBinding(isBamlType, useV3Rules), 
			3626720937u => Create_BamlType_TableRowGroup(isBamlType, useV3Rules), 
			3627100744u => Create_BamlType_AnimationClock(isBamlType, useV3Rules), 
			3633058264u => Create_BamlType_Drawing(isBamlType, useV3Rules), 
			3638153921u => Create_BamlType_RelativeSource(isBamlType, useV3Rules), 
			3639115055u => Create_BamlType_BooleanAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3646628024u => Create_BamlType_Matrix3DConverter(isBamlType, useV3Rules), 
			3656924396u => Create_BamlType_PathSegment(isBamlType, useV3Rules), 
			3657564178u => Create_BamlType_TiffBitmapEncoder(isBamlType, useV3Rules), 
			3660631367u => Create_BamlType_TabPanel(isBamlType, useV3Rules), 
			3661012133u => Create_BamlType_LinearGradientBrush(isBamlType, useV3Rules), 
			3666191229u => Create_BamlType_Selector(isBamlType, useV3Rules), 
			3666411286u => Create_BamlType_SingleConverter(isBamlType, useV3Rules), 
			3670807738u => Create_BamlType_GradientBrush(isBamlType, useV3Rules), 
			3681601765u => Create_BamlType_GlyphRun(isBamlType, useV3Rules), 
			3692579028u => Create_BamlType_Application(isBamlType, useV3Rules), 
			3693227583u => Create_BamlType_WmpBitmapDecoder(isBamlType, useV3Rules), 
			3705322145u => Create_BamlType_DateTime(isBamlType, useV3Rules), 
			3707266540u => Create_BamlType_Int32Animation(isBamlType, useV3Rules), 
			3711361102u => Create_BamlType_Figure(isBamlType, useV3Rules), 
			3714572384u => Create_BamlType_Label(isBamlType, useV3Rules), 
			3722866108u => Create_BamlType_Light(isBamlType, useV3Rules), 
			3725053631u => Create_BamlType_EmbossBitmapEffect(isBamlType, useV3Rules), 
			3726867806u => Create_BamlType_FlowDocumentReader(isBamlType, useV3Rules), 
			3734175699u => Create_BamlType_PixelFormatConverter(isBamlType, useV3Rules), 
			3741909743u => Create_BamlType_FixedDocument(isBamlType, useV3Rules), 
			3743141867u => Create_BamlType_PointIListConverter(isBamlType, useV3Rules), 
			3746015879u => Create_BamlType_XamlWriter(isBamlType, useV3Rules), 
			3746078580u => Create_BamlType_RectAnimationUsingKeyFrames(isBamlType, useV3Rules), 
			3761318360u => Create_BamlType_ContentPropertyAttribute(isBamlType, useV3Rules), 
			3780531133u => Create_BamlType_TransformGroup(isBamlType, useV3Rules), 
			3797319853u => Create_BamlType_TextDecorationCollection(isBamlType, useV3Rules), 
			3800911244u => Create_BamlType_Vector3DCollectionConverter(isBamlType, useV3Rules), 
			3822069102u => Create_BamlType_SingleAnimation(isBamlType, useV3Rules), 
			3831772414u => Create_BamlType_Int32Rect(isBamlType, useV3Rules), 
			3850431872u => Create_BamlType_ScaleTransform(isBamlType, useV3Rules), 
			3862075430u => Create_BamlType_PointConverter(isBamlType, useV3Rules), 
			3870219055u => Create_BamlType_LostFocusEventManager(isBamlType, useV3Rules), 
			3870757565u => Create_BamlType_MatrixKeyFrameCollection(isBamlType, useV3Rules), 
			3894194634u => Create_BamlType_IAddChild(isBamlType, useV3Rules), 
			3894580134u => Create_BamlType_EllipseGeometry(isBamlType, useV3Rules), 
			3895289908u => Create_BamlType_AmbientLight(isBamlType, useV3Rules), 
			3903386330u => Create_BamlType_RelativeSourceMode(isBamlType, useV3Rules), 
			3908473888u => Create_BamlType_SoundPlayerAction(isBamlType, useV3Rules), 
			3908606180u => Create_BamlType_DrawingVisual(isBamlType, useV3Rules), 
			3921789478u => Create_BamlType_Vector3DCollection(isBamlType, useV3Rules), 
			3924959263u => Create_BamlType_Transform3D(isBamlType, useV3Rules), 
			3925684252u => Create_BamlType_Transform(isBamlType, useV3Rules), 
			3927457614u => Create_BamlType_QuadraticBezierSegment(isBamlType, useV3Rules), 
			3928766041u => Create_BamlType_DiscretePointKeyFrame(isBamlType, useV3Rules), 
			3938642252u => Create_BamlType_GridViewColumnHeader(isBamlType, useV3Rules), 
			3944256480u => Create_BamlType_NullExtension(isBamlType, useV3Rules), 
			3948871275u => Create_BamlType_Vector(isBamlType, useV3Rules), 
			3950543104u => Create_BamlType_ImageSourceConverter(isBamlType, useV3Rules), 
			3957573606u => Create_BamlType_LinearRotation3DKeyFrame(isBamlType, useV3Rules), 
			3963681416u => Create_BamlType_LinearInt64KeyFrame(isBamlType, useV3Rules), 
			3965893796u => Create_BamlType_DocumentReferenceCollection(isBamlType, useV3Rules), 
			3969230566u => Create_BamlType_SingleKeyFrame(isBamlType, useV3Rules), 
			3973477021u => Create_BamlType_Int64KeyFrame(isBamlType, useV3Rules), 
			3977525494u => Create_BamlType_NavigationUIVisibility(isBamlType, useV3Rules), 
			3981616693u => Create_BamlType_DiscreteSingleKeyFrame(isBamlType, useV3Rules), 
			3991721253u => Create_BamlType_RoutedEventConverter(isBamlType, useV3Rules), 
			4017733246u => Create_BamlType_PointAnimation(isBamlType, useV3Rules), 
			4029087036u => Create_BamlType_VirtualizingPanel(isBamlType, useV3Rules), 
			4029842000u => Create_BamlType_ImageSource(isBamlType, useV3Rules), 
			4034507500u => Create_BamlType_ByteKeyFrame(isBamlType, useV3Rules), 
			4035632042u => Create_BamlType_ObjectAnimationBase(isBamlType, useV3Rules), 
			4039072664u => Create_BamlType_XamlTemplateSerializer(isBamlType, useV3Rules), 
			4043614448u => Create_BamlType_CultureInfoIetfLanguageTagConverter(isBamlType, useV3Rules), 
			4048739983u => Create_BamlType_VectorCollectionConverter(isBamlType, useV3Rules), 
			4056415476u => Create_BamlType_BezierSegment(isBamlType, useV3Rules), 
			4056944842u => Create_BamlType_SpellCheck(isBamlType, useV3Rules), 
			4059589051u => Create_BamlType_String(isBamlType, useV3Rules), 
			4059695088u => Create_BamlType_BooleanKeyFrameCollection(isBamlType, useV3Rules), 
			4061092706u => Create_BamlType_TreeViewItem(isBamlType, useV3Rules), 
			4064409625u => Create_BamlType_FontFamilyConverter(isBamlType, useV3Rules), 
			4066832480u => Create_BamlType_Stylus(isBamlType, useV3Rules), 
			4070164174u => Create_BamlType_DependencyObject(isBamlType, useV3Rules), 
			4080336864u => Create_BamlType_GridLengthConverter(isBamlType, useV3Rules), 
			4083954870u => Create_BamlType_BitmapEffectCollection(isBamlType, useV3Rules), 
			4093700264u => Create_BamlType_GradientStop(isBamlType, useV3Rules), 
			4095303241u => Create_BamlType_Int64Converter(isBamlType, useV3Rules), 
			4099991372u => Create_BamlType_INameScope(isBamlType, useV3Rules), 
			4114749745u => Create_BamlType_UInt32Converter(isBamlType, useV3Rules), 
			4130936400u => Create_BamlType_Panel(isBamlType, useV3Rules), 
			4135277332u => Create_BamlType_Model3D(isBamlType, useV3Rules), 
			4138410030u => Create_BamlType_VirtualizingStackPanel(isBamlType, useV3Rules), 
			4145310526u => Create_BamlType_Point(isBamlType, useV3Rules), 
			4145382636u => Create_BamlType_Popup(isBamlType, useV3Rules), 
			4147170844u => Create_BamlType_MatrixAnimationUsingPath(isBamlType, useV3Rules), 
			4147517467u => Create_BamlType_InputManager(isBamlType, useV3Rules), 
			4156353754u => Create_BamlType_Duration(isBamlType, useV3Rules), 
			4156625073u => Create_BamlType_DataChangedEventManager(isBamlType, useV3Rules), 
			4199195260u => Create_BamlType_TransformCollection(isBamlType, useV3Rules), 
			4200653599u => Create_BamlType_DocumentPageView(isBamlType, useV3Rules), 
			4202675952u => Create_BamlType_TimelineGroup(isBamlType, useV3Rules), 
			4205340967u => Create_BamlType_PerspectiveCamera(isBamlType, useV3Rules), 
			4212267451u => Create_BamlType_AccessText(isBamlType, useV3Rules), 
			4219821822u => Create_BamlType_RoutedCommand(isBamlType, useV3Rules), 
			4221592645u => Create_BamlType_SplineSingleKeyFrame(isBamlType, useV3Rules), 
			4227383631u => Create_BamlType_ImageBrush(isBamlType, useV3Rules), 
			4232579606u => Create_BamlType_Vector3D(isBamlType, useV3Rules), 
			4233318098u => Create_BamlType_StackPanel(isBamlType, useV3Rules), 
			4234029471u => Create_BamlType_Rotation3DKeyFrame(isBamlType, useV3Rules), 
			4239529341u => Create_BamlType_PriorityBinding(isBamlType, useV3Rules), 
			4243618870u => Create_BamlType_PointCollectionConverter(isBamlType, useV3Rules), 
			4250838544u => Create_BamlType_ArrayExtension(isBamlType, useV3Rules), 
			4250961057u => Create_BamlType_Int64Animation(isBamlType, useV3Rules), 
			4259355998u => Create_BamlType_DiscreteDecimalKeyFrame(isBamlType, useV3Rules), 
			4260680252u => Create_BamlType_VisualBrush(isBamlType, useV3Rules), 
			4265248728u => Create_BamlType_DynamicResourceExtensionConverter(isBamlType, useV3Rules), 
			4268703175u => Create_BamlType_VectorConverter(isBamlType, useV3Rules), 
			4291638393u => Create_BamlType_WeakEventManager(isBamlType, useV3Rules), 
			_ => null, 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AccessText(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 1, "AccessText", typeof(AccessText), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new AccessText();
		wpfKnownType.ContentPropertyName = "Text";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AdornedElementPlaceholder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 2, "AdornedElementPlaceholder", typeof(AdornedElementPlaceholder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new AdornedElementPlaceholder();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Adorner(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 3, "Adorner", typeof(Adorner), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AdornerDecorator(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 4, "AdornerDecorator", typeof(AdornerDecorator), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new AdornerDecorator();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AdornerLayer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 5, "AdornerLayer", typeof(AdornerLayer), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AffineTransform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 6, "AffineTransform3D", typeof(AffineTransform3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AmbientLight(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 7, "AmbientLight", typeof(AmbientLight), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new AmbientLight();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AnchoredBlock(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 8, "AnchoredBlock", typeof(AnchoredBlock), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Animatable(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 9, "Animatable", typeof(Animatable), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AnimationClock(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 10, "AnimationClock", typeof(AnimationClock), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AnimationTimeline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 11, "AnimationTimeline", typeof(AnimationTimeline), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Application(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 12, "Application", typeof(Application), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Application();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ArcSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 13, "ArcSegment", typeof(ArcSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ArcSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ArrayExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 14, "ArrayExtension", typeof(ArrayExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ArrayExtension();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(Type) }, (object[] arguments) => new ArrayExtension((Type)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_AxisAngleRotation3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 15, "AxisAngleRotation3D", typeof(AxisAngleRotation3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new AxisAngleRotation3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BaseIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 16, "BaseIListConverter", typeof(BaseIListConverter), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BeginStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 17, "BeginStoryboard", typeof(BeginStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BeginStoryboard();
		wpfKnownType.ContentPropertyName = "Storyboard";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BevelBitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 18, "BevelBitmapEffect", typeof(BevelBitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BevelBitmapEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BezierSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 19, "BezierSegment", typeof(BezierSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BezierSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Binding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 20, "Binding", typeof(Binding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Binding();
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(string) }, (object[] arguments) => new Binding((string)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BindingBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 21, "BindingBase", typeof(BindingBase), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BindingExpression(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 22, "BindingExpression", typeof(BindingExpression), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BindingExpressionBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 23, "BindingExpressionBase", typeof(BindingExpressionBase), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BindingListCollectionView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 24, "BindingListCollectionView", typeof(BindingListCollectionView), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 25, "BitmapDecoder", typeof(BitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 26, "BitmapEffect", typeof(BitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapEffectCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 27, "BitmapEffectCollection", typeof(BitmapEffectCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BitmapEffectCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapEffectGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 28, "BitmapEffectGroup", typeof(BitmapEffectGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BitmapEffectGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapEffectInput(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 29, "BitmapEffectInput", typeof(BitmapEffectInput), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BitmapEffectInput();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 30, "BitmapEncoder", typeof(BitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 31, "BitmapFrame", typeof(BitmapFrame), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapImage(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 32, "BitmapImage", typeof(BitmapImage), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BitmapImage();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapMetadata(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 33, "BitmapMetadata", typeof(BitmapMetadata), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapPalette(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 34, "BitmapPalette", typeof(BitmapPalette), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BitmapSource(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 35, "BitmapSource", typeof(BitmapSource), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Block(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 36, "Block", typeof(Block), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BlockUIContainer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 37, "BlockUIContainer", typeof(BlockUIContainer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BlockUIContainer();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BlurBitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 38, "BlurBitmapEffect", typeof(BlurBitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BlurBitmapEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BmpBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 39, "BmpBitmapDecoder", typeof(BmpBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BmpBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 40, "BmpBitmapEncoder", typeof(BmpBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BmpBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Bold(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 41, "Bold", typeof(Bold), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Bold();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BoolIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 42, "BoolIListConverter", typeof(BoolIListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BoolIListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Boolean(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 43, "Boolean", typeof(bool), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => false;
		wpfKnownType.TypeConverterType = typeof(BooleanConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 44, "BooleanAnimationBase", typeof(BooleanAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 45, "BooleanAnimationUsingKeyFrames", typeof(BooleanAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BooleanAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 46, "BooleanConverter", typeof(BooleanConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BooleanConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 47, "BooleanKeyFrame", typeof(BooleanKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 48, "BooleanKeyFrameCollection", typeof(BooleanKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BooleanKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BooleanToVisibilityConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 49, "BooleanToVisibilityConverter", typeof(BooleanToVisibilityConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BooleanToVisibilityConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Border(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 50, "Border", typeof(Border), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Border();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BorderGapMaskConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 51, "BorderGapMaskConverter", typeof(BorderGapMaskConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BorderGapMaskConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Brush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 52, "Brush", typeof(Brush), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BrushConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 53, "BrushConverter", typeof(BrushConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BrushConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_BulletDecorator(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 54, "BulletDecorator", typeof(BulletDecorator), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new BulletDecorator();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Button(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 55, "Button", typeof(Button), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Button();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ButtonBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 56, "ButtonBase", typeof(ButtonBase), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Byte(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 57, "Byte", typeof(byte), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => (byte)0;
		wpfKnownType.TypeConverterType = typeof(ByteConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 58, "ByteAnimation", typeof(ByteAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ByteAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 59, "ByteAnimationBase", typeof(ByteAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 60, "ByteAnimationUsingKeyFrames", typeof(ByteAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ByteAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 61, "ByteConverter", typeof(ByteConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ByteConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 62, "ByteKeyFrame", typeof(ByteKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ByteKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 63, "ByteKeyFrameCollection", typeof(ByteKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ByteKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CachedBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 64, "CachedBitmap", typeof(CachedBitmap), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Camera(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 65, "Camera", typeof(Camera), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Canvas(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 66, "Canvas", typeof(Canvas), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Canvas();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Char(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 67, "Char", typeof(char), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => '\0';
		wpfKnownType.TypeConverterType = typeof(CharConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 68, "CharAnimationBase", typeof(CharAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 69, "CharAnimationUsingKeyFrames", typeof(CharAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CharAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 70, "CharConverter", typeof(CharConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CharConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 71, "CharIListConverter", typeof(CharIListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CharIListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 72, "CharKeyFrame", typeof(CharKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CharKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 73, "CharKeyFrameCollection", typeof(CharKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CharKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CheckBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 74, "CheckBox", typeof(CheckBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CheckBox();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Clock(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 75, "Clock", typeof(Clock), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ClockController(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 76, "ClockController", typeof(ClockController), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ClockGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 77, "ClockGroup", typeof(ClockGroup), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CollectionContainer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 78, "CollectionContainer", typeof(CollectionContainer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CollectionContainer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CollectionView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 79, "CollectionView", typeof(CollectionView), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CollectionViewSource(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 80, "CollectionViewSource", typeof(CollectionViewSource), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CollectionViewSource();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Color(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 81, "Color", typeof(Color), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Color);
		wpfKnownType.TypeConverterType = typeof(ColorConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 82, "ColorAnimation", typeof(ColorAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 83, "ColorAnimationBase", typeof(ColorAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 84, "ColorAnimationUsingKeyFrames", typeof(ColorAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorConvertedBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 85, "ColorConvertedBitmap", typeof(ColorConvertedBitmap), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorConvertedBitmap();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorConvertedBitmapExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 86, "ColorConvertedBitmapExtension", typeof(ColorConvertedBitmapExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorConvertedBitmapExtension();
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(object) }, (object[] arguments) => new ColorConvertedBitmapExtension(arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 87, "ColorConverter", typeof(ColorConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 88, "ColorKeyFrame", typeof(ColorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColorKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 89, "ColorKeyFrameCollection", typeof(ColorKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColorKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColumnDefinition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 90, "ColumnDefinition", typeof(ColumnDefinition), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ColumnDefinition();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CombinedGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 91, "CombinedGeometry", typeof(CombinedGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CombinedGeometry();
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ComboBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 92, "ComboBox", typeof(ComboBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ComboBox();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ComboBoxItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 93, "ComboBoxItem", typeof(ComboBoxItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ComboBoxItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CommandConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 94, "CommandConverter", typeof(CommandConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CommandConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ComponentResourceKey(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 95, "ComponentResourceKey", typeof(ComponentResourceKey), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ComponentResourceKey();
		wpfKnownType.TypeConverterType = typeof(ComponentResourceKeyConverter);
		wpfKnownType.Constructors.Add(2, new Baml6ConstructorInfo(new List<Type>
		{
			typeof(Type),
			typeof(object)
		}, (object[] arguments) => new ComponentResourceKey((Type)arguments[0], arguments[1])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ComponentResourceKeyConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 96, "ComponentResourceKeyConverter", typeof(ComponentResourceKeyConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ComponentResourceKeyConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CompositionTarget(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 97, "CompositionTarget", typeof(CompositionTarget), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Condition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 98, "Condition", typeof(Condition), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Condition();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContainerVisual(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 99, "ContainerVisual", typeof(ContainerVisual), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContainerVisual();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContentControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 100, "ContentControl", typeof(ContentControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContentControl();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContentElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 101, "ContentElement", typeof(ContentElement), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContentElement();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContentPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 102, "ContentPresenter", typeof(ContentPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContentPresenter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContentPropertyAttribute(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 103, "ContentPropertyAttribute", typeof(ContentPropertyAttribute), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContentPropertyAttribute();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContentWrapperAttribute(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 104, "ContentWrapperAttribute", typeof(ContentWrapperAttribute), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContextMenu(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 105, "ContextMenu", typeof(ContextMenu), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ContextMenu();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ContextMenuService(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 106, "ContextMenuService", typeof(ContextMenuService), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Control(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 107, "Control", typeof(Control), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Control();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ControlTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 108, "ControlTemplate", typeof(ControlTemplate), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ControlTemplate();
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.DictionaryKeyPropertyName = "TargetType";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ControllableStoryboardAction(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 109, "ControllableStoryboardAction", typeof(ControllableStoryboardAction), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CornerRadius(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 110, "CornerRadius", typeof(CornerRadius), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(CornerRadius);
		wpfKnownType.TypeConverterType = typeof(CornerRadiusConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CornerRadiusConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 111, "CornerRadiusConverter", typeof(CornerRadiusConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CornerRadiusConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CroppedBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 112, "CroppedBitmap", typeof(CroppedBitmap), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CroppedBitmap();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CultureInfo(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 113, "CultureInfo", typeof(CultureInfo), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(CultureInfoConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CultureInfoConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 114, "CultureInfoConverter", typeof(CultureInfoConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CultureInfoConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CultureInfoIetfLanguageTagConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 115, "CultureInfoIetfLanguageTagConverter", typeof(CultureInfoIetfLanguageTagConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CultureInfoIetfLanguageTagConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Cursor(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 116, "Cursor", typeof(Cursor), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(CursorConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CursorConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 117, "CursorConverter", typeof(CursorConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CursorConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DashStyle(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 118, "DashStyle", typeof(DashStyle), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DashStyle();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DataChangedEventManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 119, "DataChangedEventManager", typeof(DataChangedEventManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DataTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 120, "DataTemplate", typeof(DataTemplate), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DataTemplate();
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.DictionaryKeyPropertyName = "DataTemplateKey";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DataTemplateKey(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 121, "DataTemplateKey", typeof(DataTemplateKey), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DataTemplateKey();
		wpfKnownType.TypeConverterType = typeof(TemplateKeyConverter);
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(object) }, (object[] arguments) => new DataTemplateKey(arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DataTrigger(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 122, "DataTrigger", typeof(DataTrigger), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DataTrigger();
		wpfKnownType.ContentPropertyName = "Setters";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DateTime(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 123, "DateTime", typeof(DateTime), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(DateTime);
		wpfKnownType.HasSpecialValueConverter = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DateTimeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 124, "DateTimeConverter", typeof(DateTimeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DateTimeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DateTimeConverter2(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 125, "DateTimeConverter2", typeof(DateTimeConverter2), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DateTimeConverter2();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Decimal(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 126, "Decimal", typeof(decimal), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0m;
		wpfKnownType.TypeConverterType = typeof(DecimalConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 127, "DecimalAnimation", typeof(DecimalAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DecimalAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 128, "DecimalAnimationBase", typeof(DecimalAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 129, "DecimalAnimationUsingKeyFrames", typeof(DecimalAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DecimalAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 130, "DecimalConverter", typeof(DecimalConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DecimalConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 131, "DecimalKeyFrame", typeof(DecimalKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DecimalKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 132, "DecimalKeyFrameCollection", typeof(DecimalKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DecimalKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Decorator(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 133, "Decorator", typeof(Decorator), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Decorator();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DefinitionBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 134, "DefinitionBase", typeof(DefinitionBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DependencyObject(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 135, "DependencyObject", typeof(DependencyObject), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DependencyObject();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DependencyProperty(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 136, "DependencyProperty", typeof(DependencyProperty), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(DependencyPropertyConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DependencyPropertyConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 137, "DependencyPropertyConverter", typeof(DependencyPropertyConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DependencyPropertyConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DialogResultConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 138, "DialogResultConverter", typeof(DialogResultConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DialogResultConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiffuseMaterial(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 139, "DiffuseMaterial", typeof(DiffuseMaterial), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiffuseMaterial();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DirectionalLight(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 140, "DirectionalLight", typeof(DirectionalLight), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DirectionalLight();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteBooleanKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 141, "DiscreteBooleanKeyFrame", typeof(DiscreteBooleanKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteBooleanKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteByteKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 142, "DiscreteByteKeyFrame", typeof(DiscreteByteKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteByteKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteCharKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 143, "DiscreteCharKeyFrame", typeof(DiscreteCharKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteCharKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteColorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 144, "DiscreteColorKeyFrame", typeof(DiscreteColorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteColorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteDecimalKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 145, "DiscreteDecimalKeyFrame", typeof(DiscreteDecimalKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteDecimalKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteDoubleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 146, "DiscreteDoubleKeyFrame", typeof(DiscreteDoubleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteDoubleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteInt16KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 147, "DiscreteInt16KeyFrame", typeof(DiscreteInt16KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteInt16KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteInt32KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 148, "DiscreteInt32KeyFrame", typeof(DiscreteInt32KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteInt32KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteInt64KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 149, "DiscreteInt64KeyFrame", typeof(DiscreteInt64KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteInt64KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteMatrixKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 150, "DiscreteMatrixKeyFrame", typeof(DiscreteMatrixKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteMatrixKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteObjectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 151, "DiscreteObjectKeyFrame", typeof(DiscreteObjectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteObjectKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscretePoint3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 152, "DiscretePoint3DKeyFrame", typeof(DiscretePoint3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscretePoint3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscretePointKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 153, "DiscretePointKeyFrame", typeof(DiscretePointKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscretePointKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteQuaternionKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 154, "DiscreteQuaternionKeyFrame", typeof(DiscreteQuaternionKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteQuaternionKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteRectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 155, "DiscreteRectKeyFrame", typeof(DiscreteRectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteRectKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteRotation3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 156, "DiscreteRotation3DKeyFrame", typeof(DiscreteRotation3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteRotation3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteSingleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 157, "DiscreteSingleKeyFrame", typeof(DiscreteSingleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteSingleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteSizeKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 158, "DiscreteSizeKeyFrame", typeof(DiscreteSizeKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteSizeKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteStringKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 159, "DiscreteStringKeyFrame", typeof(DiscreteStringKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteStringKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteThicknessKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 160, "DiscreteThicknessKeyFrame", typeof(DiscreteThicknessKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteThicknessKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteVector3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 161, "DiscreteVector3DKeyFrame", typeof(DiscreteVector3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteVector3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DiscreteVectorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 162, "DiscreteVectorKeyFrame", typeof(DiscreteVectorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DiscreteVectorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DockPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 163, "DockPanel", typeof(DockPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DockPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DocumentPageView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 164, "DocumentPageView", typeof(DocumentPageView), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DocumentPageView();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DocumentReference(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 165, "DocumentReference", typeof(DocumentReference), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DocumentReference();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DocumentViewer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 166, "DocumentViewer", typeof(DocumentViewer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DocumentViewer();
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DocumentViewerBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 167, "DocumentViewerBase", typeof(DocumentViewerBase), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Double(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 168, "Double", typeof(double), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0.0;
		wpfKnownType.TypeConverterType = typeof(DoubleConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 169, "DoubleAnimation", typeof(DoubleAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 170, "DoubleAnimationBase", typeof(DoubleAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 171, "DoubleAnimationUsingKeyFrames", typeof(DoubleAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleAnimationUsingPath(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 172, "DoubleAnimationUsingPath", typeof(DoubleAnimationUsingPath), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleAnimationUsingPath();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 173, "DoubleCollection", typeof(DoubleCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleCollection();
		wpfKnownType.TypeConverterType = typeof(DoubleCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 174, "DoubleCollectionConverter", typeof(DoubleCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 175, "DoubleConverter", typeof(DoubleConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 176, "DoubleIListConverter", typeof(DoubleIListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleIListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 177, "DoubleKeyFrame", typeof(DoubleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DoubleKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 178, "DoubleKeyFrameCollection", typeof(DoubleKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DoubleKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Drawing(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 179, "Drawing", typeof(System.Windows.Media.Drawing), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 180, "DrawingBrush", typeof(DrawingBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DrawingBrush();
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 181, "DrawingCollection", typeof(DrawingCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DrawingCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingContext(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 182, "DrawingContext", typeof(DrawingContext), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 183, "DrawingGroup", typeof(DrawingGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DrawingGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingImage(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 184, "DrawingImage", typeof(DrawingImage), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DrawingImage();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DrawingVisual(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 185, "DrawingVisual", typeof(DrawingVisual), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DrawingVisual();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DropShadowBitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 186, "DropShadowBitmapEffect", typeof(DropShadowBitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DropShadowBitmapEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Duration(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 187, "Duration", typeof(Duration), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Duration);
		wpfKnownType.TypeConverterType = typeof(DurationConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DurationConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 188, "DurationConverter", typeof(DurationConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DurationConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DynamicResourceExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 189, "DynamicResourceExtension", typeof(DynamicResourceExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DynamicResourceExtension();
		wpfKnownType.TypeConverterType = typeof(DynamicResourceExtensionConverter);
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(object) }, (object[] arguments) => new DynamicResourceExtension(arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DynamicResourceExtensionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 190, "DynamicResourceExtensionConverter", typeof(DynamicResourceExtensionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new DynamicResourceExtensionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Ellipse(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 191, "Ellipse", typeof(Ellipse), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Ellipse();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EllipseGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 192, "EllipseGeometry", typeof(EllipseGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new EllipseGeometry();
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EmbossBitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 193, "EmbossBitmapEffect", typeof(EmbossBitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new EmbossBitmapEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EmissiveMaterial(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 194, "EmissiveMaterial", typeof(EmissiveMaterial), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new EmissiveMaterial();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EnumConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 195, "EnumConverter", typeof(EnumConverter), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EventManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 196, "EventManager", typeof(EventManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EventSetter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 197, "EventSetter", typeof(EventSetter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new EventSetter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_EventTrigger(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 198, "EventTrigger", typeof(EventTrigger), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new EventTrigger();
		wpfKnownType.ContentPropertyName = "Actions";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Expander(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 199, "Expander", typeof(Expander), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Expander();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Expression(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 200, "Expression", typeof(Expression), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ExpressionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 201, "ExpressionConverter", typeof(ExpressionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ExpressionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Figure(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 202, "Figure", typeof(Figure), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Figure();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FigureLength(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 203, "FigureLength", typeof(FigureLength), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(FigureLength);
		wpfKnownType.TypeConverterType = typeof(FigureLengthConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FigureLengthConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 204, "FigureLengthConverter", typeof(FigureLengthConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FigureLengthConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FixedDocument(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 205, "FixedDocument", typeof(FixedDocument), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FixedDocument();
		wpfKnownType.ContentPropertyName = "Pages";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FixedDocumentSequence(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 206, "FixedDocumentSequence", typeof(FixedDocumentSequence), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FixedDocumentSequence();
		wpfKnownType.ContentPropertyName = "References";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FixedPage(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 207, "FixedPage", typeof(FixedPage), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FixedPage();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Floater(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 208, "Floater", typeof(Floater), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Floater();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FlowDocument(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 209, "FlowDocument", typeof(FlowDocument), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FlowDocument();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FlowDocumentPageViewer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 210, "FlowDocumentPageViewer", typeof(FlowDocumentPageViewer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FlowDocumentPageViewer();
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FlowDocumentReader(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 211, "FlowDocumentReader", typeof(FlowDocumentReader), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FlowDocumentReader();
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FlowDocumentScrollViewer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 212, "FlowDocumentScrollViewer", typeof(FlowDocumentScrollViewer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FlowDocumentScrollViewer();
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FocusManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 213, "FocusManager", typeof(FocusManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontFamily(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 214, "FontFamily", typeof(FontFamily), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontFamily();
		wpfKnownType.TypeConverterType = typeof(FontFamilyConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontFamilyConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 215, "FontFamilyConverter", typeof(FontFamilyConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontFamilyConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontSizeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 216, "FontSizeConverter", typeof(FontSizeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontSizeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontStretch(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 217, "FontStretch", typeof(FontStretch), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(FontStretch);
		wpfKnownType.TypeConverterType = typeof(FontStretchConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontStretchConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 218, "FontStretchConverter", typeof(FontStretchConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontStretchConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontStyle(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 219, "FontStyle", typeof(FontStyle), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(FontStyle);
		wpfKnownType.TypeConverterType = typeof(FontStyleConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontStyleConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 220, "FontStyleConverter", typeof(FontStyleConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontStyleConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontWeight(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 221, "FontWeight", typeof(FontWeight), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(FontWeight);
		wpfKnownType.TypeConverterType = typeof(FontWeightConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FontWeightConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 222, "FontWeightConverter", typeof(FontWeightConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FontWeightConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FormatConvertedBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 223, "FormatConvertedBitmap", typeof(FormatConvertedBitmap), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FormatConvertedBitmap();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Frame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 224, "Frame", typeof(Frame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Frame();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkContentElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 225, "FrameworkContentElement", typeof(FrameworkContentElement), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FrameworkContentElement();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 226, "FrameworkElement", typeof(FrameworkElement), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FrameworkElement();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkElementFactory(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 227, "FrameworkElementFactory", typeof(FrameworkElementFactory), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FrameworkElementFactory();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkPropertyMetadata(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 228, "FrameworkPropertyMetadata", typeof(FrameworkPropertyMetadata), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new FrameworkPropertyMetadata();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkPropertyMetadataOptions(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 229, "FrameworkPropertyMetadataOptions", typeof(FrameworkPropertyMetadataOptions), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => FrameworkPropertyMetadataOptions.None;
		wpfKnownType.TypeConverterType = typeof(FrameworkPropertyMetadataOptions);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkRichTextComposition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 230, "FrameworkRichTextComposition", typeof(FrameworkRichTextComposition), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 231, "FrameworkTemplate", typeof(FrameworkTemplate), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_FrameworkTextComposition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 232, "FrameworkTextComposition", typeof(FrameworkTextComposition), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Freezable(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 233, "Freezable", typeof(Freezable), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeneralTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 234, "GeneralTransform", typeof(GeneralTransform), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeneralTransformCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 235, "GeneralTransformCollection", typeof(GeneralTransformCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeneralTransformCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeneralTransformGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 236, "GeneralTransformGroup", typeof(GeneralTransformGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeneralTransformGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Geometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 237, "Geometry", typeof(Geometry), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Geometry3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 238, "Geometry3D", typeof(Geometry3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeometryCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 239, "GeometryCollection", typeof(GeometryCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeometryCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeometryConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 240, "GeometryConverter", typeof(GeometryConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeometryConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeometryDrawing(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 241, "GeometryDrawing", typeof(GeometryDrawing), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeometryDrawing();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeometryGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 242, "GeometryGroup", typeof(GeometryGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeometryGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GeometryModel3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 243, "GeometryModel3D", typeof(GeometryModel3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GeometryModel3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GestureRecognizer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 244, "GestureRecognizer", typeof(GestureRecognizer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GestureRecognizer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GifBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 245, "GifBitmapDecoder", typeof(GifBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GifBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 246, "GifBitmapEncoder", typeof(GifBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GifBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GlyphRun(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 247, "GlyphRun", typeof(GlyphRun), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GlyphRun();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GlyphRunDrawing(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 248, "GlyphRunDrawing", typeof(GlyphRunDrawing), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GlyphRunDrawing();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GlyphTypeface(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 249, "GlyphTypeface", typeof(GlyphTypeface), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GlyphTypeface();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Glyphs(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 250, "Glyphs", typeof(Glyphs), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Glyphs();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GradientBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 251, "GradientBrush", typeof(GradientBrush), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "GradientStops";
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GradientStop(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 252, "GradientStop", typeof(GradientStop), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GradientStop();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GradientStopCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 253, "GradientStopCollection", typeof(GradientStopCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GradientStopCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Grid(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 254, "Grid", typeof(Grid), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Grid();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridLength(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 255, "GridLength", typeof(GridLength), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(GridLength);
		wpfKnownType.TypeConverterType = typeof(GridLengthConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridLengthConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 256, "GridLengthConverter", typeof(GridLengthConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridLengthConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridSplitter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 257, "GridSplitter", typeof(GridSplitter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridSplitter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 258, "GridView", typeof(GridView), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridView();
		wpfKnownType.ContentPropertyName = "Columns";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridViewColumn(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 259, "GridViewColumn", typeof(GridViewColumn), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridViewColumn();
		wpfKnownType.ContentPropertyName = "Header";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridViewColumnHeader(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 260, "GridViewColumnHeader", typeof(GridViewColumnHeader), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridViewColumnHeader();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridViewHeaderRowPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 261, "GridViewHeaderRowPresenter", typeof(GridViewHeaderRowPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridViewHeaderRowPresenter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridViewRowPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 262, "GridViewRowPresenter", typeof(GridViewRowPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GridViewRowPresenter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GridViewRowPresenterBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 263, "GridViewRowPresenterBase", typeof(GridViewRowPresenterBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GroupBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 264, "GroupBox", typeof(GroupBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GroupBox();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GroupItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 265, "GroupItem", typeof(GroupItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GroupItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Guid(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 266, "Guid", typeof(Guid), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Guid);
		wpfKnownType.TypeConverterType = typeof(GuidConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GuidConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 267, "GuidConverter", typeof(GuidConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GuidConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_GuidelineSet(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 268, "GuidelineSet", typeof(GuidelineSet), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new GuidelineSet();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_HeaderedContentControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 269, "HeaderedContentControl", typeof(HeaderedContentControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new HeaderedContentControl();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_HeaderedItemsControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 270, "HeaderedItemsControl", typeof(HeaderedItemsControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new HeaderedItemsControl();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_HierarchicalDataTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 271, "HierarchicalDataTemplate", typeof(HierarchicalDataTemplate), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new HierarchicalDataTemplate();
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.DictionaryKeyPropertyName = "DataTemplateKey";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_HostVisual(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 272, "HostVisual", typeof(HostVisual), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new HostVisual();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Hyperlink(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 273, "Hyperlink", typeof(Hyperlink), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Hyperlink();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_IAddChild(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 274, "IAddChild", typeof(IAddChild), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_IAddChildInternal(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 275, "IAddChildInternal", typeof(IAddChildInternal), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ICommand(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 276, "ICommand", typeof(ICommand), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(CommandConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_IComponentConnector(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 277, "IComponentConnector", typeof(IComponentConnector), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_INameScope(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 278, "INameScope", typeof(INameScope), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_IStyleConnector(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 279, "IStyleConnector", typeof(IStyleConnector), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_IconBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 280, "IconBitmapDecoder", typeof(IconBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Image(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 281, "Image", typeof(Image), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Image();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ImageBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 282, "ImageBrush", typeof(ImageBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ImageBrush();
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ImageDrawing(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 283, "ImageDrawing", typeof(ImageDrawing), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ImageDrawing();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ImageMetadata(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 284, "ImageMetadata", typeof(ImageMetadata), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ImageSource(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 285, "ImageSource", typeof(ImageSource), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ImageSourceConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 286, "ImageSourceConverter", typeof(ImageSourceConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ImageSourceConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InPlaceBitmapMetadataWriter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 287, "InPlaceBitmapMetadataWriter", typeof(InPlaceBitmapMetadataWriter), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InkCanvas(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 288, "InkCanvas", typeof(InkCanvas), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InkCanvas();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InkPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 289, "InkPresenter", typeof(InkPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InkPresenter();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Inline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 290, "Inline", typeof(Inline), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InlineCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 291, "InlineCollection", typeof(InlineCollection), isBamlType, useV3Rules);
		wpfKnownType.WhitespaceSignificantCollection = true;
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InlineUIContainer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 292, "InlineUIContainer", typeof(InlineUIContainer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InlineUIContainer();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 293, "InputBinding", typeof(InputBinding), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputDevice(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 294, "InputDevice", typeof(InputDevice), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputLanguageManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 295, "InputLanguageManager", typeof(InputLanguageManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 296, "InputManager", typeof(InputManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputMethod(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 297, "InputMethod", typeof(InputMethod), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputScope(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 298, "InputScope", typeof(InputScope), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InputScope();
		wpfKnownType.TypeConverterType = typeof(InputScopeConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputScopeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 299, "InputScopeConverter", typeof(InputScopeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InputScopeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputScopeName(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 300, "InputScopeName", typeof(InputScopeName), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InputScopeName();
		wpfKnownType.ContentPropertyName = "NameValue";
		wpfKnownType.TypeConverterType = typeof(InputScopeNameConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputScopeNameConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 301, "InputScopeNameConverter", typeof(InputScopeNameConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InputScopeNameConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 302, "Int16", typeof(short), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => (short)0;
		wpfKnownType.TypeConverterType = typeof(Int16Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16Animation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 303, "Int16Animation", typeof(Int16Animation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int16Animation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16AnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 304, "Int16AnimationBase", typeof(Int16AnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16AnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 305, "Int16AnimationUsingKeyFrames", typeof(Int16AnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int16AnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 306, "Int16Converter", typeof(Int16Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int16Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 307, "Int16KeyFrame", typeof(Int16KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int16KeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 308, "Int16KeyFrameCollection", typeof(Int16KeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int16KeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 309, "Int32", typeof(int), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0;
		wpfKnownType.TypeConverterType = typeof(Int32Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32Animation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 310, "Int32Animation", typeof(Int32Animation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32Animation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32AnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 311, "Int32AnimationBase", typeof(Int32AnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32AnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 312, "Int32AnimationUsingKeyFrames", typeof(Int32AnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32AnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32Collection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 313, "Int32Collection", typeof(Int32Collection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32Collection();
		wpfKnownType.TypeConverterType = typeof(Int32CollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32CollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 314, "Int32CollectionConverter", typeof(Int32CollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32CollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 315, "Int32Converter", typeof(Int32Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 316, "Int32KeyFrame", typeof(Int32KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32KeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 317, "Int32KeyFrameCollection", typeof(Int32KeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32KeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32Rect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 318, "Int32Rect", typeof(Int32Rect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Int32Rect);
		wpfKnownType.TypeConverterType = typeof(Int32RectConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int32RectConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 319, "Int32RectConverter", typeof(Int32RectConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int32RectConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 320, "Int64", typeof(long), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0L;
		wpfKnownType.TypeConverterType = typeof(Int64Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64Animation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 321, "Int64Animation", typeof(Int64Animation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int64Animation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64AnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 322, "Int64AnimationBase", typeof(Int64AnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64AnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 323, "Int64AnimationUsingKeyFrames", typeof(Int64AnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int64AnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 324, "Int64Converter", typeof(Int64Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int64Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 325, "Int64KeyFrame", typeof(Int64KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Int64KeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 326, "Int64KeyFrameCollection", typeof(Int64KeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Int64KeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Italic(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 327, "Italic", typeof(Italic), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Italic();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 328, "ItemCollection", typeof(ItemCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemsControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 329, "ItemsControl", typeof(ItemsControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ItemsControl();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemsPanelTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 330, "ItemsPanelTemplate", typeof(ItemsPanelTemplate), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ItemsPanelTemplate();
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemsPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 331, "ItemsPresenter", typeof(ItemsPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ItemsPresenter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JournalEntry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 332, "JournalEntry", typeof(JournalEntry), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JournalEntryListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 333, "JournalEntryListConverter", typeof(JournalEntryListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new JournalEntryListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JournalEntryUnifiedViewConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 334, "JournalEntryUnifiedViewConverter", typeof(JournalEntryUnifiedViewConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new JournalEntryUnifiedViewConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JpegBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 335, "JpegBitmapDecoder", typeof(JpegBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JpegBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 336, "JpegBitmapEncoder", typeof(JpegBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new JpegBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 337, "KeyBinding", typeof(KeyBinding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeyBinding();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 338, "KeyConverter", typeof(KeyConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeyConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyGesture(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 339, "KeyGesture", typeof(KeyGesture), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(KeyGestureConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyGestureConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 340, "KeyGestureConverter", typeof(KeyGestureConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeyGestureConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeySpline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 341, "KeySpline", typeof(KeySpline), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeySpline();
		wpfKnownType.TypeConverterType = typeof(KeySplineConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeySplineConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 342, "KeySplineConverter", typeof(KeySplineConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeySplineConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyTime(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 343, "KeyTime", typeof(KeyTime), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(KeyTime);
		wpfKnownType.TypeConverterType = typeof(KeyTimeConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyTimeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 344, "KeyTimeConverter", typeof(KeyTimeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new KeyTimeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyboardDevice(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 345, "KeyboardDevice", typeof(KeyboardDevice), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Label(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 346, "Label", typeof(Label), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Label();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LateBoundBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 347, "LateBoundBitmapDecoder", typeof(LateBoundBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LengthConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 348, "LengthConverter", typeof(LengthConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LengthConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Light(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 349, "Light", typeof(Light), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Line(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 350, "Line", typeof(Line), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Line();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LineBreak(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 351, "LineBreak", typeof(LineBreak), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LineBreak();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LineGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 352, "LineGeometry", typeof(LineGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LineGeometry();
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LineSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 353, "LineSegment", typeof(LineSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LineSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearByteKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 354, "LinearByteKeyFrame", typeof(LinearByteKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearByteKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearColorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 355, "LinearColorKeyFrame", typeof(LinearColorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearColorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearDecimalKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 356, "LinearDecimalKeyFrame", typeof(LinearDecimalKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearDecimalKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearDoubleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 357, "LinearDoubleKeyFrame", typeof(LinearDoubleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearDoubleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearGradientBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 358, "LinearGradientBrush", typeof(LinearGradientBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearGradientBrush();
		wpfKnownType.ContentPropertyName = "GradientStops";
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearInt16KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 359, "LinearInt16KeyFrame", typeof(LinearInt16KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearInt16KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearInt32KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 360, "LinearInt32KeyFrame", typeof(LinearInt32KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearInt32KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearInt64KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 361, "LinearInt64KeyFrame", typeof(LinearInt64KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearInt64KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearPoint3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 362, "LinearPoint3DKeyFrame", typeof(LinearPoint3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearPoint3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearPointKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 363, "LinearPointKeyFrame", typeof(LinearPointKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearPointKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearQuaternionKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 364, "LinearQuaternionKeyFrame", typeof(LinearQuaternionKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearQuaternionKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearRectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 365, "LinearRectKeyFrame", typeof(LinearRectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearRectKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearRotation3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 366, "LinearRotation3DKeyFrame", typeof(LinearRotation3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearRotation3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearSingleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 367, "LinearSingleKeyFrame", typeof(LinearSingleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearSingleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearSizeKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 368, "LinearSizeKeyFrame", typeof(LinearSizeKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearSizeKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearThicknessKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 369, "LinearThicknessKeyFrame", typeof(LinearThicknessKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearThicknessKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearVector3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 370, "LinearVector3DKeyFrame", typeof(LinearVector3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearVector3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LinearVectorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 371, "LinearVectorKeyFrame", typeof(LinearVectorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new LinearVectorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_List(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 372, "List", typeof(List), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new List();
		wpfKnownType.ContentPropertyName = "ListItems";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 373, "ListBox", typeof(ListBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ListBox();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListBoxItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 374, "ListBoxItem", typeof(ListBoxItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ListBoxItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListCollectionView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 375, "ListCollectionView", typeof(ListCollectionView), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 376, "ListItem", typeof(ListItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ListItem();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 377, "ListView", typeof(ListView), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ListView();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ListViewItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 378, "ListViewItem", typeof(ListViewItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ListViewItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Localization(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 379, "Localization", typeof(Localization), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_LostFocusEventManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 380, "LostFocusEventManager", typeof(LostFocusEventManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MarkupExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 381, "MarkupExtension", typeof(MarkupExtension), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Material(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 382, "Material", typeof(Material), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MaterialCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 383, "MaterialCollection", typeof(MaterialCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MaterialCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MaterialGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 384, "MaterialGroup", typeof(MaterialGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MaterialGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Matrix(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 385, "Matrix", typeof(Matrix), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Matrix);
		wpfKnownType.TypeConverterType = typeof(MatrixConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Matrix3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 386, "Matrix3D", typeof(Matrix3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Matrix3D);
		wpfKnownType.TypeConverterType = typeof(Matrix3DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Matrix3DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 387, "Matrix3DConverter", typeof(Matrix3DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Matrix3DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 388, "MatrixAnimationBase", typeof(MatrixAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 389, "MatrixAnimationUsingKeyFrames", typeof(MatrixAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixAnimationUsingPath(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 390, "MatrixAnimationUsingPath", typeof(MatrixAnimationUsingPath), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixAnimationUsingPath();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixCamera(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 391, "MatrixCamera", typeof(MatrixCamera), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixCamera();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 392, "MatrixConverter", typeof(MatrixConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 393, "MatrixKeyFrame", typeof(MatrixKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 394, "MatrixKeyFrameCollection", typeof(MatrixKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 395, "MatrixTransform", typeof(MatrixTransform), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixTransform();
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MatrixTransform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 396, "MatrixTransform3D", typeof(MatrixTransform3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MatrixTransform3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MediaClock(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 397, "MediaClock", typeof(MediaClock), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MediaElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 398, "MediaElement", typeof(MediaElement), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MediaElement();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MediaPlayer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 399, "MediaPlayer", typeof(MediaPlayer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MediaPlayer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MediaTimeline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 400, "MediaTimeline", typeof(MediaTimeline), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MediaTimeline();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Menu(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 401, "Menu", typeof(Menu), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Menu();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MenuBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 402, "MenuBase", typeof(MenuBase), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MenuItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 403, "MenuItem", typeof(MenuItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MenuItem();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MenuScrollingVisibilityConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 404, "MenuScrollingVisibilityConverter", typeof(MenuScrollingVisibilityConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MenuScrollingVisibilityConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MeshGeometry3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 405, "MeshGeometry3D", typeof(MeshGeometry3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MeshGeometry3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Model3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 406, "Model3D", typeof(Model3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Model3DCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 407, "Model3DCollection", typeof(Model3DCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Model3DCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Model3DGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 408, "Model3DGroup", typeof(Model3DGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Model3DGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ModelVisual3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 409, "ModelVisual3D", typeof(ModelVisual3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ModelVisual3D();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ModifierKeysConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 410, "ModifierKeysConverter", typeof(ModifierKeysConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ModifierKeysConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MouseActionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 411, "MouseActionConverter", typeof(MouseActionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MouseActionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MouseBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 412, "MouseBinding", typeof(MouseBinding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MouseBinding();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MouseDevice(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 413, "MouseDevice", typeof(MouseDevice), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MouseGesture(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 414, "MouseGesture", typeof(MouseGesture), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MouseGesture();
		wpfKnownType.TypeConverterType = typeof(MouseGestureConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MouseGestureConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 415, "MouseGestureConverter", typeof(MouseGestureConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MouseGestureConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MultiBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 416, "MultiBinding", typeof(MultiBinding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MultiBinding();
		wpfKnownType.ContentPropertyName = "Bindings";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MultiBindingExpression(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 417, "MultiBindingExpression", typeof(MultiBindingExpression), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MultiDataTrigger(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 418, "MultiDataTrigger", typeof(MultiDataTrigger), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MultiDataTrigger();
		wpfKnownType.ContentPropertyName = "Setters";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_MultiTrigger(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 419, "MultiTrigger", typeof(MultiTrigger), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MultiTrigger();
		wpfKnownType.ContentPropertyName = "Setters";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NameScope(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 420, "NameScope", typeof(NameScope), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new NameScope();
		wpfKnownType.CollectionKind = XamlCollectionKind.Dictionary;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NavigationWindow(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 421, "NavigationWindow", typeof(NavigationWindow), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new NavigationWindow();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NullExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 422, "NullExtension", typeof(NullExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new NullExtension();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NullableBoolConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 423, "NullableBoolConverter", typeof(NullableBoolConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new NullableBoolConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NullableConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 424, "NullableConverter", typeof(NullableConverter), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NumberSubstitution(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 425, "NumberSubstitution", typeof(NumberSubstitution), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new NumberSubstitution();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Object(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 426, "Object", typeof(object), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new object();
		wpfKnownType.HasSpecialValueConverter = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ObjectAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 427, "ObjectAnimationBase", typeof(ObjectAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ObjectAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 428, "ObjectAnimationUsingKeyFrames", typeof(ObjectAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ObjectAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ObjectDataProvider(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 429, "ObjectDataProvider", typeof(ObjectDataProvider), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ObjectDataProvider();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ObjectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 430, "ObjectKeyFrame", typeof(ObjectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ObjectKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 431, "ObjectKeyFrameCollection", typeof(ObjectKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ObjectKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_OrthographicCamera(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 432, "OrthographicCamera", typeof(OrthographicCamera), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new OrthographicCamera();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_OuterGlowBitmapEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 433, "OuterGlowBitmapEffect", typeof(OuterGlowBitmapEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new OuterGlowBitmapEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Page(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 434, "Page", typeof(Page), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Page();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PageContent(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 435, "PageContent", typeof(PageContent), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PageContent();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PageFunctionBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 436, "PageFunctionBase", typeof(PageFunctionBase), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Panel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 437, "Panel", typeof(Panel), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Paragraph(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 438, "Paragraph", typeof(Paragraph), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Paragraph();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ParallelTimeline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 439, "ParallelTimeline", typeof(ParallelTimeline), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ParallelTimeline();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ParserContext(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 440, "ParserContext", typeof(ParserContext), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ParserContext();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PasswordBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 441, "PasswordBox", typeof(PasswordBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PasswordBox();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Path(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 442, "Path", typeof(Path), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Path();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathFigure(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 443, "PathFigure", typeof(PathFigure), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PathFigure();
		wpfKnownType.ContentPropertyName = "Segments";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathFigureCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 444, "PathFigureCollection", typeof(PathFigureCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PathFigureCollection();
		wpfKnownType.TypeConverterType = typeof(PathFigureCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathFigureCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 445, "PathFigureCollectionConverter", typeof(PathFigureCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PathFigureCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 446, "PathGeometry", typeof(PathGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PathGeometry();
		wpfKnownType.ContentPropertyName = "Figures";
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 447, "PathSegment", typeof(PathSegment), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PathSegmentCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 448, "PathSegmentCollection", typeof(PathSegmentCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PathSegmentCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PauseStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 449, "PauseStoryboard", typeof(PauseStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PauseStoryboard();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Pen(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 450, "Pen", typeof(Pen), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Pen();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PerspectiveCamera(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 451, "PerspectiveCamera", typeof(PerspectiveCamera), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PerspectiveCamera();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PixelFormat(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 452, "PixelFormat", typeof(PixelFormat), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(PixelFormat);
		wpfKnownType.TypeConverterType = typeof(PixelFormatConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PixelFormatConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 453, "PixelFormatConverter", typeof(PixelFormatConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PixelFormatConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PngBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 454, "PngBitmapDecoder", typeof(PngBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PngBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 455, "PngBitmapEncoder", typeof(PngBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PngBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 456, "Point", typeof(Point), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Point);
		wpfKnownType.TypeConverterType = typeof(PointConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 457, "Point3D", typeof(Point3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Point3D);
		wpfKnownType.TypeConverterType = typeof(Point3DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 458, "Point3DAnimation", typeof(Point3DAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 459, "Point3DAnimationBase", typeof(Point3DAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 460, "Point3DAnimationUsingKeyFrames", typeof(Point3DAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 461, "Point3DCollection", typeof(Point3DCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DCollection();
		wpfKnownType.TypeConverterType = typeof(Point3DCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 462, "Point3DCollectionConverter", typeof(Point3DCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 463, "Point3DConverter", typeof(Point3DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 464, "Point3DKeyFrame", typeof(Point3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point3DKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 465, "Point3DKeyFrameCollection", typeof(Point3DKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point3DKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point4D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 466, "Point4D", typeof(Point4D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Point4D);
		wpfKnownType.TypeConverterType = typeof(Point4DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Point4DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 467, "Point4DConverter", typeof(Point4DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Point4DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 468, "PointAnimation", typeof(PointAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 469, "PointAnimationBase", typeof(PointAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 470, "PointAnimationUsingKeyFrames", typeof(PointAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointAnimationUsingPath(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 471, "PointAnimationUsingPath", typeof(PointAnimationUsingPath), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointAnimationUsingPath();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 472, "PointCollection", typeof(PointCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointCollection();
		wpfKnownType.TypeConverterType = typeof(PointCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 473, "PointCollectionConverter", typeof(PointCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 474, "PointConverter", typeof(PointConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 475, "PointIListConverter", typeof(PointIListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointIListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 476, "PointKeyFrame", typeof(PointKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 477, "PointKeyFrameCollection", typeof(PointKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointLight(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 478, "PointLight", typeof(PointLight), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PointLight();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PointLightBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 479, "PointLightBase", typeof(PointLightBase), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PolyBezierSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 480, "PolyBezierSegment", typeof(PolyBezierSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PolyBezierSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PolyLineSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 481, "PolyLineSegment", typeof(PolyLineSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PolyLineSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PolyQuadraticBezierSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 482, "PolyQuadraticBezierSegment", typeof(PolyQuadraticBezierSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PolyQuadraticBezierSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Polygon(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 483, "Polygon", typeof(Polygon), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Polygon();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Polyline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 484, "Polyline", typeof(Polyline), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Polyline();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Popup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 485, "Popup", typeof(Popup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Popup();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PresentationSource(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 486, "PresentationSource", typeof(PresentationSource), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PriorityBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 487, "PriorityBinding", typeof(PriorityBinding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PriorityBinding();
		wpfKnownType.ContentPropertyName = "Bindings";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PriorityBindingExpression(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 488, "PriorityBindingExpression", typeof(PriorityBindingExpression), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ProgressBar(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 489, "ProgressBar", typeof(ProgressBar), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ProgressBar();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ProjectionCamera(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 490, "ProjectionCamera", typeof(ProjectionCamera), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PropertyPath(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 491, "PropertyPath", typeof(PropertyPath), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(PropertyPathConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PropertyPathConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 492, "PropertyPathConverter", typeof(PropertyPathConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new PropertyPathConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuadraticBezierSegment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 493, "QuadraticBezierSegment", typeof(QuadraticBezierSegment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuadraticBezierSegment();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Quaternion(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 494, "Quaternion", typeof(Quaternion), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Quaternion);
		wpfKnownType.TypeConverterType = typeof(QuaternionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 495, "QuaternionAnimation", typeof(QuaternionAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuaternionAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 496, "QuaternionAnimationBase", typeof(QuaternionAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 497, "QuaternionAnimationUsingKeyFrames", typeof(QuaternionAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuaternionAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 498, "QuaternionConverter", typeof(QuaternionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuaternionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 499, "QuaternionKeyFrame", typeof(QuaternionKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 500, "QuaternionKeyFrameCollection", typeof(QuaternionKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuaternionKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_QuaternionRotation3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 501, "QuaternionRotation3D", typeof(QuaternionRotation3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new QuaternionRotation3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RadialGradientBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 502, "RadialGradientBrush", typeof(RadialGradientBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RadialGradientBrush();
		wpfKnownType.ContentPropertyName = "GradientStops";
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RadioButton(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 503, "RadioButton", typeof(RadioButton), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RadioButton();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RangeBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 504, "RangeBase", typeof(RangeBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 505, "Rect", typeof(Rect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Rect);
		wpfKnownType.TypeConverterType = typeof(RectConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rect3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 506, "Rect3D", typeof(Rect3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Rect3D);
		wpfKnownType.TypeConverterType = typeof(Rect3DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rect3DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 507, "Rect3DConverter", typeof(Rect3DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Rect3DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 508, "RectAnimation", typeof(RectAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RectAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 509, "RectAnimationBase", typeof(RectAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 510, "RectAnimationUsingKeyFrames", typeof(RectAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RectAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 511, "RectConverter", typeof(RectConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RectConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 512, "RectKeyFrame", typeof(RectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 513, "RectKeyFrameCollection", typeof(RectKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RectKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rectangle(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 514, "Rectangle", typeof(Rectangle), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Rectangle();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RectangleGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 515, "RectangleGeometry", typeof(RectangleGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RectangleGeometry();
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RelativeSource(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 516, "RelativeSource", typeof(RelativeSource), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RelativeSource();
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(RelativeSourceMode) }, (object[] arguments) => new RelativeSource((RelativeSourceMode)arguments[0])));
		wpfKnownType.Constructors.Add(3, new Baml6ConstructorInfo(new List<Type>
		{
			typeof(RelativeSourceMode),
			typeof(Type),
			typeof(int)
		}, (object[] arguments) => new RelativeSource((RelativeSourceMode)arguments[0], (Type)arguments[1], (int)arguments[2])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RemoveStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 517, "RemoveStoryboard", typeof(RemoveStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RemoveStoryboard();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RenderOptions(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 518, "RenderOptions", typeof(RenderOptions), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RenderTargetBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 519, "RenderTargetBitmap", typeof(RenderTargetBitmap), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RepeatBehavior(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 520, "RepeatBehavior", typeof(RepeatBehavior), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(RepeatBehavior);
		wpfKnownType.TypeConverterType = typeof(RepeatBehaviorConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RepeatBehaviorConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 521, "RepeatBehaviorConverter", typeof(RepeatBehaviorConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RepeatBehaviorConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RepeatButton(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 522, "RepeatButton", typeof(RepeatButton), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RepeatButton();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ResizeGrip(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 523, "ResizeGrip", typeof(ResizeGrip), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ResizeGrip();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ResourceDictionary(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 524, "ResourceDictionary", typeof(ResourceDictionary), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ResourceDictionary();
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.CollectionKind = XamlCollectionKind.Dictionary;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ResourceKey(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 525, "ResourceKey", typeof(ResourceKey), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ResumeStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 526, "ResumeStoryboard", typeof(ResumeStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ResumeStoryboard();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RichTextBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 527, "RichTextBox", typeof(RichTextBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RichTextBox();
		wpfKnownType.ContentPropertyName = "Document";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RotateTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 528, "RotateTransform", typeof(RotateTransform), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RotateTransform();
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RotateTransform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 529, "RotateTransform3D", typeof(RotateTransform3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RotateTransform3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 530, "Rotation3D", typeof(Rotation3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3DAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 531, "Rotation3DAnimation", typeof(Rotation3DAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Rotation3DAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3DAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 532, "Rotation3DAnimationBase", typeof(Rotation3DAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3DAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 533, "Rotation3DAnimationUsingKeyFrames", typeof(Rotation3DAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Rotation3DAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 534, "Rotation3DKeyFrame", typeof(Rotation3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Rotation3DKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 535, "Rotation3DKeyFrameCollection", typeof(Rotation3DKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Rotation3DKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RoutedCommand(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 536, "RoutedCommand", typeof(RoutedCommand), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RoutedCommand();
		wpfKnownType.TypeConverterType = typeof(CommandConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RoutedEvent(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 537, "RoutedEvent", typeof(RoutedEvent), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(RoutedEventConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RoutedEventConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 538, "RoutedEventConverter", typeof(RoutedEventConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RoutedEventConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RoutedUICommand(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 539, "RoutedUICommand", typeof(RoutedUICommand), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RoutedUICommand();
		wpfKnownType.TypeConverterType = typeof(CommandConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RoutingStrategy(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 540, "RoutingStrategy", typeof(RoutingStrategy), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => RoutingStrategy.Tunnel;
		wpfKnownType.TypeConverterType = typeof(RoutingStrategy);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RowDefinition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 541, "RowDefinition", typeof(RowDefinition), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new RowDefinition();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Run(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 542, "Run", typeof(Run), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Run();
		wpfKnownType.ContentPropertyName = "Text";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RuntimeNamePropertyAttribute(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 543, "RuntimeNamePropertyAttribute", typeof(RuntimeNamePropertyAttribute), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SByte(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 544, "SByte", typeof(sbyte), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => (sbyte)0;
		wpfKnownType.TypeConverterType = typeof(SByteConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SByteConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 545, "SByteConverter", typeof(SByteConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SByteConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScaleTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 546, "ScaleTransform", typeof(ScaleTransform), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ScaleTransform();
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScaleTransform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 547, "ScaleTransform3D", typeof(ScaleTransform3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ScaleTransform3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScrollBar(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 548, "ScrollBar", typeof(ScrollBar), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ScrollBar();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScrollContentPresenter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 549, "ScrollContentPresenter", typeof(ScrollContentPresenter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ScrollContentPresenter();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScrollViewer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 550, "ScrollViewer", typeof(ScrollViewer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ScrollViewer();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Section(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 551, "Section", typeof(Section), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Section();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SeekStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 552, "SeekStoryboard", typeof(SeekStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SeekStoryboard();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Selector(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 553, "Selector", typeof(Selector), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Separator(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 554, "Separator", typeof(Separator), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Separator();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SetStoryboardSpeedRatio(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 555, "SetStoryboardSpeedRatio", typeof(SetStoryboardSpeedRatio), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SetStoryboardSpeedRatio();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Setter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 556, "Setter", typeof(Setter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Setter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SetterBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 557, "SetterBase", typeof(SetterBase), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Shape(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 558, "Shape", typeof(Shape), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Single(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 559, "Single", typeof(float), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0f;
		wpfKnownType.TypeConverterType = typeof(SingleConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 560, "SingleAnimation", typeof(SingleAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SingleAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 561, "SingleAnimationBase", typeof(SingleAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 562, "SingleAnimationUsingKeyFrames", typeof(SingleAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SingleAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 563, "SingleConverter", typeof(SingleConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SingleConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 564, "SingleKeyFrame", typeof(SingleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SingleKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 565, "SingleKeyFrameCollection", typeof(SingleKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SingleKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Size(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 566, "Size", typeof(Size), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Size);
		wpfKnownType.TypeConverterType = typeof(SizeConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Size3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 567, "Size3D", typeof(Size3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Size3D);
		wpfKnownType.TypeConverterType = typeof(Size3DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Size3DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 568, "Size3DConverter", typeof(Size3DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Size3DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 569, "SizeAnimation", typeof(SizeAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SizeAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 570, "SizeAnimationBase", typeof(SizeAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 571, "SizeAnimationUsingKeyFrames", typeof(SizeAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SizeAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 572, "SizeConverter", typeof(SizeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SizeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 573, "SizeKeyFrame", typeof(SizeKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SizeKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 574, "SizeKeyFrameCollection", typeof(SizeKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SizeKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SkewTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 575, "SkewTransform", typeof(SkewTransform), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SkewTransform();
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SkipStoryboardToFill(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 576, "SkipStoryboardToFill", typeof(SkipStoryboardToFill), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SkipStoryboardToFill();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Slider(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 577, "Slider", typeof(Slider), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Slider();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SolidColorBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 578, "SolidColorBrush", typeof(SolidColorBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SolidColorBrush();
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SoundPlayerAction(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 579, "SoundPlayerAction", typeof(SoundPlayerAction), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SoundPlayerAction();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Span(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 580, "Span", typeof(Span), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Span();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SpecularMaterial(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 581, "SpecularMaterial", typeof(SpecularMaterial), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SpecularMaterial();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SpellCheck(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 582, "SpellCheck", typeof(SpellCheck), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineByteKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 583, "SplineByteKeyFrame", typeof(SplineByteKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineByteKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineColorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 584, "SplineColorKeyFrame", typeof(SplineColorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineColorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineDecimalKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 585, "SplineDecimalKeyFrame", typeof(SplineDecimalKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineDecimalKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineDoubleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 586, "SplineDoubleKeyFrame", typeof(SplineDoubleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineDoubleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineInt16KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 587, "SplineInt16KeyFrame", typeof(SplineInt16KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineInt16KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineInt32KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 588, "SplineInt32KeyFrame", typeof(SplineInt32KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineInt32KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineInt64KeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 589, "SplineInt64KeyFrame", typeof(SplineInt64KeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineInt64KeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplinePoint3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 590, "SplinePoint3DKeyFrame", typeof(SplinePoint3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplinePoint3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplinePointKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 591, "SplinePointKeyFrame", typeof(SplinePointKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplinePointKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineQuaternionKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 592, "SplineQuaternionKeyFrame", typeof(SplineQuaternionKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineQuaternionKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineRectKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 593, "SplineRectKeyFrame", typeof(SplineRectKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineRectKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineRotation3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 594, "SplineRotation3DKeyFrame", typeof(SplineRotation3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineRotation3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineSingleKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 595, "SplineSingleKeyFrame", typeof(SplineSingleKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineSingleKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineSizeKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 596, "SplineSizeKeyFrame", typeof(SplineSizeKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineSizeKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineThicknessKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 597, "SplineThicknessKeyFrame", typeof(SplineThicknessKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineThicknessKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineVector3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 598, "SplineVector3DKeyFrame", typeof(SplineVector3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineVector3DKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SplineVectorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 599, "SplineVectorKeyFrame", typeof(SplineVectorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SplineVectorKeyFrame();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SpotLight(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 600, "SpotLight", typeof(SpotLight), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SpotLight();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StackPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 601, "StackPanel", typeof(StackPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StackPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StaticExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 602, "StaticExtension", typeof(System.Windows.Markup.StaticExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new MS.Internal.Markup.StaticExtension();
		wpfKnownType.HasSpecialValueConverter = true;
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(string) }, (object[] arguments) => new MS.Internal.Markup.StaticExtension((string)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StaticResourceExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 603, "StaticResourceExtension", typeof(StaticResourceExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StaticResourceExtension();
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(object) }, (object[] arguments) => new StaticResourceExtension(arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StatusBar(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 604, "StatusBar", typeof(StatusBar), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StatusBar();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StatusBarItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 605, "StatusBarItem", typeof(StatusBarItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StatusBarItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StickyNoteControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 606, "StickyNoteControl", typeof(StickyNoteControl), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StopStoryboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 607, "StopStoryboard", typeof(StopStoryboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StopStoryboard();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Storyboard(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 608, "Storyboard", typeof(Storyboard), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Storyboard();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StreamGeometry(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 609, "StreamGeometry", typeof(StreamGeometry), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StreamGeometry();
		wpfKnownType.TypeConverterType = typeof(GeometryConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StreamGeometryContext(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 610, "StreamGeometryContext", typeof(StreamGeometryContext), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StreamResourceInfo(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 611, "StreamResourceInfo", typeof(StreamResourceInfo), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StreamResourceInfo();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_String(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 612, "String", typeof(string), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(StringConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StringAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 613, "StringAnimationBase", typeof(StringAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StringAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 614, "StringAnimationUsingKeyFrames", typeof(StringAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StringAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StringConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 615, "StringConverter", typeof(StringConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StringConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StringKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 616, "StringKeyFrame", typeof(StringKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StringKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 617, "StringKeyFrameCollection", typeof(StringKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StringKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StrokeCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 618, "StrokeCollection", typeof(StrokeCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StrokeCollection();
		wpfKnownType.TypeConverterType = typeof(StrokeCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StrokeCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 619, "StrokeCollectionConverter", typeof(StrokeCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new StrokeCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Style(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 620, "Style", typeof(Style), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Style();
		wpfKnownType.ContentPropertyName = "Setters";
		wpfKnownType.DictionaryKeyPropertyName = "TargetType";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Stylus(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 621, "Stylus", typeof(Stylus), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_StylusDevice(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 622, "StylusDevice", typeof(StylusDevice), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TabControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 623, "TabControl", typeof(TabControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TabControl();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TabItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 624, "TabItem", typeof(TabItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TabItem();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TabPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 625, "TabPanel", typeof(TabPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TabPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Table(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 626, "Table", typeof(Table), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Table();
		wpfKnownType.ContentPropertyName = "RowGroups";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TableCell(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 627, "TableCell", typeof(TableCell), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TableCell();
		wpfKnownType.ContentPropertyName = "Blocks";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TableColumn(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 628, "TableColumn", typeof(TableColumn), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TableColumn();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TableRow(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 629, "TableRow", typeof(TableRow), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TableRow();
		wpfKnownType.ContentPropertyName = "Cells";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TableRowGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 630, "TableRowGroup", typeof(TableRowGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TableRowGroup();
		wpfKnownType.ContentPropertyName = "Rows";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TabletDevice(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 631, "TabletDevice", typeof(TabletDevice), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateBindingExpression(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 632, "TemplateBindingExpression", typeof(TemplateBindingExpression), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(TemplateBindingExpressionConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateBindingExpressionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 633, "TemplateBindingExpressionConverter", typeof(TemplateBindingExpressionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TemplateBindingExpressionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateBindingExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 634, "TemplateBindingExtension", typeof(TemplateBindingExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TemplateBindingExtension();
		wpfKnownType.TypeConverterType = typeof(TemplateBindingExtensionConverter);
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(DependencyProperty) }, (object[] arguments) => new TemplateBindingExtension((DependencyProperty)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateBindingExtensionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 635, "TemplateBindingExtensionConverter", typeof(TemplateBindingExtensionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TemplateBindingExtensionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateKey(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 636, "TemplateKey", typeof(TemplateKey), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(TemplateKeyConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TemplateKeyConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 637, "TemplateKeyConverter", typeof(TemplateKeyConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TemplateKeyConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextBlock(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 638, "TextBlock", typeof(TextBlock), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextBlock();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextBox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 639, "TextBox", typeof(TextBox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextBox();
		wpfKnownType.ContentPropertyName = "Text";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextBoxBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 640, "TextBoxBase", typeof(TextBoxBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextComposition(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 641, "TextComposition", typeof(TextComposition), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextCompositionManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 642, "TextCompositionManager", typeof(TextCompositionManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextDecoration(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 643, "TextDecoration", typeof(TextDecoration), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextDecoration();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextDecorationCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 644, "TextDecorationCollection", typeof(TextDecorationCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextDecorationCollection();
		wpfKnownType.TypeConverterType = typeof(TextDecorationCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextDecorationCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 645, "TextDecorationCollectionConverter", typeof(TextDecorationCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextDecorationCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextEffect(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 646, "TextEffect", typeof(TextEffect), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextEffect();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextEffectCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 647, "TextEffectCollection", typeof(TextEffectCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TextEffectCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 648, "TextElement", typeof(TextElement), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextSearch(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 649, "TextSearch", typeof(TextSearch), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThemeDictionaryExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 650, "ThemeDictionaryExtension", typeof(ThemeDictionaryExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ThemeDictionaryExtension();
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(string) }, (object[] arguments) => new ThemeDictionaryExtension((string)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Thickness(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 651, "Thickness", typeof(Thickness), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Thickness);
		wpfKnownType.TypeConverterType = typeof(ThicknessConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 652, "ThicknessAnimation", typeof(ThicknessAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ThicknessAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 653, "ThicknessAnimationBase", typeof(ThicknessAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 654, "ThicknessAnimationUsingKeyFrames", typeof(ThicknessAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ThicknessAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 655, "ThicknessConverter", typeof(ThicknessConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ThicknessConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 656, "ThicknessKeyFrame", typeof(ThicknessKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ThicknessKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 657, "ThicknessKeyFrameCollection", typeof(ThicknessKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ThicknessKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Thumb(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 658, "Thumb", typeof(Thumb), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Thumb();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TickBar(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 659, "TickBar", typeof(TickBar), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TickBar();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TiffBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 660, "TiffBitmapDecoder", typeof(TiffBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TiffBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 661, "TiffBitmapEncoder", typeof(TiffBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TiffBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TileBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 662, "TileBrush", typeof(TileBrush), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TimeSpan(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 663, "TimeSpan", typeof(TimeSpan), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(TimeSpan);
		wpfKnownType.TypeConverterType = typeof(TimeSpanConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TimeSpanConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 664, "TimeSpanConverter", typeof(TimeSpanConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TimeSpanConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Timeline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 665, "Timeline", typeof(Timeline), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TimelineCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 666, "TimelineCollection", typeof(TimelineCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TimelineCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TimelineGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 667, "TimelineGroup", typeof(TimelineGroup), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToggleButton(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 668, "ToggleButton", typeof(ToggleButton), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToggleButton();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolBar(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 669, "ToolBar", typeof(ToolBar), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToolBar();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolBarOverflowPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 670, "ToolBarOverflowPanel", typeof(ToolBarOverflowPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToolBarOverflowPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolBarPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 671, "ToolBarPanel", typeof(ToolBarPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToolBarPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolBarTray(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 672, "ToolBarTray", typeof(ToolBarTray), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToolBarTray();
		wpfKnownType.ContentPropertyName = "ToolBars";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolTip(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 673, "ToolTip", typeof(ToolTip), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ToolTip();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ToolTipService(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 674, "ToolTipService", typeof(ToolTipService), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Track(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 675, "Track", typeof(Track), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Track();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Transform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 676, "Transform", typeof(Transform), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Transform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 677, "Transform3D", typeof(Transform3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Transform3DCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 678, "Transform3DCollection", typeof(Transform3DCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Transform3DCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Transform3DGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 679, "Transform3DGroup", typeof(Transform3DGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Transform3DGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TransformCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 680, "TransformCollection", typeof(TransformCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TransformCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TransformConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 681, "TransformConverter", typeof(TransformConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TransformConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TransformGroup(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 682, "TransformGroup", typeof(TransformGroup), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TransformGroup();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TransformedBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 683, "TransformedBitmap", typeof(TransformedBitmap), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TransformedBitmap();
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TranslateTransform(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 684, "TranslateTransform", typeof(TranslateTransform), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TranslateTransform();
		wpfKnownType.TypeConverterType = typeof(TransformConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TranslateTransform3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 685, "TranslateTransform3D", typeof(TranslateTransform3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TranslateTransform3D();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TreeView(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 686, "TreeView", typeof(TreeView), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TreeView();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TreeViewItem(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 687, "TreeViewItem", typeof(TreeViewItem), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TreeViewItem();
		wpfKnownType.ContentPropertyName = "Items";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Trigger(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 688, "Trigger", typeof(Trigger), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Trigger();
		wpfKnownType.ContentPropertyName = "Setters";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TriggerAction(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 689, "TriggerAction", typeof(TriggerAction), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TriggerBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 690, "TriggerBase", typeof(TriggerBase), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TypeExtension(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 691, "TypeExtension", typeof(TypeExtension), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TypeExtension();
		wpfKnownType.HasSpecialValueConverter = true;
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(Type) }, (object[] arguments) => new TypeExtension((Type)arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TypeTypeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 692, "TypeTypeConverter", typeof(TypeTypeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new TypeTypeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Typography(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 693, "Typography", typeof(Typography), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UIElement(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 694, "UIElement", typeof(UIElement), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UIElement();
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt16(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 695, "UInt16", typeof(ushort), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => (ushort)0;
		wpfKnownType.TypeConverterType = typeof(UInt16Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt16Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 696, "UInt16Converter", typeof(UInt16Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UInt16Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt32(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 697, "UInt32", typeof(uint), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0u;
		wpfKnownType.TypeConverterType = typeof(UInt32Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt32Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 698, "UInt32Converter", typeof(UInt32Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UInt32Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt64(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 699, "UInt64", typeof(ulong), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => 0uL;
		wpfKnownType.TypeConverterType = typeof(UInt64Converter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UInt64Converter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 700, "UInt64Converter", typeof(UInt64Converter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UInt64Converter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UShortIListConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 701, "UShortIListConverter", typeof(UShortIListConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UShortIListConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Underline(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 702, "Underline", typeof(Underline), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Underline();
		wpfKnownType.ContentPropertyName = "Inlines";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UniformGrid(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 703, "UniformGrid", typeof(UniformGrid), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UniformGrid();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Uri(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 704, "Uri", typeof(Uri), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(UriTypeConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UriTypeConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 705, "UriTypeConverter", typeof(UriTypeConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UriTypeConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UserControl(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 706, "UserControl", typeof(UserControl), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new UserControl();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Validation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 707, "Validation", typeof(Validation), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 708, "Vector", typeof(Vector), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Vector);
		wpfKnownType.TypeConverterType = typeof(VectorConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 709, "Vector3D", typeof(Vector3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => default(Vector3D);
		wpfKnownType.TypeConverterType = typeof(Vector3DConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 710, "Vector3DAnimation", typeof(Vector3DAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 711, "Vector3DAnimationBase", typeof(Vector3DAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 712, "Vector3DAnimationUsingKeyFrames", typeof(Vector3DAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 713, "Vector3DCollection", typeof(Vector3DCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DCollection();
		wpfKnownType.TypeConverterType = typeof(Vector3DCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 714, "Vector3DCollectionConverter", typeof(Vector3DCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 715, "Vector3DConverter", typeof(Vector3DConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 716, "Vector3DKeyFrame", typeof(Vector3DKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Vector3DKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 717, "Vector3DKeyFrameCollection", typeof(Vector3DKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Vector3DKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorAnimation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 718, "VectorAnimation", typeof(VectorAnimation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorAnimation();
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorAnimationBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 719, "VectorAnimationBase", typeof(VectorAnimationBase), isBamlType, useV3Rules);
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorAnimationUsingKeyFrames(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 720, "VectorAnimationUsingKeyFrames", typeof(VectorAnimationUsingKeyFrames), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorAnimationUsingKeyFrames();
		wpfKnownType.ContentPropertyName = "KeyFrames";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 721, "VectorCollection", typeof(VectorCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorCollection();
		wpfKnownType.TypeConverterType = typeof(VectorCollectionConverter);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorCollectionConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 722, "VectorCollectionConverter", typeof(VectorCollectionConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorCollectionConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 723, "VectorConverter", typeof(VectorConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorKeyFrame(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 724, "VectorKeyFrame", typeof(VectorKeyFrame), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VectorKeyFrameCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 725, "VectorKeyFrameCollection", typeof(VectorKeyFrameCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VectorKeyFrameCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VideoDrawing(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 726, "VideoDrawing", typeof(VideoDrawing), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VideoDrawing();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ViewBase(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 727, "ViewBase", typeof(ViewBase), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Viewbox(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 728, "Viewbox", typeof(Viewbox), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Viewbox();
		wpfKnownType.ContentPropertyName = "Child";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Viewport3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 729, "Viewport3D", typeof(Viewport3D), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Viewport3D();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Viewport3DVisual(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 730, "Viewport3DVisual", typeof(Viewport3DVisual), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Viewport3DVisual();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VirtualizingPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 731, "VirtualizingPanel", typeof(VirtualizingPanel), isBamlType, useV3Rules);
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VirtualizingStackPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 732, "VirtualizingStackPanel", typeof(VirtualizingStackPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VirtualizingStackPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Visual(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 733, "Visual", typeof(Visual), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Visual3D(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 734, "Visual3D", typeof(Visual3D), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VisualBrush(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 735, "VisualBrush", typeof(VisualBrush), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new VisualBrush();
		wpfKnownType.TypeConverterType = typeof(BrushConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_VisualTarget(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 736, "VisualTarget", typeof(VisualTarget), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WeakEventManager(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 737, "WeakEventManager", typeof(WeakEventManager), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WhitespaceSignificantCollectionAttribute(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 738, "WhitespaceSignificantCollectionAttribute", typeof(WhitespaceSignificantCollectionAttribute), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new WhitespaceSignificantCollectionAttribute();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Window(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 739, "Window", typeof(Window), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new Window();
		wpfKnownType.ContentPropertyName = "Content";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WmpBitmapDecoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 740, "WmpBitmapDecoder", typeof(WmpBitmapDecoder), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WmpBitmapEncoder(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 741, "WmpBitmapEncoder", typeof(WmpBitmapEncoder), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new WmpBitmapEncoder();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WrapPanel(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 742, "WrapPanel", typeof(WrapPanel), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new WrapPanel();
		wpfKnownType.ContentPropertyName = "Children";
		wpfKnownType.RuntimeNamePropertyName = "Name";
		wpfKnownType.XmlLangPropertyName = "Language";
		wpfKnownType.UidPropertyName = "Uid";
		wpfKnownType.IsUsableDuringInit = true;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_WriteableBitmap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 743, "WriteableBitmap", typeof(WriteableBitmap), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(ImageSourceConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlBrushSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 744, "XamlBrushSerializer", typeof(XamlBrushSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlBrushSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlInt32CollectionSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 745, "XamlInt32CollectionSerializer", typeof(XamlInt32CollectionSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlInt32CollectionSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlPathDataSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 746, "XamlPathDataSerializer", typeof(XamlPathDataSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlPathDataSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlPoint3DCollectionSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 747, "XamlPoint3DCollectionSerializer", typeof(XamlPoint3DCollectionSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlPoint3DCollectionSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlPointCollectionSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 748, "XamlPointCollectionSerializer", typeof(XamlPointCollectionSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlPointCollectionSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlReader(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 749, "XamlReader", typeof(System.Windows.Markup.XamlReader), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new System.Windows.Markup.XamlReader();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlStyleSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 750, "XamlStyleSerializer", typeof(XamlStyleSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlStyleSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlTemplateSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 751, "XamlTemplateSerializer", typeof(XamlTemplateSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlTemplateSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlVector3DCollectionSerializer(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 752, "XamlVector3DCollectionSerializer", typeof(XamlVector3DCollectionSerializer), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XamlVector3DCollectionSerializer();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XamlWriter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 753, "XamlWriter", typeof(System.Windows.Markup.XamlWriter), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlDataProvider(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 754, "XmlDataProvider", typeof(XmlDataProvider), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XmlDataProvider();
		wpfKnownType.ContentPropertyName = "XmlSerializer";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlLangPropertyAttribute(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 755, "XmlLangPropertyAttribute", typeof(XmlLangPropertyAttribute), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlLanguage(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 756, "XmlLanguage", typeof(XmlLanguage), isBamlType, useV3Rules);
		wpfKnownType.TypeConverterType = typeof(XmlLanguageConverter);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlLanguageConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 757, "XmlLanguageConverter", typeof(XmlLanguageConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XmlLanguageConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlNamespaceMapping(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 758, "XmlNamespaceMapping", typeof(XmlNamespaceMapping), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XmlNamespaceMapping();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ZoomPercentageConverter(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 759, "ZoomPercentageConverter", typeof(ZoomPercentageConverter), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ZoomPercentageConverter();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CommandBinding(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "CommandBinding", typeof(CommandBinding), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CommandBinding();
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_XmlNamespaceMappingCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "XmlNamespaceMappingCollection", typeof(XmlNamespaceMappingCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new XmlNamespaceMappingCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PageContentCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "PageContentCollection", typeof(PageContentCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_DocumentReferenceCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "DocumentReferenceCollection", typeof(DocumentReferenceCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyboardNavigationMode(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "KeyboardNavigationMode", typeof(KeyboardNavigationMode), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => KeyboardNavigationMode.Continue;
		wpfKnownType.TypeConverterType = typeof(KeyboardNavigationMode);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Enum(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "Enum", typeof(Enum), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RelativeSourceMode(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "RelativeSourceMode", typeof(RelativeSourceMode), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => RelativeSourceMode.PreviousData;
		wpfKnownType.TypeConverterType = typeof(RelativeSourceMode);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PenLineJoin(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "PenLineJoin", typeof(PenLineJoin), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => PenLineJoin.Miter;
		wpfKnownType.TypeConverterType = typeof(PenLineJoin);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_PenLineCap(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "PenLineCap", typeof(PenLineCap), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => PenLineCap.Flat;
		wpfKnownType.TypeConverterType = typeof(PenLineCap);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_InputBindingCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "InputBindingCollection", typeof(InputBindingCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new InputBindingCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_CommandBindingCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "CommandBindingCollection", typeof(CommandBindingCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new CommandBindingCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Stretch(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "Stretch", typeof(Stretch), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => Stretch.None;
		wpfKnownType.TypeConverterType = typeof(Stretch);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_Orientation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "Orientation", typeof(Orientation), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => Orientation.Horizontal;
		wpfKnownType.TypeConverterType = typeof(Orientation);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TextAlignment(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "TextAlignment", typeof(TextAlignment), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => TextAlignment.Left;
		wpfKnownType.TypeConverterType = typeof(TextAlignment);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_NavigationUIVisibility(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "NavigationUIVisibility", typeof(NavigationUIVisibility), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => NavigationUIVisibility.Automatic;
		wpfKnownType.TypeConverterType = typeof(NavigationUIVisibility);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_JournalOwnership(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "JournalOwnership", typeof(JournalOwnership), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => JournalOwnership.Automatic;
		wpfKnownType.TypeConverterType = typeof(JournalOwnership);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ScrollBarVisibility(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "ScrollBarVisibility", typeof(ScrollBarVisibility), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => ScrollBarVisibility.Disabled;
		wpfKnownType.TypeConverterType = typeof(ScrollBarVisibility);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_TriggerCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "TriggerCollection", typeof(TriggerCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_UIElementCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "UIElementCollection", typeof(UIElementCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_SetterBaseCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "SetterBaseCollection", typeof(SetterBaseCollection), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new SetterBaseCollection();
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ColumnDefinitionCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "ColumnDefinitionCollection", typeof(ColumnDefinitionCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_RowDefinitionCollection(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "RowDefinitionCollection", typeof(RowDefinitionCollection), isBamlType, useV3Rules);
		wpfKnownType.CollectionKind = XamlCollectionKind.Collection;
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemContainerTemplate(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "ItemContainerTemplate", typeof(ItemContainerTemplate), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ItemContainerTemplate();
		wpfKnownType.ContentPropertyName = "Template";
		wpfKnownType.DictionaryKeyPropertyName = "ItemContainerTemplateKey";
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_ItemContainerTemplateKey(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "ItemContainerTemplateKey", typeof(ItemContainerTemplateKey), isBamlType, useV3Rules);
		wpfKnownType.DefaultConstructor = () => new ItemContainerTemplateKey();
		wpfKnownType.TypeConverterType = typeof(TemplateKeyConverter);
		wpfKnownType.Constructors.Add(1, new Baml6ConstructorInfo(new List<Type> { typeof(object) }, (object[] arguments) => new ItemContainerTemplateKey(arguments[0])));
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private WpfKnownType Create_BamlType_KeyboardNavigation(bool isBamlType, bool useV3Rules)
	{
		WpfKnownType wpfKnownType = new WpfKnownType(this, 0, "KeyboardNavigation", typeof(KeyboardNavigation), isBamlType, useV3Rules);
		wpfKnownType.Freeze();
		return wpfKnownType;
	}

	public WpfSharedBamlSchemaContext()
	{
		Initialize();
	}

	public WpfSharedBamlSchemaContext(XamlSchemaContextSettings settings)
		: base(settings)
	{
		Initialize();
	}

	private void Initialize()
	{
		_syncObject = new object();
		_knownBamlAssemblies = new Baml6Assembly[5];
		_knownBamlTypes = new WpfKnownType[760];
		_masterTypeTable = new Dictionary<Type, XamlType>(256);
		_knownBamlMembers = new WpfKnownMember[271];
	}

	internal string GetKnownBamlString(short stringId)
	{
		return stringId switch
		{
			-1 => "Name", 
			-2 => "Uid", 
			_ => null, 
		};
	}

	internal Baml6Assembly GetKnownBamlAssembly(short assemblyId)
	{
		if (assemblyId > 0)
		{
			throw new ArgumentException(SR.AssemblyIdNegative);
		}
		assemblyId = (short)(-assemblyId);
		Baml6Assembly baml6Assembly = _knownBamlAssemblies[assemblyId];
		if (baml6Assembly == null)
		{
			baml6Assembly = CreateKnownBamlAssembly(assemblyId);
			_knownBamlAssemblies[assemblyId] = baml6Assembly;
		}
		return baml6Assembly;
	}

	internal Baml6Assembly CreateKnownBamlAssembly(short assemblyId)
	{
		return assemblyId switch
		{
			0 => new Baml6Assembly(typeof(double).Assembly), 
			1 => new Baml6Assembly(typeof(Uri).Assembly), 
			2 => new Baml6Assembly(typeof(DependencyObject).Assembly), 
			3 => new Baml6Assembly(typeof(UIElement).Assembly), 
			4 => new Baml6Assembly(typeof(FrameworkElement).Assembly), 
			_ => null, 
		};
	}

	internal WpfKnownType GetKnownBamlType(short typeId)
	{
		if (typeId >= 0)
		{
			throw new ArgumentException(SR.KnownTypeIdNegative);
		}
		typeId = (short)(-typeId);
		WpfKnownType wpfKnownType;
		lock (_syncObject)
		{
			wpfKnownType = _knownBamlTypes[typeId];
			if (wpfKnownType == null)
			{
				wpfKnownType = CreateKnownBamlType(typeId, isBamlType: true, useV3Rules: true);
				_knownBamlTypes[typeId] = wpfKnownType;
				_masterTypeTable.Add(wpfKnownType.UnderlyingType, wpfKnownType);
			}
		}
		return wpfKnownType;
	}

	internal WpfKnownMember GetKnownBamlMember(short memberId)
	{
		if (memberId >= 0)
		{
			throw new ArgumentException(SR.KnownTypeIdNegative);
		}
		memberId = (short)(-memberId);
		WpfKnownMember wpfKnownMember;
		lock (_syncObject)
		{
			wpfKnownMember = _knownBamlMembers[memberId];
			if (wpfKnownMember == null)
			{
				wpfKnownMember = CreateKnownMember(memberId);
				_knownBamlMembers[memberId] = wpfKnownMember;
			}
		}
		return wpfKnownMember;
	}

	public override XamlType GetXamlType(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		XamlType xamlType = GetKnownXamlType(type);
		if (xamlType == null)
		{
			xamlType = GetUnknownXamlType(type);
		}
		return xamlType;
	}

	private XamlType GetUnknownXamlType(Type type)
	{
		XamlType value;
		lock (_syncObject)
		{
			if (!_masterTypeTable.TryGetValue(type, out value))
			{
				WpfSharedXamlSchemaContext.RequireRuntimeType(type);
				value = new WpfXamlType(type, this, isBamlScenario: true, useV3Rules: true);
				_masterTypeTable.Add(type, value);
			}
		}
		return value;
	}

	internal XamlType GetKnownXamlType(Type type)
	{
		XamlType value;
		lock (_syncObject)
		{
			if (!_masterTypeTable.TryGetValue(type, out value))
			{
				value = CreateKnownBamlType(type.Name, isBamlType: true, useV3Rules: true);
				if (value == null && _themeHelpers != null)
				{
					foreach (ThemeKnownTypeHelper themeHelper in _themeHelpers)
					{
						value = themeHelper.GetKnownXamlType(type.Name);
						if (value != null && value.UnderlyingType == type)
						{
							break;
						}
					}
				}
				if (value != null && value.UnderlyingType == type)
				{
					WpfKnownType wpfKnownType = value as WpfKnownType;
					if (wpfKnownType != null)
					{
						_knownBamlTypes[wpfKnownType.BamlNumber] = wpfKnownType;
					}
					_masterTypeTable.Add(type, value);
				}
				else
				{
					value = null;
				}
			}
		}
		return value;
	}

	internal XamlValueConverter<XamlDeferringLoader> GetDeferringLoader(Type loaderType)
	{
		return GetValueConverter<XamlDeferringLoader>(loaderType, null);
	}

	internal XamlValueConverter<TypeConverter> GetTypeConverter(Type converterType)
	{
		if (converterType.IsEnum)
		{
			return GetValueConverter<TypeConverter>(typeof(EnumConverter), GetXamlType(converterType));
		}
		return GetValueConverter<TypeConverter>(converterType, null);
	}

	protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		return base.GetXamlType(xamlNamespace, name, typeArguments);
	}

	public XamlType GetXamlTypeExposed(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		return base.GetXamlType(xamlNamespace, name, typeArguments);
	}

	internal Type ResolvePrefixedNameWithAdditionalWpfSemantics(string prefixedName, DependencyObject element)
	{
		XmlnsDictionary xmlnsDictionary = element.GetValue(XmlAttributeProperties.XmlnsDictionaryProperty) as XmlnsDictionary;
		Hashtable hashtable = element.GetValue(XmlAttributeProperties.XmlNamespaceMapsProperty) as Hashtable;
		if (xmlnsDictionary == null)
		{
			if (_wpfDefaultNamespace == null)
			{
				XmlnsDictionary xmlnsDictionary2 = new XmlnsDictionary();
				xmlnsDictionary2.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
				_wpfDefaultNamespace = xmlnsDictionary2;
			}
			xmlnsDictionary = _wpfDefaultNamespace;
		}
		else if (hashtable != null && hashtable.Count > 0)
		{
			Type typeFromName = XamlTypeMapper.GetTypeFromName(prefixedName, element);
			if (typeFromName != null)
			{
				return typeFromName;
			}
		}
		if (XamlTypeName.TryParse(prefixedName, xmlnsDictionary, out var result))
		{
			XamlType xamlType = GetXamlType(result);
			if (xamlType != null)
			{
				return xamlType.UnderlyingType;
			}
		}
		return null;
	}
}
