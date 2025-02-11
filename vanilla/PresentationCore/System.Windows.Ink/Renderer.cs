using System.Collections.Generic;
using System.Windows.Media;
using MS.Internal.Ink;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

[FriendAccessAllowed]
internal class Renderer
{
	private class StrokeVisual : DrawingVisual
	{
		private Stroke _stroke;

		private bool _cachedIsHighlighter;

		private Color _cachedColor;

		private Renderer _renderer;

		internal Stroke Stroke => _stroke;

		internal bool CachedIsHighlighter
		{
			get
			{
				return _cachedIsHighlighter;
			}
			set
			{
				_cachedIsHighlighter = value;
			}
		}

		internal Color CachedColor
		{
			get
			{
				return _cachedColor;
			}
			set
			{
				_cachedColor = value;
			}
		}

		internal StrokeVisual(Stroke stroke, Renderer renderer)
		{
			if (stroke == null)
			{
				throw new ArgumentNullException("stroke");
			}
			_stroke = stroke;
			_renderer = renderer;
			_cachedColor = stroke.DrawingAttributes.Color;
			_cachedIsHighlighter = stroke.DrawingAttributes.IsHighlighter;
			Update();
		}

		internal void Update()
		{
			using DrawingContext dc = RenderOpen();
			bool flag = _renderer.IsHighContrast();
			if (!flag || !_stroke.DrawingAttributes.IsHighlighter)
			{
				DrawingAttributes drawingAttributes;
				if (!flag)
				{
					drawingAttributes = ((!_stroke.DrawingAttributes.IsHighlighter) ? _stroke.DrawingAttributes : StrokeRenderer.GetHighlighterAttributes(_stroke, _stroke.DrawingAttributes));
				}
				else
				{
					drawingAttributes = _stroke.DrawingAttributes.Clone();
					drawingAttributes.Color = _renderer.GetHighContrastColor();
				}
				_stroke.DrawInternal(dc, drawingAttributes, _stroke.IsSelected);
			}
		}

		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParams)
		{
			return null;
		}
	}

	private class HighlighterContainerVisual : ContainerVisual
	{
		private Color _color;

		internal Color Color => _color;

		internal HighlighterContainerVisual(Color color)
		{
			_color = color;
		}
	}

	private ContainerVisual _rootVisual;

	private ContainerVisual _highlightersRoot;

	private ContainerVisual _incrementalRenderingVisuals;

	private ContainerVisual _regularInkVisuals;

	private Dictionary<Stroke, StrokeVisual> _visuals;

	private Dictionary<Color, HighlighterContainerVisual> _highlighters;

	private StrokeCollection _strokes;

	private List<Visual> _attachedVisuals;

	private bool _highContrast;

	private Color _highContrastColor = Colors.White;

	internal Visual RootVisual => _rootVisual;

	internal StrokeCollection Strokes
	{
		get
		{
			if (_strokes == null)
			{
				_strokes = new StrokeCollection();
				_strokes.StrokesChangedInternal += OnStrokesChanged;
			}
			return _strokes;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value == _strokes)
			{
				return;
			}
			if (_strokes != null)
			{
				_strokes.StrokesChangedInternal -= OnStrokesChanged;
				foreach (StrokeVisual value2 in _visuals.Values)
				{
					StopListeningOnStrokeEvents(value2.Stroke);
					DetachVisual(value2);
				}
				_visuals.Clear();
			}
			_strokes = value;
			foreach (Stroke stroke in _strokes)
			{
				StrokeVisual strokeVisual = new StrokeVisual(stroke, this);
				_visuals.Add(stroke, strokeVisual);
				StartListeningOnStrokeEvents(strokeVisual.Stroke);
				AttachVisual(strokeVisual, buildingStrokeCollection: true);
			}
			_strokes.StrokesChangedInternal += OnStrokesChanged;
		}
	}

	internal Renderer()
	{
		_rootVisual = new ContainerVisual();
		_highlightersRoot = new ContainerVisual();
		_regularInkVisuals = new ContainerVisual();
		_incrementalRenderingVisuals = new ContainerVisual();
		VisualCollection children = _rootVisual.Children;
		children.Add(_highlightersRoot);
		children.Add(_regularInkVisuals);
		children.Add(_incrementalRenderingVisuals);
		_highContrast = false;
		_visuals = new Dictionary<Stroke, StrokeVisual>();
	}

	internal void AttachIncrementalRendering(Visual visual, DrawingAttributes drawingAttributes)
	{
		if (visual == null)
		{
			throw new ArgumentNullException("visual");
		}
		if (drawingAttributes == null)
		{
			throw new ArgumentNullException("drawingAttributes");
		}
		bool flag = false;
		if (_attachedVisuals != null)
		{
			foreach (Visual attachedVisual in _attachedVisuals)
			{
				if (visual == attachedVisual)
				{
					flag = true;
					throw new InvalidOperationException(SR.CannotAttachVisualTwice);
				}
			}
		}
		else
		{
			_attachedVisuals = new List<Visual>();
		}
		if (!flag)
		{
			(drawingAttributes.IsHighlighter ? GetContainerVisual(drawingAttributes) : _incrementalRenderingVisuals).Children.Add(visual);
			_attachedVisuals.Add(visual);
		}
	}

	internal void DetachIncrementalRendering(Visual visual)
	{
		if (visual == null)
		{
			throw new ArgumentNullException("visual");
		}
		if (_attachedVisuals == null || !_attachedVisuals.Remove(visual))
		{
			throw new InvalidOperationException(SR.VisualCannotBeDetached);
		}
		DetachVisual(visual);
	}

	internal bool ContainsAttachedIncrementalRenderingVisual(Visual visual)
	{
		if (visual == null || _attachedVisuals == null)
		{
			return false;
		}
		return _attachedVisuals.Contains(visual);
	}

	internal bool AttachedVisualIsPositionedCorrectly(Visual visual, DrawingAttributes drawingAttributes)
	{
		if (visual == null || drawingAttributes == null || _attachedVisuals == null || !_attachedVisuals.Contains(visual))
		{
			return false;
		}
		ContainerVisual containerVisual = (drawingAttributes.IsHighlighter ? GetContainerVisual(drawingAttributes) : _incrementalRenderingVisuals);
		if (!(VisualTreeHelper.GetParent(visual) is ContainerVisual containerVisual2) || containerVisual != containerVisual2)
		{
			return false;
		}
		return true;
	}

	internal void TurnHighContrastOn(Color strokeColor)
	{
		if (!_highContrast || strokeColor != _highContrastColor)
		{
			_highContrast = true;
			_highContrastColor = strokeColor;
			UpdateStrokeVisuals();
		}
	}

	internal void TurnHighContrastOff()
	{
		if (_highContrast)
		{
			_highContrast = false;
			UpdateStrokeVisuals();
		}
	}

	internal bool IsHighContrast()
	{
		return _highContrast;
	}

	public Color GetHighContrastColor()
	{
		return _highContrastColor;
	}

	private void OnStrokesChanged(object sender, StrokeCollectionChangedEventArgs eventArgs)
	{
		StrokeCollection added = eventArgs.Added;
		StrokeCollection removed = eventArgs.Removed;
		foreach (Stroke item in added)
		{
			if (_visuals.ContainsKey(item))
			{
				throw new ArgumentException(SR.DuplicateStrokeAdded);
			}
			StrokeVisual strokeVisual = new StrokeVisual(item, this);
			_visuals.Add(item, strokeVisual);
			StartListeningOnStrokeEvents(strokeVisual.Stroke);
			AttachVisual(strokeVisual, buildingStrokeCollection: false);
		}
		foreach (Stroke item2 in removed)
		{
			StrokeVisual value = null;
			if (_visuals.TryGetValue(item2, out value))
			{
				DetachVisual(value);
				StopListeningOnStrokeEvents(value.Stroke);
				_visuals.Remove(item2);
				continue;
			}
			throw new ArgumentException(SR.UnknownStroke3);
		}
	}

	private void OnStrokeInvalidated(object sender, EventArgs eventArgs)
	{
		Stroke stroke = (Stroke)sender;
		if (!_visuals.TryGetValue(stroke, out var value))
		{
			throw new ArgumentException(SR.UnknownStroke1);
		}
		if (value.CachedIsHighlighter != stroke.DrawingAttributes.IsHighlighter || (stroke.DrawingAttributes.IsHighlighter && StrokeRenderer.GetHighlighterColor(value.CachedColor) != StrokeRenderer.GetHighlighterColor(stroke.DrawingAttributes.Color)))
		{
			DetachVisual(value);
			AttachVisual(value, buildingStrokeCollection: false);
			value.CachedIsHighlighter = stroke.DrawingAttributes.IsHighlighter;
			value.CachedColor = stroke.DrawingAttributes.Color;
		}
		value.Update();
	}

	private void UpdateStrokeVisuals()
	{
		foreach (StrokeVisual value in _visuals.Values)
		{
			value.Update();
		}
	}

	private void AttachVisual(StrokeVisual visual, bool buildingStrokeCollection)
	{
		if (visual.Stroke.DrawingAttributes.IsHighlighter)
		{
			ContainerVisual containerVisual = GetContainerVisual(visual.Stroke.DrawingAttributes);
			int index = 0;
			for (int num = containerVisual.Children.Count - 1; num >= 0; num--)
			{
				if (containerVisual.Children[num] is StrokeVisual)
				{
					index = num + 1;
					break;
				}
			}
			containerVisual.Children.Insert(index, visual);
			return;
		}
		StrokeVisual value = null;
		int num2 = 0;
		if (buildingStrokeCollection)
		{
			Stroke stroke = visual.Stroke;
			num2 = Math.Min(_visuals.Count, _strokes.Count);
			while (--num2 >= 0 && _strokes[num2] != stroke)
			{
			}
		}
		else
		{
			num2 = _strokes.IndexOf(visual.Stroke);
		}
		while (--num2 >= 0)
		{
			Stroke stroke2 = _strokes[num2];
			if (!stroke2.DrawingAttributes.IsHighlighter && _visuals.TryGetValue(stroke2, out value) && VisualTreeHelper.GetParent(value) != null)
			{
				VisualCollection children = ((ContainerVisual)VisualTreeHelper.GetParent(value)).Children;
				int num3 = children.IndexOf(value);
				children.Insert(num3 + 1, visual);
				break;
			}
		}
		if (num2 < 0)
		{
			GetContainerVisual(visual.Stroke.DrawingAttributes).Children.Insert(0, visual);
		}
	}

	private void DetachVisual(Visual visual)
	{
		ContainerVisual containerVisual = (ContainerVisual)VisualTreeHelper.GetParent(visual);
		if (containerVisual != null)
		{
			containerVisual.Children.Remove(visual);
			if (containerVisual is HighlighterContainerVisual highlighterContainerVisual && highlighterContainerVisual.Children.Count == 0 && _highlighters != null && _highlighters.ContainsValue(highlighterContainerVisual))
			{
				DetachVisual(highlighterContainerVisual);
				_highlighters.Remove(highlighterContainerVisual.Color);
			}
		}
	}

	private void StartListeningOnStrokeEvents(Stroke stroke)
	{
		stroke.Invalidated += OnStrokeInvalidated;
	}

	private void StopListeningOnStrokeEvents(Stroke stroke)
	{
		stroke.Invalidated -= OnStrokeInvalidated;
	}

	private ContainerVisual GetContainerVisual(DrawingAttributes drawingAttributes)
	{
		if (drawingAttributes.IsHighlighter)
		{
			Color highlighterColor = StrokeRenderer.GetHighlighterColor(drawingAttributes.Color);
			if (_highlighters == null || !_highlighters.TryGetValue(highlighterColor, out var value))
			{
				if (_highlighters == null)
				{
					_highlighters = new Dictionary<Color, HighlighterContainerVisual>();
				}
				value = new HighlighterContainerVisual(highlighterColor);
				value.Opacity = 0.5;
				_highlightersRoot.Children.Add(value);
				_highlighters.Add(highlighterColor, value);
			}
			else if (VisualTreeHelper.GetParent(value) == null)
			{
				_highlightersRoot.Children.Add(value);
			}
			return value;
		}
		return _regularInkVisuals;
	}
}
