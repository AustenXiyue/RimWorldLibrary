using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
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
using MS.Internal.PresentationFramework;
using MS.Internal.WindowsBase;

namespace System.Windows.Baml2006;

internal class Baml2006SchemaContext : XamlSchemaContext
{
	private delegate Type LazyTypeOf();

	internal static class KnownTypes
	{
		public const short BooleanConverter = 46;

		public const short DependencyPropertyConverter = 137;

		public const short EnumConverter = 195;

		public const short StringConverter = 615;

		public const short XamlBrushSerializer = 744;

		public const short XamlInt32CollectionSerializer = 745;

		public const short XamlPathDataSerializer = 746;

		public const short XamlPoint3DCollectionSerializer = 747;

		public const short XamlPointCollectionSerializer = 748;

		public const short XamlVector3DCollectionSerializer = 752;

		public const short MaxKnownType = 759;

		public const short MaxKnownProperty = 268;

		public const short MinKnownProperty = -268;

		public const short VisualTreeKnownPropertyId = -174;

		public static Type GetAttachableTargetType(short propertyId)
		{
			return propertyId switch
			{
				-39 => typeof(UIElement), 
				-61 => typeof(UIElement), 
				-62 => typeof(UIElement), 
				-63 => typeof(UIElement), 
				-64 => typeof(UIElement), 
				_ => typeof(DependencyObject), 
			};
		}

		public static Assembly GetKnownAssembly(short assemblyId)
		{
			return -assemblyId switch
			{
				0 => typeof(double).Assembly, 
				1 => typeof(Uri).Assembly, 
				2 => typeof(DependencyObject).Assembly, 
				3 => typeof(UIElement).Assembly, 
				4 => typeof(FrameworkElement).Assembly, 
				_ => null, 
			};
		}

