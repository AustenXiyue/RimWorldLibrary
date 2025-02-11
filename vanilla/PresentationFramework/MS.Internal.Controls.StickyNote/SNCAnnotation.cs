using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using MS.Internal.Annotations.Component;

namespace MS.Internal.Controls.StickyNote;

internal class SNCAnnotation
{
	public const XmlToken AllValues = XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder;

	public const XmlToken PositionValues = XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset;

	public const XmlToken Sizes = XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height;

	public const XmlToken AllContents = XmlToken.Text | XmlToken.Ink;

	public const XmlToken NegativeAllContents = XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.ZOrder;

	private static Dictionary<XmlToken, string> s_xmlTokeFullNames;

	private Dictionary<XmlToken, object> _cachedXmlElements;

	private Annotation _annotation;

	private readonly bool _isNewAnnotation;

	public bool IsNewAnnotation => _isNewAnnotation;

	public bool HasInkData => FindData(XmlToken.Ink) != null;

	public bool HasTextData => FindData(XmlToken.Text) != null;

	static SNCAnnotation()
	{
		s_xmlTokeFullNames = new Dictionary<XmlToken, string>();
		foreach (XmlToken value in Enum.GetValues(typeof(XmlToken)))
		{
			AddXmlTokenNames(value);
		}
	}

	public SNCAnnotation(Annotation annotation)
	{
		_annotation = annotation;
		_isNewAnnotation = _annotation.Cargos.Count == 0;
		_cachedXmlElements = new Dictionary<XmlToken, object>();
	}

	private SNCAnnotation()
	{
	}

	public static void UpdateAnnotation(XmlToken token, StickyNoteControl snc, SNCAnnotation sncAnnotation)
	{
		AnnotationService annotationService = null;
		bool autoFlush = false;
		try
		{
			annotationService = AnnotationService.GetService(((IAnnotationComponent)snc).AnnotatedElement);
			if (annotationService != null && annotationService.Store != null)
			{
				autoFlush = annotationService.Store.AutoFlush;
				annotationService.Store.AutoFlush = false;
			}
			if ((token & XmlToken.Ink) != 0 && snc.Content.Type == StickyNoteType.Ink)
			{
				sncAnnotation.UpdateContent(snc, updateAnnotation: true, XmlToken.Ink);
			}
			if ((token & XmlToken.Text) != 0 && snc.Content.Type == StickyNoteType.Text)
			{
				sncAnnotation.UpdateContent(snc, updateAnnotation: true, XmlToken.Text);
			}
			if ((token & (XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.ZOrder)) != 0)
			{
				UpdateMetaData(token, snc, sncAnnotation);
			}
		}
		finally
		{
			if (annotationService != null && annotationService.Store != null)
			{
				annotationService.Store.AutoFlush = autoFlush;
			}
		}
	}

