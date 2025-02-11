using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace System.Windows.Markup;

internal static class KnownTypes
{
	private static TypeIndexer _typeIndexer = new TypeIndexer(760);

	internal static TypeIndexer Types => _typeIndexer;

	internal static object CreateKnownElement(KnownElements knownElement)
	{
		object result = null;
		switch (knownElement)
		{
		case KnownElements.AccessText:
			result = new AccessText();
			break;
		case KnownElements.AdornedElementPlaceholder:
			result = new AdornedElementPlaceholder();
			break;
		case KnownElements.AdornerDecorator:
			result = new AdornerDecorator();
			break;
		case KnownElements.AmbientLight:
			result = new AmbientLight();
			break;
		case KnownElements.Application:
			result = new Application();
			break;
		case KnownElements.ArcSegment:
			result = new ArcSegment();
			break;
		case KnownElements.ArrayExtension:
			result = new ArrayExtension();
			break;
		case KnownElements.AxisAngleRotation3D:
			result = new AxisAngleRotation3D();
			break;
		case KnownElements.BeginStoryboard:
			result = new BeginStoryboard();
			break;
		case KnownElements.BevelBitmapEffect:
			result = new BevelBitmapEffect();
			break;
		case KnownElements.BezierSegment:
			result = new BezierSegment();
			break;
		case KnownElements.Binding:
			result = new Binding();
			break;
		case KnownElements.BitmapEffectCollection:
			result = new BitmapEffectCollection();
			break;
		case KnownElements.BitmapEffectGroup:
			result = new BitmapEffectGroup();
			break;
		case KnownElements.BitmapEffectInput:
			result = new BitmapEffectInput();
			break;
		case KnownElements.BitmapImage:
			result = new BitmapImage();
			break;
		case KnownElements.BlockUIContainer:
			result = new BlockUIContainer();
			break;
		case KnownElements.BlurBitmapEffect:
			result = new BlurBitmapEffect();
			break;
		case KnownElements.BmpBitmapEncoder:
			result = new BmpBitmapEncoder();
			break;
		case KnownElements.Bold:
			result = new Bold();
			break;
		case KnownElements.BoolIListConverter:
			result = new BoolIListConverter();
			break;
		case KnownElements.BooleanAnimationUsingKeyFrames:
			result = new BooleanAnimationUsingKeyFrames();
			break;
		case KnownElements.BooleanConverter:
			result = new BooleanConverter();
			break;
		case KnownElements.BooleanKeyFrameCollection:
			result = new BooleanKeyFrameCollection();
			break;
		case KnownElements.BooleanToVisibilityConverter:
			result = new BooleanToVisibilityConverter();
			break;
		case KnownElements.Border:
			result = new Border();
			break;
		case KnownElements.BorderGapMaskConverter:
			result = new BorderGapMaskConverter();
			break;
		case KnownElements.BrushConverter:
			result = new BrushConverter();
			break;
		case KnownElements.BulletDecorator:
			result = new BulletDecorator();
			break;
		case KnownElements.Button:
			result = new Button();
			break;
		case KnownElements.ByteAnimation:
			result = new ByteAnimation();
			break;
		case KnownElements.ByteAnimationUsingKeyFrames:
			result = new ByteAnimationUsingKeyFrames();
			break;
		case KnownElements.ByteConverter:
			result = new ByteConverter();
			break;
		case KnownElements.ByteKeyFrameCollection:
			result = new ByteKeyFrameCollection();
			break;
		case KnownElements.Canvas:
			result = new Canvas();
			break;
		case KnownElements.CharAnimationUsingKeyFrames:
			result = new CharAnimationUsingKeyFrames();
			break;
		case KnownElements.CharConverter:
			result = new CharConverter();
			break;
		case KnownElements.CharIListConverter:
			result = new CharIListConverter();
			break;
		case KnownElements.CharKeyFrameCollection:
			result = new CharKeyFrameCollection();
			break;
		case KnownElements.CheckBox:
			result = new CheckBox();
			break;
		case KnownElements.CollectionContainer:
			result = new CollectionContainer();
			break;
		case KnownElements.CollectionViewSource:
			result = new CollectionViewSource();
			break;
		case KnownElements.Color:
			result = default(Color);
			break;
		case KnownElements.ColorAnimation:
			result = new ColorAnimation();
			break;
		case KnownElements.ColorAnimationUsingKeyFrames:
			result = new ColorAnimationUsingKeyFrames();
			break;
		case KnownElements.ColorConvertedBitmap:
			result = new ColorConvertedBitmap();
			break;
		case KnownElements.ColorConvertedBitmapExtension:
			result = new ColorConvertedBitmapExtension();
			break;
		case KnownElements.ColorConverter:
			result = new ColorConverter();
			break;
		case KnownElements.ColorKeyFrameCollection:
			result = new ColorKeyFrameCollection();
			break;
		case KnownElements.ColumnDefinition:
			result = new ColumnDefinition();
			break;
		case KnownElements.CombinedGeometry:
			result = new CombinedGeometry();
			break;
		case KnownElements.ComboBox:
			result = new ComboBox();
			break;
		case KnownElements.ComboBoxItem:
			result = new ComboBoxItem();
			break;
		case KnownElements.CommandConverter:
			result = new CommandConverter();
			break;
		case KnownElements.ComponentResourceKey:
			result = new ComponentResourceKey();
			break;
		case KnownElements.ComponentResourceKeyConverter:
			result = new ComponentResourceKeyConverter();
			break;
		case KnownElements.Condition:
			result = new Condition();
			break;
		case KnownElements.ContainerVisual:
			result = new ContainerVisual();
			break;
		case KnownElements.ContentControl:
			result = new ContentControl();
			break;
		case KnownElements.ContentElement:
			result = new ContentElement();
			break;
		case KnownElements.ContentPresenter:
			result = new ContentPresenter();
			break;
		case KnownElements.ContextMenu:
			result = new ContextMenu();
			break;
		case KnownElements.Control:
			result = new Control();
			break;
		case KnownElements.ControlTemplate:
			result = new ControlTemplate();
			break;
		case KnownElements.CornerRadius:
			result = default(CornerRadius);
			break;
		case KnownElements.CornerRadiusConverter:
			result = new CornerRadiusConverter();
			break;
		case KnownElements.CroppedBitmap:
			result = new CroppedBitmap();
			break;
		case KnownElements.CultureInfoConverter:
			result = new CultureInfoConverter();
			break;
		case KnownElements.CultureInfoIetfLanguageTagConverter:
			result = new CultureInfoIetfLanguageTagConverter();
			break;
		case KnownElements.CursorConverter:
			result = new CursorConverter();
			break;
		case KnownElements.DashStyle:
			result = new DashStyle();
			break;
		case KnownElements.DataTemplate:
			result = new DataTemplate();
			break;
		case KnownElements.DataTemplateKey:
			result = new DataTemplateKey();
			break;
		case KnownElements.DataTrigger:
			result = new DataTrigger();
			break;
		case KnownElements.DateTimeConverter:
			result = new DateTimeConverter();
			break;
		case KnownElements.DateTimeConverter2:
			result = new DateTimeConverter2();
			break;
		case KnownElements.DecimalAnimation:
			result = new DecimalAnimation();
			break;
		case KnownElements.DecimalAnimationUsingKeyFrames:
			result = new DecimalAnimationUsingKeyFrames();
			break;
		case KnownElements.DecimalConverter:
			result = new DecimalConverter();
			break;
		case KnownElements.DecimalKeyFrameCollection:
			result = new DecimalKeyFrameCollection();
			break;
		case KnownElements.Decorator:
			result = new Decorator();
			break;
		case KnownElements.DependencyObject:
			result = new DependencyObject();
			break;
		case KnownElements.DependencyPropertyConverter:
			result = new DependencyPropertyConverter();
			break;
		case KnownElements.DialogResultConverter:
			result = new DialogResultConverter();
			break;
		case KnownElements.DiffuseMaterial:
			result = new DiffuseMaterial();
			break;
		case KnownElements.DirectionalLight:
			result = new DirectionalLight();
			break;
		case KnownElements.DiscreteBooleanKeyFrame:
			result = new DiscreteBooleanKeyFrame();
			break;
		case KnownElements.DiscreteByteKeyFrame:
			result = new DiscreteByteKeyFrame();
			break;
		case KnownElements.DiscreteCharKeyFrame:
			result = new DiscreteCharKeyFrame();
			break;
		case KnownElements.DiscreteColorKeyFrame:
			result = new DiscreteColorKeyFrame();
			break;
		case KnownElements.DiscreteDecimalKeyFrame:
			result = new DiscreteDecimalKeyFrame();
			break;
		case KnownElements.DiscreteDoubleKeyFrame:
			result = new DiscreteDoubleKeyFrame();
			break;
		case KnownElements.DiscreteInt16KeyFrame:
			result = new DiscreteInt16KeyFrame();
			break;
		case KnownElements.DiscreteInt32KeyFrame:
			result = new DiscreteInt32KeyFrame();
			break;
		case KnownElements.DiscreteInt64KeyFrame:
			result = new DiscreteInt64KeyFrame();
			break;
		case KnownElements.DiscreteMatrixKeyFrame:
			result = new DiscreteMatrixKeyFrame();
			break;
		case KnownElements.DiscreteObjectKeyFrame:
			result = new DiscreteObjectKeyFrame();
			break;
		case KnownElements.DiscretePoint3DKeyFrame:
			result = new DiscretePoint3DKeyFrame();
			break;
		case KnownElements.DiscretePointKeyFrame:
			result = new DiscretePointKeyFrame();
			break;
		case KnownElements.DiscreteQuaternionKeyFrame:
			result = new DiscreteQuaternionKeyFrame();
			break;
		case KnownElements.DiscreteRectKeyFrame:
			result = new DiscreteRectKeyFrame();
			break;
		case KnownElements.DiscreteRotation3DKeyFrame:
			result = new DiscreteRotation3DKeyFrame();
			break;
		case KnownElements.DiscreteSingleKeyFrame:
			result = new DiscreteSingleKeyFrame();
			break;
		case KnownElements.DiscreteSizeKeyFrame:
			result = new DiscreteSizeKeyFrame();
			break;
		case KnownElements.DiscreteStringKeyFrame:
			result = new DiscreteStringKeyFrame();
			break;
		case KnownElements.DiscreteThicknessKeyFrame:
			result = new DiscreteThicknessKeyFrame();
			break;
		case KnownElements.DiscreteVector3DKeyFrame:
			result = new DiscreteVector3DKeyFrame();
			break;
		case KnownElements.DiscreteVectorKeyFrame:
			result = new DiscreteVectorKeyFrame();
			break;
		case KnownElements.DockPanel:
			result = new DockPanel();
			break;
		case KnownElements.DocumentPageView:
			result = new DocumentPageView();
			break;
		case KnownElements.DocumentReference:
			result = new DocumentReference();
			break;
		case KnownElements.DocumentViewer:
			result = new DocumentViewer();
			break;
		case KnownElements.DoubleAnimation:
			result = new DoubleAnimation();
			break;
		case KnownElements.DoubleAnimationUsingKeyFrames:
			result = new DoubleAnimationUsingKeyFrames();
			break;
		case KnownElements.DoubleAnimationUsingPath:
			result = new DoubleAnimationUsingPath();
			break;
		case KnownElements.DoubleCollection:
			result = new DoubleCollection();
			break;
		case KnownElements.DoubleCollectionConverter:
			result = new DoubleCollectionConverter();
			break;
		case KnownElements.DoubleConverter:
			result = new DoubleConverter();
			break;
		case KnownElements.DoubleIListConverter:
			result = new DoubleIListConverter();
			break;
		case KnownElements.DoubleKeyFrameCollection:
			result = new DoubleKeyFrameCollection();
			break;
		case KnownElements.DrawingBrush:
			result = new DrawingBrush();
			break;
		case KnownElements.DrawingCollection:
			result = new DrawingCollection();
			break;
		case KnownElements.DrawingGroup:
			result = new DrawingGroup();
			break;
		case KnownElements.DrawingImage:
			result = new DrawingImage();
			break;
		case KnownElements.DrawingVisual:
			result = new DrawingVisual();
			break;
		case KnownElements.DropShadowBitmapEffect:
			result = new DropShadowBitmapEffect();
			break;
		case KnownElements.Duration:
			result = default(Duration);
			break;
		case KnownElements.DurationConverter:
			result = new DurationConverter();
			break;
		case KnownElements.DynamicResourceExtension:
			result = new DynamicResourceExtension();
			break;
		case KnownElements.DynamicResourceExtensionConverter:
			result = new DynamicResourceExtensionConverter();
			break;
		case KnownElements.Ellipse:
			result = new Ellipse();
			break;
		case KnownElements.EllipseGeometry:
			result = new EllipseGeometry();
			break;
		case KnownElements.EmbossBitmapEffect:
			result = new EmbossBitmapEffect();
			break;
		case KnownElements.EmissiveMaterial:
			result = new EmissiveMaterial();
			break;
		case KnownElements.EventSetter:
			result = new EventSetter();
			break;
		case KnownElements.EventTrigger:
			result = new EventTrigger();
			break;
		case KnownElements.Expander:
			result = new Expander();
			break;
		case KnownElements.ExpressionConverter:
			result = new ExpressionConverter();
			break;
		case KnownElements.Figure:
			result = new Figure();
			break;
		case KnownElements.FigureLength:
			result = default(FigureLength);
			break;
		case KnownElements.FigureLengthConverter:
			result = new FigureLengthConverter();
			break;
		case KnownElements.FixedDocument:
			result = new FixedDocument();
			break;
		case KnownElements.FixedDocumentSequence:
			result = new FixedDocumentSequence();
			break;
		case KnownElements.FixedPage:
			result = new FixedPage();
			break;
		case KnownElements.Floater:
			result = new Floater();
			break;
		case KnownElements.FlowDocument:
			result = new FlowDocument();
			break;
		case KnownElements.FlowDocumentPageViewer:
			result = new FlowDocumentPageViewer();
			break;
		case KnownElements.FlowDocumentReader:
			result = new FlowDocumentReader();
			break;
		case KnownElements.FlowDocumentScrollViewer:
			result = new FlowDocumentScrollViewer();
			break;
		case KnownElements.FontFamily:
			result = new FontFamily();
			break;
		case KnownElements.FontFamilyConverter:
			result = new FontFamilyConverter();
			break;
		case KnownElements.FontSizeConverter:
			result = new FontSizeConverter();
			break;
		case KnownElements.FontStretch:
			result = default(FontStretch);
			break;
		case KnownElements.FontStretchConverter:
			result = new FontStretchConverter();
			break;
		case KnownElements.FontStyle:
			result = default(FontStyle);
			break;
		case KnownElements.FontStyleConverter:
			result = new FontStyleConverter();
			break;
		case KnownElements.FontWeight:
			result = default(FontWeight);
			break;
		case KnownElements.FontWeightConverter:
			result = new FontWeightConverter();
			break;
		case KnownElements.FormatConvertedBitmap:
			result = new FormatConvertedBitmap();
			break;
		case KnownElements.Frame:
			result = new Frame();
			break;
		case KnownElements.FrameworkContentElement:
			result = new FrameworkContentElement();
			break;
		case KnownElements.FrameworkElement:
			result = new FrameworkElement();
			break;
		case KnownElements.FrameworkElementFactory:
			result = new FrameworkElementFactory();
			break;
		case KnownElements.FrameworkPropertyMetadata:
			result = new FrameworkPropertyMetadata();
			break;
		case KnownElements.GeneralTransformCollection:
			result = new GeneralTransformCollection();
			break;
		case KnownElements.GeneralTransformGroup:
			result = new GeneralTransformGroup();
			break;
		case KnownElements.GeometryCollection:
			result = new GeometryCollection();
			break;
		case KnownElements.GeometryConverter:
			result = new GeometryConverter();
			break;
		case KnownElements.GeometryDrawing:
			result = new GeometryDrawing();
			break;
		case KnownElements.GeometryGroup:
			result = new GeometryGroup();
			break;
		case KnownElements.GeometryModel3D:
			result = new GeometryModel3D();
			break;
		case KnownElements.GestureRecognizer:
			result = new GestureRecognizer();
			break;
		case KnownElements.GifBitmapEncoder:
			result = new GifBitmapEncoder();
			break;
		case KnownElements.GlyphRun:
			result = new GlyphRun();
			break;
		case KnownElements.GlyphRunDrawing:
			result = new GlyphRunDrawing();
			break;
		case KnownElements.GlyphTypeface:
			result = new GlyphTypeface();
			break;
		case KnownElements.Glyphs:
			result = new Glyphs();
			break;
		case KnownElements.GradientStop:
			result = new GradientStop();
			break;
		case KnownElements.GradientStopCollection:
			result = new GradientStopCollection();
			break;
		case KnownElements.Grid:
			result = new Grid();
			break;
		case KnownElements.GridLength:
			result = default(GridLength);
			break;
		case KnownElements.GridLengthConverter:
			result = new GridLengthConverter();
			break;
		case KnownElements.GridSplitter:
			result = new GridSplitter();
			break;
		case KnownElements.GridView:
			result = new GridView();
			break;
		case KnownElements.GridViewColumn:
			result = new GridViewColumn();
			break;
		case KnownElements.GridViewColumnHeader:
			result = new GridViewColumnHeader();
			break;
		case KnownElements.GridViewHeaderRowPresenter:
			result = new GridViewHeaderRowPresenter();
			break;
		case KnownElements.GridViewRowPresenter:
			result = new GridViewRowPresenter();
			break;
		case KnownElements.GroupBox:
			result = new GroupBox();
			break;
		case KnownElements.GroupItem:
			result = new GroupItem();
			break;
		case KnownElements.GuidConverter:
			result = new GuidConverter();
			break;
		case KnownElements.GuidelineSet:
			result = new GuidelineSet();
			break;
		case KnownElements.HeaderedContentControl:
			result = new HeaderedContentControl();
			break;
		case KnownElements.HeaderedItemsControl:
			result = new HeaderedItemsControl();
			break;
		case KnownElements.HierarchicalDataTemplate:
			result = new HierarchicalDataTemplate();
			break;
		case KnownElements.HostVisual:
			result = new HostVisual();
			break;
		case KnownElements.Hyperlink:
			result = new Hyperlink();
			break;
		case KnownElements.Image:
			result = new Image();
			break;
		case KnownElements.ImageBrush:
			result = new ImageBrush();
			break;
		case KnownElements.ImageDrawing:
			result = new ImageDrawing();
			break;
		case KnownElements.ImageSourceConverter:
			result = new ImageSourceConverter();
			break;
		case KnownElements.InkCanvas:
			result = new InkCanvas();
			break;
		case KnownElements.InkPresenter:
			result = new InkPresenter();
			break;
		case KnownElements.InlineUIContainer:
			result = new InlineUIContainer();
			break;
		case KnownElements.InputScope:
			result = new InputScope();
			break;
		case KnownElements.InputScopeConverter:
			result = new InputScopeConverter();
			break;
		case KnownElements.InputScopeName:
			result = new InputScopeName();
			break;
		case KnownElements.InputScopeNameConverter:
			result = new InputScopeNameConverter();
			break;
		case KnownElements.Int16Animation:
			result = new Int16Animation();
			break;
		case KnownElements.Int16AnimationUsingKeyFrames:
			result = new Int16AnimationUsingKeyFrames();
			break;
		case KnownElements.Int16Converter:
			result = new Int16Converter();
			break;
		case KnownElements.Int16KeyFrameCollection:
			result = new Int16KeyFrameCollection();
			break;
		case KnownElements.Int32Animation:
			result = new Int32Animation();
			break;
		case KnownElements.Int32AnimationUsingKeyFrames:
			result = new Int32AnimationUsingKeyFrames();
			break;
		case KnownElements.Int32Collection:
			result = new Int32Collection();
			break;
		case KnownElements.Int32CollectionConverter:
			result = new Int32CollectionConverter();
			break;
		case KnownElements.Int32Converter:
			result = new Int32Converter();
			break;
		case KnownElements.Int32KeyFrameCollection:
			result = new Int32KeyFrameCollection();
			break;
		case KnownElements.Int32Rect:
			result = default(Int32Rect);
			break;
		case KnownElements.Int32RectConverter:
			result = new Int32RectConverter();
			break;
		case KnownElements.Int64Animation:
			result = new Int64Animation();
			break;
		case KnownElements.Int64AnimationUsingKeyFrames:
			result = new Int64AnimationUsingKeyFrames();
			break;
		case KnownElements.Int64Converter:
			result = new Int64Converter();
			break;
		case KnownElements.Int64KeyFrameCollection:
			result = new Int64KeyFrameCollection();
			break;
		case KnownElements.Italic:
			result = new Italic();
			break;
		case KnownElements.ItemsControl:
			result = new ItemsControl();
			break;
		case KnownElements.ItemsPanelTemplate:
			result = new ItemsPanelTemplate();
			break;
		case KnownElements.ItemsPresenter:
			result = new ItemsPresenter();
			break;
		case KnownElements.JournalEntryListConverter:
			result = new JournalEntryListConverter();
			break;
		case KnownElements.JournalEntryUnifiedViewConverter:
			result = new JournalEntryUnifiedViewConverter();
			break;
		case KnownElements.JpegBitmapEncoder:
			result = new JpegBitmapEncoder();
			break;
		case KnownElements.KeyBinding:
			result = new KeyBinding();
			break;
		case KnownElements.KeyConverter:
			result = new KeyConverter();
			break;
		case KnownElements.KeyGestureConverter:
			result = new KeyGestureConverter();
			break;
		case KnownElements.KeySpline:
			result = new KeySpline();
			break;
		case KnownElements.KeySplineConverter:
			result = new KeySplineConverter();
			break;
		case KnownElements.KeyTime:
			result = default(KeyTime);
			break;
		case KnownElements.KeyTimeConverter:
			result = new KeyTimeConverter();
			break;
		case KnownElements.Label:
			result = new Label();
			break;
		case KnownElements.LengthConverter:
			result = new LengthConverter();
			break;
		case KnownElements.Line:
			result = new Line();
			break;
		case KnownElements.LineBreak:
			result = new LineBreak();
			break;
		case KnownElements.LineGeometry:
			result = new LineGeometry();
			break;
		case KnownElements.LineSegment:
			result = new LineSegment();
			break;
		case KnownElements.LinearByteKeyFrame:
			result = new LinearByteKeyFrame();
			break;
		case KnownElements.LinearColorKeyFrame:
			result = new LinearColorKeyFrame();
			break;
		case KnownElements.LinearDecimalKeyFrame:
			result = new LinearDecimalKeyFrame();
			break;
		case KnownElements.LinearDoubleKeyFrame:
			result = new LinearDoubleKeyFrame();
			break;
		case KnownElements.LinearGradientBrush:
			result = new LinearGradientBrush();
			break;
		case KnownElements.LinearInt16KeyFrame:
			result = new LinearInt16KeyFrame();
			break;
		case KnownElements.LinearInt32KeyFrame:
			result = new LinearInt32KeyFrame();
			break;
		case KnownElements.LinearInt64KeyFrame:
			result = new LinearInt64KeyFrame();
			break;
		case KnownElements.LinearPoint3DKeyFrame:
			result = new LinearPoint3DKeyFrame();
			break;
		case KnownElements.LinearPointKeyFrame:
			result = new LinearPointKeyFrame();
			break;
		case KnownElements.LinearQuaternionKeyFrame:
			result = new LinearQuaternionKeyFrame();
			break;
		case KnownElements.LinearRectKeyFrame:
			result = new LinearRectKeyFrame();
			break;
		case KnownElements.LinearRotation3DKeyFrame:
			result = new LinearRotation3DKeyFrame();
			break;
		case KnownElements.LinearSingleKeyFrame:
			result = new LinearSingleKeyFrame();
			break;
		case KnownElements.LinearSizeKeyFrame:
			result = new LinearSizeKeyFrame();
			break;
		case KnownElements.LinearThicknessKeyFrame:
			result = new LinearThicknessKeyFrame();
			break;
		case KnownElements.LinearVector3DKeyFrame:
			result = new LinearVector3DKeyFrame();
			break;
		case KnownElements.LinearVectorKeyFrame:
			result = new LinearVectorKeyFrame();
			break;
		case KnownElements.List:
			result = new List();
			break;
		case KnownElements.ListBox:
			result = new ListBox();
			break;
		case KnownElements.ListBoxItem:
			result = new ListBoxItem();
			break;
		case KnownElements.ListItem:
			result = new ListItem();
			break;
		case KnownElements.ListView:
			result = new ListView();
			break;
		case KnownElements.ListViewItem:
			result = new ListViewItem();
			break;
		case KnownElements.MaterialCollection:
			result = new MaterialCollection();
			break;
		case KnownElements.MaterialGroup:
			result = new MaterialGroup();
			break;
		case KnownElements.Matrix:
			result = default(Matrix);
			break;
		case KnownElements.Matrix3D:
			result = default(Matrix3D);
			break;
		case KnownElements.Matrix3DConverter:
			result = new Matrix3DConverter();
			break;
		case KnownElements.MatrixAnimationUsingKeyFrames:
			result = new MatrixAnimationUsingKeyFrames();
			break;
		case KnownElements.MatrixAnimationUsingPath:
			result = new MatrixAnimationUsingPath();
			break;
		case KnownElements.MatrixCamera:
			result = new MatrixCamera();
			break;
		case KnownElements.MatrixConverter:
			result = new MatrixConverter();
			break;
		case KnownElements.MatrixKeyFrameCollection:
			result = new MatrixKeyFrameCollection();
			break;
		case KnownElements.MatrixTransform:
			result = new MatrixTransform();
			break;
		case KnownElements.MatrixTransform3D:
			result = new MatrixTransform3D();
			break;
		case KnownElements.MediaElement:
			result = new MediaElement();
			break;
		case KnownElements.MediaPlayer:
			result = new MediaPlayer();
			break;
		case KnownElements.MediaTimeline:
			result = new MediaTimeline();
			break;
		case KnownElements.Menu:
			result = new Menu();
			break;
		case KnownElements.MenuItem:
			result = new MenuItem();
			break;
		case KnownElements.MenuScrollingVisibilityConverter:
			result = new MenuScrollingVisibilityConverter();
			break;
		case KnownElements.MeshGeometry3D:
			result = new MeshGeometry3D();
			break;
		case KnownElements.Model3DCollection:
			result = new Model3DCollection();
			break;
		case KnownElements.Model3DGroup:
			result = new Model3DGroup();
			break;
		case KnownElements.ModelVisual3D:
			result = new ModelVisual3D();
			break;
		case KnownElements.ModifierKeysConverter:
			result = new ModifierKeysConverter();
			break;
		case KnownElements.MouseActionConverter:
			result = new MouseActionConverter();
			break;
		case KnownElements.MouseBinding:
			result = new MouseBinding();
			break;
		case KnownElements.MouseGesture:
			result = new MouseGesture();
			break;
		case KnownElements.MouseGestureConverter:
			result = new MouseGestureConverter();
			break;
		case KnownElements.MultiBinding:
			result = new MultiBinding();
			break;
		case KnownElements.MultiDataTrigger:
			result = new MultiDataTrigger();
			break;
		case KnownElements.MultiTrigger:
			result = new MultiTrigger();
			break;
		case KnownElements.NameScope:
			result = new NameScope();
			break;
		case KnownElements.NavigationWindow:
			result = new NavigationWindow();
			break;
		case KnownElements.NullExtension:
			result = new NullExtension();
			break;
		case KnownElements.NullableBoolConverter:
			result = new NullableBoolConverter();
			break;
		case KnownElements.NumberSubstitution:
			result = new NumberSubstitution();
			break;
		case KnownElements.Object:
			result = new object();
			break;
		case KnownElements.ObjectAnimationUsingKeyFrames:
			result = new ObjectAnimationUsingKeyFrames();
			break;
		case KnownElements.ObjectDataProvider:
			result = new ObjectDataProvider();
			break;
		case KnownElements.ObjectKeyFrameCollection:
			result = new ObjectKeyFrameCollection();
			break;
		case KnownElements.OrthographicCamera:
			result = new OrthographicCamera();
			break;
		case KnownElements.OuterGlowBitmapEffect:
			result = new OuterGlowBitmapEffect();
			break;
		case KnownElements.Page:
			result = new Page();
			break;
		case KnownElements.PageContent:
			result = new PageContent();
			break;
		case KnownElements.Paragraph:
			result = new Paragraph();
			break;
		case KnownElements.ParallelTimeline:
			result = new ParallelTimeline();
			break;
		case KnownElements.ParserContext:
			result = new ParserContext();
			break;
		case KnownElements.PasswordBox:
			result = new PasswordBox();
			break;
		case KnownElements.Path:
			result = new Path();
			break;
		case KnownElements.PathFigure:
			result = new PathFigure();
			break;
		case KnownElements.PathFigureCollection:
			result = new PathFigureCollection();
			break;
		case KnownElements.PathFigureCollectionConverter:
			result = new PathFigureCollectionConverter();
			break;
		case KnownElements.PathGeometry:
			result = new PathGeometry();
			break;
		case KnownElements.PathSegmentCollection:
			result = new PathSegmentCollection();
			break;
		case KnownElements.PauseStoryboard:
			result = new PauseStoryboard();
			break;
		case KnownElements.Pen:
			result = new Pen();
			break;
		case KnownElements.PerspectiveCamera:
			result = new PerspectiveCamera();
			break;
		case KnownElements.PixelFormat:
			result = default(PixelFormat);
			break;
		case KnownElements.PixelFormatConverter:
			result = new PixelFormatConverter();
			break;
		case KnownElements.PngBitmapEncoder:
			result = new PngBitmapEncoder();
			break;
		case KnownElements.Point:
			result = default(Point);
			break;
		case KnownElements.Point3D:
			result = default(Point3D);
			break;
		case KnownElements.Point3DAnimation:
			result = new Point3DAnimation();
			break;
		case KnownElements.Point3DAnimationUsingKeyFrames:
			result = new Point3DAnimationUsingKeyFrames();
			break;
		case KnownElements.Point3DCollection:
			result = new Point3DCollection();
			break;
		case KnownElements.Point3DCollectionConverter:
			result = new Point3DCollectionConverter();
			break;
		case KnownElements.Point3DConverter:
			result = new Point3DConverter();
			break;
		case KnownElements.Point3DKeyFrameCollection:
			result = new Point3DKeyFrameCollection();
			break;
		case KnownElements.Point4D:
			result = default(Point4D);
			break;
		case KnownElements.Point4DConverter:
			result = new Point4DConverter();
			break;
		case KnownElements.PointAnimation:
			result = new PointAnimation();
			break;
		case KnownElements.PointAnimationUsingKeyFrames:
			result = new PointAnimationUsingKeyFrames();
			break;
		case KnownElements.PointAnimationUsingPath:
			result = new PointAnimationUsingPath();
			break;
		case KnownElements.PointCollection:
			result = new PointCollection();
			break;
		case KnownElements.PointCollectionConverter:
			result = new PointCollectionConverter();
			break;
		case KnownElements.PointConverter:
			result = new PointConverter();
			break;
		case KnownElements.PointIListConverter:
			result = new PointIListConverter();
			break;
		case KnownElements.PointKeyFrameCollection:
			result = new PointKeyFrameCollection();
			break;
		case KnownElements.PointLight:
			result = new PointLight();
			break;
		case KnownElements.PolyBezierSegment:
			result = new PolyBezierSegment();
			break;
		case KnownElements.PolyLineSegment:
			result = new PolyLineSegment();
			break;
		case KnownElements.PolyQuadraticBezierSegment:
			result = new PolyQuadraticBezierSegment();
			break;
		case KnownElements.Polygon:
			result = new Polygon();
			break;
		case KnownElements.Polyline:
			result = new Polyline();
			break;
		case KnownElements.Popup:
			result = new Popup();
			break;
		case KnownElements.PriorityBinding:
			result = new PriorityBinding();
			break;
		case KnownElements.ProgressBar:
			result = new ProgressBar();
			break;
		case KnownElements.PropertyPathConverter:
			result = new PropertyPathConverter();
			break;
		case KnownElements.QuadraticBezierSegment:
			result = new QuadraticBezierSegment();
			break;
		case KnownElements.Quaternion:
			result = default(Quaternion);
			break;
		case KnownElements.QuaternionAnimation:
			result = new QuaternionAnimation();
			break;
		case KnownElements.QuaternionAnimationUsingKeyFrames:
			result = new QuaternionAnimationUsingKeyFrames();
			break;
		case KnownElements.QuaternionConverter:
			result = new QuaternionConverter();
			break;
		case KnownElements.QuaternionKeyFrameCollection:
			result = new QuaternionKeyFrameCollection();
			break;
		case KnownElements.QuaternionRotation3D:
			result = new QuaternionRotation3D();
			break;
		case KnownElements.RadialGradientBrush:
			result = new RadialGradientBrush();
			break;
		case KnownElements.RadioButton:
			result = new RadioButton();
			break;
		case KnownElements.Rect:
			result = default(Rect);
			break;
		case KnownElements.Rect3D:
			result = default(Rect3D);
			break;
		case KnownElements.Rect3DConverter:
			result = new Rect3DConverter();
			break;
		case KnownElements.RectAnimation:
			result = new RectAnimation();
			break;
		case KnownElements.RectAnimationUsingKeyFrames:
			result = new RectAnimationUsingKeyFrames();
			break;
		case KnownElements.RectConverter:
			result = new RectConverter();
			break;
		case KnownElements.RectKeyFrameCollection:
			result = new RectKeyFrameCollection();
			break;
		case KnownElements.Rectangle:
			result = new Rectangle();
			break;
		case KnownElements.RectangleGeometry:
			result = new RectangleGeometry();
			break;
		case KnownElements.RelativeSource:
			result = new RelativeSource();
			break;
		case KnownElements.RemoveStoryboard:
			result = new RemoveStoryboard();
			break;
		case KnownElements.RepeatBehavior:
			result = default(RepeatBehavior);
			break;
		case KnownElements.RepeatBehaviorConverter:
			result = new RepeatBehaviorConverter();
			break;
		case KnownElements.RepeatButton:
			result = new RepeatButton();
			break;
		case KnownElements.ResizeGrip:
			result = new ResizeGrip();
			break;
		case KnownElements.ResourceDictionary:
			result = new ResourceDictionary();
			break;
		case KnownElements.ResumeStoryboard:
			result = new ResumeStoryboard();
			break;
		case KnownElements.RichTextBox:
			result = new RichTextBox();
			break;
		case KnownElements.RotateTransform:
			result = new RotateTransform();
			break;
		case KnownElements.RotateTransform3D:
			result = new RotateTransform3D();
			break;
		case KnownElements.Rotation3DAnimation:
			result = new Rotation3DAnimation();
			break;
		case KnownElements.Rotation3DAnimationUsingKeyFrames:
			result = new Rotation3DAnimationUsingKeyFrames();
			break;
		case KnownElements.Rotation3DKeyFrameCollection:
			result = new Rotation3DKeyFrameCollection();
			break;
		case KnownElements.RoutedCommand:
			result = new RoutedCommand();
			break;
		case KnownElements.RoutedEventConverter:
			result = new RoutedEventConverter();
			break;
		case KnownElements.RoutedUICommand:
			result = new RoutedUICommand();
			break;
		case KnownElements.RowDefinition:
			result = new RowDefinition();
			break;
		case KnownElements.Run:
			result = new Run();
			break;
		case KnownElements.SByteConverter:
			result = new SByteConverter();
			break;
		case KnownElements.ScaleTransform:
			result = new ScaleTransform();
			break;
		case KnownElements.ScaleTransform3D:
			result = new ScaleTransform3D();
			break;
		case KnownElements.ScrollBar:
			result = new ScrollBar();
			break;
		case KnownElements.ScrollContentPresenter:
			result = new ScrollContentPresenter();
			break;
		case KnownElements.ScrollViewer:
			result = new ScrollViewer();
			break;
		case KnownElements.Section:
			result = new Section();
			break;
		case KnownElements.SeekStoryboard:
			result = new SeekStoryboard();
			break;
		case KnownElements.Separator:
			result = new Separator();
			break;
		case KnownElements.SetStoryboardSpeedRatio:
			result = new SetStoryboardSpeedRatio();
			break;
		case KnownElements.Setter:
			result = new Setter();
			break;
		case KnownElements.SingleAnimation:
			result = new SingleAnimation();
			break;
		case KnownElements.SingleAnimationUsingKeyFrames:
			result = new SingleAnimationUsingKeyFrames();
			break;
		case KnownElements.SingleConverter:
			result = new SingleConverter();
			break;
		case KnownElements.SingleKeyFrameCollection:
			result = new SingleKeyFrameCollection();
			break;
		case KnownElements.Size:
			result = default(Size);
			break;
		case KnownElements.Size3D:
			result = default(Size3D);
			break;
		case KnownElements.Size3DConverter:
			result = new Size3DConverter();
			break;
		case KnownElements.SizeAnimation:
			result = new SizeAnimation();
			break;
		case KnownElements.SizeAnimationUsingKeyFrames:
			result = new SizeAnimationUsingKeyFrames();
			break;
		case KnownElements.SizeConverter:
			result = new SizeConverter();
			break;
		case KnownElements.SizeKeyFrameCollection:
			result = new SizeKeyFrameCollection();
			break;
		case KnownElements.SkewTransform:
			result = new SkewTransform();
			break;
		case KnownElements.SkipStoryboardToFill:
			result = new SkipStoryboardToFill();
			break;
		case KnownElements.Slider:
			result = new Slider();
			break;
		case KnownElements.SolidColorBrush:
			result = new SolidColorBrush();
			break;
		case KnownElements.SoundPlayerAction:
			result = new SoundPlayerAction();
			break;
		case KnownElements.Span:
			result = new Span();
			break;
		case KnownElements.SpecularMaterial:
			result = new SpecularMaterial();
			break;
		case KnownElements.SplineByteKeyFrame:
			result = new SplineByteKeyFrame();
			break;
		case KnownElements.SplineColorKeyFrame:
			result = new SplineColorKeyFrame();
			break;
		case KnownElements.SplineDecimalKeyFrame:
			result = new SplineDecimalKeyFrame();
			break;
		case KnownElements.SplineDoubleKeyFrame:
			result = new SplineDoubleKeyFrame();
			break;
		case KnownElements.SplineInt16KeyFrame:
			result = new SplineInt16KeyFrame();
			break;
		case KnownElements.SplineInt32KeyFrame:
			result = new SplineInt32KeyFrame();
			break;
		case KnownElements.SplineInt64KeyFrame:
			result = new SplineInt64KeyFrame();
			break;
		case KnownElements.SplinePoint3DKeyFrame:
			result = new SplinePoint3DKeyFrame();
			break;
		case KnownElements.SplinePointKeyFrame:
			result = new SplinePointKeyFrame();
			break;
		case KnownElements.SplineQuaternionKeyFrame:
			result = new SplineQuaternionKeyFrame();
			break;
		case KnownElements.SplineRectKeyFrame:
			result = new SplineRectKeyFrame();
			break;
		case KnownElements.SplineRotation3DKeyFrame:
			result = new SplineRotation3DKeyFrame();
			break;
		case KnownElements.SplineSingleKeyFrame:
			result = new SplineSingleKeyFrame();
			break;
		case KnownElements.SplineSizeKeyFrame:
			result = new SplineSizeKeyFrame();
			break;
		case KnownElements.SplineThicknessKeyFrame:
			result = new SplineThicknessKeyFrame();
			break;
		case KnownElements.SplineVector3DKeyFrame:
			result = new SplineVector3DKeyFrame();
			break;
		case KnownElements.SplineVectorKeyFrame:
			result = new SplineVectorKeyFrame();
			break;
		case KnownElements.SpotLight:
			result = new SpotLight();
			break;
		case KnownElements.StackPanel:
			result = new StackPanel();
			break;
		case KnownElements.StaticExtension:
			result = new StaticExtension();
			break;
		case KnownElements.StaticResourceExtension:
			result = new StaticResourceExtension();
			break;
		case KnownElements.StatusBar:
			result = new StatusBar();
			break;
		case KnownElements.StatusBarItem:
			result = new StatusBarItem();
			break;
		case KnownElements.StopStoryboard:
			result = new StopStoryboard();
			break;
		case KnownElements.Storyboard:
			result = new Storyboard();
			break;
		case KnownElements.StreamGeometry:
			result = new StreamGeometry();
			break;
		case KnownElements.StreamResourceInfo:
			result = new StreamResourceInfo();
			break;
		case KnownElements.StringAnimationUsingKeyFrames:
			result = new StringAnimationUsingKeyFrames();
			break;
		case KnownElements.StringConverter:
			result = new StringConverter();
			break;
		case KnownElements.StringKeyFrameCollection:
			result = new StringKeyFrameCollection();
			break;
		case KnownElements.StrokeCollection:
			result = new StrokeCollection();
			break;
		case KnownElements.StrokeCollectionConverter:
			result = new StrokeCollectionConverter();
			break;
		case KnownElements.Style:
			result = new Style();
			break;
		case KnownElements.TabControl:
			result = new TabControl();
			break;
		case KnownElements.TabItem:
			result = new TabItem();
			break;
		case KnownElements.TabPanel:
			result = new TabPanel();
			break;
		case KnownElements.Table:
			result = new Table();
			break;
		case KnownElements.TableCell:
			result = new TableCell();
			break;
		case KnownElements.TableColumn:
			result = new TableColumn();
			break;
		case KnownElements.TableRow:
			result = new TableRow();
			break;
		case KnownElements.TableRowGroup:
			result = new TableRowGroup();
			break;
		case KnownElements.TemplateBindingExpressionConverter:
			result = new TemplateBindingExpressionConverter();
			break;
		case KnownElements.TemplateBindingExtension:
			result = new TemplateBindingExtension();
			break;
		case KnownElements.TemplateBindingExtensionConverter:
			result = new TemplateBindingExtensionConverter();
			break;
		case KnownElements.TemplateKeyConverter:
			result = new TemplateKeyConverter();
			break;
		case KnownElements.TextBlock:
			result = new TextBlock();
			break;
		case KnownElements.TextBox:
			result = new TextBox();
			break;
		case KnownElements.TextDecoration:
			result = new TextDecoration();
			break;
		case KnownElements.TextDecorationCollection:
			result = new TextDecorationCollection();
			break;
		case KnownElements.TextDecorationCollectionConverter:
			result = new TextDecorationCollectionConverter();
			break;
		case KnownElements.TextEffect:
			result = new TextEffect();
			break;
		case KnownElements.TextEffectCollection:
			result = new TextEffectCollection();
			break;
		case KnownElements.ThemeDictionaryExtension:
			result = new ThemeDictionaryExtension();
			break;
		case KnownElements.Thickness:
			result = default(Thickness);
			break;
		case KnownElements.ThicknessAnimation:
			result = new ThicknessAnimation();
			break;
		case KnownElements.ThicknessAnimationUsingKeyFrames:
			result = new ThicknessAnimationUsingKeyFrames();
			break;
		case KnownElements.ThicknessConverter:
			result = new ThicknessConverter();
			break;
		case KnownElements.ThicknessKeyFrameCollection:
			result = new ThicknessKeyFrameCollection();
			break;
		case KnownElements.Thumb:
			result = new Thumb();
			break;
		case KnownElements.TickBar:
			result = new TickBar();
			break;
		case KnownElements.TiffBitmapEncoder:
			result = new TiffBitmapEncoder();
			break;
		case KnownElements.TimeSpanConverter:
			result = new TimeSpanConverter();
			break;
		case KnownElements.TimelineCollection:
			result = new TimelineCollection();
			break;
		case KnownElements.ToggleButton:
			result = new ToggleButton();
			break;
		case KnownElements.ToolBar:
			result = new ToolBar();
			break;
		case KnownElements.ToolBarOverflowPanel:
			result = new ToolBarOverflowPanel();
			break;
		case KnownElements.ToolBarPanel:
			result = new ToolBarPanel();
			break;
		case KnownElements.ToolBarTray:
			result = new ToolBarTray();
			break;
		case KnownElements.ToolTip:
			result = new ToolTip();
			break;
		case KnownElements.Track:
			result = new Track();
			break;
		case KnownElements.Transform3DCollection:
			result = new Transform3DCollection();
			break;
		case KnownElements.Transform3DGroup:
			result = new Transform3DGroup();
			break;
		case KnownElements.TransformCollection:
			result = new TransformCollection();
			break;
		case KnownElements.TransformConverter:
			result = new TransformConverter();
			break;
		case KnownElements.TransformGroup:
			result = new TransformGroup();
			break;
		case KnownElements.TransformedBitmap:
			result = new TransformedBitmap();
			break;
		case KnownElements.TranslateTransform:
			result = new TranslateTransform();
			break;
		case KnownElements.TranslateTransform3D:
			result = new TranslateTransform3D();
			break;
		case KnownElements.TreeView:
			result = new TreeView();
			break;
		case KnownElements.TreeViewItem:
			result = new TreeViewItem();
			break;
		case KnownElements.Trigger:
			result = new Trigger();
			break;
		case KnownElements.TypeExtension:
			result = new TypeExtension();
			break;
		case KnownElements.TypeTypeConverter:
			result = new TypeTypeConverter();
			break;
		case KnownElements.UIElement:
			result = new UIElement();
			break;
		case KnownElements.UInt16Converter:
			result = new UInt16Converter();
			break;
		case KnownElements.UInt32Converter:
			result = new UInt32Converter();
			break;
		case KnownElements.UInt64Converter:
			result = new UInt64Converter();
			break;
		case KnownElements.UShortIListConverter:
			result = new UShortIListConverter();
			break;
		case KnownElements.Underline:
			result = new Underline();
			break;
		case KnownElements.UniformGrid:
			result = new UniformGrid();
			break;
		case KnownElements.UriTypeConverter:
			result = new UriTypeConverter();
			break;
		case KnownElements.UserControl:
			result = new UserControl();
			break;
		case KnownElements.Vector:
			result = default(Vector);
			break;
		case KnownElements.Vector3D:
			result = default(Vector3D);
			break;
		case KnownElements.Vector3DAnimation:
			result = new Vector3DAnimation();
			break;
		case KnownElements.Vector3DAnimationUsingKeyFrames:
			result = new Vector3DAnimationUsingKeyFrames();
			break;
		case KnownElements.Vector3DCollection:
			result = new Vector3DCollection();
			break;
		case KnownElements.Vector3DCollectionConverter:
			result = new Vector3DCollectionConverter();
			break;
		case KnownElements.Vector3DConverter:
			result = new Vector3DConverter();
			break;
		case KnownElements.Vector3DKeyFrameCollection:
			result = new Vector3DKeyFrameCollection();
			break;
		case KnownElements.VectorAnimation:
			result = new VectorAnimation();
			break;
		case KnownElements.VectorAnimationUsingKeyFrames:
			result = new VectorAnimationUsingKeyFrames();
			break;
		case KnownElements.VectorCollection:
			result = new VectorCollection();
			break;
		case KnownElements.VectorCollectionConverter:
			result = new VectorCollectionConverter();
			break;
		case KnownElements.VectorConverter:
			result = new VectorConverter();
			break;
		case KnownElements.VectorKeyFrameCollection:
			result = new VectorKeyFrameCollection();
			break;
		case KnownElements.VideoDrawing:
			result = new VideoDrawing();
			break;
		case KnownElements.Viewbox:
			result = new Viewbox();
			break;
		case KnownElements.Viewport3D:
			result = new Viewport3D();
			break;
		case KnownElements.Viewport3DVisual:
			result = new Viewport3DVisual();
			break;
		case KnownElements.VirtualizingStackPanel:
			result = new VirtualizingStackPanel();
			break;
		case KnownElements.VisualBrush:
			result = new VisualBrush();
			break;
		case KnownElements.Window:
			result = new Window();
			break;
		case KnownElements.WmpBitmapEncoder:
			result = new WmpBitmapEncoder();
			break;
		case KnownElements.WrapPanel:
			result = new WrapPanel();
			break;
		case KnownElements.XamlBrushSerializer:
			result = new XamlBrushSerializer();
			break;
		case KnownElements.XamlInt32CollectionSerializer:
			result = new XamlInt32CollectionSerializer();
			break;
		case KnownElements.XamlPathDataSerializer:
			result = new XamlPathDataSerializer();
			break;
		case KnownElements.XamlPoint3DCollectionSerializer:
			result = new XamlPoint3DCollectionSerializer();
			break;
		case KnownElements.XamlPointCollectionSerializer:
			result = new XamlPointCollectionSerializer();
			break;
		case KnownElements.XamlStyleSerializer:
			result = new XamlStyleSerializer();
			break;
		case KnownElements.XamlTemplateSerializer:
			result = new XamlTemplateSerializer();
			break;
		case KnownElements.XamlVector3DCollectionSerializer:
			result = new XamlVector3DCollectionSerializer();
			break;
		case KnownElements.XmlDataProvider:
			result = new XmlDataProvider();
			break;
		case KnownElements.XmlLanguageConverter:
			result = new XmlLanguageConverter();
			break;
		case KnownElements.XmlNamespaceMapping:
			result = new XmlNamespaceMapping();
			break;
		case KnownElements.ZoomPercentageConverter:
			result = new ZoomPercentageConverter();
			break;
		}
		return result;
	}

