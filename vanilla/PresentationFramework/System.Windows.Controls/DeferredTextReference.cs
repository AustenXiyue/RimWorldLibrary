using System.Windows.Documents;

namespace System.Windows.Controls;

internal class DeferredTextReference : DeferredReference
{
	private readonly ITextContainer _textContainer;

	internal DeferredTextReference(ITextContainer textContainer)
	{
		_textContainer = textContainer;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		string textInternal = TextRangeBase.GetTextInternal(_textContainer.Start, _textContainer.End);
		if (_textContainer.Parent is TextBox textBox)
		{
			textBox.OnDeferredTextReferenceResolved(this, textInternal);
		}
		return textInternal;
	}

	internal override Type GetValueType()
	{
		return typeof(string);
	}
}
