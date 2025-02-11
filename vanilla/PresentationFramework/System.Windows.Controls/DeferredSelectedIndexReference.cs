using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

internal class DeferredSelectedIndexReference : DeferredReference
{
	private readonly Selector _selector;

	internal DeferredSelectedIndexReference(Selector selector)
	{
		_selector = selector;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		return _selector.InternalSelectedIndex;
	}

	internal override Type GetValueType()
	{
		return typeof(int);
	}
}
