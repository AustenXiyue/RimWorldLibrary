using System.Windows;

namespace MS.Internal.Data;

internal abstract class ObjectRef
{
	internal bool UsesMentor => ProtectedUsesMentor;

	protected virtual bool ProtectedUsesMentor => true;

	internal virtual object GetObject(DependencyObject d, ObjectRefArgs args)
	{
		return null;
	}

	internal virtual object GetDataObject(DependencyObject d, ObjectRefArgs args)
	{
		return GetObject(d, args);
	}

	internal bool TreeContextIsRequired(DependencyObject target)
	{
		return ProtectedTreeContextIsRequired(target);
	}

	protected virtual bool ProtectedTreeContextIsRequired(DependencyObject target)
	{
		return false;
	}

	internal abstract string Identify();
}