	internal static DependencyProperty GetKnownDependencyPropertyFromId(KnownProperties knownProperty)
	{
		return knownProperty switch
		{
			KnownProperties.AccessText_Text => AccessText.TextProperty, 
			KnownProperties.BeginStoryboard_Storyboard => BeginStoryboard.StoryboardProperty, 
			KnownProperties.BitmapEffectGroup_Children => BitmapEffectGroup.ChildrenProperty, 
			KnownProperties.Border_Background => Border.BackgroundProperty, 
			KnownProperties.Border_BorderBrush => Border.BorderBrushProperty, 
			KnownProperties.Border_BorderThickness => Border.BorderThicknessProperty, 
			KnownProperties.ButtonBase_Command => ButtonBase.CommandProperty, 
			KnownProperties.ButtonBase_CommandParameter => ButtonBase.CommandParameterProperty, 
			KnownProperties.ButtonBase_CommandTarget => ButtonBase.CommandTargetProperty, 
			KnownProperties.ButtonBase_IsPressed => ButtonBase.IsPressedProperty, 
			KnownProperties.ColumnDefinition_MaxWidth => ColumnDefinition.MaxWidthProperty, 
			KnownProperties.ColumnDefinition_MinWidth => ColumnDefinition.MinWidthProperty, 
			KnownProperties.ColumnDefinition_Width => ColumnDefinition.WidthProperty, 
			KnownProperties.ContentControl_Content => ContentControl.ContentProperty, 
			KnownProperties.ContentControl_ContentTemplate => ContentControl.ContentTemplateProperty, 
			KnownProperties.ContentControl_ContentTemplateSelector => ContentControl.ContentTemplateSelectorProperty, 
			KnownProperties.ContentControl_HasContent => ContentControl.HasContentProperty, 
			KnownProperties.ContentElement_Focusable => ContentElement.FocusableProperty, 
			KnownProperties.ContentPresenter_Content => ContentPresenter.ContentProperty, 
			KnownProperties.ContentPresenter_ContentSource => ContentPresenter.ContentSourceProperty, 
			KnownProperties.ContentPresenter_ContentTemplate => ContentPresenter.ContentTemplateProperty, 
			KnownProperties.ContentPresenter_ContentTemplateSelector => ContentPresenter.ContentTemplateSelectorProperty, 
			KnownProperties.ContentPresenter_RecognizesAccessKey => ContentPresenter.RecognizesAccessKeyProperty, 
			KnownProperties.Control_Background => Control.BackgroundProperty, 
			KnownProperties.Control_BorderBrush => Control.BorderBrushProperty, 
			KnownProperties.Control_BorderThickness => Control.BorderThicknessProperty, 
			KnownProperties.Control_FontFamily => Control.FontFamilyProperty, 
			KnownProperties.Control_FontSize => Control.FontSizeProperty, 
			KnownProperties.Control_FontStretch => Control.FontStretchProperty, 
			KnownProperties.Control_FontStyle => Control.FontStyleProperty, 
			KnownProperties.Control_FontWeight => Control.FontWeightProperty, 
			KnownProperties.Control_Foreground => Control.ForegroundProperty, 
			KnownProperties.Control_HorizontalContentAlignment => Control.HorizontalContentAlignmentProperty, 
			KnownProperties.Control_IsTabStop => Control.IsTabStopProperty, 
			KnownProperties.Control_Padding => Control.PaddingProperty, 
			KnownProperties.Control_TabIndex => Control.TabIndexProperty, 
			KnownProperties.Control_Template => Control.TemplateProperty, 
			KnownProperties.Control_VerticalContentAlignment => Control.VerticalContentAlignmentProperty, 
			KnownProperties.DockPanel_Dock => DockPanel.DockProperty, 
			KnownProperties.DockPanel_LastChildFill => DockPanel.LastChildFillProperty, 
			KnownProperties.DocumentViewerBase_Document => DocumentViewerBase.DocumentProperty, 
			KnownProperties.DrawingGroup_Children => DrawingGroup.ChildrenProperty, 
			KnownProperties.FlowDocumentReader_Document => FlowDocumentReader.DocumentProperty, 
			KnownProperties.FlowDocumentScrollViewer_Document => FlowDocumentScrollViewer.DocumentProperty, 
			KnownProperties.FrameworkContentElement_Style => FrameworkContentElement.StyleProperty, 
			KnownProperties.FrameworkElement_FlowDirection => FrameworkElement.FlowDirectionProperty, 
			KnownProperties.FrameworkElement_Height => FrameworkElement.HeightProperty, 
			KnownProperties.FrameworkElement_HorizontalAlignment => FrameworkElement.HorizontalAlignmentProperty, 
			KnownProperties.FrameworkElement_Margin => FrameworkElement.MarginProperty, 
			KnownProperties.FrameworkElement_MaxHeight => FrameworkElement.MaxHeightProperty, 
			KnownProperties.FrameworkElement_MaxWidth => FrameworkElement.MaxWidthProperty, 
			KnownProperties.FrameworkElement_MinHeight => FrameworkElement.MinHeightProperty, 
			KnownProperties.FrameworkElement_MinWidth => FrameworkElement.MinWidthProperty, 
			KnownProperties.FrameworkElement_Name => FrameworkElement.NameProperty, 
			KnownProperties.FrameworkElement_Style => FrameworkElement.StyleProperty, 
			KnownProperties.FrameworkElement_VerticalAlignment => FrameworkElement.VerticalAlignmentProperty, 
			KnownProperties.FrameworkElement_Width => FrameworkElement.WidthProperty, 
			KnownProperties.GeneralTransformGroup_Children => GeneralTransformGroup.ChildrenProperty, 
			KnownProperties.GeometryGroup_Children => GeometryGroup.ChildrenProperty, 
			KnownProperties.GradientBrush_GradientStops => GradientBrush.GradientStopsProperty, 
			KnownProperties.Grid_Column => Grid.ColumnProperty, 
			KnownProperties.Grid_ColumnSpan => Grid.ColumnSpanProperty, 
			KnownProperties.Grid_Row => Grid.RowProperty, 
			KnownProperties.Grid_RowSpan => Grid.RowSpanProperty, 
			KnownProperties.GridViewColumn_Header => GridViewColumn.HeaderProperty, 
			KnownProperties.HeaderedContentControl_HasHeader => HeaderedContentControl.HasHeaderProperty, 
			KnownProperties.HeaderedContentControl_Header => HeaderedContentControl.HeaderProperty, 
			KnownProperties.HeaderedContentControl_HeaderTemplate => HeaderedContentControl.HeaderTemplateProperty, 
			KnownProperties.HeaderedContentControl_HeaderTemplateSelector => HeaderedContentControl.HeaderTemplateSelectorProperty, 
			KnownProperties.HeaderedItemsControl_HasHeader => HeaderedItemsControl.HasHeaderProperty, 
			KnownProperties.HeaderedItemsControl_Header => HeaderedItemsControl.HeaderProperty, 
			KnownProperties.HeaderedItemsControl_HeaderTemplate => HeaderedItemsControl.HeaderTemplateProperty, 
			KnownProperties.HeaderedItemsControl_HeaderTemplateSelector => HeaderedItemsControl.HeaderTemplateSelectorProperty, 
			KnownProperties.Hyperlink_NavigateUri => Hyperlink.NavigateUriProperty, 
			KnownProperties.Image_Source => Image.SourceProperty, 
			KnownProperties.Image_Stretch => Image.StretchProperty, 
			KnownProperties.ItemsControl_ItemContainerStyle => ItemsControl.ItemContainerStyleProperty, 
			KnownProperties.ItemsControl_ItemContainerStyleSelector => ItemsControl.ItemContainerStyleSelectorProperty, 
			KnownProperties.ItemsControl_ItemTemplate => ItemsControl.ItemTemplateProperty, 
			KnownProperties.ItemsControl_ItemTemplateSelector => ItemsControl.ItemTemplateSelectorProperty, 
			KnownProperties.ItemsControl_ItemsPanel => ItemsControl.ItemsPanelProperty, 
			KnownProperties.ItemsControl_ItemsSource => ItemsControl.ItemsSourceProperty, 
			KnownProperties.MaterialGroup_Children => MaterialGroup.ChildrenProperty, 
			KnownProperties.Model3DGroup_Children => Model3DGroup.ChildrenProperty, 
			KnownProperties.Page_Content => Page.ContentProperty, 
			KnownProperties.Panel_Background => Panel.BackgroundProperty, 
			KnownProperties.Path_Data => Path.DataProperty, 
			KnownProperties.PathFigure_Segments => PathFigure.SegmentsProperty, 
			KnownProperties.PathGeometry_Figures => PathGeometry.FiguresProperty, 
			KnownProperties.Popup_Child => Popup.ChildProperty, 
			KnownProperties.Popup_IsOpen => Popup.IsOpenProperty, 
			KnownProperties.Popup_Placement => Popup.PlacementProperty, 
			KnownProperties.Popup_PopupAnimation => Popup.PopupAnimationProperty, 
			KnownProperties.RichTextBox_IsReadOnly => TextBoxBase.IsReadOnlyProperty, 
			KnownProperties.RowDefinition_Height => RowDefinition.HeightProperty, 
			KnownProperties.RowDefinition_MaxHeight => RowDefinition.MaxHeightProperty, 
			KnownProperties.RowDefinition_MinHeight => RowDefinition.MinHeightProperty, 
			KnownProperties.Run_Text => Run.TextProperty, 
			KnownProperties.ScrollViewer_CanContentScroll => ScrollViewer.CanContentScrollProperty, 
			KnownProperties.ScrollViewer_HorizontalScrollBarVisibility => ScrollViewer.HorizontalScrollBarVisibilityProperty, 
			KnownProperties.ScrollViewer_VerticalScrollBarVisibility => ScrollViewer.VerticalScrollBarVisibilityProperty, 
			KnownProperties.Shape_Fill => Shape.FillProperty, 
			KnownProperties.Shape_Stroke => Shape.StrokeProperty, 
			KnownProperties.Shape_StrokeThickness => Shape.StrokeThicknessProperty, 
			KnownProperties.TextBlock_Background => TextBlock.BackgroundProperty, 
			KnownProperties.TextBlock_FontFamily => TextBlock.FontFamilyProperty, 
			KnownProperties.TextBlock_FontSize => TextBlock.FontSizeProperty, 
			KnownProperties.TextBlock_FontStretch => TextBlock.FontStretchProperty, 
			KnownProperties.TextBlock_FontStyle => TextBlock.FontStyleProperty, 
			KnownProperties.TextBlock_FontWeight => TextBlock.FontWeightProperty, 
			KnownProperties.TextBlock_Foreground => TextBlock.ForegroundProperty, 
			KnownProperties.TextBlock_Text => TextBlock.TextProperty, 
			KnownProperties.TextBlock_TextDecorations => TextBlock.TextDecorationsProperty, 
			KnownProperties.TextBlock_TextTrimming => TextBlock.TextTrimmingProperty, 
			KnownProperties.TextBlock_TextWrapping => TextBlock.TextWrappingProperty, 
			KnownProperties.TextBox_Text => TextBox.TextProperty, 
			KnownProperties.TextBox_IsReadOnly => TextBoxBase.IsReadOnlyProperty, 
			KnownProperties.TextElement_Background => TextElement.BackgroundProperty, 
			KnownProperties.TextElement_FontFamily => TextElement.FontFamilyProperty, 
			KnownProperties.TextElement_FontSize => TextElement.FontSizeProperty, 
			KnownProperties.TextElement_FontStretch => TextElement.FontStretchProperty, 
			KnownProperties.TextElement_FontStyle => TextElement.FontStyleProperty, 
			KnownProperties.TextElement_FontWeight => TextElement.FontWeightProperty, 
			KnownProperties.TextElement_Foreground => TextElement.ForegroundProperty, 
			KnownProperties.TimelineGroup_Children => TimelineGroup.ChildrenProperty, 
			KnownProperties.Track_IsDirectionReversed => Track.IsDirectionReversedProperty, 
			KnownProperties.Track_Maximum => Track.MaximumProperty, 
			KnownProperties.Track_Minimum => Track.MinimumProperty, 
			KnownProperties.Track_Orientation => Track.OrientationProperty, 
			KnownProperties.Track_Value => Track.ValueProperty, 
			KnownProperties.Track_ViewportSize => Track.ViewportSizeProperty, 
			KnownProperties.Transform3DGroup_Children => Transform3DGroup.ChildrenProperty, 
			KnownProperties.TransformGroup_Children => TransformGroup.ChildrenProperty, 
			KnownProperties.UIElement_ClipToBounds => UIElement.ClipToBoundsProperty, 
			KnownProperties.UIElement_Focusable => UIElement.FocusableProperty, 
			KnownProperties.UIElement_IsEnabled => UIElement.IsEnabledProperty, 
			KnownProperties.UIElement_RenderTransform => UIElement.RenderTransformProperty, 
			KnownProperties.UIElement_Visibility => UIElement.VisibilityProperty, 
			KnownProperties.Viewport3D_Children => Viewport3D.ChildrenProperty, 
			_ => null, 
		};
	}