		public static Type GetKnownType(short typeId)
		{
			typeId = (short)(-typeId);
			return (typeId switch
			{
				1 => () => typeof(AccessText), 
				2 => () => typeof(AdornedElementPlaceholder), 
				3 => () => typeof(Adorner), 
				4 => () => typeof(AdornerDecorator), 
				5 => () => typeof(AdornerLayer), 
				6 => () => typeof(AffineTransform3D), 
				7 => () => typeof(AmbientLight), 
				8 => () => typeof(AnchoredBlock), 
				9 => () => typeof(Animatable), 
				10 => () => typeof(AnimationClock), 
				11 => () => typeof(AnimationTimeline), 
				12 => () => typeof(Application), 
				13 => () => typeof(ArcSegment), 
				14 => () => typeof(ArrayExtension), 
				15 => () => typeof(AxisAngleRotation3D), 
				16 => () => typeof(BaseIListConverter), 
				17 => () => typeof(BeginStoryboard), 
				18 => () => typeof(BevelBitmapEffect), 
				19 => () => typeof(BezierSegment), 
				20 => () => typeof(Binding), 
				21 => () => typeof(BindingBase), 
				22 => () => typeof(BindingExpression), 
				23 => () => typeof(BindingExpressionBase), 
				24 => () => typeof(BindingListCollectionView), 
				25 => () => typeof(BitmapDecoder), 
				26 => () => typeof(BitmapEffect), 
				27 => () => typeof(BitmapEffectCollection), 
				28 => () => typeof(BitmapEffectGroup), 
				29 => () => typeof(BitmapEffectInput), 
				30 => () => typeof(BitmapEncoder), 
				31 => () => typeof(BitmapFrame), 
				32 => () => typeof(BitmapImage), 
				33 => () => typeof(BitmapMetadata), 
				34 => () => typeof(BitmapPalette), 
				35 => () => typeof(BitmapSource), 
				36 => () => typeof(Block), 
				37 => () => typeof(BlockUIContainer), 
				38 => () => typeof(BlurBitmapEffect), 
				39 => () => typeof(BmpBitmapDecoder), 
				40 => () => typeof(BmpBitmapEncoder), 
				41 => () => typeof(Bold), 
				42 => () => typeof(BoolIListConverter), 
				43 => () => typeof(bool), 
				44 => () => typeof(BooleanAnimationBase), 
				45 => () => typeof(BooleanAnimationUsingKeyFrames), 
				46 => () => typeof(BooleanConverter), 
				47 => () => typeof(BooleanKeyFrame), 
				48 => () => typeof(BooleanKeyFrameCollection), 
				49 => () => typeof(BooleanToVisibilityConverter), 
				50 => () => typeof(Border), 
				51 => () => typeof(BorderGapMaskConverter), 
				52 => () => typeof(Brush), 
				53 => () => typeof(BrushConverter), 
				54 => () => typeof(BulletDecorator), 
				55 => () => typeof(Button), 
				56 => () => typeof(ButtonBase), 
				57 => () => typeof(byte), 
				58 => () => typeof(ByteAnimation), 
				59 => () => typeof(ByteAnimationBase), 
				60 => () => typeof(ByteAnimationUsingKeyFrames), 
				61 => () => typeof(ByteConverter), 
				62 => () => typeof(ByteKeyFrame), 
				63 => () => typeof(ByteKeyFrameCollection), 
				64 => () => typeof(CachedBitmap), 
				65 => () => typeof(Camera), 
				66 => () => typeof(Canvas), 
				67 => () => typeof(char), 
				68 => () => typeof(CharAnimationBase), 
				69 => () => typeof(CharAnimationUsingKeyFrames), 
				70 => () => typeof(CharConverter), 
				71 => () => typeof(CharIListConverter), 
				72 => () => typeof(CharKeyFrame), 
				73 => () => typeof(CharKeyFrameCollection), 
				74 => () => typeof(CheckBox), 
				75 => () => typeof(Clock), 
				76 => () => typeof(ClockController), 
				77 => () => typeof(ClockGroup), 
				78 => () => typeof(CollectionContainer), 
				79 => () => typeof(CollectionView), 
				80 => () => typeof(CollectionViewSource), 
				81 => () => typeof(Color), 
				82 => () => typeof(ColorAnimation), 
				83 => () => typeof(ColorAnimationBase), 
				84 => () => typeof(ColorAnimationUsingKeyFrames), 
				85 => () => typeof(ColorConvertedBitmap), 
				86 => () => typeof(ColorConvertedBitmapExtension), 
				87 => () => typeof(ColorConverter), 
				88 => () => typeof(ColorKeyFrame), 
				89 => () => typeof(ColorKeyFrameCollection), 
				90 => () => typeof(ColumnDefinition), 
				91 => () => typeof(CombinedGeometry), 
				92 => () => typeof(ComboBox), 
				93 => () => typeof(ComboBoxItem), 
				94 => () => typeof(CommandConverter), 
				95 => () => typeof(ComponentResourceKey), 
				96 => () => typeof(ComponentResourceKeyConverter), 
				97 => () => typeof(CompositionTarget), 
				98 => () => typeof(Condition), 
				99 => () => typeof(ContainerVisual), 
				100 => () => typeof(ContentControl), 
				101 => () => typeof(ContentElement), 
				102 => () => typeof(ContentPresenter), 
				103 => () => typeof(ContentPropertyAttribute), 
				104 => () => typeof(ContentWrapperAttribute), 
				105 => () => typeof(ContextMenu), 
				106 => () => typeof(ContextMenuService), 
				107 => () => typeof(Control), 
				108 => () => typeof(ControlTemplate), 
				109 => () => typeof(ControllableStoryboardAction), 
				110 => () => typeof(CornerRadius), 
				111 => () => typeof(CornerRadiusConverter), 
				112 => () => typeof(CroppedBitmap), 
				113 => () => typeof(CultureInfo), 
				114 => () => typeof(CultureInfoConverter), 
				115 => () => typeof(CultureInfoIetfLanguageTagConverter), 
				116 => () => typeof(Cursor), 
				117 => () => typeof(CursorConverter), 
				118 => () => typeof(DashStyle), 
				119 => () => typeof(DataChangedEventManager), 
				120 => () => typeof(DataTemplate), 
				121 => () => typeof(DataTemplateKey), 
				122 => () => typeof(DataTrigger), 
				123 => () => typeof(DateTime), 
				124 => () => typeof(DateTimeConverter), 
				125 => () => typeof(DateTimeConverter2), 
				126 => () => typeof(decimal), 
				127 => () => typeof(DecimalAnimation), 
				128 => () => typeof(DecimalAnimationBase), 
				129 => () => typeof(DecimalAnimationUsingKeyFrames), 
				130 => () => typeof(DecimalConverter), 
				131 => () => typeof(DecimalKeyFrame), 
				132 => () => typeof(DecimalKeyFrameCollection), 
				133 => () => typeof(Decorator), 
				134 => () => typeof(DefinitionBase), 
				135 => () => typeof(DependencyObject), 
				136 => () => typeof(DependencyProperty), 
				137 => () => typeof(DependencyPropertyConverter), 
				138 => () => typeof(DialogResultConverter), 
				139 => () => typeof(DiffuseMaterial), 
				140 => () => typeof(DirectionalLight), 
				141 => () => typeof(DiscreteBooleanKeyFrame), 
				142 => () => typeof(DiscreteByteKeyFrame), 
				143 => () => typeof(DiscreteCharKeyFrame), 
				144 => () => typeof(DiscreteColorKeyFrame), 
				145 => () => typeof(DiscreteDecimalKeyFrame), 
				146 => () => typeof(DiscreteDoubleKeyFrame), 
				147 => () => typeof(DiscreteInt16KeyFrame), 
				148 => () => typeof(DiscreteInt32KeyFrame), 
				149 => () => typeof(DiscreteInt64KeyFrame), 
				150 => () => typeof(DiscreteMatrixKeyFrame), 
				151 => () => typeof(DiscreteObjectKeyFrame), 
				152 => () => typeof(DiscretePoint3DKeyFrame), 
				153 => () => typeof(DiscretePointKeyFrame), 
				154 => () => typeof(DiscreteQuaternionKeyFrame), 
				155 => () => typeof(DiscreteRectKeyFrame), 
				156 => () => typeof(DiscreteRotation3DKeyFrame), 
				157 => () => typeof(DiscreteSingleKeyFrame), 
				158 => () => typeof(DiscreteSizeKeyFrame), 
				159 => () => typeof(DiscreteStringKeyFrame), 
				160 => () => typeof(DiscreteThicknessKeyFrame), 
				161 => () => typeof(DiscreteVector3DKeyFrame), 
				162 => () => typeof(DiscreteVectorKeyFrame), 
				163 => () => typeof(DockPanel), 
				164 => () => typeof(DocumentPageView), 
				165 => () => typeof(DocumentReference), 
				166 => () => typeof(DocumentViewer), 
				167 => () => typeof(DocumentViewerBase), 
				168 => () => typeof(double), 
				169 => () => typeof(DoubleAnimation), 
				170 => () => typeof(DoubleAnimationBase), 
				171 => () => typeof(DoubleAnimationUsingKeyFrames), 
				172 => () => typeof(DoubleAnimationUsingPath), 
				173 => () => typeof(DoubleCollection), 
				174 => () => typeof(DoubleCollectionConverter), 
				175 => () => typeof(DoubleConverter), 
				176 => () => typeof(DoubleIListConverter), 
				177 => () => typeof(DoubleKeyFrame), 
				178 => () => typeof(DoubleKeyFrameCollection), 
				179 => () => typeof(System.Windows.Media.Drawing), 
				180 => () => typeof(DrawingBrush), 
				181 => () => typeof(DrawingCollection), 
				182 => () => typeof(DrawingContext), 
				183 => () => typeof(DrawingGroup), 
				184 => () => typeof(DrawingImage), 
				185 => () => typeof(DrawingVisual), 
				186 => () => typeof(DropShadowBitmapEffect), 
				187 => () => typeof(Duration), 
				188 => () => typeof(DurationConverter), 
				189 => () => typeof(DynamicResourceExtension), 
				190 => () => typeof(DynamicResourceExtensionConverter), 
				191 => () => typeof(Ellipse), 
				192 => () => typeof(EllipseGeometry), 
				193 => () => typeof(EmbossBitmapEffect), 
				194 => () => typeof(EmissiveMaterial), 
				195 => () => typeof(EnumConverter), 
				196 => () => typeof(EventManager), 
				197 => () => typeof(EventSetter), 
				198 => () => typeof(EventTrigger), 
				199 => () => typeof(Expander), 
				200 => () => typeof(Expression), 
				201 => () => typeof(ExpressionConverter), 
				202 => () => typeof(Figure), 
				203 => () => typeof(FigureLength), 
				204 => () => typeof(FigureLengthConverter), 
				205 => () => typeof(FixedDocument), 
				206 => () => typeof(FixedDocumentSequence), 
				207 => () => typeof(FixedPage), 
				208 => () => typeof(Floater), 
				209 => () => typeof(FlowDocument), 
				210 => () => typeof(FlowDocumentPageViewer), 
				211 => () => typeof(FlowDocumentReader), 
				212 => () => typeof(FlowDocumentScrollViewer), 
				213 => () => typeof(FocusManager), 
				214 => () => typeof(FontFamily), 
				215 => () => typeof(FontFamilyConverter), 
				216 => () => typeof(FontSizeConverter), 
				217 => () => typeof(FontStretch), 
				218 => () => typeof(FontStretchConverter), 
				219 => () => typeof(FontStyle), 
				220 => () => typeof(FontStyleConverter), 
				221 => () => typeof(FontWeight), 
				222 => () => typeof(FontWeightConverter), 
				223 => () => typeof(FormatConvertedBitmap), 
				224 => () => typeof(Frame), 
				225 => () => typeof(FrameworkContentElement), 
				226 => () => typeof(FrameworkElement), 
				227 => () => typeof(FrameworkElementFactory), 
				228 => () => typeof(FrameworkPropertyMetadata), 
				229 => () => typeof(FrameworkPropertyMetadataOptions), 
				230 => () => typeof(FrameworkRichTextComposition), 
				231 => () => typeof(FrameworkTemplate), 
				232 => () => typeof(FrameworkTextComposition), 
				233 => () => typeof(Freezable), 
				234 => () => typeof(GeneralTransform), 
				235 => () => typeof(GeneralTransformCollection), 
				236 => () => typeof(GeneralTransformGroup), 
				237 => () => typeof(Geometry), 
				238 => () => typeof(Geometry3D), 
				239 => () => typeof(GeometryCollection), 
				240 => () => typeof(GeometryConverter), 
				241 => () => typeof(GeometryDrawing), 
				242 => () => typeof(GeometryGroup), 
				243 => () => typeof(GeometryModel3D), 
				244 => () => typeof(GestureRecognizer), 
				245 => () => typeof(GifBitmapDecoder), 
				246 => () => typeof(GifBitmapEncoder), 
				247 => () => typeof(GlyphRun), 
				248 => () => typeof(GlyphRunDrawing), 
				249 => () => typeof(GlyphTypeface), 
				250 => () => typeof(Glyphs), 
				251 => () => typeof(GradientBrush), 
				252 => () => typeof(GradientStop), 
				253 => () => typeof(GradientStopCollection), 
				254 => () => typeof(Grid), 
				255 => () => typeof(GridLength), 
				256 => () => typeof(GridLengthConverter), 
				257 => () => typeof(GridSplitter), 
				258 => () => typeof(GridView), 
				259 => () => typeof(GridViewColumn), 
				260 => () => typeof(GridViewColumnHeader), 
				261 => () => typeof(GridViewHeaderRowPresenter), 
				262 => () => typeof(GridViewRowPresenter), 
				263 => () => typeof(GridViewRowPresenterBase), 
				264 => () => typeof(GroupBox), 
				265 => () => typeof(GroupItem), 
				266 => () => typeof(Guid), 
				267 => () => typeof(GuidConverter), 
				268 => () => typeof(GuidelineSet), 
				269 => () => typeof(HeaderedContentControl), 
				270 => () => typeof(HeaderedItemsControl), 
				271 => () => typeof(HierarchicalDataTemplate), 
				272 => () => typeof(HostVisual), 
				273 => () => typeof(Hyperlink), 
				274 => () => typeof(IAddChild), 
				275 => () => typeof(IAddChildInternal), 
				276 => () => typeof(ICommand), 
				277 => () => typeof(IComponentConnector), 
				278 => () => typeof(INameScope), 
				279 => () => typeof(IStyleConnector), 
				280 => () => typeof(IconBitmapDecoder), 
				281 => () => typeof(Image), 
				282 => () => typeof(ImageBrush), 
				283 => () => typeof(ImageDrawing), 
				284 => () => typeof(ImageMetadata), 
				285 => () => typeof(ImageSource), 
				286 => () => typeof(ImageSourceConverter), 
				287 => () => typeof(InPlaceBitmapMetadataWriter), 
				288 => () => typeof(InkCanvas), 
				289 => () => typeof(InkPresenter), 
				290 => () => typeof(Inline), 
				291 => () => typeof(InlineCollection), 
				292 => () => typeof(InlineUIContainer), 
				293 => () => typeof(InputBinding), 
				294 => () => typeof(InputDevice), 
				295 => () => typeof(InputLanguageManager), 
				296 => () => typeof(InputManager), 
				297 => () => typeof(InputMethod), 
				298 => () => typeof(InputScope), 
				299 => () => typeof(InputScopeConverter), 
				300 => () => typeof(InputScopeName), 
				301 => () => typeof(InputScopeNameConverter), 
				302 => () => typeof(short), 
				303 => () => typeof(Int16Animation), 
				304 => () => typeof(Int16AnimationBase), 
				305 => () => typeof(Int16AnimationUsingKeyFrames), 
				306 => () => typeof(Int16Converter), 
				307 => () => typeof(Int16KeyFrame), 
				308 => () => typeof(Int16KeyFrameCollection), 
				309 => () => typeof(int), 
				310 => () => typeof(Int32Animation), 
				311 => () => typeof(Int32AnimationBase), 
				312 => () => typeof(Int32AnimationUsingKeyFrames), 
				313 => () => typeof(Int32Collection), 
				314 => () => typeof(Int32CollectionConverter), 
				315 => () => typeof(Int32Converter), 
				316 => () => typeof(Int32KeyFrame), 
				317 => () => typeof(Int32KeyFrameCollection), 
				318 => () => typeof(Int32Rect), 
				319 => () => typeof(Int32RectConverter), 
				320 => () => typeof(long), 
				321 => () => typeof(Int64Animation), 
				322 => () => typeof(Int64AnimationBase), 
				323 => () => typeof(Int64AnimationUsingKeyFrames), 
				324 => () => typeof(Int64Converter), 
				325 => () => typeof(Int64KeyFrame), 
				326 => () => typeof(Int64KeyFrameCollection), 
				327 => () => typeof(Italic), 
				328 => () => typeof(ItemCollection), 
				329 => () => typeof(ItemsControl), 
				330 => () => typeof(ItemsPanelTemplate), 
				331 => () => typeof(ItemsPresenter), 
				332 => () => typeof(JournalEntry), 
				333 => () => typeof(JournalEntryListConverter), 
				334 => () => typeof(JournalEntryUnifiedViewConverter), 
				335 => () => typeof(JpegBitmapDecoder), 
				336 => () => typeof(JpegBitmapEncoder), 
				337 => () => typeof(KeyBinding), 
				338 => () => typeof(KeyConverter), 
				339 => () => typeof(KeyGesture), 
				340 => () => typeof(KeyGestureConverter), 
				341 => () => typeof(KeySpline), 
				342 => () => typeof(KeySplineConverter), 
				343 => () => typeof(KeyTime), 
				344 => () => typeof(KeyTimeConverter), 
				345 => () => typeof(KeyboardDevice), 
				346 => () => typeof(Label), 
				347 => () => typeof(LateBoundBitmapDecoder), 
				348 => () => typeof(LengthConverter), 
				349 => () => typeof(Light), 
				350 => () => typeof(Line), 
				351 => () => typeof(LineBreak), 
				352 => () => typeof(LineGeometry), 
				353 => () => typeof(LineSegment), 
				354 => () => typeof(LinearByteKeyFrame), 
				355 => () => typeof(LinearColorKeyFrame), 
				356 => () => typeof(LinearDecimalKeyFrame), 
				357 => () => typeof(LinearDoubleKeyFrame), 
				358 => () => typeof(LinearGradientBrush), 
				359 => () => typeof(LinearInt16KeyFrame), 
				360 => () => typeof(LinearInt32KeyFrame), 
				361 => () => typeof(LinearInt64KeyFrame), 
				362 => () => typeof(LinearPoint3DKeyFrame), 
				363 => () => typeof(LinearPointKeyFrame), 
				364 => () => typeof(LinearQuaternionKeyFrame), 
				365 => () => typeof(LinearRectKeyFrame), 
				366 => () => typeof(LinearRotation3DKeyFrame), 
				367 => () => typeof(LinearSingleKeyFrame), 
				368 => () => typeof(LinearSizeKeyFrame), 
				369 => () => typeof(LinearThicknessKeyFrame), 
				370 => () => typeof(LinearVector3DKeyFrame), 
				371 => () => typeof(LinearVectorKeyFrame), 
				372 => () => typeof(List), 
				373 => () => typeof(ListBox), 
				374 => () => typeof(ListBoxItem), 
				375 => () => typeof(ListCollectionView), 
				376 => () => typeof(ListItem), 
				377 => () => typeof(ListView), 
				378 => () => typeof(ListViewItem), 
				379 => () => typeof(Localization), 
				380 => () => typeof(LostFocusEventManager), 
				381 => () => typeof(MarkupExtension), 
				382 => () => typeof(Material), 
				383 => () => typeof(MaterialCollection), 
				384 => () => typeof(MaterialGroup), 
				385 => () => typeof(Matrix), 
				386 => () => typeof(Matrix3D), 
				387 => () => typeof(Matrix3DConverter), 
				388 => () => typeof(MatrixAnimationBase), 
				389 => () => typeof(MatrixAnimationUsingKeyFrames), 
				390 => () => typeof(MatrixAnimationUsingPath), 
				391 => () => typeof(MatrixCamera), 
				392 => () => typeof(MatrixConverter), 
				393 => () => typeof(MatrixKeyFrame), 
				394 => () => typeof(MatrixKeyFrameCollection), 
				395 => () => typeof(MatrixTransform), 
				396 => () => typeof(MatrixTransform3D), 
				397 => () => typeof(MediaClock), 
				398 => () => typeof(MediaElement), 
				399 => () => typeof(MediaPlayer), 
				400 => () => typeof(MediaTimeline), 
				401 => () => typeof(Menu), 
				402 => () => typeof(MenuBase), 
				403 => () => typeof(MenuItem), 
				404 => () => typeof(MenuScrollingVisibilityConverter), 
				405 => () => typeof(MeshGeometry3D), 
				406 => () => typeof(Model3D), 
				407 => () => typeof(Model3DCollection), 
				408 => () => typeof(Model3DGroup), 
				409 => () => typeof(ModelVisual3D), 
				410 => () => typeof(ModifierKeysConverter), 
				411 => () => typeof(MouseActionConverter), 
				412 => () => typeof(MouseBinding), 
				413 => () => typeof(MouseDevice), 
				414 => () => typeof(MouseGesture), 
				415 => () => typeof(MouseGestureConverter), 
				416 => () => typeof(MultiBinding), 
				417 => () => typeof(MultiBindingExpression), 
				418 => () => typeof(MultiDataTrigger), 
				419 => () => typeof(MultiTrigger), 
				420 => () => typeof(NameScope), 
				421 => () => typeof(NavigationWindow), 
				422 => () => typeof(NullExtension), 
				423 => () => typeof(NullableBoolConverter), 
				424 => () => typeof(NullableConverter), 
				425 => () => typeof(NumberSubstitution), 
				426 => () => typeof(object), 
				427 => () => typeof(ObjectAnimationBase), 
				428 => () => typeof(ObjectAnimationUsingKeyFrames), 
				429 => () => typeof(ObjectDataProvider), 
				430 => () => typeof(ObjectKeyFrame), 
				431 => () => typeof(ObjectKeyFrameCollection), 
				432 => () => typeof(OrthographicCamera), 
				433 => () => typeof(OuterGlowBitmapEffect), 
				434 => () => typeof(Page), 
				435 => () => typeof(PageContent), 
				436 => () => typeof(PageFunctionBase), 
				437 => () => typeof(Panel), 
				438 => () => typeof(Paragraph), 
				439 => () => typeof(ParallelTimeline), 
				440 => () => typeof(ParserContext), 
				441 => () => typeof(PasswordBox), 
				442 => () => typeof(Path), 
				443 => () => typeof(PathFigure), 
				444 => () => typeof(PathFigureCollection), 
				445 => () => typeof(PathFigureCollectionConverter), 
				446 => () => typeof(PathGeometry), 
				447 => () => typeof(PathSegment), 
				448 => () => typeof(PathSegmentCollection), 
				449 => () => typeof(PauseStoryboard), 
				450 => () => typeof(Pen), 
				451 => () => typeof(PerspectiveCamera), 
				452 => () => typeof(PixelFormat), 
				453 => () => typeof(PixelFormatConverter), 
				454 => () => typeof(PngBitmapDecoder), 
				455 => () => typeof(PngBitmapEncoder), 
				456 => () => typeof(Point), 
				457 => () => typeof(Point3D), 
				458 => () => typeof(Point3DAnimation), 
				459 => () => typeof(Point3DAnimationBase), 
				460 => () => typeof(Point3DAnimationUsingKeyFrames), 
				461 => () => typeof(Point3DCollection), 
				462 => () => typeof(Point3DCollectionConverter), 
				463 => () => typeof(Point3DConverter), 
				464 => () => typeof(Point3DKeyFrame), 
				465 => () => typeof(Point3DKeyFrameCollection), 
				466 => () => typeof(Point4D), 
				467 => () => typeof(Point4DConverter), 
				468 => () => typeof(PointAnimation), 
				469 => () => typeof(PointAnimationBase), 
				470 => () => typeof(PointAnimationUsingKeyFrames), 
				471 => () => typeof(PointAnimationUsingPath), 
				472 => () => typeof(PointCollection), 
				473 => () => typeof(PointCollectionConverter), 
				474 => () => typeof(PointConverter), 
				475 => () => typeof(PointIListConverter), 
				476 => () => typeof(PointKeyFrame), 
				477 => () => typeof(PointKeyFrameCollection), 
				478 => () => typeof(PointLight), 
				479 => () => typeof(PointLightBase), 
				480 => () => typeof(PolyBezierSegment), 
				481 => () => typeof(PolyLineSegment), 
				482 => () => typeof(PolyQuadraticBezierSegment), 
				483 => () => typeof(Polygon), 
				484 => () => typeof(Polyline), 
				485 => () => typeof(Popup), 
				486 => () => typeof(PresentationSource), 
				487 => () => typeof(PriorityBinding), 
				488 => () => typeof(PriorityBindingExpression), 
				489 => () => typeof(ProgressBar), 
				490 => () => typeof(ProjectionCamera), 
				491 => () => typeof(PropertyPath), 
				492 => () => typeof(PropertyPathConverter), 
				493 => () => typeof(QuadraticBezierSegment), 
				494 => () => typeof(Quaternion), 
				495 => () => typeof(QuaternionAnimation), 
				496 => () => typeof(QuaternionAnimationBase), 
				497 => () => typeof(QuaternionAnimationUsingKeyFrames), 
				498 => () => typeof(QuaternionConverter), 
				499 => () => typeof(QuaternionKeyFrame), 
				500 => () => typeof(QuaternionKeyFrameCollection), 
				501 => () => typeof(QuaternionRotation3D), 
				502 => () => typeof(RadialGradientBrush), 
				503 => () => typeof(RadioButton), 
				504 => () => typeof(RangeBase), 
				505 => () => typeof(Rect), 
				506 => () => typeof(Rect3D), 
				507 => () => typeof(Rect3DConverter), 
				508 => () => typeof(RectAnimation), 
				509 => () => typeof(RectAnimationBase), 
				510 => () => typeof(RectAnimationUsingKeyFrames), 
				511 => () => typeof(RectConverter), 
				512 => () => typeof(RectKeyFrame), 
				513 => () => typeof(RectKeyFrameCollection), 
				514 => () => typeof(Rectangle), 
				515 => () => typeof(RectangleGeometry), 
				516 => () => typeof(RelativeSource), 
				517 => () => typeof(RemoveStoryboard), 
				518 => () => typeof(RenderOptions), 
				519 => () => typeof(RenderTargetBitmap), 
				520 => () => typeof(RepeatBehavior), 
				521 => () => typeof(RepeatBehaviorConverter), 
				522 => () => typeof(RepeatButton), 
				523 => () => typeof(ResizeGrip), 
				524 => () => typeof(ResourceDictionary), 
				525 => () => typeof(ResourceKey), 
				526 => () => typeof(ResumeStoryboard), 
				527 => () => typeof(RichTextBox), 
				528 => () => typeof(RotateTransform), 
				529 => () => typeof(RotateTransform3D), 
				530 => () => typeof(Rotation3D), 
				531 => () => typeof(Rotation3DAnimation), 
				532 => () => typeof(Rotation3DAnimationBase), 
				533 => () => typeof(Rotation3DAnimationUsingKeyFrames), 
				534 => () => typeof(Rotation3DKeyFrame), 
				535 => () => typeof(Rotation3DKeyFrameCollection), 
				536 => () => typeof(RoutedCommand), 
				537 => () => typeof(RoutedEvent), 
				538 => () => typeof(RoutedEventConverter), 
				539 => () => typeof(RoutedUICommand), 
				540 => () => typeof(RoutingStrategy), 
				541 => () => typeof(RowDefinition), 
				542 => () => typeof(Run), 
				543 => () => typeof(RuntimeNamePropertyAttribute), 
				544 => () => typeof(sbyte), 
				545 => () => typeof(SByteConverter), 
				546 => () => typeof(ScaleTransform), 
				547 => () => typeof(ScaleTransform3D), 
				548 => () => typeof(ScrollBar), 
				549 => () => typeof(ScrollContentPresenter), 
				550 => () => typeof(ScrollViewer), 
				551 => () => typeof(Section), 
				552 => () => typeof(SeekStoryboard), 
				553 => () => typeof(Selector), 
				554 => () => typeof(Separator), 
				555 => () => typeof(SetStoryboardSpeedRatio), 
				556 => () => typeof(Setter), 
				557 => () => typeof(SetterBase), 
				558 => () => typeof(Shape), 
				559 => () => typeof(float), 
				560 => () => typeof(SingleAnimation), 
				561 => () => typeof(SingleAnimationBase), 
				562 => () => typeof(SingleAnimationUsingKeyFrames), 
				563 => () => typeof(SingleConverter), 
				564 => () => typeof(SingleKeyFrame), 
				565 => () => typeof(SingleKeyFrameCollection), 
				566 => () => typeof(Size), 
				567 => () => typeof(Size3D), 
				568 => () => typeof(Size3DConverter), 
				569 => () => typeof(SizeAnimation), 
				570 => () => typeof(SizeAnimationBase), 
				571 => () => typeof(SizeAnimationUsingKeyFrames), 
				572 => () => typeof(SizeConverter), 
				573 => () => typeof(SizeKeyFrame), 
				574 => () => typeof(SizeKeyFrameCollection), 
				575 => () => typeof(SkewTransform), 
				576 => () => typeof(SkipStoryboardToFill), 
				577 => () => typeof(Slider), 
				578 => () => typeof(SolidColorBrush), 
				579 => () => typeof(SoundPlayerAction), 
				580 => () => typeof(Span), 
				581 => () => typeof(SpecularMaterial), 
				582 => () => typeof(SpellCheck), 
				583 => () => typeof(SplineByteKeyFrame), 
				584 => () => typeof(SplineColorKeyFrame), 
				585 => () => typeof(SplineDecimalKeyFrame), 
				586 => () => typeof(SplineDoubleKeyFrame), 
				587 => () => typeof(SplineInt16KeyFrame), 
				588 => () => typeof(SplineInt32KeyFrame), 
				589 => () => typeof(SplineInt64KeyFrame), 
				590 => () => typeof(SplinePoint3DKeyFrame), 
				591 => () => typeof(SplinePointKeyFrame), 
				592 => () => typeof(SplineQuaternionKeyFrame), 
				593 => () => typeof(SplineRectKeyFrame), 
				594 => () => typeof(SplineRotation3DKeyFrame), 
				595 => () => typeof(SplineSingleKeyFrame), 
				596 => () => typeof(SplineSizeKeyFrame), 
				597 => () => typeof(SplineThicknessKeyFrame), 
				598 => () => typeof(SplineVector3DKeyFrame), 
				599 => () => typeof(SplineVectorKeyFrame), 
				600 => () => typeof(SpotLight), 
				601 => () => typeof(StackPanel), 
				602 => () => typeof(StaticExtension), 
				603 => () => typeof(StaticResourceExtension), 
				604 => () => typeof(StatusBar), 
				605 => () => typeof(StatusBarItem), 
				606 => () => typeof(StickyNoteControl), 
				607 => () => typeof(StopStoryboard), 
				608 => () => typeof(Storyboard), 
				609 => () => typeof(StreamGeometry), 
				610 => () => typeof(StreamGeometryContext), 
				611 => () => typeof(StreamResourceInfo), 
				612 => () => typeof(string), 
				613 => () => typeof(StringAnimationBase), 
				614 => () => typeof(StringAnimationUsingKeyFrames), 
				615 => () => typeof(StringConverter), 
				616 => () => typeof(StringKeyFrame), 
				617 => () => typeof(StringKeyFrameCollection), 
				618 => () => typeof(StrokeCollection), 
				619 => () => typeof(StrokeCollectionConverter), 
				620 => () => typeof(Style), 
				621 => () => typeof(Stylus), 
				622 => () => typeof(StylusDevice), 
				623 => () => typeof(TabControl), 
				624 => () => typeof(TabItem), 
				625 => () => typeof(TabPanel), 
				626 => () => typeof(Table), 
				627 => () => typeof(TableCell), 
				628 => () => typeof(TableColumn), 
				629 => () => typeof(TableRow), 
				630 => () => typeof(TableRowGroup), 
				631 => () => typeof(TabletDevice), 
				632 => () => typeof(TemplateBindingExpression), 
				633 => () => typeof(TemplateBindingExpressionConverter), 
				634 => () => typeof(TemplateBindingExtension), 
				635 => () => typeof(TemplateBindingExtensionConverter), 
				636 => () => typeof(TemplateKey), 
				637 => () => typeof(TemplateKeyConverter), 
				638 => () => typeof(TextBlock), 
				639 => () => typeof(TextBox), 
				640 => () => typeof(TextBoxBase), 
				641 => () => typeof(TextComposition), 
				642 => () => typeof(TextCompositionManager), 
				643 => () => typeof(TextDecoration), 
				644 => () => typeof(TextDecorationCollection), 
				645 => () => typeof(TextDecorationCollectionConverter), 
				646 => () => typeof(TextEffect), 
				647 => () => typeof(TextEffectCollection), 
				648 => () => typeof(TextElement), 
				649 => () => typeof(TextSearch), 
				650 => () => typeof(ThemeDictionaryExtension), 
				651 => () => typeof(Thickness), 
				652 => () => typeof(ThicknessAnimation), 
				653 => () => typeof(ThicknessAnimationBase), 
				654 => () => typeof(ThicknessAnimationUsingKeyFrames), 
				655 => () => typeof(ThicknessConverter), 
				656 => () => typeof(ThicknessKeyFrame), 
				657 => () => typeof(ThicknessKeyFrameCollection), 
				658 => () => typeof(Thumb), 
				659 => () => typeof(TickBar), 
				660 => () => typeof(TiffBitmapDecoder), 
				661 => () => typeof(TiffBitmapEncoder), 
				662 => () => typeof(TileBrush), 
				663 => () => typeof(TimeSpan), 
				664 => () => typeof(TimeSpanConverter), 
				665 => () => typeof(Timeline), 
				666 => () => typeof(TimelineCollection), 
				667 => () => typeof(TimelineGroup), 
				668 => () => typeof(ToggleButton), 
				669 => () => typeof(ToolBar), 
				670 => () => typeof(ToolBarOverflowPanel), 
				671 => () => typeof(ToolBarPanel), 
				672 => () => typeof(ToolBarTray), 
				673 => () => typeof(ToolTip), 
				674 => () => typeof(ToolTipService), 
				675 => () => typeof(Track), 
				676 => () => typeof(Transform), 
				677 => () => typeof(Transform3D), 
				678 => () => typeof(Transform3DCollection), 
				679 => () => typeof(Transform3DGroup), 
				680 => () => typeof(TransformCollection), 
				681 => () => typeof(TransformConverter), 
				682 => () => typeof(TransformGroup), 
				683 => () => typeof(TransformedBitmap), 
				684 => () => typeof(TranslateTransform), 
				685 => () => typeof(TranslateTransform3D), 
				686 => () => typeof(TreeView), 
				687 => () => typeof(TreeViewItem), 
				688 => () => typeof(Trigger), 
				689 => () => typeof(TriggerAction), 
				690 => () => typeof(TriggerBase), 
				691 => () => typeof(TypeExtension), 
				692 => () => typeof(TypeTypeConverter), 
				693 => () => typeof(Typography), 
				694 => () => typeof(UIElement), 
				695 => () => typeof(ushort), 
				696 => () => typeof(UInt16Converter), 
				697 => () => typeof(uint), 
				698 => () => typeof(UInt32Converter), 
				699 => () => typeof(ulong), 
				700 => () => typeof(UInt64Converter), 
				701 => () => typeof(UShortIListConverter), 
				702 => () => typeof(Underline), 
				703 => () => typeof(UniformGrid), 
				704 => () => typeof(Uri), 
				705 => () => typeof(UriTypeConverter), 
				706 => () => typeof(UserControl), 
				707 => () => typeof(Validation), 
				708 => () => typeof(Vector), 
				709 => () => typeof(Vector3D), 
				710 => () => typeof(Vector3DAnimation), 
				711 => () => typeof(Vector3DAnimationBase), 
				712 => () => typeof(Vector3DAnimationUsingKeyFrames), 
				713 => () => typeof(Vector3DCollection), 
				714 => () => typeof(Vector3DCollectionConverter), 
				715 => () => typeof(Vector3DConverter), 
				716 => () => typeof(Vector3DKeyFrame), 
				717 => () => typeof(Vector3DKeyFrameCollection), 
				718 => () => typeof(VectorAnimation), 
				719 => () => typeof(VectorAnimationBase), 
				720 => () => typeof(VectorAnimationUsingKeyFrames), 
				721 => () => typeof(VectorCollection), 
				722 => () => typeof(VectorCollectionConverter), 
				723 => () => typeof(VectorConverter), 
				724 => () => typeof(VectorKeyFrame), 
				725 => () => typeof(VectorKeyFrameCollection), 
				726 => () => typeof(VideoDrawing), 
				727 => () => typeof(ViewBase), 
				728 => () => typeof(Viewbox), 
				729 => () => typeof(Viewport3D), 
				730 => () => typeof(Viewport3DVisual), 
				731 => () => typeof(VirtualizingPanel), 
				732 => () => typeof(VirtualizingStackPanel), 
				733 => () => typeof(Visual), 
				734 => () => typeof(Visual3D), 
				735 => () => typeof(VisualBrush), 
				736 => () => typeof(VisualTarget), 
				737 => () => typeof(WeakEventManager), 
				738 => () => typeof(WhitespaceSignificantCollectionAttribute), 
				739 => () => typeof(Window), 
				740 => () => typeof(WmpBitmapDecoder), 
				741 => () => typeof(WmpBitmapEncoder), 
				742 => () => typeof(WrapPanel), 
				743 => () => typeof(WriteableBitmap), 
				744 => () => typeof(XamlBrushSerializer), 
				745 => () => typeof(XamlInt32CollectionSerializer), 
				746 => () => typeof(XamlPathDataSerializer), 
				747 => () => typeof(XamlPoint3DCollectionSerializer), 
				748 => () => typeof(XamlPointCollectionSerializer), 
				749 => () => typeof(System.Windows.Markup.XamlReader), 
				750 => () => typeof(XamlStyleSerializer), 
				751 => () => typeof(XamlTemplateSerializer), 
				752 => () => typeof(XamlVector3DCollectionSerializer), 
				753 => () => typeof(System.Windows.Markup.XamlWriter), 
				754 => () => typeof(XmlDataProvider), 
				755 => () => typeof(XmlLangPropertyAttribute), 
				756 => () => typeof(XmlLanguage), 
				757 => () => typeof(XmlLanguageConverter), 
				758 => () => typeof(XmlNamespaceMapping), 
				759 => () => typeof(ZoomPercentageConverter), 
				_ => () => (Type)null, 
			})();
		}

