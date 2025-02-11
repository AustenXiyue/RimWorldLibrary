namespace System.Windows;

internal struct InheritablePropertyChangeInfo
{
	private DependencyObject _rootElement;

	private DependencyProperty _property;

	private EffectiveValueEntry _oldEntry;

	private EffectiveValueEntry _newEntry;

	internal DependencyObject RootElement => _rootElement;

	internal DependencyProperty Property => _property;

	internal EffectiveValueEntry OldEntry => _oldEntry;

	internal EffectiveValueEntry NewEntry => _newEntry;

	internal InheritablePropertyChangeInfo(DependencyObject rootElement, DependencyProperty property, EffectiveValueEntry oldEntry, EffectiveValueEntry newEntry)
	{
		_rootElement = rootElement;
		_property = property;
		_oldEntry = oldEntry;
		_newEntry = newEntry;
	}
}
