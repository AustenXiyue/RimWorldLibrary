namespace System.Windows.Documents;

internal class TextContainerChangeEventArgs : EventArgs
{
	private readonly ITextPointer _textPosition;

	private readonly int _count;

	private readonly int _charCount;

	private readonly TextChangeType _textChange;

	private readonly DependencyProperty _property;

	private readonly bool _affectsRenderOnly;

	internal ITextPointer ITextPosition => _textPosition;

	internal int IMECharCount => _charCount;

	internal bool AffectsRenderOnly => _affectsRenderOnly;

	internal int Count => _count;

	internal TextChangeType TextChange => _textChange;

	internal DependencyProperty Property => _property;

	internal TextContainerChangeEventArgs(ITextPointer textPosition, int count, int charCount, TextChangeType textChange)
		: this(textPosition, count, charCount, textChange, null, affectsRenderOnly: false)
	{
	}

	internal TextContainerChangeEventArgs(ITextPointer textPosition, int count, int charCount, TextChangeType textChange, DependencyProperty property, bool affectsRenderOnly)
	{
		_textPosition = textPosition.GetFrozenPointer(LogicalDirection.Forward);
		_count = count;
		_charCount = charCount;
		_textChange = textChange;
		_property = property;
		_affectsRenderOnly = affectsRenderOnly;
	}
}
