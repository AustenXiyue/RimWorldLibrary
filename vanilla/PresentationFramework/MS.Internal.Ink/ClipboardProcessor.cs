using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace MS.Internal.Ink;

internal class ClipboardProcessor
{
	private InkCanvas _inkCanvas;

	private static DependencyObjectType s_InkCanvasDType;

	private Dictionary<InkCanvasClipboardFormat, ClipboardData> _preferredClipboardData;

	internal IEnumerable<InkCanvasClipboardFormat> PreferredFormats
	{
		get
		{
			foreach (KeyValuePair<InkCanvasClipboardFormat, ClipboardData> preferredClipboardDatum in _preferredClipboardData)
			{
				yield return preferredClipboardDatum.Key;
			}
		}
		set
		{
			Dictionary<InkCanvasClipboardFormat, ClipboardData> dictionary = new Dictionary<InkCanvasClipboardFormat, ClipboardData>();
			foreach (InkCanvasClipboardFormat item in value)
			{
				if (!dictionary.ContainsKey(item))
				{
					ClipboardData clipboardData = null;
					dictionary.Add(item, item switch
					{
						InkCanvasClipboardFormat.InkSerializedFormat => new ISFClipboardData(), 
						InkCanvasClipboardFormat.Xaml => new XamlClipboardData(), 
						InkCanvasClipboardFormat.Text => new TextClipboardData(), 
						_ => throw new ArgumentException(SR.InvalidClipboardFormat, "value"), 
					});
				}
			}
			_preferredClipboardData = dictionary;
		}
	}

	private InkCanvas InkCanvas => _inkCanvas;

	private static DependencyObjectType InkCanvasDType
	{
		get
		{
			if (s_InkCanvasDType == null)
			{
				s_InkCanvasDType = DependencyObjectType.FromSystemTypeInternal(typeof(InkCanvas));
			}
			return s_InkCanvasDType;
		}
	}

	internal ClipboardProcessor(InkCanvas inkCanvas)
	{
		if (inkCanvas == null)
		{
			throw new ArgumentNullException("inkCanvas");
		}
		_inkCanvas = inkCanvas;
		_preferredClipboardData = new Dictionary<InkCanvasClipboardFormat, ClipboardData>();
		_preferredClipboardData.Add(InkCanvasClipboardFormat.InkSerializedFormat, new ISFClipboardData());
	}

	internal bool CheckDataFormats(IDataObject dataObject)
	{
		foreach (KeyValuePair<InkCanvasClipboardFormat, ClipboardData> preferredClipboardDatum in _preferredClipboardData)
		{
			if (preferredClipboardDatum.Value.CanPaste(dataObject))
			{
				return true;
			}
		}
		return false;
	}

	internal InkCanvasClipboardDataFormats CopySelectedData(IDataObject dataObject)
	{
		InkCanvasClipboardDataFormats inkCanvasClipboardDataFormats = InkCanvasClipboardDataFormats.None;
		InkCanvasSelection inkCanvasSelection = InkCanvas.InkCanvasSelection;
		StrokeCollection selectedStrokes = inkCanvasSelection.SelectedStrokes;
		if (selectedStrokes.Count > 1)
		{
			StrokeCollection strokeCollection = new StrokeCollection();
			StrokeCollection strokes = InkCanvas.Strokes;
			for (int i = 0; i < strokes.Count; i++)
			{
				if (selectedStrokes.Count == strokeCollection.Count)
				{
					break;
				}
				for (int j = 0; j < selectedStrokes.Count; j++)
				{
					if (strokes[i] == selectedStrokes[j])
					{
						strokeCollection.Add(selectedStrokes[j]);
						break;
					}
				}
			}
			selectedStrokes = strokeCollection.Clone();
		}
		else
		{
			selectedStrokes = selectedStrokes.Clone();
		}
		List<UIElement> list = new List<UIElement>(inkCanvasSelection.SelectedElements);
		Rect selectionBounds = inkCanvasSelection.SelectionBounds;
		if (selectedStrokes.Count != 0 || list.Count != 0)
		{
			Matrix identity = Matrix.Identity;
			identity.OffsetX = 0.0 - selectionBounds.Left;
			identity.OffsetY = 0.0 - selectionBounds.Top;
			if (selectedStrokes.Count != 0)
			{
				inkCanvasSelection.TransformStrokes(selectedStrokes, identity);
				new ISFClipboardData(selectedStrokes).CopyToDataObject(dataObject);
				inkCanvasClipboardDataFormats |= InkCanvasClipboardDataFormats.ISF;
			}
			if (CopySelectionInXAML(dataObject, selectedStrokes, list, identity, selectionBounds.Size))
			{
				inkCanvasClipboardDataFormats |= InkCanvasClipboardDataFormats.XAML;
			}
		}
		return inkCanvasClipboardDataFormats;
	}

