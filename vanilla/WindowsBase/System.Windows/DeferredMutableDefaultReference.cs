namespace System.Windows;

internal class DeferredMutableDefaultReference : DeferredReference
{
	private readonly PropertyMetadata _sourceMetadata;

	private readonly DependencyObject _sourceObject;

	private readonly DependencyProperty _sourceProperty;

	internal PropertyMetadata SourceMetadata => _sourceMetadata;

	protected DependencyObject SourceObject => _sourceObject;

	protected DependencyProperty SourceProperty => _sourceProperty;

	internal DeferredMutableDefaultReference(PropertyMetadata metadata, DependencyObject d, DependencyProperty dp)
	{
		_sourceObject = d;
		_sourceProperty = dp;
		_sourceMetadata = metadata;
	}

	internal override object GetValue(BaseValueSourceInternal valueSource)
	{
		return _sourceMetadata.GetDefaultValue(_sourceObject, _sourceProperty);
	}

	internal override Type GetValueType()
	{
		return _sourceProperty.PropertyType;
	}
}
