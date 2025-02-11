using System.Windows.Documents;

namespace System.Windows.Controls;

internal class DeferredRunTextReference : DeferredReference
{
	private readonly Run _run;

	internal DeferredRunTextReference(Run run)
	{
		_run = run;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		return TextRangeBase.GetTextInternal(_run.ContentStart, _run.ContentEnd);
	}

	internal override Type GetValueType()
	{
		return typeof(string);
	}
}