	public static void UpdateStickyNoteControl(XmlToken token, StickyNoteControl snc, SNCAnnotation sncAnnotation)
	{
		Invariant.Assert((token & (XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder)) != 0, "No token specified.");
		Invariant.Assert(snc != null, "Sticky Note Control is null.");
		Invariant.Assert(sncAnnotation != null, "Annotation is null.");
		if ((token & XmlToken.Ink) != 0 && sncAnnotation.HasInkData)
		{
			sncAnnotation.UpdateContent(snc, updateAnnotation: false, XmlToken.Ink);
		}
		if ((token & XmlToken.Text) != 0 && sncAnnotation.HasTextData)
		{
			sncAnnotation.UpdateContent(snc, updateAnnotation: false, XmlToken.Text);
		}
		if ((token & XmlToken.Author) != 0)
		{
			int count = sncAnnotation._annotation.Authors.Count;
			string listSeparator = snc.Language.GetSpecificCulture().TextInfo.ListSeparator;
			string text = string.Empty;
			for (int i = 0; i < count; i++)
			{
				text = ((i == 0) ? (text + sncAnnotation._annotation.Authors[i]) : (text + listSeparator + sncAnnotation._annotation.Authors[i]));
			}
			snc.SetValue(StickyNoteControl.AuthorPropertyKey, text);
		}
		if ((token & XmlToken.Height) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.Height);
			if (xmlAttribute != null)
			{
				double num = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
				snc.SetValue(FrameworkElement.HeightProperty, num);
			}
			else
			{
				snc.ClearValue(FrameworkElement.HeightProperty);
			}
		}
		if ((token & XmlToken.Width) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.Width);
			if (xmlAttribute != null)
			{
				double num2 = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
				snc.SetValue(FrameworkElement.WidthProperty, num2);
			}
			else
			{
				snc.ClearValue(FrameworkElement.WidthProperty);
			}
		}
		if ((token & XmlToken.IsExpanded) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.IsExpanded);
			if (xmlAttribute != null)
			{
				bool isExpanded = Convert.ToBoolean(xmlAttribute.Value, CultureInfo.InvariantCulture);
				snc.IsExpanded = isExpanded;
			}
			else
			{
				snc.ClearValue(StickyNoteControl.IsExpandedProperty);
			}
		}
		if ((token & XmlToken.ZOrder) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.ZOrder);
			if (xmlAttribute != null)
			{
				((IAnnotationComponent)snc).ZOrder = Convert.ToInt32(xmlAttribute.Value, CultureInfo.InvariantCulture);
			}
		}
		if ((token & (XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset)) == 0)
		{
			return;
		}
		TranslateTransform translateTransform = new TranslateTransform();
		if ((token & XmlToken.Left) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.Left);
			if (xmlAttribute != null)
			{
				double num3 = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
				if (snc.FlipBothOrigins)
				{
					num3 = 0.0 - (num3 + snc.Width);
				}
				translateTransform.X = num3;
			}
		}
		if ((token & XmlToken.Top) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.Top);
			if (xmlAttribute != null)
			{
				double y = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
				translateTransform.Y = y;
			}
		}
		if ((token & XmlToken.XOffset) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.XOffset);
			if (xmlAttribute != null)
			{
				snc.XOffset = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
			}
		}
		if ((token & XmlToken.YOffset) != 0)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)sncAnnotation.FindData(XmlToken.YOffset);
			if (xmlAttribute != null)
			{
				snc.YOffset = Convert.ToDouble(xmlAttribute.Value, CultureInfo.InvariantCulture);
			}
		}
		snc.PositionTransform = translateTransform;
	}

	private AnnotationResource FindCargo(string cargoName)
	{
		foreach (AnnotationResource cargo in _annotation.Cargos)
		{
			if (cargoName.Equals(cargo.Name))
			{
				return cargo;
			}
		}
		return null;
	}

	private object FindData(XmlToken token)
	{
		object obj = null;
		if (_cachedXmlElements.ContainsKey(token))
		{
			obj = _cachedXmlElements[token];
		}
		else
		{
			AnnotationResource annotationResource = FindCargo(GetCargoName(token));
			if (annotationResource != null)
			{
				obj = FindContent(token, annotationResource);
				if (obj != null)
				{
					_cachedXmlElements.Add(token, obj);
				}
			}
		}
		return obj;
	}

	private static void GetCargoAndRoot(SNCAnnotation annotation, XmlToken token, out AnnotationResource cargo, out XmlElement root, out bool newCargo, out bool newRoot)
	{
		Invariant.Assert(annotation != null, "Annotation is null.");
		Invariant.Assert((token & (XmlToken.MetaData | XmlToken.Left | XmlToken.Top | XmlToken.XOffset | XmlToken.YOffset | XmlToken.Width | XmlToken.Height | XmlToken.IsExpanded | XmlToken.Author | XmlToken.Text | XmlToken.Ink | XmlToken.ZOrder)) != 0, "No token specified.");
		string cargoName = GetCargoName(token);
		newRoot = false;
		newCargo = false;
		cargo = annotation.FindCargo(cargoName);
		if (cargo != null)
		{
			root = FindRootXmlElement(token, cargo);
			if (root == null)
			{
				newRoot = true;
				XmlDocument xmlDocument = new XmlDocument();
				root = xmlDocument.CreateElement(GetXmlName(token), "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			}
		}
		else
		{
			newCargo = true;
			cargo = new AnnotationResource(cargoName);
			XmlDocument xmlDocument2 = new XmlDocument();
			root = xmlDocument2.CreateElement(GetXmlName(token), "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			cargo.Contents.Add(root);
		}
	}

	private void UpdateAttribute(XmlElement root, XmlToken token, string value)
	{
		string xmlName = GetXmlName(token);
		XmlNode attributeNode = root.GetAttributeNode(xmlName, "http://schemas.microsoft.com/windows/annotations/2003/11/base");
		if (attributeNode == null)
		{
			if (value != null)
			{
				root.SetAttribute(xmlName, "http://schemas.microsoft.com/windows/annotations/2003/11/base", value);
			}
		}
		else if (value == null)
		{
			root.RemoveAttribute(xmlName, "http://schemas.microsoft.com/windows/annotations/2003/11/base");
		}
		else if (attributeNode.Value != value)
		{
			root.SetAttribute(xmlName, "http://schemas.microsoft.com/windows/annotations/2003/11/base", value);
		}
	}

	private static string GetXmlName(XmlToken token)
	{
		return s_xmlTokeFullNames[token];
	}

	private static void AddXmlTokenNames(XmlToken token)
	{
		string text = token.ToString();
		switch (token)
		{
		case XmlToken.MetaData:
		case XmlToken.Text:
		case XmlToken.Ink:
			s_xmlTokeFullNames.Add(token, "anb:" + text);
			break;
		default:
			s_xmlTokeFullNames.Add(token, text);
			break;
		}
	}

	private static string GetCargoName(XmlToken token)
	{
		switch (token)
		{
		case XmlToken.MetaData:
		case XmlToken.Left:
		case XmlToken.Top:
		case XmlToken.XOffset:
		case XmlToken.YOffset:
		case XmlToken.Width:
		case XmlToken.Height:
		case XmlToken.IsExpanded:
		case XmlToken.ZOrder:
			return "Meta Data";
		case XmlToken.Text:
			return "Text Data";
		case XmlToken.Ink:
			return "Ink Data";
		default:
			return string.Empty;
		}
	}

	private static XmlElement FindRootXmlElement(XmlToken token, AnnotationResource cargo)
	{
		XmlElement result = null;
		string value = string.Empty;
		switch (token)
		{
		case XmlToken.Text:
		case XmlToken.Ink:
			value = GetXmlName(token);
			break;
		case XmlToken.MetaData:
		case XmlToken.Left:
		case XmlToken.Top:
		case XmlToken.XOffset:
		case XmlToken.YOffset:
		case XmlToken.Width:
		case XmlToken.Height:
		case XmlToken.IsExpanded:
		case XmlToken.ZOrder:
			value = GetXmlName(XmlToken.MetaData);
			break;
		}
		foreach (XmlElement content in cargo.Contents)
		{
			if (content.Name.Equals(value))
			{
				result = content;
				break;
			}
		}
		return result;
	}

	private static object FindContent(XmlToken token, AnnotationResource cargo)
	{
		object result = null;
		XmlElement xmlElement = FindRootXmlElement(token, cargo);
		if (xmlElement != null)
		{
			switch (token)
			{
			case XmlToken.Text:
			case XmlToken.Ink:
				return xmlElement;
			case XmlToken.Left:
			case XmlToken.Top:
			case XmlToken.XOffset:
			case XmlToken.YOffset:
			case XmlToken.Width:
			case XmlToken.Height:
			case XmlToken.IsExpanded:
			case XmlToken.ZOrder:
				return xmlElement.GetAttributeNode(GetXmlName(token), "http://schemas.microsoft.com/windows/annotations/2003/11/base");
			}
		}
		return result;
	}

	private void UpdateContent(StickyNoteControl snc, bool updateAnnotation, XmlToken token)
	{
		Invariant.Assert(snc != null, "Sticky Note Control is null.");
		Invariant.Assert((token & (XmlToken.Text | XmlToken.Ink)) != 0, "No token specified.");
		StickyNoteContentControl content = snc.Content;
		if (content == null || (token == XmlToken.Ink && content.Type != StickyNoteType.Ink) || (token == XmlToken.Text && content.Type != 0))
		{
			return;
		}
		XmlElement root = null;
		if (updateAnnotation)
		{
			AnnotationResource cargo = null;
			bool newRoot = false;
			bool newCargo = false;
			if (!content.IsEmpty)
			{
				GetCargoAndRoot(this, token, out cargo, out root, out newCargo, out newRoot);
				content.Save(root);
			}
			else
			{
				string cargoName = GetCargoName(token);
				cargo = FindCargo(cargoName);
				if (cargo != null)
				{
					_annotation.Cargos.Remove(cargo);
					_cachedXmlElements.Remove(token);
				}
			}
			if (newRoot)
			{
				Invariant.Assert(root != null, "XmlElement should have been created.");
				Invariant.Assert(cargo != null, "Cargo should have been retrieved.");
				cargo.Contents.Add(root);
			}
			if (newCargo)
			{
				Invariant.Assert(cargo != null, "Cargo should have been created.");
				_annotation.Cargos.Add(cargo);
			}
		}
		else
		{
			XmlElement xmlElement = (XmlElement)FindData(token);
			if (xmlElement != null)
			{
				content.Load(xmlElement);
			}
			else if (!content.IsEmpty)
			{
				content.Clear();
			}
		}
	}

	private static void UpdateMetaData(XmlToken token, StickyNoteControl snc, SNCAnnotation sncAnnotation)
	{
		GetCargoAndRoot(sncAnnotation, XmlToken.MetaData, out var cargo, out var root, out var newCargo, out var newRoot);
		if ((token & XmlToken.IsExpanded) != 0)
		{
			bool isExpanded = snc.IsExpanded;
			sncAnnotation.UpdateAttribute(root, XmlToken.IsExpanded, isExpanded.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.Height) != 0)
		{
			double num = (double)snc.GetValue(FrameworkElement.HeightProperty);
			sncAnnotation.UpdateAttribute(root, XmlToken.Height, num.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.Width) != 0)
		{
			double num2 = (double)snc.GetValue(FrameworkElement.WidthProperty);
			sncAnnotation.UpdateAttribute(root, XmlToken.Width, num2.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.Left) != 0)
		{
			double num3 = snc.PositionTransform.X;
			if (snc.FlipBothOrigins)
			{
				num3 = 0.0 - (num3 + snc.Width);
			}
			sncAnnotation.UpdateAttribute(root, XmlToken.Left, num3.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.Top) != 0)
		{
			sncAnnotation.UpdateAttribute(root, XmlToken.Top, snc.PositionTransform.Y.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.XOffset) != 0)
		{
			sncAnnotation.UpdateAttribute(root, XmlToken.XOffset, snc.XOffset.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.YOffset) != 0)
		{
			sncAnnotation.UpdateAttribute(root, XmlToken.YOffset, snc.YOffset.ToString(CultureInfo.InvariantCulture));
		}
		if ((token & XmlToken.ZOrder) != 0)
		{
			sncAnnotation.UpdateAttribute(root, XmlToken.ZOrder, ((IAnnotationComponent)snc).ZOrder.ToString(CultureInfo.InvariantCulture));
		}
		if (newRoot)
		{
			cargo.Contents.Add(root);
		}
		if (newCargo)
		{
			sncAnnotation._annotation.Cargos.Add(cargo);
		}
	}
}