		internal static TypeConverter CreateKnownTypeConverter(short converterId)
		{
			TypeConverter result = null;
			switch (converterId)
			{
			case -42:
				result = new BoolIListConverter();
				break;
			case -46:
				result = new BooleanConverter();
				break;
			case -53:
				result = new BrushConverter();
				break;
			case -61:
				result = new ByteConverter();
				break;
			case -70:
				result = new CharConverter();
				break;
			case -71:
				result = new CharIListConverter();
				break;
			case -87:
				result = new ColorConverter();
				break;
			case -94:
				result = new CommandConverter();
				break;
			case -96:
				result = new ComponentResourceKeyConverter();
				break;
			case -111:
				result = new CornerRadiusConverter();
				break;
			case -114:
				result = new CultureInfoConverter();
				break;
			case -115:
				result = new CultureInfoIetfLanguageTagConverter();
				break;
			case -117:
				result = new CursorConverter();
				break;
			case -124:
				result = new DateTimeConverter();
				break;
			case -125:
				result = new DateTimeConverter2();
				break;
			case -130:
				result = new DecimalConverter();
				break;
			case -137:
				result = new DependencyPropertyConverter();
				break;
			case -138:
				result = new DialogResultConverter();
				break;
			case -174:
				result = new DoubleCollectionConverter();
				break;
			case -175:
				result = new DoubleConverter();
				break;
			case -176:
				result = new DoubleIListConverter();
				break;
			case -188:
				result = new DurationConverter();
				break;
			case -190:
				result = new DynamicResourceExtensionConverter();
				break;
			case -201:
				result = new ExpressionConverter();
				break;
			case -204:
				result = new FigureLengthConverter();
				break;
			case -215:
				result = new FontFamilyConverter();
				break;
			case -216:
				result = new FontSizeConverter();
				break;
			case -218:
				result = new FontStretchConverter();
				break;
			case -220:
				result = new FontStyleConverter();
				break;
			case -222:
				result = new FontWeightConverter();
				break;
			case -240:
				result = new GeometryConverter();
				break;
			case -256:
				result = new GridLengthConverter();
				break;
			case -267:
				result = new GuidConverter();
				break;
			case -286:
				result = new ImageSourceConverter();
				break;
			case -299:
				result = new InputScopeConverter();
				break;
			case -301:
				result = new InputScopeNameConverter();
				break;
			case -306:
				result = new Int16Converter();
				break;
			case -314:
				result = new Int32CollectionConverter();
				break;
			case -315:
				result = new Int32Converter();
				break;
			case -319:
				result = new Int32RectConverter();
				break;
			case -324:
				result = new Int64Converter();
				break;
			case -338:
				result = new KeyConverter();
				break;
			case -340:
				result = new KeyGestureConverter();
				break;
			case -342:
				result = new KeySplineConverter();
				break;
			case -344:
				result = new KeyTimeConverter();
				break;
			case -348:
				result = new LengthConverter();
				break;
			case -387:
				result = new Matrix3DConverter();
				break;
			case -392:
				result = new MatrixConverter();
				break;
			case -410:
				result = new ModifierKeysConverter();
				break;
			case -411:
				result = new MouseActionConverter();
				break;
			case -415:
				result = new MouseGestureConverter();
				break;
			case -423:
				result = new NullableBoolConverter();
				break;
			case -445:
				result = new PathFigureCollectionConverter();
				break;
			case -453:
				result = new PixelFormatConverter();
				break;
			case -462:
				result = new Point3DCollectionConverter();
				break;
			case -463:
				result = new Point3DConverter();
				break;
			case -467:
				result = new Point4DConverter();
				break;
			case -473:
				result = new PointCollectionConverter();
				break;
			case -474:
				result = new PointConverter();
				break;
			case -475:
				result = new PointIListConverter();
				break;
			case -492:
				result = new PropertyPathConverter();
				break;
			case -498:
				result = new QuaternionConverter();
				break;
			case -507:
				result = new Rect3DConverter();
				break;
			case -511:
				result = new RectConverter();
				break;
			case -521:
				result = new RepeatBehaviorConverter();
				break;
			case -538:
				result = new RoutedEventConverter();
				break;
			case -545:
				result = new SByteConverter();
				break;
			case -563:
				result = new SingleConverter();
				break;
			case -568:
				result = new Size3DConverter();
				break;
			case -572:
				result = new SizeConverter();
				break;
			case -615:
				result = new StringConverter();
				break;
			case -619:
				result = new StrokeCollectionConverter();
				break;
			case -633:
				result = new TemplateBindingExpressionConverter();
				break;
			case -635:
				result = new TemplateBindingExtensionConverter();
				break;
			case -637:
				result = new TemplateKeyConverter();
				break;
			case -645:
				result = new TextDecorationCollectionConverter();
				break;
			case -655:
				result = new ThicknessConverter();
				break;
			case -664:
				result = new TimeSpanConverter();
				break;
			case -681:
				result = new TransformConverter();
				break;
			case -692:
				result = new TypeTypeConverter();
				break;
			case -696:
				result = new UInt16Converter();
				break;
			case -698:
				result = new UInt32Converter();
				break;
			case -700:
				result = new UInt64Converter();
				break;
			case -701:
				result = new UShortIListConverter();
				break;
			case -705:
				result = new UriTypeConverter();
				break;
			case -714:
				result = new Vector3DCollectionConverter();
				break;
			case -715:
				result = new Vector3DConverter();
				break;
			case -722:
				result = new VectorCollectionConverter();
				break;
			case -723:
				result = new VectorConverter();
				break;
			case -757:
				result = new XmlLanguageConverter();
				break;
			}
			return result;
		}