	internal static KnownElements GetKnownElementFromKnownCommonProperty(KnownProperties knownProperty)
	{
		switch (knownProperty)
		{
		case KnownProperties.AccessText_Text:
			return KnownElements.AccessText;
		case KnownProperties.AdornedElementPlaceholder_Child:
			return KnownElements.AdornedElementPlaceholder;
		case KnownProperties.AdornerDecorator_Child:
			return KnownElements.AdornerDecorator;
		case KnownProperties.AnchoredBlock_Blocks:
			return KnownElements.AnchoredBlock;
		case KnownProperties.ArrayExtension_Items:
			return KnownElements.ArrayExtension;
		case KnownProperties.BeginStoryboard_Storyboard:
			return KnownElements.BeginStoryboard;
		case KnownProperties.BitmapEffectGroup_Children:
			return KnownElements.BitmapEffectGroup;
		case KnownProperties.BlockUIContainer_Child:
			return KnownElements.BlockUIContainer;
		case KnownProperties.Bold_Inlines:
			return KnownElements.Bold;
		case KnownProperties.BooleanAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.BooleanAnimationUsingKeyFrames;
		case KnownProperties.Border_Background:
		case KnownProperties.Border_BorderBrush:
		case KnownProperties.Border_BorderThickness:
		case KnownProperties.Border_Child:
			return KnownElements.Border;
		case KnownProperties.BulletDecorator_Child:
			return KnownElements.BulletDecorator;
		case KnownProperties.Button_Content:
			return KnownElements.Button;
		case KnownProperties.ButtonBase_Command:
		case KnownProperties.ButtonBase_CommandParameter:
		case KnownProperties.ButtonBase_CommandTarget:
		case KnownProperties.ButtonBase_IsPressed:
		case KnownProperties.ButtonBase_Content:
			return KnownElements.ButtonBase;
		case KnownProperties.ByteAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.ByteAnimationUsingKeyFrames;
		case KnownProperties.Canvas_Children:
			return KnownElements.Canvas;
		case KnownProperties.CharAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.CharAnimationUsingKeyFrames;
		case KnownProperties.CheckBox_Content:
			return KnownElements.CheckBox;
		case KnownProperties.ColorAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.ColorAnimationUsingKeyFrames;
		case KnownProperties.ColumnDefinition_MaxWidth:
		case KnownProperties.ColumnDefinition_MinWidth:
		case KnownProperties.ColumnDefinition_Width:
			return KnownElements.ColumnDefinition;
		case KnownProperties.ComboBox_Items:
			return KnownElements.ComboBox;
		case KnownProperties.ComboBoxItem_Content:
			return KnownElements.ComboBoxItem;
		case KnownProperties.ContentControl_Content:
		case KnownProperties.ContentControl_ContentTemplate:
		case KnownProperties.ContentControl_ContentTemplateSelector:
		case KnownProperties.ContentControl_HasContent:
			return KnownElements.ContentControl;
		case KnownProperties.ContentElement_Focusable:
			return KnownElements.ContentElement;
		case KnownProperties.ContentPresenter_Content:
		case KnownProperties.ContentPresenter_ContentSource:
		case KnownProperties.ContentPresenter_ContentTemplate:
		case KnownProperties.ContentPresenter_ContentTemplateSelector:
		case KnownProperties.ContentPresenter_RecognizesAccessKey:
			return KnownElements.ContentPresenter;
		case KnownProperties.ContextMenu_Items:
			return KnownElements.ContextMenu;
		case KnownProperties.Control_Background:
		case KnownProperties.Control_BorderBrush:
		case KnownProperties.Control_BorderThickness:
		case KnownProperties.Control_FontFamily:
		case KnownProperties.Control_FontSize:
		case KnownProperties.Control_FontStretch:
		case KnownProperties.Control_FontStyle:
		case KnownProperties.Control_FontWeight:
		case KnownProperties.Control_Foreground:
		case KnownProperties.Control_HorizontalContentAlignment:
		case KnownProperties.Control_IsTabStop:
		case KnownProperties.Control_Padding:
		case KnownProperties.Control_TabIndex:
		case KnownProperties.Control_Template:
		case KnownProperties.Control_VerticalContentAlignment:
			return KnownElements.Control;
		case KnownProperties.ControlTemplate_VisualTree:
			return KnownElements.ControlTemplate;
		case KnownProperties.DataTemplate_VisualTree:
			return KnownElements.DataTemplate;
		case KnownProperties.DataTrigger_Setters:
			return KnownElements.DataTrigger;
		case KnownProperties.DecimalAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.DecimalAnimationUsingKeyFrames;
		case KnownProperties.Decorator_Child:
			return KnownElements.Decorator;
		case KnownProperties.DockPanel_Dock:
		case KnownProperties.DockPanel_LastChildFill:
		case KnownProperties.DockPanel_Children:
			return KnownElements.DockPanel;
		case KnownProperties.DocumentViewer_Document:
			return KnownElements.DocumentViewer;
		case KnownProperties.DocumentViewerBase_Document:
			return KnownElements.DocumentViewerBase;
		case KnownProperties.DoubleAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.DoubleAnimationUsingKeyFrames;
		case KnownProperties.DrawingGroup_Children:
			return KnownElements.DrawingGroup;
		case KnownProperties.EventTrigger_Actions:
			return KnownElements.EventTrigger;
		case KnownProperties.Expander_Content:
			return KnownElements.Expander;
		case KnownProperties.Figure_Blocks:
			return KnownElements.Figure;
		case KnownProperties.FixedDocument_Pages:
			return KnownElements.FixedDocument;
		case KnownProperties.FixedDocumentSequence_References:
			return KnownElements.FixedDocumentSequence;
		case KnownProperties.FixedPage_Children:
			return KnownElements.FixedPage;
		case KnownProperties.Floater_Blocks:
			return KnownElements.Floater;
		case KnownProperties.FlowDocument_Blocks:
			return KnownElements.FlowDocument;
		case KnownProperties.FlowDocumentPageViewer_Document:
			return KnownElements.FlowDocumentPageViewer;
		case KnownProperties.FlowDocumentReader_Document:
			return KnownElements.FlowDocumentReader;
		case KnownProperties.FlowDocumentScrollViewer_Document:
			return KnownElements.FlowDocumentScrollViewer;
		case KnownProperties.FrameworkContentElement_Style:
			return KnownElements.FrameworkContentElement;
		case KnownProperties.FrameworkElement_FlowDirection:
		case KnownProperties.FrameworkElement_Height:
		case KnownProperties.FrameworkElement_HorizontalAlignment:
		case KnownProperties.FrameworkElement_Margin:
		case KnownProperties.FrameworkElement_MaxHeight:
		case KnownProperties.FrameworkElement_MaxWidth:
		case KnownProperties.FrameworkElement_MinHeight:
		case KnownProperties.FrameworkElement_MinWidth:
		case KnownProperties.FrameworkElement_Name:
		case KnownProperties.FrameworkElement_Style:
		case KnownProperties.FrameworkElement_VerticalAlignment:
		case KnownProperties.FrameworkElement_Width:
			return KnownElements.FrameworkElement;
		case KnownProperties.FrameworkTemplate_VisualTree:
			return KnownElements.FrameworkTemplate;
		case KnownProperties.GeneralTransformGroup_Children:
			return KnownElements.GeneralTransformGroup;
		case KnownProperties.GeometryGroup_Children:
			return KnownElements.GeometryGroup;
		case KnownProperties.GradientBrush_GradientStops:
			return KnownElements.GradientBrush;
		case KnownProperties.Grid_Column:
		case KnownProperties.Grid_ColumnSpan:
		case KnownProperties.Grid_Row:
		case KnownProperties.Grid_RowSpan:
		case KnownProperties.Grid_Children:
			return KnownElements.Grid;
		case KnownProperties.GridView_Columns:
			return KnownElements.GridView;
		case KnownProperties.GridViewColumn_Header:
			return KnownElements.GridViewColumn;
		case KnownProperties.GridViewColumnHeader_Content:
			return KnownElements.GridViewColumnHeader;
		case KnownProperties.GroupBox_Content:
			return KnownElements.GroupBox;
		case KnownProperties.GroupItem_Content:
			return KnownElements.GroupItem;
		case KnownProperties.HeaderedContentControl_HasHeader:
		case KnownProperties.HeaderedContentControl_Header:
		case KnownProperties.HeaderedContentControl_HeaderTemplate:
		case KnownProperties.HeaderedContentControl_HeaderTemplateSelector:
		case KnownProperties.HeaderedContentControl_Content:
			return KnownElements.HeaderedContentControl;
		case KnownProperties.HeaderedItemsControl_HasHeader:
		case KnownProperties.HeaderedItemsControl_Header:
		case KnownProperties.HeaderedItemsControl_HeaderTemplate:
		case KnownProperties.HeaderedItemsControl_HeaderTemplateSelector:
		case KnownProperties.HeaderedItemsControl_Items:
			return KnownElements.HeaderedItemsControl;
		case KnownProperties.HierarchicalDataTemplate_VisualTree:
			return KnownElements.HierarchicalDataTemplate;
		case KnownProperties.Hyperlink_NavigateUri:
		case KnownProperties.Hyperlink_Inlines:
			return KnownElements.Hyperlink;
		case KnownProperties.Image_Source:
		case KnownProperties.Image_Stretch:
			return KnownElements.Image;
		case KnownProperties.InkCanvas_Children:
			return KnownElements.InkCanvas;
		case KnownProperties.InkPresenter_Child:
			return KnownElements.InkPresenter;
		case KnownProperties.InlineUIContainer_Child:
			return KnownElements.InlineUIContainer;
		case KnownProperties.InputScopeName_NameValue:
			return KnownElements.InputScopeName;
		case KnownProperties.Int16AnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Int16AnimationUsingKeyFrames;
		case KnownProperties.Int32AnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Int32AnimationUsingKeyFrames;
		case KnownProperties.Int64AnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Int64AnimationUsingKeyFrames;
		case KnownProperties.Italic_Inlines:
			return KnownElements.Italic;
		case KnownProperties.ItemsControl_ItemContainerStyle:
		case KnownProperties.ItemsControl_ItemContainerStyleSelector:
		case KnownProperties.ItemsControl_ItemTemplate:
		case KnownProperties.ItemsControl_ItemTemplateSelector:
		case KnownProperties.ItemsControl_ItemsPanel:
		case KnownProperties.ItemsControl_ItemsSource:
		case KnownProperties.ItemsControl_Items:
			return KnownElements.ItemsControl;
		case KnownProperties.ItemsPanelTemplate_VisualTree:
			return KnownElements.ItemsPanelTemplate;
		case KnownProperties.Label_Content:
			return KnownElements.Label;
		case KnownProperties.LinearGradientBrush_GradientStops:
			return KnownElements.LinearGradientBrush;
		case KnownProperties.List_ListItems:
			return KnownElements.List;
		case KnownProperties.ListBox_Items:
			return KnownElements.ListBox;
		case KnownProperties.ListBoxItem_Content:
			return KnownElements.ListBoxItem;
		case KnownProperties.ListItem_Blocks:
			return KnownElements.ListItem;
		case KnownProperties.ListView_Items:
			return KnownElements.ListView;
		case KnownProperties.ListViewItem_Content:
			return KnownElements.ListViewItem;
		case KnownProperties.MaterialGroup_Children:
			return KnownElements.MaterialGroup;
		case KnownProperties.MatrixAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.MatrixAnimationUsingKeyFrames;
		case KnownProperties.Menu_Items:
			return KnownElements.Menu;
		case KnownProperties.MenuBase_Items:
			return KnownElements.MenuBase;
		case KnownProperties.MenuItem_Items:
			return KnownElements.MenuItem;
		case KnownProperties.Model3DGroup_Children:
			return KnownElements.Model3DGroup;
		case KnownProperties.ModelVisual3D_Children:
			return KnownElements.ModelVisual3D;
		case KnownProperties.MultiBinding_Bindings:
			return KnownElements.MultiBinding;
		case KnownProperties.MultiDataTrigger_Setters:
			return KnownElements.MultiDataTrigger;
		case KnownProperties.MultiTrigger_Setters:
			return KnownElements.MultiTrigger;
		case KnownProperties.ObjectAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.ObjectAnimationUsingKeyFrames;
		case KnownProperties.Page_Content:
			return KnownElements.Page;
		case KnownProperties.PageContent_Child:
			return KnownElements.PageContent;
		case KnownProperties.PageFunctionBase_Content:
			return KnownElements.PageFunctionBase;
		case KnownProperties.Panel_Background:
		case KnownProperties.Panel_Children:
			return KnownElements.Panel;
		case KnownProperties.Paragraph_Inlines:
			return KnownElements.Paragraph;
		case KnownProperties.ParallelTimeline_Children:
			return KnownElements.ParallelTimeline;
		case KnownProperties.Path_Data:
			return KnownElements.Path;
		case KnownProperties.PathFigure_Segments:
			return KnownElements.PathFigure;
		case KnownProperties.PathGeometry_Figures:
			return KnownElements.PathGeometry;
		case KnownProperties.Point3DAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Point3DAnimationUsingKeyFrames;
		case KnownProperties.PointAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.PointAnimationUsingKeyFrames;
		case KnownProperties.Popup_Child:
		case KnownProperties.Popup_IsOpen:
		case KnownProperties.Popup_Placement:
		case KnownProperties.Popup_PopupAnimation:
			return KnownElements.Popup;
		case KnownProperties.PriorityBinding_Bindings:
			return KnownElements.PriorityBinding;
		case KnownProperties.QuaternionAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.QuaternionAnimationUsingKeyFrames;
		case KnownProperties.RadialGradientBrush_GradientStops:
			return KnownElements.RadialGradientBrush;
		case KnownProperties.RadioButton_Content:
			return KnownElements.RadioButton;
		case KnownProperties.RectAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.RectAnimationUsingKeyFrames;
		case KnownProperties.RepeatButton_Content:
			return KnownElements.RepeatButton;
		case KnownProperties.RichTextBox_Document:
			return KnownElements.RichTextBox;
		case KnownProperties.RichTextBox_IsReadOnly:
			return KnownElements.RichTextBox;
		case KnownProperties.Rotation3DAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Rotation3DAnimationUsingKeyFrames;
		case KnownProperties.RowDefinition_Height:
		case KnownProperties.RowDefinition_MaxHeight:
		case KnownProperties.RowDefinition_MinHeight:
			return KnownElements.RowDefinition;
		case KnownProperties.Run_Text:
			return KnownElements.Run;
		case KnownProperties.ScrollViewer_CanContentScroll:
		case KnownProperties.ScrollViewer_HorizontalScrollBarVisibility:
		case KnownProperties.ScrollViewer_VerticalScrollBarVisibility:
		case KnownProperties.ScrollViewer_Content:
			return KnownElements.ScrollViewer;
		case KnownProperties.Section_Blocks:
			return KnownElements.Section;
		case KnownProperties.Selector_Items:
			return KnownElements.Selector;
		case KnownProperties.Shape_Fill:
		case KnownProperties.Shape_Stroke:
		case KnownProperties.Shape_StrokeThickness:
			return KnownElements.Shape;
		case KnownProperties.SingleAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.SingleAnimationUsingKeyFrames;
		case KnownProperties.SizeAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.SizeAnimationUsingKeyFrames;
		case KnownProperties.Span_Inlines:
			return KnownElements.Span;
		case KnownProperties.StackPanel_Children:
			return KnownElements.StackPanel;
		case KnownProperties.StatusBar_Items:
			return KnownElements.StatusBar;
		case KnownProperties.StatusBarItem_Content:
			return KnownElements.StatusBarItem;
		case KnownProperties.Storyboard_Children:
			return KnownElements.Storyboard;
		case KnownProperties.StringAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.StringAnimationUsingKeyFrames;
		case KnownProperties.Style_Setters:
			return KnownElements.Style;
		case KnownProperties.TabControl_Items:
			return KnownElements.TabControl;
		case KnownProperties.TabItem_Content:
			return KnownElements.TabItem;
		case KnownProperties.TabPanel_Children:
			return KnownElements.TabPanel;
		case KnownProperties.Table_RowGroups:
			return KnownElements.Table;
		case KnownProperties.TableCell_Blocks:
			return KnownElements.TableCell;
		case KnownProperties.TableRow_Cells:
			return KnownElements.TableRow;
		case KnownProperties.TableRowGroup_Rows:
			return KnownElements.TableRowGroup;
		case KnownProperties.TextBlock_Background:
		case KnownProperties.TextBlock_FontFamily:
		case KnownProperties.TextBlock_FontSize:
		case KnownProperties.TextBlock_FontStretch:
		case KnownProperties.TextBlock_FontStyle:
		case KnownProperties.TextBlock_FontWeight:
		case KnownProperties.TextBlock_Foreground:
		case KnownProperties.TextBlock_Text:
		case KnownProperties.TextBlock_TextDecorations:
		case KnownProperties.TextBlock_TextTrimming:
		case KnownProperties.TextBlock_TextWrapping:
		case KnownProperties.TextBlock_Inlines:
			return KnownElements.TextBlock;
		case KnownProperties.TextBox_Text:
			return KnownElements.TextBox;
		case KnownProperties.TextBox_IsReadOnly:
			return KnownElements.TextBox;
		case KnownProperties.TextElement_Background:
		case KnownProperties.TextElement_FontFamily:
		case KnownProperties.TextElement_FontSize:
		case KnownProperties.TextElement_FontStretch:
		case KnownProperties.TextElement_FontStyle:
		case KnownProperties.TextElement_FontWeight:
		case KnownProperties.TextElement_Foreground:
			return KnownElements.TextElement;
		case KnownProperties.ThicknessAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.ThicknessAnimationUsingKeyFrames;
		case KnownProperties.TimelineGroup_Children:
			return KnownElements.TimelineGroup;
		case KnownProperties.ToggleButton_Content:
			return KnownElements.ToggleButton;
		case KnownProperties.ToolBar_Items:
			return KnownElements.ToolBar;
		case KnownProperties.ToolBarOverflowPanel_Children:
			return KnownElements.ToolBarOverflowPanel;
		case KnownProperties.ToolBarPanel_Children:
			return KnownElements.ToolBarPanel;
		case KnownProperties.ToolBarTray_ToolBars:
			return KnownElements.ToolBarTray;
		case KnownProperties.ToolTip_Content:
			return KnownElements.ToolTip;
		case KnownProperties.Track_IsDirectionReversed:
		case KnownProperties.Track_Maximum:
		case KnownProperties.Track_Minimum:
		case KnownProperties.Track_Orientation:
		case KnownProperties.Track_Value:
		case KnownProperties.Track_ViewportSize:
			return KnownElements.Track;
		case KnownProperties.Transform3DGroup_Children:
			return KnownElements.Transform3DGroup;
		case KnownProperties.TransformGroup_Children:
			return KnownElements.TransformGroup;
		case KnownProperties.TreeView_Items:
			return KnownElements.TreeView;
		case KnownProperties.TreeViewItem_Items:
			return KnownElements.TreeViewItem;
		case KnownProperties.Trigger_Setters:
			return KnownElements.Trigger;
		case KnownProperties.UIElement_ClipToBounds:
		case KnownProperties.UIElement_Focusable:
		case KnownProperties.UIElement_IsEnabled:
		case KnownProperties.UIElement_RenderTransform:
		case KnownProperties.UIElement_Visibility:
			return KnownElements.UIElement;
		case KnownProperties.Underline_Inlines:
			return KnownElements.Underline;
		case KnownProperties.UniformGrid_Children:
			return KnownElements.UniformGrid;
		case KnownProperties.UserControl_Content:
			return KnownElements.UserControl;
		case KnownProperties.Vector3DAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.Vector3DAnimationUsingKeyFrames;
		case KnownProperties.VectorAnimationUsingKeyFrames_KeyFrames:
			return KnownElements.VectorAnimationUsingKeyFrames;
		case KnownProperties.Viewbox_Child:
			return KnownElements.Viewbox;
		case KnownProperties.Viewport3D_Children:
			return KnownElements.Viewport3D;
		case KnownProperties.Viewport3DVisual_Children:
			return KnownElements.Viewport3DVisual;
		case KnownProperties.VirtualizingPanel_Children:
			return KnownElements.VirtualizingPanel;
		case KnownProperties.VirtualizingStackPanel_Children:
			return KnownElements.VirtualizingStackPanel;
		case KnownProperties.Window_Content:
			return KnownElements.Window;
		case KnownProperties.WrapPanel_Children:
			return KnownElements.WrapPanel;
		case KnownProperties.XmlDataProvider_XmlSerializer:
			return KnownElements.XmlDataProvider;
		default:
			return KnownElements.UnknownElement;
		}
	}

