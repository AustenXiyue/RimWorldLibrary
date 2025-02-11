using System.Windows;

namespace MS.Internal;

internal class PrePostDescendentsWalker<T> : DescendentsWalker<T>
{
	private VisitedCallback<T> _postCallback;

	public PrePostDescendentsWalker(TreeWalkPriority priority, VisitedCallback<T> preCallback, VisitedCallback<T> postCallback, T data)
		: base(priority, preCallback, data)
	{
		_postCallback = postCallback;
	}

	public override void StartWalk(DependencyObject startNode, bool skipStartNode)
	{
		try
		{
			base.StartWalk(startNode, skipStartNode);
		}
		finally
		{
			if (!skipStartNode && _postCallback != null && ((startNode is FrameworkElement || startNode is FrameworkContentElement) ? true : false))
			{
				_postCallback(startNode, base.Data, _priority == TreeWalkPriority.VisualTree);
			}
		}
	}

	protected override void _VisitNode(DependencyObject d, bool visitedViaVisualTree)
	{
		try
		{
			base._VisitNode(d, visitedViaVisualTree);
		}
		finally
		{
			if (_postCallback != null)
			{
				_postCallback(d, base.Data, visitedViaVisualTree);
			}
		}
	}
}
