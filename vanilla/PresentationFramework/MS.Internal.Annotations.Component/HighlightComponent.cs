using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using MS.Utility;

namespace MS.Internal.Annotations.Component;

internal class HighlightComponent : Canvas, IAnnotationComponent, IHighlightRange
{
	public static DependencyProperty HighlightBrushProperty = DependencyProperty.Register("HighlightBrushProperty", typeof(Brush), typeof(HighlightComponent));

	public const string HighlightResourceName = "Highlight";

	public const string ColorsContentName = "Colors";

	public const string BackgroundAttributeName = "Background";

	public const string ActiveBackgroundAttributeName = "ActiveBackground";

	private Color _background;

	private Color _selectedBackground;

	private TextAnchor _range;

	private IAttachedAnnotation _attachedAnnotation;

	private PresentationContext _presentationContext;

	private static readonly XmlQualifiedName _name = new XmlQualifiedName("Highlight", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	private XmlQualifiedName _type = _name;

	private int _priority;

	private bool _highlightContent = true;

	private bool _active;

	private bool _isDirty = true;

	private Color _defaultBackroundColor = (Color)ColorConverter.ConvertFromString("#33FFFF00");

	private Color _defaultActiveBackgroundColor = (Color)ColorConverter.ConvertFromString("#339ACD32");

	public IList AttachedAnnotations
	{
		get
		{
			ArrayList arrayList = new ArrayList();
			if (_attachedAnnotation != null)
			{
				arrayList.Add(_attachedAnnotation);
			}
			return arrayList;
		}
	}

	public PresentationContext PresentationContext
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

	public int ZOrder
	{
		get
		{
			return -1;
		}
		set
		{
		}
	}

	public static XmlQualifiedName TypeName => _name;

	public Color DefaultBackground
	{
		get
		{
			return _defaultBackroundColor;
		}
		set
		{
			_defaultBackroundColor = value;
		}
	}

	public Color DefaultActiveBackground
	{
		get
		{
			return _defaultActiveBackgroundColor;
		}
		set
		{
			_defaultActiveBackgroundColor = value;
		}
	}

	public Brush HighlightBrush
	{
		set
		{
			SetValue(HighlightBrushProperty, value);
		}
	}

	public UIElement AnnotatedElement
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

	public bool IsDirty
	{
		get
		{
			return _isDirty;
		}
		set
		{
			_isDirty = value;
			if (value)
			{
				InvalidateChildren();
			}
		}
	}

	Color IHighlightRange.Background => _background;

	Color IHighlightRange.SelectedBackground => _selectedBackground;

	TextAnchor IHighlightRange.Range => _range;

	int IHighlightRange.Priority => _priority;

	bool IHighlightRange.HighlightContent => _highlightContent;

	public HighlightComponent()
	{
	}

	public HighlightComponent(int priority, bool highlightContent, XmlQualifiedName type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		_priority = priority;
		_type = type;
		_highlightContent = highlightContent;
	}

	public GeneralTransform GetDesiredTransform(GeneralTransform transform)
	{
		return transform;
	}

	public void AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (_attachedAnnotation != null)
		{
			throw new ArgumentException(SR.MoreThanOneAttachedAnnotation);
		}
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedHighlightBegin);
		ITextContainer textContainer = CheckInputData(attachedAnnotation);
		TextAnchor range = attachedAnnotation.AttachedAnchor as TextAnchor;
		GetColors(attachedAnnotation.Annotation, out _background, out _selectedBackground);
		_range = range;
		Invariant.Assert(textContainer.Highlights != null, "textContainer.Highlights is null");
		AnnotationHighlightLayer annotationHighlightLayer = textContainer.Highlights.GetLayer(typeof(HighlightComponent)) as AnnotationHighlightLayer;
		if (annotationHighlightLayer == null)
		{
			annotationHighlightLayer = new AnnotationHighlightLayer();
			textContainer.Highlights.AddLayer(annotationHighlightLayer);
		}
		_attachedAnnotation = attachedAnnotation;
		_attachedAnnotation.Annotation.CargoChanged += OnAnnotationUpdated;
		annotationHighlightLayer.AddRange(this);
		HighlightBrush = new SolidColorBrush(_background);
		base.IsHitTestVisible = false;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.AddAttachedHighlightEnd);
	}

	public void RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		if (attachedAnnotation != _attachedAnnotation)
		{
			throw new ArgumentException(SR.InvalidAttachedAnnotation, "attachedAnnotation");
		}
		Invariant.Assert(_range != null, "null highlight range");
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedHighlightBegin);
		ITextContainer textContainer = CheckInputData(attachedAnnotation);
		Invariant.Assert(textContainer.Highlights != null, "textContainer.Highlights is null");
		AnnotationHighlightLayer obj = textContainer.Highlights.GetLayer(typeof(HighlightComponent)) as AnnotationHighlightLayer;
		Invariant.Assert(obj != null, "AnnotationHighlightLayer is not initialized");
		_attachedAnnotation.Annotation.CargoChanged -= OnAnnotationUpdated;
		obj.RemoveRange(this);
		_attachedAnnotation = null;
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordAnnotation, EventTrace.Event.RemoveAttachedHighlightEnd);
	}

	public void ModifyAttachedAnnotation(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		throw new NotSupportedException(SR.NotSupported);
	}

	public void Activate(bool active)
	{
		if (_active != active)
		{
			if (_attachedAnnotation == null)
			{
				throw new InvalidOperationException(SR.NoAttachedAnnotationToModify);
			}
			TextAnchor obj = _attachedAnnotation.AttachedAnchor as TextAnchor;
			Invariant.Assert(obj != null, "AttachedAnchor is not a text anchor");
			ITextContainer textContainer = obj.Start.TextContainer;
			Invariant.Assert(textContainer != null, "TextAnchor does not belong to a TextContainer");
			AnnotationHighlightLayer obj2 = textContainer.Highlights.GetLayer(typeof(HighlightComponent)) as AnnotationHighlightLayer;
			Invariant.Assert(obj2 != null, "AnnotationHighlightLayer is not initialized");
			obj2.ActivateRange(this, active);
			_active = active;
			if (active)
			{
				HighlightBrush = new SolidColorBrush(_selectedBackground);
			}
			else
			{
				HighlightBrush = new SolidColorBrush(_background);
			}
		}
	}

	void IHighlightRange.AddChild(Shape child)
	{
		base.Children.Add(child);
	}

	void IHighlightRange.RemoveChild(Shape child)
	{
		base.Children.Remove(child);
	}

	internal bool IsSelected(ITextRange selection)
	{
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		Invariant.Assert(_attachedAnnotation != null, "No _attachedAnnotation");
		if (!(_attachedAnnotation.FullyAttachedAnchor is TextAnchor textAnchor))
		{
			return false;
		}
		return textAnchor.IsOverlapping(selection.TextSegments);
	}

	internal static void GetCargoColors(Annotation annot, ref Color? backgroundColor, ref Color? activeBackgroundColor)
	{
		Invariant.Assert(annot != null, "annotation is null");
		ICollection<AnnotationResource> cargos = annot.Cargos;
		if (cargos == null)
		{
			return;
		}
		foreach (AnnotationResource item in cargos)
		{
			if (!(item.Name == "Highlight"))
			{
				continue;
			}
			foreach (XmlElement item2 in (IEnumerable)item.Contents)
			{
				if (item2.LocalName == "Colors" && item2.NamespaceURI == "http://schemas.microsoft.com/windows/annotations/2003/11/base")
				{
					if (item2.Attributes["Background"] != null)
					{
						backgroundColor = GetColor(item2.Attributes["Background"].Value);
					}
					if (item2.Attributes["ActiveBackground"] != null)
					{
						activeBackgroundColor = GetColor(item2.Attributes["ActiveBackground"].Value);
					}
				}
			}
		}
	}

	private ITextContainer CheckInputData(IAttachedAnnotation attachedAnnotation)
	{
		if (attachedAnnotation == null)
		{
			throw new ArgumentNullException("attachedAnnotation");
		}
		ITextContainer textContainer = ((attachedAnnotation.AttachedAnchor as TextAnchor) ?? throw new ArgumentException(SR.InvalidAttachedAnchor, "attachedAnnotation")).Start.TextContainer;
		Invariant.Assert(textContainer != null, "TextAnchor does not belong to a TextContainer");
		if (attachedAnnotation.Annotation == null)
		{
			throw new ArgumentException(SR.AnnotationIsNull, "attachedAnnotation");
		}
		if (!_type.Equals(attachedAnnotation.Annotation.AnnotationType))
		{
			throw new ArgumentException(SR.Format(SR.NotHighlightAnnotationType, attachedAnnotation.Annotation.AnnotationType.ToString()), "attachedAnnotation");
		}
		return textContainer;
	}

	private static Color GetColor(string color)
	{
		return (Color)ColorConverter.ConvertFromString(color);
	}

	private void GetColors(Annotation annot, out Color backgroundColor, out Color activeBackgroundColor)
	{
		Color? backgroundColor2 = _defaultBackroundColor;
		Color? activeBackgroundColor2 = _defaultActiveBackgroundColor;
		GetCargoColors(annot, ref backgroundColor2, ref activeBackgroundColor2);
		backgroundColor = backgroundColor2.Value;
		activeBackgroundColor = activeBackgroundColor2.Value;
	}

	private void OnAnnotationUpdated(object sender, AnnotationResourceChangedEventArgs args)
	{
		Invariant.Assert(_attachedAnnotation != null && _attachedAnnotation.Annotation == args.Annotation, "_attachedAnnotation is different than the input one");
		Invariant.Assert(_range != null, "The highlight range is null");
		TextAnchor obj = _attachedAnnotation.AttachedAnchor as TextAnchor;
		Invariant.Assert(obj != null, "wrong anchor type of the saved attached annotation");
		ITextContainer textContainer = obj.Start.TextContainer;
		Invariant.Assert(textContainer != null, "TextAnchor does not belong to a TextContainer");
		GetColors(args.Annotation, out var backgroundColor, out var activeBackgroundColor);
		if (!_background.Equals(backgroundColor) || !_selectedBackground.Equals(activeBackgroundColor))
		{
			Invariant.Assert(textContainer.Highlights != null, "textContainer.Highlights is null");
			AnnotationHighlightLayer obj2 = (textContainer.Highlights.GetLayer(typeof(HighlightComponent)) as AnnotationHighlightLayer) ?? throw new InvalidDataException(SR.MissingAnnotationHighlightLayer);
			_background = backgroundColor;
			_selectedBackground = activeBackgroundColor;
			obj2.ModifiedRange(this);
		}
	}

	private void InvalidateChildren()
	{
		foreach (Visual child in base.Children)
		{
			Shape obj = child as Shape;
			Invariant.Assert(obj != null, "HighlightComponent has non-Shape children.");
			obj.InvalidateMeasure();
		}
		IsDirty = false;
	}
}
