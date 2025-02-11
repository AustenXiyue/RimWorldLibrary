using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Ink;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.SelectionChanging" />.</summary>
public class InkCanvasSelectionChangingEventArgs : CancelEventArgs
{
	private StrokeCollection _strokes;

	private List<UIElement> _elements;

	private bool _strokesChanged;

	private bool _elementsChanged;

	internal bool StrokesChanged => _strokesChanged;

	internal bool ElementsChanged => _elementsChanged;

	internal InkCanvasSelectionChangingEventArgs(StrokeCollection selectedStrokes, IEnumerable<UIElement> selectedElements)
	{
		if (selectedStrokes == null)
		{
			throw new ArgumentNullException("selectedStrokes");
		}
		if (selectedElements == null)
		{
			throw new ArgumentNullException("selectedElements");
		}
		_strokes = selectedStrokes;
		List<UIElement> elements = new List<UIElement>(selectedElements);
		_elements = elements;
		_strokesChanged = false;
		_elementsChanged = false;
	}

	/// <summary>Sets the selected elements.</summary>
	/// <param name="selectedElements">The elements to select.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="selectedElements" /> is null.</exception>
	public void SetSelectedElements(IEnumerable<UIElement> selectedElements)
	{
		if (selectedElements == null)
		{
			throw new ArgumentNullException("selectedElements");
		}
		List<UIElement> elements = new List<UIElement>(selectedElements);
		_elements = elements;
		_elementsChanged = true;
	}

	/// <summary>Returns the selected elements.</summary>
	/// <returns>The selected elements.</returns>
	public ReadOnlyCollection<UIElement> GetSelectedElements()
	{
		return new ReadOnlyCollection<UIElement>(_elements);
	}

	/// <summary>Sets the selected strokes.</summary>
	/// <param name="selectedStrokes">The strokes to select.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="selectedStrokes" /> is null.</exception>
	public void SetSelectedStrokes(StrokeCollection selectedStrokes)
	{
		if (selectedStrokes == null)
		{
			throw new ArgumentNullException("selectedStrokes");
		}
		_strokes = selectedStrokes;
		_strokesChanged = true;
	}

	/// <summary>Returns the selected strokes.</summary>
	/// <returns>The selected strokes.</returns>
	public StrokeCollection GetSelectedStrokes()
	{
		return new StrokeCollection { _strokes };
	}
}
