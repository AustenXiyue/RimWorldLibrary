using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal.Ink.GestureRecognition;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Recognizes ink gestures.</summary>
public sealed class GestureRecognizer : DependencyObject, IDisposable
{
	private ApplicationGesture[] _enabledGestures;

	private NativeRecognizer _nativeRecognizer;

	private bool _disposed;

	/// <summary>Gets a Boolean that indicates whether the gesture recognizer is available on the user's system.</summary>
	/// <returns>true if the recognition component is available; otherwise, false.</returns>
	public bool IsRecognizerAvailable
	{
		get
		{
			VerifyAccess();
			VerifyDisposed();
			if (_nativeRecognizer == null)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.GestureRecognizer" /> class. </summary>
	public GestureRecognizer()
		: this(new ApplicationGesture[1])
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.GestureRecognizer" /> class. </summary>
	/// <param name="enabledApplicationGestures">An array of type <see cref="T:System.Windows.Ink.ApplicationGesture" /> that specifies the application gestures the <see cref="T:System.Windows.Ink.GestureRecognizer" /> will recognize.</param>
	public GestureRecognizer(IEnumerable<ApplicationGesture> enabledApplicationGestures)
	{
		_nativeRecognizer = NativeRecognizer.CreateInstance();
		if (_nativeRecognizer == null)
		{
			NativeRecognizer.GetApplicationGestureArrayAndVerify(enabledApplicationGestures);
		}
		else
		{
			SetEnabledGestures(enabledApplicationGestures);
		}
	}

	/// <summary>Sets the application gestures that the <see cref="T:System.Windows.Ink.GestureRecognizer" /> recognizes.</summary>
	/// <param name="applicationGestures">An array of type <see cref="T:System.Windows.Ink.ApplicationGesture" /> that specifies the application gestures you wish the <see cref="T:System.Windows.Ink.GestureRecognizer" /> to recognize.</param>
	public void SetEnabledGestures(IEnumerable<ApplicationGesture> applicationGestures)
	{
		VerifyAccess();
		VerifyDisposed();
		VerifyRecognizerAvailable();
		ApplicationGesture[] enabledGestures = _nativeRecognizer.SetEnabledGestures(applicationGestures);
		_enabledGestures = enabledGestures;
	}

	/// <summary>Gets the gestures that the <see cref="T:System.Windows.Ink.GestureRecognizer" /> recognizes.</summary>
	/// <returns>An array of type <see cref="T:System.Windows.Ink.ApplicationGesture" /> that contains gestures the <see cref="T:System.Windows.Ink.GestureRecognizer" /> is set to recognize.</returns>
	public ReadOnlyCollection<ApplicationGesture> GetEnabledGestures()
	{
		VerifyAccess();
		VerifyDisposed();
		VerifyRecognizerAvailable();
		if (_enabledGestures == null)
		{
			_enabledGestures = Array.Empty<ApplicationGesture>();
		}
		return new ReadOnlyCollection<ApplicationGesture>(_enabledGestures);
	}

	/// <summary>Looks for gestures in the specified <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>An array of type <see cref="T:System.Windows.Ink.GestureRecognitionResult" /> that contains application gestures that the <see cref="T:System.Windows.Ink.GestureRecognizer" /> recognized.</returns>
	/// <param name="strokes">The <see cref="T:System.Windows.Ink.StrokeCollection" /> to search for gestures.</param>
	public ReadOnlyCollection<GestureRecognitionResult> Recognize(StrokeCollection strokes)
	{
		return RecognizeImpl(strokes);
	}

	internal ReadOnlyCollection<GestureRecognitionResult> CriticalRecognize(StrokeCollection strokes)
	{
		return RecognizeImpl(strokes);
	}

	private ReadOnlyCollection<GestureRecognitionResult> RecognizeImpl(StrokeCollection strokes)
	{
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		if (strokes.Count > 2)
		{
			throw new ArgumentException(SR.StrokeCollectionCountTooBig, "strokes");
		}
		VerifyAccess();
		VerifyDisposed();
		VerifyRecognizerAvailable();
		return new ReadOnlyCollection<GestureRecognitionResult>(_nativeRecognizer.Recognize(strokes));
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Windows.Ink.GestureRecognizer" />. </summary>
	public void Dispose()
	{
		VerifyAccess();
		if (!_disposed)
		{
			if (_nativeRecognizer != null)
			{
				_nativeRecognizer.Dispose();
				_nativeRecognizer = null;
			}
			_disposed = true;
		}
	}

	private void VerifyRecognizerAvailable()
	{
		if (_nativeRecognizer == null)
		{
			throw new InvalidOperationException(SR.GestureRecognizerNotAvailable);
		}
	}

	private void VerifyDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GestureRecognizer");
		}
	}
}
