namespace System.Windows.Media.Animation;

internal class IndependentlyAnimatedPropertyMetadata : UIPropertyMetadata
{
	internal IndependentlyAnimatedPropertyMetadata(object defaultValue)
		: base(defaultValue)
	{
	}

	internal IndependentlyAnimatedPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
	}

	internal override PropertyMetadata CreateInstance()
	{
		return new IndependentlyAnimatedPropertyMetadata(base.DefaultValue);
	}
}
