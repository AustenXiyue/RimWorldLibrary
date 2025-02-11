using System.Windows.Automation.Peers;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Ink;

namespace System.Windows.Controls;

/// <summary>Renders ink on a surface.</summary>
public class InkPresenter : Decorator
{
	private class InkPresenterHighContrastCallback : HighContrastCallback
	{
		private InkPresenter _thisInkPresenter;

		internal override Dispatcher Dispatcher => _thisInkPresenter.Dispatcher;

		internal InkPresenterHighContrastCallback(InkPresenter inkPresenter)
		{
			_thisInkPresenter = inkPresenter;
		}

		private InkPresenterHighContrastCallback()
		{
		}

		internal override void TurnHighContrastOn(Color highContrastColor)
		{
			_thisInkPresenter._renderer.TurnHighContrastOn(highContrastColor);
			_thisInkPresenter.OnStrokeChanged();
		}

		internal override void TurnHighContrastOff()
		{
			_thisInkPresenter._renderer.TurnHighContrastOff();
			_thisInkPresenter.OnStrokeChanged();
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.InkPresenter.Strokes" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.InkPresenter.Strokes" /> dependency property.</returns>
	public static readonly DependencyProperty StrokesProperty = DependencyProperty.Register("Strokes", typeof(StrokeCollection), typeof(InkPresenter), new FrameworkPropertyMetadata(new StrokeCollectionDefaultValueFactory(), OnStrokesChanged), (object value) => value != null);

	private System.Windows.Ink.Renderer _renderer;

	private Rect? _cachedBounds;

	private bool _hasAddedRoot;

	private InkPresenterHighContrastCallback _contrastCallback;

	private Size _constraintSize;

	/// <summary>Gets and sets the strokes that the <see cref="T:System.Windows.Controls.InkPresenter" /> displays. </summary>
	/// <returns>The strokes that the <see cref="T:System.Windows.Controls.InkPresenter" /> displays.</returns>
	public StrokeCollection Strokes
	{
		get
		{
			return (StrokeCollection)GetValue(StrokesProperty);
		}
		set
		{
			SetValue(StrokesProperty, value);
		}
	}

	protected override int VisualChildrenCount
	{
		get
		{
			if (base.Child != null)
			{
				if (_hasAddedRoot)
				{
					return 2;
				}
				return 1;
			}
			if (_hasAddedRoot)
			{
				return 1;
			}
			return 0;
		}
	}

	private Rect StrokesBounds
	{
		get
		{
			if (!_cachedBounds.HasValue)
			{
				_cachedBounds = Strokes.GetBounds();
			}
			return _cachedBounds.Value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.InkPresenter" /> class. </summary>
	public InkPresenter()
	{
		_renderer = new System.Windows.Ink.Renderer();
		SetStrokesChangedHandlers(Strokes, null);
		_contrastCallback = new InkPresenterHighContrastCallback(this);
		HighContrastHelper.RegisterHighContrastCallback(_contrastCallback);
		if (SystemParameters.HighContrast)
		{
			_contrastCallback.TurnHighContrastOn(SystemColors.WindowTextColor);
		}
		_constraintSize = Size.Empty;
	}

	/// <summary>Attaches the visual of a <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" /> to an <see cref="T:System.Windows.Controls.InkPresenter" />. </summary>
	/// <param name="visual">The visual of a <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" />.</param>
	/// <param name="drawingAttributes">The <see cref="T:System.Windows.Ink.DrawingAttributes" /> that specifies the appearance of the dynamically rendered ink.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="visual" /> is already attached to a visual tree.</exception>
	public void AttachVisuals(Visual visual, DrawingAttributes drawingAttributes)
	{
		VerifyAccess();
		EnsureRootVisual();
		_renderer.AttachIncrementalRendering(visual, drawingAttributes);
	}

	/// <summary>Detaches the visual of the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" /> from the <see cref="T:System.Windows.Controls.InkPresenter" />.</summary>
	/// <param name="visual">The visual of the <see cref="T:System.Windows.Input.StylusPlugIns.DynamicRenderer" /> to detach.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="visual" /> is not attached to the <see cref="T:System.Windows.Controls.InkPresenter" />.</exception>
	public void DetachVisuals(Visual visual)
	{
		VerifyAccess();
		EnsureRootVisual();
		_renderer.DetachIncrementalRendering(visual);
	}

	private static void OnStrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		InkPresenter obj = (InkPresenter)d;
		StrokeCollection oldStrokes = (StrokeCollection)e.OldValue;
		StrokeCollection newStrokes = (StrokeCollection)e.NewValue;
		obj.SetStrokesChangedHandlers(newStrokes, oldStrokes);
		obj.OnStrokeChanged(obj, EventArgs.Empty);
	}

	protected override Size MeasureOverride(Size constraint)
	{
		StrokeCollection strokes = Strokes;
		Size result = base.MeasureOverride(constraint);
		if (strokes != null && strokes.Count != 0)
		{
			Rect strokesBounds = StrokesBounds;
			if (!strokesBounds.IsEmpty && strokesBounds.Right > 0.0 && strokesBounds.Bottom > 0.0)
			{
				Size size = new Size(strokesBounds.Right, strokesBounds.Bottom);
				result.Width = Math.Max(result.Width, size.Width);
				result.Height = Math.Max(result.Height, size.Height);
			}
		}
		if (Child != null)
		{
			_constraintSize = constraint;
		}
		else
		{
			_constraintSize = Size.Empty;
		}
		return result;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		VerifyAccess();
		EnsureRootVisual();
		Size size = arrangeSize;
		if (!_constraintSize.IsEmpty)
		{
			size = new Size(Math.Min(arrangeSize.Width, _constraintSize.Width), Math.Min(arrangeSize.Height, _constraintSize.Height));
		}
		Child?.Arrange(new Rect(size));
		return arrangeSize;
	}

	/// <summary>Returns a clipping geometry that indicates the area that will be clipped if the <see cref="P:System.Windows.UIElement.ClipToBounds" /> property is set to true. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> that represents the area that is clipped when <see cref="P:System.Windows.UIElement.ClipToBounds" /> is true. </returns>
	/// <param name="layoutSlotSize">The available size of the element.</param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		if (base.ClipToBounds)
		{
			return base.GetLayoutClip(layoutSlotSize);
		}
		return null;
	}

	protected override Visual GetVisualChild(int index)
	{
		int visualChildrenCount = VisualChildrenCount;
		if (visualChildrenCount == 2)
		{
			return index switch
			{
				0 => base.Child, 
				1 => _renderer.RootVisual, 
				_ => throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange), 
			};
		}
		if (index == 0 && visualChildrenCount == 1)
		{
			if (_hasAddedRoot)
			{
				return _renderer.RootVisual;
			}
			if (base.Child != null)
			{
				return base.Child;
			}
		}
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.InkPresenterAutomationPeer" /> implementation for this control, as part of the WPF infrastructure.</summary>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new InkPresenterAutomationPeer(this);
	}