		public static bool GetKnownProperty(short propertyId, out short typeId, out string propertyName)
		{
			switch (propertyId)
			{
			case -1:
				typeId = -1;
				propertyName = "Text";
				break;
			case -2:
				typeId = -17;
				propertyName = "Storyboard";
				break;
			case -3:
				typeId = -28;
				propertyName = "Children";
				break;
			case -4:
				typeId = -50;
				propertyName = "Background";
				break;
			case -5:
				typeId = -50;
				propertyName = "BorderBrush";
				break;
			case -6:
				typeId = -50;
				propertyName = "BorderThickness";
				break;
			case -7:
				typeId = -56;
				propertyName = "Command";
				break;
			case -8:
				typeId = -56;
				propertyName = "CommandParameter";
				break;
			case -9:
				typeId = -56;
				propertyName = "CommandTarget";
				break;
			case -10:
				typeId = -56;
				propertyName = "IsPressed";
				break;
			case -11:
				typeId = -90;
				propertyName = "MaxWidth";
				break;
			case -12:
				typeId = -90;
				propertyName = "MinWidth";
				break;
			case -13:
				typeId = -90;
				propertyName = "Width";
				break;
			case -14:
				typeId = -100;
				propertyName = "Content";
				break;
			case -15:
				typeId = -100;
				propertyName = "ContentTemplate";
				break;
			case -16:
				typeId = -100;
				propertyName = "ContentTemplateSelector";
				break;
			case -17:
				typeId = -100;
				propertyName = "HasContent";
				break;
			case -18:
				typeId = -101;
				propertyName = "Focusable";
				break;
			case -19:
				typeId = -102;
				propertyName = "Content";
				break;
			case -20:
				typeId = -102;
				propertyName = "ContentSource";
				break;
			case -21:
				typeId = -102;
				propertyName = "ContentTemplate";
				break;
			case -22:
				typeId = -102;
				propertyName = "ContentTemplateSelector";
				break;
			case -23:
				typeId = -102;
				propertyName = "RecognizesAccessKey";
				break;
			case -24:
				typeId = -107;
				propertyName = "Background";
				break;
			case -25:
				typeId = -107;
				propertyName = "BorderBrush";
				break;
			case -26:
				typeId = -107;
				propertyName = "BorderThickness";
				break;
			case -27:
				typeId = -107;
				propertyName = "FontFamily";
				break;
			case -28:
				typeId = -107;
				propertyName = "FontSize";
				break;
			case -29:
				typeId = -107;
				propertyName = "FontStretch";
				break;
			case -30:
				typeId = -107;
				propertyName = "FontStyle";
				break;
			case -31:
				typeId = -107;
				propertyName = "FontWeight";
				break;
			case -32:
				typeId = -107;
				propertyName = "Foreground";
				break;
			case -33:
				typeId = -107;
				propertyName = "HorizontalContentAlignment";
				break;
			case -34:
				typeId = -107;
				propertyName = "IsTabStop";
				break;
			case -35:
				typeId = -107;
				propertyName = "Padding";
				break;
			case -36:
				typeId = -107;
				propertyName = "TabIndex";
				break;
			case -37:
				typeId = -107;
				propertyName = "Template";
				break;
			case -38:
				typeId = -107;
				propertyName = "VerticalContentAlignment";
				break;
			case -39:
				typeId = -163;
				propertyName = "Dock";
				break;
			case -40:
				typeId = -163;
				propertyName = "LastChildFill";
				break;
			case -41:
				typeId = -167;
				propertyName = "Document";
				break;
			case -42:
				typeId = -183;
				propertyName = "Children";
				break;
			case -43:
				typeId = -211;
				propertyName = "Document";
				break;
			case -44:
				typeId = -212;
				propertyName = "Document";
				break;
			case -45:
				typeId = -225;
				propertyName = "Style";
				break;
			case -46:
				typeId = -226;
				propertyName = "FlowDirection";
				break;
			case -47:
				typeId = -226;
				propertyName = "Height";
				break;
			case -48:
				typeId = -226;
				propertyName = "HorizontalAlignment";
				break;
			case -49:
				typeId = -226;
				propertyName = "Margin";
				break;
			case -50:
				typeId = -226;
				propertyName = "MaxHeight";
				break;
			case -51:
				typeId = -226;
				propertyName = "MaxWidth";
				break;
			case -52:
				typeId = -226;
				propertyName = "MinHeight";
				break;
			case -53:
				typeId = -226;
				propertyName = "MinWidth";
				break;
			case -54:
				typeId = -226;
				propertyName = "Name";
				break;
			case -55:
				typeId = -226;
				propertyName = "Style";
				break;
			case -56:
				typeId = -226;
				propertyName = "VerticalAlignment";
				break;
			case -57:
				typeId = -226;
				propertyName = "Width";
				break;
			case -58:
				typeId = -236;
				propertyName = "Children";
				break;
			case -59:
				typeId = -242;
				propertyName = "Children";
				break;
			case -60:
				typeId = -251;
				propertyName = "GradientStops";
				break;
			case -61:
				typeId = -254;
				propertyName = "Column";
				break;
			case -62:
				typeId = -254;
				propertyName = "ColumnSpan";
				break;
			case -63:
				typeId = -254;
				propertyName = "Row";
				break;
			case -64:
				typeId = -254;
				propertyName = "RowSpan";
				break;
			case -65:
				typeId = -259;
				propertyName = "Header";
				break;
			case -66:
				typeId = -269;
				propertyName = "HasHeader";
				break;
			case -67:
				typeId = -269;
				propertyName = "Header";
				break;
			case -68:
				typeId = -269;
				propertyName = "HeaderTemplate";
				break;
			case -69:
				typeId = -269;
				propertyName = "HeaderTemplateSelector";
				break;
			case -70:
				typeId = -270;
				propertyName = "HasHeader";
				break;
			case -71:
				typeId = -270;
				propertyName = "Header";
				break;
			case -72:
				typeId = -270;
				propertyName = "HeaderTemplate";
				break;
			case -73:
				typeId = -270;
				propertyName = "HeaderTemplateSelector";
				break;
			case -74:
				typeId = -273;
				propertyName = "NavigateUri";
				break;
			case -75:
				typeId = -281;
				propertyName = "Source";
				break;
			case -76:
				typeId = -281;
				propertyName = "Stretch";
				break;
			case -77:
				typeId = -329;
				propertyName = "ItemContainerStyle";
				break;
			case -78:
				typeId = -329;
				propertyName = "ItemContainerStyleSelector";
				break;
			case -79:
				typeId = -329;
				propertyName = "ItemTemplate";
				break;
			case -80:
				typeId = -329;
				propertyName = "ItemTemplateSelector";
				break;
			case -81:
				typeId = -329;
				propertyName = "ItemsPanel";
				break;
			case -82:
				typeId = -329;
				propertyName = "ItemsSource";
				break;
			case -83:
				typeId = -384;
				propertyName = "Children";
				break;
			case -84:
				typeId = -408;
				propertyName = "Children";
				break;
			case -85:
				typeId = -434;
				propertyName = "Content";
				break;
			case -86:
				typeId = -437;
				propertyName = "Background";
				break;
			case -87:
				typeId = -442;
				propertyName = "Data";
				break;
			case -88:
				typeId = -443;
				propertyName = "Segments";
				break;
			case -89:
				typeId = -446;
				propertyName = "Figures";
				break;
			case -90:
				typeId = -485;
				propertyName = "Child";
				break;
			case -91:
				typeId = -485;
				propertyName = "IsOpen";
				break;
			case -92:
				typeId = -485;
				propertyName = "Placement";
				break;
			case -93:
				typeId = -485;
				propertyName = "PopupAnimation";
				break;
			case -94:
				typeId = -541;
				propertyName = "Height";
				break;
			case -95:
				typeId = -541;
				propertyName = "MaxHeight";
				break;
			case -96:
				typeId = -541;
				propertyName = "MinHeight";
				break;
			case -97:
				typeId = -550;
				propertyName = "CanContentScroll";
				break;
			case -98:
				typeId = -550;
				propertyName = "HorizontalScrollBarVisibility";
				break;
			case -99:
				typeId = -550;
				propertyName = "VerticalScrollBarVisibility";
				break;
			case -100:
				typeId = -558;
				propertyName = "Fill";
				break;
			case -101:
				typeId = -558;
				propertyName = "Stroke";
				break;
			case -102:
				typeId = -558;
				propertyName = "StrokeThickness";
				break;
			case -103:
				typeId = -638;
				propertyName = "Background";
				break;
			case -104:
				typeId = -638;
				propertyName = "FontFamily";
				break;
			case -105:
				typeId = -638;
				propertyName = "FontSize";
				break;
			case -106:
				typeId = -638;
				propertyName = "FontStretch";
				break;
			case -107:
				typeId = -638;
				propertyName = "FontStyle";
				break;
			case -108:
				typeId = -638;
				propertyName = "FontWeight";
				break;
			case -109:
				typeId = -638;
				propertyName = "Foreground";
				break;
			case -110:
				typeId = -638;
				propertyName = "Text";
				break;
			case -111:
				typeId = -638;
				propertyName = "TextDecorations";
				break;
			case -112:
				typeId = -638;
				propertyName = "TextTrimming";
				break;
			case -113:
				typeId = -638;
				propertyName = "TextWrapping";
				break;
			case -114:
				typeId = -639;
				propertyName = "Text";
				break;
			case -115:
				typeId = -648;
				propertyName = "Background";
				break;
			case -116:
				typeId = -648;
				propertyName = "FontFamily";
				break;
			case -117:
				typeId = -648;
				propertyName = "FontSize";
				break;
			case -118:
				typeId = -648;
				propertyName = "FontStretch";
				break;
			case -119:
				typeId = -648;
				propertyName = "FontStyle";
				break;
			case -120:
				typeId = -648;
				propertyName = "FontWeight";
				break;
			case -121:
				typeId = -648;
				propertyName = "Foreground";
				break;
			case -122:
				typeId = -667;
				propertyName = "Children";
				break;
			case -123:
				typeId = -675;
				propertyName = "IsDirectionReversed";
				break;
			case -124:
				typeId = -675;
				propertyName = "Maximum";
				break;
			case -125:
				typeId = -675;
				propertyName = "Minimum";
				break;
			case -126:
				typeId = -675;
				propertyName = "Orientation";
				break;
			case -127:
				typeId = -675;
				propertyName = "Value";
				break;
			case -128:
				typeId = -675;
				propertyName = "ViewportSize";
				break;
			case -129:
				typeId = -679;
				propertyName = "Children";
				break;
			case -130:
				typeId = -682;
				propertyName = "Children";
				break;
			case -131:
				typeId = -694;
				propertyName = "ClipToBounds";
				break;
			case -132:
				typeId = -694;
				propertyName = "Focusable";
				break;
			case -133:
				typeId = -694;
				propertyName = "IsEnabled";
				break;
			case -134:
				typeId = -694;
				propertyName = "RenderTransform";
				break;
			case -135:
				typeId = -694;
				propertyName = "Visibility";
				break;
			case -136:
				typeId = -729;
				propertyName = "Children";
				break;
			case -138:
				typeId = -2;
				propertyName = "Child";
				break;
			case -139:
				typeId = -4;
				propertyName = "Child";
				break;
			case -140:
				typeId = -8;
				propertyName = "Blocks";
				break;
			case -141:
				typeId = -14;
				propertyName = "Items";
				break;
			case -142:
				typeId = -37;
				propertyName = "Child";
				break;
			case -143:
				typeId = -41;
				propertyName = "Inlines";
				break;
			case -144:
				typeId = -45;
				propertyName = "KeyFrames";
				break;
			case -145:
				typeId = -50;
				propertyName = "Child";
				break;
			case -146:
				typeId = -54;
				propertyName = "Child";
				break;
			case -147:
				typeId = -55;
				propertyName = "Content";
				break;
			case -148:
				typeId = -56;
				propertyName = "Content";
				break;
			case -149:
				typeId = -60;
				propertyName = "KeyFrames";
				break;
			case -150:
				typeId = -66;
				propertyName = "Children";
				break;
			case -151:
				typeId = -69;
				propertyName = "KeyFrames";
				break;
			case -152:
				typeId = -74;
				propertyName = "Content";
				break;
			case -153:
				typeId = -84;
				propertyName = "KeyFrames";
				break;
			case -154:
				typeId = -92;
				propertyName = "Items";
				break;
			case -155:
				typeId = -93;
				propertyName = "Content";
				break;
			case -156:
				typeId = -105;
				propertyName = "Items";
				break;
			case -157:
				typeId = -108;
				propertyName = "VisualTree";
				break;
			case -158:
				typeId = -120;
				propertyName = "VisualTree";
				break;
			case -159:
				typeId = -122;
				propertyName = "Setters";
				break;
			case -160:
				typeId = -129;
				propertyName = "KeyFrames";
				break;
			case -161:
				typeId = -133;
				propertyName = "Child";
				break;
			case -162:
				typeId = -163;
				propertyName = "Children";
				break;
			case -163:
				typeId = -166;
				propertyName = "Document";
				break;
			case -164:
				typeId = -171;
				propertyName = "KeyFrames";
				break;
			case -165:
				typeId = -198;
				propertyName = "Actions";
				break;
			case -166:
				typeId = -199;
				propertyName = "Content";
				break;
			case -167:
				typeId = -202;
				propertyName = "Blocks";
				break;
			case -168:
				typeId = -205;
				propertyName = "Pages";
				break;
			case -169:
				typeId = -206;
				propertyName = "References";
				break;
			case -170:
				typeId = -207;
				propertyName = "Children";
				break;
			case -171:
				typeId = -208;
				propertyName = "Blocks";
				break;
			case -172:
				typeId = -209;
				propertyName = "Blocks";
				break;
			case -173:
				typeId = -210;
				propertyName = "Document";
				break;
			case -174:
				typeId = -231;
				propertyName = "VisualTree";
				break;
			case -175:
				typeId = -254;
				propertyName = "Children";
				break;
			case -176:
				typeId = -258;
				propertyName = "Columns";
				break;
			case -177:
				typeId = -260;
				propertyName = "Content";
				break;
			case -178:
				typeId = -264;
				propertyName = "Content";
				break;
			case -179:
				typeId = -265;
				propertyName = "Content";
				break;
			case -180:
				typeId = -269;
				propertyName = "Content";
				break;
			case -181:
				typeId = -270;
				propertyName = "Items";
				break;
			case -182:
				typeId = -271;
				propertyName = "VisualTree";
				break;
			case -183:
				typeId = -273;
				propertyName = "Inlines";
				break;
			case -184:
				typeId = -288;
				propertyName = "Children";
				break;
			case -185:
				typeId = -289;
				propertyName = "Child";
				break;
			case -186:
				typeId = -292;
				propertyName = "Child";
				break;
			case -187:
				typeId = -300;
				propertyName = "NameValue";
				break;
			case -188:
				typeId = -305;
				propertyName = "KeyFrames";
				break;
			case -189:
				typeId = -312;
				propertyName = "KeyFrames";
				break;
			case -190:
				typeId = -323;
				propertyName = "KeyFrames";
				break;
			case -191:
				typeId = -327;
				propertyName = "Inlines";
				break;
			case -192:
				typeId = -329;
				propertyName = "Items";
				break;
			case -193:
				typeId = -330;
				propertyName = "VisualTree";
				break;
			case -194:
				typeId = -346;
				propertyName = "Content";
				break;
			case -195:
				typeId = -358;
				propertyName = "GradientStops";
				break;
			case -196:
				typeId = -372;
				propertyName = "ListItems";
				break;
			case -197:
				typeId = -373;
				propertyName = "Items";
				break;
			case -198:
				typeId = -374;
				propertyName = "Content";
				break;
			case -199:
				typeId = -376;
				propertyName = "Blocks";
				break;
			case -200:
				typeId = -377;
				propertyName = "Items";
				break;
			case -201:
				typeId = -378;
				propertyName = "Content";
				break;
			case -202:
				typeId = -389;
				propertyName = "KeyFrames";
				break;
			case -203:
				typeId = -401;
				propertyName = "Items";
				break;
			case -204:
				typeId = -402;
				propertyName = "Items";
				break;
			case -205:
				typeId = -403;
				propertyName = "Items";
				break;
			case -206:
				typeId = -409;
				propertyName = "Children";
				break;
			case -207:
				typeId = -416;
				propertyName = "Bindings";
				break;
			case -208:
				typeId = -418;
				propertyName = "Setters";
				break;
			case -209:
				typeId = -419;
				propertyName = "Setters";
				break;
			case -210:
				typeId = -428;
				propertyName = "KeyFrames";
				break;
			case -211:
				typeId = -435;
				propertyName = "Child";
				break;
			case -212:
				typeId = -436;
				propertyName = "Content";
				break;
			case -213:
				typeId = -437;
				propertyName = "Children";
				break;
			case -214:
				typeId = -438;
				propertyName = "Inlines";
				break;
			case -215:
				typeId = -439;
				propertyName = "Children";
				break;
			case -216:
				typeId = -460;
				propertyName = "KeyFrames";
				break;
			case -217:
				typeId = -470;
				propertyName = "KeyFrames";
				break;
			case -218:
				typeId = -487;
				propertyName = "Bindings";
				break;
			case -219:
				typeId = -497;
				propertyName = "KeyFrames";
				break;
			case -220:
				typeId = -502;
				propertyName = "GradientStops";
				break;
			case -221:
				typeId = -503;
				propertyName = "Content";
				break;
			case -222:
				typeId = -510;
				propertyName = "KeyFrames";
				break;
			case -223:
				typeId = -522;
				propertyName = "Content";
				break;
			case -224:
				typeId = -527;
				propertyName = "Document";
				break;
			case -225:
				typeId = -533;
				propertyName = "KeyFrames";
				break;
			case -226:
				typeId = -542;
				propertyName = "Text";
				break;
			case -227:
				typeId = -550;
				propertyName = "Content";
				break;
			case -228:
				typeId = -551;
				propertyName = "Blocks";
				break;
			case -229:
				typeId = -553;
				propertyName = "Items";
				break;
			case -230:
				typeId = -562;
				propertyName = "KeyFrames";
				break;
			case -231:
				typeId = -571;
				propertyName = "KeyFrames";
				break;
			case -232:
				typeId = -580;
				propertyName = "Inlines";
				break;
			case -233:
				typeId = -601;
				propertyName = "Children";
				break;
			case -234:
				typeId = -604;
				propertyName = "Items";
				break;
			case -235:
				typeId = -605;
				propertyName = "Content";
				break;
			case -236:
				typeId = -608;
				propertyName = "Children";
				break;
			case -237:
				typeId = -614;
				propertyName = "KeyFrames";
				break;
			case -238:
				typeId = -620;
				propertyName = "Setters";
				break;
			case -239:
				typeId = -623;
				propertyName = "Items";
				break;
			case -240:
				typeId = -624;
				propertyName = "Content";
				break;
			case -241:
				typeId = -625;
				propertyName = "Children";
				break;
			case -242:
				typeId = -626;
				propertyName = "RowGroups";
				break;
			case -243:
				typeId = -627;
				propertyName = "Blocks";
				break;
			case -244:
				typeId = -629;
				propertyName = "Cells";
				break;
			case -245:
				typeId = -630;
				propertyName = "Rows";
				break;
			case -246:
				typeId = -638;
				propertyName = "Inlines";
				break;
			case -247:
				typeId = -654;
				propertyName = "KeyFrames";
				break;
			case -248:
				typeId = -668;
				propertyName = "Content";
				break;
			case -249:
				typeId = -669;
				propertyName = "Items";
				break;
			case -250:
				typeId = -670;
				propertyName = "Children";
				break;
			case -251:
				typeId = -671;
				propertyName = "Children";
				break;
			case -252:
				typeId = -672;
				propertyName = "ToolBars";
				break;
			case -253:
				typeId = -673;
				propertyName = "Content";
				break;
			case -254:
				typeId = -686;
				propertyName = "Items";
				break;
			case -255:
				typeId = -687;
				propertyName = "Items";
				break;
			case -256:
				typeId = -688;
				propertyName = "Setters";
				break;
			case -257:
				typeId = -702;
				propertyName = "Inlines";
				break;
			case -258:
				typeId = -703;
				propertyName = "Children";
				break;
			case -259:
				typeId = -706;
				propertyName = "Content";
				break;
			case -260:
				typeId = -712;
				propertyName = "KeyFrames";
				break;
			case -261:
				typeId = -720;
				propertyName = "KeyFrames";
				break;
			case -262:
				typeId = -728;
				propertyName = "Child";
				break;
			case -263:
				typeId = -730;
				propertyName = "Children";
				break;
			case -264:
				typeId = -731;
				propertyName = "Children";
				break;
			case -265:
				typeId = -732;
				propertyName = "Children";
				break;
			case -266:
				typeId = -739;
				propertyName = "Content";
				break;
			case -267:
				typeId = -742;
				propertyName = "Children";
				break;
			case -268:
				typeId = -754;
				propertyName = "XmlSerializer";
				break;
			default:
				typeId = short.MinValue;
				propertyName = null;
				break;
			}
			return propertyName != null;
		}

