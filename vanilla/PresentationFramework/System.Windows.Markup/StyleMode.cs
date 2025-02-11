namespace System.Windows.Markup;

internal enum StyleMode : byte
{
	Base,
	TargetTypeProperty,
	BasedOnProperty,
	DataTypeProperty,
	ComplexProperty,
	Resources,
	Setters,
	Key,
	TriggerBase,
	TriggerActions,
	TriggerSetters,
	TriggerEnterExitActions,
	VisualTree
}