	internal bool ContainsAttachedVisual(Visual visual)
	{
		VerifyAccess();
		return _renderer.ContainsAttachedIncrementalRenderingVisual(visual);
	}

	internal bool AttachedVisualIsPositionedCorrectly(Visual visual, DrawingAttributes drawingAttributes)
	{
		VerifyAccess();
		return _renderer.AttachedVisualIsPositionedCorrectly(visual, drawingAttributes);
	}

	private void SetStrokesChangedHandlers(StrokeCollection newStrokes, StrokeCollection oldStrokes)
	{
		if (oldStrokes != null)
		{
			oldStrokes.StrokesChanged -= OnStrokesChanged;
		}
		newStrokes.StrokesChanged += OnStrokesChanged;
		_renderer.Strokes = newStrokes;
		SetStrokeChangedHandlers(newStrokes, oldStrokes);
	}

	private void OnStrokesChanged(object sender, StrokeCollectionChangedEventArgs eventArgs)
	{
		SetStrokeChangedHandlers(eventArgs.Added, eventArgs.Removed);
		OnStrokeChanged(this, EventArgs.Empty);
	}

	private void SetStrokeChangedHandlers(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
	{
		int count;
		if (removedStrokes != null)
		{
			count = removedStrokes.Count;
			for (int i = 0; i < count; i++)
			{
				StopListeningOnStrokeEvents(removedStrokes[i]);
			}
		}
		count = addedStrokes.Count;
		for (int i = 0; i < count; i++)
		{
			StartListeningOnStrokeEvents(addedStrokes[i]);
		}
	}

	private void OnStrokeChanged(object sender, EventArgs e)
	{
		OnStrokeChanged();
	}

	private void OnStrokeChanged()
	{
		_cachedBounds = null;
		InvalidateMeasure();
	}

	private void StartListeningOnStrokeEvents(Stroke stroke)
	{
		stroke.Invalidated += OnStrokeChanged;
	}

	private void StopListeningOnStrokeEvents(Stroke stroke)
	{
		stroke.Invalidated -= OnStrokeChanged;
	}

	private void EnsureRootVisual()
	{
		if (!_hasAddedRoot)
		{
			_renderer.RootVisual._parentIndex = 0;
			AddVisualChild(_renderer.RootVisual);
			_hasAddedRoot = true;
		}
	}
}