		public static string GetKnownString(short stringId)
		{
			return stringId switch
			{
				-1 => "Name", 
				-2 => "Uid", 
				_ => null, 
			};
		}

		public static Type GetTypeConverterForKnownProperty(short propertyId)
		{
			if (!GetKnownProperty(propertyId, out var typeId, out var propertyName))
			{
				return null;
			}
			KnownElements knownTypeConverterIdForProperty = System.Windows.Markup.KnownTypes.GetKnownTypeConverterIdForProperty((KnownElements)(-1 * typeId), propertyName);
			if (knownTypeConverterIdForProperty == KnownElements.UnknownElement)
			{
				return null;
			}
			return GetKnownType((short)(-1 * (int)knownTypeConverterIdForProperty));
		}

		public static bool? IsKnownPropertyAttachable(short propertyId)
		{
			if (propertyId >= 0 || propertyId < -268)
			{
				return null;
			}
			return propertyId switch
			{
				-39 => true, 
				-46 => null, 
				-61 => true, 
				-62 => true, 
				-63 => true, 
				-64 => true, 
				-97 => null, 
				-98 => null, 
				-99 => null, 
				-104 => null, 
				-105 => null, 
				-106 => null, 
				-107 => null, 
				-108 => null, 
				-109 => null, 
				-116 => null, 
				-117 => null, 
				-118 => null, 
				-119 => null, 
				-120 => null, 
				-121 => null, 
				_ => false, 
			};
		}
	}

