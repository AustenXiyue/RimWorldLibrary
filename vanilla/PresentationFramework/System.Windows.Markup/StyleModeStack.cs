using System.Collections.Generic;

namespace System.Windows.Markup;

internal class StyleModeStack
{
	private Stack<StyleMode> _stack = new Stack<StyleMode>(64);

	internal int Depth => _stack.Count - 1;

	internal StyleMode Mode => _stack.Peek();

	internal StyleModeStack()
	{
		Push(StyleMode.Base);
	}

	internal void Push(StyleMode mode)
	{
		_stack.Push(mode);
	}

	internal void Push()
	{
		Push(Mode);
	}

	internal StyleMode Pop()
	{
		return _stack.Pop();
	}
}
