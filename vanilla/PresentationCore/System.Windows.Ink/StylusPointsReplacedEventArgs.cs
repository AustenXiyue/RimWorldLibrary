using System.Windows.Input;

namespace System.Windows.Ink;

/// <summary>Provides data for the <see cref="E:System.Windows.Ink.Stroke.StylusPointsReplaced" /> event.  </summary>
public class StylusPointsReplacedEventArgs : EventArgs
{
	private StylusPointCollection _newStylusPoints;

	private StylusPointCollection _previousStylusPoints;

	/// <summary>Gets the new <see cref="T:System.Windows.Input.StylusPointCollection" /> for the <see cref="T:System.Windows.Ink.Stroke" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Input.StylusPointCollection" /> for the <see cref="T:System.Windows.Ink.Stroke" />.</returns>
	public StylusPointCollection NewStylusPoints => _newStylusPoints;

	/// <summary>Gets the replaced <see cref="T:System.Windows.Input.StylusPointCollection" />.</summary>
	/// <returns>The replaced <see cref="T:System.Windows.Input.StylusPointCollection" />.</returns>
	public StylusPointCollection PreviousStylusPoints => _previousStylusPoints;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.StylusPointsReplacedEventArgs" /> class. </summary>
	/// <param name="newStylusPoints">The new <see cref="T:System.Windows.Input.StylusPointCollection" /> for the <see cref="T:System.Windows.Ink.Stroke" />.</param>
	/// <param name="previousStylusPoints">The replaced <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	public StylusPointsReplacedEventArgs(StylusPointCollection newStylusPoints, StylusPointCollection previousStylusPoints)
	{
		if (newStylusPoints == null)
		{
			throw new ArgumentNullException("newStylusPoints");
		}
		if (previousStylusPoints == null)
		{
			throw new ArgumentNullException("previousStylusPoints");
		}
		_newStylusPoints = newStylusPoints;
		_previousStylusPoints = previousStylusPoints;
	}
}
