namespace System.Windows.Media;

internal class AncestorChangedEventArgs
{
	private DependencyObject _subRoot;

	private DependencyObject _oldParent;

	public DependencyObject Ancestor => _subRoot;

	public DependencyObject OldParent => _oldParent;

	public AncestorChangedEventArgs(DependencyObject subRoot, DependencyObject oldParent)
	{
		_subRoot = subRoot;
		_oldParent = oldParent;
	}
}
