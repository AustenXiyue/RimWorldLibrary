using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Ink;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.InkCanvas.Gesture" /> event. </summary>
public class InkCanvasGestureEventArgs : RoutedEventArgs
{
	private StrokeCollection _strokes;

	private List<GestureRecognitionResult> _gestureRecognitionResults;

	private bool _cancel;

	/// <summary>Gets the strokes that represent the possible gesture.</summary>
	/// <returns>The strokes that represent the possible gesture.</returns>
	public StrokeCollection Strokes => _strokes;

	/// <summary>Gets or sets a Boolean value that indicates whether strokes should be considered a gesture.</summary>
	/// <returns>true if the strokes are ink; false if the strokes are a gesture.</returns>
	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			_cancel = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.InkCanvasGestureEventArgs" /> class. </summary>
	/// <param name="gestureRecognitionResults">The results from the gesture recognizer.</param>
	public InkCanvasGestureEventArgs(StrokeCollection strokes, IEnumerable<GestureRecognitionResult> gestureRecognitionResults)
		: base(InkCanvas.GestureEvent)
	{
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		if (strokes.Count < 1)
		{
			throw new ArgumentException(SR.InvalidEmptyStrokeCollection, "strokes");
		}
		if (gestureRecognitionResults == null)
		{
			throw new ArgumentNullException("strokes");
		}
		List<GestureRecognitionResult> list = new List<GestureRecognitionResult>(gestureRecognitionResults);
		if (list.Count == 0)
		{
			throw new ArgumentException(SR.InvalidEmptyArray, "gestureRecognitionResults");
		}
		_strokes = strokes;
		_gestureRecognitionResults = list;
	}

	/// <summary>Returns results from the gesture recognizer.</summary>
	/// <returns>A collection of possible application gestures that the <see cref="P:System.Windows.Controls.InkCanvasGestureEventArgs.Strokes" /> might be.</returns>
	public ReadOnlyCollection<GestureRecognitionResult> GetGestureRecognitionResults()
	{
		return new ReadOnlyCollection<GestureRecognitionResult>(_gestureRecognitionResults);
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((InkCanvasGestureEventHandler)genericHandler)(genericTarget, this);
	}
}
