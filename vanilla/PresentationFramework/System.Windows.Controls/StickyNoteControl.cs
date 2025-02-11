using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Annotations;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using MS.Internal;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Component;
using MS.Internal.Commands;
using MS.Internal.Controls.StickyNote;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Represents a control that lets users attach typed text or handwritten annotations to documents.</summary>
[TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_TitleThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_ResizeBottomRightThumb", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_ContentControl", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_IconButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CopyMenuItem", Type = typeof(MenuItem))]
[TemplatePart(Name = "PART_PasteMenuItem", Type = typeof(MenuItem))]
[TemplatePart(Name = "PART_InkMenuItem", Type = typeof(MenuItem))]
[TemplatePart(Name = "PART_SelectMenuItem", Type = typeof(MenuItem))]
[TemplatePart(Name = "PART_EraseMenuItem", Type = typeof(MenuItem))]
public sealed class StickyNoteControl : Control, IAnnotationComponent
{
	private class InkEditingModeConverter : IValueConverter
	{
		public object Convert(object o, Type type, object parameter, CultureInfo culture)
		{
			InkCanvasEditingMode inkCanvasEditingMode = (InkCanvasEditingMode)parameter;
			if ((InkCanvasEditingMode)o == inkCanvasEditingMode)
			{
				return true;
			}
			return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	private class InkEditingModeIsKeyboardFocusWithin2EditingMode : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			InkCanvasEditingMode inkCanvasEditingMode = (InkCanvasEditingMode)values[0];
			if ((bool)values[1])
			{
				return inkCanvasEditingMode;
			}
			return InkCanvasEditingMode.None;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new object[2]
			{
				value,
				Binding.DoNothing
			};
		}
	}

	private class StrokeChangedHandler<TEventArgs>
	{
		private StickyNoteControl _snc;

		public StrokeChangedHandler(StickyNoteControl snc)
		{
			Invariant.Assert(snc != null);
			_snc = snc;
		}

		public void OnStrokeChanged(object sender, TEventArgs t)
		{
			_snc.UpdateAnnotationWithSNC(XmlToken.Ink);
			_snc._dirty = true;
		}
	}

	/// <summary>Gets the Xml type of the text sticky note annotation. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> of the type that a text <see cref="T:System.Windows.Controls.StickyNoteControl" /> instantiates. </returns>
	public static readonly XmlQualifiedName TextSchemaName;

	/// <summary>Gets the Xml type of the ink sticky note annotation. </summary>
	/// <returns>An <see cref="T:System.Xml.XmlQualifiedName" /> of the XML type that an ink <see cref="T:System.Windows.Controls.StickyNoteControl" /> instantiates. </returns>
	public static readonly XmlQualifiedName InkSchemaName;

	private PresentationContext _presentationContext;

	private TranslateTransform _positionTransform = new TranslateTransform(0.0, 0.0);

	private IAttachedAnnotation _attachedAnnotation;

	private SNCAnnotation _sncAnnotation;

	private double _offsetX;

	private double _offsetY;

	private double _deltaX;

	private double _deltaY;

	private int _zOrder;

	private bool _selfMirroring;

	internal static readonly DependencyPropertyKey AuthorPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.Author" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.Author" /> dependency property. </returns>
	public static readonly DependencyProperty AuthorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.IsExpanded" /> dependency property. </summary>
	public static readonly DependencyProperty IsExpandedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.IsActive" /> dependency property. </summary>
	public static readonly DependencyProperty IsActiveProperty;

	internal static readonly DependencyPropertyKey IsMouseOverAnchorPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.IsMouseOverAnchor" /> dependency property. </summary>
	public static readonly DependencyProperty IsMouseOverAnchorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontFamily" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontFamily" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionFontFamilyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontSize" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontSize" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionFontSizeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontStretch" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontStretch" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionFontStretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionFontStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontWeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.CaptionFontWeight" /> dependency property.</returns>
	public static readonly DependencyProperty CaptionFontWeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.PenWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StickyNoteControl.PenWidth" /> dependency property.</returns>
	public static readonly DependencyProperty PenWidthProperty;

	private static readonly DependencyPropertyKey StickyNoteTypePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StickyNoteControl.StickyNoteType" /> dependency property. </summary>
	public static readonly DependencyProperty StickyNoteTypeProperty;

	/// <summary>Represents a command whose <see cref="M:System.Windows.Input.RoutedCommand.Execute(System.Object,System.Windows.IInputElement)" /> method deletes a <see cref="T:System.Windows.Controls.StickyNoteControl" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Input.RoutedCommand" /> that can be used to remove a <see cref="T:System.Windows.Controls.StickyNoteControl" />.</returns>
	public static readonly RoutedCommand DeleteNoteCommand;

	/// <summary>Represents a command whose <see cref="M:System.Windows.Input.RoutedCommand.Execute(System.Object,System.Windows.IInputElement)" /> method will switch the cursor in an ink sticky note to one of several possible modes, including draw and erase. </summary>
	public static readonly RoutedCommand InkCommand;

	private static readonly DependencyProperty InkEditingModeProperty;

	private LockHelper _lockHelper;

	private MarkedHighlightComponent _anchor;

	private bool _dirty;

	private StickyNoteType _stickyNoteType;

	private StickyNoteContentControl _contentControl;

	private StrokeChangedHandler<PropertyDataChangedEventArgs> _propertyDataChangedHandler;

	private StrokeChangedHandler<DrawingAttributesReplacedEventArgs> _strokeDrawingAttributesReplacedHandler;

	private StrokeChangedHandler<EventArgs> _strokePacketDataChangedHandler;

	IList IAnnotationComponent.AttachedAnnotations
	{
		get
		{
			ArrayList arrayList = new ArrayList(1);
			if (_attachedAnnotation != null)
			{
				arrayList.Add(_attachedAnnotation);
			}
			return arrayList;
		}
	}

	UIElement IAnnotationComponent.AnnotatedElement
	{
		get
		{
			if (_attachedAnnotation == null)
			{
				return null;
			}
			return _attachedAnnotation.Parent as UIElement;
		}
	}

	PresentationContext IAnnotationComponent.PresentationContext
	{
		get
		{
			return _presentationContext;
		}
		set
		{
			_presentationContext = value;
		}
	}

	int IAnnotationComponent.ZOrder
	{
		get
		{
			return _zOrder;
		}
		set
		{
			_zOrder = value;
			UpdateAnnotationWithSNC(XmlToken.ZOrder);
		}
	}

	bool IAnnotationComponent.IsDirty
	{
		get
		{
			if (_anchor != null)
			{
				return _anchor.IsDirty;
			}
			return false;
		}
		set
		{
			if (_anchor != null)
			{
				_anchor.IsDirty = value;
			}
			if (value)
			{
				InvalidateVisual();
			}
		}
	}

	internal TranslateTransform PositionTransform
	{
		get
		{
			return _positionTransform;
		}
		set
		{
			Invariant.Assert(value != null, "PositionTransform cannot be null.");
			_positionTransform = value;
			InvalidateTransform();
		}
	}

	internal double XOffset
	{
		get
		{
			return _offsetX;
		}
		set
		{
			_offsetX = value;
		}
	}

	internal double YOffset
	{
		get
		{
			return _offsetY;
		}
		set
		{
			_offsetY = value;
		}
	}

	internal bool FlipBothOrigins
	{
		get
		{
			if (IsExpanded && base.FlowDirection == FlowDirection.RightToLeft && _attachedAnnotation != null)
			{
				return _attachedAnnotation.Parent is DocumentPageHost;
			}
			return false;
		}
	}

	private Rect StickyNoteBounds
	{
		get
		{
			Rect result = Rect.Empty;
			Point anchorPoint = _attachedAnnotation.AnchorPoint;
			if (!double.IsNaN(anchorPoint.X) && !double.IsNaN(anchorPoint.Y) && PositionTransform != null)
			{
				result = new Rect(anchorPoint.X + PositionTransform.X + _deltaX, anchorPoint.Y + PositionTransform.Y + _deltaY, base.Width, base.Height);
			}
			return result;
		}
	}

	private Rect PageBounds
	{
		get
		{
			Rect result = Rect.Empty;
			if (((IAnnotationComponent)this).AnnotatedElement is IScrollInfo scrollInfo)
			{
				result = new Rect(0.0 - scrollInfo.HorizontalOffset, 0.0 - scrollInfo.VerticalOffset, scrollInfo.ExtentWidth, scrollInfo.ExtentHeight);
			}
			else
			{
				UIElement annotatedElement = ((IAnnotationComponent)this).AnnotatedElement;
				if (annotatedElement != null)
				{
					Size renderSize = annotatedElement.RenderSize;
					result = new Rect(0.0, 0.0, renderSize.Width, renderSize.Height);
				}
			}
			return result;
		}
	}

	/// <summary>Gets the name of the author who created the sticky note.  </summary>
	/// <returns>The name of the author who created the sticky note.</returns>
	public string Author => (string)GetValue(AuthorProperty);

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Windows.Controls.StickyNoteControl" /> is expanded.  </summary>
	/// <returns>true if the control is expanded; otherwise, false. The default is true. </returns>
	public bool IsExpanded
	{
		get
		{
			return (bool)GetValue(IsExpandedProperty);
		}
		set
		{
			SetValue(IsExpandedProperty, value);
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Windows.Controls.StickyNoteControl" /> is active.  </summary>
	/// <returns>true if the control is active; otherwise, false. The default is false.</returns>
	public bool IsActive => (bool)GetValue(IsActiveProperty);

	/// <summary>Gets a value indicating whether the mouse cursor is over the anchor of the <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>true if the mouse cursor is over the anchor; otherwise, false. The default is false.</returns>
	public bool IsMouseOverAnchor => (bool)GetValue(IsMouseOverAnchorProperty);

	/// <summary>Gets or sets the font family of the caption for the <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>A <see cref="T:System.Drawing.FontFamily" /> for the control's caption. The default is the value of <see cref="P:System.Windows.SystemFonts.MessageFontFamily" />. </returns>
	public FontFamily CaptionFontFamily
	{
		get
		{
			return (FontFamily)GetValue(CaptionFontFamilyProperty);
		}
		set
		{
			SetValue(CaptionFontFamilyProperty, value);
		}
	}

	/// <summary>Gets or sets the size of the font used for the caption of the <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>A <see cref="T:System.Double" /> representing the font size. The default is the value of <see cref="P:System.Windows.SystemFonts.MessageFontSize" />.</returns>
	public double CaptionFontSize
	{
		get
		{
			return (double)GetValue(CaptionFontSizeProperty);
		}
		set
		{
			SetValue(CaptionFontSizeProperty, value);
		}
	}

	/// <summary>Gets or sets the degree to which the font used for the caption of the <see cref="T:System.Windows.Controls.StickyNoteControl" /> is stretched.  </summary>
	/// <returns>A <see cref="T:System.Windows.FontStretch" /> structure representing the degree of stretching compared to a font's normal aspect ratio. The default is <see cref="P:System.Windows.FontStretches.Normal" />. </returns>
	public FontStretch CaptionFontStretch
	{
		get
		{
			return (FontStretch)GetValue(CaptionFontStretchProperty);
		}
		set
		{
			SetValue(CaptionFontStretchProperty, value);
		}
	}

	/// <summary>Gets or sets the style of the font used for the caption of the <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.FontStyle" /> structure representing the style of the caption as normal, italic, or oblique. The default is the value of <see cref="P:System.Windows.SystemFonts.MessageFontStyle" />.</returns>
	public FontStyle CaptionFontStyle
	{
		get
		{
			return (FontStyle)GetValue(CaptionFontStyleProperty);
		}
		set
		{
			SetValue(CaptionFontStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the weight of the font used for the caption of the <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>A <see cref="T:System.Windows.FontWeight" /> structure representing the weight of the font, for example, bold, ultrabold, or extralight. The default is the value of <see cref="P:System.Windows.SystemFonts.MessageFontWeight" />. </returns>
	public FontWeight CaptionFontWeight
	{
		get
		{
			return (FontWeight)GetValue(CaptionFontWeightProperty);
		}
		set
		{
			SetValue(CaptionFontWeightProperty, value);
		}
	}

	/// <summary>Gets or sets the width of the pen for an ink <see cref="T:System.Windows.Controls.StickyNoteControl" />.  </summary>
	/// <returns>A <see cref="T:System.Double" /> representing the width of the pen. The default is the value of <see cref="P:System.Windows.Ink.DrawingAttributes.Width" />.</returns>
	public double PenWidth
	{
		get
		{
			return (double)GetValue(PenWidthProperty);
		}
		set
		{
			SetValue(PenWidthProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the sticky note is text or ink.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.StickyNoteType" /> value indicating the type of note. The default is <see cref="F:System.Windows.Controls.StickyNoteType.Text" />. </returns>
	public StickyNoteType StickyNoteType => (StickyNoteType)GetValue(StickyNoteTypeProperty);

	/// <summary>Gets an object that provides information about the annotated object.</summary>
	/// <returns>An <see cref="T:System.Windows.Annotations.IAnchorInfo" /> object that provides information about the annotated object.</returns>
	public IAnchorInfo AnchorInfo
	{
		get
		{
			if (_attachedAnnotation != null)
			{
				return _attachedAnnotation;
			}
			return null;
		}
	}

	internal StickyNoteContentControl Content => _contentControl;

	private bool IsDirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
		}
	}

	private LockHelper InternalLocker
	{
		get
		{
			if (_lockHelper == null)
			{
				_lockHelper = new LockHelper();
			}
			return _lockHelper;
		}
	}

	void IAnnotationComponent.AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		if (_attachedAnnotation == null)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedSNBegin);
			SetAnnotation(attachedAnnotation);
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedSNEnd);
			return;
		}
		throw new InvalidOperationException(SR.AddAnnotationsNotImplemented);
	}

	void IAnnotationComponent.RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		if (attachedAnnotation == _attachedAnnotation)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedSNBegin);
			GiveUpFocus();
			ClearAnnotation();
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedSNEnd);
			return;
		}
		throw new ArgumentException(SR.InvalidValueSpecified, "attachedAnnotation");
	}

	void IAnnotationComponent.ModifyAttachedAnnotation(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		throw new NotSupportedException(SR.NotSupported);
	}

	GeneralTransform IAnnotationComponent.GetDesiredTransform(GeneralTransform transform)
	{
		if (_attachedAnnotation != null)
		{
			if (IsExpanded && base.FlowDirection == FlowDirection.RightToLeft && _attachedAnnotation.Parent is DocumentPageHost)
			{
				_selfMirroring = true;
			}
			else
			{
				_selfMirroring = false;
			}
			Point anchorPoint = _attachedAnnotation.AnchorPoint;
			if (double.IsInfinity(anchorPoint.X) || double.IsInfinity(anchorPoint.Y))
			{
				throw new InvalidOperationException(SR.InvalidAnchorPosition);
			}
			if (double.IsNaN(anchorPoint.X) || double.IsNaN(anchorPoint.Y))
			{
				return null;
			}
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			if (_selfMirroring)
			{
				generalTransformGroup.Children.Add(new MatrixTransform(-1.0, 0.0, 0.0, 1.0, base.Width, 0.0));
			}
			generalTransformGroup.Children.Add(new TranslateTransform(anchorPoint.X, anchorPoint.Y));
			TranslateTransform translateTransform = new TranslateTransform(0.0, 0.0);
			if (IsExpanded)
			{
				translateTransform = PositionTransform.Clone();
				_deltaX = (_deltaY = 0.0);
				Rect pageBounds = PageBounds;
				Rect stickyNoteBounds = StickyNoteBounds;
				GetOffsets(pageBounds, stickyNoteBounds, out var offsetX, out var offsetY);
				if (DoubleUtil.GreaterThan(Math.Abs(offsetX), Math.Abs(_offsetX)))
				{
					double num = _offsetX - offsetX;
					if (DoubleUtil.LessThan(num, 0.0))
					{
						num = Math.Max(num, 0.0 - (stickyNoteBounds.Left - pageBounds.Left));
					}
					translateTransform.X += num;
					_deltaX = num;
				}
				if (DoubleUtil.GreaterThan(Math.Abs(offsetY), Math.Abs(_offsetY)))
				{
					double num2 = _offsetY - offsetY;
					if (DoubleUtil.LessThan(num2, 0.0))
					{
						num2 = Math.Max(num2, 0.0 - (stickyNoteBounds.Top - pageBounds.Top));
					}
					translateTransform.Y += num2;
					_deltaY = num2;
				}
			}
			if (translateTransform != null)
			{
				generalTransformGroup.Children.Add(translateTransform);
			}
			if (transform != null)
			{
				generalTransformGroup.Children.Add(transform);
			}
			return generalTransformGroup;
		}
		return null;
	}

	private void OnAuthorUpdated(object obj, AnnotationAuthorChangedEventArgs args)
	{
		if (!InternalLocker.IsLocked(LockHelper.LockFlag.AnnotationChanged))
		{
			UpdateSNCWithAnnotation(XmlToken.Author);
			IsDirty = true;
		}
	}

	private void OnAnnotationUpdated(object obj, AnnotationResourceChangedEventArgs args)
	{
		if (!InternalLocker.IsLocked(LockHelper.LockFlag.AnnotationChanged))
		{
			SNCAnnotation sncAnnotation = new SNCAnnotation(args.Annotation);
			_sncAnnotation = sncAnnotation;
			UpdateSNCWithAnnotation(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder);
			IsDirty = true;
		}
	}

	private void SetAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		SNCAnnotation sNCAnnotation = new SNCAnnotation(attachedAnnotation.Annotation);
		bool hasInkData = sNCAnnotation.HasInkData;
		bool hasTextData = sNCAnnotation.HasTextData;
		if (hasInkData && hasTextData)
		{
			throw new ArgumentException(SR.InvalidStickyNoteAnnotation, "attachedAnnotation");
		}
		if (hasInkData)
		{
			_stickyNoteType = StickyNoteType.Ink;
		}
		else if (hasTextData)
		{
			_stickyNoteType = StickyNoteType.Text;
		}
		if (Content != null)
		{
			EnsureStickyNoteType();
		}
		if (sNCAnnotation.IsNewAnnotation)
		{
			AnnotationResource item = new AnnotationResource("Meta Data");
			attachedAnnotation.Annotation.Cargos.Add(item);
		}
		_attachedAnnotation = attachedAnnotation;
		_attachedAnnotation.Annotation.CargoChanged += OnAnnotationUpdated;
		_attachedAnnotation.Annotation.AuthorChanged += OnAuthorUpdated;
		_sncAnnotation = sNCAnnotation;
		_anchor.AddAttachedAnnotation(attachedAnnotation);
		UpdateSNCWithAnnotation(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder);
		IsDirty = false;
		if ((_attachedAnnotation.AttachmentLevel & AttachmentLevel.StartPortion) == 0)
		{
			SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
		}
		else
		{
			base.RequestBringIntoView += OnRequestBringIntoView;
		}
	}

	private void ClearAnnotation()
	{
		_attachedAnnotation.Annotation.CargoChanged -= OnAnnotationUpdated;
		_attachedAnnotation.Annotation.AuthorChanged -= OnAuthorUpdated;
		_anchor.RemoveAttachedAnnotation(_attachedAnnotation);
		_sncAnnotation = null;
		_attachedAnnotation = null;
		base.RequestBringIntoView -= OnRequestBringIntoView;
	}

	private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
	{
		FrameworkElement frameworkElement = ((IAnnotationComponent)this).AnnotatedElement as FrameworkElement;
		if (frameworkElement is DocumentPageHost documentPageHost)
		{
			frameworkElement = documentPageHost.PageVisual as FrameworkElement;
		}
		if (frameworkElement == null)
		{
			return;
		}
		if (frameworkElement is IScrollInfo scrollInfo)
		{
			Rect stickyNoteBounds = StickyNoteBounds;
			Rect rect = new Rect(0.0, 0.0, scrollInfo.ViewportWidth, scrollInfo.ViewportHeight);
			if (stickyNoteBounds.IntersectsWith(rect))
			{
				return;
			}
		}
		Transform transform = (Transform)TransformToVisual(frameworkElement);
		Rect rect2 = new Rect(0.0, 0.0, base.Width, base.Height);
		rect2.Transform(transform.Value);
		base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(DispatchBringIntoView), new object[2] { frameworkElement, rect2 });
	}

	private object DispatchBringIntoView(object arg)
	{
		object[] obj = (object[])arg;
		FrameworkElement frameworkElement = (FrameworkElement)obj[0];
		Rect targetRectangle = (Rect)obj[1];
		frameworkElement.BringIntoView(targetRectangle);
		return null;
	}

	private void UpdateSNCWithAnnotation(XmlToken tokens)
	{
		if (_sncAnnotation != null)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.UpdateSNCWithAnnotationBegin);
			using (new LockHelper.AutoLocker(InternalLocker, LockHelper.LockFlag.DataChanged))
			{
				SNCAnnotation.UpdateStickyNoteControl(tokens, this, _sncAnnotation);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.UpdateSNCWithAnnotationEnd);
		}
	}

	private void UpdateAnnotationWithSNC(XmlToken tokens)
	{
		if (_sncAnnotation != null && !InternalLocker.IsLocked(LockHelper.LockFlag.DataChanged))
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.UpdateAnnotationWithSNCBegin);
			using (new LockHelper.AutoLocker(InternalLocker, LockHelper.LockFlag.AnnotationChanged))
			{
				SNCAnnotation.UpdateAnnotation(tokens, this, _sncAnnotation);
			}
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.UpdateAnnotationWithSNCEnd);
		}
	}

	private void UpdateOffsets()
	{
		if (_attachedAnnotation == null)
		{
			return;
		}
		Rect pageBounds = PageBounds;
		Rect stickyNoteBounds = StickyNoteBounds;
		if (!pageBounds.IsEmpty && !stickyNoteBounds.IsEmpty)
		{
			Invariant.Assert(DoubleUtil.GreaterThan(stickyNoteBounds.Right, pageBounds.Left), "Note's right is off left of page.");
			Invariant.Assert(DoubleUtil.LessThan(stickyNoteBounds.Left, pageBounds.Right), "Note's left is off right of page.");
			Invariant.Assert(DoubleUtil.GreaterThan(stickyNoteBounds.Bottom, pageBounds.Top), "Note's bottom is off top of page.");
			Invariant.Assert(DoubleUtil.LessThan(stickyNoteBounds.Top, pageBounds.Bottom), "Note's top is off bottom of page.");
			GetOffsets(pageBounds, stickyNoteBounds, out var offsetX, out var offsetY);
			if (!DoubleUtil.AreClose(XOffset, offsetX))
			{
				XOffset = offsetX;
			}
			if (!DoubleUtil.AreClose(YOffset, offsetY))
			{
				YOffset = offsetY;
			}
		}
	}

	private static void GetOffsets(Rect rectPage, Rect rectStickyNote, out double offsetX, out double offsetY)
	{
		offsetX = 0.0;
		if (DoubleUtil.LessThan(rectStickyNote.Left, rectPage.Left))
		{
			offsetX = rectStickyNote.Left - rectPage.Left;
		}
		else if (DoubleUtil.GreaterThan(rectStickyNote.Right, rectPage.Right))
		{
			offsetX = rectStickyNote.Right - rectPage.Right;
		}
		offsetY = 0.0;
		if (DoubleUtil.LessThan(rectStickyNote.Top, rectPage.Top))
		{
			offsetY = rectStickyNote.Top - rectPage.Top;
		}
		else if (DoubleUtil.GreaterThan(rectStickyNote.Bottom, rectPage.Bottom))
		{
			offsetY = rectStickyNote.Bottom - rectPage.Bottom;
		}
	}

	static StickyNoteControl()
	{
		TextSchemaName = new XmlQualifiedName("TextStickyNote", "http://schemas.microsoft.com/windows/annotations/2003/11/base");
		InkSchemaName = new XmlQualifiedName("InkStickyNote", "http://schemas.microsoft.com/windows/annotations/2003/11/base");
		AuthorPropertyKey = DependencyProperty.RegisterReadOnly("Author", typeof(string), typeof(StickyNoteControl), new FrameworkPropertyMetadata(string.Empty));
		AuthorProperty = AuthorPropertyKey.DependencyProperty;
		IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(StickyNoteControl), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, _OnIsExpandedChanged));
		IsActiveProperty = DependencyProperty.RegisterAttached("IsActive", typeof(bool), typeof(StickyNoteControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		IsMouseOverAnchorPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseOverAnchor", typeof(bool), typeof(StickyNoteControl), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsMouseOverAnchorProperty = IsMouseOverAnchorPropertyKey.DependencyProperty;
		CaptionFontFamilyProperty = DependencyProperty.Register("CaptionFontFamily", typeof(FontFamily), typeof(StickyNoteControl), new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.AffectsMeasure));
		CaptionFontSizeProperty = DependencyProperty.Register("CaptionFontSize", typeof(double), typeof(StickyNoteControl), new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.AffectsMeasure));
		CaptionFontStretchProperty = DependencyProperty.Register("CaptionFontStretch", typeof(FontStretch), typeof(StickyNoteControl), new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure));
		CaptionFontStyleProperty = DependencyProperty.Register("CaptionFontStyle", typeof(FontStyle), typeof(StickyNoteControl), new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.AffectsMeasure));
		CaptionFontWeightProperty = DependencyProperty.Register("CaptionFontWeight", typeof(FontWeight), typeof(StickyNoteControl), new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.AffectsMeasure));
		PenWidthProperty = DependencyProperty.Register("PenWidth", typeof(double), typeof(StickyNoteControl), new FrameworkPropertyMetadata(new DrawingAttributes().Width, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, _UpdateInkDrawingAttributes));
		StickyNoteTypePropertyKey = DependencyProperty.RegisterReadOnly("StickyNoteType", typeof(StickyNoteType), typeof(StickyNoteControl), new FrameworkPropertyMetadata(StickyNoteType.Text));
		StickyNoteTypeProperty = StickyNoteTypePropertyKey.DependencyProperty;
		DeleteNoteCommand = new RoutedCommand("DeleteNote", typeof(StickyNoteControl));
		InkCommand = new RoutedCommand("Ink", typeof(StickyNoteControl));
		InkEditingModeProperty = DependencyProperty.Register("InkEditingMode", typeof(InkCanvasEditingMode), typeof(StickyNoteControl), new FrameworkPropertyMetadata(InkCanvasEditingMode.None));
		Type typeFromHandle = typeof(StickyNoteControl);
		EventManager.RegisterClassHandler(typeFromHandle, Stylus.PreviewStylusDownEvent, new StylusDownEventHandler(_OnPreviewDeviceDown));
		EventManager.RegisterClassHandler(typeFromHandle, Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(_OnPreviewDeviceDown));
		EventManager.RegisterClassHandler(typeFromHandle, Mouse.MouseDownEvent, new MouseButtonEventHandler(_OnDeviceDown));
		EventManager.RegisterClassHandler(typeFromHandle, ContextMenuService.ContextMenuOpeningEvent, new ContextMenuEventHandler(_OnContextMenuOpening));
		CommandHelpers.RegisterCommandHandler(typeof(StickyNoteControl), DeleteNoteCommand, _OnCommandExecuted, _OnQueryCommandEnabled);
		CommandHelpers.RegisterCommandHandler(typeof(StickyNoteControl), InkCommand, _OnCommandExecuted, _OnQueryCommandEnabled);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(new ComponentResourceKey(typeof(PresentationUIStyleResources), "StickyNoteControlStyleKey")));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		Control.IsTabStopProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(false));
		Control.ForegroundProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(_UpdateInkDrawingAttributes));
		Control.FontFamilyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnFontPropertyChanged));
		Control.FontSizeProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnFontPropertyChanged));
		Control.FontStretchProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnFontPropertyChanged));
		Control.FontStyleProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnFontPropertyChanged));
		Control.FontWeightProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnFontPropertyChanged));
	}

	private StickyNoteControl()
		: this(StickyNoteType.Text)
	{
	}

	internal StickyNoteControl(StickyNoteType type)
	{
		_stickyNoteType = type;
		SetValue(StickyNoteTypePropertyKey, type);
		InitStickyNoteControl();
	}

	/// <summary>Registers event handlers for all the children of a template.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (IsExpanded)
		{
			EnsureStickyNoteType();
		}
		UpdateSNCWithAnnotation(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder);
		if (!IsExpanded)
		{
			GetIconButton()?.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));
			return;
		}
		GetCloseButton()?.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));
		Thumb titleThumb = GetTitleThumb();
		if (titleThumb != null)
		{
			titleThumb.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			titleThumb.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		}
		Thumb resizeThumb = GetResizeThumb();
		if (resizeThumb != null)
		{
			resizeThumb.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			resizeThumb.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		}
		SetupMenu();
	}

	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
		ClearCachedControls();
	}

	protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs args)
	{
		base.OnIsKeyboardFocusWithinChanged(args);
		if (!(Keyboard.FocusedElement is ContextMenu { PlacementTarget: not null } contextMenu) || !contextMenu.PlacementTarget.IsDescendantOf(this))
		{
			_anchor.Focused = base.IsKeyboardFocusWithin;
		}
	}

	protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs args)
	{
		base.OnGotKeyboardFocus(args);
		ApplyTemplate();
		if (!IsExpanded)
		{
			return;
		}
		Invariant.Assert(Content != null);
		BringToFront();
		if (args.NewFocus == this)
		{
			UIElement innerControl = Content.InnerControl;
			Invariant.Assert(innerControl != null, "InnerControl is null or not a UIElement.");
			if (!innerControl.IsKeyboardFocused)
			{
				innerControl.Focus();
			}
		}
	}

	private void EnsureStickyNoteType()
	{
		UIElement contentContainer = GetContentContainer();
		if (_contentControl != null)
		{
			if (_contentControl.Type != _stickyNoteType)
			{
				DisconnectContent();
				_contentControl = StickyNoteContentControlFactory.CreateContentControl(_stickyNoteType, contentContainer);
				ConnectContent();
			}
		}
		else
		{
			_contentControl = StickyNoteContentControlFactory.CreateContentControl(_stickyNoteType, contentContainer);
			ConnectContent();
		}
	}

	private void DisconnectContent()
	{
		Invariant.Assert(Content != null, "Content is null.");
		StopListenToContentControlEvent();
		UnbindContentControlProperties();
		_contentControl = null;
	}

	private void ConnectContent()
	{
		Invariant.Assert(Content != null);
		if (Content.InnerControl is InkCanvas)
		{
			InitializeEventHandlers();
			SetValue(InkEditingModeProperty, InkCanvasEditingMode.Ink);
			UpdateInkDrawingAttributes();
		}
		StartListenToContentControlEvent();
		BindContentControlProperties();
	}

	private Button GetCloseButton()
	{
		return GetTemplateChild("PART_CloseButton") as Button;
	}

	private Button GetIconButton()
	{
		return GetTemplateChild("PART_IconButton") as Button;
	}

	private Thumb GetTitleThumb()
	{
		return GetTemplateChild("PART_TitleThumb") as Thumb;
	}

	private UIElement GetContentContainer()
	{
		return GetTemplateChild("PART_ContentControl") as UIElement;
	}

	private Thumb GetResizeThumb()
	{
		return GetTemplateChild("PART_ResizeBottomRightThumb") as Thumb;
	}

	private static void _OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((StickyNoteControl)d).OnIsExpandedChanged();
	}

	private static void OnFontPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StickyNoteControl stickyNoteControl = (StickyNoteControl)d;
		if (stickyNoteControl.Content != null && stickyNoteControl.Content.Type != StickyNoteType.Ink)
		{
			stickyNoteControl.Content.InnerControl?.SetValue(e.Property, e.NewValue);
		}
	}

	private static void _UpdateInkDrawingAttributes(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		StickyNoteControl stickyNoteControl = (StickyNoteControl)d;
		stickyNoteControl.UpdateInkDrawingAttributes();
		if (e.Property == Control.ForegroundProperty && stickyNoteControl.Content != null && stickyNoteControl.Content.Type != StickyNoteType.Ink)
		{
			stickyNoteControl.Content.InnerControl?.SetValue(Control.ForegroundProperty, e.NewValue);
		}
	}

	private void OnTextChanged(object obj, TextChangedEventArgs args)
	{
		if (!InternalLocker.IsLocked(LockHelper.LockFlag.DataChanged))
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AnnotationTextChangedBegin);
			try
			{
				AsyncUpdateAnnotation(XmlToken.Text);
				IsDirty = true;
			}
			finally
			{
				EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AnnotationTextChangedEnd);
			}
		}
	}

	private static void _OnDeviceDown<TEventArgs>(object sender, TEventArgs args) where TEventArgs : InputEventArgs
	{
		args.Handled = true;
	}

	private static void _OnContextMenuOpening(object sender, ContextMenuEventArgs args)
	{
		if (!(args.TargetElement is ScrollBar))
		{
			args.Handled = true;
		}
	}

	private static void _OnPreviewDeviceDown<TEventArgs>(object sender, TEventArgs args) where TEventArgs : InputEventArgs
	{
		StickyNoteControl stickyNoteControl = sender as StickyNoteControl;
		IInputElement inputElement = null;
		StylusDevice stylusDevice = null;
		MouseDevice mouseDevice = null;
		if (args.Device is StylusDevice stylusDevice2)
		{
			inputElement = stylusDevice2.Captured;
		}
		else if (args.Device is MouseDevice mouseDevice2)
		{
			inputElement = mouseDevice2.Captured;
		}
		if (stickyNoteControl != null && (inputElement == stickyNoteControl || inputElement == null))
		{
			stickyNoteControl.OnPreviewDeviceDown(sender, args);
		}
	}

	private void OnInkCanvasStrokesReplacedEventHandler(object sender, InkCanvasStrokesReplacedEventArgs e)
	{
		StopListenToStrokesEvent(e.PreviousStrokes);
		StartListenToStrokesEvent(e.NewStrokes);
	}

	private void OnInkCanvasSelectionMovingEventHandler(object sender, InkCanvasSelectionEditingEventArgs e)
	{
		Rect newRectangle = e.NewRectangle;
		if (newRectangle.X < 0.0 || newRectangle.Y < 0.0)
		{
			newRectangle.X = ((newRectangle.X < 0.0) ? 0.0 : newRectangle.X);
			newRectangle.Y = ((newRectangle.Y < 0.0) ? 0.0 : newRectangle.Y);
			e.NewRectangle = newRectangle;
		}
	}

	private void OnInkCanvasSelectionResizingEventHandler(object sender, InkCanvasSelectionEditingEventArgs e)
	{
		Rect newRectangle = e.NewRectangle;
		if (newRectangle.X < 0.0 || newRectangle.Y < 0.0)
		{
			if (newRectangle.X < 0.0)
			{
				newRectangle.Width += newRectangle.X;
				newRectangle.X = 0.0;
			}
			if (newRectangle.Y < 0.0)
			{
				newRectangle.Height += newRectangle.Y;
				newRectangle.Y = 0.0;
			}
			e.NewRectangle = newRectangle;
		}
	}

	private void OnInkStrokesChanged(object sender, StrokeCollectionChangedEventArgs args)
	{
		StopListenToStrokeEvent(args.Removed);
		StartListenToStrokeEvent(args.Added);
		if (args.Removed.Count > 0 || args.Added.Count > 0)
		{
			Invariant.Assert(Content != null && Content.InnerControl is InkCanvas);
			if (VisualTreeHelper.GetParent(Content.InnerControl) is FrameworkElement frameworkElement)
			{
				frameworkElement.InvalidateMeasure();
			}
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AnnotationInkChangedBegin);
		try
		{
			UpdateAnnotationWithSNC(XmlToken.Ink);
			IsDirty = true;
		}
		finally
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AnnotationInkChangedEnd);
		}
	}

	private void InitStickyNoteControl()
	{
		XmlQualifiedName type = ((_stickyNoteType == StickyNoteType.Text) ? TextSchemaName : InkSchemaName);
		_anchor = new MarkedHighlightComponent(type, this);
		IsDirty = false;
		base.Loaded += OnLoadedEventHandler;
	}

	private void InitializeEventHandlers()
	{
		_propertyDataChangedHandler = new StrokeChangedHandler<PropertyDataChangedEventArgs>(this);
		_strokeDrawingAttributesReplacedHandler = new StrokeChangedHandler<DrawingAttributesReplacedEventArgs>(this);
		_strokePacketDataChangedHandler = new StrokeChangedHandler<EventArgs>(this);
	}

	private void OnButtonClick(object sender, RoutedEventArgs e)
	{
		bool isExpanded = IsExpanded;
		SetCurrentValueInternal(IsExpandedProperty, BooleanBoxes.Box(!isExpanded));
	}

	private void DeleteStickyNote()
	{
		Invariant.Assert(_attachedAnnotation != null, "AttachedAnnotation is null.");
		Invariant.Assert(_attachedAnnotation.Store != null, "AttachedAnnotation's Store is null.");
		_attachedAnnotation.Store.DeleteAnnotation(_attachedAnnotation.Annotation.Id);
	}

	private void OnDragCompleted(object sender, DragCompletedEventArgs args)
	{
		Thumb thumb = args.Source as Thumb;
		if (thumb == GetTitleThumb())
		{
			UpdateAnnotationWithSNC(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset);
		}
		else if (thumb == GetResizeThumb())
		{
			UpdateAnnotationWithSNC(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height);
		}
	}

	private void OnDragDelta(object sender, DragDeltaEventArgs args)
	{
		Invariant.Assert(IsExpanded, "Dragging occurred when the StickyNoteControl was not expanded.");
		Thumb thumb = args.Source as Thumb;
		double num = args.HorizontalChange;
		if (_selfMirroring)
		{
			num = 0.0 - num;
		}
		if (thumb == GetTitleThumb())
		{
			OnTitleDragDelta(num, args.VerticalChange);
		}
		else if (thumb == GetResizeThumb())
		{
			OnResizeDragDelta(args.HorizontalChange, args.VerticalChange);
		}
		UpdateOffsets();
	}

	private void OnTitleDragDelta(double horizontalChange, double verticalChange)
	{
		Invariant.Assert(IsExpanded);
		Rect stickyNoteBounds = StickyNoteBounds;
		Rect pageBounds = PageBounds;
		double num = 45.0;
		double num2 = 20.0;
		if (_selfMirroring)
		{
			double num3 = num2;
			num2 = num;
			num = num3;
		}
		Point point = new Point(0.0 - (stickyNoteBounds.X + stickyNoteBounds.Width - num), 0.0 - stickyNoteBounds.Y);
		Point point2 = new Point(pageBounds.Width - stickyNoteBounds.X - num2, pageBounds.Height - stickyNoteBounds.Y - 20.0);
		horizontalChange = Math.Min(Math.Max(point.X, horizontalChange), point2.X);
		verticalChange = Math.Min(Math.Max(point.Y, verticalChange), point2.Y);
		TranslateTransform positionTransform = PositionTransform;
		PositionTransform = new TranslateTransform(positionTransform.X + horizontalChange + _deltaX, positionTransform.Y + verticalChange + _deltaY);
		_deltaX = (_deltaY = 0.0);
		IsDirty = true;
	}

	private void OnResizeDragDelta(double horizontalChange, double verticalChange)
	{
		Invariant.Assert(IsExpanded);
		Rect stickyNoteBounds = StickyNoteBounds;
		double num = stickyNoteBounds.Width + horizontalChange;
		double num2 = stickyNoteBounds.Height + verticalChange;
		if (!_selfMirroring && stickyNoteBounds.X + num < 45.0)
		{
			num = stickyNoteBounds.Width;
		}
		double minWidth = base.MinWidth;
		double minHeight = base.MinHeight;
		if (num < minWidth)
		{
			num = minWidth;
			horizontalChange = num - base.Width;
		}
		if (num2 < minHeight)
		{
			num2 = minHeight;
		}
		SetCurrentValueInternal(FrameworkElement.WidthProperty, num);
		SetCurrentValueInternal(FrameworkElement.HeightProperty, num2);
		if (_selfMirroring)
		{
			OnTitleDragDelta(0.0 - horizontalChange, 0.0);
		}
		else
		{
			OnTitleDragDelta(0.0, 0.0);
		}
		IsDirty = true;
	}

	private void OnPreviewDeviceDown(object dc, InputEventArgs args)
	{
		if (IsExpanded)
		{
			bool flag = false;
			if (!base.IsKeyboardFocusWithin && StickyNoteType == StickyNoteType.Ink && args.OriginalSource is Visual visual)
			{
				Invariant.Assert(Content.InnerControl != null, "InnerControl is null.");
				flag = visual.IsDescendantOf(Content.InnerControl);
			}
			BringToFront();
			if (!IsActive || !base.IsKeyboardFocusWithin)
			{
				Focus();
			}
			if (flag)
			{
				args.Handled = true;
			}
		}
	}

	private void OnLoadedEventHandler(object sender, RoutedEventArgs e)
	{
		if (IsExpanded)
		{
			UpdateSNCWithAnnotation(XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height);
			if (_sncAnnotation.IsNewAnnotation)
			{
				Focus();
			}
		}
		base.Loaded -= OnLoadedEventHandler;
	}

	private void ClearCachedControls()
	{
		if (Content != null)
		{
			DisconnectContent();
		}
		GetCloseButton()?.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));
		GetIconButton()?.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnButtonClick));
		Thumb titleThumb = GetTitleThumb();
		if (titleThumb != null)
		{
			titleThumb.RemoveHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			titleThumb.RemoveHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		}
		Thumb resizeThumb = GetResizeThumb();
		if (resizeThumb != null)
		{
			resizeThumb.RemoveHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
			resizeThumb.RemoveHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		}
	}

	private void OnIsExpandedChanged()
	{
		InvalidateTransform();
		UpdateAnnotationWithSNC(XmlToken.IsExpanded);
		IsDirty = true;
		if (IsExpanded)
		{
			BringToFront();
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(AsyncTakeFocus), null);
		}
		else
		{
			GiveUpFocus();
			SendToBack();
		}
	}

	private object AsyncTakeFocus(object notUsed)
	{
		Focus();
		return null;
	}

	private void GiveUpFocus()
	{
		if (!base.IsKeyboardFocusWithin)
		{
			return;
		}
		bool flag = false;
		DependencyObject dependencyObject = _attachedAnnotation.Parent;
		IInputElement inputElement = null;
		while (dependencyObject != null && !flag)
		{
			if (dependencyObject is IInputElement inputElement2)
			{
				flag = inputElement2.Focus();
			}
			if (!flag)
			{
				dependencyObject = FrameworkElement.GetFrameworkParent(dependencyObject);
			}
		}
		if (!flag)
		{
			Keyboard.Focus(null);
		}
	}

	private void BringToFront()
	{
		((IAnnotationComponent)this).PresentationContext?.BringToFront(this);
	}

	private void SendToBack()
	{
		((IAnnotationComponent)this).PresentationContext?.SendToBack(this);
	}

	private void InvalidateTransform()
	{
		((IAnnotationComponent)this).PresentationContext?.InvalidateTransform(this);
	}

	private object AsyncUpdateAnnotation(object arg)
	{
		UpdateAnnotationWithSNC((XmlToken)arg);
		return null;
	}

	private void BindContentControlProperties()
	{
		Invariant.Assert(Content != null);
		if (Content.Type != StickyNoteType.Ink)
		{
			FrameworkElement innerControl = Content.InnerControl;
			innerControl.SetValue(Control.FontFamilyProperty, GetValue(Control.FontFamilyProperty));
			innerControl.SetValue(Control.FontSizeProperty, GetValue(Control.FontSizeProperty));
			innerControl.SetValue(Control.FontStretchProperty, GetValue(Control.FontStretchProperty));
			innerControl.SetValue(Control.FontStyleProperty, GetValue(Control.FontStyleProperty));
			innerControl.SetValue(Control.FontWeightProperty, GetValue(Control.FontWeightProperty));
			innerControl.SetValue(Control.ForegroundProperty, GetValue(Control.ForegroundProperty));
		}
		else
		{
			MultiBinding multiBinding = new MultiBinding();
			multiBinding.Mode = BindingMode.TwoWay;
			multiBinding.Converter = new InkEditingModeIsKeyboardFocusWithin2EditingMode();
			Binding binding = new Binding();
			binding.Mode = BindingMode.TwoWay;
			binding.Path = new PropertyPath(InkEditingModeProperty);
			binding.Source = this;
			multiBinding.Bindings.Add(binding);
			Binding binding2 = new Binding();
			binding2.Path = new PropertyPath(UIElement.IsKeyboardFocusWithinProperty);
			binding2.Source = this;
			multiBinding.Bindings.Add(binding2);
			Content.InnerControl.SetBinding(InkCanvas.EditingModeProperty, multiBinding);
		}
	}

	private void UnbindContentControlProperties()
	{
		Invariant.Assert(Content != null);
		FrameworkElement innerControl = Content.InnerControl;
		if (Content.Type != StickyNoteType.Ink)
		{
			innerControl.ClearValue(Control.FontFamilyProperty);
			innerControl.ClearValue(Control.FontSizeProperty);
			innerControl.ClearValue(Control.FontStretchProperty);
			innerControl.ClearValue(Control.FontStyleProperty);
			innerControl.ClearValue(Control.FontWeightProperty);
			innerControl.ClearValue(Control.ForegroundProperty);
		}
		else
		{
			BindingOperations.ClearBinding(innerControl, InkCanvas.EditingModeProperty);
		}
	}

	private void StartListenToContentControlEvent()
	{
		Invariant.Assert(Content != null);
		if (Content.Type == StickyNoteType.Ink)
		{
			InkCanvas inkCanvas = Content.InnerControl as InkCanvas;
			Invariant.Assert(inkCanvas != null, "InnerControl is not an InkCanvas for note of type Ink.");
			inkCanvas.StrokesReplaced += OnInkCanvasStrokesReplacedEventHandler;
			inkCanvas.SelectionMoving += OnInkCanvasSelectionMovingEventHandler;
			inkCanvas.SelectionResizing += OnInkCanvasSelectionResizingEventHandler;
			StartListenToStrokesEvent(inkCanvas.Strokes);
		}
		else
		{
			TextBoxBase obj = Content.InnerControl as TextBoxBase;
			Invariant.Assert(obj != null, "InnerControl is not a TextBoxBase for note of type Text.");
			obj.TextChanged += OnTextChanged;
		}
	}

	private void StopListenToContentControlEvent()
	{
		Invariant.Assert(Content != null);
		if (Content.Type == StickyNoteType.Ink)
		{
			InkCanvas inkCanvas = Content.InnerControl as InkCanvas;
			Invariant.Assert(inkCanvas != null, "InnerControl is not an InkCanvas for note of type Ink.");
			inkCanvas.StrokesReplaced -= OnInkCanvasStrokesReplacedEventHandler;
			inkCanvas.SelectionMoving -= OnInkCanvasSelectionMovingEventHandler;
			inkCanvas.SelectionResizing -= OnInkCanvasSelectionResizingEventHandler;
			StopListenToStrokesEvent(inkCanvas.Strokes);
		}
		else
		{
			TextBoxBase obj = Content.InnerControl as TextBoxBase;
			Invariant.Assert(obj != null, "InnerControl is not a TextBoxBase for note of type Text.");
			obj.TextChanged -= OnTextChanged;
		}
	}

	private void StartListenToStrokesEvent(StrokeCollection strokes)
	{
		strokes.StrokesChanged += OnInkStrokesChanged;
		strokes.PropertyDataChanged += _propertyDataChangedHandler.OnStrokeChanged;
		StartListenToStrokeEvent(strokes);
	}

	private void StopListenToStrokesEvent(StrokeCollection strokes)
	{
		strokes.StrokesChanged -= OnInkStrokesChanged;
		strokes.PropertyDataChanged -= _propertyDataChangedHandler.OnStrokeChanged;
		StopListenToStrokeEvent(strokes);
	}

	private void StartListenToStrokeEvent(IEnumerable<Stroke> strokes)
	{
		foreach (Stroke stroke in strokes)
		{
			stroke.DrawingAttributes.AttributeChanged += _propertyDataChangedHandler.OnStrokeChanged;
			stroke.DrawingAttributesReplaced += _strokeDrawingAttributesReplacedHandler.OnStrokeChanged;
			stroke.StylusPointsReplaced += _strokePacketDataChangedHandler.OnStrokeChanged;
			stroke.StylusPoints.Changed += _strokePacketDataChangedHandler.OnStrokeChanged;
			stroke.PropertyDataChanged += _propertyDataChangedHandler.OnStrokeChanged;
		}
	}

	private void StopListenToStrokeEvent(IEnumerable<Stroke> strokes)
	{
		foreach (Stroke stroke in strokes)
		{
			stroke.DrawingAttributes.AttributeChanged -= _propertyDataChangedHandler.OnStrokeChanged;
			stroke.DrawingAttributesReplaced -= _strokeDrawingAttributesReplacedHandler.OnStrokeChanged;
			stroke.StylusPointsReplaced -= _strokePacketDataChangedHandler.OnStrokeChanged;
			stroke.StylusPoints.Changed -= _strokePacketDataChangedHandler.OnStrokeChanged;
			stroke.PropertyDataChanged -= _propertyDataChangedHandler.OnStrokeChanged;
		}
	}

	private void SetupMenu()
	{
		MenuItem inkMenuItem = GetInkMenuItem();
		if (inkMenuItem != null)
		{
			Binding binding = new Binding("InkEditingMode");
			binding.Mode = BindingMode.OneWay;
			binding.RelativeSource = RelativeSource.TemplatedParent;
			binding.Converter = new InkEditingModeConverter();
			binding.ConverterParameter = InkCanvasEditingMode.Ink;
			inkMenuItem.SetBinding(MenuItem.IsCheckedProperty, binding);
		}
		MenuItem selectMenuItem = GetSelectMenuItem();
		if (selectMenuItem != null)
		{
			Binding binding2 = new Binding("InkEditingMode");
			binding2.Mode = BindingMode.OneWay;
			binding2.RelativeSource = RelativeSource.TemplatedParent;
			binding2.Converter = new InkEditingModeConverter();
			binding2.ConverterParameter = InkCanvasEditingMode.Select;
			selectMenuItem.SetBinding(MenuItem.IsCheckedProperty, binding2);
		}
		MenuItem eraseMenuItem = GetEraseMenuItem();
		if (eraseMenuItem != null)
		{
			Binding binding3 = new Binding("InkEditingMode");
			binding3.Mode = BindingMode.OneWay;
			binding3.RelativeSource = RelativeSource.TemplatedParent;
			binding3.Converter = new InkEditingModeConverter();
			binding3.ConverterParameter = InkCanvasEditingMode.EraseByStroke;
			eraseMenuItem.SetBinding(MenuItem.IsCheckedProperty, binding3);
		}
		MenuItem copyMenuItem = GetCopyMenuItem();
		if (copyMenuItem != null)
		{
			copyMenuItem.CommandTarget = Content.InnerControl;
		}
		MenuItem pasteMenuItem = GetPasteMenuItem();
		if (pasteMenuItem != null)
		{
			pasteMenuItem.CommandTarget = Content.InnerControl;
		}
	}

	private static void _OnCommandExecuted(object sender, ExecutedRoutedEventArgs args)
	{
		RoutedCommand routedCommand = (RoutedCommand)args.Command;
		StickyNoteControl stickyNoteControl = sender as StickyNoteControl;
		Invariant.Assert(stickyNoteControl != null, "Unexpected Commands");
		Invariant.Assert(routedCommand == DeleteNoteCommand || routedCommand == InkCommand, "Unknown Commands");
		if (routedCommand == DeleteNoteCommand)
		{
			stickyNoteControl.DeleteStickyNote();
		}
		else if (routedCommand == InkCommand)
		{
			StickyNoteContentControl content = stickyNoteControl.Content;
			if (content == null || content.Type != StickyNoteType.Ink)
			{
				throw new InvalidOperationException(SR.CannotProcessInkCommand);
			}
			InkCanvasEditingMode inkCanvasEditingMode = (InkCanvasEditingMode)args.Parameter;
			stickyNoteControl.SetValue(InkEditingModeProperty, inkCanvasEditingMode);
		}
	}

	private static void _OnQueryCommandEnabled(object sender, CanExecuteRoutedEventArgs args)
	{
		RoutedCommand routedCommand = (RoutedCommand)args.Command;
		StickyNoteControl stickyNoteControl = sender as StickyNoteControl;
		Invariant.Assert(stickyNoteControl != null, "Unexpected Commands");
		Invariant.Assert(routedCommand == DeleteNoteCommand || routedCommand == InkCommand, "Unknown Commands");
		if (routedCommand == DeleteNoteCommand)
		{
			args.CanExecute = stickyNoteControl._attachedAnnotation != null;
		}
		else if (routedCommand == InkCommand)
		{
			StickyNoteContentControl content = stickyNoteControl.Content;
			args.CanExecute = content != null && content.Type == StickyNoteType.Ink;
		}
		else
		{
			Invariant.Assert(condition: false, "Unknown command.");
		}
	}

	private void UpdateInkDrawingAttributes()
	{
		if (Content != null && Content.Type == StickyNoteType.Ink)
		{
			DrawingAttributes drawingAttributes = new DrawingAttributes();
			if (!(base.Foreground is SolidColorBrush solidColorBrush))
			{
				throw new ArgumentException(SR.InvalidInkForeground);
			}
			drawingAttributes.StylusTip = StylusTip.Ellipse;
			drawingAttributes.Width = PenWidth;
			drawingAttributes.Height = PenWidth;
			drawingAttributes.Color = solidColorBrush.Color;
			((InkCanvas)Content.InnerControl).DefaultDrawingAttributes = drawingAttributes;
		}
	}

	private MenuItem GetInkMenuItem()
	{
		return GetTemplateChild("PART_InkMenuItem") as MenuItem;
	}

	private MenuItem GetSelectMenuItem()
	{
		return GetTemplateChild("PART_SelectMenuItem") as MenuItem;
	}

	private MenuItem GetEraseMenuItem()
	{
		return GetTemplateChild("PART_EraseMenuItem") as MenuItem;
	}

	private MenuItem GetCopyMenuItem()
	{
		return GetTemplateChild("PART_CopyMenuItem") as MenuItem;
	}

	private MenuItem GetPasteMenuItem()
	{
		return GetTemplateChild("PART_PasteMenuItem") as MenuItem;
	}

	private Separator GetClipboardSeparator()
	{
		return GetTemplateChild("PART_ClipboardSeparator") as Separator;
	}
}
