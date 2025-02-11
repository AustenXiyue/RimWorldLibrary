namespace System.Windows;

internal class DeferredResourceReferenceHolder : DeferredResourceReference
{
	internal override object Key => ((object[])_keyOrValue)[0];

	internal override object Value
	{
		get
		{
			return ((object[])_keyOrValue)[1];
		}
		set
		{
			((object[])_keyOrValue)[1] = value;
		}
	}

	internal override bool IsUnset => Value == DependencyProperty.UnsetValue;

	internal DeferredResourceReferenceHolder(object resourceKey, object value)
		: base(null, null)
	{
		_keyOrValue = new object[2] { resourceKey, value };
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		return Value;
	}

	internal override Type GetValueType()
	{
		return Value?.GetType();
	}
}
