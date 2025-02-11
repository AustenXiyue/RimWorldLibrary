namespace System.Windows;

internal class BindingValueCache
{
	internal readonly Type BindingValueType;

	internal readonly object ValueAsBindingValueType;

	internal BindingValueCache(Type bindingValueType, object valueAsBindingValueType)
	{
		BindingValueType = bindingValueType;
		ValueAsBindingValueType = valueAsBindingValueType;
	}
}
