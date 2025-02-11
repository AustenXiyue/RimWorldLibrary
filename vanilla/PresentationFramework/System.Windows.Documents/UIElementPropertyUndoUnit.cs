using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal class UIElementPropertyUndoUnit : IUndoUnit
{
	private readonly UIElement _uiElement;

	private readonly DependencyProperty _property;

	private readonly object _oldValue;

	private UIElementPropertyUndoUnit(UIElement uiElement, DependencyProperty property, object oldValue)
	{
		_uiElement = uiElement;
		_property = property;
		_oldValue = oldValue;
	}

	public void Do()
	{
		if (_oldValue != DependencyProperty.UnsetValue)
		{
			_uiElement.SetValue(_property, _oldValue);
		}
		else
		{
			_uiElement.ClearValue(_property);
		}
	}

	public bool Merge(IUndoUnit unit)
	{
		Invariant.Assert(unit != null);
		return false;
	}

	internal static void Add(ITextContainer textContainer, UIElement uiElement, DependencyProperty property, HorizontalAlignment newValue)
	{
		AddPrivate(textContainer, uiElement, property, newValue);
	}

	internal static void Add(ITextContainer textContainer, UIElement uiElement, DependencyProperty property, FlowDirection newValue)
	{
		AddPrivate(textContainer, uiElement, property, newValue);
	}

	private static void AddPrivate(ITextContainer textContainer, UIElement uiElement, DependencyProperty property, object newValue)
	{
		UndoManager orClearUndoManager = TextTreeUndo.GetOrClearUndoManager(textContainer);
		if (orClearUndoManager == null)
		{
			return;
		}
		object obj = uiElement.ReadLocalValue(property);
		if (obj is Expression)
		{
			if (orClearUndoManager.IsEnabled)
			{
				orClearUndoManager.Clear();
			}
		}
		else if (!obj.Equals(newValue))
		{
			orClearUndoManager.Add(new UIElementPropertyUndoUnit(uiElement, property, obj));
		}
	}
}