	private sealed class BamlAssembly
	{
		public readonly string Name;

		internal Assembly Assembly;

		public BamlAssembly(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			Name = name;
			Assembly = null;
		}

		public BamlAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			Name = null;
			Assembly = assembly;
		}
	}

	private sealed class BamlType
	{
		public short AssemblyId;

		public string Name;

		public TypeInfoFlags Flags;

		public string ClrNamespace;

		public BamlType(short assemblyId, string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			AssemblyId = assemblyId;
			Name = name;
		}
	}

	private sealed class BamlProperty
	{
		public readonly short DeclaringTypeId;

		public readonly string Name;

		public BamlProperty(short declaringTypeId, string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			DeclaringTypeId = declaringTypeId;
			Name = name;
		}
	}

	[Flags]
	internal enum TypeInfoFlags : byte
	{
		Internal = 1,
		UnusedTwo = 2,
		UnusedThree = 4
	}

	internal const short StaticExtensionTypeId = 602;

	internal const short StaticResourceTypeId = 603;

	internal const short DynamicResourceTypeId = 189;

	internal const short TemplateBindingTypeId = 634;

	internal const short TypeExtensionTypeId = 691;

	internal const string WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	private readonly List<BamlAssembly> _bamlAssembly = new List<BamlAssembly>();

	private readonly List<object> _bamlType = new List<object>();

	private readonly List<object> _bamlProperty = new List<object>();

	private readonly List<string> _bamlString = new List<string>();

	private readonly Dictionary<string, short[]> _bamlXmlnsMappings = new Dictionary<string, short[]>();

	private static readonly Lazy<XamlMember> _xStaticMemberProperty = new Lazy<XamlMember>(() => XamlLanguage.Static.GetMember("MemberType"));

	private static readonly Lazy<XamlMember> _xTypeTypeProperty = new Lazy<XamlMember>(() => XamlLanguage.Static.GetMember("Type"));

	private static readonly Lazy<XamlMember> _resourceDictionaryDefContentProperty = new Lazy<XamlMember>(() => _resourceDictionaryType.Value.GetMember("DeferrableContent"));

	private static readonly Lazy<XamlType> _resourceDictionaryType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(ResourceDictionary)));

	private static readonly Lazy<XamlType> _eventSetterType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(EventSetter)));

	private static readonly Lazy<XamlMember> _eventSetterEventProperty = new Lazy<XamlMember>(() => _eventSetterType.Value.GetMember("Event"));

	private static readonly Lazy<XamlMember> _eventSetterHandlerProperty = new Lazy<XamlMember>(() => _eventSetterType.Value.GetMember("Handler"));

	private static readonly Lazy<XamlMember> _frameworkTemplateTemplateProperty = new Lazy<XamlMember>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(FrameworkTemplate)).GetMember("Template"));

	private static readonly Lazy<XamlType> _staticResourceExtensionType = new Lazy<XamlType>(() => System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(typeof(StaticResourceExtension)));

	private readonly object _syncObject = new object();

	private Assembly _localAssembly;

	private XamlSchemaContext _parentSchemaContext;

	internal XamlMember StaticExtensionMemberTypeProperty => _xStaticMemberProperty.Value;

	internal XamlMember TypeExtensionTypeProperty => _xTypeTypeProperty.Value;

	internal XamlMember ResourceDictionaryDeferredContentProperty => _resourceDictionaryDefContentProperty.Value;

	internal XamlType ResourceDictionaryType => _resourceDictionaryType.Value;

	internal XamlType EventSetterType => _eventSetterType.Value;

	internal XamlMember EventSetterEventProperty => _eventSetterEventProperty.Value;

	internal XamlMember EventSetterHandlerProperty => _eventSetterHandlerProperty.Value;

	internal XamlMember FrameworkTemplateTemplateProperty => _frameworkTemplateTemplateProperty.Value;

	internal XamlType StaticResourceExtensionType => _staticResourceExtensionType.Value;

	internal Assembly LocalAssembly => _localAssembly;

	internal Baml2006ReaderSettings Settings { get; set; }

	public Baml2006SchemaContext(Assembly localAssembly)
		: this(localAssembly, System.Windows.Markup.XamlReader.BamlSharedSchemaContext)
	{
	}

	internal Baml2006SchemaContext(Assembly localAssembly, XamlSchemaContext parentSchemaContext)
		: base(Array.Empty<Assembly>())
	{
		_localAssembly = localAssembly;
		_parentSchemaContext = parentSchemaContext;
	}

	public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
	{
		return _parentSchemaContext.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace);
	}

	public override XamlDirective GetXamlDirective(string xamlNamespace, string name)
	{
		return _parentSchemaContext.GetXamlDirective(xamlNamespace, name);
	}

	public override IEnumerable<string> GetAllXamlNamespaces()
	{
		return _parentSchemaContext.GetAllXamlNamespaces();
	}

	public override ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
	{
		return _parentSchemaContext.GetAllXamlTypes(xamlNamespace);
	}

	public override string GetPreferredPrefix(string xmlns)
	{
		return _parentSchemaContext.GetPreferredPrefix(xmlns);
	}

	public override XamlType GetXamlType(Type type)
	{
		return _parentSchemaContext.GetXamlType(type);
	}

	protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		EnsureXmlnsAssembliesLoaded(xamlNamespace);
		XamlTypeName xamlTypeName = new XamlTypeName
		{
			Namespace = xamlNamespace,
			Name = name
		};
		if (typeArguments != null)
		{
			foreach (XamlType xamlType in typeArguments)
			{
				xamlTypeName.TypeArguments.Add(new XamlTypeName(xamlType));
			}
		}
		return _parentSchemaContext.GetXamlType(xamlTypeName);
	}

	internal void Reset()
	{
		lock (_syncObject)
		{
			_bamlAssembly.Clear();
			_bamlType.Clear();
			_bamlProperty.Clear();
			_bamlString.Clear();
			_bamlXmlnsMappings.Clear();
		}
	}

	internal Assembly GetAssembly(short assemblyId)
	{
		if (TryGetBamlAssembly(assemblyId, out var bamlAssembly))
		{
			return ResolveAssembly(bamlAssembly);
		}
		throw new KeyNotFoundException();
	}

	internal string GetAssemblyName(short assemblyId)
	{
		if (TryGetBamlAssembly(assemblyId, out var bamlAssembly))
		{
			return bamlAssembly.Name;
		}
		throw new KeyNotFoundException(SR.Format(SR.BamlAssemblyIdNotFound, assemblyId.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS)));
	}

	internal Type GetClrType(short typeId)
	{
		if (TryGetBamlType(typeId, out var bamlType, out var xamlType))
		{
			if (xamlType != null)
			{
				return xamlType.UnderlyingType;
			}
			return ResolveBamlTypeToType(bamlType);
		}
		throw new KeyNotFoundException(SR.Format(SR.BamlTypeIdNotFound, typeId.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS)));
	}

	internal XamlType GetXamlType(short typeId)
	{
		if (TryGetBamlType(typeId, out var bamlType, out var xamlType))
		{
			if (xamlType != null)
			{
				return xamlType;
			}
			return ResolveBamlType(bamlType, typeId);
		}
		throw new KeyNotFoundException(SR.Format(SR.BamlTypeIdNotFound, typeId.ToString(System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS)));
	}

	internal DependencyProperty GetDependencyProperty(short propertyId)
	{
		WpfXamlMember wpfXamlMember = GetProperty(propertyId, isAttached: false) as WpfXamlMember;
		if (wpfXamlMember != null)
		{
			return wpfXamlMember.DependencyProperty;
		}
		throw new KeyNotFoundException();
	}

	internal XamlMember GetProperty(short propertyId, XamlType parentType)
	{
		if (TryGetBamlProperty(propertyId, out var bamlProperty, out var xamlMember))
		{
			if (xamlMember != null)
			{
				return xamlMember;
			}
			XamlType xamlType = GetXamlType(bamlProperty.DeclaringTypeId);
			if (parentType.CanAssignTo(xamlType))
			{
				xamlMember = xamlType.GetMember(bamlProperty.Name);
				if (xamlMember == null)
				{
					xamlMember = xamlType.GetAttachableMember(bamlProperty.Name);
				}
			}
			else
			{
				xamlMember = xamlType.GetAttachableMember(bamlProperty.Name);
			}
			lock (_syncObject)
			{
				_bamlProperty[propertyId] = xamlMember;
				return xamlMember;
			}
		}
		throw new KeyNotFoundException();
	}

	internal XamlMember GetProperty(short propertyId, bool isAttached)
	{
		if (TryGetBamlProperty(propertyId, out var bamlProperty, out var xamlMember))
		{
			XamlType declaringType;
			if (xamlMember != null)
			{
				if (xamlMember.IsAttachable != isAttached)
				{
					declaringType = xamlMember.DeclaringType;
					if (isAttached)
					{
						return declaringType.GetAttachableMember(xamlMember.Name);
					}
					return declaringType.GetMember(xamlMember.Name);
				}
				return xamlMember;
			}
			declaringType = GetXamlType(bamlProperty.DeclaringTypeId);
			xamlMember = ((!isAttached) ? declaringType.GetMember(bamlProperty.Name) : declaringType.GetAttachableMember(bamlProperty.Name));
			lock (_syncObject)
			{
				_bamlProperty[propertyId] = xamlMember;
				return xamlMember;
			}
		}
		throw new KeyNotFoundException();
	}

	internal XamlType GetPropertyDeclaringType(short propertyId)
	{
		if (TryGetBamlProperty(propertyId, out var bamlProperty, out var xamlMember))
		{
			if (xamlMember != null)
			{
				return xamlMember.DeclaringType;
			}
			return GetXamlType(bamlProperty.DeclaringTypeId);
		}
		throw new KeyNotFoundException();
	}

	internal string GetPropertyName(short propertyId, bool fullName)
	{
		BamlProperty bamlProperty = null;
		if (TryGetBamlProperty(propertyId, out bamlProperty, out var xamlMember))
		{
			if (xamlMember != null)
			{
				return xamlMember.Name;
			}
			return bamlProperty.Name;
		}
		throw new KeyNotFoundException();
	}

	internal string GetString(short stringId)
	{
		string text;
		lock (_syncObject)
		{
			text = ((stringId < 0 || stringId >= _bamlString.Count) ? KnownTypes.GetKnownString(stringId) : _bamlString[stringId]);
		}
		if (text == null)
		{
			throw new KeyNotFoundException();
		}
		return text;
	}

	internal void AddAssembly(short assemblyId, string assemblyName)
	{
		if (assemblyId < 0)
		{
			throw new ArgumentOutOfRangeException("assemblyId");
		}
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		lock (_bamlAssembly)
		{
			if (assemblyId == _bamlAssembly.Count)
			{
				BamlAssembly item = new BamlAssembly(assemblyName);
				_bamlAssembly.Add(item);
			}
			else if (assemblyId > _bamlAssembly.Count)
			{
				throw new ArgumentOutOfRangeException("assemblyId", SR.Format(SR.AssemblyIdOutOfSequence, assemblyId));
			}
		}
	}

	internal void AddXamlType(short typeId, short assemblyId, string typeName, TypeInfoFlags flags)
	{
		if (typeId < 0)
		{
			throw new ArgumentOutOfRangeException("typeId");
		}
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		lock (_syncObject)
		{
			if (typeId == _bamlType.Count)
			{
				BamlType bamlType = new BamlType(assemblyId, typeName);
				bamlType.Flags = flags;
				_bamlType.Add(bamlType);
			}
			else if (typeId > _bamlType.Count)
			{
				throw new ArgumentOutOfRangeException("typeId", SR.Format(SR.TypeIdOutOfSequence, typeId));
			}
		}
	}

	internal void AddProperty(short propertyId, short declaringTypeId, string propertyName)
	{
		if (propertyId < 0)
		{
			throw new ArgumentOutOfRangeException("propertyId");
		}
		if (propertyName == null)
		{
			throw new ArgumentNullException("propertyName");
		}
		lock (_syncObject)
		{
			if (propertyId == _bamlProperty.Count)
			{
				BamlProperty item = new BamlProperty(declaringTypeId, propertyName);
				_bamlProperty.Add(item);
			}
			else if (propertyId > _bamlProperty.Count)
			{
				throw new ArgumentOutOfRangeException("propertyId", SR.Format(SR.PropertyIdOutOfSequence, propertyId));
			}
		}
	}

	internal void AddString(short stringId, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		lock (_syncObject)
		{
			if (stringId == _bamlString.Count)
			{
				_bamlString.Add(value);
			}
			else if (stringId > _bamlString.Count)
			{
				throw new ArgumentOutOfRangeException("stringId", SR.Format(SR.StringIdOutOfSequence, stringId));
			}
		}
	}

	internal void AddXmlnsMapping(string xmlns, short[] assemblies)
	{
		lock (_syncObject)
		{
			_bamlXmlnsMappings[xmlns] = assemblies;
		}
	}

	private void EnsureXmlnsAssembliesLoaded(string xamlNamespace)
	{
		short[] value;
		lock (_syncObject)
		{
			_bamlXmlnsMappings.TryGetValue(xamlNamespace, out value);
		}
		if (value != null)
		{
			short[] array = value;
			foreach (short assemblyId in array)
			{
				GetAssembly(assemblyId);
			}
		}
	}

	private Assembly ResolveAssembly(BamlAssembly bamlAssembly)
	{
		if (bamlAssembly.Assembly != null)
		{
			return bamlAssembly.Assembly;
		}
		AssemblyName assemblyName = new AssemblyName(bamlAssembly.Name);
		bamlAssembly.Assembly = MS.Internal.WindowsBase.SafeSecurityHelper.GetLoadedAssembly(assemblyName);
		if (bamlAssembly.Assembly == null)
		{
			byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
			if (assemblyName.Version != null || assemblyName.CultureInfo != null || publicKeyToken != null)
			{
				try
				{
					bamlAssembly.Assembly = Assembly.Load(assemblyName.FullName);
				}
				catch
				{
					if (bamlAssembly.Assembly == null)
					{
						if (MatchesLocalAssembly(assemblyName.Name, publicKeyToken))
						{
							bamlAssembly.Assembly = _localAssembly;
						}
						else
						{
							AssemblyName assemblyName2 = new AssemblyName(assemblyName.Name);
							if (publicKeyToken != null)
							{
								assemblyName2.SetPublicKeyToken(publicKeyToken);
							}
							bamlAssembly.Assembly = Assembly.Load(assemblyName2);
						}
					}
				}
			}
			else
			{
				bamlAssembly.Assembly = Assembly.LoadWithPartialName(assemblyName.Name);
			}
		}
		return bamlAssembly.Assembly;
	}

	private bool MatchesLocalAssembly(string shortName, byte[] publicKeyToken)
	{
		if (_localAssembly == null)
		{
			return false;
		}
		AssemblyName assemblyName = new AssemblyName(_localAssembly.FullName);
		if (shortName != assemblyName.Name)
		{
			return false;
		}
		if (publicKeyToken == null)
		{
			return true;
		}
		return MS.Internal.PresentationFramework.SafeSecurityHelper.IsSameKeyToken(publicKeyToken, assemblyName.GetPublicKeyToken());
	}

	private Type ResolveBamlTypeToType(BamlType bamlType)
	{
		if (TryGetBamlAssembly(bamlType.AssemblyId, out var bamlAssembly))
		{
			Assembly assembly = ResolveAssembly(bamlAssembly);
			if (assembly != null)
			{
				return assembly.GetType(bamlType.Name, throwOnError: false);
			}
		}
		return null;
	}

	private XamlType ResolveBamlType(BamlType bamlType, short typeId)
	{
		Type type = ResolveBamlTypeToType(bamlType);
		if (type != null)
		{
			bamlType.ClrNamespace = type.Namespace;
			XamlType xamlType = _parentSchemaContext.GetXamlType(type);
			lock (_syncObject)
			{
				_bamlType[typeId] = xamlType;
				return xamlType;
			}
		}
		throw new NotImplementedException();
	}

	private bool TryGetBamlAssembly(short assemblyId, out BamlAssembly bamlAssembly)
	{
		lock (_syncObject)
		{
			if (assemblyId >= 0 && assemblyId < _bamlAssembly.Count)
			{
				bamlAssembly = _bamlAssembly[assemblyId];
				return true;
			}
		}
		Assembly knownAssembly = KnownTypes.GetKnownAssembly(assemblyId);
		if (knownAssembly != null)
		{
			bamlAssembly = new BamlAssembly(knownAssembly);
			return true;
		}
		bamlAssembly = null;
		return false;
	}

	private bool TryGetBamlType(short typeId, out BamlType bamlType, out XamlType xamlType)
	{
		bamlType = null;
		xamlType = null;
		lock (_syncObject)
		{
			if (typeId >= 0 && typeId < _bamlType.Count)
			{
				object obj = _bamlType[typeId];
				bamlType = obj as BamlType;
				xamlType = obj as XamlType;
				return obj != null;
			}
		}
		if (typeId < 0)
		{
			if (_parentSchemaContext == System.Windows.Markup.XamlReader.BamlSharedSchemaContext)
			{
				xamlType = System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetKnownBamlType(typeId);
			}
			else
			{
				xamlType = _parentSchemaContext.GetXamlType(KnownTypes.GetKnownType(typeId));
			}
			return true;
		}
		return bamlType != null;
	}

	private bool TryGetBamlProperty(short propertyId, out BamlProperty bamlProperty, out XamlMember xamlMember)
	{
		lock (_syncObject)
		{
			if (propertyId >= 0 && propertyId < _bamlProperty.Count)
			{
				object obj = _bamlProperty[propertyId];
				xamlMember = obj as XamlMember;
				bamlProperty = obj as BamlProperty;
				return true;
			}
		}
		if (propertyId < 0)
		{
			if (_parentSchemaContext == System.Windows.Markup.XamlReader.BamlSharedSchemaContext)
			{
				xamlMember = System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetKnownBamlMember(propertyId);
			}
			else
			{
				KnownTypes.GetKnownProperty(propertyId, out var typeId, out var propertyName);
				xamlMember = GetXamlType(typeId).GetMember(propertyName);
			}
			bamlProperty = null;
			return true;
		}
		xamlMember = null;
		bamlProperty = null;
		return false;
	}
}