	internal static string GetKnownClrPropertyNameFromId(KnownProperties knownProperty)
	{
		return GetContentPropertyName(GetKnownElementFromKnownCommonProperty(knownProperty));
	}

	internal static IList GetCollectionForCPA(object o, KnownElements knownElement)
	{
		switch (knownElement)
		{
		case KnownElements.Canvas:
		case KnownElements.DockPanel:
		case KnownElements.Grid:
		case KnownElements.Panel:
		case KnownElements.StackPanel:
		case KnownElements.TabPanel:
		case KnownElements.ToolBarOverflowPanel:
		case KnownElements.ToolBarPanel:
		case KnownElements.UniformGrid:
		case KnownElements.VirtualizingPanel:
		case KnownElements.VirtualizingStackPanel:
		case KnownElements.WrapPanel:
			return (o as Panel).Children;
		case KnownElements.ComboBox:
		case KnownElements.ContextMenu:
		case KnownElements.HeaderedItemsControl:
		case KnownElements.ItemsControl:
		case KnownElements.ListBox:
		case KnownElements.ListView:
		case KnownElements.Menu:
		case KnownElements.MenuBase:
		case KnownElements.MenuItem:
		case KnownElements.Selector:
		case KnownElements.StatusBar:
		case KnownElements.TabControl:
		case KnownElements.ToolBar:
		case KnownElements.TreeView:
		case KnownElements.TreeViewItem:
			return (o as ItemsControl).Items;
		case KnownElements.Bold:
		case KnownElements.Hyperlink:
		case KnownElements.Italic:
		case KnownElements.Span:
		case KnownElements.Underline:
			return (o as Span).Inlines;
		case KnownElements.AnchoredBlock:
		case KnownElements.Figure:
		case KnownElements.Floater:
			return (o as AnchoredBlock).Blocks;
		case KnownElements.GradientBrush:
		case KnownElements.LinearGradientBrush:
		case KnownElements.RadialGradientBrush:
			return (o as GradientBrush).GradientStops;
		case KnownElements.ParallelTimeline:
		case KnownElements.Storyboard:
		case KnownElements.TimelineGroup:
			return (o as TimelineGroup).Children;
		case KnownElements.BitmapEffectGroup:
			return (o as BitmapEffectGroup).Children;
		case KnownElements.BooleanAnimationUsingKeyFrames:
			return (o as BooleanAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.ByteAnimationUsingKeyFrames:
			return (o as ByteAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.CharAnimationUsingKeyFrames:
			return (o as CharAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.ColorAnimationUsingKeyFrames:
			return (o as ColorAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.DataTrigger:
			return (o as DataTrigger).Setters;
		case KnownElements.DecimalAnimationUsingKeyFrames:
			return (o as DecimalAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.DoubleAnimationUsingKeyFrames:
			return (o as DoubleAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.DrawingGroup:
			return (o as DrawingGroup).Children;
		case KnownElements.EventTrigger:
			return (o as EventTrigger).Actions;
		case KnownElements.FixedPage:
			return (o as FixedPage).Children;
		case KnownElements.FlowDocument:
			return (o as FlowDocument).Blocks;
		case KnownElements.GeneralTransformGroup:
			return (o as GeneralTransformGroup).Children;
		case KnownElements.GeometryGroup:
			return (o as GeometryGroup).Children;
		case KnownElements.GridView:
			return (o as GridView).Columns;
		case KnownElements.InkCanvas:
			return (o as InkCanvas).Children;
		case KnownElements.Int16AnimationUsingKeyFrames:
			return (o as Int16AnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Int32AnimationUsingKeyFrames:
			return (o as Int32AnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Int64AnimationUsingKeyFrames:
			return (o as Int64AnimationUsingKeyFrames).KeyFrames;
		case KnownElements.List:
			return (o as List).ListItems;
		case KnownElements.ListItem:
			return (o as ListItem).Blocks;
		case KnownElements.MaterialGroup:
			return (o as MaterialGroup).Children;
		case KnownElements.MatrixAnimationUsingKeyFrames:
			return (o as MatrixAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Model3DGroup:
			return (o as Model3DGroup).Children;
		case KnownElements.ModelVisual3D:
			return (o as ModelVisual3D).Children;
		case KnownElements.MultiBinding:
			return (o as MultiBinding).Bindings;
		case KnownElements.MultiDataTrigger:
			return (o as MultiDataTrigger).Setters;
		case KnownElements.MultiTrigger:
			return (o as MultiTrigger).Setters;
		case KnownElements.ObjectAnimationUsingKeyFrames:
			return (o as ObjectAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Paragraph:
			return (o as Paragraph).Inlines;
		case KnownElements.PathFigure:
			return (o as PathFigure).Segments;
		case KnownElements.PathGeometry:
			return (o as PathGeometry).Figures;
		case KnownElements.Point3DAnimationUsingKeyFrames:
			return (o as Point3DAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.PointAnimationUsingKeyFrames:
			return (o as PointAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.PriorityBinding:
			return (o as PriorityBinding).Bindings;
		case KnownElements.QuaternionAnimationUsingKeyFrames:
			return (o as QuaternionAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.RectAnimationUsingKeyFrames:
			return (o as RectAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Rotation3DAnimationUsingKeyFrames:
			return (o as Rotation3DAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Section:
			return (o as Section).Blocks;
		case KnownElements.SingleAnimationUsingKeyFrames:
			return (o as SingleAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.SizeAnimationUsingKeyFrames:
			return (o as SizeAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.StringAnimationUsingKeyFrames:
			return (o as StringAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Style:
			return (o as Style).Setters;
		case KnownElements.Table:
			return (o as Table).RowGroups;
		case KnownElements.TableCell:
			return (o as TableCell).Blocks;
		case KnownElements.TableRow:
			return (o as TableRow).Cells;
		case KnownElements.TableRowGroup:
			return (o as TableRowGroup).Rows;
		case KnownElements.TextBlock:
			return (o as TextBlock).Inlines;
		case KnownElements.ThicknessAnimationUsingKeyFrames:
			return (o as ThicknessAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.ToolBarTray:
			return (o as ToolBarTray).ToolBars;
		case KnownElements.Transform3DGroup:
			return (o as Transform3DGroup).Children;
		case KnownElements.TransformGroup:
			return (o as TransformGroup).Children;
		case KnownElements.Trigger:
			return (o as Trigger).Setters;
		case KnownElements.Vector3DAnimationUsingKeyFrames:
			return (o as Vector3DAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.VectorAnimationUsingKeyFrames:
			return (o as VectorAnimationUsingKeyFrames).KeyFrames;
		case KnownElements.Viewport3D:
			return (o as Viewport3D).Children;
		case KnownElements.Viewport3DVisual:
			return (o as Viewport3DVisual).Children;
		default:
			return null;
		}
	}

	internal static bool CanCollectionTypeAcceptStrings(KnownElements knownElement)
	{
		switch (knownElement)
		{
		case KnownElements.BitmapEffectCollection:
		case KnownElements.DoubleCollection:
		case KnownElements.DrawingCollection:
		case KnownElements.GeneralTransformCollection:
		case KnownElements.GeometryCollection:
		case KnownElements.GradientStopCollection:
		case KnownElements.Int32Collection:
		case KnownElements.MaterialCollection:
		case KnownElements.Model3DCollection:
		case KnownElements.PathFigureCollection:
		case KnownElements.PathSegmentCollection:
		case KnownElements.Point3DCollection:
		case KnownElements.PointCollection:
		case KnownElements.StrokeCollection:
		case KnownElements.TextDecorationCollection:
		case KnownElements.TextEffectCollection:
		case KnownElements.TimelineCollection:
		case KnownElements.Transform3DCollection:
		case KnownElements.TransformCollection:
		case KnownElements.Vector3DCollection:
		case KnownElements.VectorCollection:
			return false;
		default:
			return true;
		}
	}

	internal static string GetContentPropertyName(KnownElements knownElement)
	{
		string result = null;
		switch (knownElement)
		{
		case KnownElements.EventTrigger:
			result = "Actions";
			break;
		case KnownElements.MultiBinding:
		case KnownElements.PriorityBinding:
			result = "Bindings";
			break;
		case KnownElements.AnchoredBlock:
		case KnownElements.Figure:
		case KnownElements.Floater:
		case KnownElements.FlowDocument:
		case KnownElements.ListItem:
		case KnownElements.Section:
		case KnownElements.TableCell:
			result = "Blocks";
			break;
		case KnownElements.TableRow:
			result = "Cells";
			break;
		case KnownElements.AdornedElementPlaceholder:
		case KnownElements.AdornerDecorator:
		case KnownElements.BlockUIContainer:
		case KnownElements.Border:
		case KnownElements.BulletDecorator:
		case KnownElements.Decorator:
		case KnownElements.InkPresenter:
		case KnownElements.InlineUIContainer:
		case KnownElements.PageContent:
		case KnownElements.Popup:
		case KnownElements.Viewbox:
			result = "Child";
			break;
		case KnownElements.BitmapEffectGroup:
		case KnownElements.Canvas:
		case KnownElements.DockPanel:
		case KnownElements.DrawingGroup:
		case KnownElements.FixedPage:
		case KnownElements.GeneralTransformGroup:
		case KnownElements.GeometryGroup:
		case KnownElements.Grid:
		case KnownElements.InkCanvas:
		case KnownElements.MaterialGroup:
		case KnownElements.Model3DGroup:
		case KnownElements.ModelVisual3D:
		case KnownElements.Panel:
		case KnownElements.ParallelTimeline:
		case KnownElements.StackPanel:
		case KnownElements.Storyboard:
		case KnownElements.TabPanel:
		case KnownElements.TimelineGroup:
		case KnownElements.ToolBarOverflowPanel:
		case KnownElements.ToolBarPanel:
		case KnownElements.Transform3DGroup:
		case KnownElements.TransformGroup:
		case KnownElements.UniformGrid:
		case KnownElements.Viewport3D:
		case KnownElements.Viewport3DVisual:
		case KnownElements.VirtualizingPanel:
		case KnownElements.VirtualizingStackPanel:
		case KnownElements.WrapPanel:
			result = "Children";
			break;
		case KnownElements.GridView:
			result = "Columns";
			break;
		case KnownElements.Button:
		case KnownElements.ButtonBase:
		case KnownElements.CheckBox:
		case KnownElements.ComboBoxItem:
		case KnownElements.ContentControl:
		case KnownElements.Expander:
		case KnownElements.GridViewColumnHeader:
		case KnownElements.GroupBox:
		case KnownElements.GroupItem:
		case KnownElements.HeaderedContentControl:
		case KnownElements.Label:
		case KnownElements.ListBoxItem:
		case KnownElements.ListViewItem:
		case KnownElements.Page:
		case KnownElements.PageFunctionBase:
		case KnownElements.RadioButton:
		case KnownElements.RepeatButton:
		case KnownElements.ScrollViewer:
		case KnownElements.StatusBarItem:
		case KnownElements.TabItem:
		case KnownElements.ToggleButton:
		case KnownElements.ToolTip:
		case KnownElements.UserControl:
		case KnownElements.Window:
			result = "Content";
			break;
		case KnownElements.DocumentViewer:
		case KnownElements.DocumentViewerBase:
		case KnownElements.FlowDocumentPageViewer:
		case KnownElements.FlowDocumentReader:
		case KnownElements.FlowDocumentScrollViewer:
		case KnownElements.RichTextBox:
			result = "Document";
			break;
		case KnownElements.PathGeometry:
			result = "Figures";
			break;
		case KnownElements.GradientBrush:
		case KnownElements.LinearGradientBrush:
		case KnownElements.RadialGradientBrush:
			result = "GradientStops";
			break;
		case KnownElements.GridViewColumn:
			result = "Header";
			break;
		case KnownElements.Bold:
		case KnownElements.Hyperlink:
		case KnownElements.Italic:
		case KnownElements.Paragraph:
		case KnownElements.Span:
		case KnownElements.TextBlock:
		case KnownElements.Underline:
			result = "Inlines";
			break;
		case KnownElements.ArrayExtension:
		case KnownElements.ComboBox:
		case KnownElements.ContextMenu:
		case KnownElements.HeaderedItemsControl:
		case KnownElements.ItemsControl:
		case KnownElements.ListBox:
		case KnownElements.ListView:
		case KnownElements.Menu:
		case KnownElements.MenuBase:
		case KnownElements.MenuItem:
		case KnownElements.Selector:
		case KnownElements.StatusBar:
		case KnownElements.TabControl:
		case KnownElements.ToolBar:
		case KnownElements.TreeView:
		case KnownElements.TreeViewItem:
			result = "Items";
			break;
		case KnownElements.BooleanAnimationUsingKeyFrames:
		case KnownElements.ByteAnimationUsingKeyFrames:
		case KnownElements.CharAnimationUsingKeyFrames:
		case KnownElements.ColorAnimationUsingKeyFrames:
		case KnownElements.DecimalAnimationUsingKeyFrames:
		case KnownElements.DoubleAnimationUsingKeyFrames:
		case KnownElements.Int16AnimationUsingKeyFrames:
		case KnownElements.Int32AnimationUsingKeyFrames:
		case KnownElements.Int64AnimationUsingKeyFrames:
		case KnownElements.MatrixAnimationUsingKeyFrames:
		case KnownElements.ObjectAnimationUsingKeyFrames:
		case KnownElements.Point3DAnimationUsingKeyFrames:
		case KnownElements.PointAnimationUsingKeyFrames:
		case KnownElements.QuaternionAnimationUsingKeyFrames:
		case KnownElements.RectAnimationUsingKeyFrames:
		case KnownElements.Rotation3DAnimationUsingKeyFrames:
		case KnownElements.SingleAnimationUsingKeyFrames:
		case KnownElements.SizeAnimationUsingKeyFrames:
		case KnownElements.StringAnimationUsingKeyFrames:
		case KnownElements.ThicknessAnimationUsingKeyFrames:
		case KnownElements.Vector3DAnimationUsingKeyFrames:
		case KnownElements.VectorAnimationUsingKeyFrames:
			result = "KeyFrames";
			break;
		case KnownElements.List:
			result = "ListItems";
			break;
		case KnownElements.InputScopeName:
			result = "NameValue";
			break;
		case KnownElements.FixedDocument:
			result = "Pages";
			break;
		case KnownElements.FixedDocumentSequence:
			result = "References";
			break;
		case KnownElements.Table:
			result = "RowGroups";
			break;
		case KnownElements.TableRowGroup:
			result = "Rows";
			break;
		case KnownElements.PathFigure:
			result = "Segments";
			break;
		case KnownElements.DataTrigger:
		case KnownElements.MultiDataTrigger:
		case KnownElements.MultiTrigger:
		case KnownElements.Style:
		case KnownElements.Trigger:
			result = "Setters";
			break;
		case KnownElements.BeginStoryboard:
			result = "Storyboard";
			break;
		case KnownElements.AccessText:
		case KnownElements.Run:
		case KnownElements.TextBox:
			result = "Text";
			break;
		case KnownElements.ToolBarTray:
			result = "ToolBars";
			break;
		case KnownElements.ControlTemplate:
		case KnownElements.DataTemplate:
		case KnownElements.FrameworkTemplate:
		case KnownElements.HierarchicalDataTemplate:
		case KnownElements.ItemsPanelTemplate:
			result = "VisualTree";
			break;
		case KnownElements.XmlDataProvider:
			result = "XmlSerializer";
			break;
		}
		return result;
	}

	internal static short GetKnownPropertyAttributeId(KnownElements typeID, string fieldName)
	{
		switch (typeID)
		{
		case KnownElements.AccessText:
			if (string.Equals(fieldName, "Text", StringComparison.Ordinal))
			{
				return 1;
			}
			break;
		case KnownElements.AdornedElementPlaceholder:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 138;
			}
			break;
		case KnownElements.AdornerDecorator:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 139;
			}
			break;
		case KnownElements.AnchoredBlock:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 140;
			}
			break;
		case KnownElements.ArrayExtension:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 141;
			}
			break;
		case KnownElements.BeginStoryboard:
			if (string.Equals(fieldName, "Storyboard", StringComparison.Ordinal))
			{
				return 2;
			}
			break;
		case KnownElements.BitmapEffectGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 3;
			}
			break;
		case KnownElements.BlockUIContainer:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 142;
			}
			break;
		case KnownElements.Bold:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 143;
			}
			break;
		case KnownElements.BooleanAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 144;
			}
			break;
		case KnownElements.Border:
			if (string.Equals(fieldName, "Background", StringComparison.Ordinal))
			{
				return 4;
			}
			if (string.Equals(fieldName, "BorderBrush", StringComparison.Ordinal))
			{
				return 5;
			}
			if (string.Equals(fieldName, "BorderThickness", StringComparison.Ordinal))
			{
				return 6;
			}
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 145;
			}
			break;
		case KnownElements.BulletDecorator:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 146;
			}
			break;
		case KnownElements.Button:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 147;
			}
			break;
		case KnownElements.ButtonBase:
			if (string.Equals(fieldName, "Command", StringComparison.Ordinal))
			{
				return 7;
			}
			if (string.Equals(fieldName, "CommandParameter", StringComparison.Ordinal))
			{
				return 8;
			}
			if (string.Equals(fieldName, "CommandTarget", StringComparison.Ordinal))
			{
				return 9;
			}
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 148;
			}
			if (string.Equals(fieldName, "IsPressed", StringComparison.Ordinal))
			{
				return 10;
			}
			break;
		case KnownElements.ByteAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 149;
			}
			break;
		case KnownElements.Canvas:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 150;
			}
			break;
		case KnownElements.CharAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 151;
			}
			break;
		case KnownElements.CheckBox:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 152;
			}
			break;
		case KnownElements.ColorAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 153;
			}
			break;
		case KnownElements.ColumnDefinition:
			if (string.Equals(fieldName, "MaxWidth", StringComparison.Ordinal))
			{
				return 11;
			}
			if (string.Equals(fieldName, "MinWidth", StringComparison.Ordinal))
			{
				return 12;
			}
			if (string.Equals(fieldName, "Width", StringComparison.Ordinal))
			{
				return 13;
			}
			break;
		case KnownElements.ComboBox:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 154;
			}
			break;
		case KnownElements.ComboBoxItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 155;
			}
			break;
		case KnownElements.ContentControl:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 14;
			}
			if (string.Equals(fieldName, "ContentTemplate", StringComparison.Ordinal))
			{
				return 15;
			}
			if (string.Equals(fieldName, "ContentTemplateSelector", StringComparison.Ordinal))
			{
				return 16;
			}
			if (string.Equals(fieldName, "HasContent", StringComparison.Ordinal))
			{
				return 17;
			}
			break;
		case KnownElements.ContentElement:
			if (string.Equals(fieldName, "Focusable", StringComparison.Ordinal))
			{
				return 18;
			}
			break;
		case KnownElements.ContentPresenter:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 19;
			}
			if (string.Equals(fieldName, "ContentSource", StringComparison.Ordinal))
			{
				return 20;
			}
			if (string.Equals(fieldName, "ContentTemplate", StringComparison.Ordinal))
			{
				return 21;
			}
			if (string.Equals(fieldName, "ContentTemplateSelector", StringComparison.Ordinal))
			{
				return 22;
			}
			if (string.Equals(fieldName, "RecognizesAccessKey", StringComparison.Ordinal))
			{
				return 23;
			}
			break;
		case KnownElements.ContextMenu:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 156;
			}
			break;
		case KnownElements.Control:
			if (string.Equals(fieldName, "Background", StringComparison.Ordinal))
			{
				return 24;
			}
			if (string.Equals(fieldName, "BorderBrush", StringComparison.Ordinal))
			{
				return 25;
			}
			if (string.Equals(fieldName, "BorderThickness", StringComparison.Ordinal))
			{
				return 26;
			}
			if (string.Equals(fieldName, "FontFamily", StringComparison.Ordinal))
			{
				return 27;
			}
			if (string.Equals(fieldName, "FontSize", StringComparison.Ordinal))
			{
				return 28;
			}
			if (string.Equals(fieldName, "FontStretch", StringComparison.Ordinal))
			{
				return 29;
			}
			if (string.Equals(fieldName, "FontStyle", StringComparison.Ordinal))
			{
				return 30;
			}
			if (string.Equals(fieldName, "FontWeight", StringComparison.Ordinal))
			{
				return 31;
			}
			if (string.Equals(fieldName, "Foreground", StringComparison.Ordinal))
			{
				return 32;
			}
			if (string.Equals(fieldName, "HorizontalContentAlignment", StringComparison.Ordinal))
			{
				return 33;
			}
			if (string.Equals(fieldName, "IsTabStop", StringComparison.Ordinal))
			{
				return 34;
			}
			if (string.Equals(fieldName, "Padding", StringComparison.Ordinal))
			{
				return 35;
			}
			if (string.Equals(fieldName, "TabIndex", StringComparison.Ordinal))
			{
				return 36;
			}
			if (string.Equals(fieldName, "Template", StringComparison.Ordinal))
			{
				return 37;
			}
			if (string.Equals(fieldName, "VerticalContentAlignment", StringComparison.Ordinal))
			{
				return 38;
			}
			break;
		case KnownElements.ControlTemplate:
			if (string.Equals(fieldName, "VisualTree", StringComparison.Ordinal))
			{
				return 157;
			}
			break;
		case KnownElements.DataTemplate:
			if (string.Equals(fieldName, "VisualTree", StringComparison.Ordinal))
			{
				return 158;
			}
			break;
		case KnownElements.DataTrigger:
			if (string.Equals(fieldName, "Setters", StringComparison.Ordinal))
			{
				return 159;
			}
			break;
		case KnownElements.DecimalAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 160;
			}
			break;
		case KnownElements.Decorator:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 161;
			}
			break;
		case KnownElements.DockPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 162;
			}
			if (string.Equals(fieldName, "Dock", StringComparison.Ordinal))
			{
				return 39;
			}
			if (string.Equals(fieldName, "LastChildFill", StringComparison.Ordinal))
			{
				return 40;
			}
			break;
		case KnownElements.DocumentViewer:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 163;
			}
			break;
		case KnownElements.DocumentViewerBase:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 41;
			}
			break;
		case KnownElements.DoubleAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 164;
			}
			break;
		case KnownElements.DrawingGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 42;
			}
			break;
		case KnownElements.EventTrigger:
			if (string.Equals(fieldName, "Actions", StringComparison.Ordinal))
			{
				return 165;
			}
			break;
		case KnownElements.Expander:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 166;
			}
			break;
		case KnownElements.Figure:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 167;
			}
			break;
		case KnownElements.FixedDocument:
			if (string.Equals(fieldName, "Pages", StringComparison.Ordinal))
			{
				return 168;
			}
			break;
		case KnownElements.FixedDocumentSequence:
			if (string.Equals(fieldName, "References", StringComparison.Ordinal))
			{
				return 169;
			}
			break;
		case KnownElements.FixedPage:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 170;
			}
			break;
		case KnownElements.Floater:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 171;
			}
			break;
		case KnownElements.FlowDocument:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 172;
			}
			break;
		case KnownElements.FlowDocumentPageViewer:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 173;
			}
			break;
		case KnownElements.FlowDocumentReader:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 43;
			}
			break;
		case KnownElements.FlowDocumentScrollViewer:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 44;
			}
			break;
		case KnownElements.FrameworkContentElement:
			if (string.Equals(fieldName, "Style", StringComparison.Ordinal))
			{
				return 45;
			}
			break;
		case KnownElements.FrameworkElement:
			if (string.Equals(fieldName, "FlowDirection", StringComparison.Ordinal))
			{
				return 46;
			}
			if (string.Equals(fieldName, "Height", StringComparison.Ordinal))
			{
				return 47;
			}
			if (string.Equals(fieldName, "HorizontalAlignment", StringComparison.Ordinal))
			{
				return 48;
			}
			if (string.Equals(fieldName, "Margin", StringComparison.Ordinal))
			{
				return 49;
			}
			if (string.Equals(fieldName, "MaxHeight", StringComparison.Ordinal))
			{
				return 50;
			}
			if (string.Equals(fieldName, "MaxWidth", StringComparison.Ordinal))
			{
				return 51;
			}
			if (string.Equals(fieldName, "MinHeight", StringComparison.Ordinal))
			{
				return 52;
			}
			if (string.Equals(fieldName, "MinWidth", StringComparison.Ordinal))
			{
				return 53;
			}
			if (string.Equals(fieldName, "Name", StringComparison.Ordinal))
			{
				return 54;
			}
			if (string.Equals(fieldName, "Style", StringComparison.Ordinal))
			{
				return 55;
			}
			if (string.Equals(fieldName, "VerticalAlignment", StringComparison.Ordinal))
			{
				return 56;
			}
			if (string.Equals(fieldName, "Width", StringComparison.Ordinal))
			{
				return 57;
			}
			break;
		case KnownElements.FrameworkTemplate:
			if (string.Equals(fieldName, "VisualTree", StringComparison.Ordinal))
			{
				return 174;
			}
			break;
		case KnownElements.GeneralTransformGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 58;
			}
			break;
		case KnownElements.GeometryGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 59;
			}
			break;
		case KnownElements.GradientBrush:
			if (string.Equals(fieldName, "GradientStops", StringComparison.Ordinal))
			{
				return 60;
			}
			break;
		case KnownElements.Grid:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 175;
			}
			if (string.Equals(fieldName, "Column", StringComparison.Ordinal))
			{
				return 61;
			}
			if (string.Equals(fieldName, "ColumnSpan", StringComparison.Ordinal))
			{
				return 62;
			}
			if (string.Equals(fieldName, "Row", StringComparison.Ordinal))
			{
				return 63;
			}
			if (string.Equals(fieldName, "RowSpan", StringComparison.Ordinal))
			{
				return 64;
			}
			break;
		case KnownElements.GridView:
			if (string.Equals(fieldName, "Columns", StringComparison.Ordinal))
			{
				return 176;
			}
			break;
		case KnownElements.GridViewColumn:
			if (string.Equals(fieldName, "Header", StringComparison.Ordinal))
			{
				return 65;
			}
			break;
		case KnownElements.GridViewColumnHeader:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 177;
			}
			break;
		case KnownElements.GroupBox:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 178;
			}
			break;
		case KnownElements.GroupItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 179;
			}
			break;
		case KnownElements.HeaderedContentControl:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 180;
			}
			if (string.Equals(fieldName, "HasHeader", StringComparison.Ordinal))
			{
				return 66;
			}
			if (string.Equals(fieldName, "Header", StringComparison.Ordinal))
			{
				return 67;
			}
			if (string.Equals(fieldName, "HeaderTemplate", StringComparison.Ordinal))
			{
				return 68;
			}
			if (string.Equals(fieldName, "HeaderTemplateSelector", StringComparison.Ordinal))
			{
				return 69;
			}
			break;
		case KnownElements.HeaderedItemsControl:
			if (string.Equals(fieldName, "HasHeader", StringComparison.Ordinal))
			{
				return 70;
			}
			if (string.Equals(fieldName, "Header", StringComparison.Ordinal))
			{
				return 71;
			}
			if (string.Equals(fieldName, "HeaderTemplate", StringComparison.Ordinal))
			{
				return 72;
			}
			if (string.Equals(fieldName, "HeaderTemplateSelector", StringComparison.Ordinal))
			{
				return 73;
			}
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 181;
			}
			break;
		case KnownElements.HierarchicalDataTemplate:
			if (string.Equals(fieldName, "VisualTree", StringComparison.Ordinal))
			{
				return 182;
			}
			break;
		case KnownElements.Hyperlink:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 183;
			}
			if (string.Equals(fieldName, "NavigateUri", StringComparison.Ordinal))
			{
				return 74;
			}
			break;
		case KnownElements.Image:
			if (string.Equals(fieldName, "Source", StringComparison.Ordinal))
			{
				return 75;
			}
			if (string.Equals(fieldName, "Stretch", StringComparison.Ordinal))
			{
				return 76;
			}
			break;
		case KnownElements.InkCanvas:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 184;
			}
			break;
		case KnownElements.InkPresenter:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 185;
			}
			break;
		case KnownElements.InlineUIContainer:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 186;
			}
			break;
		case KnownElements.InputScopeName:
			if (string.Equals(fieldName, "NameValue", StringComparison.Ordinal))
			{
				return 187;
			}
			break;
		case KnownElements.Int16AnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 188;
			}
			break;
		case KnownElements.Int32AnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 189;
			}
			break;
		case KnownElements.Int64AnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 190;
			}
			break;
		case KnownElements.Italic:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 191;
			}
			break;
		case KnownElements.ItemsControl:
			if (string.Equals(fieldName, "ItemContainerStyle", StringComparison.Ordinal))
			{
				return 77;
			}
			if (string.Equals(fieldName, "ItemContainerStyleSelector", StringComparison.Ordinal))
			{
				return 78;
			}
			if (string.Equals(fieldName, "ItemTemplate", StringComparison.Ordinal))
			{
				return 79;
			}
			if (string.Equals(fieldName, "ItemTemplateSelector", StringComparison.Ordinal))
			{
				return 80;
			}
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 192;
			}
			if (string.Equals(fieldName, "ItemsPanel", StringComparison.Ordinal))
			{
				return 81;
			}
			if (string.Equals(fieldName, "ItemsSource", StringComparison.Ordinal))
			{
				return 82;
			}
			break;
		case KnownElements.ItemsPanelTemplate:
			if (string.Equals(fieldName, "VisualTree", StringComparison.Ordinal))
			{
				return 193;
			}
			break;
		case KnownElements.Label:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 194;
			}
			break;
		case KnownElements.LinearGradientBrush:
			if (string.Equals(fieldName, "GradientStops", StringComparison.Ordinal))
			{
				return 195;
			}
			break;
		case KnownElements.List:
			if (string.Equals(fieldName, "ListItems", StringComparison.Ordinal))
			{
				return 196;
			}
			break;
		case KnownElements.ListBox:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 197;
			}
			break;
		case KnownElements.ListBoxItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 198;
			}
			break;
		case KnownElements.ListItem:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 199;
			}
			break;
		case KnownElements.ListView:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 200;
			}
			break;
		case KnownElements.ListViewItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 201;
			}
			break;
		case KnownElements.MaterialGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 83;
			}
			break;
		case KnownElements.MatrixAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 202;
			}
			break;
		case KnownElements.Menu:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 203;
			}
			break;
		case KnownElements.MenuBase:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 204;
			}
			break;
		case KnownElements.MenuItem:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 205;
			}
			break;
		case KnownElements.Model3DGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 84;
			}
			break;
		case KnownElements.ModelVisual3D:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 206;
			}
			break;
		case KnownElements.MultiBinding:
			if (string.Equals(fieldName, "Bindings", StringComparison.Ordinal))
			{
				return 207;
			}
			break;
		case KnownElements.MultiDataTrigger:
			if (string.Equals(fieldName, "Setters", StringComparison.Ordinal))
			{
				return 208;
			}
			break;
		case KnownElements.MultiTrigger:
			if (string.Equals(fieldName, "Setters", StringComparison.Ordinal))
			{
				return 209;
			}
			break;
		case KnownElements.ObjectAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 210;
			}
			break;
		case KnownElements.Page:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 85;
			}
			break;
		case KnownElements.PageContent:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 211;
			}
			break;
		case KnownElements.PageFunctionBase:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 212;
			}
			break;
		case KnownElements.Panel:
			if (string.Equals(fieldName, "Background", StringComparison.Ordinal))
			{
				return 86;
			}
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 213;
			}
			break;
		case KnownElements.Paragraph:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 214;
			}
			break;
		case KnownElements.ParallelTimeline:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 215;
			}
			break;
		case KnownElements.Path:
			if (string.Equals(fieldName, "Data", StringComparison.Ordinal))
			{
				return 87;
			}
			break;
		case KnownElements.PathFigure:
			if (string.Equals(fieldName, "Segments", StringComparison.Ordinal))
			{
				return 88;
			}
			break;
		case KnownElements.PathGeometry:
			if (string.Equals(fieldName, "Figures", StringComparison.Ordinal))
			{
				return 89;
			}
			break;
		case KnownElements.Point3DAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 216;
			}
			break;
		case KnownElements.PointAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 217;
			}
			break;
		case KnownElements.Popup:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 90;
			}
			if (string.Equals(fieldName, "IsOpen", StringComparison.Ordinal))
			{
				return 91;
			}
			if (string.Equals(fieldName, "Placement", StringComparison.Ordinal))
			{
				return 92;
			}
			if (string.Equals(fieldName, "PopupAnimation", StringComparison.Ordinal))
			{
				return 93;
			}
			break;
		case KnownElements.PriorityBinding:
			if (string.Equals(fieldName, "Bindings", StringComparison.Ordinal))
			{
				return 218;
			}
			break;
		case KnownElements.QuaternionAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 219;
			}
			break;
		case KnownElements.RadialGradientBrush:
			if (string.Equals(fieldName, "GradientStops", StringComparison.Ordinal))
			{
				return 220;
			}
			break;
		case KnownElements.RadioButton:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 221;
			}
			break;
		case KnownElements.RectAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 222;
			}
			break;
		case KnownElements.RepeatButton:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 223;
			}
			break;
		case KnownElements.RichTextBox:
			if (string.Equals(fieldName, "Document", StringComparison.Ordinal))
			{
				return 224;
			}
			if (string.Equals(fieldName, "IsReadOnly", StringComparison.Ordinal))
			{
				return 271;
			}
			break;
		case KnownElements.Rotation3DAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 225;
			}
			break;
		case KnownElements.RowDefinition:
			if (string.Equals(fieldName, "Height", StringComparison.Ordinal))
			{
				return 94;
			}
			if (string.Equals(fieldName, "MaxHeight", StringComparison.Ordinal))
			{
				return 95;
			}
			if (string.Equals(fieldName, "MinHeight", StringComparison.Ordinal))
			{
				return 96;
			}
			break;
		case KnownElements.Run:
			if (string.Equals(fieldName, "Text", StringComparison.Ordinal))
			{
				return 226;
			}
			break;
		case KnownElements.ScrollViewer:
			if (string.Equals(fieldName, "CanContentScroll", StringComparison.Ordinal))
			{
				return 97;
			}
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 227;
			}
			if (string.Equals(fieldName, "HorizontalScrollBarVisibility", StringComparison.Ordinal))
			{
				return 98;
			}
			if (string.Equals(fieldName, "VerticalScrollBarVisibility", StringComparison.Ordinal))
			{
				return 99;
			}
			break;
		case KnownElements.Section:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 228;
			}
			break;
		case KnownElements.Selector:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 229;
			}
			break;
		case KnownElements.Shape:
			if (string.Equals(fieldName, "Fill", StringComparison.Ordinal))
			{
				return 100;
			}
			if (string.Equals(fieldName, "Stroke", StringComparison.Ordinal))
			{
				return 101;
			}
			if (string.Equals(fieldName, "StrokeThickness", StringComparison.Ordinal))
			{
				return 102;
			}
			break;
		case KnownElements.SingleAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 230;
			}
			break;
		case KnownElements.SizeAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 231;
			}
			break;
		case KnownElements.Span:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 232;
			}
			break;
		case KnownElements.StackPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 233;
			}
			break;
		case KnownElements.StatusBar:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 234;
			}
			break;
		case KnownElements.StatusBarItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 235;
			}
			break;
		case KnownElements.Storyboard:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 236;
			}
			break;
		case KnownElements.StringAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 237;
			}
			break;
		case KnownElements.Style:
			if (string.Equals(fieldName, "Setters", StringComparison.Ordinal))
			{
				return 238;
			}
			break;
		case KnownElements.TabControl:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 239;
			}
			break;
		case KnownElements.TabItem:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 240;
			}
			break;
		case KnownElements.TabPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 241;
			}
			break;
		case KnownElements.Table:
			if (string.Equals(fieldName, "RowGroups", StringComparison.Ordinal))
			{
				return 242;
			}
			break;
		case KnownElements.TableCell:
			if (string.Equals(fieldName, "Blocks", StringComparison.Ordinal))
			{
				return 243;
			}
			break;
		case KnownElements.TableRow:
			if (string.Equals(fieldName, "Cells", StringComparison.Ordinal))
			{
				return 244;
			}
			break;
		case KnownElements.TableRowGroup:
			if (string.Equals(fieldName, "Rows", StringComparison.Ordinal))
			{
				return 245;
			}
			break;
		case KnownElements.TextBlock:
			if (string.Equals(fieldName, "Background", StringComparison.Ordinal))
			{
				return 103;
			}
			if (string.Equals(fieldName, "FontFamily", StringComparison.Ordinal))
			{
				return 104;
			}
			if (string.Equals(fieldName, "FontSize", StringComparison.Ordinal))
			{
				return 105;
			}
			if (string.Equals(fieldName, "FontStretch", StringComparison.Ordinal))
			{
				return 106;
			}
			if (string.Equals(fieldName, "FontStyle", StringComparison.Ordinal))
			{
				return 107;
			}
			if (string.Equals(fieldName, "FontWeight", StringComparison.Ordinal))
			{
				return 108;
			}
			if (string.Equals(fieldName, "Foreground", StringComparison.Ordinal))
			{
				return 109;
			}
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 246;
			}
			if (string.Equals(fieldName, "Text", StringComparison.Ordinal))
			{
				return 110;
			}
			if (string.Equals(fieldName, "TextDecorations", StringComparison.Ordinal))
			{
				return 111;
			}
			if (string.Equals(fieldName, "TextTrimming", StringComparison.Ordinal))
			{
				return 112;
			}
			if (string.Equals(fieldName, "TextWrapping", StringComparison.Ordinal))
			{
				return 113;
			}
			break;
		case KnownElements.TextBox:
			if (string.Equals(fieldName, "Text", StringComparison.Ordinal))
			{
				return 114;
			}
			if (string.Equals(fieldName, "IsReadOnly", StringComparison.Ordinal))
			{
				return 270;
			}
			break;
		case KnownElements.TextElement:
			if (string.Equals(fieldName, "Background", StringComparison.Ordinal))
			{
				return 115;
			}
			if (string.Equals(fieldName, "FontFamily", StringComparison.Ordinal))
			{
				return 116;
			}
			if (string.Equals(fieldName, "FontSize", StringComparison.Ordinal))
			{
				return 117;
			}
			if (string.Equals(fieldName, "FontStretch", StringComparison.Ordinal))
			{
				return 118;
			}
			if (string.Equals(fieldName, "FontStyle", StringComparison.Ordinal))
			{
				return 119;
			}
			if (string.Equals(fieldName, "FontWeight", StringComparison.Ordinal))
			{
				return 120;
			}
			if (string.Equals(fieldName, "Foreground", StringComparison.Ordinal))
			{
				return 121;
			}
			break;
		case KnownElements.ThicknessAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 247;
			}
			break;
		case KnownElements.TimelineGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 122;
			}
			break;
		case KnownElements.ToggleButton:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 248;
			}
			break;
		case KnownElements.ToolBar:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 249;
			}
			break;
		case KnownElements.ToolBarOverflowPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 250;
			}
			break;
		case KnownElements.ToolBarPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 251;
			}
			break;
		case KnownElements.ToolBarTray:
			if (string.Equals(fieldName, "ToolBars", StringComparison.Ordinal))
			{
				return 252;
			}
			break;
		case KnownElements.ToolTip:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 253;
			}
			break;
		case KnownElements.Track:
			if (string.Equals(fieldName, "IsDirectionReversed", StringComparison.Ordinal))
			{
				return 123;
			}
			if (string.Equals(fieldName, "Maximum", StringComparison.Ordinal))
			{
				return 124;
			}
			if (string.Equals(fieldName, "Minimum", StringComparison.Ordinal))
			{
				return 125;
			}
			if (string.Equals(fieldName, "Orientation", StringComparison.Ordinal))
			{
				return 126;
			}
			if (string.Equals(fieldName, "Value", StringComparison.Ordinal))
			{
				return 127;
			}
			if (string.Equals(fieldName, "ViewportSize", StringComparison.Ordinal))
			{
				return 128;
			}
			break;
		case KnownElements.Transform3DGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 129;
			}
			break;
		case KnownElements.TransformGroup:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 130;
			}
			break;
		case KnownElements.TreeView:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 254;
			}
			break;
		case KnownElements.TreeViewItem:
			if (string.Equals(fieldName, "Items", StringComparison.Ordinal))
			{
				return 255;
			}
			break;
		case KnownElements.Trigger:
			if (string.Equals(fieldName, "Setters", StringComparison.Ordinal))
			{
				return 256;
			}
			break;
		case KnownElements.UIElement:
			if (string.Equals(fieldName, "ClipToBounds", StringComparison.Ordinal))
			{
				return 131;
			}
			if (string.Equals(fieldName, "Focusable", StringComparison.Ordinal))
			{
				return 132;
			}
			if (string.Equals(fieldName, "IsEnabled", StringComparison.Ordinal))
			{
				return 133;
			}
			if (string.Equals(fieldName, "RenderTransform", StringComparison.Ordinal))
			{
				return 134;
			}
			if (string.Equals(fieldName, "Visibility", StringComparison.Ordinal))
			{
				return 135;
			}
			break;
		case KnownElements.Underline:
			if (string.Equals(fieldName, "Inlines", StringComparison.Ordinal))
			{
				return 257;
			}
			break;
		case KnownElements.UniformGrid:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 258;
			}
			break;
		case KnownElements.UserControl:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 259;
			}
			break;
		case KnownElements.Vector3DAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 260;
			}
			break;
		case KnownElements.VectorAnimationUsingKeyFrames:
			if (string.Equals(fieldName, "KeyFrames", StringComparison.Ordinal))
			{
				return 261;
			}
			break;
		case KnownElements.Viewbox:
			if (string.Equals(fieldName, "Child", StringComparison.Ordinal))
			{
				return 262;
			}
			break;
		case KnownElements.Viewport3D:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 136;
			}
			break;
		case KnownElements.Viewport3DVisual:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 263;
			}
			break;
		case KnownElements.VirtualizingPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 264;
			}
			break;
		case KnownElements.VirtualizingStackPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 265;
			}
			break;
		case KnownElements.Window:
			if (string.Equals(fieldName, "Content", StringComparison.Ordinal))
			{
				return 266;
			}
			break;
		case KnownElements.WrapPanel:
			if (string.Equals(fieldName, "Children", StringComparison.Ordinal))
			{
				return 267;
			}
			break;
		case KnownElements.XmlDataProvider:
			if (string.Equals(fieldName, "XmlSerializer", StringComparison.Ordinal))
			{
				return 268;
			}
			break;
		}
		return 0;
	}

	private static bool IsStandardLengthProp(string propName)
	{
		if (!string.Equals(propName, "Width", StringComparison.Ordinal) && !string.Equals(propName, "MinWidth", StringComparison.Ordinal) && !string.Equals(propName, "MaxWidth", StringComparison.Ordinal) && !string.Equals(propName, "Height", StringComparison.Ordinal) && !string.Equals(propName, "MinHeight", StringComparison.Ordinal))
		{
			return string.Equals(propName, "MaxHeight", StringComparison.Ordinal);
		}
		return true;
	}

	internal static KnownElements GetKnownTypeConverterId(KnownElements knownElement)
	{
		KnownElements result = KnownElements.UnknownElement;
		switch (knownElement)
		{
		case KnownElements.ComponentResourceKey:
			result = KnownElements.ComponentResourceKeyConverter;
			break;
		case KnownElements.CornerRadius:
			result = KnownElements.CornerRadiusConverter;
			break;
		case KnownElements.BindingExpressionBase:
			result = KnownElements.ExpressionConverter;
			break;
		case KnownElements.BindingExpression:
			result = KnownElements.ExpressionConverter;
			break;
		case KnownElements.MultiBindingExpression:
			result = KnownElements.ExpressionConverter;
			break;
		case KnownElements.PriorityBindingExpression:
			result = KnownElements.ExpressionConverter;
			break;
		case KnownElements.TemplateKey:
			result = KnownElements.TemplateKeyConverter;
			break;
		case KnownElements.DataTemplateKey:
			result = KnownElements.TemplateKeyConverter;
			break;
		case KnownElements.DynamicResourceExtension:
			result = KnownElements.DynamicResourceExtensionConverter;
			break;
		case KnownElements.FigureLength:
			result = KnownElements.FigureLengthConverter;
			break;
		case KnownElements.GridLength:
			result = KnownElements.GridLengthConverter;
			break;
		case KnownElements.PropertyPath:
			result = KnownElements.PropertyPathConverter;
			break;
		case KnownElements.TemplateBindingExpression:
			result = KnownElements.TemplateBindingExpressionConverter;
			break;
		case KnownElements.TemplateBindingExtension:
			result = KnownElements.TemplateBindingExtensionConverter;
			break;
		case KnownElements.Thickness:
			result = KnownElements.ThicknessConverter;
			break;
		case KnownElements.Duration:
			result = KnownElements.DurationConverter;
			break;
		case KnownElements.FontStyle:
			result = KnownElements.FontStyleConverter;
			break;
		case KnownElements.FontStretch:
			result = KnownElements.FontStretchConverter;
			break;
		case KnownElements.FontWeight:
			result = KnownElements.FontWeightConverter;
			break;
		case KnownElements.RoutedEvent:
			result = KnownElements.RoutedEventConverter;
			break;
		case KnownElements.TextDecorationCollection:
			result = KnownElements.TextDecorationCollectionConverter;
			break;
		case KnownElements.StrokeCollection:
			result = KnownElements.StrokeCollectionConverter;
			break;
		case KnownElements.ICommand:
			result = KnownElements.CommandConverter;
			break;
		case KnownElements.KeyGesture:
			result = KnownElements.KeyGestureConverter;
			break;
		case KnownElements.MouseGesture:
			result = KnownElements.MouseGestureConverter;
			break;
		case KnownElements.RoutedCommand:
			result = KnownElements.CommandConverter;
			break;
		case KnownElements.RoutedUICommand:
			result = KnownElements.CommandConverter;
			break;
		case KnownElements.Cursor:
			result = KnownElements.CursorConverter;
			break;
		case KnownElements.InputScope:
			result = KnownElements.InputScopeConverter;
			break;
		case KnownElements.InputScopeName:
			result = KnownElements.InputScopeNameConverter;
			break;
		case KnownElements.KeySpline:
			result = KnownElements.KeySplineConverter;
			break;
		case KnownElements.KeyTime:
			result = KnownElements.KeyTimeConverter;
			break;
		case KnownElements.RepeatBehavior:
			result = KnownElements.RepeatBehaviorConverter;
			break;
		case KnownElements.Brush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.Color:
			result = KnownElements.ColorConverter;
			break;
		case KnownElements.Geometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.CombinedGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.TileBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.DrawingBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.ImageSource:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.DrawingImage:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.EllipseGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.FontFamily:
			result = KnownElements.FontFamilyConverter;
			break;
		case KnownElements.DoubleCollection:
			result = KnownElements.DoubleCollectionConverter;
			break;
		case KnownElements.GeometryGroup:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.GradientBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.ImageBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.Int32Collection:
			result = KnownElements.Int32CollectionConverter;
			break;
		case KnownElements.LinearGradientBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.LineGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.Transform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.MatrixTransform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.PathFigureCollection:
			result = KnownElements.PathFigureCollectionConverter;
			break;
		case KnownElements.PathGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.PointCollection:
			result = KnownElements.PointCollectionConverter;
			break;
		case KnownElements.RadialGradientBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.RectangleGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.RotateTransform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.ScaleTransform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.SkewTransform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.SolidColorBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.StreamGeometry:
			result = KnownElements.GeometryConverter;
			break;
		case KnownElements.TransformGroup:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.TranslateTransform:
			result = KnownElements.TransformConverter;
			break;
		case KnownElements.VectorCollection:
			result = KnownElements.VectorCollectionConverter;
			break;
		case KnownElements.VisualBrush:
			result = KnownElements.BrushConverter;
			break;
		case KnownElements.BitmapSource:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.BitmapFrame:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.BitmapImage:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.CachedBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.ColorConvertedBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.CroppedBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.FormatConvertedBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.RenderTargetBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.TransformedBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.WriteableBitmap:
			result = KnownElements.ImageSourceConverter;
			break;
		case KnownElements.PixelFormat:
			result = KnownElements.PixelFormatConverter;
			break;
		case KnownElements.Matrix3D:
			result = KnownElements.Matrix3DConverter;
			break;
		case KnownElements.Point3D:
			result = KnownElements.Point3DConverter;
			break;
		case KnownElements.Point3DCollection:
			result = KnownElements.Point3DCollectionConverter;
			break;
		case KnownElements.Vector3DCollection:
			result = KnownElements.Vector3DCollectionConverter;
			break;
		case KnownElements.Point4D:
			result = KnownElements.Point4DConverter;
			break;
		case KnownElements.Quaternion:
			result = KnownElements.QuaternionConverter;
			break;
		case KnownElements.Rect3D:
			result = KnownElements.Rect3DConverter;
			break;
		case KnownElements.Size3D:
			result = KnownElements.Size3DConverter;
			break;
		case KnownElements.Vector3D:
			result = KnownElements.Vector3DConverter;
			break;
		case KnownElements.XmlLanguage:
			result = KnownElements.XmlLanguageConverter;
			break;
		case KnownElements.Point:
			result = KnownElements.PointConverter;
			break;
		case KnownElements.Size:
			result = KnownElements.SizeConverter;
			break;
		case KnownElements.Vector:
			result = KnownElements.VectorConverter;
			break;
		case KnownElements.Rect:
			result = KnownElements.RectConverter;
			break;
		case KnownElements.Matrix:
			result = KnownElements.MatrixConverter;
			break;
		case KnownElements.DependencyProperty:
			result = KnownElements.DependencyPropertyConverter;
			break;
		case KnownElements.Expression:
			result = KnownElements.ExpressionConverter;
			break;
		case KnownElements.Int32Rect:
			result = KnownElements.Int32RectConverter;
			break;
		case KnownElements.Boolean:
			result = KnownElements.BooleanConverter;
			break;
		case KnownElements.Int16:
			result = KnownElements.Int16Converter;
			break;
		case KnownElements.Int32:
			result = KnownElements.Int32Converter;
			break;
		case KnownElements.Int64:
			result = KnownElements.Int64Converter;
			break;
		case KnownElements.UInt16:
			result = KnownElements.UInt16Converter;
			break;
		case KnownElements.UInt32:
			result = KnownElements.UInt32Converter;
			break;
		case KnownElements.UInt64:
			result = KnownElements.UInt64Converter;
			break;
		case KnownElements.Single:
			result = KnownElements.SingleConverter;
			break;
		case KnownElements.Double:
			result = KnownElements.DoubleConverter;
			break;
		case KnownElements.Object:
			result = KnownElements.StringConverter;
			break;
		case KnownElements.String:
			result = KnownElements.StringConverter;
			break;
		case KnownElements.Byte:
			result = KnownElements.ByteConverter;
			break;
		case KnownElements.SByte:
			result = KnownElements.SByteConverter;
			break;
		case KnownElements.Char:
			result = KnownElements.CharConverter;
			break;
		case KnownElements.Decimal:
			result = KnownElements.DecimalConverter;
			break;
		case KnownElements.TimeSpan:
			result = KnownElements.TimeSpanConverter;
			break;
		case KnownElements.Guid:
			result = KnownElements.GuidConverter;
			break;
		case KnownElements.DateTime:
			result = KnownElements.DateTimeConverter2;
			break;
		case KnownElements.Uri:
			result = KnownElements.UriTypeConverter;
			break;
		case KnownElements.CultureInfo:
			result = KnownElements.CultureInfoConverter;
			break;
		}
		return result;
	}

	internal static KnownElements GetKnownTypeConverterIdForProperty(KnownElements id, string propName)
	{
		KnownElements result = KnownElements.UnknownElement;
		switch (id)
		{
		case KnownElements.ColumnDefinition:
			if (string.Equals(propName, "MinWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MaxWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.RowDefinition:
			if (string.Equals(propName, "MinHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MaxHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.FrameworkElement:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Adorner:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Shape:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Panel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Canvas:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Left", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Top", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Right", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Bottom", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Control:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ContentControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Window:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Top", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Left", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "DialogResult", StringComparison.Ordinal))
			{
				result = KnownElements.DialogResultConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.NavigationWindow:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Top", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Left", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "DialogResult", StringComparison.Ordinal))
			{
				result = KnownElements.DialogResultConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.CollectionView:
			if (string.Equals(propName, "Culture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.StickyNoteControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ItemsControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.MenuBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ContextMenu:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.HeaderedItemsControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.MenuItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.FlowDocumentScrollViewer:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.DocumentViewerBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.FlowDocumentPageViewer:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.AccessText:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.AdornedElementPlaceholder:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Decorator:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Border:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ButtonBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Button:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ToggleButton:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsChecked", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.CheckBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsChecked", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Selector:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsSynchronizedWithCurrentItem", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ComboBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MaxDropDownHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsSynchronizedWithCurrentItem", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ListBoxItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ComboBoxItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ContentPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ContextMenuService:
			if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.DockPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.DocumentViewer:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.HeaderedContentControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Expander:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.FlowDocumentReader:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Frame:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Grid:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.GridViewColumn:
			if (string.Equals(propName, "Width", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.GridViewColumnHeader:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.GridViewRowPresenterBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.GridViewHeaderRowPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.GridViewRowPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Thumb:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.GridSplitter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.GroupBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.GroupItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Image:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.InkCanvas:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Top", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Bottom", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Left", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Right", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.InkPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ItemCollection:
			if (string.Equals(propName, "Culture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.ItemsPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Label:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ListBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsSynchronizedWithCurrentItem", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ListView:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsSynchronizedWithCurrentItem", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ListViewItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.MediaElement:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Menu:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Page:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.PasswordBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.BulletDecorator:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.DocumentPageView:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Popup:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.RangeBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.RepeatButton:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ResizeGrip:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ScrollBar:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ScrollContentPresenter:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.StatusBar:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.StatusBarItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TabPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.TextBoxBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TickBar:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ToolBarOverflowPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.StackPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ToolBarPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Track:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.UniformGrid:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ProgressBar:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.RadioButton:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsChecked", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.RichTextBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ScrollViewer:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Separator:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Slider:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TabControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "IsSynchronizedWithCurrentItem", StringComparison.Ordinal))
			{
				result = KnownElements.NullableBoolConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TabItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TextBlock:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.TextBox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ToolBar:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ToolBarTray:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.ToolTip:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ToolTipService:
			if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.TreeView:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TreeViewItem:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.UserControl:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Viewbox:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Viewport3D:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.VirtualizingPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.VirtualizingStackPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.WrapPanel:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "ItemWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "ItemHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Binding:
			if (string.Equals(propName, "ConverterCulture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.BindingListCollectionView:
			if (string.Equals(propName, "Culture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.CollectionViewSource:
			if (string.Equals(propName, "Culture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.ListCollectionView:
			if (string.Equals(propName, "Culture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.MultiBinding:
			if (string.Equals(propName, "ConverterCulture", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.AdornerDecorator:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.AdornerLayer:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.TextElement:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Inline:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.AnchoredBlock:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Block:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.BlockUIContainer:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Span:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Bold:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.DocumentReference:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Figure:
			if (string.Equals(propName, "HorizontalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "VerticalOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.FixedPage:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Left", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Top", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Right", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Bottom", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Floater:
			if (string.Equals(propName, "Width", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.FlowDocument:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "ColumnWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "ColumnGap", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "ColumnRuleWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "PageWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MinPageWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MaxPageWidth", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "PageHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MinPageHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "MaxPageHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Glyphs:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontRenderingEmSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			else if (string.Equals(propName, "OriginX", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "OriginY", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Hyperlink:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.InlineUIContainer:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Italic:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.LineBreak:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.List:
			if (string.Equals(propName, "MarkerOffset", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.ListItem:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.PageContent:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Paragraph:
			if (string.Equals(propName, "TextIndent", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Run:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Section:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Table:
			if (string.Equals(propName, "CellSpacing", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TableCell:
			if (string.Equals(propName, "LineHeight", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TableRow:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.TableRowGroup:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Underline:
			if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.PageFunctionBase:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "FontSize", StringComparison.Ordinal))
			{
				result = KnownElements.FontSizeConverter;
			}
			break;
		case KnownElements.Ellipse:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Line:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "X1", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Y1", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "X2", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "Y2", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Path:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Polygon:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Polyline:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.Rectangle:
			if (IsStandardLengthProp(propName))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "RadiusX", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "RadiusY", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			else if (string.Equals(propName, "StrokeThickness", StringComparison.Ordinal))
			{
				result = KnownElements.LengthConverter;
			}
			break;
		case KnownElements.InputBinding:
			if (string.Equals(propName, "Command", StringComparison.Ordinal))
			{
				result = KnownElements.CommandConverter;
			}
			break;
		case KnownElements.KeyBinding:
			if (string.Equals(propName, "Gesture", StringComparison.Ordinal))
			{
				result = KnownElements.KeyGestureConverter;
			}
			else if (string.Equals(propName, "Command", StringComparison.Ordinal))
			{
				result = KnownElements.CommandConverter;
			}
			break;
		case KnownElements.MouseBinding:
			if (string.Equals(propName, "Gesture", StringComparison.Ordinal))
			{
				result = KnownElements.MouseGestureConverter;
			}
			else if (string.Equals(propName, "Command", StringComparison.Ordinal))
			{
				result = KnownElements.CommandConverter;
			}
			break;
		case KnownElements.InputLanguageManager:
			if (string.Equals(propName, "CurrentInputLanguage", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			else if (string.Equals(propName, "InputLanguage", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		case KnownElements.GlyphRun:
			if (string.Equals(propName, "CaretStops", StringComparison.Ordinal))
			{
				result = KnownElements.BoolIListConverter;
			}
			else if (string.Equals(propName, "ClusterMap", StringComparison.Ordinal))
			{
				result = KnownElements.UShortIListConverter;
			}
			else if (string.Equals(propName, "Characters", StringComparison.Ordinal))
			{
				result = KnownElements.CharIListConverter;
			}
			else if (string.Equals(propName, "GlyphIndices", StringComparison.Ordinal))
			{
				result = KnownElements.UShortIListConverter;
			}
			else if (string.Equals(propName, "AdvanceWidths", StringComparison.Ordinal))
			{
				result = KnownElements.DoubleIListConverter;
			}
			else if (string.Equals(propName, "GlyphOffsets", StringComparison.Ordinal))
			{
				result = KnownElements.PointIListConverter;
			}
			break;
		case KnownElements.NumberSubstitution:
			if (string.Equals(propName, "CultureOverride", StringComparison.Ordinal))
			{
				result = KnownElements.CultureInfoIetfLanguageTagConverter;
			}
			break;
		}
		return result;
	}
}
