namespace System.Windows.Diagnostics;

public class VisualTreeChangeEventArgs : EventArgs
{
	public DependencyObject Parent { get; private set; }

	public DependencyObject Child { get; private set; }

	public int ChildIndex { get; private set; }

	public VisualTreeChangeType ChangeType { get; private set; }

	public VisualTreeChangeEventArgs(DependencyObject parent, DependencyObject child, int childIndex, VisualTreeChangeType changeType)
	{
		Parent = parent;
		Child = child;
		ChildIndex = childIndex;
		ChangeType = changeType;
	}
}
