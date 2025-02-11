namespace System.Windows.Ink;

/// <summary>Contains information about an ink gesture.</summary>
public class GestureRecognitionResult
{
	private RecognitionConfidence _confidence;

	private ApplicationGesture _gesture;

	/// <summary>Gets the level of confidence that the <see cref="T:System.Windows.Ink.GestureRecognizer" /> has in the recognition of the gesture.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Ink.RecognitionConfidence" /> levels.</returns>
	public RecognitionConfidence RecognitionConfidence => _confidence;

	/// <summary>Gets the recognized ink gesture.</summary>
	/// <returns>The recognized ink gesture.</returns>
	public ApplicationGesture ApplicationGesture => _gesture;

	internal GestureRecognitionResult(RecognitionConfidence confidence, ApplicationGesture gesture)
	{
		_confidence = confidence;
		_gesture = gesture;
	}
}