	internal bool PasteData(IDataObject dataObject, ref StrokeCollection newStrokes, ref List<UIElement> newElements)
	{
		foreach (KeyValuePair<InkCanvasClipboardFormat, ClipboardData> preferredClipboardDatum in _preferredClipboardData)
		{
			InkCanvasClipboardFormat key = preferredClipboardDatum.Key;
			ClipboardData value = preferredClipboardDatum.Value;
			if (!value.CanPaste(dataObject))
			{
				continue;
			}
			switch (key)
			{
			case InkCanvasClipboardFormat.Xaml:
			{
				XamlClipboardData obj = (XamlClipboardData)value;
				obj.PasteFromDataObject(dataObject);
				List<UIElement> elements = obj.Elements;
				if (elements != null && elements.Count != 0)
				{
					if (elements.Count == 1 && elements[0] is InkCanvas rootInkCanvas)
					{
						TearDownInkCanvasContainer(rootInkCanvas, ref newStrokes, ref newElements);
					}
					else
					{
						newElements = elements;
					}
				}
				break;
			}
			case InkCanvasClipboardFormat.InkSerializedFormat:
			{
				ISFClipboardData iSFClipboardData = (ISFClipboardData)value;
				iSFClipboardData.PasteFromDataObject(dataObject);
				newStrokes = iSFClipboardData.Strokes;
				break;
			}
			case InkCanvasClipboardFormat.Text:
			{
				TextClipboardData textClipboardData = (TextClipboardData)value;
				textClipboardData.PasteFromDataObject(dataObject);
				newElements = textClipboardData.Elements;
				break;
			}
			}
			return true;
		}
		return false;
	}

	private bool CopySelectionInXAML(IDataObject dataObject, StrokeCollection strokes, List<UIElement> elements, Matrix transform, Size size)
	{
		InkCanvas inkCanvas = new InkCanvas();
		if (strokes.Count != 0)
		{
			inkCanvas.Strokes = strokes;
		}
		int count = elements.Count;
		if (count != 0)
		{
			InkCanvasSelection inkCanvasSelection = InkCanvas.InkCanvasSelection;
			for (int i = 0; i < count; i++)
			{
				UIElement uIElement = XamlReader.Load(new XmlTextReader(new StringReader(XamlWriter.Save(elements[i])))) as UIElement;
				((IAddChild)inkCanvas).AddChild((object)uIElement);
				inkCanvasSelection.UpdateElementBounds(elements[i], uIElement, transform);
			}
		}
		if (inkCanvas != null)
		{
			inkCanvas.Width = size.Width;
			inkCanvas.Height = size.Height;
			ClipboardData clipboardData = new XamlClipboardData(new UIElement[1] { inkCanvas });
			try
			{
				clipboardData.CopyToDataObject(dataObject);
			}
			catch (SecurityException)
			{
				inkCanvas = null;
			}
		}
		return inkCanvas != null;
	}

	private void TearDownInkCanvasContainer(InkCanvas rootInkCanvas, ref StrokeCollection newStrokes, ref List<UIElement> newElements)
	{
		newStrokes = rootInkCanvas.Strokes;
		if (rootInkCanvas.Children.Count == 0)
		{
			return;
		}
		List<UIElement> list = new List<UIElement>(rootInkCanvas.Children.Count);
		foreach (UIElement child in rootInkCanvas.Children)
		{
			list.Add(child);
		}
		foreach (UIElement item in list)
		{
			rootInkCanvas.Children.Remove(item);
		}
		newElements = list;
	}
}
