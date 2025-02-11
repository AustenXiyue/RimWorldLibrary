using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;

namespace System.Windows.Documents;

internal class Highlights
{
	private class LayerHighlightChangedEventArgs : HighlightChangedEventArgs
	{
		private readonly ReadOnlyCollection<TextSegment> _ranges;

		private readonly Type _ownerType;

		internal override IList Ranges => _ranges;

		internal override Type OwnerType => _ownerType;

		internal LayerHighlightChangedEventArgs(ReadOnlyCollection<TextSegment> ranges, Type ownerType)
		{
			_ranges = ranges;
			_ownerType = ownerType;
		}
	}

	private readonly ITextContainer _textContainer;

	private ArrayList _layers;

	protected ITextContainer TextContainer => _textContainer;

	private int LayerCount
	{
		get
		{
			if (_layers != null)
			{
				return _layers.Count;
			}
			return 0;
		}
	}

	internal event HighlightChangedEventHandler Changed;

	internal Highlights(ITextContainer textContainer)
	{
		_textContainer = textContainer;
	}

	internal virtual object GetHighlightValue(StaticTextPointer textPosition, LogicalDirection direction, Type highlightLayerOwnerType)
	{
		object obj = DependencyProperty.UnsetValue;
		for (int i = 0; i < LayerCount; i++)
		{
			HighlightLayer layer = GetLayer(i);
			if (layer.OwnerType == highlightLayerOwnerType)
			{
				obj = layer.GetHighlightValue(textPosition, direction);
				if (obj != DependencyProperty.UnsetValue)
				{
					break;
				}
			}
		}
		return obj;
	}

	internal virtual bool IsContentHighlighted(StaticTextPointer textPosition, LogicalDirection direction)
	{
		int i;
		for (i = 0; i < LayerCount && !GetLayer(i).IsContentHighlighted(textPosition, direction); i++)
		{
		}
		return i < LayerCount;
	}

	internal virtual StaticTextPointer GetNextHighlightChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		StaticTextPointer staticTextPointer = StaticTextPointer.Null;
		for (int i = 0; i < LayerCount; i++)
		{
			StaticTextPointer nextChangePosition = GetLayer(i).GetNextChangePosition(textPosition, direction);
			if (!nextChangePosition.IsNull)
			{
				staticTextPointer = ((!staticTextPointer.IsNull) ? ((direction != LogicalDirection.Forward) ? StaticTextPointer.Max(staticTextPointer, nextChangePosition) : StaticTextPointer.Min(staticTextPointer, nextChangePosition)) : nextChangePosition);
			}
		}
		return staticTextPointer;
	}

	internal virtual StaticTextPointer GetNextPropertyChangePosition(StaticTextPointer textPosition, LogicalDirection direction)
	{
		StaticTextPointer staticTextPointer;
		switch (textPosition.GetPointerContext(direction))
		{
		case TextPointerContext.None:
			staticTextPointer = StaticTextPointer.Null;
			break;
		case TextPointerContext.Text:
		{
			staticTextPointer = GetNextHighlightChangePosition(textPosition, direction);
			StaticTextPointer nextContextPosition = textPosition.GetNextContextPosition(LogicalDirection.Forward);
			if (staticTextPointer.IsNull || nextContextPosition.CompareTo(staticTextPointer) < 0)
			{
				staticTextPointer = nextContextPosition;
			}
			break;
		}
		default:
			staticTextPointer = textPosition.CreatePointer(1);
			break;
		}
		return staticTextPointer;
	}

	internal void AddLayer(HighlightLayer highlightLayer)
	{
		if (_layers == null)
		{
			_layers = new ArrayList(1);
		}
		Invariant.Assert(!_layers.Contains(highlightLayer));
		_layers.Add(highlightLayer);
		highlightLayer.Changed += OnLayerChanged;
		RaiseChangedEventForLayerContent(highlightLayer);
	}

	internal void RemoveLayer(HighlightLayer highlightLayer)
	{
		Invariant.Assert(_layers != null && _layers.Contains(highlightLayer));
		RaiseChangedEventForLayerContent(highlightLayer);
		highlightLayer.Changed -= OnLayerChanged;
		_layers.Remove(highlightLayer);
	}

	internal HighlightLayer GetLayer(Type highlightLayerType)
	{
		for (int i = 0; i < LayerCount; i++)
		{
			if (highlightLayerType == GetLayer(i).OwnerType)
			{
				return GetLayer(i);
			}
		}
		return null;
	}

	private HighlightLayer GetLayer(int index)
	{
		return (HighlightLayer)_layers[index];
	}

	private void OnLayerChanged(object sender, HighlightChangedEventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(this, args);
		}
	}

	private void RaiseChangedEventForLayerContent(HighlightLayer highlightLayer)
	{
		if (this.Changed == null)
		{
			return;
		}
		List<TextSegment> list = new List<TextSegment>();
		StaticTextPointer staticTextPointer = _textContainer.CreateStaticPointerAtOffset(0);
		while (true)
		{
			if (!highlightLayer.IsContentHighlighted(staticTextPointer, LogicalDirection.Forward))
			{
				staticTextPointer = highlightLayer.GetNextChangePosition(staticTextPointer, LogicalDirection.Forward);
				if (staticTextPointer.IsNull)
				{
					break;
				}
			}
			StaticTextPointer staticTextPointer2 = staticTextPointer;
			staticTextPointer = highlightLayer.GetNextChangePosition(staticTextPointer, LogicalDirection.Forward);
			Invariant.Assert(!staticTextPointer.IsNull, "Highlight start not followed by highlight end!");
			list.Add(new TextSegment(staticTextPointer2.CreateDynamicTextPointer(LogicalDirection.Forward), staticTextPointer.CreateDynamicTextPointer(LogicalDirection.Forward)));
		}
		if (list.Count > 0)
		{
			this.Changed(this, new LayerHighlightChangedEventArgs(new ReadOnlyCollection<TextSegment>(list), highlightLayer.OwnerType));
		}
	}
}
